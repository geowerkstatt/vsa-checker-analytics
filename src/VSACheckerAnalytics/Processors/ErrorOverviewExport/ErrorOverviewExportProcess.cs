using ClosedXML.Excel;
using Geopilot.PipelineCore.Pipeline;
using Geopilot.PipelineCore.Pipeline.Process;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;

namespace VsaCheckerAnalytics.Processors.ErrorOverviewExport;

/// <summary>
/// Exports the error overview workbook from the generated GeoPackage: the error sheets (<c>ca_error_data</c>,
/// <c>ca_error_object</c>) and the statistics sheets of the Teileinzugsgebiete, Knoten and Leitungen, each configured
/// the same way (sheet name and columns) and preceded by its pivot sheets, which are fixed in code. Also fills the
/// canton's error matrix template.
/// </summary>
public sealed partial class ErrorOverviewExportProcess
{
    private const string StatisticsTable = "ca_statistics_attribute";
    private const string StatisticsOrderColumn = "sortierung";
    private const string CantonMatrixFileName = "kantonale_fehlermatrix";

    // Excel's built-in number format "0.00%".
    private const int PercentNumberFormatId = 10;

    // The sources of the data sheets: a table, a view, or a subquery adding attributes the view lacks.
    private const string ErrorDataSource = "\"ca_error_data\"";
    private const string ErrorObjectSource = "\"ca_error_object\"";
    private const string CatchmentAreaSource = "\"v_vsa_teileinzugsgebiet\"";
    private const string NodeSource = """
        (SELECT v.*, k.detailgeometrie AS geom_detail
           FROM v_vsa_knoten v
           LEFT JOIN knoten k ON k.T_Id = v.fid)
        """;

    private const string ReachSource = """
        (SELECT *,
                -- the parent group (PAA or SAA) of values like PAA.Sammelkanal
                substr(function_hierarchic, 1, instr(function_hierarchic || '.', '.') - 1) AS function_hierarchic_group
           FROM v_vsa_leitung)
        """;

    private static readonly IReadOnlyList<string> OverviewFilters = ["check_type", "function_hierarchic", "owner", "status"];
    private static readonly IReadOnlyList<PivotValueConfig> ErrorCount = [new("fid", "Anzahl Fehler", Share: false)];
    private static readonly IReadOnlyList<double> OverviewColumnWidths = [105, 13];
    private static readonly IReadOnlyList<PivotValueConfig> TidCountAndShare = [new("tid", "Anzahl von tid", Share: false), new("tid", "Anteil", Share: true)];

    // The pivot sheets of the data sheets, by attribute key: the configured columns must contain their fields.
    private static readonly IReadOnlyList<PivotConfig> ErrorDataPivots =
    [
        new("Uebersicht WK", "A1", ["uc", "class", "error"], OverviewFilters, ErrorCount, Subtotals: false, OverviewColumnWidths),
        new("Uebersicht GEP", "A1", ["gsp", "class", "error"], OverviewFilters, ErrorCount, Subtotals: false, OverviewColumnWidths),
    ];

    private static readonly IReadOnlyList<PivotConfig> CatchmentAreaPivots = [TemplatePivot("drainage_system_current", "drainage_system_current")];
    private static readonly IReadOnlyList<PivotConfig> NodePivots = [TemplatePivot("function", "function_hierarchic", "function")];
    private static readonly IReadOnlyList<PivotConfig> ReachPivots =
    [
        TemplatePivot("function_hierarchic", "function_hierarchic"),
        TemplatePivot("usage_current", "function_hierarchic_group", "usage_current"),
    ];

    private static readonly LocalizedText ErrorOverviewStatusMessageFormat = new Dictionary<string, string>
    {
        { "de", "Fehler-Übersicht erstellt: {0} Fehler exportiert." },
        { "fr", "Aperçu des erreurs créé : {0} erreurs exportées." },
        { "it", "Panoramica degli errori creata: {0} errori esportati." },
        { "en", "Error overview created: {0} errors exported." },
    };

