using Microsoft.Data.Sqlite;
using System.Diagnostics.CodeAnalysis;
using VsaCheckerAnalytics.Geopackage;

namespace VsaCheckerAnalytics.Processors.NetworkTopologyPatcher;

/// <summary>
/// Creates the two output feature tables <c>ca_topo_network_edges</c> and
/// <c>ca_topo_extra_edges</c>, and registers them via <see cref="GeopackageMetadata"/> so
/// QGIS auto-discovers them as renderable layers.
/// </summary>
internal static class TopologyOutputTables
{
    internal const string NetworkEdgesTable = "ca_topo_network_edges";
    internal const string ExtraEdgesTable = "ca_topo_extra_edges";

    private const string GeometryColumn = "geom";
    private const string GeometryType = "LINESTRING";

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

        GeopackageMetadata.RegisterFeature(connection, NetworkEdgesTable, GeometryColumn, GeometryType, srid);
        GeopackageMetadata.RegisterFeature(connection, ExtraEdgesTable, GeometryColumn, GeometryType, srid);
    }

    [SuppressMessage("Security", "CA2100", Justification = "All SQL is built from internal constants, not user input.")]
    private static void ExecuteNonQuery(SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }
}
