using Geopilot.Pipeline;
using Geopilot.PipelineCore.Pipeline;
using Moq;
using Moq.Protected;
using System.Net;
using VsaCheckerAnalytics.TestHelpers;

namespace VsaCheckerAnalytics.Processors.VsaMatcher;

[TestClass]
public class VsaMatcherIntegrationTest
{
    private const string StepId = "vsa_matcher";
    private const string UpstreamStepId = "gep_checker_unzipper";

    /// <summary>
    /// Stands in for the result of <see cref="UpstreamStepId"/>. Only the property the definition references
    /// via <c>${step_output(gep_checker_unzipper.ExtractedFiles)}</c> is needed.
    /// </summary>
    private sealed record UpstreamStepResult(IPipelineFile[] ExtractedFiles);

    private TestPipelineHost host = null!;
    private Mock<HttpMessageHandler> httpMessageHandlerMock = null!;
    private HttpClient httpClient = null!;

    [TestInitialize]
    public void SetUp()
    {
        host = TestPipelineHost.Create();

        httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        httpMessageHandlerMock.Protected().Setup("Dispose", ItExpr.IsAny<bool>());
        SetupOrgTableMock(TestPipelineHost.VsaOrgTableUrl2020);
        SetupOrgTableMock(TestPipelineHost.VsaOrgTableUrl20201);
        httpClient = new HttpClient(httpMessageHandlerMock.Object);
    }

    [TestCleanup]
    public void Cleanup()
    {
        httpClient?.Dispose();
        host?.Dispose();
    }

    [TestMethod]
    public async Task RunVsaMatcherStep()
    {
        using var pipeline = host.CreatePipeline();
        var step = pipeline.Steps.Single(s => s.Id == StepId);
        TestPipelineHost.ReplaceDependency(step, "httpClient", httpClient);

        var context = new PipelineContext
        {
            Upload = [new TestPipelineFile(CreateGepXtf("VSADSSMINI_2020_LV95"))],
            StepResults = new Dictionary<string, StepResult>
            {
                [UpstreamStepId] = new()
                {
                    Result = new UpstreamStepResult([
                        CreateCsvFile("gep_a_err.csv"),
                        CreateCsvFile("gep_fp_err.csv"),
                        CreateCsvFile("gep_t_err.csv"),
                    ]),
                },
            },
        };

        var result = await step.Run(context, CancellationToken.None);

        Assert.AreEqual(StepState.Success, step.State);

        var gepFiles = Assert.IsInstanceOfType<IPipelineFile[]>(result.ExtractProperty(nameof(VsaMatcherResult.Gep)));
        Assert.HasCount(1, gepFiles);

        Assert.AreEqual("2020", result.ExtractProperty(nameof(VsaMatcherResult.ModelVersion)));
        Assert.AreEqual("DE", result.ExtractProperty(nameof(VsaMatcherResult.Language)));

        var checkerCsvA = Assert.IsInstanceOfType<IPipelineFile[]>(result.ExtractProperty(nameof(VsaMatcherResult.CheckerCsvA)));
        var checkerCsvFp = Assert.IsInstanceOfType<IPipelineFile[]>(result.ExtractProperty(nameof(VsaMatcherResult.CheckerCsvFp)));
        var checkerCsvT = Assert.IsInstanceOfType<IPipelineFile[]>(result.ExtractProperty(nameof(VsaMatcherResult.CheckerCsvT)));
        Assert.HasCount(1, checkerCsvA);
        Assert.HasCount(1, checkerCsvFp);
        Assert.HasCount(1, checkerCsvT);

        // Selected from the templates the deployment ships, addressed through the app-settings config layer.
        Assert.IsNotNull(result.ExtractProperty(nameof(VsaMatcherResult.GpkgTemplate)));
        Assert.IsNotNull(result.ExtractProperty(nameof(VsaMatcherResult.StandardOrgTable)));
    }

    private void SetupOrgTableMock(string url)
    {
#pragma warning disable CA2000 // Response is disposed by the code under test
        _ = httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri != null &&
                    req.RequestUri.ToString() == url),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("<org-table/>"),
            });
#pragma warning restore CA2000
    }

    private TestPipelineFile CreateCsvFile(string fileName)
    {
        var path = Path.Combine(host.WorkingDirectory, "check", fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, "col1;col2\nval1;val2");
        return new TestPipelineFile(path, "check");
    }

    private string CreateGepXtf(string modelName)
    {
        var xtf = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n"
            + "<ili:transfer xmlns:ili=\"http://www.interlis.ch/xtf/2.4/INTERLIS\">\n"
            + "  <ili:headersection>\n"
            + "    <ili:models>\n"
            + $"      <ili:model>{modelName}</ili:model>\n"
            + "    </ili:models>\n"
            + "    <ili:sender>IntegrationTest</ili:sender>\n"
            + "  </ili:headersection>\n"
            + "  <ili:datasection/>\n"
            + "</ili:transfer>";

        var path = Path.Combine(host.WorkingDirectory, "gep.xtf");
        File.WriteAllText(path, xtf);
        return path;
    }
}