    private readonly IReadOnlyList<SheetDefinition> sheets;
    private readonly string cantonSheetName;
    private readonly IReadOnlyList<(string Attribute, int Number)> cantonColumns;
    private readonly IPipelineFileManager pipelineFileManager;
    private readonly ILogger logger;

    /// <summary>
    /// Initializes a new <see cref="ErrorOverviewExportProcess"/>.
    /// </summary>
    /// <param name="errorData">The sheet of <c>ca_error_data</c>, the first data sheet; the status message counts its rows.</param>
    /// <param name="errorObject">The sheet of <c>ca_error_object</c>.</param>
    /// <param name="catchmentArea">The statistics sheet of the Teileinzugsgebiete (<c>v_vsa_teileinzugsgebiet</c>).</param>
    /// <param name="node">The statistics sheet of the Knoten (<c>v_vsa_knoten</c> plus <c>geom_detail</c>, the detail geometry).</param>
    /// <param name="reach">The statistics sheet of the Leitungen (<c>v_vsa_leitung</c> plus <c>function_hierarchic_group</c>, PAA or SAA).</param>
    /// <param name="cantonMatrix">The raw data sheet of the canton error matrix template, filled from <c>ca_statistics_attribute</c>.</param>
    /// <param name="pipelineFileManager">Pipeline file manager for output file allocation.</param>
    /// <param name="logger">Logger.</param>
    public ErrorOverviewExportProcess(
        DataSheetConfig errorData,
        DataSheetConfig errorObject,
        DataSheetConfig catchmentArea,
        DataSheetConfig node,
        DataSheetConfig reach,
        CantonMatrixConfig cantonMatrix,
        IPipelineFileManager pipelineFileManager,
        ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(cantonMatrix);

        this.pipelineFileManager = pipelineFileManager ?? throw new ArgumentNullException(nameof(pipelineFileManager));
        this.logger = logger ?? NullLogger.Instance;

        this.sheets =
        [
            BuildSheet(errorData, ErrorDataSource, ErrorDataPivots),
            BuildSheet(errorObject, ErrorObjectSource, []),
            BuildSheet(catchmentArea, CatchmentAreaSource, CatchmentAreaPivots),
            BuildSheet(node, NodeSource, NodePivots),
            BuildSheet(reach, ReachSource, ReachPivots),
        ];
        EnsureUniqueSheetNames(this.sheets);

        this.cantonSheetName = cantonMatrix.Sheet;
        this.cantonColumns = BuildCantonColumns(cantonMatrix);
    }

    /// <summary>
    /// Reads every configured data sheet from the <paramref name="geopackage"/> and writes the error overview workbook,
    /// each data sheet preceded by its pivot sheets. Then fills the canton error matrix.
    /// </summary>
    /// <param name="geopackage">The generated GeoPackage with the error tables and the analytical views.</param>
    /// <param name="cantonErrorMatrixTemplate">Excel template for the canton specific error matrix.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ErrorOverviewExportResult"/> with the error overview, the filled canton error matrix, and a localized status message.</returns>
    [PipelineProcessRun]
    public async Task<ErrorOverviewExportResult> RunAsync(IPipelineFile geopackage, IPipelineFile cantonErrorMatrixTemplate, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(geopackage);
        ArgumentNullException.ThrowIfNull(cantonErrorMatrixTemplate);

        var gpkgPath = await geopackage.GetLocalPathAsync(cancellationToken);

        using var connection = OpenGeoPackage(gpkgPath);
        var dataSheets = sheets.Select(sheet => DataSheetReader.Read(connection, sheet, logger)).ToList();
        cancellationToken.ThrowIfCancellationRequested();

        using var workbook = new XLWorkbook();
        foreach (var sheet in dataSheets)
        {
            var data = AddDataSheet(workbook, sheet);
            foreach (var pivot in sheet.Pivots)
            {
                // At the data sheet's position, so every pivot sheet precedes its data, as in the canton's templates.
                AddPivotSheet(workbook, pivot, data, data.Worksheet.Position);
            }
        }

        var outputFile = pipelineFileManager.GeneratePipelineFile("errorOverview", "xlsx");
        await using var writeStream = outputFile.OpenWriteFileStream();
        workbook.SaveAs(writeStream);

        logger.LogInformation("Exported error overview to <{FileName}>.", outputFile.OriginalFileName);

        cancellationToken.ThrowIfCancellationRequested();
        var cantonMatrix = await ExportCantonMatrixAsync(cantonErrorMatrixTemplate, connection, cancellationToken);

        var statusMessage = ErrorOverviewStatusMessageFormat
            .Map(msg => string.Format(CultureInfo.InvariantCulture, msg, dataSheets[0].Rows.Count));

        return new ErrorOverviewExportResult
        {
            ErrorOverview = outputFile,
            CantonErrorMatrix = cantonMatrix,
            StatusMessage = statusMessage,
        };
    }

