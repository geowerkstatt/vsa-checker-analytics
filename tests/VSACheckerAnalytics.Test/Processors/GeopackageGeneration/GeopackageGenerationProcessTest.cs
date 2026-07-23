using Geopilot.PipelineCore.Pipeline;
using Microsoft.Extensions.Logging.Abstractions;
using System.Reflection;
using VsaCheckerAnalytics.Ili2Gpkg;
using VsaCheckerAnalytics.Processors.GeopackageGeneration;
using VsaCheckerAnalytics.TestHelpers;

namespace VsaCheckerAnalytics.Processors.GeopackageGeneration;

[TestClass]
public sealed class GeopackageGenerationProcessTest
{
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
                Assert.IsGreaterThan(0, inv.GeoPackageText.Length, "GeoPackage stream should not be empty when client is invoked.");
                Assert.IsGreaterThan(0, inv.TransferFileText.Length, "Transfer file stream should not be empty when client is invoked.");
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
            cancellationToken: CancellationToken.None);

        Assert.HasCount(2, fake.Invocations);
        Assert.AreEqual("default-bytes", fake.Invocations[0].TransferFileText);
        Assert.AreEqual("dss-bytes", fake.Invocations[1].TransferFileText);

        Assert.AreEqual("gpkg-bytes", fake.Invocations[0].GeoPackageText);

        foreach (var inv in fake.Invocations)
        {
            Assert.IsFalse(inv.Args.DoSchemaImport);
            Assert.IsTrue(inv.Args.SkipReferenceErrors);
            Assert.IsTrue(inv.Args.SkipGeometryErrors);
            Assert.IsTrue(inv.Args.DisableValidation);
            Assert.IsTrue(inv.Args.ImportTid);
        }

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
            cancellationToken: CancellationToken.None);

        Assert.HasCount(3, fake.Invocations);
        Assert.AreEqual("default-bytes", fake.Invocations[0].TransferFileText);
        Assert.AreEqual("user-bytes", fake.Invocations[1].TransferFileText);
        Assert.AreEqual("dss-bytes", fake.Invocations[2].TransferFileText);
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
            cancellationToken: CancellationToken.None);

        Assert.IsNull(result.GeneratedGeopackage);

        var statusMessage = result.StatusMessage;
        Assert.AreEqual("GeoPackage could not be created: INTERLIS import failed.", statusMessage["en"]);
    }

    private GeopackageGenerationProcess CreateProcess(IIli2GpkgClient client)
    {
        var process = new GeopackageGenerationProcess(
            jobsDirectory: Path.GetTempPath(),
            pipelineFileManager: fileManager,
            logger: NullLogger.Instance);

        typeof(GeopackageGenerationProcess)
            .GetField("ili2GpkgClient", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(process, client);

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
