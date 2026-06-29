using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using VsaCheckerAnalytics.Geopackage;

namespace VsaCheckerAnalytics.Processors.GeopackageGeneration;

/// <summary>
/// Materializes <c>ca_error_data</c> and <c>ca_error_object</c> from the checker errors
/// view by joining with VSA feature classes. Creates a build view that maps each feature
/// class to a common schema, then populates the target tables via INSERT ... SELECT.
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

        var sql = $"CREATE VIEW \"{viewName}\" AS\n{string.Join("\nUNION ALL\n", selects)}";

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
        foreach (var (_, tableName, _) in KnownFeatureClasses)
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
