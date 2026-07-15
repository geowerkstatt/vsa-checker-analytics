using Microsoft.Data.Sqlite;
using System.Globalization;

namespace VsaCheckerAnalytics.Processors.GeopackageGeneration;

[TestClass]
public class ErrorMatrixSchemaTest
{
    private static readonly string[] ExpectedColumns =
    [
        "cid", "ccat", "cmsg_de", "cmsg_fr", "class_de", "class_fr", "checkmodel", "model",
        "prio_uc", "prio_gsp", "sub_project_gsp_de", "sub_project_gsp_fr",
        "required_action_de", "required_action_fr", "action_context_de", "action_context_fr",
        "cmsg_it", "error_type_de", "error_type_fr", "error_type_it",
        "required_action_it", "action_context_it",
    ];

    [TestMethod]
    public void Schema_CreatesErrorMatrix_WithAllColumnsAndJoinIndex()
    {
        using var connection = CreateOpenConnection();

        EmbeddedSql.Execute(connection, "ErrorMatrixSchema.sql");

        var columns = GetColumnNames(connection, "error_matrix");
        Assert.AreEqual("t_id", columns[0]);
        CollectionAssert.AreEqual(ExpectedColumns, columns.Skip(1).ToList());

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT name FROM sqlite_master WHERE type = 'index' AND name = 'ix_error_matrix_cid_model_class_de'";
        Assert.IsNotNull(command.ExecuteScalar());
    }

    [TestMethod]
    public void Schema_IsReRunnable_LeavesEmptyTable()
    {
        using var connection = CreateOpenConnection();

        EmbeddedSql.Execute(connection, "ErrorMatrixSchema.sql");

        using (var command = connection.CreateCommand())
        {
            command.CommandText = "INSERT INTO error_matrix (cid, checkmodel) VALUES ('X', 'vsa')";
            command.ExecuteNonQuery();
        }

        Assert.AreEqual(1, GetRowCount(connection, "error_matrix"));

        // Re-running must DROP and recreate the table (clear it), not preserve existing rows.
        EmbeddedSql.Execute(connection, "ErrorMatrixSchema.sql");

        Assert.AreEqual(0, GetRowCount(connection, "error_matrix"));
    }

    private static SqliteConnection CreateOpenConnection()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        return connection;
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
}
