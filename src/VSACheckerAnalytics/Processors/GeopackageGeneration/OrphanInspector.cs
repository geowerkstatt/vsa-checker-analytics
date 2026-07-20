using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace VsaCheckerAnalytics.Processors.GeopackageGeneration;

/// <summary>
/// Detects orphan rows: checker CSV rows that no enrichment can describe, i.e. the exact complement of
/// the enriched errors. Orphans are the rows the classified view flags <c>is_known = 0</c> (an igcheck
/// row with no matching non-base error matrix entry, or a reader row with no base row for its ErrorId).
/// The same <c>is_known</c> flag drives <c>ca_error_data</c>, so the two cannot disagree. When orphans
/// exist they are materialized into a table (not a view, so it stays cheap to load) for the domain team
/// to review; when there are none, no table is created, so the mere presence of the table is the signal.
/// </summary>
internal sealed class OrphanInspector
{
    private readonly SqliteConnection connection;
    private readonly ILogger logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrphanInspector"/> class.
    /// </summary>
    /// <param name="connection">An open SQLite connection.</param>
    /// <param name="logger">Logger for the orphan warning.</param>
    internal OrphanInspector(SqliteConnection connection, ILogger logger)
    {
        this.connection = connection;
        this.logger = logger;
    }

    /// <summary>
    /// Materializes the orphan rows into <paramref name="tableName"/> and logs a warning with the
    /// affected error keys, but only when orphans exist. When there are none, no table is created,
    /// so the mere presence of the table is the signal that something needs review.
    /// </summary>
    /// <param name="tableName">Name of the orphan table to create when orphans exist.</param>
    /// <param name="classifiedViewName">Name of the classified checker CSV view (rows plus the <c>is_known</c> flag).</param>
    internal void MaterializeOrphans(string tableName, string classifiedViewName)
    {
        var orphanCount = CountUnmatched(classifiedViewName);
        if (orphanCount == 0)
        {
            return;
        }

        Materialize(tableName, classifiedViewName);

        var unmatchedKeys = GetUnmatchedKeys(tableName);
        logger.LogWarning(
            "{OrphanCount} checker error row(s) have no error matrix entry; written to {TableName} for domain review. Affected (ErrorId, Model, Class): {UnmatchedKeys}",
            orphanCount,
            tableName,
            string.Join("; ", unmatchedKeys.Select(k => $"({k.ErrorId}, {k.Model}, {k.Class})")));
    }

    [SuppressMessage("Security", "CA2100", Justification = "View and column names are internal pipeline constants, not user input.")]
    private long CountUnmatched(string classifiedViewName)
    {
        using var command = connection.CreateCommand();
        command.CommandText =
            $"""
            SELECT COUNT(*)
            FROM "{classifiedViewName}" c
            WHERE c.is_known = 0
            """;

        return (long)(command.ExecuteScalar() ?? 0L);
    }

    [SuppressMessage("Security", "CA2100", Justification = "View and table names are internal pipeline constants, not user input.")]
    private void Materialize(string tableName, string classifiedViewName)
    {
        using var command = connection.CreateCommand();
        command.CommandText =
            $"""
            CREATE TABLE "{tableName}" AS
            SELECT c.*
            FROM "{classifiedViewName}" c
            WHERE c.is_known = 0
            """;

        command.ExecuteNonQuery();
    }

    [SuppressMessage("Security", "CA2100", Justification = "Table name is an internal pipeline constant, not user input.")]
    private List<(string ErrorId, string Model, string Class)> GetUnmatchedKeys(string tableName)
    {
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT DISTINCT \"ErrorId\", \"Model\", \"Class\" FROM \"{tableName}\"";

        var keys = new List<(string ErrorId, string Model, string Class)>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            keys.Add((
                reader.IsDBNull(0) ? string.Empty : reader.GetString(0),
                reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                reader.IsDBNull(2) ? string.Empty : reader.GetString(2)));
        }

        return keys;
    }
}
