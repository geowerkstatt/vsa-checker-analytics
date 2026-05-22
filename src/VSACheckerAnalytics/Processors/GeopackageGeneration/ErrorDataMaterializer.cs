using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace VsaCheckerAnalytics.Processors.GeopackageGeneration;

/// <summary>
/// Materializes analytics tables from the GeoPackage's imported data.
/// <list type="bullet">
///   <item><c>ca_error_data</c> / <c>ca_error_object</c>: error details and per-object
///     aggregation, built from the checker errors view joined with VSA feature classes.</item>
///   <item><c>ca_leitung</c>: denormalized pipe data with resolved foreign keys and
///     computed classification columns, ready for direct Excel export.</item>
///   <item><c>ca_knoten</c>: denormalized node data with resolved foreign keys and
///     computed classification columns, ready for direct Excel export.</item>
/// </list>
/// </summary>
internal sealed class ErrorDataMaterializer
{
    private static readonly (string ClassName, string TableName, bool HasEnrichment)[] KnownFeatureClasses =
    [
        ("Leitung", "leitung", true),
        ("Knoten", "knoten", true),
        ("Teileinzugsgebiet", "teileinzugsgebiet", false),
        ("SK_Regenueberlauf", "sk_regenueberlauf", false),
        ("SK_Regenueberlaufbecken", "sk_regenueberlaufbecken", false),
        ("SK_Einleitstelle", "sk_einleitstelle", false),
        ("Ueberlauf_Foerderaggregat", "ueberlauf_foerderaggregat", false),
    ];

    private readonly SqliteConnection connection;
    private readonly ILogger logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorDataMaterializer"/> class.
    /// </summary>
    /// <param name="connection">An open SQLite connection to the GeoPackage.</param>
    /// <param name="logger">Logger for diagnostic messages.</param>
    internal ErrorDataMaterializer(SqliteConnection connection, ILogger logger)
    {
        this.connection = connection;
        this.logger = logger;
    }

    /// <summary>
    /// Creates the build view that unions all VSA feature class joins into the
    /// <c>ca_error_data</c> target shape. Each known feature class contributes a typed
    /// SELECT block; unknown classes are included with NULL enrichment columns.
    /// </summary>
    /// <param name="viewName">Name of the build view to create.</param>
    /// <param name="errorsViewName">Name of the source checker errors view.</param>
    /// <param name="language">Language code (<c>"DE"</c> or <c>"FR"</c>) for localized error matrix columns.</param>
    internal void CreateBuildView(string viewName, string errorsViewName, string language)
    {
        var cmsg = LanguageColumn("cmsg", language);
        var recommendation = LanguageColumn("required_action", language);
        var recommendationDetail = LanguageColumn("action_context", language);

        var presentClasses = KnownFeatureClasses
            .Where(c => TableExists(c.TableName))
            .ToList();

        var excludedClassNames = presentClasses.Select(c => c.ClassName).ToList();

        var selects = presentClasses
            .Select(c => BuildClassSelect(errorsViewName, c.ClassName, c.TableName, c.HasEnrichment, cmsg, recommendation, recommendationDetail))
            .Append(BuildCatchAllSelect(errorsViewName, excludedClassNames, cmsg, recommendation, recommendationDetail));

        var sql = $"CREATE VIEW IF NOT EXISTS \"{viewName}\" AS\n{string.Join("\nUNION ALL\n", selects)}";

        ExecuteNonQuery(sql);
        logger.LogDebug("Created build view '{ViewName}'.", viewName);
    }

