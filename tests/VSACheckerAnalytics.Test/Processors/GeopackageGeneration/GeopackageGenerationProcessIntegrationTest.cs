using Geopilot.Pipeline;
using Geopilot.Pipeline.Config;
using Geopilot.Pipeline.Process;
using Geopilot.PipelineCore.Pipeline;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using VsaCheckerAnalytics.TestHelpers;

namespace VsaCheckerAnalytics.Processors.GeopackageGeneration;

[TestClass]
public class GeopackageGenerationProcessIntegrationTest
{
    private const string GeopackageGenerationImplementation = "VsaCheckerAnalytics.Processors.GeopackageGeneration.GeopackageGenerationProcess";
    private const string UpstreamStepId = "vsa_matcher";
    private static readonly string PluginDllPath = typeof(GeopackageGenerationProcess).Assembly.Location;
    private static readonly string ResourceDir = Path.Combine(AppContext.BaseDirectory, "Testdata");

    private static readonly List<OutputConfig> GeopackageGenerationOutputs =
    [
        new() { Take = "generatedGeopackage", As = "generatedGeopackage" },
    ];

    private static readonly IReadOnlyDictionary<string, InputValue> GeopackageGenerationInputs =
        new Dictionary<string, InputValue>
        {
            ["geoPackage"] = new InputValue.StepOutputReference(UpstreamStepId, "gpkg_template"),
            ["dssMiniXtf"] = new InputValue.StepOutputReference(UpstreamStepId, "gep"),
            ["defaultOrgsXtf"] = new InputValue.StepOutputReference(UpstreamStepId, "standard_org_table"),
            ["userOrgsXtf"] = new InputValue.StepOutputReference(UpstreamStepId, "user_org_table"),
            ["checkerCsvT"] = new InputValue.StepOutputReference(UpstreamStepId, "checker_csv_t"),
            ["checkerCsvA"] = new InputValue.StepOutputReference(UpstreamStepId, "checker_csv_a"),
            ["checkerCsvFp"] = new InputValue.StepOutputReference(UpstreamStepId, "checker_csv_fp"),
            ["errorMatrix"] = new InputValue.StepOutputReference(UpstreamStepId, "error_matrix"),
            ["language"] = new InputValue.StepOutputReference(UpstreamStepId, "language"),
        };

    private PipelineProcessFactory pipelineProcessFactory = null!;
    private Mock<ILoggerFactory> loggerFactoryMock = null!;
    private string tempDir = null!;
    private string jobsDir = null!;
    private FakeIli2GpkgWorker worker = null!;

