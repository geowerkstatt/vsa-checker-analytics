using ClosedXML.Excel;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace VsaCheckerAnalytics.Processors.GeopackageGeneration;

/// <summary>
/// Imports the error matrix from an Excel file into a SQLite table.
/// </summary>
internal sealed class ErrorMatrixImporter
{
    private readonly SqliteConnection connection;
    private readonly ILogger logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorMatrixImporter"/> class.
    /// </summary>
    /// <param name="connection">An open SQLite connection.</param>
    /// <param name="logger">Logger for diagnostic messages.</param>
    internal ErrorMatrixImporter(SqliteConnection connection, ILogger logger)
    {
        this.connection = connection;
        this.logger = logger;
    }

    /// <summary>
    /// Reads the first worksheet of the Excel file from <paramref name="excelStream"/> and
    /// imports it into a table named <paramref name="tableName"/>. The table schema is defined by
    /// <paramref name="columns"/>; the first row with data is skipped but validated against
    /// the expected columns. All values are stored as TEXT.
    /// </summary>
    /// <param name="excelStream">Stream containing the XLSX data.</param>
    /// <param name="tableName">Name of the table to create.</param>
    /// <param name="columns">Fixed column names defining the table schema.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <param name="indexColumns">
    /// Optional list of columns on which a composite index is created after the rows
    /// have been inserted. Pass <c>null</c> or an empty array to skip index creation.
    /// </param>
    internal Task ImportAsync(
        Stream excelStream,
        string tableName,
        string[] columns,
        CancellationToken cancellationToken,
        string[]? indexColumns = null)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var workbook = new XLWorkbook(excelStream);
        var worksheet = workbook.Worksheets.First();
        var usedRange = worksheet.RangeUsed();

        if (usedRange == null)
        {
            return Task.CompletedTask;
        }

        var firstRow = usedRange.FirstRow();
        var headerColumns = firstRow.CellsUsed()
            .Select(c => c.GetString().Trim())
            .ToArray();

        if (!headerColumns.SequenceEqual(columns))
        {
            logger.LogWarning(
                "Error matrix header does not match expected columns. Expected: [{Expected}], Actual: [{Actual}].",
                string.Join(", ", columns),
                string.Join(", ", headerColumns));
        }

        CreateTable(tableName, columns);
        InsertRows(usedRange, tableName, columns);

        if (indexColumns is { Length: > 0 })
        {
            CreateIndex(tableName, indexColumns);
        }

        return Task.CompletedTask;
    }

    [SuppressMessage("Security", "CA2100", Justification = "Table and column names are application-defined constants, not user input.")]
    private void CreateIndex(string tableName, string[] indexColumns)
    {
        var indexName = $"ix_{tableName}_{string.Join("_", indexColumns).ToLowerInvariant()}";
        var quotedIndexColumns = string.Join(", ", indexColumns.Select(c => $"\"{c}\""));

        using var command = connection.CreateCommand();
        command.CommandText = $"CREATE INDEX IF NOT EXISTS \"{indexName}\" ON \"{tableName}\" ({quotedIndexColumns})";
        command.ExecuteNonQuery();
    }

    [SuppressMessage("Security", "CA2100", Justification = "Column names are application-defined constants, not user input.")]
    private void CreateTable(string tableName, string[] columns)
    {
        var columnDefs = string.Join(", ", columns.Select(c => $"\"{c}\" TEXT"));
        using var command = connection.CreateCommand();
        command.CommandText = $"CREATE TABLE IF NOT EXISTS \"{tableName}\" (\"t_id\" INTEGER PRIMARY KEY AUTOINCREMENT, {columnDefs})";
        command.ExecuteNonQuery();
    }

    [SuppressMessage("Security", "CA2100", Justification = "Column names are application-defined constants, not user input.")]
    private void InsertRows(IXLRange usedRange, string tableName, string[] columns)
    {
        using var transaction = connection.BeginTransaction();
        using var command = connection.CreateCommand();

        var parameterNames = columns.Select((_, i) => $"@p{i}").ToArray();
        var quotedColumns = string.Join(", ", columns.Select(c => $"\"{c}\""));
        command.CommandText = $"INSERT INTO \"{tableName}\" ({quotedColumns}) VALUES ({string.Join(", ", parameterNames)})";

        var parameters = new SqliteParameter[columns.Length];
        for (var i = 0; i < columns.Length; i++)
        {
            parameters[i] = command.Parameters.Add(parameterNames[i], SqliteType.Text);
        }

        command.Prepare();

        var dataRows = usedRange.RowsUsed().Skip(1);
        foreach (var row in dataRows)
        {
            for (var i = 0; i < columns.Length; i++)
            {
                var value = row.Cell(i + 1).GetString();
                parameters[i].Value = string.IsNullOrEmpty(value) ? DBNull.Value : value;
            }

            command.ExecuteNonQuery();
        }

        transaction.Commit();
    }
}
