using ClosedXML.Excel;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using NetTopologySuite.Geometries;
using System.Diagnostics.CodeAnalysis;
using VsaCheckerAnalytics.TestHelpers;

namespace VsaCheckerAnalytics.Processors.ErrorOverviewExport;

[TestClass]
public sealed class ErrorOverviewExportProcessTest
{
    private static readonly Dictionary<string, ColumnConfig> DataColumns = new()
    {
        ["tid"] = new("A", "TID"),
        ["class"] = new("B", "Klasse"),
        ["errorid"] = new("C", "Fehler-ID"),
        ["uc"] = new("D", "WK"),
        ["gsp"] = new("E", "GEP"),
        ["error"] = new("F", "Fehler"),
        ["check_type"] = new("G", "Prueftyp"),
        ["function_hierarchic"] = new("H", "Funktionhierarchisch"),
        ["owner"] = new("I", "Eigentuemer"),
        ["status"] = new("J", "Status"),
        ["fid"] = new("K", "FID"),
    };

    private static readonly Dictionary<string, ColumnConfig> ObjectColumns = new()
    {
        ["tid"] = new("A", "TID"),
        ["class"] = new("B", "Klasse"),
        ["count_error"] = new("C", "Anzahl Fehler"),
        ["uc_max"] = new("D", "WK Maximum"),
        ["gsp_max"] = new("E", "GEP Maximum"),
    };

    // A few columns per object class are enough here; the full template columns live in the pipeline definition.
    private static readonly Dictionary<string, ColumnConfig> CatchmentAreaColumns = new()
    {
        ["tid"] = new("A", "tid"),
        ["drainage_system_current"] = new("B", "drainage_system_current"),
        ["geom"] = new("C", "geom"),
    };

    private static readonly Dictionary<string, ColumnConfig> NodeColumns = new()
    {
        ["tid"] = new("A", "tid"),
        ["function_hierarchic"] = new("B", "function_hierarchic"),
        ["function"] = new("C", "function"),
        ["geom"] = new("D", "geometry (WKT)"),
        ["geom_detail"] = new("E", "geometry_detail (WKT)"),
    };

