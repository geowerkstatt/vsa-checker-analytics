using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using VsaCheckerAnalytics.Geopackage;

namespace VsaCheckerAnalytics.Processors.GeopackageGeneration;

/// <summary>
/// Applies the geowerkstatt-maintained reader-error knowledge on top of the imported igcheck matrix:
/// the Italian and error-type columns, the category-level <c>base</c> rows for reader ErrorIds, and the
/// <c>reader_error_rules</c> override / suppression table. The content lives in the embedded
/// <c>ReaderErrorEnrichment.sql</c> resource so domain edits stay in one reviewable place.
/// </summary>
internal sealed class ReaderErrorRulesInitializer
{
    private readonly SqliteConnection connection;
    private readonly ILogger logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReaderErrorRulesInitializer"/> class.
    /// </summary>
    /// <param name="connection">An open SQLite connection to the GeoPackage.</param>
    /// <param name="logger">Logger for diagnostic messages.</param>
    internal ReaderErrorRulesInitializer(SqliteConnection connection, ILogger logger)
    {
        this.connection = connection;
        this.logger = logger;
    }

    /// <summary>
    /// Extends <c>error_matrix</c> with the base reader rows and creates the seeded
    /// <c>reader_error_rules</c> table. Must run after the igcheck matrix has been imported and before
    /// the analytics views are built.
    /// </summary>
    internal void Initialize()
    {
        EmbeddedSql.Execute(connection, "ReaderErrorEnrichment.sql");
        GeopackageMetadata.RegisterAttributes(connection, "reader_error_rules");
        EnsureEveryRuleHasBaseRow();
        logger.LogInformation("Applied reader error enrichment: base rows and reader_error_rules seeded.");
    }

    /// <summary>
    /// Verifies that every <c>reader_error_rules</c> row targets a reader ErrorId that has a <c>base</c>
    /// row in <c>error_matrix</c>. A rule without a base row would be classified as unknown (an orphan)
    /// instead of enriched, so a mismatch is a seeding mistake in <c>ReaderErrorEnrichment.sql</c> and
    /// must fail loudly rather than silently drop the affected errors.
    /// </summary>
    /// <exception cref="InvalidOperationException">A rule references an ErrorId with no base row.</exception>
    internal void EnsureEveryRuleHasBaseRow()
    {
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT DISTINCT r.error_id
            FROM reader_error_rules r
            WHERE NOT EXISTS (
                SELECT 1 FROM error_matrix b
                WHERE b.checkmodel = 'base' AND b.cid = CAST(r.error_id AS TEXT))
            """;

        var missing = new List<long>();
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                missing.Add(reader.GetInt64(0));
            }
        }

        if (missing.Count > 0)
        {
            throw new InvalidOperationException(
                $"reader_error_rules references ErrorId(s) without a base row in error_matrix: [{string.Join(", ", missing)}]. " +
                "Every rule must target an ErrorId that has a base row in ReaderErrorEnrichment.sql.");
        }
    }
}
