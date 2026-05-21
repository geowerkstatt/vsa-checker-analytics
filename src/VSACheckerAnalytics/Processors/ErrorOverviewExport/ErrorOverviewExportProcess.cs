using ClosedXML.Excel;
using Geopilot.PipelineCore.Pipeline;
using Geopilot.PipelineCore.Pipeline.Process;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Diagnostics.CodeAnalysis;

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

    private sealed record ExcelColumn(string Name, string Attribute);

    private sealed record ExcelSheet(string TableName, string Name, Dictionary<string, ExcelColumn> ColumnsMapping);

    private readonly ExcelSheet errorDataSheet;
    private readonly ExcelSheet errorObjectSheet;
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
    /// <param name="pipelineFileManager">Pipeline file manager for output file allocation.</param>
    /// <param name="logger">Logger.</param>
    public ErrorOverviewExportProcess(
        string errorDataSheet,
        IDictionary<string, string> errorDataAttributeMapping,
        IDictionary<string, string> errorDataColumnMapping,
        string errorObjectSheet,
        IDictionary<string, string> errorObjectAttributeMapping,
        IDictionary<string, string> errorObjectColumnMapping,
        IPipelineFileManager pipelineFileManager,
        ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(errorDataAttributeMapping);
        ArgumentNullException.ThrowIfNull(errorDataColumnMapping);
        ArgumentNullException.ThrowIfNull(errorObjectAttributeMapping);
        ArgumentNullException.ThrowIfNull(errorObjectColumnMapping);

        this.pipelineFileManager = pipelineFileManager ?? throw new ArgumentNullException(nameof(pipelineFileManager));
        this.logger = logger ?? NullLogger.Instance;

        this.errorDataSheet = BuildSheetConfig(ErrorDataTable, errorDataSheet, errorDataAttributeMapping, errorDataColumnMapping);
        this.errorObjectSheet = BuildSheetConfig(ErrorObjectTable, errorObjectSheet, errorObjectAttributeMapping, errorObjectColumnMapping);
    }

    /// <summary>
    /// Reads <c>ca_error_data</c> and <c>ca_error_object</c> from the
    /// <paramref name="geopackage"/> and exports them to an Excel workbook.
    /// </summary>
    /// <param name="geopackage">GeoPackage containing the materialized error tables.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A dictionary with the exported Excel file under <c>errorOverview</c>.</returns>
    [PipelineProcessRun]
    public async Task<Dictionary<string, object?>> RunAsync(IPipelineFile geopackage, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(geopackage);

        string gpkgPath;
        await using (var stream = geopackage.OpenReadFileStream())
        {
            gpkgPath = stream.Name;
        }

        using var connection = OpenGeoPackage(gpkgPath);
        using var workbook = new XLWorkbook();

        ExportSheet(workbook, errorDataSheet, connection);
        cancellationToken.ThrowIfCancellationRequested();
        ExportSheet(workbook, errorObjectSheet, connection);

        var outputFile = pipelineFileManager.GeneratePipelineFile("errorOverview", "xlsx");
        await using var writeStream = outputFile.OpenWriteFileStream();
        workbook.SaveAs(writeStream);

        logger.LogInformation("Exported error overview to <{FileName}>.", outputFile.OriginalFileName);

        return new Dictionary<string, object?>
        {
            { "error_overview", outputFile },
        };
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

    [SuppressMessage("Security", "CA2100", Justification = "Column and table names are internal pipeline constants, not user input.")]
    private void ExportSheet(XLWorkbook workbook, ExcelSheet sheetConfig, SqliteConnection connection)
    {
        var worksheet = workbook.AddWorksheet(sheetConfig.Name);

        var orderedColumns = sheetConfig.ColumnsMapping
            .OrderBy(kvp => kvp.Key, StringComparer.Ordinal)
            .ToList();

        foreach (var (columnLetter, column) in orderedColumns)
        {
            worksheet.Cell($"{columnLetter}1").Value = column.Name;
        }

        var attributes = orderedColumns.Select(c => c.Value.Attribute).ToArray();
        var columnLetters = orderedColumns.Select(c => c.Key).ToArray();
        var columnList = string.Join(", ", attributes.Select(a => $"\"{a}\""));

        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT {columnList} FROM \"{sheetConfig.TableName}\"";
        using var reader = command.ExecuteReader();

        var rowNumber = 2;
        while (reader.Read())
        {
            for (var i = 0; i < attributes.Length; i++)
            {
                if (!reader.IsDBNull(i))
                {
                    var cell = worksheet.Cell($"{columnLetters[i]}{rowNumber}");
                    var value = reader.GetValue(i);
                    if (value is long l)
                    {
                        cell.Value = l;
                    }
                    else if (value is double d)
                    {
                        cell.Value = d;
                    }
                    else
                    {
                        cell.Value = value?.ToString() ?? string.Empty;
                    }
                }
            }

            rowNumber++;
        }

        worksheet.RangeUsed()?.SetAutoFilter();
        worksheet.SheetView.FreezeRows(1);

        logger.LogDebug("Exported {RowCount} rows to sheet '{SheetName}'.", rowNumber - 2, sheetConfig.Name);
    }

    private static SqliteConnection OpenGeoPackage(string path)
    {
        var connection = new SqliteConnection($"Data Source={path};Mode=ReadOnly;Pooling=false");
        connection.Open();
        return connection;
    }
}