    /// <summary>
    /// Creates the <c>ca_error_data</c> and <c>ca_error_object</c> tables, populates
    /// <c>ca_error_data</c> from the build view, and aggregates <c>ca_error_object</c>
    /// from the materialized detail rows. The insert and aggregation run in a single
    /// transaction.
    /// </summary>
    /// <param name="buildViewName">Name of the build view to read from.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    internal async Task MaterializeAsync(string buildViewName, CancellationToken cancellationToken)
    {
        CreateTables();
        CreateFeatureTableIndexes();

        cancellationToken.ThrowIfCancellationRequested();

        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        ExecuteNonQuery("DELETE FROM ca_error_object");
        ExecuteNonQuery("DELETE FROM ca_error_data");

        ExecuteNonQuery($"""
            INSERT INTO ca_error_data (
                tid, check_type, topic, class, errorid, error, detail,
                funktionhierarchisch, eigentuemer, status, category, model, module,
                wk, gep, recommendation, recommendation_detail)
            SELECT
                tid, check_type, topic, class, errorid, error, detail,
                funktionhierarchisch, eigentuemer, status, category, model, module,
                wk, gep, recommendation, recommendation_detail
            FROM "{buildViewName}"
            """);

        ExecuteNonQuery("""
            INSERT INTO ca_error_object (tid, class, count_error, wk_max, gep_max)
            SELECT tid, class, COUNT(*), MAX(wk), MAX(gep)
            FROM ca_error_data
            GROUP BY tid, class
            """);

        await transaction.CommitAsync(cancellationToken);

        CreateResultIndexes();

        var errorDataCount = CountRows("ca_error_data");
        var errorObjectCount = CountRows("ca_error_object");
        logger.LogInformation(
            "Materialized {ErrorDataCount} rows into ca_error_data and {ErrorObjectCount} rows into ca_error_object.",
            errorDataCount,
            errorObjectCount);
    }

    /// <summary>
    /// Creates a build view for <c>ca_leitung</c> that denormalizes the <c>leitung</c>
    /// table by resolving foreign keys and adding computed classification columns.
    /// The view definition persists in the GeoPackage, documenting the derivation logic.
    /// </summary>
    /// <param name="viewName">Name of the build view to create.</param>
    internal void CreateLeitungBuildView(string viewName)
    {
        if (!TableExists("leitung"))
        {
            logger.LogDebug("Table 'leitung' not found, skipping view '{ViewName}'.", viewName);
            return;
        }

        var sql = $"""
            CREATE VIEW IF NOT EXISTS "{viewName}" AS
            SELECT
                l.T_Ili_Tid AS tid,
                l.baujahr, l.baulicherzustand, l.bemerkung, l.bezeichnung,
                l.finanzierung, l.funktionhierarchisch, l.funktionhydraulisch,
                l.hoehengenauigkeit_nach, l.hoehengenauigkeit_von, l.hydr_belastung_ist,
                l.kote_nach, l.kote_von, l.laengeeffektiv, l.lagebestimmung, l.leckschutz,
                l.lichte_breite, l.lichte_hoehe, l.material,
                l.nutzungsart_geplant, l.nutzungsart_ist,
                l.obj_id_abwasserbauwerk, l.obj_id_nachhaltungspunkt, l.obj_id_vonhaltungspunkt,
                l.profiltyp, l.reliner_art, l.reliner_nennweite,
                l.sanierungsbedarf, l.astatus AS status, l.wandrauhigkeit,
                l.wbw_basisjahr, l.wbw_bauart, l.wiederbeschaffungswert,
                l.zustandserhebung_jahr,
                bet.bezeichnung AS betreiber,
                eig.bezeichnung AS eigentuemer,
                kn.T_Ili_Tid AS knoten_nachref,
                kv.T_Ili_Tid AS knoten_vonref,
                ln.T_Ili_Tid AS leitung_nachref,
                rp.bezeichnung AS rohrprofilref,
                dh.bezeichnung AS datenherr,
                dl.bezeichnung AS datenlieferant,
                l.letzte_aenderung,
                CASE WHEN l.funktionhierarchisch LIKE '%.%'
                     THEN SUBSTR(l.funktionhierarchisch, 1, INSTR(l.funktionhierarchisch, '.') - 1)
                     ELSE l.funktionhierarchisch
                END AS funktionhierarchisch_klasse,
                CASE WHEN l.baujahr IS NOT NULL
                     THEN CAST((l.baujahr / 10) * 10 AS TEXT) || '-' || CAST((l.baujahr / 10) * 10 + 9 AS TEXT)
                     ELSE NULL
                END AS baujahr_klasse
            FROM "leitung" l
            LEFT JOIN "organisation" bet ON bet.T_Id = l.betreiberref
            LEFT JOIN "organisation" eig ON eig.T_Id = l.eigentuemerref
            LEFT JOIN "knoten" kn ON kn.T_Id = l.knoten_nachref
            LEFT JOIN "knoten" kv ON kv.T_Id = l.knoten_vonref
            LEFT JOIN "leitung" ln ON ln.T_Id = l.leitung_nachref
            LEFT JOIN "rohrprofil" rp ON rp.T_Id = l.rohrprofilref
            LEFT JOIN "organisation" dh ON dh.T_Id = l.datenherrref
            LEFT JOIN "organisation" dl ON dl.T_Id = l.datenlieferantref
            """;

        ExecuteNonQuery(sql);
        logger.LogDebug("Created build view '{ViewName}'.", viewName);
    }