    [TestInitialize]
    public void SetUp()
    {
        tempDir = Path.Combine(Path.GetTempPath(), "gpkg-gen-integration-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(tempDir);

        jobsDir = Path.Combine(tempDir, "ili2gpkg-jobs");
        Directory.CreateDirectory(jobsDir);

        worker = new FakeIli2GpkgWorker(jobsDir);
        worker.Start();

        var pipelineOptions = new PipelineOptions
        {
            Definition = "unused",
            Plugins = [PluginDllPath],
            ProcessConfigs = new Dictionary<string, Parameterization>
            {
                {
                    GeopackageGenerationImplementation, new Parameterization
                    {
                        { "jobsDirectory", jobsDir },
                    }
                },
            },
        };

        var pipelineOptionsMock = new Mock<IOptions<PipelineOptions>>();
        pipelineOptionsMock.SetupGet(o => o.Value).Returns(pipelineOptions);

        loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(new Mock<ILogger>().Object);

        pipelineProcessFactory = new PipelineProcessFactory(pipelineOptionsMock.Object, loggerFactoryMock.Object);
    }

    [TestCleanup]
    public void Cleanup()
    {
        worker?.Dispose();
        pipelineProcessFactory?.Dispose();
        if (Directory.Exists(tempDir))
        {
            try { Directory.Delete(tempDir, recursive: true); }
            catch (IOException) { }
        }
    }

    [TestMethod]
    public async Task RunGeopackageGenerationPipeline_WithoutUserOrgs()
    {
        var result = await RunDePipelineAsync(includeUserOrgs: false);

        Assert.AreEqual(StepState.Success, result.StepState);

        var gpkgFile = result.StepResult.Outputs["generatedGeopackage"].Data as IPipelineFile;
        Assert.IsNotNull(gpkgFile);

        string gpkgPath;
        using (var stream = gpkgFile.OpenReadFileStream())
        {
            Assert.IsGreaterThan(0, stream.Length);
            gpkgPath = stream.Name;
        }

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

        command.CommandText = "SELECT COUNT(*) FROM gpkg_geometry_columns WHERE table_name = 'v_vsa_knoten' AND column_name = 'geom'";
        Assert.AreEqual(1L, (long)(command.ExecuteScalar() ?? 0L), "v_vsa_knoten geometry column should be declared.");
    }

    [TestMethod]
    public async Task RunGeopackageGenerationPipeline_WithUserOrgs()
    {
        var result = await RunDePipelineAsync(includeUserOrgs: true);

        Assert.AreEqual(StepState.Success, result.StepState);

        var gpkgFile = result.StepResult.Outputs["generatedGeopackage"].Data as IPipelineFile;
        Assert.IsNotNull(gpkgFile);

        using var stream = gpkgFile.OpenReadFileStream();
        Assert.IsGreaterThan(0, stream.Length);
    }

    [TestMethod]
    public async Task RunGeopackageGenerationPipeline_WithFrenchReaderErrors()
    {
        const string readerCsv = "reader_errors_2020_1_f_sample_fp_err.csv";
        var result = await RunPipelineAsync(includeUserOrgs: false, "FR", readerCsv, readerCsv, readerCsv);

        Assert.AreEqual(StepState.Success, result.StepState);

        var gpkgFile = result.StepResult.Outputs["generatedGeopackage"].Data as IPipelineFile;
        Assert.IsNotNull(gpkgFile);

        string gpkgPath;
        using (var stream = gpkgFile.OpenReadFileStream())
        {
            gpkgPath = stream.Name;
        }

        using var connection = new SqliteConnection($"Data Source={gpkgPath};Mode=ReadOnly;Pooling=false");
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

    private async Task<(StepState StepState, StepResult StepResult)> RunPipelineAsync(bool includeUserOrgs, string language, string checkerCsvT, string checkerCsvA, string checkerCsvFp)
    {
        var stepConfig = new StepConfig
        {
            Id = "geopackage_generation",
            DisplayName = new Dictionary<string, string> { { "en", "GeoPackage Generation" } },
            ProcessId = "geopackage_generation",
            Output = GeopackageGenerationOutputs,
        };

        var processes = new List<ProcessConfig>
        {
            new() { Id = "geopackage_generation", Implementation = GeopackageGenerationImplementation },
        };

        var process = pipelineProcessFactory.Builder()
            .PipelineId("test")
            .StepConfig(stepConfig)
            .Processes(processes)
            .PipelineDirectory(tempDir)
            .JobId(Guid.NewGuid())
            .Build();

        using var step = PipelineStep.Builder()
            .Id("geopackage_generation")
            .DisplayName(new Dictionary<string, string> { { "en", "GeoPackage Generation" } })
            .Inputs(GeopackageGenerationInputs)
            .OutputConfig(GeopackageGenerationOutputs)
            .Process(process)
            .Logger(new Mock<ILogger>().Object)
            .Build();

        var upstream = new StepResult();
        upstream.Outputs["gpkg_template"] = FileOutput(CopyFromTestdata("template_ca_dssmini_2020_1_d.gpkg"));
        upstream.Outputs["gep"] = FileOutput(CreateFile("dssMini.xtf", "dss-bytes"));
        upstream.Outputs["standard_org_table"] = FileOutput(CreateFile("defaultOrgs.xtf", "default-bytes"));
        upstream.Outputs["user_org_table"] = includeUserOrgs
            ? FileOutput(CreateFile("userOrgs.xtf", "user-bytes"))
            : FileOutput();
        upstream.Outputs["checker_csv_t"] = FileOutput(CopyFromTestdata(checkerCsvT));
        upstream.Outputs["checker_csv_a"] = FileOutput(CopyFromTestdata(checkerCsvA));
        upstream.Outputs["checker_csv_fp"] = FileOutput(CopyFromTestdata(checkerCsvFp));
        upstream.Outputs["error_matrix"] = FileOutput(CopyFromTestdata("errorMatrix.xlsx"));
        upstream.Outputs["language"] = new StepOutput { Data = language, Action = [] };

        var context = new PipelineContext
        {
            Upload = new PipelineFileList([]),
            StepResults = new Dictionary<string, StepResult> { { UpstreamStepId, upstream } },
        };

        var stepResult = await step.Run(context, CancellationToken.None);
        return (step.State, stepResult);
    }

    private static StepOutput FileOutput(params IPipelineFile[] files)
        => new() { Data = files, Action = [] };

    private Task<(StepState StepState, StepResult StepResult)> RunDePipelineAsync(bool includeUserOrgs)
        => RunPipelineAsync(
            includeUserOrgs,
            "DE",
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
        var path = Path.Combine(tempDir, fileName);
        File.WriteAllText(path, content);
        return new TestPipelineFile(path);
    }

    private TestPipelineFile CopyFromTestdata(string fileName)
    {
        var source = Path.Combine(ResourceDir, fileName);
        var target = Path.Combine(tempDir, fileName);
        File.Copy(source, target, overwrite: true);
        return new TestPipelineFile(target);
    }

    /// <summary>
    /// Watches the ili2gpkg jobs directory and responds to each submitted job by writing
    /// success.log + output.ready, so the real <c>Ili2GpkgClient</c> proceeds without a
    /// real worker container. The input <c>dbfile.gpkg</c> already contains a valid SQLite
    /// database (the template gpkg), so we leave it in place to be copied back as the result.
    /// </summary>
    private sealed class FakeIli2GpkgWorker : IDisposable
    {
        private readonly string jobsDir;
        private readonly CancellationTokenSource cts = new();
        private Task? loop;

        public FakeIli2GpkgWorker(string jobsDir)
        {
            this.jobsDir = jobsDir;
        }

        public void Start()
        {
            loop = Task.Run(() => RunLoopAsync(cts.Token));
        }

        private async Task RunLoopAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    foreach (var jobDir in Directory.EnumerateDirectories(jobsDir))
                    {
                        var inputReady = Path.Combine(jobDir, "input.ready");
                        var outputReady = Path.Combine(jobDir, "output.ready");
                        if (File.Exists(inputReady) && !File.Exists(outputReady))
                        {
                            File.WriteAllText(Path.Combine(jobDir, "success.log"), "fake worker ok");
                            File.WriteAllText(outputReady, string.Empty);
                        }
                    }
                }
                catch (DirectoryNotFoundException)
                {
                    // Jobs dir was cleaned up; exit.
                    return;
                }
                catch (IOException)
                {
                    // Transient — keep polling.
                }

                await Task.Delay(20, cancellationToken).ConfigureAwait(false);
            }
        }

        public void Dispose()
        {
            cts.Cancel();
            try { loop?.Wait(TimeSpan.FromSeconds(2)); }
            catch (AggregateException) { }
            cts.Dispose();
        }
    }
}
