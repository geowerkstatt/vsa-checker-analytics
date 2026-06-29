using Microsoft.Data.Sqlite;
using System.Diagnostics.CodeAnalysis;

namespace VsaCheckerAnalytics.TestHelpers;

/// <summary>
/// Creates the GeoPackage metadata tables (<c>gpkg_contents</c> and <c>gpkg_geometry_columns</c>)
/// on an in-memory connection. Unit tests that exercise table/view creation need these because
/// creation now also registers the layer in the GeoPackage metadata; a real GeoPackage always
/// carries these OGC system tables.
/// </summary>
internal static class GeopackageMetadataSchema
{
    internal static void Create(SqliteConnection connection)
    {
        const string createContentsSql = """
            CREATE TABLE gpkg_contents (
                table_name TEXT NOT NULL PRIMARY KEY,
                data_type TEXT NOT NULL,
                identifier TEXT UNIQUE,
                description TEXT DEFAULT '',
                last_change DATETIME NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ','now')),
                min_x DOUBLE, min_y DOUBLE, max_x DOUBLE, max_y DOUBLE,
                srs_id INTEGER)
            """;
        Exec(connection, createContentsSql);

        const string createGeometryColumnsSql = """
            CREATE TABLE gpkg_geometry_columns (
                table_name TEXT NOT NULL,
                column_name TEXT NOT NULL,
                geometry_type_name TEXT NOT NULL,
                srs_id INTEGER NOT NULL,
                z TINYINT NOT NULL,
                m TINYINT NOT NULL,
                PRIMARY KEY (table_name, column_name))
            """;
        Exec(connection, createGeometryColumnsSql);
    }

    [SuppressMessage("Security", "CA2100", Justification = "Test schema DDL is a hardcoded constant, not user input.")]
    private static void Exec(SqliteConnection connection, string sql)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }
}
