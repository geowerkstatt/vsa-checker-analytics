using Geopilot.Pipeline;
using Geopilot.PipelineCore.Pipeline;
using Microsoft.Data.Sqlite;
using VsaCheckerAnalytics.TestHelpers;

namespace VsaCheckerAnalytics.Processors.GeopackageGeneration;

[TestClass]
public class GeopackageGenerationProcessIntegrationTest
{
    /// <summary>
    /// Stands in for the result of <see cref="UpstreamStepId"/>. Only the properties the definition
    /// references via <c>${step_output(vsa_matcher....)}</c> are needed; the error matrix is not among them,
    /// it enters the step from the deployment resources via <c>${file()}</c>.
    /// </summary>
    private sealed record UpstreamStepResult(
        IPipelineFile GpkgTemplate,
        IPipelineFile[] Gep,
        IPipelineFile StandardOrgTable,
        IPipelineFile[] UserOrgTable,
        IPipelineFile[] CheckerCsvA,
        IPipelineFile[] CheckerCsvT,
        IPipelineFile[] CheckerCsvFp,
        string Language,
        string ModelVersion);

    private const string StepId = "geopackage_generation";
    private const string UpstreamStepId = "vsa_matcher";
    private static readonly string ResourceDir = Path.Combine(AppContext.BaseDirectory, "Testdata");

    private TestPipelineHost host = null!;

    /// <summary>
    /// Kept alive for the whole test: disposing the pipeline removes its working directory, and the assertions
    /// read the generated GeoPackage from there.
    /// </summary>
    private IPipeline? pipeline;

    [TestInitialize]
    public void SetUp() => host = TestPipelineHost.Create();

    [TestCleanup]
    public void Cleanup()
    {
        pipeline?.Dispose();
        host?.Dispose();
    }

    [TestMethod]
    public async Task RunGeopackageGenerationPipeline_WithoutUserOrgs()
    {
        var result = await RunDePipelineAsync(includeUserOrgs: false);

        Assert.AreEqual(StepState.Success, result.StepState);

        var generatedGeopackage = result.StepResult.ExtractProperty(nameof(GeopackageGenerationResult.GeneratedGeopackage));
        var gpkgFile = Assert.IsInstanceOfType<IPipelineFile>(generatedGeopackage);

        var gpkgPath = await gpkgFile.GetLocalPathAsync();

        Assert.IsGreaterThan(0, new FileInfo(gpkgPath).Length);

        // Orphan baseline (drift guard), exercised through the real process: these SK_* checker
        // errors are matrix-defined under the generic "SK" class, not the specific subclass, so
        // they do not join and land in ca_error_orphans. If this count changes, the fixtures,
        // the error matrix, or the join changed.
        using var connection = new SqliteConnection($"Data Source={gpkgPath};Mode=ReadOnly;Pooling=false");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM ca_error_orphans";
        Assert.AreEqual(
            34L,
            (long)(command.ExecuteScalar() ?? 0L),
            "Orphan baseline drifted: the fixtures no longer produce 34 SK_* orphans. If you changed the checker CSVs, the error matrix, or the join, update the expected count.");

        // The analytical views and tables must be registered so GIS clients discover them as
        // layers: feature views in gpkg_contents + gpkg_geometry_columns, data tables as attributes.
        command.CommandText = "SELECT data_type FROM gpkg_contents WHERE table_name = 'v_vsa_knoten'";
        Assert.AreEqual("features", command.ExecuteScalar(), "v_vsa_knoten should be registered as a feature layer.");

        command.CommandText = "SELECT data_type FROM gpkg_contents WHERE table_name = 'ca_error_data'";
        Assert.AreEqual("attributes", command.ExecuteScalar(), "ca_error_data should be registered as an attributes table.");

        command.CommandText = "SELECT data_type FROM gpkg_contents WHERE table_name = 'ca_statistics_attribute'";
        Assert.AreEqual("attributes", command.ExecuteScalar(), "ca_statistics_attribute should be registered as an attributes table.");

        command.CommandText = "SELECT COUNT(*) FROM ca_statistics_attribute";
        Assert.IsGreaterThan(0L, (long)(command.ExecuteScalar() ?? 0L));

        // 2020.1 lacks obj_id_gesamteinzugsgebiet_ist_optimiert; StatisticsViews_2020_1.sql keeps the
        // attribute row but reports a NULL count instead of referencing the missing column.
        command.CommandText = "SELECT COUNT(*) FROM ca_statistics_attribute WHERE tabelle = 'sk_regenrueckhaltebecken_kanal' AND attribut = 'obj_id_gesamteinzugsgebiet_ist_optimiert' AND anzahl_null IS NULL";
        Assert.AreEqual(1L, (long)(command.ExecuteScalar() ?? 0L), "2020.1 should keep the missing attribute row with a NULL count.");

        command.CommandText = "SELECT COUNT(*) FROM gpkg_geometry_columns WHERE table_name = 'v_vsa_knoten' AND column_name = 'geom'";
        Assert.AreEqual(1L, (long)(command.ExecuteScalar() ?? 0L), "v_vsa_knoten geometry column should be declared.");
    }

