using Geopilot.PipelineCore.Pipeline;
using Microsoft.Extensions.Logging.Abstractions;
using System.Reflection;
using VsaCheckerAnalytics.TestHelpers;

namespace VsaCheckerAnalytics.Processors.VsaMatcher;

[TestClass]
public sealed class VsaMatcherProcessTest
{
    private TestFileFactory fileFactory = null!;
    private TestPipelineFileManager fileManager = null!;
    private FakeHttpMessageHandler httpHandler = null!;
    private HttpClient httpClient = null!;

    private const string OrgTableUrl2020 = "http://test.local/org_2020.xtf";
    private const string OrgTableUrl20201 = "http://test.local/org_2020_1.xtf";
    private const string OrgTableContent = "<org-table-content/>";

    [TestInitialize]
    public void Setup()
    {
        fileFactory = new TestFileFactory();
        fileManager = new TestPipelineFileManager();
        httpHandler = new FakeHttpMessageHandler();
        httpHandler.Register(OrgTableUrl2020, OrgTableContent);
        httpHandler.Register(OrgTableUrl20201, OrgTableContent);
        httpClient = new HttpClient(httpHandler);
    }

    [TestCleanup]
    public void Cleanup()
    {
        httpClient.Dispose();
        fileManager.Dispose();
        fileFactory.Dispose();
    }

    [TestMethod]
    public async Task RunAsync_IdentifiesGep2020()
    {
        var gepFile = fileFactory.CreateXtf24("gep.xtf", "VSADSSMINI_2020_LV95");
        IPipelineFile[] uploads = [gepFile];
        var process = CreateProcess();

        var result = await process.RunAsync(uploads, [], CancellationToken.None);

        var gepFiles = result.Gep;
        Assert.AreSame(gepFile, gepFiles.Single());
        Assert.AreEqual("2020", result.ModelVersion);
        Assert.AreEqual("DE", result.Language);
        var statusMessage = result.StatusMessage;
        Assert.AreEqual("GEP file identified (model 2020, DE), 0 checker CSV(s) found.", statusMessage["en"]);
    }

    [TestMethod]
    public async Task RunAsync_IdentifiesGep20201()
    {
        var gepFile = fileFactory.CreateXtf24("gep.xtf", "VSADSSMINI_2020_1_LV95");
        IPipelineFile[] uploads = [gepFile];
        var process = CreateProcess();

        var result = await process.RunAsync(uploads, [], CancellationToken.None);

        var gepFiles = result.Gep;
        Assert.AreSame(gepFile, gepFiles.Single());
        Assert.AreEqual("2020.1", result.ModelVersion);
        Assert.AreEqual("DE", result.Language);
    }

    [TestMethod]
    public async Task RunAsync_Prefers20201_OverMultipleModels()
    {
        var gepFile = fileFactory.CreateXtf24("gep.xtf", "VSADSSMINI_2020_LV95", "VSADSSMINI_2020_1_LV95");
        IPipelineFile[] uploads = [gepFile];
        var process = CreateProcess();

        var result = await process.RunAsync(uploads, [], CancellationToken.None);

        var gepFiles = result.Gep;
        Assert.AreSame(gepFile, gepFiles.Single());
        Assert.AreEqual("2020.1", result.ModelVersion);
        Assert.AreEqual("DE", result.Language);
    }

    [TestMethod]
    public async Task RunAsync_IdentifiesFrenchGep2020()
    {
        var gepFile = fileFactory.CreateXtf24("gep.xtf", "VSASDEEMINI_2020_LV95");
        IPipelineFile[] uploads = [gepFile];
        var process = CreateProcess();

        var result = await process.RunAsync(uploads, [], CancellationToken.None);

        var gepFiles = result.Gep;
        Assert.AreSame(gepFile, gepFiles.Single());
        Assert.AreEqual("2020", result.ModelVersion);
        Assert.AreEqual("FR", result.Language);
    }

    [TestMethod]
    public async Task RunAsync_IdentifiesFrenchGep20201()
    {
        var gepFile = fileFactory.CreateXtf24("gep.xtf", "VSASDEEMINI_2020_1_LV95");
        IPipelineFile[] uploads = [gepFile];
        var process = CreateProcess();

        var result = await process.RunAsync(uploads, [], CancellationToken.None);

        var gepFiles = result.Gep;
        Assert.AreSame(gepFile, gepFiles.Single());
        Assert.AreEqual("2020.1", result.ModelVersion);
        Assert.AreEqual("FR", result.Language);
    }

    [TestMethod]
    public async Task RunAsync_IdentifiesGepFromInterlis23()
    {
        var gepFile = fileFactory.CreateXtf23("gep.xtf", "VSADSSMINI_2020_LV95");
        IPipelineFile[] uploads = [gepFile];
        var process = CreateProcess();

        var result = await process.RunAsync(uploads, [], CancellationToken.None);

        var gepFiles = result.Gep;
        Assert.AreSame(gepFile, gepFiles.Single());
        Assert.AreEqual("2020", result.ModelVersion);
        Assert.AreEqual("DE", result.Language);
    }

