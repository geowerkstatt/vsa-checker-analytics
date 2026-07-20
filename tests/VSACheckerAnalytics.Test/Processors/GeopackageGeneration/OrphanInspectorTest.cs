using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using System.Globalization;
using System.Text;
using VsaCheckerAnalytics.TestHelpers;

namespace VsaCheckerAnalytics.Processors.GeopackageGeneration;

[TestClass]
public class OrphanInspectorTest
{
    [TestMethod]
    public async Task MaterializeOrphans_WithUnmatchedRows_CreatesTableWithOrphans()
    {
        using var connection = CreateOpenConnection();
        await SeedCsvTables(connection);
        SeedErrorMatrix(connection);
        CreateClassifiedView(connection);

        new OrphanInspector(connection, NullLogger.Instance)
            .MaterializeOrphans("ca_error_orphans", "v_checker_csv_classified");

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT \"ErrorId\" FROM \"ca_error_orphans\"";
        Assert.AreEqual("FP1", command.ExecuteScalar());
    }

    [TestMethod]
    public async Task MaterializeOrphans_AllRowsMatched_CreatesNoTable()
    {
        using var connection = CreateOpenConnection();
        await SeedCsvTables(connection);
        SeedErrorMatrix(connection);
        AddErrorMatrixRow(connection, "FP1", "2020", "ClassZ");
        CreateClassifiedView(connection);

        new OrphanInspector(connection, NullLogger.Instance)
            .MaterializeOrphans("ca_error_orphans", "v_checker_csv_classified");

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = 'ca_error_orphans'";
        Assert.AreEqual(0, Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture));
    }

    private static SqliteConnection CreateOpenConnection()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        GeopackageMetadataSchema.Create(connection);
        return connection;
    }

    private static async Task SeedCsvTables(SqliteConnection connection)
    {
        var importer = new CsvImporter(connection, Encoding.UTF8, NullLogger.Instance);
        var tables = new[] { ("checker_csv_t", "T1", "ClassX"), ("checker_csv_a", "A1", "ClassY"), ("checker_csv_fp", "FP1", "ClassZ") };
        foreach (var (tableName, errorId, cls) in tables)
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes($"ErrorId;Model;Class;Module\n{errorId};2020;{cls};igcheck"));
            await importer.ImportAsync(stream, tableName, ["ErrorId", "Model", "Class", "Module"], CancellationToken.None);
        }
    }

    private static void SeedErrorMatrix(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE "error_matrix" ("t_id" INTEGER PRIMARY KEY AUTOINCREMENT, "cid" TEXT, "checkmodel" TEXT, "model" TEXT, "class_de" TEXT, "class_fr" TEXT, "severity" TEXT);
            INSERT INTO "error_matrix" ("cid", "checkmodel", "model", "class_de", "class_fr", "severity") VALUES ('T1', 'vsa', '2020', 'ClassX', 'ClassX', 'high');
            INSERT INTO "error_matrix" ("cid", "checkmodel", "model", "class_de", "class_fr", "severity") VALUES ('A1', 'vsa', '2020', 'ClassY', 'ClassY', 'low');
            """;
        command.ExecuteNonQuery();
    }

    private static void AddErrorMatrixRow(SqliteConnection connection, string cid, string model, string classDe)
    {
        using var command = connection.CreateCommand();
        command.CommandText =
            "INSERT INTO \"error_matrix\" (\"cid\", \"checkmodel\", \"model\", \"class_de\", \"class_fr\", \"severity\") VALUES (@cid, 'vsa', @model, @class, @class, 'low')";
        command.Parameters.AddWithValue("@cid", cid);
        command.Parameters.AddWithValue("@model", model);
        command.Parameters.AddWithValue("@class", classDe);
        command.ExecuteNonQuery();
    }

    private static void CreateClassifiedView(SqliteConnection connection)
    {
        var viewCreator = new ViewCreator(connection);
        viewCreator.CreateCheckerCsvUnionView(
            "v_checker_csv_all",
            ["checker_csv_t", "checker_csv_a", "checker_csv_fp"],
            ["T", "A", "FP"]);
        viewCreator.CreateCheckerCsvClassifiedView("v_checker_csv_classified", "v_checker_csv_all", "error_matrix", "DE");
    }
}
