using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using System.Diagnostics.CodeAnalysis;
using VsaCheckerAnalytics.TestHelpers;

namespace VsaCheckerAnalytics.Processors.GeopackageGeneration;

[TestClass]
public class ReaderErrorRulesInitializerTest
{
    [TestMethod]
    public void Initialize_AddsBaseRows_KeepsExistingMatrixRows()
    {
        using var connection = SetUpAndInitialize();

        Assert.AreEqual(33L, QueryLong(connection, "SELECT COUNT(*) FROM error_matrix WHERE checkmodel = 'base'"));
        Assert.AreEqual(1L, QueryLong(connection, "SELECT COUNT(*) FROM error_matrix WHERE checkmodel = 'vsa'"));
    }

    [TestMethod]
    public void Initialize_AddsItalianColumn_AndPopulatesBaseRow()
    {
        using var connection = SetUpAndInitialize();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT cmsg_de, cmsg_it FROM error_matrix WHERE cid = '11' AND checkmodel = 'base'";
        using var reader = cmd.ExecuteReader();

        Assert.IsTrue(reader.Read());
        Assert.AreEqual("Pflichtattribut {ATTR} fehlt", reader.GetString(0));
        Assert.AreEqual("Attributo obbligatorio {ATTR} mancante", reader.GetString(1));
    }

    [TestMethod]
    public void Initialize_SeedsReaderErrorRules_WithOverridesAndSuppression()
    {
        using var connection = SetUpAndInitialize();

        Assert.IsGreaterThanOrEqualTo(
            6L,
            QueryLong(connection, "SELECT COUNT(*) FROM reader_error_rules"));

        Assert.AreEqual(
            0L,
            QueryLong(connection, "SELECT suppress FROM reader_error_rules WHERE error_id = 11 AND attr_name = 'BetreiberRef'"));

        Assert.AreEqual(
            1L,
            QueryLong(connection, "SELECT suppress FROM reader_error_rules WHERE error_id = 12 AND attr_name = 'OBJ_ID_Abwasserbauwerk'"));
    }

    [TestMethod]
    public void Initialize_EverySeededRuleHasBaseRow()
    {
        using var connection = SetUpAndInitialize();

        // The classification treats a reader error as known only if it has a base row, so every rule
        // must target an ErrorId that has one. Initialize() guards this; assert it holds on the seed.
        const string unmatchedRuleQuery = """
            SELECT COUNT(*) FROM reader_error_rules r
            WHERE NOT EXISTS (
                SELECT 1 FROM error_matrix b
                WHERE b.checkmodel = 'base' AND b.cid = CAST(r.error_id AS TEXT))
            """;

        Assert.AreEqual(0L, QueryLong(connection, unmatchedRuleQuery));
    }

    [TestMethod]
    public void EnsureEveryRuleHasBaseRow_ThrowsWhenRuleHasNoBaseRow()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = """
                CREATE TABLE error_matrix (t_id INTEGER PRIMARY KEY AUTOINCREMENT, cid TEXT, checkmodel TEXT);
                INSERT INTO error_matrix (cid, checkmodel) VALUES ('11', 'base');
                CREATE TABLE reader_error_rules (rule_id INTEGER PRIMARY KEY AUTOINCREMENT, error_id INTEGER);
                INSERT INTO reader_error_rules (error_id) VALUES (11), (99);
                """;
            cmd.ExecuteNonQuery();
        }

        var initializer = new ReaderErrorRulesInitializer(connection, NullLogger.Instance);

        var ex = Assert.ThrowsExactly<InvalidOperationException>(() => initializer.EnsureEveryRuleHasBaseRow());
        Assert.Contains("99", ex.Message);
    }

    private static SqliteConnection SetUpAndInitialize()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        GeopackageMetadataSchema.Create(connection);
        CreateImportedErrorMatrix(connection);

        new ReaderErrorRulesInitializer(connection, NullLogger.Instance).Initialize();

        return connection;
    }

    // Mirrors the table the ErrorMatrixImporter produces from the igcheck XLSX (t_id + the fixed
    // column set), with one vsa row so the base-row replacement can be shown to leave it untouched.
    private static void CreateImportedErrorMatrix(SqliteConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE error_matrix (
                t_id INTEGER PRIMARY KEY AUTOINCREMENT,
                cid TEXT, ccat TEXT, cmsg_de TEXT, cmsg_fr TEXT,
                class_de TEXT, class_fr TEXT, checkmodel TEXT, model TEXT,
                prio_uc TEXT, prio_gsp TEXT,
                sub_project_gsp_de TEXT, sub_project_gsp_fr TEXT,
                required_action_de TEXT, required_action_fr TEXT,
                action_context_de TEXT, action_context_fr TEXT);

            INSERT INTO error_matrix (cid, ccat, cmsg_de, cmsg_fr, class_de, class_fr, checkmodel, model)
            VALUES ('1001', 'error', 'Fehler 1001', 'Erreur 1001', 'Leitung', 'Conduite', 'vsa', '2020');
            """;
        cmd.ExecuteNonQuery();
    }

    [SuppressMessage("Security", "CA2100", Justification = "Test queries use hardcoded SQL, not user input.")]
    private static long QueryLong(SqliteConnection connection, string sql)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        return (long)(cmd.ExecuteScalar() ?? 0L);
    }
}