    /// <summary>
    /// Materializes <c>ca_leitung</c> from the given build view.
    /// </summary>
    /// <param name="buildViewName">Name of the build view to read from.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    internal async Task MaterializeLeitungAsync(string buildViewName, CancellationToken cancellationToken)
    {
        if (!TableExists("leitung"))
        {
            logger.LogDebug("Table 'leitung' not found, skipping ca_leitung materialization.");
            return;
        }

        CreateLeitungTable();

        cancellationToken.ThrowIfCancellationRequested();

        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        ExecuteNonQuery("DELETE FROM ca_leitung");

        ExecuteNonQuery($"""
            INSERT INTO ca_leitung (
                tid, baujahr, baulicherzustand, bemerkung, bezeichnung,
                finanzierung, funktionhierarchisch, funktionhydraulisch,
                hoehengenauigkeit_nach, hoehengenauigkeit_von, hydr_belastung_ist,
                kote_nach, kote_von, laengeeffektiv, lagebestimmung, leckschutz,
                lichte_breite, lichte_hoehe, material,
                nutzungsart_geplant, nutzungsart_ist,
                obj_id_abwasserbauwerk, obj_id_nachhaltungspunkt, obj_id_vonhaltungspunkt,
                profiltyp, reliner_art, reliner_nennweite,
                sanierungsbedarf, status, wandrauhigkeit,
                wbw_basisjahr, wbw_bauart, wiederbeschaffungswert,
                zustandserhebung_jahr,
                betreiber, eigentuemer,
                knoten_nachref, knoten_vonref, leitung_nachref, rohrprofilref,
                datenherr, datenlieferant, letzte_aenderung,
                funktionhierarchisch_klasse, baujahr_klasse)
            SELECT
                tid, baujahr, baulicherzustand, bemerkung, bezeichnung,
                finanzierung, funktionhierarchisch, funktionhydraulisch,
                hoehengenauigkeit_nach, hoehengenauigkeit_von, hydr_belastung_ist,
                kote_nach, kote_von, laengeeffektiv, lagebestimmung, leckschutz,
                lichte_breite, lichte_hoehe, material,
                nutzungsart_geplant, nutzungsart_ist,
                obj_id_abwasserbauwerk, obj_id_nachhaltungspunkt, obj_id_vonhaltungspunkt,
                profiltyp, reliner_art, reliner_nennweite,
                sanierungsbedarf, status, wandrauhigkeit,
                wbw_basisjahr, wbw_bauart, wiederbeschaffungswert,
                zustandserhebung_jahr,
                betreiber, eigentuemer,
                knoten_nachref, knoten_vonref, leitung_nachref, rohrprofilref,
                datenherr, datenlieferant, letzte_aenderung,
                funktionhierarchisch_klasse, baujahr_klasse
            FROM "{buildViewName}"
            """);

        await transaction.CommitAsync(cancellationToken);

        var count = CountRows("ca_leitung");
        logger.LogInformation("Materialized {RowCount} rows into ca_leitung.", count);
    }

