using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using NetTopologySuite.Geometries;
using VsaCheckerAnalytics.TestHelpers;

namespace VsaCheckerAnalytics.Processors.ErrorOverviewExport;

[TestClass]
public sealed class ObjectStatisticsTest
{
    // Header rows of the canton's templates statistics_catchment_area.xlsx, statistics_node.xlsx and statistics_reach.xlsx.
    private static readonly string[] CatchmentAreaColumns =
    [
        "tid", "runoff_limit_planned", "runoff_limit_current", "discharge_coefficient_rw_planned", "discharge_coefficient_rw_current",
        "discharge_coefficient_ww_planned", "discharge_coefficient_ww_current", "seal_factor_rw_planned", "seal_factor_rw_current",
        "seal_factor_ww_planned", "seal_factor_ww_current", "remark", "identifier", "direct_discharge_planned", "direct_discharge_current",
        "population_density_planned", "population_density_current", "drainage_system_planned", "drainage_system_current", "surface_area",
        "sewer_infiltration_water_production_planned", "sewer_infiltration_water_production_current", "geom", "retention_planned",
        "retention_current", "waste_water_production_planned", "waste_water_production_current", "infiltration_planned", "infiltration_current",
        "fk_node_rw_current", "fk_node_rw_planned", "fk_node_ww_current", "fk_node_ww_planned",
        "fk_sb_rw_current_lc_syphon_head", "fk_sb_rw_current_lc_stormwater_retention_channel", "fk_sb_rw_current_lc_separating_structure",
        "fk_sb_rw_current_lc_discharge_point", "fk_sb_rw_current_lc_stormwater_overflow", "fk_sb_rw_current_lc_stormwater_overflow_tank",
        "fk_sb_rw_current_lc_pump_station", "fk_sb_rw_current_lc_autonomous_measuring_point", "fk_sb_rw_current_lc_other",
        "fk_sb_rw_planned_lc_syphon_head", "fk_sb_rw_planned_lc_stormwater_retention_channel", "fk_sb_rw_planned_lc_separating_structure",
        "fk_sb_rw_planned_lc_discharge_point", "fk_sb_rw_planned_lc_stormwater_overflow", "fk_sb_rw_planned_lc_stormwater_overflow_tank",
        "fk_sb_rw_planned_lc_pump_station", "fk_sb_rw_planned_lc_autonomous_measuring_point", "fk_sb_rw_planned_lc_other",
        "fk_sb_ww_current_lc_syphon_head", "fk_sb_ww_current_lc_stormwater_retention_channel", "fk_sb_ww_current_lc_separating_structure",
        "fk_sb_ww_current_lc_discharge_point", "fk_sb_ww_current_lc_stormwater_overflow", "fk_sb_ww_current_lc_stormwater_overflow_tank",
        "fk_sb_ww_current_lc_pump_station", "fk_sb_ww_current_lc_autonomous_measuring_point", "fk_sb_ww_current_lc_other",
        "fk_sb_ww_planned_lc_syphon_head", "fk_sb_ww_planned_lc_stormwater_retention_channel", "fk_sb_ww_planned_lc_separating_structure",
        "fk_sb_ww_planned_lc_discharge_point", "fk_sb_ww_planned_lc_stormwater_overflow", "fk_sb_ww_planned_lc_stormwater_overflow_tank",
        "fk_sb_ww_planned_lc_pump_station", "fk_sb_ww_planned_lc_autonomous_measuring_point", "fk_sb_ww_planned_lc_other",
        "data_owner", "data_owner_org_type", "data_provider", "data_provider_org_type", "last_modification",
    ];

    private static readonly string[] NodeColumns =
    [
        "tid", "wwtp_no", "year_of_construction", "structure_condition", "remark", "identifier", "cover_level", "dimension1", "dimension2",
        "financing", "function", "function_hierarchic", "positional_accuracy", "usage_planned", "usage_current", "fk_wastewater_structure",
        "fk_cover", "backflow_level", "renovation_necessity", "bottom_level", "status", "symbolori", "accessibility", "condition_survey_year",
        "operator", "operator_org_type", "owner", "owner_org_type", "data_owner", "data_owner_org_type", "data_provider",
        "data_provider_org_type", "last_modification", "geometry (WKT)", "geometry_detail (WKT)",
    ];

    private static readonly string[] ReachColumns =
    [
        "tid", "year_of_construction", "structure_condition", "remark", "identifier", "financing", "function_hierarchic",
        "function_hierarchic_group", "function_hydraulic", "elevation_accuracy_to", "elevation_accuracy_from", "hydraulic_load_current",
        "level_to", "level_from", "length_effective", "horizontal_positioning", "leak_protection", "clear_width", "clear_height", "material",
        "usage_planned", "usage_current", "fk_wastewater_structure", "fk_reach_point_to", "fk_reach_point_from", "profile_type",
        "relining_kind", "reliner_nominal_size", "renovation_necessity", "status", "wall_roughness", "rv_base_year", "rv_construction_type",
        "replacement_value", "condition_survey_year", "operator", "operator_org_type", "owner", "owner_org_type", "fk_node_to",
        "fk_node_from", "fk_reach_to", "fk_pipe_profile", "data_owner", "data_owner_org_type", "data_provider", "data_provider_org_type",
        "last_modification", "geometry (WKT)",
    ];

