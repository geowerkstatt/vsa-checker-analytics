using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using VsaCheckerAnalytics.Processors.NetworkTopologyPatcher;

namespace VsaCheckerAnalytics.Processors.ErrorOverviewExport;

/// <summary>
/// Reads the object statistics of the error overview from the generated GeoPackage: for Teileinzugsgebiete,
/// Knoten and Leitungen the rows of the columns the canton's templates define, with geometries as WKT, and the
/// templates' pivot sheets over them.
/// </summary>
internal static class ObjectStatistics
{
    // Excel stores at most this many characters in a cell.
    private const int MaxCellLength = 32767;

    private const string CatchmentAreaQuery = """
        SELECT tid, runoff_limit_planned, runoff_limit_current, discharge_coefficient_rw_planned, discharge_coefficient_rw_current,
               discharge_coefficient_ww_planned, discharge_coefficient_ww_current, seal_factor_rw_planned, seal_factor_rw_current,
               seal_factor_ww_planned, seal_factor_ww_current, remark, identifier, direct_discharge_planned, direct_discharge_current,
               population_density_planned, population_density_current, drainage_system_planned, drainage_system_current, surface_area,
               sewer_infiltration_water_production_planned, sewer_infiltration_water_production_current, geom, retention_planned,
               retention_current, waste_water_production_planned, waste_water_production_current, infiltration_planned, infiltration_current,
               fk_node_rw_current, fk_node_rw_planned, fk_node_ww_current, fk_node_ww_planned,
               fk_sb_rw_current_lc_syphon_head, fk_sb_rw_current_lc_stormwater_retention_channel, fk_sb_rw_current_lc_separating_structure,
               fk_sb_rw_current_lc_discharge_point, fk_sb_rw_current_lc_stormwater_overflow, fk_sb_rw_current_lc_stormwater_overflow_tank,
               fk_sb_rw_current_lc_pump_station, fk_sb_rw_current_lc_autonomous_measuring_point, fk_sb_rw_current_lc_other,
               fk_sb_rw_planned_lc_syphon_head, fk_sb_rw_planned_lc_stormwater_retention_channel, fk_sb_rw_planned_lc_separating_structure,
               fk_sb_rw_planned_lc_discharge_point, fk_sb_rw_planned_lc_stormwater_overflow, fk_sb_rw_planned_lc_stormwater_overflow_tank,
               fk_sb_rw_planned_lc_pump_station, fk_sb_rw_planned_lc_autonomous_measuring_point, fk_sb_rw_planned_lc_other,
               fk_sb_ww_current_lc_syphon_head, fk_sb_ww_current_lc_stormwater_retention_channel, fk_sb_ww_current_lc_separating_structure,
               fk_sb_ww_current_lc_discharge_point, fk_sb_ww_current_lc_stormwater_overflow, fk_sb_ww_current_lc_stormwater_overflow_tank,
               fk_sb_ww_current_lc_pump_station, fk_sb_ww_current_lc_autonomous_measuring_point, fk_sb_ww_current_lc_other,
               fk_sb_ww_planned_lc_syphon_head, fk_sb_ww_planned_lc_stormwater_retention_channel, fk_sb_ww_planned_lc_separating_structure,
               fk_sb_ww_planned_lc_discharge_point, fk_sb_ww_planned_lc_stormwater_overflow, fk_sb_ww_planned_lc_stormwater_overflow_tank,
               fk_sb_ww_planned_lc_pump_station, fk_sb_ww_planned_lc_autonomous_measuring_point, fk_sb_ww_planned_lc_other,
               data_owner, data_owner_org_type, data_provider, data_provider_org_type, last_modification
          FROM v_vsa_teileinzugsgebiet
        """;

    private const string NodeQuery = """
        SELECT tid, wwtp_no, year_of_construction, structure_condition, remark, identifier, cover_level, dimension1, dimension2,
               financing, function, function_hierarchic, positional_accuracy, usage_planned, usage_current, fk_wastewater_structure,
               fk_cover, backflow_level, renovation_necessity, bottom_level, status, symbolori, accessibility, condition_survey_year,
               operator, operator_org_type, owner, owner_org_type, data_owner, data_owner_org_type, data_provider,
               data_provider_org_type, last_modification,
               geom AS "geometry (WKT)",
               (SELECT k.detailgeometrie FROM knoten k WHERE k.T_Id = v_vsa_knoten.fid) AS "geometry_detail (WKT)"
          FROM v_vsa_knoten
        """;