    [TestMethod]
    public async Task RunGeopackageGenerationPipeline_WithUserOrgs()
    {
        var result = await RunDePipelineAsync(includeUserOrgs: true);

        Assert.AreEqual(StepState.Success, result.StepState);

        var generatedGeopackage = result.StepResult.ExtractProperty(nameof(GeopackageGenerationResult.GeneratedGeopackage));
        var gpkgFile = Assert.IsInstanceOfType<IPipelineFile>(generatedGeopackage);

        using var stream = await gpkgFile.OpenReadAsync();
        Assert.IsGreaterThan(0, stream.Length);
    }

    [TestMethod]
    public async Task RunGeopackageGenerationPipeline_WithFrenchReaderErrors()
    {
        const string readerCsv = "reader_errors_2020_1_f_sample_fp_err.csv";
        var result = await RunPipelineAsync(includeUserOrgs: false, "FR", "2020.1", readerCsv, readerCsv, readerCsv);

        Assert.AreEqual(StepState.Success, result.StepState);

        var generatedGeopackage = result.StepResult.ExtractProperty(nameof(GeopackageGenerationResult.GeneratedGeopackage));
        var gpkgFile = Assert.IsInstanceOfType<IPipelineFile>(generatedGeopackage);

        using var connection = new SqliteConnection($"Data Source={await gpkgFile.GetLocalPathAsync()};Mode=ReadOnly;Pooling=false");
        connection.Open();

        // Six distinct real reader errors (ErrorId 15), all known via the base row, mapped reader -> igcheck.
        Assert.AreEqual(6L, CountErrorData(connection, "errorid = '15'"));
        Assert.AreEqual(6L, CountErrorData(connection, "errorid = '15' AND module = 'igcheck' AND check_type = 'ig'"));

        // French base-15 template rendered with the translated SIA405 role names on the real extracted values.
        Assert.AreEqual(1L, CountErrorData(connection, "detail LIKE '%DatenherrRef%' AND error LIKE '%MAITRE_DES_DONNEESRef%' AND error LIKE '%ch080qwzPR000018%'"));
        Assert.AreEqual(1L, CountErrorData(connection, "detail LIKE '%DatenlieferantRef%' AND error LIKE '%FOURNISSEUR_DES_DONNEESRef%'"));
        Assert.AreEqual(2L, CountErrorData(connection, "detail LIKE '%BetreiberRef%' AND error LIKE '%EXPLOITANTRef%'"));
        Assert.AreEqual(2L, CountErrorData(connection, "detail LIKE '%EigentuemerRef%' AND error LIKE '%PROPRIETAIRERef%'"));
    }