    /// <summary>
    /// Fills the canton error matrix by writing the <c>ca_statistics_attribute</c> rows into a
    /// writable copy of <paramref name="template"/>. The workbook's validation sheets reference the
    /// raw data sheet by fixed cell, so values are written in place (existing contents cleared, no
    /// rows deleted) to keep those references intact.
    /// </summary>
    private async Task<IPipelineFile> ExportCantonMatrixAsync(IPipelineFile template, SqliteConnection connection, CancellationToken cancellationToken)
    {
        var copy = await pipelineFileManager.CreateWritableCopyAsync(template, CantonMatrixFileName, cancellationToken);
        var copyPath = await copy.GetLocalPathAsync(cancellationToken);

        using (var workbook = new XLWorkbook(copyPath))
        {
            FillCantonRawData(workbook.Worksheet(cantonSheetName), connection);

            // Writing into the raw data sheet makes it the active tab; restore the first sheet (the
            // canton's landing/validation sheet) so the workbook opens there, not on the raw data.
            workbook.Worksheets.First().SetTabActive();
            workbook.Save();
        }

        logger.LogInformation("Exported canton error matrix to <{FileName}>.", copy.OriginalFileName);
        return copy;
    }

    [SuppressMessage("Security", "CA2100", Justification = "Column and table names are internal pipeline constants, not user input.")]
    private void FillCantonRawData(IXLWorksheet worksheet, SqliteConnection connection)
    {
        if (!TableExists(connection, StatisticsTable))
        {
            logger.LogWarning("Table '{Table}' not found; canton raw data sheet '{Sheet}' left empty.", StatisticsTable, cantonSheetName);
            return;
        }

        // Clear existing values in place so the sheet's cells survive and the workbook's validation
        // formulas that reference them stay valid; deleting rows would turn those into #REF!.
        worksheet.RangeUsed()?.Clear(XLClearOptions.Contents);

        var columnList = string.Join(", ", cantonColumns.Select(column => $"\"{column.Attribute}\""));

        // Order explicitly by the statistics view's sortierung column, not by rowid: the canton
        // template's validation sheets reference raw_data rows by fixed cell, so the rows must match
        // the sortierung order the template was built against. Relying on rowid would silently depend
        // on the materialization happening to insert in the view's ORDER BY order.
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT {columnList} FROM \"{StatisticsTable}\" ORDER BY \"{StatisticsOrderColumn}\"";
        using var reader = command.ExecuteReader();

        var rowNumber = 1;
        while (reader.Read())
        {
            for (var i = 0; i < cantonColumns.Count; i++)
            {
                if (reader.IsDBNull(i))
                {
                    continue;
                }

                var cell = worksheet.Cell(rowNumber, cantonColumns[i].Number);
                var value = reader.GetValue(i);
                if (value is long longValue)
                {
                    cell.Value = longValue;
                }
                else if (value is double doubleValue)
                {
                    cell.Value = doubleValue;
                }
                else
                {
                    cell.Value = value?.ToString() ?? string.Empty;
                }
            }

            rowNumber++;
        }

        logger.LogInformation("Wrote {RowCount} row(s) into canton raw data sheet '{Sheet}'.", rowNumber - 1, cantonSheetName);
    }

