using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using VsaCheckerAnalytics.Geopackage;

namespace VsaCheckerAnalytics.Processors.GeopackageGeneration;

/// <summary>
/// Materializes <c>ca_error_data</c> and <c>ca_error_object</c> from the deduplicated checker CSV
/// union. Creates a build view that enriches each logical error (igcheck from the error matrix, reader
/// from the base rows and reader error rules) and joins the VSA feature classes for object attributes,
/// then populates the target tables via INSERT ... SELECT.
/// </summary>
internal sealed class ErrorDataMaterializer
{
    // Parameter extraction from the raw validator message (English), per reader ErrorId.
    // Offsets match the igcheck output format: "the value of " is 13 characters, so the attribute
    // name for ErrorIds 12/15/21 starts at position 14.
    private const string ExtractedAttribute = """
        CASE CAST(re."ErrorId" AS INTEGER)
            WHEN 11 THEN TRIM(SUBSTR(re."Description", 1,  INSTR(re."Description", ' has to be defined') - 1))
            WHEN 12 THEN TRIM(SUBSTR(re."Description", 14, INSTR(re."Description", ' is out of range') - 14))
            WHEN 15 THEN TRIM(SUBSTR(re."Description", 14, INSTR(re."Description", ' is out of range') - 14))
            WHEN 21 THEN TRIM(SUBSTR(re."Description", 14, INSTR(re."Description", ' is out of range') - 14))
            ELSE NULL
        END
        """;

    private const string ExtractedLength = """
        CASE CAST(re."ErrorId" AS INTEGER)
            WHEN 12 THEN TRIM(SUBSTR(re."Description",
                             INSTR(re."Description", 'too long ') + 9,
                             INSTR(re."Description", ' > ') - INSTR(re."Description", 'too long ') - 9))
            ELSE NULL
        END
        """;

    private const string ExtractedMaxOrTid = """
        CASE CAST(re."ErrorId" AS INTEGER)
            WHEN 12 THEN TRIM(SUBSTR(re."Description", INSTR(re."Description", '> ') + 2))
            WHEN 15 THEN TRIM(SUBSTR(re."Description", INSTR(re."Description", 'tid=') + 4))
            ELSE NULL
        END
        """;

    private const string ExtractedConstraint = """
        CASE CAST(re."ErrorId" AS INTEGER)
            WHEN 60 THEN TRIM(SUBSTR(re."Description",
                             INSTR(re."Description", 'constraint ') + 11,
                             INSTR(re."Description", ' failed') - INSTR(re."Description", 'constraint ') - 11))
            ELSE NULL
        END
        """;

    private const string ExtractedAttrs = """
        CASE CAST(re."ErrorId" AS INTEGER)
            WHEN 70 THEN TRIM(SUBSTR(re."Description",
                             INSTR(re."Description", 'constraint ') + 11,
                             INSTR(re."Description", ' (values=') - INSTR(re."Description", 'constraint ') - 11))
            ELSE NULL
        END
        """;

