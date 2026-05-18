using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Globalization;
using System.Text;
using VsaCheckerAnalytics.Process.GeopackageGeneration;

namespace VsaCheckerAnalytics.Process.GeopackageGeneration;

[TestClass]
public class CsvImporterTest
{
    private static readonly string[] TwoColumns = ["CID", "model"];
    private static readonly string[] ThreeColumns = ["CID", "model", "description"];

    [TestMethod]
    public async Task ImportAsync_CreatesTableWithCorrectColumns()
    {
        using var connection = CreateOpenConnection();
        var importer = new CsvImporter(connection, Encoding.UTF8, NullLogger.Instance);
        using var stream = CreateCsvStream("CID;model;description", "1;2020;test");

        await importer.ImportAsync(stream, "test_table", ThreeColumns, CancellationToken.None);

        var columns = GetColumnNames(connection, "test_table");
        Assert.AreEqual("t_id", columns[0]);
        CollectionAssert.AreEqual(ThreeColumns, columns.Skip(1).ToList());
    }

    [TestMethod]
    public async Task ImportAsync_InsertsAllRows()
    {
        using var connection = CreateOpenConnection();
        var importer = new CsvImporter(connection, Encoding.UTF8, NullLogger.Instance);
        using var stream = CreateCsvStream(
            "CID;model",
            "1;2020",
            "2;2020",
            "3;2020.1");

        await importer.ImportAsync(stream, "test_table", TwoColumns, CancellationToken.None);

        Assert.AreEqual(3, GetRowCount(connection, "test_table"));
    }

    [TestMethod]
    public async Task ImportAsync_EmptyValuesAreStoredAsNull()
    {
        using var connection = CreateOpenConnection();
        var importer = new CsvImporter(connection, Encoding.UTF8, NullLogger.Instance);
        using var stream = CreateCsvStream("CID;model;note", "1;;");

        await importer.ImportAsync(stream, "test_table", ["CID", "model", "note"], CancellationToken.None);

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT \"CID\", \"model\", \"note\" FROM \"test_table\"";
        using var reader = command.ExecuteReader();
        Assert.IsTrue(reader.Read());
        Assert.AreEqual("1", reader.GetString(0));
        Assert.IsTrue(reader.IsDBNull(1));
        Assert.IsTrue(reader.IsDBNull(2));
    }

    [TestMethod]
    public async Task ImportAsync_HeaderOnly_CreatesEmptyTable()
    {
        using var connection = CreateOpenConnection();
        var importer = new CsvImporter(connection, Encoding.UTF8, NullLogger.Instance);
        using var stream = CreateCsvStream("CID;model");

        await importer.ImportAsync(stream, "test_table", TwoColumns, CancellationToken.None);

        var columns = GetColumnNames(connection, "test_table");
        Assert.HasCount(3, columns);
        Assert.AreEqual(0, GetRowCount(connection, "test_table"));
    }

    [TestMethod]
    public async Task ImportAsync_SemicolonDelimiter()
    {
        using var connection = CreateOpenConnection();
        var importer = new CsvImporter(connection, Encoding.UTF8, NullLogger.Instance);
        using var stream = CreateCsvStream("a;b;c", "val1;val2;val3");

        await importer.ImportAsync(stream, "test_table", ["a", "b", "c"], CancellationToken.None);

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT \"a\", \"b\", \"c\" FROM \"test_table\"";
        using var reader = command.ExecuteReader();
        Assert.IsTrue(reader.Read());
        Assert.AreEqual("val1", reader.GetString(0));
        Assert.AreEqual("val2", reader.GetString(1));
        Assert.AreEqual("val3", reader.GetString(2));
    }

    [TestMethod]
    public async Task ImportAsync_EmptyStream_DoesNotCreateTable()
    {
        using var connection = CreateOpenConnection();
        var importer = new CsvImporter(connection, Encoding.UTF8, NullLogger.Instance);
        using var stream = new MemoryStream();

        await importer.ImportAsync(stream, "test_table", TwoColumns, CancellationToken.None);

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='test_table'";
        Assert.IsNull(command.ExecuteScalar());
    }

    [TestMethod]
    public async Task ImportAsync_MismatchedHeader_LogsWarningAndImports()
    {
        using var connection = CreateOpenConnection();
        var logger = new TestLogger();
        var importer = new CsvImporter(connection, Encoding.UTF8, logger);
        using var stream = CreateCsvStream("WRONG;HEADER", "1;2020");

        await importer.ImportAsync(stream, "test_table", TwoColumns, CancellationToken.None);

        Assert.AreEqual(1, GetRowCount(connection, "test_table"));
        Assert.AreEqual(LogLevel.Warning, logger.LastLogLevel);
    }

    [TestMethod]
    public async Task ImportAsync_FieldCountMismatch_LogsWarningAndSkipsRow()
    {
        using var connection = CreateOpenConnection();
        var logger = new TestLogger();
        var importer = new CsvImporter(connection, Encoding.UTF8, logger);
        using var stream = CreateCsvStream(
            "CID;model",
            "1;2020",
            "2;2020;extra",       // too many fields → skipped
            "3",                   // too few fields → skipped
            "4;2020");

        await importer.ImportAsync(stream, "test_table", TwoColumns, CancellationToken.None);

        Assert.AreEqual(2, GetRowCount(connection, "test_table"));
        Assert.AreEqual(LogLevel.Warning, logger.LastLogLevel);
        Assert.IsGreaterThanOrEqualTo(3, logger.WarningCount); // 2 row warnings + 1 summary
    }

    [TestMethod]
    public async Task ImportAsync_BlankLines_AreSilentlySkipped()
    {
        using var connection = CreateOpenConnection();
        var logger = new TestLogger();
        var importer = new CsvImporter(connection, Encoding.UTF8, logger);
        using var stream = CreateCsvStream(
            "CID;model",
            "1;2020",
            "",
            "2;2020",
            "");

        await importer.ImportAsync(stream, "test_table", TwoColumns, CancellationToken.None);

        Assert.AreEqual(2, GetRowCount(connection, "test_table"));
        Assert.AreEqual(0, logger.WarningCount);
    }

    [TestMethod]
    public async Task ImportAsync_MultipleTablesOnSameConnection()
    {
        using var connection = CreateOpenConnection();
        var importer = new CsvImporter(connection, Encoding.UTF8, NullLogger.Instance);

        using var streamT = CreateCsvStream("CID;model", "T1;2020");
        using var streamA = CreateCsvStream("CID;model", "A1;2020");
        using var streamFp = CreateCsvStream("CID;model", "FP1;2020");

        await importer.ImportAsync(streamT, "checker_csv_t", TwoColumns, CancellationToken.None);
        await importer.ImportAsync(streamA, "checker_csv_a", TwoColumns, CancellationToken.None);
        await importer.ImportAsync(streamFp, "checker_csv_fp", TwoColumns, CancellationToken.None);

        Assert.AreEqual(1, GetRowCount(connection, "checker_csv_t"));
        Assert.AreEqual(1, GetRowCount(connection, "checker_csv_a"));
        Assert.AreEqual(1, GetRowCount(connection, "checker_csv_fp"));
    }

    private static SqliteConnection CreateOpenConnection()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        return connection;
    }

    private static MemoryStream CreateCsvStream(params string[] lines)
    {
        var content = string.Join("\n", lines);
        return new MemoryStream(Encoding.UTF8.GetBytes(content));
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

        public int WarningCount { get; private set; }

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            LastLogLevel = logLevel;
            if (logLevel == LogLevel.Warning)
            {
                WarningCount++;
            }
        }
    }
}
