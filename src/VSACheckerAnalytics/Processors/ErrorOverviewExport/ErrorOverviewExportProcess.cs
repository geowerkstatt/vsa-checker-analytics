using ClosedXML.Excel;
using Geopilot.PipelineCore.Pipeline;
using Geopilot.PipelineCore.Pipeline.Process;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace VsaCheckerAnalytics.Processors.ErrorOverviewExport;

/// <summary>
/// Exports error overview data from a GeoPackage's <c>ca_error_data</c> and
/// <c>ca_error_object</c> tables into an Excel workbook with configurable
/// sheet names and column mappings.
/// </summary>
public sealed class ErrorOverviewExportProcess
{
    private const string ErrorDataTable = "ca_error_data";
    private const string ErrorObjectTable = "ca_error_object";
    private const string StatisticsTable = "ca_statistics_attribute";
    private const string StatisticsOrderColumn = "sortierung";
    private const string CantonMatrixFileName = "kantonale_fehlermatrix";

    // Excel's built-in number format "0.00%".
    private const int PercentNumberFormatId = 10;

    private static readonly LocalizedText ErrorOverviewStatusMessageFormat = new Dictionary<string, string>
    {
        { "de", "Fehler-Übersicht erstellt: {0} Fehler exportiert." },
        { "fr", "Aperçu des erreurs créé : {0} erreurs exportées." },
        { "it", "Panoramica degli errori creata: {0} errori esportati." },
        { "en", "Error overview created: {0} errors exported." },
    };

    private sealed record ExcelColumn(string Name, string Attribute);

    private sealed record ExcelSheet(string TableName, string Name, Dictionary<string, ExcelColumn> ColumnsMapping);

    private readonly ExcelSheet errorDataSheet;
    private readonly ExcelSheet errorObjectSheet;
    private readonly IReadOnlyList<PivotSheet> overviews;
    private readonly string cantonSheetName;
    private readonly IReadOnlyDictionary<string, string> cantonColumns;
    private readonly IPipelineFileManager pipelineFileManager;
    private readonly ILogger logger;

