using Geopilot.Pipeline;
using Geopilot.Pipeline.Config;
using Geopilot.Pipeline.Process;
using Geopilot.PipelineCore.Pipeline;
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

    private static readonly List<InputConfig> GeopackageGenerationInputs =
    [
        new() { From = UpstreamStepId, Take = "gpkg_template", As = "geoPackage" },
        new() { From = UpstreamStepId, Take = "gep", As = "dssMiniXtf" },
        new() { From = UpstreamStepId, Take = "standard_org_table", As = "defaultOrgsXtf" },
        new() { From = UpstreamStepId, Take = "user_org_table", As = "userOrgsXtf" },
        new() { From = UpstreamStepId, Take = "checker_csv_t", As = "checkerCsvT" },
        new() { From = UpstreamStepId, Take = "checker_csv_a", As = "checkerCsvA" },
        new() { From = UpstreamStepId, Take = "checker_csv_fp", As = "checkerCsvFp" },
        new() { From = UpstreamStepId, Take = "error_matrix", As = "errorMatrix" },
        new() { From = UpstreamStepId, Take = "language", As = "language" },
    ];

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
        var result = await RunPipelineAsync(includeUserOrgs: false);

        Assert.AreEqual(StepState.Success, result.StepState);

        var gpkgFile = result.StepResult.Outputs["generatedGeopackage"].Data as IPipelineFile;
        Assert.IsNotNull(gpkgFile);

        using var stream = gpkgFile.OpenReadFileStream();
        Assert.IsGreaterThan(0, stream.Length);
    }

    [TestMethod]
    public async Task RunGeopackageGenerationPipeline_WithUserOrgs()
    {
        var result = await RunPipelineAsync(includeUserOrgs: true);

        Assert.AreEqual(StepState.Success, result.StepState);

        var gpkgFile = result.StepResult.Outputs["generatedGeopackage"].Data as IPipelineFile;
        Assert.IsNotNull(gpkgFile);

        using var stream = gpkgFile.OpenReadFileStream();
        Assert.IsGreaterThan(0, stream.Length);
    }

    private async Task<(StepState StepState, StepResult StepResult)> RunPipelineAsync(bool includeUserOrgs)
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
            .InputConfig(GeopackageGenerationInputs)
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
        upstream.Outputs["checker_csv_t"] = FileOutput(CopyFromTestdata("transferdatensatz_2020_1_d_LV95_T-20231205_mini_t_err.csv"));
        upstream.Outputs["checker_csv_a"] = FileOutput(CopyFromTestdata("transferdatensatz_2020_1_d_LV95_T-20231205_mini_a_err.csv"));
        upstream.Outputs["checker_csv_fp"] = FileOutput(CopyFromTestdata("transferdatensatz_2020_1_d_LV95_T-20231205_mini_fp_err.csv"));
        upstream.Outputs["error_matrix"] = FileOutput(CopyFromTestdata("errorMatrix.xlsx"));
        upstream.Outputs["language"] = new StepOutput { Data = "DE", Action = [] };

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
