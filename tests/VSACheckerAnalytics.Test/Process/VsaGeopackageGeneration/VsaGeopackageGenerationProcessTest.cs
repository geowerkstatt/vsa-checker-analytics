using Geopilot.PipelineCore.Pipeline;
using Microsoft.Extensions.Logging.Abstractions;
using System.Reflection;
using VsaCheckerAnalytics.Ili2Gpkg;
using VsaCheckerAnalytics.TestHelpers;

namespace VsaCheckerAnalytics.Process.VsaGeopackageGeneration;

[TestClass]
public sealed class VsaGeopackageGenerationProcessTest
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
                Assert.IsTrue(File.Exists(inv.GeoPackagePath), $"GeoPackage path should exist when client is invoked: {inv.GeoPackagePath}");
                Assert.IsTrue(File.Exists(inv.TransferFilePath), $"Transfer file path should exist when client is invoked: {inv.TransferFilePath}");
            },
        };
        using var process = CreateProcess(fake);

        var result = await process.RunAsync([gpkg], [dssMini], [defaultOrgs], [null], CancellationToken.None);

        Assert.HasCount(2, fake.Invocations);
        Assert.AreEqual("defaultOrgs.xtf", Path.GetFileName(fake.Invocations[0].TransferFilePath));
        Assert.AreEqual("dssMini.xtf", Path.GetFileName(fake.Invocations[1].TransferFilePath));

        foreach (var inv in fake.Invocations)
        {
            Assert.IsFalse(inv.Args.DoSchemaImport);
            Assert.IsTrue(inv.Args.SkipReferenceErrors);
            Assert.IsTrue(inv.Args.SkipGeometryErrors);
            Assert.IsTrue(inv.Args.DisableValidation);
            Assert.IsTrue(inv.Args.ImportTid);
        }

        var output = (IPipelineFile)result["generatedGeopackage"]!;
        Assert.IsNotNull(output);
        using var stream = output.OpenReadFileStream();
        Assert.IsGreaterThan(0, stream.Length);
    }

    [TestMethod]
    public async Task RunAsync_WithUserOrgs()
    {
        var gpkg = fileFactory.CreateFile("schema.gpkg", "gpkg-bytes");
        var dssMini = fileFactory.CreateFile("dssMini.xtf", "dss-bytes");
        var defaultOrgs = fileFactory.CreateFile("defaultOrgs.xtf", "default-bytes");
        var userOrgs = fileFactory.CreateFile("userOrgs.xtf", "user-bytes");

        var fake = new FakeIli2GpkgClient
        {
            OnInvocation = inv =>
            {
                Assert.IsTrue(File.Exists(inv.GeoPackagePath), $"GeoPackage path should exist when client is invoked: {inv.GeoPackagePath}");
                Assert.IsTrue(File.Exists(inv.TransferFilePath), $"Transfer file path should exist when client is invoked: {inv.TransferFilePath}");
            },
        };
        using var process = CreateProcess(fake);

        await process.RunAsync([gpkg], [dssMini], [defaultOrgs], [userOrgs], CancellationToken.None);

        Assert.HasCount(3, fake.Invocations);
        Assert.AreEqual("defaultOrgs.xtf", Path.GetFileName(fake.Invocations[0].TransferFilePath));
        Assert.AreEqual("userOrgs.xtf", Path.GetFileName(fake.Invocations[1].TransferFilePath));
        Assert.AreEqual("dssMini.xtf", Path.GetFileName(fake.Invocations[2].TransferFilePath));
    }

    [TestMethod]
    public async Task RunAsync_WhenImportFails()
    {
        var gpkg = fileFactory.CreateFile("schema.gpkg", "gpkg-bytes");
        var dssMini = fileFactory.CreateFile("dssMini.xtf", "dss-bytes");
        var defaultOrgs = fileFactory.CreateFile("defaultOrgs.xtf", "default-bytes");

        var fake = new FakeIli2GpkgClient { ResultSelector = _ => false };
        using var process = CreateProcess(fake);

        var result = await process.RunAsync([gpkg], [dssMini], [defaultOrgs], [null], CancellationToken.None);

        Assert.IsNull(result["generatedGeopackage"]);
    }

    private VsaGeopackageGenerationProcess CreateProcess(IIli2GpkgClient client)
    {
        var process = new VsaGeopackageGenerationProcess(
            jobsDirectory: Path.GetTempPath(),
            pipelineFileManager: fileManager,
            logger: NullLogger.Instance);

        typeof(VsaGeopackageGenerationProcess)
            .GetField("ili2GpkgClient", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(process, client);

        return process;
    }
}