    [TestMethod]
    public async Task RunAsync_NoGepFound_ReturnsEmptyArray()
    {
        var otherFile = fileFactory.CreateXtf24("other.xtf", "UnknownModel");
        IPipelineFile[] uploads = [otherFile];
        var process = CreateProcess();

        var result = await process.RunAsync(uploads, [], CancellationToken.None);

        var gepFiles = result.Gep;
        Assert.IsEmpty(gepFiles);
        Assert.IsNull(result.ModelVersion);
        Assert.IsNull(result.Language);
        Assert.IsNull(result.GpkgTemplate);
        Assert.IsNull(result.StandardOrgTable);
        var statusMessage = result.StatusMessage;
        Assert.AreEqual("Keine GEP-Transferdatei in den hochgeladenen Dateien gefunden.", statusMessage["de"]);
        Assert.AreEqual("Aucun fichier de transfert GEP trouvé dans les fichiers téléchargés.", statusMessage["fr"]);
        Assert.AreEqual("Nessun file di trasferimento GEP trovato nei file caricati.", statusMessage["it"]);
        Assert.AreEqual("No GEP transfer file found in uploads.", statusMessage["en"]);
    }

    [TestMethod]
    public async Task RunAsync_MultipleGepFiles_ReturnsAllWithNullMetadata()
    {
        var gepFile1 = fileFactory.CreateXtf24("gep1.xtf", "VSADSSMINI_2020_LV95");
        var gepFile2 = fileFactory.CreateXtf24("gep2.xtf", "VSADSSMINI_2020_1_LV95");
        IPipelineFile[] uploads = [gepFile1, gepFile2];
        var process = CreateProcess();

        var result = await process.RunAsync(uploads, [], CancellationToken.None);

        var gepFiles = result.Gep;
        Assert.HasCount(2, gepFiles);
        Assert.IsNull(result.ModelVersion);
        Assert.IsNull(result.Language);
        var statusMessage = result.StatusMessage;
        Assert.AreEqual("2 GEP files found (2020 DE, 2020.1 DE), unambiguous assignment not possible.", statusMessage["en"]);
    }

    [TestMethod]
    public async Task RunAsync_FindsUserOrgTable()
    {
        var gepFile = fileFactory.CreateXtf24("gep.xtf", "VSADSSMINI_2020_LV95");
        var orgFile = fileFactory.CreateXtf24("org.xtf", "SIA405_Base_Abwasser_LV95");
        IPipelineFile[] uploads = [gepFile, orgFile];
        var process = CreateProcess();

        var result = await process.RunAsync(uploads, [], CancellationToken.None);

        var orgTables = result.UserOrgTable;
        Assert.AreSame(orgFile, orgTables.Single());
    }

    [TestMethod]
    public async Task RunAsync_FindsFrenchUserOrgTable()
    {
        var gepFile = fileFactory.CreateXtf24("gep.xtf", "VSADSSMINI_2020_LV95");
        var orgFile = fileFactory.CreateXtf24("org.xtf", "SIA405_Base_Eaux_usees_LV95");
        IPipelineFile[] uploads = [gepFile, orgFile];
        var process = CreateProcess();

        var result = await process.RunAsync(uploads, [], CancellationToken.None);

        var orgTables = result.UserOrgTable;
        Assert.AreSame(orgFile, orgTables.Single());
    }

    [TestMethod]
    public async Task RunAsync_NoOrgTable_ReturnsEmptyArray()
    {
        var gepFile = fileFactory.CreateXtf24("gep.xtf", "VSADSSMINI_2020_LV95");
        IPipelineFile[] uploads = [gepFile];
        var process = CreateProcess();

        var result = await process.RunAsync(uploads, [], CancellationToken.None);

        var orgTables = result.UserOrgTable;
        Assert.IsEmpty(orgTables);
    }

    [TestMethod]
    public async Task RunAsync_FindsCheckerCsvs()
    {
        var gepFile = fileFactory.CreateXtf24("gep.xtf", "VSADSSMINI_2020_LV95");
        var csvA = fileFactory.CreateCsv("gep_a_err.csv", "check");
        var csvFp = fileFactory.CreateCsv("gep_fp_err.csv", "check");
        var csvT = fileFactory.CreateCsv("gep_t_err.csv", "check");
        var logFile = fileFactory.CreateFile("a.log", "log content", "check");
        IPipelineFile[] uploads = [gepFile];
        IPipelineFile[] unzipped = [csvA, csvFp, csvT, logFile];
        var process = CreateProcess();

        var result = await process.RunAsync(uploads, unzipped, CancellationToken.None);

        var csvsA = result.CheckerCsvA;
        var csvsFp = result.CheckerCsvFp;
        var csvsT = result.CheckerCsvT;
        Assert.AreSame(csvA, csvsA.Single());
        Assert.AreSame(csvFp, csvsFp.Single());
        Assert.AreSame(csvT, csvsT.Single());
        var statusMessage = result.StatusMessage;
        Assert.AreEqual("GEP-Datei erkannt (Modell 2020, DE), 3 Checker-CSV(s) gefunden.", statusMessage["de"]);
        Assert.AreEqual("Fichier GEP identifié (modèle 2020, DE), 3 CSV de vérification trouvé(s).", statusMessage["fr"]);
        Assert.AreEqual("File GEP identificato (modello 2020, DE), 3 CSV di verifica trovati.", statusMessage["it"]);
        Assert.AreEqual("GEP file identified (model 2020, DE), 3 checker CSV(s) found.", statusMessage["en"]);
    }

