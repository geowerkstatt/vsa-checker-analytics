using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using VsaCheckerAnalytics.TestHelpers;

namespace VsaCheckerAnalytics.Processors.GeopackageGeneration;

[TestClass]
public class StatisticsMaterializerTest
{
    [TestMethod]
    public void Materialize_CopiesViewRowsIntoTable_PreservingOrder()
    {
        using var connection = CreateOpenConnection();
        SeedDssTables(connection);
        SeedStatisticsView(connection);

        new StatisticsMaterializer(connection, NullLogger.Instance)
            .Materialize("ca_statistics_attribute", "v_statistics_attribute");

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT tabelle, attribut, anzahl_total FROM \"ca_statistics_attribute\"";
        using var reader = command.ExecuteReader();

        var rows = new List<(string Tabelle, string Attribut, long Total)>();
        while (reader.Read())
        {
            rows.Add((reader.GetString(0), reader.GetString(1), reader.GetInt64(2)));
        }

        Assert.HasCount(2, rows);
        Assert.AreEqual("knoten", rows[0].Tabelle);
        Assert.AreEqual("funktion", rows[0].Attribut);
        Assert.AreEqual(5L, rows[0].Total);
        Assert.AreEqual("leitung", rows[1].Tabelle);
        Assert.AreEqual(3L, rows[1].Total);
    }

    [TestMethod]
    public void Materialize_RegistersTableAsAttributesLayer()
    {
        using var connection = CreateOpenConnection();
        SeedDssTables(connection);
        SeedStatisticsView(connection);

        new StatisticsMaterializer(connection, NullLogger.Instance)
            .Materialize("ca_statistics_attribute", "v_statistics_attribute");

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT data_type FROM gpkg_contents WHERE table_name = 'ca_statistics_attribute'";
        Assert.AreEqual("attributes", command.ExecuteScalar());
    }

    [TestMethod]
    public void Materialize_WithoutDssTables_SkipsTableCreation()
    {
        using var connection = CreateOpenConnection();
        SeedStatisticsView(connection);

        new StatisticsMaterializer(connection, NullLogger.Instance)
            .Materialize("ca_statistics_attribute", "v_statistics_attribute");

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = 'ca_statistics_attribute'";
        Assert.AreEqual(0L, (long)command.ExecuteScalar()!);
    }

    private static SqliteConnection CreateOpenConnection()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        GeopackageMetadataSchema.Create(connection);
        return connection;
    }

    private static void SeedDssTables(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE leitung (t_ili_tid TEXT);
            CREATE TABLE knoten (t_ili_tid TEXT);
            """;
        command.ExecuteNonQuery();
    }

    private static void SeedStatisticsView(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE VIEW v_statistics_attribute AS
            SELECT 'knoten' AS tabelle, 'funktion' AS attribut, 5 AS anzahl_total,
                   2 AS anzahl_paa, 3 AS anzahl_saa, 1 AS anzahl_null, 0 AS anzahl_null_paa, 1 AS anzahl_null_saa
            UNION ALL
            SELECT 'leitung', 'material', 3, NULL, NULL, 0, NULL, NULL
            """;
        command.ExecuteNonQuery();
    }
}
