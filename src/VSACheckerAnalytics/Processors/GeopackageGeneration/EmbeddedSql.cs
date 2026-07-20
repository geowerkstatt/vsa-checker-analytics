using Microsoft.Data.Sqlite;
using System.Diagnostics.CodeAnalysis;

namespace VsaCheckerAnalytics.Processors.GeopackageGeneration;

/// <summary>
/// Loads and executes SQL scripts embedded in the assembly under the
/// <c>VsaCheckerAnalytics.EmbeddedResources</c> namespace. The whole script runs as a single
/// multi-statement batch.
/// </summary>
internal static class EmbeddedSql
{
    /// <summary>
    /// Reads the embedded SQL resource <paramref name="fileName"/> and executes its whole content as a
    /// single multi-statement batch on <paramref name="connection"/>.
    /// </summary>
    /// <param name="connection">An open SQLite connection.</param>
    /// <param name="fileName">File name of the embedded <c>.sql</c> resource, without the namespace prefix.</param>
    [SuppressMessage("Security", "CA2100", Justification = "SQL is loaded from an embedded resource compiled into the assembly, not user input.")]
    internal static void Execute(SqliteConnection connection, string fileName)
    {
        using var stream = EmbeddedResource.OpenRead(fileName);
        using var reader = new StreamReader(stream);
        var sql = reader.ReadToEnd();

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }
}
