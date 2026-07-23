using Geopilot.Pipeline;
using Geopilot.Pipeline.Config;
using Geopilot.Pipeline.Process;
using Geopilot.PipelineCore.Pipeline;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Diagnostics.CodeAnalysis;
using VsaCheckerAnalytics.TestHelpers;

namespace VsaCheckerAnalytics.Processors.ErrorOverviewExport;

[TestClass]
public class ErrorOverviewExportProcessIntegrationTest
{
    private const string ErrorOverviewExportImplementation = "VsaCheckerAnalytics.Processors.ErrorOverviewExport.ErrorOverviewExportProcess";
    private const string UpstreamStepId = "geopackage_generation";
    private static readonly string PluginDllPath = typeof(ErrorOverviewExportProcess).Assembly.Location;

    private static readonly List<OutputConfig> ErrorOverviewExportOutputs =
    [
        new() { Take = "ErrorOverview", As = "errorOverview" },
    ];

    private static readonly IReadOnlyDictionary<string, InputValue> ErrorOverviewExportInputs =
        new Dictionary<string, InputValue>
        {
            ["geopackage"] = new InputValue.StepOutputReference(UpstreamStepId, "generatedGeopackage"),
        };

    private PipelineProcessFactory pipelineProcessFactory = null!;
    private string tempDir = null!;