    /// <summary>
    /// Initializes a new <see cref="ErrorOverviewExportProcess"/>.
    /// </summary>
    /// <param name="errorDataSheet">Sheet name for <c>ca_error_data</c>.</param>
    /// <param name="errorDataAttributeMapping">Maps attribute keys to Excel header display names for error data.</param>
    /// <param name="errorDataColumnMapping">Maps attribute keys to Excel column letters for error data.</param>
    /// <param name="errorObjectSheet">Sheet name for <c>ca_error_object</c>.</param>
    /// <param name="errorObjectAttributeMapping">Maps attribute keys to Excel header display names for error objects.</param>
    /// <param name="errorObjectColumnMapping">Maps attribute keys to Excel column letters for error objects.</param>
    /// <param name="overviewWkSheet">Sheet name for the WK pivot overview, or <c>null</c> to skip.</param>
    /// <param name="overviewGepSheet">Sheet name for the GEP pivot overview, or <c>null</c> to skip.</param>
    /// <param name="overviewRowFields">Attribute keys for additional pivot row fields (after the priority field).</param>
    /// <param name="overviewFilterFields">Attribute keys for pivot report filter fields.</param>
    /// <param name="overviewValueField">Attribute key for the pivot count value field.</param>
    /// <param name="overviewValueName">Display name for the pivot value column.</param>
    /// <param name="cantonErrorObjectSheet">Sheet name for the raw data export in the canton error Excel workbook.</param>
    /// <param name="cantonErrorColumnMapping">Maps attribute keys to canton Excel column letters for error objects.</param>
    /// <param name="pipelineFileManager">Pipeline file manager for output file allocation.</param>
    /// <param name="logger">Logger.</param>
    public ErrorOverviewExportProcess(
        string errorDataSheet,
        IDictionary<string, string> errorDataAttributeMapping,
        IDictionary<string, string> errorDataColumnMapping,
        string errorObjectSheet,
        IDictionary<string, string> errorObjectAttributeMapping,
        IDictionary<string, string> errorObjectColumnMapping,
        string? overviewWkSheet,
        string? overviewGepSheet,
        IList<string>? overviewRowFields,
        IList<string>? overviewFilterFields,
        string? overviewValueField,
        string? overviewValueName,
        string cantonErrorObjectSheet,
        IDictionary<string, string> cantonErrorColumnMapping,
        IPipelineFileManager pipelineFileManager,
        ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(errorDataAttributeMapping);
        ArgumentNullException.ThrowIfNull(errorDataColumnMapping);
        ArgumentNullException.ThrowIfNull(errorObjectAttributeMapping);
        ArgumentNullException.ThrowIfNull(errorObjectColumnMapping);
        ArgumentNullException.ThrowIfNull(cantonErrorObjectSheet);
        ArgumentNullException.ThrowIfNull(cantonErrorColumnMapping);

        this.pipelineFileManager = pipelineFileManager ?? throw new ArgumentNullException(nameof(pipelineFileManager));
        this.logger = logger ?? NullLogger.Instance;

        this.errorDataSheet = BuildSheetConfig(ErrorDataTable, errorDataSheet, errorDataAttributeMapping, errorDataColumnMapping);
        this.errorObjectSheet = BuildSheetConfig(ErrorObjectTable, errorObjectSheet, errorObjectAttributeMapping, errorObjectColumnMapping);

        this.cantonSheetName = cantonErrorObjectSheet;
        this.cantonColumns = new Dictionary<string, string>(cantonErrorColumnMapping, StringComparer.Ordinal);

        var overviews = new List<PivotSheet>();
        if (overviewWkSheet is not null || overviewGepSheet is not null)
        {
            ArgumentNullException.ThrowIfNull(overviewRowFields);
            ArgumentNullException.ThrowIfNull(overviewFilterFields);
            ArgumentNullException.ThrowIfNull(overviewValueField);
            ArgumentNullException.ThrowIfNull(overviewValueName);

            if (overviewWkSheet is not null)
            {
                overviews.Add(BuildPivotConfig(
                    overviewWkSheet,
                    "uc",
                    overviewRowFields,
                    overviewFilterFields,
                    overviewValueField,
                    overviewValueName,
                    errorDataAttributeMapping));
            }

            if (overviewGepSheet is not null)
            {
                overviews.Add(BuildPivotConfig(
                    overviewGepSheet,
                    "gsp",
                    overviewRowFields,
                    overviewFilterFields,
                    overviewValueField,
                    overviewValueName,
                    errorDataAttributeMapping));
            }
        }

        this.overviews = overviews;
    }

    /// <summary>
    /// Reads <c>ca_error_data</c> and <c>ca_error_object</c> from the
    /// <paramref name="geopackage"/> and exports them to an Excel workbook.
    /// When configured, adds pivot table overview sheets for WK and GEP priorities.
    /// Appends the statistics sheets for Teileinzugsgebiete, Knoten and Leitungen.
    /// </summary>
    /// <param name="geopackage">GeoPackage containing the materialized error tables.</param>
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
        var errorData = ReadSheet(connection, errorDataSheet, overviews);
        cancellationToken.ThrowIfCancellationRequested();
        var errorObjects = ReadSheet(connection, errorObjectSheet, []);
        var statistics = ObjectStatistics.Read(connection, logger);

        using var workbook = new XLWorkbook();
        var errorDataRange = AddDataSheet(workbook, errorData);
        AddDataSheet(workbook, errorObjects);
        foreach (var overview in errorData.Pivots)
        {
            // Unlike the pivot sheets of the statistics, the overviews follow both error sheets.
            AddPivotSheet(workbook, overview, errorDataRange, workbook.Worksheets.Count + 1);
        }