    private const string ReachQuery = """
        SELECT tid, year_of_construction, structure_condition, remark, identifier, financing, function_hierarchic,
               -- the parent group (PAA or SAA) of values like PAA.Sammelkanal
               substr(function_hierarchic, 1, instr(function_hierarchic || '.', '.') - 1) AS function_hierarchic_group,
               function_hydraulic, elevation_accuracy_to, elevation_accuracy_from, hydraulic_load_current, level_to, level_from,
               length_effective, horizontal_positioning, leak_protection, clear_width, clear_height, material, usage_planned,
               usage_current, fk_wastewater_structure, fk_reach_point_to, fk_reach_point_from, profile_type, relining_kind,
               reliner_nominal_size, renovation_necessity, status, wall_roughness, rv_base_year, rv_construction_type,
               replacement_value, condition_survey_year, operator, operator_org_type, owner, owner_org_type, fk_node_to,
               fk_node_from, fk_reach_to, fk_pipe_profile, data_owner, data_owner_org_type, data_provider,
               data_provider_org_type, last_modification,
               geom AS "geometry (WKT)"
          FROM v_vsa_leitung
        """;

    private static readonly Definition[] Definitions =
    [
        new("v_vsa_teileinzugsgebiet", "catchment_area_data", CatchmentAreaQuery, [TemplatePivot("drainage_system_current", "drainage_system_current")]),
        new("v_vsa_knoten", "node_data", NodeQuery, [TemplatePivot("function", "function_hierarchic", "function")]),
        new(
            "v_vsa_leitung",
            "reach_data",
            ReachQuery,
            [TemplatePivot("function_hierarchic", "function_hierarchic"), TemplatePivot("usage_current", "function_hierarchic_group", "usage_current")]),
    ];

    private sealed record Definition(string SourceView, string SheetName, string Query, IReadOnlyList<PivotSheet> Pivots);

    // As in the canton's templates: at A3, the count of tid and its share of the total per value of the row fields, with
    // Excel's default subtotals, so a two-level pivot totals each group in its group row.
    private static PivotSheet TemplatePivot(string sheetName, params string[] rowFields) => new(
        sheetName,
        Location: "A3",
        RowFields: rowFields,
        Subtotals: true,
        Filters: [],
        Values: [new("tid", "Anzahl von tid", ShareOfTotal: false), new("tid", "Anteil", ShareOfTotal: true)],
        ColumnWidths: []);

    /// <summary>
    /// Reads the raw data sheets in the order of the templates. An object class whose source view is missing,
    /// as in a GeoPackage that predates the view, is left out with a warning. Geometries that cannot be read, or
    /// whose WKT exceeds what an Excel cell holds, are returned as <see langword="null"/>.
    /// </summary>
    /// <param name="connection">An open connection to the generated GeoPackage.</param>
    /// <param name="logger">Logger for skipped object classes and geometries.</param>
    /// <returns>The raw data sheets with their pivot sheets.</returns>
    internal static IReadOnlyList<DataSheet> Read(SqliteConnection connection, ILogger logger)
    {
        var sheets = new List<DataSheet>();
        foreach (var definition in Definitions)
        {
            if (!ViewExists(connection, definition.SourceView))
            {
                logger.LogWarning("View '{View}' not found; statistics sheet '{Sheet}' and its pivots skipped.", definition.SourceView, definition.SheetName);
                continue;
            }

            sheets.Add(ReadSheet(connection, definition, logger));
        }

        return sheets;
    }

    [SuppressMessage("Security", "CA2100", Justification = "The queries are internal constants, not user input.")]
    private static DataSheet ReadSheet(SqliteConnection connection, Definition definition, ILogger logger)
    {
        using var command = connection.CreateCommand();
        command.CommandText = definition.Query;
        using var reader = command.ExecuteReader();

        var columns = Enumerable.Range(0, reader.FieldCount).Select(column => new SheetColumn(column + 1, reader.GetName(column))).ToArray();
        var rows = new List<IReadOnlyList<object?>>();
        var skippedGeometries = 0;
        while (reader.Read())
        {
            var values = new object?[reader.FieldCount];
            for (var column = 0; column < values.Length; column++)
            {
                var value = reader.IsDBNull(column) ? null : reader.GetValue(column);

                // The geometry columns are the only blob columns of the source views.
                if (value is byte[] geometry)
                {
                    value = ToWkt(geometry);
                    if (value is null)
                        skippedGeometries++;
                }

                values[column] = value;
            }

            rows.Add(values);
        }

        if (skippedGeometries > 0)
        {
            logger.LogWarning(
                "{Count} geometries for sheet '{Sheet}' left empty: unreadable, or longer than the {MaxCellLength} characters an Excel cell holds.",
                skippedGeometries,
                definition.SheetName,
                MaxCellLength);
        }

        logger.LogDebug("Read {RowCount} rows for statistics sheet '{Sheet}'.", rows.Count, definition.SheetName);
        return new DataSheet(definition.SheetName, columns, rows, definition.Pivots);
    }

    private static string? ToWkt(byte[] geometry)
    {
        try
        {
            var wkt = GeoPackageGeometryCodec.ReadGeometry(geometry, out _).AsText();
            return wkt.Length <= MaxCellLength ? wkt : null;
        }
        catch (InvalidDataException)
        {
            return null;
        }
    }

    private static bool ViewExists(SqliteConnection connection, string viewName)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'view' AND name = @name";
        command.Parameters.AddWithValue("@name", viewName);
        return (long)(command.ExecuteScalar() ?? 0L) > 0;
    }
}