    [TestInitialize]
    public void SetUp()
    {
        tempDir = Path.Combine(Path.GetTempPath(), "error-export-integration-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(tempDir);

        var pipelineOptions = new PipelineOptions
        {
            Definition = "unused",
            Plugins = [PluginDllPath],
            ProcessConfigs = new Dictionary<string, Parameterization>
            {
                {
                    ErrorOverviewExportImplementation, new Parameterization
                    {
                        { "errorDataSheet", "Error Data" },
                        {
                            "errorDataAttributeMapping", new Parameterization
                            {
                                { "tid", "TID" },
                                { "class", "Klasse" },
                                { "errorid", "Fehler-ID" },
                                { "wk", "WK" },
                                { "gep", "GEP" },
                                { "error", "Fehlertyp" },
                                { "check_type", "Prueftyp" },
                                { "funktionhierarchisch", "Funktionhierarchisch" },
                                { "eigentuemer", "Eigentuemer" },
                                { "status", "Status" },
                                { "fid", "FID" },
                            }
                        },
                        {
                            "errorDataColumnMapping", new Parameterization
                            {
                                { "tid", "A" },
                                { "class", "B" },
                                { "errorid", "C" },
                                { "wk", "D" },
                                { "gep", "E" },
                                { "error", "F" },
                                { "check_type", "G" },
                                { "funktionhierarchisch", "H" },
                                { "eigentuemer", "I" },
                                { "status", "J" },
                                { "fid", "K" },
                            }
                        },
                        { "errorObjectSheet", "Error Object" },
                        {
                            "errorObjectAttributeMapping", new Parameterization
                            {
                                { "tid", "TID" },
                                { "class", "Klasse" },
                                { "count_error", "Anzahl Fehler" },
                                { "wk_max", "WK Maximum" },
                                { "gep_max", "GEP Maximum" },
                            }
                        },
                        {
                            "errorObjectColumnMapping", new Parameterization
                            {
                                { "tid", "A" },
                                { "class", "B" },
                                { "count_error", "C" },
                                { "wk_max", "D" },
                                { "gep_max", "E" },
                            }
                        },
                        { "overviewWkSheet", "Uebersicht WK" },
                        { "overviewGepSheet", "Uebersicht GEP" },
                        { "overviewRowFields", new List<object> { "class", "error" } },
                        { "overviewFilterFields", new List<object> { "check_type", "funktionhierarchisch", "eigentuemer", "status" } },
                        { "overviewValueField", "fid" },
                        { "overviewValueName", "Anzahl Fehler" },
                    }
                },
            },
        };

        var pipelineOptionsMock = new Mock<IOptions<PipelineOptions>>();
        pipelineOptionsMock.SetupGet(o => o.Value).Returns(pipelineOptions);

        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(new Mock<ILogger>().Object);

        pipelineProcessFactory = new PipelineProcessFactory(pipelineOptionsMock.Object, loggerFactoryMock.Object);
    }

    [TestCleanup]
    public void Cleanup()
    {
        pipelineProcessFactory?.Dispose();
        if (Directory.Exists(tempDir))
        {
            try { Directory.Delete(tempDir, recursive: true); }
            catch (IOException) { }
        }
    }

    [TestMethod]
    public async Task RunErrorOverviewExportPipeline()
    {
        var stepConfig = new StepConfig
        {
            Id = "error_overview_export",
            DisplayName = new Dictionary<string, string> { { "en", "Error Overview Export" } },
            ProcessId = "error_overview_export",
            Output = ErrorOverviewExportOutputs,
        };

        var processes = new List<ProcessConfig>
        {
            new() { Id = "error_overview_export", Implementation = ErrorOverviewExportImplementation },
        };

        var process = pipelineProcessFactory.Builder()
            .PipelineId("test")
            .StepConfig(stepConfig)
            .Processes(processes)
            .PipelineDirectory(tempDir)
            .JobId(Guid.NewGuid())
            .Build();

        using var step = PipelineStep.Builder()
            .Id("error_overview_export")
            .DisplayName(new Dictionary<string, string> { { "en", "Error Overview Export" } })
            .Inputs(ErrorOverviewExportInputs)
            .OutputConfig(ErrorOverviewExportOutputs)
            .Process(process)
            .Logger(new Mock<ILogger>().Object)
            .Build();

        var upstream = new StepResult();
        upstream.Outputs["generatedGeopackage"] = FileOutput(CreateTestGeoPackage());

        var context = new PipelineContext
        {
            Upload = [],
            StepResults = new Dictionary<string, StepResult> { { UpstreamStepId, upstream } },
        };

        var result = await step.Run(context, CancellationToken.None);

        Assert.AreEqual(StepState.Success, step.State);

        var outputFile = result.Outputs["errorOverview"].Data as IPipelineFile;
        Assert.IsNotNull(outputFile);

        using var stream = outputFile.OpenReadFileStream();
        Assert.IsGreaterThan(0, stream.Length);
    }

    private static StepOutput FileOutput(params IPipelineFile[] files)
        => new() { Data = files, Action = [] };

    [SuppressMessage("Security", "CA2100", Justification = "Test queries use hardcoded SQL, not user input.")]
    private TestPipelineFile CreateTestGeoPackage()
    {
        var gpkgPath = Path.Combine(tempDir, $"test-{Guid.NewGuid():N}.gpkg");

        using var connection = new SqliteConnection($"Data Source={gpkgPath};Pooling=false");
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE ca_error_data (
                fid INTEGER NOT NULL PRIMARY KEY,
                tid TEXT, check_type TEXT, topic TEXT, class TEXT,
                errorid TEXT, error TEXT, detail TEXT,
                funktionhierarchisch TEXT, eigentuemer TEXT, status TEXT,
                category TEXT, model TEXT, module TEXT,
                wk INTEGER, gep INTEGER,
                recommendation TEXT, recommendation_detail TEXT);

            CREATE TABLE ca_error_object (
                fid INTEGER NOT NULL PRIMARY KEY,
                tid TEXT, class TEXT,
                count_error INTEGER, wk_max INTEGER, gep_max INTEGER);

            INSERT INTO ca_error_data (tid, class, errorid, wk, gep, error, check_type, funktionhierarchisch, eigentuemer, status, fid)
            VALUES ('LT001', 'Leitung', 't_001', 1, 2, 'Fehler 1', 'Traegerschaft', 'primaer', 'Gemeinde', 'in_Betrieb', 1),
                   ('LT001', 'Leitung', 'a_001', 2, 1, 'Fehler 2', 'ARA', 'sekundaer', 'Kanton', 'geplant', 2);

            INSERT INTO ca_error_object (tid, class, count_error, wk_max, gep_max)
            VALUES ('LT001', 'Leitung', 2, 2, 2);
            """;
        cmd.ExecuteNonQuery();

        return new TestPipelineFile(gpkgPath);
    }
}