    private static bool TableExists(SqliteConnection connection, string tableName)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = @name";
        command.Parameters.AddWithValue("@name", tableName);
        return (long)(command.ExecuteScalar() ?? 0L) > 0;
    }

    // A pivot as in the canton's statistics templates: at A3, the count of tid and its share of the total, with subtotals.
    private static PivotConfig TemplatePivot(string sheet, params string[] rows)
        => new(sheet, "A3", rows, [], TidCountAndShare, Subtotals: true, []);

    // Validates a data sheet's configuration and resolves its pivots' fields to the sheet's headers.
    private static SheetDefinition BuildSheet(DataSheetConfig config, string source, IReadOnlyList<PivotConfig> pivots)
    {
        if (config is null || string.IsNullOrWhiteSpace(config.Sheet))
            throw new ArgumentException("Every data sheet of the error overview needs a sheet name.");

        if (config.Columns is null || config.Columns.Count == 0)
            throw new ArgumentException($"Sheet '{config.Sheet}' needs at least one column.");

        var columns = new List<ConfiguredColumn>();
        foreach (var (attribute, column) in config.Columns)
        {
            if (column is null || string.IsNullOrWhiteSpace(column.Header))
                throw new ArgumentException($"Column '{attribute}' of sheet '{config.Sheet}' needs a header.");

            columns.Add(new ConfiguredColumn(attribute, new SheetColumn(ColumnNumber(config.Sheet, attribute, column.Column), column.Header)));
        }

        EnsureUniqueColumns(config.Sheet, columns.Select(column => (column.Attribute, column.Column.Number)));

        var resolvedPivots = pivots.Select(pivot => BuildPivot(pivot, config)).ToList();
        return new SheetDefinition(config.Sheet, source, columns.OrderBy(column => column.Column.Number).ToList(), resolvedPivots);
    }

    private static PivotSheet BuildPivot(PivotConfig pivot, DataSheetConfig sheet)
    {
        string Header(string field) => sheet.Columns.TryGetValue(field, out var column)
            ? column.Header
            : throw new ArgumentException($"Pivot '{pivot.Sheet}' needs the column '{field}' on sheet '{sheet.Sheet}'.");

        return new PivotSheet(
            pivot.Sheet,
            pivot.Location,
            RowFields: pivot.Rows.Select(Header).ToList(),
            Subtotals: pivot.Subtotals,
            Filters: pivot.Filters.Select(Header).ToList(),
            Values: pivot.Values.Select(value => new PivotValue(Header(value.Field), value.Name, value.Share)).ToList(),
            ColumnWidths: pivot.ColumnWidths);
    }

    // Validates the canton matrix's configuration like a data sheet's, but without headers.
    private static IReadOnlyList<(string Attribute, int Number)> BuildCantonColumns(CantonMatrixConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.Sheet))
            throw new ArgumentException("The canton error matrix needs the name of its raw data sheet.");

        if (config.Columns is null || config.Columns.Count == 0)
            throw new ArgumentException($"Sheet '{config.Sheet}' needs at least one column.");

        IReadOnlyList<(string Attribute, int Number)> columns = [.. config.Columns.Select(column => (column.Key, ColumnNumber(config.Sheet, column.Key, column.Value)))];
        EnsureUniqueColumns(config.Sheet, columns);
        return columns;
    }

    // The attribute keys are quoted into SELECTs, so they stay plain column names.
    private static int ColumnNumber(string sheet, string attribute, string? letter)
    {
        if (!PlainColumnName().IsMatch(attribute))
            throw new ArgumentException($"'{attribute}' on sheet '{sheet}' is not a plain column name.");

        if (string.IsNullOrWhiteSpace(letter) || !XLHelper.IsValidColumn(letter))
            throw new ArgumentException($"'{letter}' of column '{attribute}' on sheet '{sheet}' is not an Excel column letter.");

        return XLHelper.GetColumnNumberFromLetter(letter);
    }

    private static void EnsureUniqueColumns(string sheet, IEnumerable<(string Attribute, int Number)> columns)
    {
        // Two attributes in one column would overwrite each other.
        var clash = columns.GroupBy(column => column.Number).FirstOrDefault(group => group.Count() > 1);
        if (clash is not null)
            throw new ArgumentException($"The columns '{string.Join("', '", clash.Select(column => column.Attribute))}' of sheet '{sheet}' share column {XLHelper.GetColumnLetterFromNumber(clash.Key)}.");
    }

    private static void EnsureUniqueSheetNames(IEnumerable<SheetDefinition> sheets)
    {
        // Excel compares sheet names case-insensitively.
        var duplicate = sheets
            .SelectMany(sheet => sheet.Pivots.Select(pivot => pivot.SheetName).Prepend(sheet.Name))
            .GroupBy(name => name, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicate is not null)
            throw new ArgumentException($"The sheet names '{string.Join("', '", duplicate)}' of the error overview clash.");
    }

    // Writes the header row and the rows below it, with an autofilter over them and the header row frozen. Returns the
    // written range, the source of the sheet's pivot tables.
    private static IXLRange AddDataSheet(XLWorkbook workbook, DataSheet sheet)
    {
        var worksheet = workbook.AddWorksheet(sheet.Name);
        foreach (var column in sheet.Columns)
        {
            worksheet.Cell(1, column.Number).Value = column.Header;
        }

        for (var row = 0; row < sheet.Rows.Count; row++)
        {
            for (var column = 0; column < sheet.Columns.Count; column++)
            {
                worksheet.Cell(row + 2, sheet.Columns[column].Number).Value = sheet.Rows[row][column] switch
                {
                    long integer => integer,
                    double real => real,
                    string text => text,
                    _ => Blank.Value,
                };
            }
        }

        var data = worksheet.Range(1, sheet.Columns.Min(column => column.Number), sheet.Rows.Count + 1, sheet.Columns.Max(column => column.Number));
        data.SetAutoFilter();
        worksheet.SheetView.FreezeRows(1);
        return data;
    }

    private static void AddPivotSheet(XLWorkbook workbook, PivotSheet pivot, IXLRange data, int position)
    {
        var worksheet = workbook.Worksheets.Add(pivot.SheetName, position);
        var pivotTable = worksheet.PivotTables.Add(pivot.SheetName, worksheet.Cell(pivot.Location), data);

        foreach (var field in pivot.RowFields)
        {
            var rowLabel = pivotTable.RowLabels.Add(field);
            if (pivot.Subtotals)
            {
                rowLabel.AddSubtotal(XLSubtotalFunction.Automatic);
            }
        }

        foreach (var field in pivot.Filters)
        {
            pivotTable.ReportFilters.Add(field);
        }

        foreach (var value in pivot.Values)
        {
            var pivotValue = pivotTable.Values.Add(value.Field, value.Name).SetSummaryFormula(XLPivotSummary.Count);
            if (value.ShareOfTotal)
            {
                pivotValue.ShowAsPercentageOfTotal().NumberFormat.SetNumberFormatId(PercentNumberFormatId);
            }
        }

        pivotTable.SetLayout(XLPivotLayout.Compact);

        for (var column = 0; column < pivot.ColumnWidths.Count; column++)
        {
            worksheet.Column(column + 1).Width = pivot.ColumnWidths[column];
        }
    }

    private static SqliteConnection OpenGeoPackage(string path)
    {
        var connection = new SqliteConnection($"Data Source={path};Mode=ReadOnly;Pooling=false");
        connection.Open();
        return connection;
    }

    [GeneratedRegex("^[A-Za-z_][A-Za-z0-9_]*$")]
    private static partial Regex PlainColumnName();
}
