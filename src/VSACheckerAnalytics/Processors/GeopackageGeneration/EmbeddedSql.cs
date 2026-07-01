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
    [SuppressMessage("Security", "CA2100", Justification = "SQL is loaded from an embedded resource compiled into the assembly, not user input.")]
    internal static void Execute(SqliteConnection connection, string fileName)
    {
        var resourceName = $"VsaCheckerAnalytics.EmbeddedResources.{fileName}";

        var assembly = typeof(EmbeddedSql).Assembly;
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException(
                $"Embedded SQL resource '{resourceName}' not found. Available resources: " +
                $"[{string.Join(", ", assembly.GetManifestResourceNames())}].");

        using var reader = new StreamReader(stream);
        var sql = reader.ReadToEnd();

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }
}
