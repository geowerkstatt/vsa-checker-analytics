using Microsoft.Data.Sqlite;
using NetTopologySuite.Geometries;
using System.Diagnostics.CodeAnalysis;

namespace VsaCheckerAnalytics.Processors.NetworkTopologyPatcher;

/// <summary>
/// Creates the two output feature tables <c>ca_topo_network_edges</c> and
/// <c>ca_topo_extra_edges</c>, and registers them in <c>gpkg_contents</c> and
/// <c>gpkg_geometry_columns</c> so QGIS auto-discovers them as renderable layers.
/// </summary>
internal static class TopologyOutputTables
{
    internal const string NetworkEdgesTable = "ca_topo_network_edges";
    internal const string ExtraEdgesTable = "ca_topo_extra_edges";

    internal static void Create(SqliteConnection connection, int srid)
    {
        var networkEdgesTableSql = $"""
            CREATE TABLE {NetworkEdgesTable} (
                fid INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                src_tid INTEGER,
                knoten_vonref INTEGER,
                knoten_nachref INTEGER,
                diff_start REAL,
                diff_end REAL,
                linetype TEXT,
                geom BLOB)
            """;
        ExecuteNonQuery(connection, networkEdgesTableSql);

        var extraEdgesTableSql = $"""
            CREATE TABLE {ExtraEdgesTable} (
                fid INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                src_tid INTEGER,
                tid_pipe INTEGER,
                diff REAL,
                linetype TEXT,
                geom BLOB)
            """;
        ExecuteNonQuery(connection, extraEdgesTableSql);

        RegisterFeatureTable(connection, NetworkEdgesTable, srid);
        RegisterFeatureTable(connection, ExtraEdgesTable, srid);
    }

    /// <summary>
    /// Updates the <c>gpkg_contents</c> bounding box of <paramref name="tableName"/> to the
    /// given <paramref name="extent"/>. No-op when the extent is empty (no features written).
    /// </summary>
    internal static void SetLayerExtent(SqliteConnection connection, string tableName, Envelope extent)
    {
        if (extent.IsNull)
        {
            return;
        }

        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            UPDATE gpkg_contents SET min_x = @min_x, min_y = @min_y, max_x = @max_x, max_y = @max_y
            WHERE table_name = @name
            """;
        cmd.Parameters.AddWithValue("@name", tableName);
        cmd.Parameters.AddWithValue("@min_x", extent.MinX);
        cmd.Parameters.AddWithValue("@min_y", extent.MinY);
        cmd.Parameters.AddWithValue("@max_x", extent.MaxX);
        cmd.Parameters.AddWithValue("@max_y", extent.MaxY);
        cmd.ExecuteNonQuery();
    }

    private static void RegisterFeatureTable(SqliteConnection connection, string tableName, int srid)
    {
        // gpkg_contents: identify as a feature table (extent is set later from the written geometries).
        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = """
                INSERT INTO gpkg_contents (table_name, data_type, identifier, description, last_change, srs_id)
                VALUES (@name, 'features', @name, '', strftime('%Y-%m-%dT%H:%M:%fZ', 'now'), @srid)
                """;
            cmd.Parameters.AddWithValue("@name", tableName);
            cmd.Parameters.AddWithValue("@srid", srid);
            cmd.ExecuteNonQuery();
        }

        // gpkg_geometry_columns: declare the geometry column.
        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = """
                INSERT INTO gpkg_geometry_columns (table_name, column_name, geometry_type_name, srs_id, z, m)
                VALUES (@name, 'geom', 'LINESTRING', @srid, 0, 0)
                """;
            cmd.Parameters.AddWithValue("@name", tableName);
            cmd.Parameters.AddWithValue("@srid", srid);
            cmd.ExecuteNonQuery();
        }
    }

    [SuppressMessage("Security", "CA2100", Justification = "All SQL is built from internal constants, not user input.")]
    private static void ExecuteNonQuery(SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }
}
