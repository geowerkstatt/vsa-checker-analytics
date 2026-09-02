using Geopilot.Pipeline;
using Geopilot.PipelineCore.Pipeline;
using Microsoft.Data.Sqlite;
using VsaCheckerAnalytics.TestHelpers;

namespace VsaCheckerAnalytics.Processors.ErrorOverviewExport;

[TestClass]
public class ErrorOverviewExportProcessIntegrationTest
{
    private const string StepId = "error_overview_export_de";
    private const string GeopackageGenerationStepId = "geopackage_generation";
    private const string VsaMatcherStepId = "vsa_matcher";

    /// <summary>
    /// Stands in for the result of <see cref="GeopackageGenerationStepId"/>. Only the property the definition references
    /// via <c>${step_output(geopackage_generation.GeneratedGeopackage)}</c> is needed.
    /// </summary>
    private sealed record GeopackageGenerationStepResult(IPipelineFile GeneratedGeopackage);

    /// <summary>
    /// Stands in for the result of <see cref="VsaMatcherStepId"/>.
    /// </summary>
    private sealed record VsaMatcherStepResult(string? Language);

    private TestPipelineHost host = null!;

    [TestInitialize]
    public void SetUp() => host = TestPipelineHost.Create();

    [TestCleanup]
    public void Cleanup() => host?.Dispose();

    [TestMethod]
    public async Task RunErrorOverviewExportStep()
    {
        using var pipeline = host.CreatePipeline();
        var step = pipeline.Steps.Single(s => s.Id == StepId);

        var context = new PipelineContext
        {
            Upload = [],
            StepResults = new Dictionary<string, StepResult>
            {
                [VsaMatcherStepId] = new() { Result = new VsaMatcherStepResult("DE") },
                [GeopackageGenerationStepId] = new() { Result = new GeopackageGenerationStepResult(CreateTestGeoPackage()) },
            },
        };

        var result = await step.Run(context, CancellationToken.None);

        Assert.AreEqual(StepState.Success, step.State);

        var errorOverview = Assert.IsInstanceOfType<IPipelineFile>(result.ExtractProperty(nameof(ErrorOverviewExportResult.ErrorOverview)));
        using var overviewStream = await errorOverview.OpenReadAsync();
        Assert.IsGreaterThan(0, overviewStream.Length);

        // The canton matrix is filled from the template wired in the definition via ${file()}.
        var cantonMatrix = Assert.IsInstanceOfType<IPipelineFile>(result.ExtractProperty(nameof(ErrorOverviewExportResult.CantonErrorMatrix)));
        using var cantonStream = await cantonMatrix.OpenReadAsync();
        Assert.IsGreaterThan(0, cantonStream.Length);
    }

    private TestPipelineFile CreateTestGeoPackage()
    {
        var gpkgPath = Path.Combine(host.WorkingDirectory, $"test-{Guid.NewGuid():N}.gpkg");

        using var connection = new SqliteConnection($"Data Source={gpkgPath};Pooling=false");
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE ca_error_data (
                fid INTEGER NOT NULL PRIMARY KEY,
                tid TEXT, check_type TEXT, topic TEXT, class TEXT,
                errorid TEXT, error TEXT, detail TEXT,
                function_hierarchic TEXT, owner TEXT, status TEXT,
                category TEXT, model TEXT, module TEXT,
                uc INTEGER, gsp INTEGER,
                recommendation TEXT, recommendation_detail TEXT);

            CREATE TABLE ca_error_object (
                fid INTEGER NOT NULL PRIMARY KEY,
                tid TEXT, class TEXT,
                count_error INTEGER, uc_max INTEGER, gsp_max INTEGER);

            INSERT INTO ca_error_data (tid, class, errorid, uc, gsp, error, check_type, function_hierarchic, owner, status, fid)
            VALUES ('LT001', 'Leitung', 't_001', 1, 2, 'Fehler 1', 'Traegerschaft', 'primaer', 'Gemeinde', 'in_Betrieb', 1),
                   ('LT001', 'Leitung', 'a_001', 2, 1, 'Fehler 2', 'ARA', 'sekundaer', 'Kanton', 'geplant', 2);

            INSERT INTO ca_error_object (tid, class, count_error, uc_max, gsp_max)
            VALUES ('LT001', 'Leitung', 2, 2, 2);
            """;
        cmd.ExecuteNonQuery();

        return new TestPipelineFile(gpkgPath);
    }
}
