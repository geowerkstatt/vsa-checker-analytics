using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using System.Globalization;
using System.Text;

namespace VsaCheckerAnalytics.Processors.GeopackageGeneration;

[TestClass]
public class ViewCreatorTest
{
    [TestMethod]
    public async Task CreateCheckerCsvUnionView_ThrowsWhenSchemasDiffer()
    {
        using var connection = CreateOpenConnection();
        var importer = new CsvImporter(connection, Encoding.UTF8, NullLogger.Instance);

        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes("ErrorId;Model;Class\nT1;2020;ClassX")))
        {
            await importer.ImportAsync(stream, "checker_csv_t", ["ErrorId", "Model", "Class"], CancellationToken.None);
        }

        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes("ErrorId;Model\nA1;2020")))
        {
            await importer.ImportAsync(stream, "checker_csv_a", ["ErrorId", "Model"], CancellationToken.None);
        }

        var viewCreator = new ViewCreator(connection);

        var ex = Assert.ThrowsExactly<InvalidOperationException>(() =>
            viewCreator.CreateCheckerCsvUnionView(
                "v_checker_csv_all",
                ["checker_csv_t", "checker_csv_a"],
                ["T", "A"]));

        Assert.Contains("Schema mismatch", ex.Message);
        Assert.Contains("checker_csv_a", ex.Message);
    }

    [TestMethod]
    public async Task CreateCheckerCsvUnionView_CombinesAllTablesWithSource()
    {
        using var connection = CreateOpenConnection();
        await SeedCsvTables(connection);

        var viewCreator = new ViewCreator(connection);
        viewCreator.CreateCheckerCsvUnionView(
            "v_checker_csv_all",
            ["checker_csv_t", "checker_csv_a", "checker_csv_fp"],
            ["T", "A", "FP"]);

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT \"ErrorId\", source FROM \"v_checker_csv_all\" ORDER BY \"ErrorId\"";
        using var reader = command.ExecuteReader();

        var rows = new List<(string ErrorId, string Source)>();
        while (reader.Read())
        {
            rows.Add((reader.GetString(0), reader.GetString(1)));
        }

        Assert.HasCount(3, rows);
        Assert.AreEqual("T", rows.First(r => r.ErrorId == "T1").Source);
        Assert.AreEqual("A", rows.First(r => r.ErrorId == "A1").Source);
        Assert.AreEqual("FP", rows.First(r => r.ErrorId == "FP1").Source);
    }

    [TestMethod]
    public async Task CreateCheckerErrorsView_JoinsOnCidModelAndClassDe_WhenLanguageDE()
    {
        using var connection = CreateOpenConnection();
        await SeedCsvTables(connection);
        SeedErrorMatrix(connection);
        CreateUnionView(connection);

        var viewCreator = new ViewCreator(connection);
        viewCreator.CreateCheckerErrorsView("v_checker_errors", "v_checker_csv_all", "error_matrix", "DE");

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT \"severity\" FROM \"v_checker_errors\" WHERE \"ErrorId\" = 'T1' AND \"Model\" = '2020'";
        Assert.AreEqual("high", command.ExecuteScalar());
    }

    [TestMethod]
    public async Task CreateCheckerErrorsView_JoinsOnCidModelAndClassFr_WhenLanguageFR()
    {
        using var connection = CreateOpenConnection();
        await SeedCsvTables(connection);
        SeedErrorMatrix(connection);
        CreateUnionView(connection);

        var viewCreator = new ViewCreator(connection);
        viewCreator.CreateCheckerErrorsView("v_checker_errors", "v_checker_csv_all", "error_matrix", "FR");

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT \"severity\" FROM \"v_checker_errors\" WHERE \"ErrorId\" = 'T1' AND \"Model\" = '2020'";
        Assert.AreEqual("high", command.ExecuteScalar());
    }

    [TestMethod]
    public async Task CreateCheckerErrorsView_ExcludesUnmatchedCsvRows()
    {
        using var connection = CreateOpenConnection();
        await SeedCsvTables(connection);
        SeedErrorMatrix(connection);
        CreateUnionView(connection);

        var viewCreator = new ViewCreator(connection);
        viewCreator.CreateCheckerErrorsView("v_checker_errors", "v_checker_csv_all", "error_matrix", "DE");

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM \"v_checker_errors\"";
        var totalRows = Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
        Assert.AreEqual(2, totalRows);

        command.CommandText = "SELECT COUNT(*) FROM \"v_checker_errors\" WHERE \"ErrorId\" = 'FP1'";
        Assert.AreEqual(0, Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture));
    }

    [TestMethod]
    public async Task CreateCheckerOrphansView_ReturnsUnmatchedRows()
    {
        using var connection = CreateOpenConnection();
        await SeedCsvTables(connection);
        SeedErrorMatrix(connection);
        CreateUnionView(connection);

        var viewCreator = new ViewCreator(connection);
        viewCreator.CreateCheckerErrorsView("v_checker_errors", "v_checker_csv_all", "error_matrix", "DE");
        viewCreator.CreateCheckerOrphansView("v_checker_orphans", "v_checker_csv_all", "v_checker_errors");

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM \"v_checker_orphans\"";
        Assert.AreEqual(1, Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture));

        command.CommandText = "SELECT \"ErrorId\" FROM \"v_checker_orphans\"";
        Assert.AreEqual("FP1", command.ExecuteScalar());
    }

    [TestMethod]
    public async Task CreateCheckerErrorsView_ExcludesJoinKeysAndUnusedClassFromErrorMatrix()
    {
        using var connection = CreateOpenConnection();
        await SeedCsvTables(connection);
        SeedErrorMatrix(connection);
        CreateUnionView(connection);

        var viewCreator = new ViewCreator(connection);
        viewCreator.CreateCheckerErrorsView("v_checker_errors", "v_checker_csv_all", "error_matrix", "DE");

        var viewColumns = GetViewColumnNames(connection, "v_checker_errors");

        Assert.Contains("severity", viewColumns);
        Assert.DoesNotContain("cid", viewColumns);
        Assert.DoesNotContain("model", viewColumns);
        Assert.DoesNotContain("class_de", viewColumns);
        Assert.DoesNotContain("class_fr", viewColumns);
    }

    private static SqliteConnection CreateOpenConnection()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        return connection;
    }

    private static async Task SeedCsvTables(SqliteConnection connection)
    {
        var importer = new CsvImporter(connection, Encoding.UTF8, NullLogger.Instance);
        var tables = new[] { ("checker_csv_t", "T1", "ClassX"), ("checker_csv_a", "A1", "ClassY"), ("checker_csv_fp", "FP1", "ClassZ") };
        foreach (var (tableName, errorId, cls) in tables)
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes($"ErrorId;Model;Class\n{errorId};2020;{cls}"));
            await importer.ImportAsync(stream, tableName, ["ErrorId", "Model", "Class"], CancellationToken.None);
        }
    }

    private static void SeedErrorMatrix(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE "error_matrix" ("t_id" INTEGER PRIMARY KEY AUTOINCREMENT, "cid" TEXT, "model" TEXT, "class_de" TEXT, "class_fr" TEXT, "severity" TEXT);
            INSERT INTO "error_matrix" ("cid", "model", "class_de", "class_fr", "severity") VALUES ('T1', '2020', 'ClassX', 'ClassX', 'high');
            INSERT INTO "error_matrix" ("cid", "model", "class_de", "class_fr", "severity") VALUES ('A1', '2020', 'ClassY', 'ClassY', 'low');
            """;
        command.ExecuteNonQuery();
    }

    private static void CreateUnionView(SqliteConnection connection)
    {
        var viewCreator = new ViewCreator(connection);
        viewCreator.CreateCheckerCsvUnionView(
            "v_checker_csv_all",
            ["checker_csv_t", "checker_csv_a", "checker_csv_fp"],
            ["T", "A", "FP"]);
    }

    private static List<string> GetViewColumnNames(SqliteConnection connection, string viewName)
    {
        var columns = new List<string>();
        using var command = connection.CreateCommand();
#pragma warning disable CA2100
        command.CommandText = $"PRAGMA table_info(\"{viewName}\")";
#pragma warning restore CA2100
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            columns.Add(reader.GetString(1));
        }

        return columns;
    }
}