    [TestMethod]
    public async Task RunGeopackageGenerationPipeline_2020Template_MaterializesStatistics()
    {
        var result = await RunPipelineAsync(
            includeUserOrgs: false,
            "DE",
            "2020",
            "transferdatensatz_2020_1_d_LV95_T-20231205_mini_t_err.csv",
            "transferdatensatz_2020_1_d_LV95_T-20231205_mini_a_err.csv",
            "transferdatensatz_2020_1_d_LV95_T-20231205_mini_fp_err.csv");

        Assert.AreEqual(StepState.Success, result.StepState);

        var generatedGeopackage = result.StepResult.ExtractProperty(nameof(GeopackageGenerationResult.GeneratedGeopackage));
        var gpkgFile = Assert.IsInstanceOfType<IPipelineFile>(generatedGeopackage);

        using var connection = new SqliteConnection($"Data Source={await gpkgFile.GetLocalPathAsync()};Mode=ReadOnly;Pooling=false");
        connection.Open();
        using var command = connection.CreateCommand();

        // The 2020 statistics views reference columns absent in 2020.1; version selection must pick
        // StatisticsViews_2020.sql so materialization succeeds against the 2020 schema.
        command.CommandText = "SELECT data_type FROM gpkg_contents WHERE table_name = 'ca_statistics_attribute'";
        Assert.AreEqual("attributes", command.ExecuteScalar(), "ca_statistics_attribute should be registered as an attributes table.");

        command.CommandText = "SELECT COUNT(*) FROM ca_statistics_attribute";
        Assert.IsGreaterThan(0L, (long)(command.ExecuteScalar() ?? 0L));
    }

    private async Task<(StepState StepState, StepResult StepResult)> RunPipelineAsync(bool includeUserOrgs, string language, string modelVersion, string checkerCsvT, string checkerCsvA, string checkerCsvFp)
    {
        pipeline = host.CreatePipeline();
        var step = pipeline.Steps.Single(s => s.Id == StepId);

        TestPipelineHost.ReplaceDependency(step, "ili2GpkgClient", new FakeIli2GpkgClient
        {
            // pass the input gpkg content through to the output
            OutputSelector = invocation => invocation.GeoPackageContent,
        });

        var templateFile = modelVersion == "2020.1" ? "template_ca_dssmini_2020_1_d.gpkg" : "template_ca_dssmini_2020_d.gpkg";
        var upstream = new StepResult
        {
            Result = new UpstreamStepResult(
                GpkgTemplate: CopyFromTestdata(templateFile),
                Gep: [CreateFile("dssMini.xtf", "dss-bytes")],
                StandardOrgTable: CreateFile("defaultOrgs.xtf", "default-bytes"),
                UserOrgTable: includeUserOrgs ? [CreateFile("userOrgs.xtf", "user-bytes")] : [],
                CheckerCsvT: [CopyFromTestdata(checkerCsvT)],
                CheckerCsvA: [CopyFromTestdata(checkerCsvA)],
                CheckerCsvFp: [CopyFromTestdata(checkerCsvFp)],
                Language: language,
                ModelVersion: modelVersion),
        };

        var context = new PipelineContext
        {
            Upload = [],
            StepResults = new Dictionary<string, StepResult> { { UpstreamStepId, upstream } },
        };

        var stepResult = await step.Run(context, CancellationToken.None);
        return (step.State, stepResult);
    }

    private Task<(StepState StepState, StepResult StepResult)> RunDePipelineAsync(bool includeUserOrgs)
        => RunPipelineAsync(
            includeUserOrgs,
            "DE",
            "2020.1",
            "transferdatensatz_2020_1_d_LV95_T-20231205_mini_t_err.csv",
            "transferdatensatz_2020_1_d_LV95_T-20231205_mini_a_err.csv",
            "transferdatensatz_2020_1_d_LV95_T-20231205_mini_fp_err.csv");

#pragma warning disable CA2100
    private static long CountErrorData(SqliteConnection connection, string whereClause)
    {
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM ca_error_data WHERE {whereClause}";
        return (long)(command.ExecuteScalar() ?? 0L);
    }
#pragma warning restore CA2100

    private TestPipelineFile CreateFile(string fileName, string content)
    {
        var path = Path.Combine(host.WorkingDirectory, fileName);
        File.WriteAllText(path, content);
        return new TestPipelineFile(path);
    }

    private TestPipelineFile CopyFromTestdata(string fileName)
    {
        var source = Path.Combine(ResourceDir, fileName);
        var target = Path.Combine(host.WorkingDirectory, fileName);
        File.Copy(source, target, overwrite: true);
        return new TestPipelineFile(target);
    }
}
