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
        { "uc", "WK" },
        { "gsp", "GEP" },
        { "error", "Fehler" },
        { "check_type", "Prueftyp" },
        { "function_hierarchic", "Funktionhierarchisch" },
        { "owner", "Eigentuemer" },
        { "status", "Status" },
        { "fid", "FID" },
    };

    private static readonly Dictionary<string, string> DataColumnMapping = new()
    {
        { "tid", "A" },
        { "class", "B" },
        { "errorid", "C" },
        { "uc", "D" },
        { "gsp", "E" },
        { "error", "F" },
        { "check_type", "G" },
        { "function_hierarchic", "H" },
        { "owner", "I" },
        { "status", "J" },
        { "fid", "K" },
    };

    private static readonly Dictionary<string, string> ObjectAttributeMapping = new()
    {
        { "tid", "TID" },
        { "class", "Klasse" },
        { "count_error", "Anzahl Fehler" },
        { "uc_max", "WK Maximum" },
        { "gsp_max", "GEP Maximum" },
    };

    private static readonly Dictionary<string, string> ObjectColumnMapping = new()
    {
        { "tid", "A" },
        { "class", "B" },
        { "count_error", "C" },
        { "uc_max", "D" },
        { "gsp_max", "E" },
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

        using var workbook = await OpenOutputWorkbookAsync(result);
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

        using var workbook = await OpenOutputWorkbookAsync(result);

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

        using var workbook = await OpenOutputWorkbookAsync(result);

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

        using var workbook = await OpenOutputWorkbookAsync(result);
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

        using var workbook = await OpenOutputWorkbookAsync(result);

        var dataSheet = workbook.Worksheet("Error Data");
        Assert.AreEqual("TID", dataSheet.Cell("A1").GetString());
        Assert.IsTrue(dataSheet.Cell("A2").IsEmpty());

        var objectSheet = workbook.Worksheet("Error Object");
        Assert.AreEqual("TID", objectSheet.Cell("A1").GetString());
        Assert.IsTrue(objectSheet.Cell("A2").IsEmpty());
    }

    [TestMethod]
    public async Task RunAsync_ColumnMappingWithGaps_WritesEachAttributeIntoItsColumn()
    {
        var geopackage = CreateTestGeoPackage(SeedStandardData);
        var process = CreateProcess(
            dataAttributeMapping: new Dictionary<string, string> { { "tid", "TID" }, { "class", "Klasse" } },
            dataColumnMapping: new Dictionary<string, string> { { "tid", "D" }, { "class", "B" } });

        var result = await process.RunAsync(geopackage, CreateCantonTemplate());

        using var workbook = await OpenOutputWorkbookAsync(result);
        var dataSheet = workbook.Worksheet("Error Data");
        Assert.AreEqual("Klasse", dataSheet.Cell("B1").GetString());
        Assert.AreEqual("TID", dataSheet.Cell("D1").GetString());
        Assert.AreEqual("Leitung", dataSheet.Cell("B2").GetString());
        Assert.AreEqual("LT001", dataSheet.Cell("D2").GetString());
        Assert.IsTrue(dataSheet.Cell("A1").IsEmpty() && dataSheet.Cell("C1").IsEmpty(), "Unmapped columns should stay empty.");
        Assert.AreEqual("B1:D3", dataSheet.AutoFilter.Range.RangeAddress.ToString());
    }

    [TestMethod]
    public async Task RunAsync_WithPivotConfig_CreatesOverviewSheets()
    {
        var geopackage = CreateTestGeoPackage(SeedStandardData);
        var process = CreateProcess(overviewWkSheet: "Uebersicht_WK", overviewGepSheet: "Uebersicht_GEP");

        var result = await process.RunAsync(geopackage, CreateCantonTemplate());

        using var workbook = await OpenOutputWorkbookAsync(result);
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

        using var workbook = await OpenOutputWorkbookAsync(result);
        Assert.AreEqual(3, workbook.Worksheets.Count);
        Assert.IsTrue(workbook.Worksheet("Uebersicht_WK").PivotTables.Contains("Uebersicht_WK"));
    }

    [TestMethod]
    public async Task RunAsync_WithPivotConfig_CountsErrorsPerPriorityClassAndErrorWithoutSubtotals()
    {
        var geopackage = CreateTestGeoPackage(SeedStandardData);
        var process = CreateProcess(overviewWkSheet: "Uebersicht_WK", overviewGepSheet: "Uebersicht_GEP");

        var result = await process.RunAsync(geopackage, CreateCantonTemplate());

        using var workbook = await OpenOutputWorkbookAsync(result);
        AssertOverviewPivot(workbook, "Uebersicht_WK", "WK");
        AssertOverviewPivot(workbook, "Uebersicht_GEP", "GEP");
    }

    [TestMethod]
    public async Task RunAsync_WithPivotConfigAndObjects_PlacesOverviewsBetweenErrorAndStatisticsSheets()
    {
        var geopackage = CreateTestGeoPackage(SeedStandardData, ObjectStatisticsGeoPackage.Create(inputDirectory));
        var process = CreateProcess(overviewWkSheet: "Uebersicht_WK", overviewGepSheet: "Uebersicht_GEP");

        var result = await process.RunAsync(geopackage, CreateCantonTemplate());

        using var workbook = await OpenOutputWorkbookAsync(result);
        Assert.AreEqual(
            "Error Data, Error Object, Uebersicht_WK, Uebersicht_GEP, drainage_system_current, catchment_area_data, function, node_data, function_hierarchic, usage_current, reach_data",
            string.Join(", ", workbook.Worksheets.Select(worksheet => worksheet.Name)));
    }

    [TestMethod]
    public async Task RunAsync_GeoPackageWithObjects_AppendsObjectStatisticsSheets()
    {
        var geopackage = CreateTestGeoPackage(SeedStandardData, ObjectStatisticsGeoPackage.Create(inputDirectory));
        var process = CreateProcess();

        var result = await process.RunAsync(geopackage, CreateCantonTemplate());

        using var workbook = await OpenOutputWorkbookAsync(result);
        Assert.AreEqual(string.Join(", ", ErrorAndObjectStatisticsSheets), string.Join(", ", workbook.Worksheets.Select(worksheet => worksheet.Name)));
    }

    [TestMethod]
    public async Task RunAsync_GeoPackageWithObjects_AddsCountAndSharePivotsWithTheRowFieldsOfTheTemplates()
    {
        var geopackage = CreateTestGeoPackage(SeedStandardData, ObjectStatisticsGeoPackage.Create(inputDirectory));
        var process = CreateProcess();

        var result = await process.RunAsync(geopackage, CreateCantonTemplate());

        using var workbook = await OpenOutputWorkbookAsync(result);
        AssertCountAndSharePivot(workbook, "drainage_system_current", "drainage_system_current");
        AssertCountAndSharePivot(workbook, "function", "function_hierarchic", "function");
        AssertCountAndSharePivot(workbook, "function_hierarchic", "function_hierarchic");
        AssertCountAndSharePivot(workbook, "usage_current", "function_hierarchic_group", "usage_current");
    }

    [TestMethod]
    public async Task RunAsync_GeoPackageWithObjects_WritesStatisticsRowsWithTheirTypes()
    {
        var geopackage = CreateTestGeoPackage(SeedStandardData, ObjectStatisticsGeoPackage.Create(inputDirectory));
        var process = CreateProcess();

        var result = await process.RunAsync(geopackage, CreateCantonTemplate());

        using var workbook = await OpenOutputWorkbookAsync(result);
        var reaches = workbook.Worksheet("reach_data");
        Assert.AreEqual("LT1", reaches.Cell("A2").GetString());
        Assert.AreEqual(XLDataType.Number, reaches.Cell("B2").DataType, "year_of_construction should stay numeric.");
        Assert.AreEqual(1989, reaches.Cell("B2").GetValue<int>());
        Assert.IsTrue(reaches.Cell("D2").IsEmpty(), "A NULL remark should stay an empty cell.");
        Assert.AreEqual("PAA", reaches.Cell("H2").GetString());
        Assert.AreEqual("LINESTRING (2600100 1200100, 2600200 1200200)", reaches.Cell("AW2").GetString());
    }

    [TestMethod]
    public async Task RunAsync_GeoPackageWithoutObjects_KeepsHeaderOnlyStatisticsSheetsAndTheirPivots()
    {
        var geopackage = CreateTestGeoPackage(SeedStandardData, ObjectStatisticsGeoPackage.Create(inputDirectory, withObjects: false));
        var process = CreateProcess();

        var result = await process.RunAsync(geopackage, CreateCantonTemplate());

        using var workbook = await OpenOutputWorkbookAsync(result);
        Assert.AreEqual("tid", workbook.Worksheet("reach_data").Cell("A1").GetString());
        Assert.IsTrue(workbook.Worksheet("reach_data").Cell("A2").IsEmpty());
        AssertCountAndSharePivot(workbook, "usage_current", "function_hierarchic_group", "usage_current");
    }

    [TestMethod]
    public async Task RunAsync_GeoPackageWithObjects_FiltersEachDataSheetBelowAFrozenHeaderRow()
    {
        var geopackage = CreateTestGeoPackage(SeedStandardData, ObjectStatisticsGeoPackage.Create(inputDirectory));
        var process = CreateProcess();

        var result = await process.RunAsync(geopackage, CreateCantonTemplate());

        using var workbook = await OpenOutputWorkbookAsync(result);
        AssertFilteredBelowFrozenHeader(workbook, "Error Data", "A1:K3");
        AssertFilteredBelowFrozenHeader(workbook, "Error Object", "A1:E2");
        AssertFilteredBelowFrozenHeader(workbook, "reach_data", "A1:AW4");
    }

    [TestMethod]
    public async Task RunAsync_FillsCantonRawDataFromStatistics()
    {
        var geopackage = CreateTestGeoPackage(SeedStatistics);
        var process = CreateProcess();

        var result = await process.RunAsync(geopackage, CreateCantonTemplate());

        var cantonFile = result.CantonErrorMatrix;

        using var fileStream = await cantonFile.OpenReadAsync();
        var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream);
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

    private static readonly string[] ErrorAndObjectStatisticsSheets =
        ["Error Data", "Error Object", "drainage_system_current", "catchment_area_data", "function", "node_data", "function_hierarchic", "usage_current", "reach_data"];

    private static readonly List<string> OverviewRowFields = ["class", "error"];

    private static readonly List<string> OverviewFilterFields = ["check_type", "function_hierarchic", "owner", "status"];

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

    private TestPipelineFile CreateTestGeoPackage(Action<SqliteConnection>? seedAction = null, string? gpkgPath = null)
    {
        gpkgPath ??= Path.Combine(inputDirectory, $"test-{Guid.NewGuid():N}.gpkg");

        using var connection = new SqliteConnection($"Data Source={gpkgPath};Pooling=false");
        connection.Open();

        var createSchema = """
            CREATE TABLE ca_error_data (
                fid INTEGER NOT NULL PRIMARY KEY,
                tid TEXT, check_type TEXT, topic TEXT, class TEXT,
                errorid TEXT, error TEXT, detail TEXT,
                function_hierarchic TEXT, owner TEXT, status TEXT,
                category TEXT, model TEXT, module TEXT,
                uc INTEGER, gsp INTEGER,
                required_action TEXT, action_context TEXT);

            CREATE TABLE ca_error_object (
                fid INTEGER NOT NULL PRIMARY KEY,
                tid TEXT, class TEXT,
                count_error INTEGER, uc_max INTEGER, gsp_max INTEGER)
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
            INSERT INTO ca_error_data (tid, class, errorid, uc, gsp, error)
            VALUES ('LT001', 'Leitung', 't_001', 1, 2, 'Fehler DE 1'),
                   ('LT001', 'Leitung', 'a_001', 2, 1, 'Fehler DE 2');

            INSERT INTO ca_error_object (tid, class, count_error, uc_max, gsp_max)
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
            INSERT INTO ca_error_data (tid, class, errorid, uc, gsp, error)
            VALUES ('UNK001', 'Massnahme', 't_999', NULL, NULL, NULL)
            """;
        ExecuteNonQuery(connection, sql);
    }

    private static async Task<XLWorkbook> OpenOutputWorkbookAsync(ErrorOverviewExportResult result)
    {
        var outputFile = result.ErrorOverview;

        using var fileStream = await outputFile.OpenReadAsync();
        var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;
        return new XLWorkbook(memoryStream);
    }

    private static void AssertOverviewPivot(XLWorkbook workbook, string sheetName, string priorityField)
    {
        var worksheet = workbook.Worksheet(sheetName);
        var pivotTable = worksheet.PivotTables.Single();

        // The fields are the error data headers of the configured attribute keys, the priority field first.
        Assert.AreEqual($"{priorityField}, Klasse, Fehler", string.Join(", ", pivotTable.RowLabels.Select(field => field.SourceName)), $"Row fields of '{sheetName}'.");
        Assert.IsFalse(pivotTable.RowLabels.Any(field => field.Subtotals.Count > 0), $"Row fields of '{sheetName}' should show no subtotals.");
        Assert.AreEqual("Prueftyp, Funktionhierarchisch, Eigentuemer, Status", string.Join(", ", pivotTable.ReportFilters.Select(field => field.SourceName)), $"Filters of '{sheetName}'.");
        CollectionAssert.AreEqual(
            new[] { ("Anzahl Fehler", "FID", XLPivotSummary.Count, XLPivotCalculation.Normal) },
            pivotTable.Values.Select(value => (value.CustomName, value.SourceName, value.SummaryFormula, value.Calculation)).ToArray(),
            $"Values of '{sheetName}'.");
        Assert.AreEqual("A1", pivotTable.TargetCell.Address.ToString(), $"Location of '{sheetName}'.");
        Assert.AreEqual(105, worksheet.Column("A").Width, $"Width of column A of '{sheetName}'.");
        Assert.AreEqual(13, worksheet.Column("B").Width, $"Width of column B of '{sheetName}'.");
    }

    private static void AssertFilteredBelowFrozenHeader(XLWorkbook workbook, string sheetName, string range)
    {
        var worksheet = workbook.Worksheet(sheetName);
        Assert.AreEqual(range, worksheet.AutoFilter.Range.RangeAddress.ToString(), $"Autofilter of '{sheetName}'.");
        Assert.AreEqual(1, worksheet.SheetView.SplitRow, $"Frozen rows of '{sheetName}'.");
    }

    private static void AssertCountAndSharePivot(XLWorkbook workbook, string sheetName, params string[] rowFields)
    {
        var pivotTable = workbook.Worksheet(sheetName).PivotTables.Single();

        Assert.AreEqual(string.Join(", ", rowFields), string.Join(", ", pivotTable.RowLabels.Select(field => field.SourceName)), $"Row fields of '{sheetName}'.");
        Assert.AreEqual("A3", pivotTable.TargetCell.Address.ToString(), $"Location of '{sheetName}'.");

        // As in the templates, a two-level pivot totals each group (e.g. PAA, SAA) in its group row.
        Assert.IsTrue(
            pivotTable.RowLabels.All(field => field.Subtotals.Contains(XLSubtotalFunction.Automatic)),
            $"Row fields of '{sheetName}' should show Excel's automatic subtotals.");
        CollectionAssert.AreEqual(
            new[]
            {
                ("Anzahl von tid", "tid", XLPivotSummary.Count, XLPivotCalculation.Normal),
                ("Anteil", "tid", XLPivotSummary.Count, XLPivotCalculation.PercentageOfTotal),
            },
            pivotTable.Values.Select(value => (value.CustomName, value.SourceName, value.SummaryFormula, value.Calculation)).ToArray(),
            $"Values of '{sheetName}'.");
    }

    [SuppressMessage("Security", "CA2100", Justification = "Test queries use hardcoded SQL, not user input.")]
    private static void ExecuteNonQuery(SqliteConnection connection, string sql)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }
}
