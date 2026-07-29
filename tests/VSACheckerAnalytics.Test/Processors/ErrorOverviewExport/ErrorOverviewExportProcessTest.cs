using ClosedXML.Excel;
using Geopilot.PipelineCore.Pipeline;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using System.Diagnostics.CodeAnalysis;
using VsaCheckerAnalytics.TestHelpers;

namespace VsaCheckerAnalytics.Processors.ErrorOverviewExport;

[TestClass]
public sealed class ErrorOverviewExportProcessTest
{
    private static readonly Dictionary<string, string> DataAttributeMapping = new()
    {
        { "tid", "TID" },
        { "class", "Klasse" },
        { "errorid", "Fehler-ID" },
        { "wk", "WK" },
        { "gep", "GEP" },
        { "error", "Fehler" },
        { "check_type", "Prueftyp" },
        { "funktionhierarchisch", "Funktionhierarchisch" },
        { "eigentuemer", "Eigentuemer" },
        { "status", "Status" },
        { "fid", "FID" },
    };

    private static readonly Dictionary<string, string> DataColumnMapping = new()
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
    };

    private static readonly Dictionary<string, string> ObjectAttributeMapping = new()
    {
        { "tid", "TID" },
        { "class", "Klasse" },
        { "count_error", "Anzahl Fehler" },
        { "wk_max", "WK Maximum" },
        { "gep_max", "GEP Maximum" },
    };

    private static readonly Dictionary<string, string> ObjectColumnMapping = new()
    {
        { "tid", "A" },
        { "class", "B" },
        { "count_error", "C" },
        { "wk_max", "D" },
        { "gep_max", "E" },
    };

    private static readonly Dictionary<string, string> CantonErrorColumnMapping = new()
    {
        { "tabelle", "A" },
        { "attribut", "B" },
        { "anzahl_total", "C" },
        { "anzahl_paa", "D" },
        { "anzahl_saa", "E" },
        { "anzahl_null", "F" },
        { "anzahl_null_paa", "G" },
        { "anzahl_null_saa", "H" },
    };

    private string inputDirectory = null!;
    private TestPipelineFileManager fileManager = null!;

    [TestInitialize]
    public void Setup()
    {
        inputDirectory = Path.Combine(Path.GetTempPath(), "vsa-test-input-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(inputDirectory);
        fileManager = new TestPipelineFileManager();
    }

    [TestCleanup]
    public void Cleanup()
    {
        fileManager.Dispose();
        if (Directory.Exists(inputDirectory))
            Directory.Delete(inputDirectory, recursive: true);
    }

    [TestMethod]
    public void Constructor_MismatchedMappingKeys_ThrowsArgumentException()
    {
        var attributeMapping = new Dictionary<string, string> { { "a", "Header A" }, { "b", "Header B" } };
        var columnMapping = new Dictionary<string, string> { { "a", "A" }, { "c", "C" } };

        var ex = Assert.ThrowsExactly<ArgumentException>(() => CreateProcess(
            dataAttributeMapping: attributeMapping,
            dataColumnMapping: columnMapping));

        StringAssert.Contains(ex.Message, "b");
        StringAssert.Contains(ex.Message, "c");
    }

    [TestMethod]
    public async Task RunAsync_PopulatedGeoPackage_ExportsBothSheetsWithHeaders()
    {
        var geopackage = CreateTestGeoPackage(SeedStandardData);
        var process = CreateProcess();

        var result = await process.RunAsync(geopackage, CreateCantonTemplate());

        var statusMessage = result.StatusMessage;
        Assert.AreEqual("Error overview created: 2 errors exported.", statusMessage["en"]);

        using var workbook = OpenOutputWorkbook(result);
        Assert.AreEqual(2, workbook.Worksheets.Count);

        var dataSheet = workbook.Worksheet("Error Data");
        Assert.AreEqual("TID", dataSheet.Cell("A1").GetString());
        Assert.AreEqual("Klasse", dataSheet.Cell("B1").GetString());
        Assert.AreEqual("Fehler-ID", dataSheet.Cell("C1").GetString());
        Assert.AreEqual("WK", dataSheet.Cell("D1").GetString());
        Assert.AreEqual("GEP", dataSheet.Cell("E1").GetString());
        Assert.AreEqual("Fehler", dataSheet.Cell("F1").GetString());

        var objectSheet = workbook.Worksheet("Error Object");
        Assert.AreEqual("TID", objectSheet.Cell("A1").GetString());
        Assert.AreEqual("Klasse", objectSheet.Cell("B1").GetString());
        Assert.AreEqual("Anzahl Fehler", objectSheet.Cell("C1").GetString());
        Assert.AreEqual("WK Maximum", objectSheet.Cell("D1").GetString());
        Assert.AreEqual("GEP Maximum", objectSheet.Cell("E1").GetString());
    }

    [TestMethod]
    public async Task RunAsync_PopulatedGeoPackage_ExportsCorrectDataRows()
    {
        var geopackage = CreateTestGeoPackage(SeedStandardData);
        var process = CreateProcess();

        var result = await process.RunAsync(geopackage, CreateCantonTemplate());

        using var workbook = OpenOutputWorkbook(result);

        var dataSheet = workbook.Worksheet("Error Data");
        Assert.AreEqual("LT001", dataSheet.Cell("A2").GetString());
        Assert.AreEqual("Leitung", dataSheet.Cell("B2").GetString());
        Assert.AreEqual("t_001", dataSheet.Cell("C2").GetString());
        Assert.AreEqual("Fehler DE 1", dataSheet.Cell("F2").GetString());
        Assert.AreEqual("LT001", dataSheet.Cell("A3").GetString());
        Assert.AreEqual("a_001", dataSheet.Cell("C3").GetString());

        var objectSheet = workbook.Worksheet("Error Object");
        Assert.AreEqual("LT001", objectSheet.Cell("A2").GetString());
        Assert.AreEqual("Leitung", objectSheet.Cell("B2").GetString());
    }

    [TestMethod]
    public async Task RunAsync_IntegerColumns_PreservesNumericType()
    {
        var geopackage = CreateTestGeoPackage(SeedStandardData);
        var process = CreateProcess();

        var result = await process.RunAsync(geopackage, CreateCantonTemplate());

        using var workbook = OpenOutputWorkbook(result);

        var dataSheet = workbook.Worksheet("Error Data");
        Assert.AreEqual(XLDataType.Number, dataSheet.Cell("D2").DataType);
        Assert.AreEqual(1, dataSheet.Cell("D2").GetValue<int>());
        Assert.AreEqual(2, dataSheet.Cell("E2").GetValue<int>());

        var objectSheet = workbook.Worksheet("Error Object");
        Assert.AreEqual(XLDataType.Number, objectSheet.Cell("C2").DataType);
        Assert.AreEqual(2, objectSheet.Cell("C2").GetValue<int>());
        Assert.AreEqual(2, objectSheet.Cell("D2").GetValue<int>());
        Assert.AreEqual(2, objectSheet.Cell("E2").GetValue<int>());
    }

    [TestMethod]
    public async Task RunAsync_NullValues_ProducesBlankCells()
    {
        var geopackage = CreateTestGeoPackage(connection =>
        {
            SeedStandardData(connection);
            SeedNullRow(connection);
        });
        var process = CreateProcess();

        var result = await process.RunAsync(geopackage, CreateCantonTemplate());

        using var workbook = OpenOutputWorkbook(result);
        var dataSheet = workbook.Worksheet("Error Data");
        var lastRow = dataSheet.LastRowUsed()!.RowNumber();
        Assert.AreEqual("UNK001", dataSheet.Cell($"A{lastRow}").GetString());
        Assert.IsTrue(dataSheet.Cell($"D{lastRow}").IsEmpty());
        Assert.IsTrue(dataSheet.Cell($"E{lastRow}").IsEmpty());
        Assert.IsTrue(dataSheet.Cell($"F{lastRow}").IsEmpty());
    }

    [TestMethod]
    public async Task RunAsync_EmptyTables_ProducesHeadersOnly()
    {
        var geopackage = CreateTestGeoPackage();
        var process = CreateProcess();

        var result = await process.RunAsync(geopackage, CreateCantonTemplate());

        using var workbook = OpenOutputWorkbook(result);

        var dataSheet = workbook.Worksheet("Error Data");
        Assert.AreEqual("TID", dataSheet.Cell("A1").GetString());
        Assert.IsTrue(dataSheet.Cell("A2").IsEmpty());

        var objectSheet = workbook.Worksheet("Error Object");
        Assert.AreEqual("TID", objectSheet.Cell("A1").GetString());
        Assert.IsTrue(objectSheet.Cell("A2").IsEmpty());
    }

    [TestMethod]
    public async Task RunAsync_WithPivotConfig_CreatesOverviewSheets()
    {
        var geopackage = CreateTestGeoPackage(SeedStandardData);
        var process = CreateProcess(overviewWkSheet: "Uebersicht_WK", overviewGepSheet: "Uebersicht_GEP");

        var result = await process.RunAsync(geopackage, CreateCantonTemplate());

        using var workbook = OpenOutputWorkbook(result);
        Assert.AreEqual(4, workbook.Worksheets.Count);
        Assert.IsTrue(workbook.Worksheet("Uebersicht_WK").PivotTables.Contains("Uebersicht_WK"));
        Assert.IsTrue(workbook.Worksheet("Uebersicht_GEP").PivotTables.Contains("Uebersicht_GEP"));
    }

    [TestMethod]
    public async Task RunAsync_WithOnlyWkPivot_CreatesSingleOverviewSheet()
    {
        var geopackage = CreateTestGeoPackage(SeedStandardData);
        var process = CreateProcess(overviewWkSheet: "Uebersicht_WK");

        var result = await process.RunAsync(geopackage, CreateCantonTemplate());

        using var workbook = OpenOutputWorkbook(result);
        Assert.AreEqual(3, workbook.Worksheets.Count);
        Assert.IsTrue(workbook.Worksheet("Uebersicht_WK").PivotTables.Contains("Uebersicht_WK"));
    }

    [TestMethod]
    public async Task RunAsync_FillsCantonRawDataFromStatistics()
    {
        var geopackage = CreateTestGeoPackage(SeedStatistics);
        var process = CreateProcess();

        var result = await process.RunAsync(geopackage, CreateCantonTemplate());

        var cantonFile = result.CantonErrorMatrix;

        using var fileStream = cantonFile.OpenReadFileStream();
        var memoryStream = new MemoryStream();
        fileStream.CopyTo(memoryStream);
        memoryStream.Position = 0;
        using var workbook = new XLWorkbook(memoryStream);
        var raw = workbook.Worksheet("raw_data");

        // No header row: statistics start at row 1, columns per cantonErrorColumnMapping, ordered by
        // sortierung (knoten sortierung=1 before leitung sortierung=2), not by insertion/rowid order.
        Assert.AreEqual("knoten", raw.Cell("A1").GetString());
        Assert.AreEqual("funktion", raw.Cell("B1").GetString());
        Assert.AreEqual(5, raw.Cell("C1").GetValue<int>());
        Assert.AreEqual(1, raw.Cell("F1").GetValue<int>());

        Assert.AreEqual("leitung", raw.Cell("A2").GetString());
        Assert.AreEqual("material", raw.Cell("B2").GetString());
        Assert.AreEqual(3, raw.Cell("C2").GetValue<int>());

        // anzahl_paa (column D) is NULL for leitung/material and must stay an empty cell.
        Assert.IsTrue(raw.Cell("D2").IsEmpty());

        // The workbook must open on the validation sheet, not on the raw data sheet we wrote into.
        Assert.IsTrue(workbook.Worksheet("Validierung_Teststufe_1").TabActive);
        Assert.IsFalse(raw.TabActive);
    }

    [TestMethod]
    public void Constructor_PivotFieldNotInAttributeMapping_ThrowsArgumentException()
    {
        var ex = Assert.ThrowsExactly<ArgumentException>(() => new ErrorOverviewExportProcess(
            errorDataSheet: "Error Data",
            errorDataAttributeMapping: DataAttributeMapping,
            errorDataColumnMapping: DataColumnMapping,
            errorObjectSheet: "Error Object",
            errorObjectAttributeMapping: ObjectAttributeMapping,
            errorObjectColumnMapping: ObjectColumnMapping,
            overviewWkSheet: "WK",
            overviewGepSheet: null,
            overviewRowFields: new List<string> { "nonexistent" },
            overviewFilterFields: OverviewFilterFields,
            overviewValueField: "fid",
            overviewValueName: "Count",
            cantonErrorObjectSheet: "raw_data",
            cantonErrorColumnMapping: CantonErrorColumnMapping,
            pipelineFileManager: fileManager,
            logger: NullLogger.Instance));

        StringAssert.Contains(ex.Message, "nonexistent");
    }

    private static readonly List<string> OverviewRowFields = ["class", "error"];

    private static readonly List<string> OverviewFilterFields = ["check_type", "funktionhierarchisch", "eigentuemer", "status"];

    private ErrorOverviewExportProcess CreateProcess(
        Dictionary<string, string>? dataAttributeMapping = null,
        Dictionary<string, string>? dataColumnMapping = null,
        string? overviewWkSheet = null,
        string? overviewGepSheet = null)
    {
        var hasOverview = overviewWkSheet is not null || overviewGepSheet is not null;
        return new ErrorOverviewExportProcess(
            errorDataSheet: "Error Data",
            errorDataAttributeMapping: dataAttributeMapping ?? DataAttributeMapping,
            errorDataColumnMapping: dataColumnMapping ?? DataColumnMapping,
            errorObjectSheet: "Error Object",
            errorObjectAttributeMapping: ObjectAttributeMapping,
            errorObjectColumnMapping: ObjectColumnMapping,
            overviewWkSheet: overviewWkSheet,
            overviewGepSheet: overviewGepSheet,
            overviewRowFields: hasOverview ? OverviewRowFields : null,
            overviewFilterFields: hasOverview ? OverviewFilterFields : null,
            overviewValueField: hasOverview ? "fid" : null,
            overviewValueName: hasOverview ? "Anzahl Fehler" : null,
            cantonErrorObjectSheet: "raw_data",
            cantonErrorColumnMapping: CantonErrorColumnMapping,
            pipelineFileManager: fileManager,
            logger: NullLogger.Instance);
    }

    private TestPipelineFile CreateTestGeoPackage(Action<SqliteConnection>? seedAction = null)
    {
        var gpkgPath = Path.Combine(inputDirectory, $"test-{Guid.NewGuid():N}.gpkg");

        using var connection = new SqliteConnection($"Data Source={gpkgPath};Pooling=false");
        connection.Open();

        var createSchema = """
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
                count_error INTEGER, wk_max INTEGER, gep_max INTEGER)
            """;
        ExecuteNonQuery(connection, createSchema);

        seedAction?.Invoke(connection);

        return new TestPipelineFile(gpkgPath);
    }

    private TestPipelineFile CreateCantonTemplate()
    {
        var templatePath = Path.Combine(inputDirectory, $"canton-template-{Guid.NewGuid():N}.xlsx");

        using var workbook = new XLWorkbook();
        workbook.AddWorksheet("Validierung_Teststufe_1");
        workbook.AddWorksheet("raw_data");
        workbook.SaveAs(templatePath);

        return new TestPipelineFile(templatePath);
    }

    private static void SeedStandardData(SqliteConnection connection)
    {
        var sql = """
            INSERT INTO ca_error_data (tid, class, errorid, wk, gep, error)
            VALUES ('LT001', 'Leitung', 't_001', 1, 2, 'Fehler DE 1'),
                   ('LT001', 'Leitung', 'a_001', 2, 1, 'Fehler DE 2');

            INSERT INTO ca_error_object (tid, class, count_error, wk_max, gep_max)
            VALUES ('LT001', 'Leitung', 2, 2, 2)
            """;
        ExecuteNonQuery(connection, sql);
    }

    private static void SeedStatistics(SqliteConnection connection)
    {
        // Insert in reverse sortierung order (leitung before knoten) so the rowid/insertion order
        // differs from the intended sortierung order. The canton export must order by sortierung, so
        // the rows land where the template's fixed-cell validation formulas expect them.
        var sql = """
            CREATE TABLE ca_statistics_attribute (
                sortierung INTEGER, tabelle TEXT, attribut TEXT, anzahl_total INTEGER,
                anzahl_paa INTEGER, anzahl_saa INTEGER, anzahl_null INTEGER,
                anzahl_null_paa INTEGER, anzahl_null_saa INTEGER);

            INSERT INTO ca_statistics_attribute
                (sortierung, tabelle, attribut, anzahl_total, anzahl_paa, anzahl_saa, anzahl_null, anzahl_null_paa, anzahl_null_saa)
            VALUES (2, 'leitung', 'material', 3, NULL, NULL, 0, NULL, NULL),
                   (1, 'knoten', 'funktion', 5, 2, 3, 1, 0, 1)
            """;
        ExecuteNonQuery(connection, sql);
    }

    private static void SeedNullRow(SqliteConnection connection)
    {
        var sql = """
            INSERT INTO ca_error_data (tid, class, errorid, wk, gep, error)
            VALUES ('UNK001', 'Massnahme', 't_999', NULL, NULL, NULL)
            """;
        ExecuteNonQuery(connection, sql);
    }

    private static XLWorkbook OpenOutputWorkbook(ErrorOverviewExportResult result)
    {
        var outputFile = result.ErrorOverview;

        using var fileStream = outputFile.OpenReadFileStream();
        var memoryStream = new MemoryStream();
        fileStream.CopyTo(memoryStream);
        memoryStream.Position = 0;
        return new XLWorkbook(memoryStream);
    }

    [SuppressMessage("Security", "CA2100", Justification = "Test queries use hardcoded SQL, not user input.")]
    private static void ExecuteNonQuery(SqliteConnection connection, string sql)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }
}