    /// <summary>
    /// Creates a build view for <c>ca_knoten</c> that denormalizes the <c>knoten</c>
    /// table by resolving foreign keys and adding computed classification columns.
    /// The view definition persists in the GeoPackage, documenting the derivation logic.
    /// </summary>
    /// <param name="viewName">Name of the build view to create.</param>
    internal void CreateKnotenBuildView(string viewName)
    {
        if (!TableExists("knoten"))
        {
            logger.LogDebug("Table 'knoten' not found, skipping view '{ViewName}'.", viewName);
            return;
        }

        var sql = $"""
            CREATE VIEW IF NOT EXISTS "{viewName}" AS
            SELECT
                k.T_Ili_Tid AS tid,
                k.ara_nr, k.baujahr, k.baulicherzustand, k.bemerkung, k.bezeichnung,
                k.deckelkote, k.dimension1, k.dimension2,
                k.finanzierung, k.funktion, k.funktionhierarchisch,
                k.lagegenauigkeit,
                k.nutzungsart_geplant, k.nutzungsart_ist,
                k.obj_id_abwasserbauwerk, k.obj_id_deckel,
                k.rueckstaukote_ist, k.sanierungsbedarf, k.sohlenkote,
                k.astatus AS status, k.symbolori, k.zugaenglichkeit,
                k.zustandserhebung_jahr,
                bet.bezeichnung AS betreiber,
                eig.bezeichnung AS eigentuemer,
                dh.bezeichnung AS datenherr,
                dl.bezeichnung AS datenlieferant,
                k.letzte_aenderung,
                CASE WHEN k.baujahr IS NOT NULL
                     THEN CAST((k.baujahr / 10) * 10 AS TEXT) || '-' || CAST((k.baujahr / 10) * 10 + 9 AS TEXT)
                     ELSE NULL
                END AS baujahr_klasse
            FROM "knoten" k
            LEFT JOIN "organisation" bet ON bet.T_Id = k.betreiberref
            LEFT JOIN "organisation" eig ON eig.T_Id = k.eigentuemerref
            LEFT JOIN "organisation" dh ON dh.T_Id = k.datenherrref
            LEFT JOIN "organisation" dl ON dl.T_Id = k.datenlieferantref
            """;

        ExecuteNonQuery(sql);
        logger.LogDebug("Created build view '{ViewName}'.", viewName);
    }

    /// <summary>
    /// Materializes <c>ca_knoten</c> from the given build view.
    /// </summary>
    /// <param name="buildViewName">Name of the build view to read from.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    internal async Task MaterializeKnotenAsync(string buildViewName, CancellationToken cancellationToken)
    {
        if (!TableExists("knoten"))
        {
            logger.LogDebug("Table 'knoten' not found, skipping ca_knoten materialization.");
            return;
        }

        CreateKnotenTable();

        cancellationToken.ThrowIfCancellationRequested();

        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        ExecuteNonQuery("DELETE FROM ca_knoten");

        ExecuteNonQuery($"""
            INSERT INTO ca_knoten (
                tid, ara_nr, baujahr, baulicherzustand, bemerkung, bezeichnung,
                deckelkote, dimension1, dimension2,
                finanzierung, funktion, funktionhierarchisch,
                lagegenauigkeit,
                nutzungsart_geplant, nutzungsart_ist,
                obj_id_abwasserbauwerk, obj_id_deckel,
                rueckstaukote_ist, sanierungsbedarf, sohlenkote,
                status, symbolori, zugaenglichkeit,
                zustandserhebung_jahr,
                betreiber, eigentuemer, datenherr, datenlieferant,
                letzte_aenderung, baujahr_klasse)
            SELECT
                tid, ara_nr, baujahr, baulicherzustand, bemerkung, bezeichnung,
                deckelkote, dimension1, dimension2,
                finanzierung, funktion, funktionhierarchisch,
                lagegenauigkeit,
                nutzungsart_geplant, nutzungsart_ist,
                obj_id_abwasserbauwerk, obj_id_deckel,
                rueckstaukote_ist, sanierungsbedarf, sohlenkote,
                status, symbolori, zugaenglichkeit,
                zustandserhebung_jahr,
                betreiber, eigentuemer, datenherr, datenlieferant,
                letzte_aenderung, baujahr_klasse
            FROM "{buildViewName}"
            """);

        await transaction.CommitAsync(cancellationToken);

        var count = CountRows("ca_knoten");
        logger.LogInformation("Materialized {RowCount} rows into ca_knoten.", count);
    }

