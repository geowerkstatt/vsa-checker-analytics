using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using VsaCheckerAnalytics.Geopackage;

namespace VsaCheckerAnalytics.Processors.GeopackageGeneration;

/// <summary>
/// Materializes a statistics view into a plain table and registers it as a GeoPackage
/// <c>attributes</c> layer. The analytical statistics views (e.g. <c>v_statistics_attribute</c>)
/// recompute their aggregates on every query; materializing one into a table gives downstream
/// consumers (the canton error export) a stable, cheap-to-read snapshot and a QGIS-visible layer,
/// mirroring how <c>ca_error_data</c> is materialized from its build view. When the GeoPackage
/// carries no DSS feature tables (e.g. a minimal fixture without an ili2gpkg schema import) there
/// is nothing to aggregate, so materialization is skipped, mirroring <c>ErrorDataMaterializer</c>'s
/// degradation when the feature classes are absent.
/// </summary>
internal sealed class StatisticsMaterializer
{
    private readonly SqliteConnection connection;
    private readonly ILogger logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="StatisticsMaterializer"/> class.
    /// </summary>
    /// <param name="connection">An open SQLite connection.</param>
    /// <param name="logger">Logger for the materialization summary.</param>
    internal StatisticsMaterializer(SqliteConnection connection, ILogger logger)
    {
        this.connection = connection;
        this.logger = logger;
    }

    /// <summary>
    /// Materializes <paramref name="viewName"/> into a new table <paramref name="tableName"/> and
    /// registers it in the GeoPackage metadata as a non-spatial <c>attributes</c> layer. The physical
    /// row order of the copy is not treated as a contract; consumers that need a specific order select
    /// an explicit ordering column (<c>v_statistics_attribute</c> carries <c>sortierung</c> for that
    /// purpose). Assumes the table does not yet exist, because each pipeline run starts from a fresh
    /// GeoPackage copy. Does nothing when the GeoPackage has no DSS feature tables to aggregate.
    /// </summary>
    /// <param name="tableName">Name of the table to create.</param>
    /// <param name="viewName">Name of the statistics view to materialize.</param>
    [SuppressMessage("Security", "CA2100", Justification = "Table and view names are internal pipeline constants, not user input.")]
    internal void Materialize(string tableName, string viewName)
    {
        if (!TableExists("leitung") && !TableExists("knoten"))
        {
            logger.LogInformation(
                "Skipping {TableName}: no DSS feature tables present to build {ViewName} from.",
                tableName,
                viewName);
            return;
        }

        using (var command = connection.CreateCommand())
        {
            command.CommandText = $"CREATE TABLE \"{tableName}\" AS SELECT * FROM \"{viewName}\"";
            command.ExecuteNonQuery();
        }

        GeopackageMetadata.RegisterAttributes(connection, tableName);

        logger.LogInformation("Materialized {RowCount} row(s) into {TableName}.", CountRows(tableName), tableName);
    }

    [SuppressMessage("Security", "CA2100", Justification = "Table name is an internal pipeline constant, not user input.")]
    private long CountRows(string tableName)
    {
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM \"{tableName}\"";
        return (long)(command.ExecuteScalar() ?? 0L);
    }

    private bool TableExists(string tableName)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = @name";
        command.Parameters.AddWithValue("@name", tableName);
        return (long)(command.ExecuteScalar() ?? 0L) > 0;
    }
}