    private string directory = null!;

    [TestInitialize]
    public void Setup()
    {
        directory = Path.Combine(Path.GetTempPath(), "vsa-object-statistics-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(directory);
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(directory))
            Directory.Delete(directory, recursive: true);
    }

    [TestMethod]
    [DataRow("template_ca_dssmini_2020_d.gpkg")]
    [DataRow("template_ca_dssmini_2020_1_d.gpkg")]
    public void Read_ReturnsTheColumnsOfTheTemplates(string template)
    {
        var sheets = ReadStatistics(ObjectStatisticsGeoPackage.Create(directory, template));

        CollectionAssert.AreEqual(CatchmentAreaColumns, Headers(Sheet(sheets, "catchment_area_data")));
        CollectionAssert.AreEqual(NodeColumns, Headers(Sheet(sheets, "node_data")));
        CollectionAssert.AreEqual(ReachColumns, Headers(Sheet(sheets, "reach_data")));
    }

    [TestMethod]
    public void Read_ReturnsGeometriesAsWkt()
    {
        var sheets = ReadStatistics(ObjectStatisticsGeoPackage.Create(directory));

        var catchmentAreas = Sheet(sheets, "catchment_area_data");
        Assert.AreEqual("POLYGON ((2600000 1200000, 2600010 1200000, 2600010 1200010, 2600000 1200000))", Value(catchmentAreas, "TEG1", "geom"));
        Assert.IsNull(Value(catchmentAreas, "TEG2", "geom"));

        var nodes = Sheet(sheets, "node_data");
        Assert.AreEqual("POINT (2600100 1200100)", Value(nodes, "KN1", "geometry (WKT)"));
        Assert.AreEqual("POLYGON ((2600099 1200099, 2600101 1200099, 2600101 1200101, 2600099 1200099))", Value(nodes, "KN1", "geometry_detail (WKT)"));
        Assert.IsNull(Value(nodes, "KN2", "geometry_detail (WKT)"));

        var reaches = Sheet(sheets, "reach_data");
        Assert.AreEqual("LINESTRING (2600100 1200100, 2600200 1200200)", Value(reaches, "LT1", "geometry (WKT)"));
        Assert.IsNull(Value(reaches, "LT3", "geometry (WKT)"));
    }

    [TestMethod]
    public void Read_GroupsReachesByTheirPaaOrSaaParent()
    {
        var reaches = Sheet(ReadStatistics(ObjectStatisticsGeoPackage.Create(directory)), "reach_data");

        Assert.AreEqual("PAA", Value(reaches, "LT1", "function_hierarchic_group"));
        Assert.AreEqual("SAA", Value(reaches, "LT2", "function_hierarchic_group"));
        Assert.IsNull(Value(reaches, "LT3", "function_hierarchic_group"));
    }

    [TestMethod]
    public void Read_DropsWktLongerThanAnExcelCell()
    {
        var path = ObjectStatisticsGeoPackage.Create(directory);

        // 2000 vertices give a WKT of about 50'000 characters, above Excel's limit of 32'767 per cell.
        var vertices = Enumerable.Range(0, 2000).Select(i => new Coordinate(2600000.125 + i, 1200000.375 + i)).ToArray();
        ObjectStatisticsGeoPackage.InsertLeitung(path, 23, "LT4", "PAA.Sammelkanal", "Mischabwasser", new LineString(vertices));

        var reaches = Sheet(ReadStatistics(path), "reach_data");

        Assert.IsNull(Value(reaches, "LT4", "geometry (WKT)"));
        Assert.AreEqual("PAA", Value(reaches, "LT4", "function_hierarchic_group"));
    }

    [TestMethod]
    public void Read_WithoutObjectViews_ReturnsNoSheets()
    {
        // A GeoPackage that predates the views, like the minimal fixtures of the error overview tests.
        var path = Path.Combine(directory, "without-views.gpkg");
        using (var connection = new SqliteConnection($"Data Source={path};Pooling=false"))
        {
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "CREATE TABLE ca_error_data (fid INTEGER NOT NULL PRIMARY KEY, tid TEXT)";
            command.ExecuteNonQuery();
        }

        Assert.IsEmpty(ReadStatistics(path));
    }

    private static IReadOnlyList<DataSheet> ReadStatistics(string gpkgPath)
    {
        using var connection = new SqliteConnection($"Data Source={gpkgPath};Mode=ReadOnly;Pooling=false");
        connection.Open();
        return ObjectStatistics.Read(connection, NullLogger.Instance);
    }

    private static DataSheet Sheet(IReadOnlyList<DataSheet> sheets, string name)
        => sheets.Single(sheet => sheet.Name == name);

    private static string[] Headers(DataSheet sheet)
        => sheet.Columns.Select(column => column.Header).ToArray();

    private static object? Value(DataSheet sheet, string tid, string column)
    {
        var columnIndex = Array.IndexOf(Headers(sheet), column);
        Assert.IsGreaterThanOrEqualTo(0, columnIndex, $"Column '{column}' not found.");
        return sheet.Rows.Single(row => Equals(row[0], tid))[columnIndex];
    }
}
