using ClosedXML.Excel;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Globalization;
using VsaCheckerAnalytics.Process.GeopackageGeneration;

namespace VsaCheckerAnalytics.Process.GeopackageGeneration;

[TestClass]
public class ErrorMatrixImporterTest
{
    private const string TableName = "error_matrix";
    private static readonly string[] ExpectedColumns = ["ErrorId", "Model", "class_de", "class_fr"];

    [TestMethod]
    public async Task ImportAsync_CreatesTableWithCorrectColumns()
    {
        using var connection = CreateOpenConnection();
        var importer = new ErrorMatrixImporter(connection, NullLogger.Instance);
        using var stream = CreateExcelStream(
            ExpectedColumns,
            [["E001", "2020", "Klasse A", "Classe A"]]);

        await importer.ImportAsync(stream, TableName, ExpectedColumns, CancellationToken.None);

        var columns = GetColumnNames(connection, TableName);
        Assert.AreEqual("t_id", columns[0]);
        CollectionAssert.AreEqual(ExpectedColumns, columns.Skip(1).ToList());
    }

    [TestMethod]
    public async Task ImportAsync_InsertsAllRows()
    {
        using var connection = CreateOpenConnection();
        var importer = new ErrorMatrixImporter(connection, NullLogger.Instance);
        using var stream = CreateExcelStream(
            ExpectedColumns,
            [
                ["E001", "2020", "Klasse A", "Classe A"],
                ["E002", "2020", "Klasse B", "Classe B"],
                ["E003", "2020.1", "Klasse C", "Classe C"],
            ]);

        await importer.ImportAsync(stream, TableName, ExpectedColumns, CancellationToken.None);

        Assert.AreEqual(3, GetRowCount(connection, TableName));
    }

    [TestMethod]
    public async Task ImportAsync_EmptyCellsAreStoredAsNull()
    {
        using var connection = CreateOpenConnection();
        var importer = new ErrorMatrixImporter(connection, NullLogger.Instance);
        using var stream = CreateExcelStream(
            ExpectedColumns,
            [["E001", "2020", string.Empty, string.Empty]]);

        await importer.ImportAsync(stream, TableName, ExpectedColumns, CancellationToken.None);

        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT \"ErrorId\", \"Model\", \"class_de\", \"class_fr\" FROM \"{TableName}\"";
        using var reader = command.ExecuteReader();
        Assert.IsTrue(reader.Read());
        Assert.AreEqual("E001", reader.GetString(0));
        Assert.AreEqual("2020", reader.GetString(1));
        Assert.IsTrue(reader.IsDBNull(2));
        Assert.IsTrue(reader.IsDBNull(3));
    }

    [TestMethod]
    public async Task ImportAsync_HandlesNumericCells()
    {
        using var connection = CreateOpenConnection();
        var importer = new ErrorMatrixImporter(connection, NullLogger.Instance);
        using var workbook = new XLWorkbook();
        var worksheet = workbook.AddWorksheet();
        worksheet.Cell(1, 1).Value = "ErrorId";
        worksheet.Cell(1, 2).Value = "NumericCol";
        worksheet.Cell(2, 1).Value = "E001";
        worksheet.Cell(2, 2).Value = 42;

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        await importer.ImportAsync(stream, TableName, ["ErrorId", "NumericCol"], CancellationToken.None);

        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT \"NumericCol\" FROM \"{TableName}\"";
        var result = command.ExecuteScalar();
        Assert.AreEqual("42", result);
    }

    [TestMethod]
    public async Task ImportAsync_EmptyWorksheet_DoesNotCreateTable()
    {
        using var connection = CreateOpenConnection();
        var importer = new ErrorMatrixImporter(connection, NullLogger.Instance);
        using var workbook = new XLWorkbook();
        workbook.AddWorksheet();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        await importer.ImportAsync(stream, TableName, ExpectedColumns, CancellationToken.None);

        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{TableName}'";
        Assert.IsNull(command.ExecuteScalar());
    }

    [TestMethod]
    public async Task ImportAsync_MismatchedHeader_LogsWarningAndImports()
    {
        using var connection = CreateOpenConnection();
        var logger = new TestLogger();
        var importer = new ErrorMatrixImporter(connection, logger);
        using var stream = CreateExcelStream(
            ["WRONG", "HEADER"],
            [["E001", "2020"]]);

        await importer.ImportAsync(stream, TableName, ["ErrorId", "Model"], CancellationToken.None);

        Assert.AreEqual(1, GetRowCount(connection, TableName));
        Assert.AreEqual(LogLevel.Warning, logger.LastLogLevel);
    }

    private static SqliteConnection CreateOpenConnection()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        return connection;
    }

    private static MemoryStream CreateExcelStream(string[] headers, string[][] rows)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.AddWorksheet();

        for (var col = 0; col < headers.Length; col++)
        {
            worksheet.Cell(1, col + 1).Value = headers[col];
        }

        for (var row = 0; row < rows.Length; row++)
        {
            for (var col = 0; col < rows[row].Length; col++)
            {
                worksheet.Cell(row + 2, col + 1).Value = rows[row][col];
            }
        }

        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        return stream;
    }

    private static List<string> GetColumnNames(SqliteConnection connection, string tableName)
    {
        var columns = new List<string>();
        using var command = connection.CreateCommand();
#pragma warning disable CA2100
        command.CommandText = $"PRAGMA table_info(\"{tableName}\")";
#pragma warning restore CA2100
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            columns.Add(reader.GetString(1));
        }

        return columns;
    }

    private static int GetRowCount(SqliteConnection connection, string tableName)
    {
        using var command = connection.CreateCommand();
#pragma warning disable CA2100
        command.CommandText = $"SELECT COUNT(*) FROM \"{tableName}\"";
#pragma warning restore CA2100
        return Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
    }

    private sealed class TestLogger : ILogger
    {
        public LogLevel? LastLogLevel { get; private set; }

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            LastLogLevel = logLevel;
        }
    }
}