        foreach (var sheet in statistics)
        {
            var data = AddDataSheet(workbook, sheet);
            foreach (var pivot in sheet.Pivots)
            {
                // At the data sheet's position, so the pivot sheets precede their data as in the canton's templates.
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
            .Map(msg => string.Format(CultureInfo.InvariantCulture, msg, errorData.Rows.Count));

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

        var attributes = cantonColumns.Keys.ToArray();
        var columnList = string.Join(", ", attributes.Select(a => $"\"{a}\""));

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
            for (var i = 0; i < attributes.Length; i++)
            {
                if (reader.IsDBNull(i))
                {
                    continue;
                }

                var cell = worksheet.Cell($"{cantonColumns[attributes[i]]}{rowNumber}");
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

    private static ExcelSheet BuildSheetConfig(
        string tableName,
        string sheetName,
        IDictionary<string, string> attributeMapping,
        IDictionary<string, string> columnMapping)
    {
        var attributeKeys = attributeMapping.Keys.ToHashSet(StringComparer.Ordinal);
        var columnKeys = columnMapping.Keys.ToHashSet(StringComparer.Ordinal);

        if (!attributeKeys.SetEquals(columnKeys))
        {
            var onlyInAttribute = attributeKeys.Except(columnKeys).ToList();
            var onlyInColumn = columnKeys.Except(attributeKeys).ToList();

            var details = new List<string>();
            if (onlyInAttribute.Count > 0)
            {
                details.Add($"only in attributeMapping: [{string.Join(", ", onlyInAttribute)}]");
            }

            if (onlyInColumn.Count > 0)
            {
                details.Add($"only in columnMapping: [{string.Join(", ", onlyInColumn)}]");
            }

            throw new ArgumentException(
                $"Attribute and column mappings for sheet '{sheetName}' must have the same keys. Mismatched keys: {string.Join("; ", details)}");
        }

        var columns = new Dictionary<string, ExcelColumn>(StringComparer.Ordinal);
        foreach (var (attributeKey, columnLetter) in columnMapping)
        {
            columns[columnLetter] = new ExcelColumn(attributeMapping[attributeKey], attributeKey);
        }

        return new ExcelSheet(tableName, sheetName, columns);
    }

    private static PivotSheet BuildPivotConfig(
        string sheetName,
        string priorityAttributeKey,
        IList<string> rowFields,
        IList<string> filterFields,
        string valueField,
        string valueName,
        IDictionary<string, string> attributeMapping)
    {
        string ResolveDisplayName(string key)
        {
            if (!attributeMapping.TryGetValue(key, out var name))
            {
                throw new ArgumentException(
                    $"Overview attribute key '{key}' not found in error data attribute mapping for sheet '{sheetName}'.");
            }

            return name;
        }

        return new PivotSheet(
            sheetName,
            Location: "A1",
            RowFields: [ResolveDisplayName(priorityAttributeKey), .. rowFields.Select(ResolveDisplayName)],
            Subtotals: false,
            Filters: filterFields.Select(ResolveDisplayName).ToList(),
            Values: [new PivotValue(ResolveDisplayName(valueField), valueName, ShareOfTotal: false)],
            ColumnWidths: [105, 13]);
    }

    [SuppressMessage("Security", "CA2100", Justification = "Column and table names are internal pipeline constants, not user input.")]
    private DataSheet ReadSheet(SqliteConnection connection, ExcelSheet sheetConfig, IReadOnlyList<PivotSheet> pivots)
    {
        var orderedColumns = sheetConfig.ColumnsMapping
            .OrderBy(kvp => kvp.Key, StringComparer.Ordinal)
            .ToList();

        var columnList = string.Join(", ", orderedColumns.Select(c => $"\"{c.Value.Attribute}\""));

        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT {columnList} FROM \"{sheetConfig.TableName}\"";
        using var reader = command.ExecuteReader();

        var rows = new List<IReadOnlyList<object?>>();
        while (reader.Read())
        {
            var values = new object?[reader.FieldCount];
            for (var i = 0; i < values.Length; i++)
            {
                values[i] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            }

            rows.Add(values);
        }

        logger.LogDebug("Read {RowCount} rows for sheet '{SheetName}'.", rows.Count, sheetConfig.Name);

        var columns = orderedColumns.Select(c => new SheetColumn(XLHelper.GetColumnNumberFromLetter(c.Key), c.Value.Name)).ToArray();
        return new DataSheet(sheetConfig.Name, columns, rows, pivots);
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
}