    private static string BuildClassSelect(
        string errorsViewName,
        string className,
        string tableName,
        bool hasEnrichment,
        string cmsgColumn,
        string recommendationColumn,
        string recommendationDetailColumn)
    {
        string funktionhierarchisch;
        string eigentuemer;
        string status;
        string fromClause;

        if (hasEnrichment)
        {
            funktionhierarchisch = "f.funktionhierarchisch";
            eigentuemer = "org.bezeichnung";
            status = "f.astatus";
            fromClause = $"""
                FROM "{errorsViewName}" e
                LEFT JOIN "{tableName}" f ON f.T_Ili_Tid = e."Tid"
                LEFT JOIN "organisation" org ON org.T_Id = f.eigentuemerref
                """;
        }
        else
        {
            funktionhierarchisch = "NULL";
            eigentuemer = "NULL";
            status = "NULL";
            fromClause = $"""
                FROM "{errorsViewName}" e
                """;
        }

        return $"""
            SELECT
                e."Tid" AS tid,
                CASE WHEN e."Module" = 'reader' THEN 'ig' ELSE e.source END AS check_type,
                e."Topic" AS topic,
                e."Class" AS class,
                e."ErrorId" AS errorid,
                COALESCE(e."{cmsgColumn}", e."Description") AS error,
                e."UserAttributes" AS detail,
                {funktionhierarchisch} AS funktionhierarchisch,
                {eigentuemer} AS eigentuemer,
                {status} AS status,
                e."Category" AS category,
                e."Model" AS model,
                CASE WHEN e."Module" = 'reader' THEN 'igcheck' ELSE 'gep_check' END AS module,
                CAST(e."prio_uc" AS INTEGER) AS wk,
                CAST(e."prio_gsp" AS INTEGER) AS gep,
                e."{recommendationColumn}" AS recommendation,
                e."{recommendationDetailColumn}" AS recommendation_detail
            {fromClause}
            WHERE e."Class" = '{className}'
            """;
    }

    private static string BuildCatchAllSelect(
        string errorsViewName,
        List<string> excludedClassNames,
        string cmsgColumn,
        string recommendationColumn,
        string recommendationDetailColumn)
    {
        var whereClause = excludedClassNames.Count > 0
            ? $"""WHERE e."Class" NOT IN ({string.Join(", ", excludedClassNames.Select(c => $"'{c}'"))})"""
            : string.Empty;

        return $"""
            SELECT
                e."Tid" AS tid,
                CASE WHEN e."Module" = 'reader' THEN 'ig' ELSE e.source END AS check_type,
                e."Topic" AS topic,
                e."Class" AS class,
                e."ErrorId" AS errorid,
                COALESCE(e."{cmsgColumn}", e."Description") AS error,
                e."UserAttributes" AS detail,
                NULL AS funktionhierarchisch,
                NULL AS eigentuemer,
                NULL AS status,
                e."Category" AS category,
                e."Model" AS model,
                CASE WHEN e."Module" = 'reader' THEN 'igcheck' ELSE 'gep_check' END AS module,
                CAST(e."prio_uc" AS INTEGER) AS wk,
                CAST(e."prio_gsp" AS INTEGER) AS gep,
                e."{recommendationColumn}" AS recommendation,
                e."{recommendationDetailColumn}" AS recommendation_detail
            FROM "{errorsViewName}" e
            {whereClause}
            """;
    }

    private void CreateTables()
    {
        ExecuteNonQuery("""
            CREATE TABLE IF NOT EXISTS ca_error_data (
                fid INTEGER NOT NULL PRIMARY KEY,
                tid TEXT,
                check_type TEXT,
                topic TEXT,
                class TEXT,
                errorid TEXT,
                error TEXT,
                detail TEXT,
                funktionhierarchisch TEXT,
                eigentuemer TEXT,
                status TEXT,
                category TEXT,
                model TEXT,
                module TEXT,
                wk INTEGER,
                gep INTEGER,
                recommendation TEXT,
                recommendation_detail TEXT)
            """);

        ExecuteNonQuery("""
            CREATE TABLE IF NOT EXISTS ca_error_object (
                fid INTEGER NOT NULL PRIMARY KEY,
                tid TEXT,
                class TEXT,
                count_error INTEGER,
                wk_max INTEGER,
                gep_max INTEGER)
            """);
    }