    [TestMethod]
    public async Task RunAsync_CheckerCsvsInWrongDirectory_ReturnsEmptyArrays()
    {
        var gepFile = fileFactory.CreateXtf24("gep.xtf", "VSADSSMINI_2020_LV95");
        var csvA = fileFactory.CreateCsv("gep_a_err.csv", "other");
        var csvFp = fileFactory.CreateCsv("gep_fp_err.csv");
        IPipelineFile[] uploads = [gepFile];
        IPipelineFile[] unzipped = [csvA, csvFp];
        var process = CreateProcess();

        var result = await process.RunAsync(uploads, unzipped, CancellationToken.None);

        var csvsA = result.CheckerCsvA;
        var csvsFp = result.CheckerCsvFp;
        Assert.IsEmpty(csvsA);
        Assert.IsEmpty(csvsFp);
    }

    [TestMethod]
    public async Task RunAsync_MissingCheckerCsv_ReturnsEmptyArray()
    {
        var gepFile = fileFactory.CreateXtf24("gep.xtf", "VSADSSMINI_2020_LV95");
        var csvA = fileFactory.CreateCsv("gep_a_err.csv", "check");
        IPipelineFile[] uploads = [gepFile];
        IPipelineFile[] unzipped = [csvA];
        var process = CreateProcess();

        var result = await process.RunAsync(uploads, unzipped, CancellationToken.None);

        var csvsA = result.CheckerCsvA;
        var csvsFp = result.CheckerCsvFp;
        var csvsT = result.CheckerCsvT;
        Assert.AreSame(csvA, csvsA.Single());
        Assert.IsEmpty(csvsFp);
        Assert.IsEmpty(csvsT);
    }

    [TestMethod]
    public async Task RunAsync_DuplicateCheckerCsv_ReturnsAll()
    {
        var gepFile = fileFactory.CreateXtf24("gep.xtf", "VSADSSMINI_2020_LV95");
        var csvA1 = fileFactory.CreateCsv("gep_a_err.csv", "check");
        var csvA2 = fileFactory.CreateCsv("other_a_err.csv", "check");
        IPipelineFile[] uploads = [gepFile];
        IPipelineFile[] unzipped = [csvA1, csvA2];
        var process = CreateProcess();

        var result = await process.RunAsync(uploads, unzipped, CancellationToken.None);

        var csvsA = result.CheckerCsvA;
        Assert.HasCount(2, csvsA);
    }

    [TestMethod]
    public async Task RunAsync_CopiesGpkgTemplateForVersion2020()
    {
        var gepFile = fileFactory.CreateXtf24("gep.xtf", "VSADSSMINI_2020_LV95");
        IPipelineFile[] uploads = [gepFile];
        var process = CreateProcess();

        var result = await process.RunAsync(uploads, [], CancellationToken.None);

        var gpkgTemplate = result.GpkgTemplate;
        Assert.IsNotNull(gpkgTemplate);
        Assert.AreEqual("gpkg", gpkgTemplate.FileExtension);
    }

    [TestMethod]
    public async Task RunAsync_FetchesStandardOrgTable()
    {
        var gepFile = fileFactory.CreateXtf24("gep.xtf", "VSADSSMINI_2020_LV95");
        IPipelineFile[] uploads = [gepFile];
        var process = CreateProcess();

        var result = await process.RunAsync(uploads, [], CancellationToken.None);

        var orgTable = result.StandardOrgTable;
        Assert.IsNotNull(orgTable);
        Assert.AreEqual("xtf", orgTable.FileExtension);
    }

    [TestMethod]
    public async Task RunAsync_IliModelMatchingIsCaseInsensitive()
    {
        var gepFile = fileFactory.CreateXtf24("gep.xtf", "vsadssmini_2020_lv95");
        IPipelineFile[] uploads = [gepFile];
        var process = CreateProcess();

        var result = await process.RunAsync(uploads, [], CancellationToken.None);

        var gepFiles = result.Gep;
        Assert.AreSame(gepFile, gepFiles.Single());
        Assert.AreEqual("2020", result.ModelVersion);
        Assert.AreEqual("DE", result.Language);
    }

    private VsaMatcherProcess CreateProcess()
    {
        var templatePath2020 = fileFactory.CreateResourceFile("template_2020.gpkg");
        var templatePath20201 = fileFactory.CreateResourceFile("template_2020_1.gpkg");

        var process = new VsaMatcherProcess(
            NullLogger.Instance,
            fileManager,
            templatePath2020,
            templatePath20201,
            OrgTableUrl2020,
            OrgTableUrl20201);

        typeof(VsaMatcherProcess)
            .GetField("httpClient", BindingFlags.NonPublic | BindingFlags.Instance)!
            .SetValue(process, httpClient);

        return process;
    }
}
