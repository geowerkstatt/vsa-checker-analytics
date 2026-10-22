using ClosedXML.Excel;
using Geopilot.Pipeline;
using Geopilot.PipelineCore.Pipeline;
using Microsoft.Data.Sqlite;
using VsaCheckerAnalytics.TestHelpers;

namespace VsaCheckerAnalytics.Processors.ErrorOverviewExport;

[TestClass]
public class ErrorOverviewExportProcessIntegrationTest
{
    private const string StepId = "error_overview_export_de";
    private const string GeopackageGenerationStepId = "geopackage_generation";
    private const string VsaMatcherStepId = "vsa_matcher";

    private static readonly string[] Sheets =
    [
        "Uebersicht WK", "Uebersicht GEP", "Error Data", "Error Object",
        "drainage_system_current", "catchment_area_data", "function", "node_data", "function_hierarchic", "usage_current", "reach_data",
    ];

    // Header rows of the canton's templates statistics_catchment_area.xlsx, statistics_node.xlsx and statistics_reach.xlsx.
    private static readonly string[] CatchmentAreaHeaders =
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

    private static readonly string[] NodeHeaders =
    [
        "tid", "wwtp_no", "year_of_construction", "structure_condition", "remark", "identifier", "cover_level", "dimension1", "dimension2",
        "financing", "function", "function_hierarchic", "positional_accuracy", "usage_planned", "usage_current", "fk_wastewater_structure",
        "fk_cover", "backflow_level", "renovation_necessity", "bottom_level", "status", "symbolori", "accessibility", "condition_survey_year",
        "operator", "operator_org_type", "owner", "owner_org_type", "data_owner", "data_owner_org_type", "data_provider",
        "data_provider_org_type", "last_modification", "geometry (WKT)", "geometry_detail (WKT)",
    ];

    private static readonly string[] ReachHeaders =
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

    /// <summary>
    /// Stands in for the result of <see cref="GeopackageGenerationStepId"/>. Only the property the definition references
    /// via <c>${step_output(geopackage_generation.GeneratedGeopackage)}</c> is needed.
    /// </summary>
    private sealed record GeopackageGenerationStepResult(IPipelineFile GeneratedGeopackage);

    /// <summary>
    /// Stands in for the result of <see cref="VsaMatcherStepId"/>.
    /// </summary>
    private sealed record VsaMatcherStepResult(string? Language);

    private TestPipelineHost host = null!;

    [TestInitialize]
    public void SetUp() => host = TestPipelineHost.Create();

    [TestCleanup]
    public void Cleanup() => host?.Dispose();

    [TestMethod]
    [DataRow("template_ca_dssmini_2020_d.gpkg")]
    [DataRow("template_ca_dssmini_2020_1_d.gpkg")]
    public async Task RunErrorOverviewExportStep(string template)
    {
        using var pipeline = host.CreatePipeline();
        var step = pipeline.Steps.Single(s => s.Id == StepId);

        var context = new PipelineContext
        {
            Upload = [],
            StepResults = new Dictionary<string, StepResult>
            {
                [VsaMatcherStepId] = new() { Result = new VsaMatcherStepResult("DE") },
                [GeopackageGenerationStepId] = new() { Result = new GeopackageGenerationStepResult(CreateTestGeoPackage(template)) },
            },
        };

        var result = await step.Run(context, CancellationToken.None);

        Assert.AreEqual(StepState.Success, step.State);

        // The canton matrix is filled from the template wired in the definition via ${file()}.
        var cantonMatrix = Assert.IsInstanceOfType<IPipelineFile>(result.ExtractProperty(nameof(ErrorOverviewExportResult.CantonErrorMatrix)));
        using var cantonStream = await cantonMatrix.OpenReadAsync();
        Assert.IsGreaterThan(0, cantonStream.Length);

        // Sheets and columns come from the definition, bound by the geopilot host; the pivots are fixed in code.
        var errorOverview = Assert.IsInstanceOfType<IPipelineFile>(result.ExtractProperty(nameof(ErrorOverviewExportResult.ErrorOverview)));
        using var overviewStream = await errorOverview.OpenReadAsync();
        using var workbook = new XLWorkbook(overviewStream);

        Assert.AreEqual(string.Join(", ", Sheets), string.Join(", ", workbook.Worksheets.Select(worksheet => worksheet.Name)));
        Assert.AreEqual("Recommendation Detail", workbook.Worksheet("Error Data").Cell("A1").GetString());
        Assert.AreEqual("FID", workbook.Worksheet("Error Data").Cell("R1").GetString());

        CollectionAssert.AreEqual(CatchmentAreaHeaders, HeaderRow(workbook.Worksheet("catchment_area_data")));
        CollectionAssert.AreEqual(NodeHeaders, HeaderRow(workbook.Worksheet("node_data")));
        CollectionAssert.AreEqual(ReachHeaders, HeaderRow(workbook.Worksheet("reach_data")));

        var overview = workbook.Worksheet("Uebersicht WK");
        Assert.AreEqual("Prüftyp, Funktionhierarchisch, Eigentümer, Status", string.Join(", ", overview.PivotTables.Single().ReportFilters.Select(field => field.SourceName)));
        Assert.AreEqual(105, overview.Column("A").Width);

        var usage = workbook.Worksheet("usage_current").PivotTables.Single();
        Assert.AreEqual("function_hierarchic_group, usage_current", string.Join(", ", usage.RowLabels.Select(field => field.SourceName)));
        Assert.IsTrue(usage.RowLabels.All(field => field.Subtotals.Contains(XLSubtotalFunction.Automatic)));
        Assert.AreEqual(XLPivotCalculation.PercentageOfTotal, usage.Values.Single(value => value.CustomName == "Anteil").Calculation);
    }

    private static string[] HeaderRow(IXLWorksheet worksheet)
        => worksheet.Row(1).CellsUsed().Select(cell => cell.GetString()).ToArray();

    private TestPipelineFile CreateTestGeoPackage(string template)
    {
        var gpkgPath = ObjectStatisticsGeoPackage.Create(host.WorkingDirectory, template);

        using var connection = new SqliteConnection($"Data Source={gpkgPath};Pooling=false");
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
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
                count_error INTEGER, uc_max INTEGER, gsp_max INTEGER);

            INSERT INTO ca_error_data (tid, class, errorid, uc, gsp, error, check_type, function_hierarchic, owner, status, fid)
            VALUES ('LT001', 'Leitung', 't_001', 1, 2, 'Fehler 1', 'Traegerschaft', 'primaer', 'Gemeinde', 'in_Betrieb', 1),
                   ('LT001', 'Leitung', 'a_001', 2, 1, 'Fehler 2', 'ARA', 'sekundaer', 'Kanton', 'geplant', 2);

            INSERT INTO ca_error_object (tid, class, count_error, uc_max, gsp_max)
            VALUES ('LT001', 'Leitung', 2, 2, 2);
            """;
        cmd.ExecuteNonQuery();

        return new TestPipelineFile(gpkgPath);
    }
}