    private static readonly Dictionary<string, ColumnConfig> ReachColumns = new()
    {
        ["tid"] = new("A", "tid"),
        ["year_of_construction"] = new("B", "year_of_construction"),
        ["remark"] = new("C", "remark"),
        ["function_hierarchic"] = new("D", "function_hierarchic"),
        ["function_hierarchic_group"] = new("E", "function_hierarchic_group"),
        ["usage_current"] = new("F", "usage_current"),
        ["geom"] = new("G", "geometry (WKT)"),
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

    private static readonly string[] Sheets =
    [
        "Uebersicht WK", "Uebersicht GEP", "Error Data", "Error Object",
        "drainage_system_current", "catchment_area_data", "function", "node_data", "function_hierarchic", "usage_current", "reach_data",
    ];

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
    public void Constructor_ColumnWithoutHeader_ThrowsArgumentException()
    {
        var ex = Assert.ThrowsExactly<ArgumentException>(() => CreateProcess(
            dataColumns: new Dictionary<string, ColumnConfig> { ["tid"] = new("A", "TID"), ["class"] = new("B", " ") }));

        StringAssert.Contains(ex.Message, "class");
    }

    [TestMethod]
    public void Constructor_InvalidColumnLetter_ThrowsArgumentException()
    {
        var ex = Assert.ThrowsExactly<ArgumentException>(() => CreateProcess(
            dataColumns: new Dictionary<string, ColumnConfig> { ["tid"] = new("A", "TID"), ["class"] = new("B2", "Klasse") }));

        StringAssert.Contains(ex.Message, "B2");
    }

    [TestMethod]
    public void Constructor_TwoAttributesInOneColumn_ThrowsArgumentException()
    {
        // Without the check the second attribute would silently replace the first in the sheet.
        var ex = Assert.ThrowsExactly<ArgumentException>(() => CreateProcess(
            dataColumns: new Dictionary<string, ColumnConfig> { ["tid"] = new("A", "TID"), ["class"] = new("A", "Klasse") }));

        StringAssert.Contains(ex.Message, "tid");
        StringAssert.Contains(ex.Message, "class");
    }

    [TestMethod]
    public void Constructor_AttributeKeyThatIsNotAPlainColumnName_ThrowsArgumentException()
    {
        // The attribute keys end up in the sheet's SELECT.
        var ex = Assert.ThrowsExactly<ArgumentException>(() => CreateProcess(
            dataColumns: new Dictionary<string, ColumnConfig> { ["tid\" FROM sqlite_master --"] = new("A", "TID") }));

        StringAssert.Contains(ex.Message, "sqlite_master");
    }

    [TestMethod]
    public void Constructor_ColumnMissingForAPivot_ThrowsArgumentException()
    {
        // The WK overview groups by uc, so the error data columns must contain it.
        var columns = DataColumns.Where(column => column.Key != "uc").ToDictionary();

        var ex = Assert.ThrowsExactly<ArgumentException>(() => CreateProcess(dataColumns: columns));

        StringAssert.Contains(ex.Message, "'uc'");
        StringAssert.Contains(ex.Message, "Uebersicht WK");
    }

    [TestMethod]
    public void Constructor_SheetNameUsedTwice_ThrowsArgumentException()
    {
        // Excel compares sheet names case-insensitively, so this clashes with the WK overview.
        var ex = Assert.ThrowsExactly<ArgumentException>(() => CreateProcess(objectSheet: "uebersicht wk"));

        StringAssert.Contains(ex.Message, "uebersicht wk");
    }

    [TestMethod]
    public async Task RunAsync_PopulatedGeoPackage_ExportsBothErrorSheetsWithHeaders()
    {
        var geopackage = CreateTestGeoPackage(SeedStandardData);
        var process = CreateProcess();

        var result = await process.RunAsync(geopackage, CreateCantonTemplate());

        Assert.AreEqual("Error overview created: 2 errors exported.", result.StatusMessage["en"]);

        using var workbook = await OpenOutputWorkbookAsync(result);
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
        Assert.AreEqual("UNK001", dataSheet.Cell("A4").GetString());
        Assert.IsTrue(dataSheet.Cell("D4").IsEmpty());
        Assert.IsTrue(dataSheet.Cell("E4").IsEmpty());
        Assert.IsTrue(dataSheet.Cell("F4").IsEmpty());
    }

    [TestMethod]
    public async Task RunAsync_EmptyTables_ProducesHeadersOnly()
    {
        var geopackage = CreateTestGeoPackage();
        var process = CreateProcess();

        var result = await process.RunAsync(geopackage, CreateCantonTemplate());

        using var workbook = await OpenOutputWorkbookAsync(result);
        foreach (var sheet in new[] { "Error Data", "Error Object", "catchment_area_data", "node_data", "reach_data" })
        {
            Assert.AreEqual(sheet.StartsWith("Error", StringComparison.Ordinal) ? "TID" : "tid", workbook.Worksheet(sheet).Cell("A1").GetString(), sheet);
            Assert.IsTrue(workbook.Worksheet(sheet).Cell("A2").IsEmpty(), sheet);
        }

        // Pivots over header-only data still open in Excel.
        AssertCountAndSharePivot(workbook, "usage_current", "function_hierarchic_group", "usage_current");
    }

    [TestMethod]
    public async Task RunAsync_ColumnsWithGaps_WritesEachAttributeIntoItsColumn()
    {
        var geopackage = CreateTestGeoPackage(SeedStandardData);
        var process = CreateProcess(
            objectColumns: new Dictionary<string, ColumnConfig> { ["tid"] = new("D", "TID"), ["class"] = new("B", "Klasse") });

        var result = await process.RunAsync(geopackage, CreateCantonTemplate());

        using var workbook = await OpenOutputWorkbookAsync(result);
        var objectSheet = workbook.Worksheet("Error Object");
        Assert.AreEqual("Klasse", objectSheet.Cell("B1").GetString());
        Assert.AreEqual("TID", objectSheet.Cell("D1").GetString());
        Assert.AreEqual("Leitung", objectSheet.Cell("B2").GetString());
        Assert.AreEqual("LT001", objectSheet.Cell("D2").GetString());
        Assert.IsTrue(objectSheet.Cell("A1").IsEmpty() && objectSheet.Cell("C1").IsEmpty(), "Unmapped columns should stay empty.");
        Assert.AreEqual("B1:D2", objectSheet.AutoFilter.Range.RangeAddress.ToString());
    }

    [TestMethod]
    public async Task RunAsync_PlacesEachPivotSheetBeforeItsDataSheet()
    {
        var geopackage = CreateTestGeoPackage(SeedStandardData, withObjects: true);
        var process = CreateProcess();

        var result = await process.RunAsync(geopackage, CreateCantonTemplate());

        using var workbook = await OpenOutputWorkbookAsync(result);
        Assert.AreEqual(string.Join(", ", Sheets), string.Join(", ", workbook.Worksheets.Select(worksheet => worksheet.Name)));
        Assert.IsTrue(workbook.Worksheet("Uebersicht WK").TabActive, "The workbook should open on its first sheet.");
    }

    [TestMethod]
    public async Task RunAsync_ErrorOverviews_CountErrorsAtA1WithFiltersAndWithoutSubtotals()
    {
        var geopackage = CreateTestGeoPackage(SeedStandardData);
        var process = CreateProcess();

        var result = await process.RunAsync(geopackage, CreateCantonTemplate());

        using var workbook = await OpenOutputWorkbookAsync(result);
        AssertOverviewPivot(workbook, "Uebersicht WK", "WK");
        AssertOverviewPivot(workbook, "Uebersicht GEP", "GEP");
    }

    [TestMethod]
    public async Task RunAsync_StatisticsPivots_CountAndShareTidAtA3WithSubtotals()
    {
        var geopackage = CreateTestGeoPackage(SeedStandardData, withObjects: true);
        var process = CreateProcess();

        var result = await process.RunAsync(geopackage, CreateCantonTemplate());

        using var workbook = await OpenOutputWorkbookAsync(result);
        AssertCountAndSharePivot(workbook, "drainage_system_current", "drainage_system_current");
        AssertCountAndSharePivot(workbook, "function", "function_hierarchic", "function");
        AssertCountAndSharePivot(workbook, "function_hierarchic", "function_hierarchic");
        AssertCountAndSharePivot(workbook, "usage_current", "function_hierarchic_group", "usage_current");
    }

    [TestMethod]
    public async Task RunAsync_StatisticsRows_KeepTheirTypes()
    {
        var geopackage = CreateTestGeoPackage(SeedStandardData, withObjects: true);
        var process = CreateProcess();

        var result = await process.RunAsync(geopackage, CreateCantonTemplate());

        using var workbook = await OpenOutputWorkbookAsync(result);
        var reaches = workbook.Worksheet("reach_data");
        Assert.AreEqual("LT1", reaches.Cell("A2").GetString());
        Assert.AreEqual(XLDataType.Number, reaches.Cell("B2").DataType, "year_of_construction should stay numeric.");
        Assert.AreEqual(1989, reaches.Cell("B2").GetValue<int>());
        Assert.IsTrue(reaches.Cell("C2").IsEmpty(), "A NULL remark should stay an empty cell.");
    }

    [TestMethod]
    public async Task RunAsync_WritesGeometriesAsWkt()
    {
        var geopackage = CreateTestGeoPackage(SeedStandardData, withObjects: true);
        var process = CreateProcess();

        var result = await process.RunAsync(geopackage, CreateCantonTemplate());

        using var workbook = await OpenOutputWorkbookAsync(result);
        var catchmentAreas = workbook.Worksheet("catchment_area_data");
        Assert.AreEqual("POLYGON ((2600000 1200000, 2600010 1200000, 2600010 1200010, 2600000 1200000))", CellText(catchmentAreas, "TEG1", "geom"));
        Assert.AreEqual(string.Empty, CellText(catchmentAreas, "TEG2", "geom"));

        var nodes = workbook.Worksheet("node_data");
        Assert.AreEqual("POINT (2600100 1200100)", CellText(nodes, "KN1", "geometry (WKT)"));
        Assert.AreEqual("POLYGON ((2600099 1200099, 2600101 1200099, 2600101 1200101, 2600099 1200099))", CellText(nodes, "KN1", "geometry_detail (WKT)"));
        Assert.AreEqual(string.Empty, CellText(nodes, "KN2", "geometry_detail (WKT)"));

        var reaches = workbook.Worksheet("reach_data");
        Assert.AreEqual("LINESTRING (2600100 1200100, 2600200 1200200)", CellText(reaches, "LT1", "geometry (WKT)"));
        Assert.AreEqual(string.Empty, CellText(reaches, "LT3", "geometry (WKT)"));
    }

    [TestMethod]
    public async Task RunAsync_GroupsReachesByTheirPaaOrSaaParent()
    {
        var geopackage = CreateTestGeoPackage(SeedStandardData, withObjects: true);
        var process = CreateProcess();

        var result = await process.RunAsync(geopackage, CreateCantonTemplate());

        using var workbook = await OpenOutputWorkbookAsync(result);
        var reaches = workbook.Worksheet("reach_data");
        Assert.AreEqual("PAA", CellText(reaches, "LT1", "function_hierarchic_group"));
        Assert.AreEqual("SAA", CellText(reaches, "LT2", "function_hierarchic_group"));
        Assert.AreEqual(string.Empty, CellText(reaches, "LT3", "function_hierarchic_group"));
    }

    [TestMethod]
    public async Task RunAsync_LeavesWktLongerThanAnExcelCellEmpty()
    {
        var geopackage = CreateTestGeoPackage(SeedStandardData, withObjects: true);

        // 2000 vertices give a WKT of about 50'000 characters, above Excel's limit of 32'767 per cell.
        var vertices = Enumerable.Range(0, 2000).Select(i => new Coordinate(2600000.125 + i, 1200000.375 + i)).ToArray();
        ObjectStatisticsGeoPackage.InsertLeitung(await geopackage.GetLocalPathAsync(), 23, "LT4", "PAA.Sammelkanal", "Mischabwasser", new LineString(vertices));
        var process = CreateProcess();

        var result = await process.RunAsync(geopackage, CreateCantonTemplate());

        using var workbook = await OpenOutputWorkbookAsync(result);
        var reaches = workbook.Worksheet("reach_data");
        Assert.AreEqual(string.Empty, CellText(reaches, "LT4", "geometry (WKT)"));
        Assert.AreEqual("PAA", CellText(reaches, "LT4", "function_hierarchic_group"));
    }

    [TestMethod]
    public async Task RunAsync_GeoPackageWithoutStatisticsViews_Throws()
    {
        // A GeoPackage that predates the views: every configured sheet needs its source, like the error tables.
        var path = Path.Combine(inputDirectory, "without-views.gpkg");
        using (var connection = new SqliteConnection($"Data Source={path};Pooling=false"))
        {
            connection.Open();
            CreateErrorTables(connection);
        }

        var process = CreateProcess();

        await Assert.ThrowsExactlyAsync<SqliteException>(() => process.RunAsync(new TestPipelineFile(path), CreateCantonTemplate()));
    }

    [TestMethod]
    public async Task RunAsync_FiltersEachDataSheetBelowAFrozenHeaderRow()
    {
        var geopackage = CreateTestGeoPackage(SeedStandardData, withObjects: true);
        var process = CreateProcess();

        var result = await process.RunAsync(geopackage, CreateCantonTemplate());

        using var workbook = await OpenOutputWorkbookAsync(result);
        AssertFilteredBelowFrozenHeader(workbook, "Error Data", "A1:K3");
        AssertFilteredBelowFrozenHeader(workbook, "Error Object", "A1:E2");
        AssertFilteredBelowFrozenHeader(workbook, "reach_data", "A1:G4");
    }

    [TestMethod]
    public async Task RunAsync_FillsCantonRawDataFromStatistics()
    {
        var geopackage = CreateTestGeoPackage(SeedStatistics);
        var process = CreateProcess();

        var result = await process.RunAsync(geopackage, CreateCantonTemplate());

        using var fileStream = await result.CantonErrorMatrix.OpenReadAsync();
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

    private ErrorOverviewExportProcess CreateProcess(
        Dictionary<string, ColumnConfig>? dataColumns = null,
        Dictionary<string, ColumnConfig>? objectColumns = null,
        string objectSheet = "Error Object")
    {
        return new ErrorOverviewExportProcess(
            errorData: new DataSheetConfig("Error Data", dataColumns ?? DataColumns),
            errorObject: new DataSheetConfig(objectSheet, objectColumns ?? ObjectColumns),
            catchmentArea: new DataSheetConfig("catchment_area_data", CatchmentAreaColumns),
            node: new DataSheetConfig("node_data", NodeColumns),
            reach: new DataSheetConfig("reach_data", ReachColumns),
            cantonErrorObjectSheet: "raw_data",
            cantonErrorColumnMapping: CantonErrorColumnMapping,
            pipelineFileManager: fileManager,
            logger: NullLogger.Instance);
    }

    // Every sheet's source must exist, so the GeoPackage starts from a model template with the analytical views.
    private TestPipelineFile CreateTestGeoPackage(Action<SqliteConnection>? seedAction = null, bool withObjects = false)
    {
        var gpkgPath = ObjectStatisticsGeoPackage.Create(inputDirectory, withObjects: withObjects);

        using var connection = new SqliteConnection($"Data Source={gpkgPath};Pooling=false");
        connection.Open();
        CreateErrorTables(connection);
        seedAction?.Invoke(connection);

        return new TestPipelineFile(gpkgPath);
    }

    private static void CreateErrorTables(SqliteConnection connection)
    {
        var sql = """
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
        ExecuteNonQuery(connection, sql);
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
        using var fileStream = await result.ErrorOverview.OpenReadAsync();
        var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;
        return new XLWorkbook(memoryStream);
    }

    private static void AssertOverviewPivot(XLWorkbook workbook, string sheetName, string priorityHeader)
    {
        var worksheet = workbook.Worksheet(sheetName);
        var pivotTable = worksheet.PivotTables.Single();

        // The fields are the error data headers of the configured attribute keys, the priority field first.
        Assert.AreEqual($"{priorityHeader}, Klasse, Fehler", string.Join(", ", pivotTable.RowLabels.Select(field => field.SourceName)), $"Row fields of '{sheetName}'.");
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

    private static void AssertFilteredBelowFrozenHeader(XLWorkbook workbook, string sheetName, string range)
    {
        var worksheet = workbook.Worksheet(sheetName);
        Assert.AreEqual(range, worksheet.AutoFilter.Range.RangeAddress.ToString(), $"Autofilter of '{sheetName}'.");
        Assert.AreEqual(1, worksheet.SheetView.SplitRow, $"Frozen rows of '{sheetName}'.");
    }

    // Column A holds the tid on every statistics sheet.
    private static string CellText(IXLWorksheet worksheet, string tid, string header)
    {
        var column = worksheet.Row(1).CellsUsed().Single(cell => cell.GetString() == header).Address.ColumnNumber;
        var row = worksheet.Column(1).CellsUsed().Single(cell => cell.GetString() == tid).Address.RowNumber;
        return worksheet.Cell(row, column).GetString();
    }

    [SuppressMessage("Security", "CA2100", Justification = "Test queries use hardcoded SQL, not user input.")]
    private static void ExecuteNonQuery(SqliteConnection connection, string sql)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }
}
