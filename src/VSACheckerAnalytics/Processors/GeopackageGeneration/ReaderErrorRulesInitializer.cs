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
        logger.LogInformation("Applied reader error enrichment: base rows and reader_error_rules seeded.");
    }
}
