using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace VsaCheckerAnalytics.Process.GeopackageGeneration;

/// <summary>
/// Imports semicolon-delimited CSV data into SQLite tables.
/// </summary>
internal sealed class CsvImporter
{
    private const char Delimiter = ';';

    private readonly SqliteConnection connection;
    private readonly Encoding encoding;
    private readonly ILogger logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CsvImporter"/> class.
    /// </summary>
    /// <param name="connection">An open SQLite connection.</param>
    /// <param name="encoding">Character encoding of the CSV files.</param>
    /// <param name="logger">Logger for diagnostic messages.</param>
    internal CsvImporter(SqliteConnection connection, Encoding encoding, ILogger logger)
    {
        this.connection = connection;
        this.encoding = encoding;
        this.logger = logger;
    }

    /// <summary>
    /// Reads a semicolon-delimited CSV from <paramref name="csvStream"/> and imports it
    /// into a new table named <paramref name="tableName"/>. The table schema is defined by
    /// <paramref name="columns"/>; the first CSV line (header) is skipped but validated
    /// against the expected columns. All values are stored as TEXT.
    /// </summary>
    /// <param name="csvStream">Stream containing the CSV data.</param>
    /// <param name="tableName">Name of the table to create.</param>
    /// <param name="columns">Fixed column names defining the table schema.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <param name="indexColumns">
    /// Optional list of columns on which a composite index is created after the rows
    /// have been inserted. Pass <c>null</c> or an empty array to skip index creation.
    /// </param>
    internal async Task ImportAsync(
        Stream csvStream,
        string tableName,
        string[] columns,
        CancellationToken cancellationToken,
        string[]? indexColumns = null)
    {
        using var reader = new StreamReader(csvStream, encoding);

        var headerLine = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
        if (headerLine == null)
        {
            return;
        }

        var headerColumns = headerLine.Split(Delimiter).Select(c => c.Trim()).ToArray();
        if (!headerColumns.SequenceEqual(columns))
        {
            logger.LogWarning(
                "CSV header in '{TableName}' does not match expected columns. Expected: [{Expected}], Actual: [{Actual}].",
                tableName,
                string.Join(", ", columns),
                string.Join(", ", headerColumns));
        }

        CreateTable(tableName, columns);
        await InsertRowsAsync(reader, tableName, columns, cancellationToken).ConfigureAwait(false);

        if (indexColumns is { Length: > 0 })
        {
            CreateIndex(tableName, indexColumns);
        }
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

    [SuppressMessage("Security", "CA2100", Justification = "Table and column names are application-defined constants, not user input.")]
    private void CreateTable(string tableName, string[] columns)
    {
        var columnDefs = string.Join(", ", columns.Select(c => $"\"{c}\" TEXT"));
        using var command = connection.CreateCommand();
        command.CommandText = $"CREATE TABLE IF NOT EXISTS \"{tableName}\" (\"t_id\" INTEGER PRIMARY KEY AUTOINCREMENT, {columnDefs})";
        command.ExecuteNonQuery();
    }

    [SuppressMessage("Security", "CA2100", Justification = "Table and column names are application-defined constants, not user input.")]
    private async Task InsertRowsAsync(StreamReader reader, string tableName, string[] columns, CancellationToken cancellationToken)
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

        // Header is line 1; data starts on line 2.
        var lineNumber = 1;
        var skippedRows = 0;

        while (true)
        {
            var line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
            if (line == null)
            {
                break;
            }

            lineNumber++;

            if (string.IsNullOrEmpty(line))
            {
                continue;
            }

            var values = line.Split(Delimiter);
            if (values.Length != columns.Length)
            {
                logger.LogWarning(
                    "CSV row {LineNumber} in '{TableName}' has {ActualFields} field(s), expected {ExpectedFields}. Skipping row. Raw: {RawLine}",
                    lineNumber,
                    tableName,
                    values.Length,
                    columns.Length,
                    line.Length > 200 ? string.Concat(line.AsSpan(0, 200), "…") : line);
                skippedRows++;
                continue;
            }

            for (var i = 0; i < parameters.Length; i++)
            {
                parameters[i].Value = string.IsNullOrEmpty(values[i]) ? DBNull.Value : values[i];
            }

            command.ExecuteNonQuery();
        }

        transaction.Commit();

        if (skippedRows > 0)
        {
            logger.LogWarning(
                "CSV import for '{TableName}' skipped {SkippedCount} malformed row(s) due to field count mismatch.",
                tableName,
                skippedRows);
        }
    }
}