    // Feature tables indexed on T_Ili_Tid to speed up the object-attribute joins in the build view.
    private static readonly string[] FeatureTableNames =
    [
        "leitung",
        "knoten",
        "teileinzugsgebiet",
        "sk_regenueberlauf",
        "sk_regenueberlaufbecken",
        "sk_einleitstelle",
        "ueberlauf_foerderaggregat",
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
    /// Creates the build view that maps every deduplicated, known checker error onto the
    /// <c>ca_error_data</c> target shape. Errors are deduplicated on
    /// (<c>Tid</c>, <c>Module</c>, <c>Description</c>) so the same logical error reported by several
    /// profiles collapses into one row with an aggregated <c>check_type</c>. Only rows the classified
    /// view flags <c>is_known = 1</c> are included: igcheck errors are enriched from <c>error_matrix</c>
    /// (class/model-specific row wins), reader errors resolve their message, priority and recommendation
    /// through a three-tier lookup (condition override, attribute override, base row) after extracting
    /// parameters from the validator message. Suppressed reader errors are excluded. Unknown errors are
    /// left out entirely and surface via the orphan detection, not here. The raw validator description
    /// is used only as a message fallback for an included row whose template is blank.
    /// </summary>
    /// <param name="viewName">Name of the build view to create.</param>
    /// <param name="classifiedViewName">Name of the classified checker CSV view (union rows plus the <c>is_known</c> flag).</param>
    /// <param name="errorMatrixTable">Name of the error matrix table (igcheck rows plus <c>base</c> reader rows).</param>
    /// <param name="readerRulesTable">Name of the reader error rules table (overrides and suppression).</param>
    /// <param name="language">Language code (<c>"DE"</c>, <c>"FR"</c> or <c>"IT"</c>) selecting the localized columns.</param>
    internal void CreateBuildView(
        string viewName,
        string classifiedViewName,
        string errorMatrixTable,
        string readerRulesTable,
        string language)
    {
        var l = LanguageSuffix(language);
        var classColumn = ClassColumn(language);
        var attrValue = AttrValueExpression(l);
        var objectAttributes = BuildObjectAttributeCte();

        var errorTemplate =
            $"CASE WHEN r.module = 'reader' THEN COALESCE(ov_c.msg_template_{l}, ov_a.msg_template_{l}, base.cmsg_{l}, r.description) ELSE COALESCE(em.cmsg_{l}, r.description) END";
        var recommendationTemplate =
            $"CASE WHEN r.module = 'reader' THEN COALESCE(ov_c.recommendation_{l}, ov_a.recommendation_{l}, base.required_action_{l}) ELSE em.required_action_{l} END";
        var recommendationDetailTemplate =
            $"CASE WHEN r.module = 'reader' THEN COALESCE(ov_c.recommendation_detail_{l}, ov_a.recommendation_detail_{l}, base.action_context_{l}) ELSE em.action_context_{l} END";

        var checkType = AggregatedCheckTypeExpression();

        var sql = $"""
            CREATE VIEW "{viewName}" AS
            WITH grp AS (
                SELECT
                    MIN(t_id)                                    AS fid,
                    "Tid"                                        AS tid,
                    "Module"                                     AS module,
                    "Description"                                AS description,
                    MAX(CASE WHEN source = 'A'  THEN 1 ELSE 0 END) AS has_a,
                    MAX(CASE WHEN source = 'FP' THEN 1 ELSE 0 END) AS has_fp,
                    MAX(CASE WHEN source = 'T'  THEN 1 ELSE 0 END) AS has_t
                FROM "{classifiedViewName}"
                GROUP BY "Tid", "Module", "Description"
            ),
            rep AS (
                SELECT
                    g.fid, g.tid, g.module, g.description, g.has_a, g.has_fp, g.has_t,
                    re."ErrorId" AS errorid,
                    re."Class"   AS class,
                    re."Model"   AS model,
                    re."Topic"   AS topic,
                    re.is_known  AS is_known,
                    {ExtractedAttribute} AS xattr,
                    {ExtractedLength}    AS xn,
                    {ExtractedMaxOrTid}  AS xmax_or_tid,
                    {ExtractedConstraint} AS xconstraint,
                    {ExtractedAttrs}     AS xattrs
                FROM grp g
                JOIN "{classifiedViewName}" re ON re.t_id = g.fid
            ),
            obj AS (
                {objectAttributes}
            ),
            mm AS (
                SELECT
                    r.fid,
                    (SELECT em.t_id
                       FROM "{errorMatrixTable}" em
                      WHERE em.cid = r.errorid
                        AND em.checkmodel != 'base'
                        AND (em."{classColumn}" IS NULL OR em."{classColumn}" = '' OR em."{classColumn}" = r.class)
                        AND (em.model IS NULL OR em.model = r.model)
                      ORDER BY em."{classColumn}" IS NULL ASC
                      LIMIT 1) AS em_tid
                FROM rep r
                WHERE r.module = 'igcheck'
            )
            SELECT
                r.tid                                            AS tid,
                {checkType}                                      AS check_type,
                r.topic                                          AS topic,
                r.class                                          AS class,
                r.errorid                                        AS errorid,
                {Render(errorTemplate, attrValue)}               AS error,
                CASE WHEN r.module = 'reader' THEN r.description ELSE '' END AS detail,
                obj.funktionhierarchisch                         AS funktionhierarchisch,
                obj.eigentuemer                                  AS eigentuemer,
                obj.status                                       AS status,
                CASE WHEN r.module = 'reader' THEN base.ccat ELSE em.ccat END AS category,
                r.model                                          AS model,
                CASE WHEN r.module = 'reader' THEN 'igcheck' ELSE 'gep_check' END AS module,
                CASE WHEN r.module = 'reader'
                     THEN COALESCE(ov_c.wk, ov_a.wk, CAST(base.prio_uc AS INTEGER))
                     ELSE CAST(em.prio_uc AS INTEGER) END        AS wk,
                CASE WHEN r.module = 'reader'
                     THEN COALESCE(ov_c.gep, ov_a.gep, CAST(base.prio_gsp AS INTEGER))
                     ELSE CAST(em.prio_gsp AS INTEGER) END        AS gep,
                {Render(recommendationTemplate, attrValue)}      AS recommendation,
                {Render(recommendationDetailTemplate, attrValue)} AS recommendation_detail
            FROM rep r
            LEFT JOIN obj ON obj.fid = r.fid
            LEFT JOIN mm  ON mm.fid = r.fid
            LEFT JOIN "{errorMatrixTable}" em ON em.t_id = mm.em_tid
            LEFT JOIN "{errorMatrixTable}" base
                   ON r.module = 'reader' AND base.cid = r.errorid AND base.checkmodel = 'base'
            LEFT JOIN "{readerRulesTable}" ov_a
                   ON r.module = 'reader'
                  AND ov_a.error_id = CAST(r.errorid AS INTEGER)
                  AND ov_a.attr_name = r.xattr
                  AND ov_a.condition_col IS NULL
            LEFT JOIN "{readerRulesTable}" ov_c
                   ON r.module = 'reader'
                  AND ov_c.error_id = CAST(r.errorid AS INTEGER)
                  AND ov_c.attr_name = r.xattr
                  AND ov_c.condition_col IS NOT NULL
                  AND (
                        (ov_c.condition_col = 'funktionhierarchisch' AND obj.funktionhierarchisch = ov_c.condition_val)
                     OR (ov_c.condition_col = 'eigentuemer'          AND obj.eigentuemer          = ov_c.condition_val)
                      )
            WHERE r.is_known = 1 AND COALESCE(ov_c.suppress, ov_a.suppress, 0) = 0
            """;

        ExecuteNonQuery(sql);
        GeopackageMetadata.RegisterAttributes(connection, viewName);
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
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        CreateTables();
        GeopackageMetadata.RegisterAttributes(connection, "ca_error_data");
        GeopackageMetadata.RegisterAttributes(connection, "ca_error_object");
        CreateFeatureTableIndexes();

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

    // Renders the {ATTR}/{N}/{MAX}/{TID}/{ATTRS}/{CONSTRAINT} placeholders into a message template.
    // Each REPLACE is a no-op when its token is absent, so the same rendering is safe for static
    // messages and for igcheck rows that carry no placeholders. Built without string interpolation so
    // the literal braces of the placeholder tokens survive.
    private static string Render(string templateExpression, string attrValueExpression) =>
        "REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE("
        + templateExpression
        + ", '{ATTR}', COALESCE(" + attrValueExpression + ", ''))"
        + ", '{N}', COALESCE(r.xn, ''))"
        + ", '{MAX}', COALESCE(r.xmax_or_tid, ''))"
        + ", '{TID}', COALESCE(r.xmax_or_tid, ''))"
        + ", '{ATTRS}', COALESCE(r.xattrs, ''))"
        + ", '{CONSTRAINT}', COALESCE(r.xconstraint, ''))";

    // The extracted attribute name substituted for {ATTR}. For FR/IT the German SIA405 administrative
    // role names are translated to the target language (verified against the localized .ili models).
    private static string AttrValueExpression(string languageSuffix) => languageSuffix switch
    {
        "fr" => """
            CASE r.xattr
                WHEN 'BetreiberRef'                 THEN 'EXPLOITANTRef'
                WHEN 'BueroRef'                     THEN 'BUREAURef'
                WHEN 'DatenherrRef'                 THEN 'MAITRE_DES_DONNEESRef'
                WHEN 'DatenlieferantRef'            THEN 'FOURNISSEUR_DES_DONNEESRef'
                WHEN 'EigentuemerRef'               THEN 'PROPRIETAIRERef'
                WHEN 'StandortgemeindeRef'          THEN 'COMMUNE_IMPLANTATIONRef'
                WHEN 'TraegerschaftRef'             THEN 'ORGANISME_RESPONSABLERef'
                WHEN 'Verantwortlich_AusloesungRef' THEN 'RESPONSABLE_DECLENCHEMENTRef'
                ELSE r.xattr
            END
            """,
        "it" => """
            CASE r.xattr
                WHEN 'BetreiberRef'                 THEN 'gestoreRef'
                WHEN 'BueroRef'                     THEN 'ufficioRef'
                WHEN 'DatenherrRef'                 THEN 'proprietario_datiRef'
                WHEN 'DatenlieferantRef'            THEN 'fornitore_datiRef'
                WHEN 'EigentuemerRef'               THEN 'proprietarioRef'
                WHEN 'StandortgemeindeRef'          THEN 'comune_appartenenzaRef'
                WHEN 'TraegerschaftRef'             THEN 'ente_gestoreRef'
                WHEN 'Verantwortlich_AusloesungRef' THEN 'responsabile_attivazioneRef'
                ELSE r.xattr
            END
            """,
        _ => "r.xattr",
    };

    // Aggregates the profiles that reported an igcheck error into a single label
    // (e.g. "vsa-a; vsa-fp; vsa-t"); reader errors are profile-independent and become "ig".
    private static string AggregatedCheckTypeExpression()
    {
        const string parts =
            "(CASE WHEN r.has_a = 1 THEN 'vsa-a; ' ELSE '' END) || "
            + "(CASE WHEN r.has_fp = 1 THEN 'vsa-fp; ' ELSE '' END) || "
            + "(CASE WHEN r.has_t = 1 THEN 'vsa-t; ' ELSE '' END)";

        return $"CASE WHEN r.module = 'reader' THEN 'ig' ELSE SUBSTR({parts}, 1, LENGTH({parts}) - 2) END";
    }

    // Object attributes (funktionhierarchisch / eigentuemer / status) joined from the feature tables
    // present in this GeoPackage. Only Leitung and Knoten carry these; other classes stay NULL.
    private string BuildObjectAttributeCte()
    {
        var leitung = TableExists("leitung");
        var knoten = TableExists("knoten");
        var organisation = TableExists("organisation");

        if (!leitung && !knoten)
        {
            return "SELECT r.fid, NULL AS funktionhierarchisch, NULL AS eigentuemer, NULL AS status FROM rep r";
        }

        var funktionParts = new List<string>();
        var statusParts = new List<string>();
        var eigentuemerRefParts = new List<string>();
        var joins = new List<string>();

        if (leitung)
        {
            funktionParts.Add("lt.funktionhierarchisch");
            statusParts.Add("lt.astatus");
            eigentuemerRefParts.Add("lt.eigentuemerref");
            joins.Add("LEFT JOIN leitung lt ON r.class = 'Leitung' AND lt.T_Ili_Tid = r.tid");
        }

        if (knoten)
        {
            funktionParts.Add("kn.funktionhierarchisch");
            statusParts.Add("kn.astatus");
            eigentuemerRefParts.Add("kn.eigentuemerref");
            joins.Add("LEFT JOIN knoten kn ON r.class = 'Knoten' AND kn.T_Ili_Tid = r.tid");
        }

        var funktion = Coalesce(funktionParts);
        var status = Coalesce(statusParts);
        var eigentuemer = "NULL";

        if (organisation)
        {
            joins.Add($"LEFT JOIN organisation org ON org.T_Id = {Coalesce(eigentuemerRefParts)}");
            eigentuemer = "org.bezeichnung";
        }

        return $"""
            SELECT r.fid,
                   {funktion} AS funktionhierarchisch,
                   {eigentuemer} AS eigentuemer,
                   {status} AS status
            FROM rep r
            {string.Join("\n            ", joins)}
            """;
    }

    private static string Coalesce(List<string> expressions) =>
        expressions.Count == 1 ? expressions[0] : $"COALESCE({string.Join(", ", expressions)})";

    private void CreateTables()
    {
        ExecuteNonQuery("""
            CREATE TABLE ca_error_data (
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
            CREATE TABLE ca_error_object (
                fid INTEGER NOT NULL PRIMARY KEY,
                tid TEXT,
                class TEXT,
                count_error INTEGER,
                wk_max INTEGER,
                gep_max INTEGER)
            """);
    }

    private void CreateFeatureTableIndexes()
    {
        foreach (var tableName in FeatureTableNames)
        {
            if (TableExists(tableName))
            {
                ExecuteNonQuery($"CREATE INDEX \"ix_{tableName}_t_ili_tid\" ON \"{tableName}\" (T_Ili_Tid)");
            }
        }
    }

    private void CreateResultIndexes()
    {
        ExecuteNonQuery("CREATE INDEX ix_ca_error_data_tid_class ON ca_error_data (tid, class)");
    }

    private static string LanguageSuffix(string language) => language?.ToUpperInvariant() switch
    {
        "FR" => "fr",
        "IT" => "it",
        _ => "de",
    };

    private static string ClassColumn(string language) =>
        string.Equals(language, "FR", StringComparison.OrdinalIgnoreCase) ? "class_fr" : "class_de";

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
