using Microsoft.Data.Sqlite;

namespace VsaCheckerAnalytics.TestHelpers;

internal static class MinimalNetworkTopologyGeoPackage
{
    public static void CreateSchema(SqliteConnection connection)
    {
        const string createSpatialRefTableSql = """
            CREATE TABLE gpkg_spatial_ref_sys (
                srs_name TEXT NOT NULL,
                srs_id INTEGER NOT NULL PRIMARY KEY,
                organization TEXT NOT NULL,
                organization_coordsys_id INTEGER NOT NULL,
                definition TEXT NOT NULL,
                description TEXT)
            """;
        Exec(connection, createSpatialRefTableSql);

        const string createGpkgContentsTableSql = """
            CREATE TABLE gpkg_contents (
                table_name TEXT NOT NULL PRIMARY KEY,
                data_type TEXT NOT NULL,
                identifier TEXT UNIQUE,
                description TEXT DEFAULT '',
                last_change DATETIME NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ','now')),
                min_x DOUBLE,
                min_y DOUBLE,
                max_x DOUBLE,
                max_y DOUBLE,
                srs_id INTEGER)
            """;
        Exec(connection, createGpkgContentsTableSql);

        const string createGeometryColumnsTableSql = """
            CREATE TABLE gpkg_geometry_columns (
                table_name TEXT NOT NULL,
                column_name TEXT NOT NULL,
                geometry_type_name TEXT NOT NULL,
                srs_id INTEGER NOT NULL,
                z TINYINT NOT NULL,
                m TINYINT NOT NULL,
                PRIMARY KEY (table_name, column_name))
            """;
        Exec(connection, createGeometryColumnsTableSql);

        Exec(connection, "CREATE TABLE knoten_lage (t_id INTEGER PRIMARY KEY, lage BLOB)");
        Exec(connection, "CREATE TABLE knoten (t_id INTEGER PRIMARY KEY, funktion TEXT, detailgeometrie BLOB)");
        Exec(connection, "CREATE TABLE leitung (t_id INTEGER PRIMARY KEY, knoten_vonref INTEGER, knoten_nachref INTEGER, verlauf BLOB)");
        Exec(connection, "CREATE TABLE ueberlauf_foerderaggregat (t_id INTEGER PRIMARY KEY, knotenref INTEGER, knoten_nachref INTEGER)");
    }

    internal static void Exec(SqliteConnection connection, string sql)
    {
        using var cmd = connection.CreateCommand();
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
        cmd.CommandText = sql;
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
        cmd.ExecuteNonQuery();
    }
}
