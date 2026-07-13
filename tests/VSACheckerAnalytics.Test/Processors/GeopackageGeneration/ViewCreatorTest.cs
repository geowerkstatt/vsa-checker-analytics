using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using System.Globalization;
using System.Text;
using VsaCheckerAnalytics.TestHelpers;

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
    public async Task CreateCheckerCsvClassifiedView_FlagsMatchedRowsKnown_AndUnmatchedRowsUnknown()
    {
        using var connection = CreateOpenConnection();
        await SeedCsvTables(connection);
        SeedErrorMatrix(connection);
        CreateUnionView(connection);

        var viewCreator = new ViewCreator(connection);
        viewCreator.CreateCheckerCsvClassifiedView("v_checker_csv_classified", "v_checker_csv_all", "error_matrix", "DE");

        // T1/ClassX and A1/ClassY have matching matrix rows => known; FP1/ClassZ has none => unknown.
        Assert.AreEqual(1, IsKnown(connection, "T1"));
        Assert.AreEqual(1, IsKnown(connection, "A1"));
        Assert.AreEqual(0, IsKnown(connection, "FP1"));
    }

    [TestMethod]
    public void CreateAdditionalViews_CreatesAllExpectedViews()
    {
        using var connection = CreateOpenConnection();

        var viewCreator = new ViewCreator(connection);
        viewCreator.CreateAdditionalViews();

        // Each view is registered as a GeoPackage layer right at its creation: spatial views as
        // features (with a geometry column), the errorlist data views as attributes.
        using (var registration = connection.CreateCommand())
        {
            registration.CommandText = "SELECT data_type FROM gpkg_contents WHERE table_name = 'v_vsa_knoten'";
            Assert.AreEqual("features", registration.ExecuteScalar());

            registration.CommandText = "SELECT data_type FROM gpkg_contents WHERE table_name = 'v_errorlist_error_knoten_data'";
            Assert.AreEqual("attributes", registration.ExecuteScalar());

            registration.CommandText = "SELECT geometry_type_name FROM gpkg_geometry_columns WHERE table_name = 'v_vsa_leitung'";
            Assert.AreEqual("LINESTRING", registration.ExecuteScalar());
        }

        var expectedViews = new[]
        {
            "v_vsa_knoten",
            "v_vsa_knoten_abwasserknoten",
            "v_vsa_knoten_detailgeometrie",
            "v_vsa_knoten_einleitstelle",
            "v_vsa_knoten_messstelle",
            "v_vsa_knoten_normschacht",
            "v_vsa_knoten_spezialbauwerk",
            "v_vsa_knoten_text",
            "v_vsa_knoten_versickerungsanlage",
            "v_vsa_leitung",
            "v_vsa_leitung_text",
            "v_vsa_ueberlauf_foerderaggregat",
            "v_errorlist_ueberlauf_foerderaggregat_data",
            "v_errorlist_error_teileinzugsgebiet_data",
            "v_errorlist_error_knoten_data",
            "v_errorlist_error_haltung_data",
            "v_error_ueberlauf_foerderaggregat",
            "v_error_teileinzugsgebiet",
            "v_error_sk_trennbauwerk",
            "v_error_sk_regenrueckhaltebecken_kanal",
            "v_error_sk_regenueberlaufbecken",
            "v_error_sk_regenueberlauf",
            "v_error_sk_pumpwerk",
            "v_error_sk_einleitstelle",
            "v_error_recommendation_teileinzugsgebiet",
            "v_error_recommendation_knoten",
            "v_error_recommendation_haltung",
            "v_error_knoten",
            "v_error_haltung",
            "v_error_error_knoten",
            "v_error_error_haltung",
            "v_error_category_knoten",
            "v_error_category_haltung",
        };

        var actualViews = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT name FROM sqlite_master WHERE type = 'view'";
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            actualViews.Add(reader.GetString(0));
        }

        foreach (var expected in expectedViews)
        {
            Assert.Contains(expected, actualViews, $"Expected view '{expected}' was not created.");
        }
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

    private static void CreateUnionView(SqliteConnection connection)
    {
        var viewCreator = new ViewCreator(connection);
        viewCreator.CreateCheckerCsvUnionView(
            "v_checker_csv_all",
            ["checker_csv_t", "checker_csv_a", "checker_csv_fp"],
            ["T", "A", "FP"]);
    }

    private static int IsKnown(SqliteConnection connection, string errorId)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT is_known FROM \"v_checker_csv_classified\" WHERE \"ErrorId\" = @id";
        command.Parameters.AddWithValue("@id", errorId);
        return Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
    }
}