    private void CreateLeitungTable()
    {
        ExecuteNonQuery("""
            CREATE TABLE IF NOT EXISTS ca_leitung (
                fid INTEGER NOT NULL PRIMARY KEY,
                tid TEXT,
                baujahr INTEGER,
                baulicherzustand TEXT,
                bemerkung TEXT,
                bezeichnung TEXT,
                finanzierung TEXT,
                funktionhierarchisch TEXT,
                funktionhydraulisch TEXT,
                hoehengenauigkeit_nach TEXT,
                hoehengenauigkeit_von TEXT,
                hydr_belastung_ist INTEGER,
                kote_nach REAL,
                kote_von REAL,
                laengeeffektiv REAL,
                lagebestimmung TEXT,
                leckschutz TEXT,
                lichte_breite INTEGER,
                lichte_hoehe INTEGER,
                material TEXT,
                nutzungsart_geplant TEXT,
                nutzungsart_ist TEXT,
                obj_id_abwasserbauwerk TEXT,
                obj_id_nachhaltungspunkt TEXT,
                obj_id_vonhaltungspunkt TEXT,
                profiltyp TEXT,
                reliner_art TEXT,
                reliner_nennweite INTEGER,
                sanierungsbedarf TEXT,
                status TEXT,
                wandrauhigkeit REAL,
                wbw_basisjahr INTEGER,
                wbw_bauart TEXT,
                wiederbeschaffungswert REAL,
                zustandserhebung_jahr INTEGER,
                betreiber TEXT,
                eigentuemer TEXT,
                knoten_nachref TEXT,
                knoten_vonref TEXT,
                leitung_nachref TEXT,
                rohrprofilref TEXT,
                datenherr TEXT,
                datenlieferant TEXT,
                letzte_aenderung TEXT,
                funktionhierarchisch_klasse TEXT,
                baujahr_klasse TEXT)
            """);
    }

    private void CreateKnotenTable()
    {
        ExecuteNonQuery("""
            CREATE TABLE IF NOT EXISTS ca_knoten (
                fid INTEGER NOT NULL PRIMARY KEY,
                tid TEXT,
                ara_nr INTEGER,
                baujahr INTEGER,
                baulicherzustand TEXT,
                bemerkung TEXT,
                bezeichnung TEXT,
                deckelkote REAL,
                dimension1 INTEGER,
                dimension2 INTEGER,
                finanzierung TEXT,
                funktion TEXT,
                funktionhierarchisch TEXT,
                lagegenauigkeit TEXT,
                nutzungsart_geplant TEXT,
                nutzungsart_ist TEXT,
                obj_id_abwasserbauwerk TEXT,
                obj_id_deckel TEXT,
                rueckstaukote_ist REAL,
                sanierungsbedarf TEXT,
                sohlenkote REAL,
                status TEXT,
                symbolori REAL,
                zugaenglichkeit TEXT,
                zustandserhebung_jahr INTEGER,
                betreiber TEXT,
                eigentuemer TEXT,
                datenherr TEXT,
                datenlieferant TEXT,
                letzte_aenderung TEXT,
                baujahr_klasse TEXT)
            """);
    }

    private void CreateFeatureTableIndexes()
    {
        foreach (var (_, tableName, _) in KnownFeatureClasses)
        {
            if (TableExists(tableName))
            {
                ExecuteNonQuery($"CREATE INDEX IF NOT EXISTS \"ix_{tableName}_t_ili_tid\" ON \"{tableName}\" (T_Ili_Tid)");
            }
        }
    }

    private void CreateResultIndexes()
    {
        ExecuteNonQuery("CREATE INDEX IF NOT EXISTS ix_ca_error_data_tid_class ON ca_error_data (tid, class)");
    }

    private static string LanguageColumn(string prefix, string language) =>
        string.Equals(language, "FR", StringComparison.OrdinalIgnoreCase) ? $"{prefix}_fr" : $"{prefix}_de";

    [SuppressMessage("Security", "CA2100", Justification = "Table names are internal pipeline constants, not user input.")]
    private bool TableExists(string tableName)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = @name";
        command.Parameters.AddWithValue("@name", tableName);
        return (long)(command.ExecuteScalar() ?? 0L) > 0;
    }

    [SuppressMessage("Security", "CA2100", Justification = "All SQL is built from internal constants, not user input.")]
    private void ExecuteNonQuery(string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    [SuppressMessage("Security", "CA2100", Justification = "Table names are internal pipeline constants, not user input.")]
    private long CountRows(string tableName)
    {
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM \"{tableName}\"";
        return (long)(command.ExecuteScalar() ?? 0L);
    }
}
