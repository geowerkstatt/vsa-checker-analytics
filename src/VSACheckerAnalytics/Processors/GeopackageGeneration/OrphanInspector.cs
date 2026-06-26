using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace VsaCheckerAnalytics.Processors.GeopackageGeneration;

/// <summary>
/// Detects orphan rows: checker CSV rows that have no matching entry in the error matrix. Orphans
/// are derived from the checker errors view (the inner join of the CSV union and the error matrix),
/// so the match definition is not duplicated here. When orphans exist they are materialized into a
/// table (not a view, so it stays cheap to load) for the domain team to review.
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
    /// <param name="unionViewName">Name of the checker CSV union view (all CSV rows).</param>
    /// <param name="errorsViewName">Name of the checker errors view (matched rows only).</param>
    internal void MaterializeOrphans(string tableName, string unionViewName, string errorsViewName)
    {
        var orphanCount = CountUnmatched(unionViewName, errorsViewName);
        if (orphanCount == 0)
        {
            return;
        }

        Materialize(tableName, unionViewName, errorsViewName);

        var unmatchedKeys = GetUnmatchedKeys(tableName);
        logger.LogWarning(
            "{OrphanCount} checker error row(s) have no error matrix entry; written to {TableName} for domain review. Affected (ErrorId, Model, Class): {UnmatchedKeys}",
            orphanCount,
            tableName,
            string.Join("; ", unmatchedKeys.Select(k => $"({k.ErrorId}, {k.Model}, {k.Class})")));
    }

    [SuppressMessage("Security", "CA2100", Justification = "View names are internal pipeline constants, not user input.")]
    private long CountUnmatched(string unionViewName, string errorsViewName)
    {
        using var command = connection.CreateCommand();
        command.CommandText =
            $"""
            SELECT COUNT(*)
            FROM "{unionViewName}" c
            WHERE c."t_id" NOT IN (SELECT "t_id" FROM "{errorsViewName}")
            """;

        return (long)(command.ExecuteScalar() ?? 0L);
    }

    [SuppressMessage("Security", "CA2100", Justification = "Table and view names are internal pipeline constants, not user input.")]
    private void Materialize(string tableName, string unionViewName, string errorsViewName)
    {
        using var command = connection.CreateCommand();
        command.CommandText =
            $"""
            CREATE TABLE "{tableName}" AS
            SELECT c.*
            FROM "{unionViewName}" c
            WHERE c."t_id" NOT IN (SELECT "t_id" FROM "{errorsViewName}")
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
