using Geopilot.PipelineCore.Ilitools;
using Geopilot.PipelineCore.Pipeline;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text;
using VsaCheckerAnalytics.TestHelpers;

namespace VsaCheckerAnalytics.Processors.GeopackageGeneration;

[TestClass]
public sealed class GeopackageGenerationProcessTest
{
    private static readonly string[] TransferFilesWithoutUserOrgs = ["default-bytes", "dss-bytes"];
    private static readonly string[] TransferFilesWithUserOrgs = ["default-bytes", "user-bytes", "dss-bytes"];

    private TestFileFactory fileFactory = null!;
    private TestPipelineFileManager fileManager = null!;

    [TestInitialize]
    public void Setup()
    {
        fileFactory = new TestFileFactory();
        fileManager = new TestPipelineFileManager();
    }

    [TestCleanup]
    public void Cleanup()
    {
        fileManager.Dispose();
        fileFactory.Dispose();
    }

    [TestMethod]
    public async Task RunAsync_WithoutUserOrgs()
    {
        var gpkg = fileFactory.CreateFile("schema.gpkg", "gpkg-bytes");
        var dssMini = fileFactory.CreateFile("dssMini.xtf", "dss-bytes");
        var defaultOrgs = fileFactory.CreateFile("defaultOrgs.xtf", "default-bytes");

        var fake = new FakeIli2GpkgClient
        {
            OnInvocation = inv =>
            {
                Assert.IsGreaterThan(0, inv.GeoPackageContent.Length, "GeoPackage stream should not be empty when client is invoked.");
                Assert.HasCount(2, inv.TransferFileTexts, "Two transfer files should be passed when client is invoked.");
                Assert.DoesNotContain("", inv.TransferFileTexts, "No transfer file should be empty.");
            },
        };
        var process = CreateProcess(fake);

        var result = await process.RunAsync(
            gpkg,
            dssMini,
            defaultOrgs,
            null,
            CreateTestCsvT(),
            CreateTestCsvA(),
            CreateTestCsvFp(),
            CreateTestErrorMatrix(),
            "DE",
            "2020",
            cancellationToken: CancellationToken.None);

        Assert.HasCount(1, fake.Invocations);
        CollectionAssert.AreEqual(TransferFilesWithoutUserOrgs, fake.Invocations[0].TransferFileTexts.ToArray());

        Assert.AreEqual("gpkg-bytes", Encoding.UTF8.GetString(fake.Invocations[0].GeoPackageContent));

        var args = fake.Invocations[0].Args;
        Assert.IsTrue(args.SkipReferenceErrors);
        Assert.IsTrue(args.SkipGeometryErrors);
        Assert.IsTrue(args.DisableValidation);
        Assert.IsTrue(args.ImportTid);
        Assert.IsTrue(args.StrokeArcs);

        var output = result.GeneratedGeopackage;
        Assert.IsNotNull(output);
        using var stream = output.OpenReadFileStream();
        Assert.IsGreaterThan(0, stream.Length);

        var statusMessage = result.StatusMessage;
        Assert.AreEqual("GeoPackage created and enriched with checker data, error matrix and analysis views.", statusMessage["en"]);
    }

    [TestMethod]
    public async Task RunAsync_WithUserOrgs()
    {
        var gpkg = fileFactory.CreateFile("schema.gpkg", "gpkg-bytes");
        var dssMini = fileFactory.CreateFile("dssMini.xtf", "dss-bytes");
        var defaultOrgs = fileFactory.CreateFile("defaultOrgs.xtf", "default-bytes");
        var userOrgs = fileFactory.CreateFile("userOrgs.xtf", "user-bytes");

        var fake = new FakeIli2GpkgClient();
        var process = CreateProcess(fake);

        await process.RunAsync(
            gpkg,
            dssMini,
            defaultOrgs,
            userOrgs,
            CreateTestCsvT(),
            CreateTestCsvA(),
            CreateTestCsvFp(),
            CreateTestErrorMatrix(),
            "DE",
            "2020",
            cancellationToken: CancellationToken.None);

        Assert.HasCount(1, fake.Invocations);
        CollectionAssert.AreEqual(TransferFilesWithUserOrgs, fake.Invocations[0].TransferFileTexts.ToArray());
    }

    [TestMethod]
    public async Task RunAsync_WhenImportFails()
    {
        var gpkg = fileFactory.CreateFile("schema.gpkg", "gpkg-bytes");
        var dssMini = fileFactory.CreateFile("dssMini.xtf", "dss-bytes");
        var defaultOrgs = fileFactory.CreateFile("defaultOrgs.xtf", "default-bytes");

        var fake = new FakeIli2GpkgClient { ResultSelector = _ => false };
        var process = CreateProcess(fake);

        var result = await process.RunAsync(
            gpkg,
            dssMini,
            defaultOrgs,
            null,
            CreateTestCsvT(),
            CreateTestCsvA(),
            CreateTestCsvFp(),
            CreateTestErrorMatrix(),
            "DE",
            "2020",
            cancellationToken: CancellationToken.None);

        Assert.IsNull(result.GeneratedGeopackage);

        var statusMessage = result.StatusMessage;
        Assert.AreEqual("GeoPackage could not be created: INTERLIS import failed.", statusMessage["en"]);
    }

    private GeopackageGenerationProcess CreateProcess(IIli2GpkgClient client)
    {
        var process = new GeopackageGenerationProcess(
            client,
            pipelineFileManager: fileManager,
            logger: NullLogger.Instance);

        return process;
    }

    private static string GetTestdataPath(string fileName)
        => Path.Combine(AppContext.BaseDirectory, "Testdata", fileName);

    private IPipelineFile CreateTestCsvT()
        => fileFactory.CreateFile("t_err.csv", File.ReadAllText(GetTestdataPath("transferdatensatz_2020_1_d_LV95_T-20231205_mini_t_err.csv")));

    private IPipelineFile CreateTestCsvA()
        => fileFactory.CreateFile("a_err.csv", File.ReadAllText(GetTestdataPath("transferdatensatz_2020_1_d_LV95_T-20231205_mini_a_err.csv")));

    private IPipelineFile CreateTestCsvFp()
        => fileFactory.CreateFile("fp_err.csv", File.ReadAllText(GetTestdataPath("transferdatensatz_2020_1_d_LV95_T-20231205_mini_fp_err.csv")));

    private TestPipelineFile CreateTestErrorMatrix()
        => new TestPipelineFile(GetTestdataPath("errorMatrix.xlsx"));
}
