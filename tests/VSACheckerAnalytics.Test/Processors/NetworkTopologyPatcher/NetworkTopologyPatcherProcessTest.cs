using Geopilot.PipelineCore.Pipeline;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using NetTopologySuite.Geometries;
using System.Globalization;
using VsaCheckerAnalytics.TestHelpers;

namespace VsaCheckerAnalytics.Processors.NetworkTopologyPatcher;

[TestClass]
public sealed class NetworkTopologyPatcherProcessTest
{
    private const int Srid = 2056;

    private static readonly GeometryFactory Factory =
        NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory();

    private TestPipelineFileManager fileManager = null!;
    private string inputGpkgPath = null!;

    [TestInitialize]
    public void Setup()
    {
        fileManager = new TestPipelineFileManager();
        inputGpkgPath = Path.Combine(Path.GetTempPath(), $"input-{Guid.NewGuid():N}.gpkg");
        BuildMinimalGeoPackage(inputGpkgPath);
    }

    [TestCleanup]
    public void Cleanup()
    {
        fileManager.Dispose();
        if (File.Exists(inputGpkgPath))
        {
            File.Delete(inputGpkgPath);
        }
    }

    [TestMethod]
    public async Task RunAsyncProducesNetworkAndExtraEdgesWithExpectedCounts()
    {
        var input = new TestPipelineFile(inputGpkgPath);
        var patcherProcess = new NetworkTopologyPatcherProcess(fileManager, NullLogger.Instance);

        var result = await patcherProcess.RunAsync(input, CancellationToken.None);

        Assert.IsTrue(result.ContainsKey("patchedGeopackage"));
        Assert.IsNotNull(result["patchedGeopackage"]);
        var output = Assert.IsInstanceOfType<IPipelineFile>(result["patchedGeopackage"]);

        Assert.AreNotSame(input, output, "Process must write to a fresh copy, not mutate the input.");

        string outputPath;
        using (var fs = output.OpenReadFileStream())
        {
            outputPath = fs.Name;
        }

        using var connection = new SqliteConnection($"Data Source={outputPath};Pooling=false");
        connection.Open();

        // 3 leitungen (L1 to L3) + L4 (null vonref, end patched) + L5 (unknown vonref, end patched)
        // + L6 (both ends patched into network edge) + L7 (both ends patched into network edge)
        // + 1 valid aggregat = 8 network edges; U2 skipped (unknown ref).
        Assert.AreEqual(8, GetCount(connection, "ca_topo_network_edges"));

        // Extra edges are filtered by the allow-list on the target Knoten's funktion:
        // L3 contributes 2 (both ends Pumpwerk); L4 contributes 1 (end, Pumpwerk); L5 contributes 1
        // (end, Pumpwerk); L6 contributes 1 (start Pumpwerk; end node has no funktion → suppressed);
        // L7 contributes 0 (both nodes have no funktion); U1 contributes 1. Total 6.
        Assert.AreEqual(6, GetCount(connection, "ca_topo_extra_edges"));

        // L1: exact fit → no extras, diff 0 on both sides
        AssertNetworkRow(connection, srcTid: 10, expectedDiffStart: 0, expectedDiffEnd: 0, expectedLinetype: "topologielinie");

        // L2: gap below MinConnectorLength (0.10 m) → no extras, diff 0 on both sides
        AssertNetworkRow(connection, srcTid: 20, expectedDiffStart: 0, expectedDiffEnd: 0, expectedLinetype: "topologielinie");

        // L3: gap on both sides → connectors emitted, merged line spans node4→node5
        var l3 = GetNetworkRow(connection, srcTid: 30);
        Assert.AreEqual(10.0, l3.DiffStart!.Value, 1e-6);
        Assert.AreEqual(5.0, l3.DiffEnd!.Value, 1e-6);
        Assert.AreEqual("topologielinie", l3.Linetype);
        var mergedL3 = (LineString)GeoPackageGeometryCodec.ReadGeometry(l3.Geom, out _);
        Assert.AreEqual(2_600_300.0, mergedL3.StartPoint.X, 1e-6);
        Assert.AreEqual(2_600_400.0, mergedL3.EndPoint.X, 1e-6);

        // U1: aggregate becomes a direct segment, also in network table
        var u1 = GetNetworkRow(connection, srcTid: 100);
        Assert.AreEqual("ueberlauf_foerderaggregat", u1.Linetype);
        Assert.IsNull(u1.DiffStart);
        Assert.IsNull(u1.DiffEnd);

        // L4: NULL knoten_vonref → no start connector, but nachref=node5 has valid funktion → end
        // connector built from verlauf end (2_600_100) to node5 (2_600_400) = 300 m.
        var l4 = GetNetworkRow(connection, srcTid: 40);
        Assert.AreEqual("topologielinie", l4.Linetype);
        Assert.AreEqual(0.0, l4.DiffStart);
        Assert.AreEqual(300.0, l4.DiffEnd!.Value, 1e-6);
        Assert.IsNull(l4.VonRef);
        Assert.AreEqual(5L, l4.NachRef);
        var l4Line = (LineString)GeoPackageGeometryCodec.ReadGeometry(l4.Geom, out _);
        Assert.AreEqual(2_600_000.0, l4Line.StartPoint.X, 1e-6);
        Assert.AreEqual(2_600_400.0, l4Line.EndPoint.X, 1e-6);

        // L5: knoten_vonref references unknown node → no start connector. NachRef=node2 has valid
        // funktion and end gap of 50 m → end connector built. Merged line spans verlauf start →
        // node2 (2_600_100).
        var l5 = GetNetworkRow(connection, srcTid: 50);
        Assert.AreEqual("topologielinie", l5.Linetype);
        Assert.AreEqual(0.0, l5.DiffStart);
        Assert.AreEqual(50.0, l5.DiffEnd!.Value, 1e-6);
        Assert.AreEqual(999L, l5.VonRef);
        Assert.AreEqual(2L, l5.NachRef);
        var l5Line = (LineString)GeoPackageGeometryCodec.ReadGeometry(l5.Geom, out _);
        Assert.AreEqual(2_600_000.0, l5Line.StartPoint.X, 1e-6);
        Assert.AreEqual(2_600_100.0, l5Line.EndPoint.X, 1e-6);

        // L6: start node 5 has Pumpwerk funktion (start patched, 10 m); end node 6 has no knoten
        // attribute row → funktion is NULL. The end connector is still built and merged into the
        // network edge (5 m); only its emission as an extra edge is suppressed.
        var l6 = GetNetworkRow(connection, srcTid: 60);
        Assert.AreEqual("topologielinie", l6.Linetype);
        Assert.AreEqual(10.0, l6.DiffStart!.Value, 1e-6);
        Assert.AreEqual(5.0, l6.DiffEnd!.Value, 1e-6);
        var l6Line = (LineString)GeoPackageGeometryCodec.ReadGeometry(l6.Geom, out _);
        Assert.AreEqual(2_600_400.0, l6Line.StartPoint.X, 1e-6);
        Assert.AreEqual(2_600_500.0, l6Line.EndPoint.X, 1e-6);

        // L7: neither endpoint has a valid funktion (nodes 6 and 7 have no knoten row) → both
        // connectors are merged into the network edge (so the verlauf is connected to the knoten
        // coordinates), but neither is emitted as an extra edge.
        var l7 = GetNetworkRow(connection, srcTid: 70);
        Assert.AreEqual("topologielinie", l7.Linetype);
        Assert.AreEqual(10.0, l7.DiffStart!.Value, 1e-6);
        Assert.AreEqual(5.0, l7.DiffEnd!.Value, 1e-6);
        var l7Line = (LineString)GeoPackageGeometryCodec.ReadGeometry(l7.Geom, out _);
        Assert.AreEqual(2_600_500.0, l7Line.StartPoint.X, 1e-6);
        Assert.AreEqual(2_600_600.0, l7Line.EndPoint.X, 1e-6);

        // GeoPackage feature-layer registration
        Assert.AreEqual(1, GetScalar(connection, "SELECT COUNT(*) FROM gpkg_contents WHERE table_name = 'ca_topo_network_edges' AND data_type = 'features'"));
        Assert.AreEqual(1, GetScalar(connection, "SELECT COUNT(*) FROM gpkg_contents WHERE table_name = 'ca_topo_extra_edges' AND data_type = 'features'"));
        Assert.AreEqual(1, GetScalar(connection, "SELECT COUNT(*) FROM gpkg_geometry_columns WHERE table_name = 'ca_topo_network_edges' AND geometry_type_name = 'LINESTRING'"));
        Assert.AreEqual(1, GetScalar(connection, "SELECT COUNT(*) FROM gpkg_geometry_columns WHERE table_name = 'ca_topo_extra_edges' AND geometry_type_name = 'LINESTRING'"));

        // The input gpkg must be left untouched, no output tables in it.
        using var inputConnection = new SqliteConnection($"Data Source={inputGpkgPath};Pooling=false");
        inputConnection.Open();
        Assert.AreEqual(0, GetScalar(inputConnection, "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = 'ca_topo_network_edges'"));
        Assert.AreEqual(0, GetScalar(inputConnection, "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = 'ca_topo_extra_edges'"));
    }

    [TestMethod]
    public async Task RunAsyncSetsGpkgContentsExtentFromWrittenGeometries()
    {
        var input = new TestPipelineFile(inputGpkgPath);
        var patcherProcess = new NetworkTopologyPatcherProcess(fileManager, NullLogger.Instance);

        var result = await patcherProcess.RunAsync(input, CancellationToken.None);
        var output = Assert.IsInstanceOfType<IPipelineFile>(result["patchedGeopackage"]);

        string outputPath;
        using (var fs = output.OpenReadFileStream())
        {
            outputPath = fs.Name;
        }

        using var connection = new SqliteConnection($"Data Source={outputPath};Pooling=false");
        connection.Open();

        // The gpkg_contents extent must equal the bounding box of the actually written
        // geometries (computed from the data, not copied from the leitung placeholder).
        foreach (var table in new[] { "ca_topo_network_edges", "ca_topo_extra_edges" })
        {
            var stored = GetExtent(connection, table);
            var actual = ComputeGeomBbox(connection, table);
            Assert.AreEqual(actual.MinX, stored.MinX, 1e-6);
            Assert.AreEqual(actual.MinY, stored.MinY, 1e-6);
            Assert.AreEqual(actual.MaxX, stored.MaxX, 1e-6);
            Assert.AreEqual(actual.MaxY, stored.MaxY, 1e-6);
        }
    }

    [TestMethod]
    public async Task RunAsyncSkipsLeitungWhoseGeometryCannotBeMerged()
    {
        var path = Path.Combine(Path.GetTempPath(), $"merge-fail-{Guid.NewGuid():N}.gpkg");
        try
        {
            BuildMergeFailureGeoPackage(path);

            var input = new TestPipelineFile(path);
            var patcherProcess = new NetworkTopologyPatcherProcess(fileManager, NullLogger.Instance);

            var result = await patcherProcess.RunAsync(input, CancellationToken.None);
            var output = Assert.IsInstanceOfType<IPipelineFile>(result["patchedGeopackage"]);

            string outputPath;
            using (var fs = output.OpenReadFileStream())
            {
                outputPath = fs.Name;
            }

            using var connection = new SqliteConnection($"Data Source={outputPath};Pooling=false");
            connection.Open();

            // The valid leitung is written; the closed-ring leitung cannot be merged into a single
            // line and is skipped with a warning instead of aborting the whole run.
            Assert.AreEqual(1, GetScalar(connection, "SELECT COUNT(*) FROM ca_topo_network_edges WHERE src_tid = 10"));
            Assert.AreEqual(0, GetScalar(connection, "SELECT COUNT(*) FROM ca_topo_network_edges WHERE src_tid = 80"));
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    private static void BuildMinimalGeoPackage(string path)
    {
        using var connection = new SqliteConnection($"Data Source={path};Pooling=false");
        connection.Open();

        MinimalNetworkTopologyGeoPackage.CreateSchema(connection);

        // Nodes along y=1_200_000, spaced 100 m. Nodes 1 to 5 have a detailgeometrie (eligible for patching).
        InsertNode(connection, 1, 2_600_000.0, 1_200_000.0, hasDetailgeometrie: true);
        InsertNode(connection, 2, 2_600_100.0, 1_200_000.0, hasDetailgeometrie: true);
        InsertNode(connection, 3, 2_600_200.0, 1_200_000.0, hasDetailgeometrie: true);
        InsertNode(connection, 4, 2_600_300.0, 1_200_000.0, hasDetailgeometrie: true);
        InsertNode(connection, 5, 2_600_400.0, 1_200_000.0, hasDetailgeometrie: true);

        // Nodes 6 and 7 have only a lage, no knoten row (funktion NULL), so their connectors are kept in the network edge but not emitted as extra edges.
        InsertNode(connection, 6, 2_600_500.0, 1_200_000.0, hasDetailgeometrie: false);
        InsertNode(connection, 7, 2_600_600.0, 1_200_000.0, hasDetailgeometrie: false);

        // L1: perfectly fits node 1 → node 2
        InsertLeitung(connection, 10, 1, 2, new Coordinate(2_600_000.0, 1_200_000.0), new Coordinate(2_600_100.0, 1_200_000.0));

        // L2: start gap 0.05 m, below MinConnectorLength (0.10 m) → no connector
        InsertLeitung(connection, 20, 3, 4, new Coordinate(2_600_200.05, 1_200_000.0), new Coordinate(2_600_300.0, 1_200_000.0));

        // L3: large gap on both sides → start connector 10 m, end connector 5 m
        InsertLeitung(connection, 30, 4, 5, new Coordinate(2_600_310.0, 1_200_000.0), new Coordinate(2_600_395.0, 1_200_000.0));

        // L4: NULL knoten_vonref → no start connector; nachref=node5 has valid funktion and 300 m
        // end gap → end connector built.
        InsertLeitung(connection, 40, null, 5, new Coordinate(2_600_000.0, 1_200_000.0), new Coordinate(2_600_100.0, 1_200_000.0));

        // L5: knoten_vonref points to unknown node → no start connector; nachref=node2 has valid
        // funktion and 50 m end gap → end connector built.
        InsertLeitung(connection, 50, 999, 2, new Coordinate(2_600_000.0, 1_200_000.0), new Coordinate(2_600_050.0, 1_200_000.0));

        // L6: start node 5 has detailgeometrie (start patched), end node 6 does not (end connector suppressed).
        // Start gap 10 m, end gap 5 m → only the start connector is emitted.
        InsertLeitung(connection, 60, 5, 6, new Coordinate(2_600_410.0, 1_200_000.0), new Coordinate(2_600_495.0, 1_200_000.0));

        // L7: neither endpoint has detailgeometrie → no connectors, original verlauf inserted unaltered.
        InsertLeitung(connection, 70, 6, 7, new Coordinate(2_600_510.0, 1_200_000.0), new Coordinate(2_600_595.0, 1_200_000.0));

        // U1: valid aggregate, node 1 → node 3
        InsertAggregat(connection, 100, 1, 3);

        // U2: references unknown node 999 → must be skipped with warning
        InsertAggregat(connection, 200, 1, 999);
    }

    private static void BuildMergeFailureGeoPackage(string path)
    {
        using var connection = new SqliteConnection($"Data Source={path};Pooling=false");
        connection.Open();
        MinimalNetworkTopologyGeoPackage.CreateSchema(connection);

        // Valid pair: leitung 10 fits node 1 to node 2 exactly, so it merges and is written.
        InsertNode(connection, 1, 2_600_000.0, 1_200_000.0, hasDetailgeometrie: true);
        InsertNode(connection, 2, 2_600_100.0, 1_200_000.0, hasDetailgeometrie: true);
        InsertLeitung(connection, 10, 1, 2, new Coordinate(2_600_000.0, 1_200_000.0), new Coordinate(2_600_100.0, 1_200_000.0));

        // Closed-ring verlauf with connectors on both ends: LineMerger yields more than one line,
        // so MergeSegments throws and the row must be skipped.
        InsertNode(connection, 8, 2_600_700.0, 1_200_000.0, hasDetailgeometrie: true);
        InsertNode(connection, 9, 2_600_710.0, 1_200_000.0, hasDetailgeometrie: true);
        InsertLeitung(
            connection,
            80,
            8,
            9,
            new Coordinate(2_600_705.0, 1_200_000.0),
            new Coordinate(2_600_706.0, 1_200_001.0),
            new Coordinate(2_600_705.0, 1_200_001.0),
            new Coordinate(2_600_705.0, 1_200_000.0));
    }

    private static void InsertNode(SqliteConnection connection, long tid, double x, double y, bool hasDetailgeometrie)
    {
        var point = Factory.CreatePoint(new Coordinate(x, y));
        var blob = GeoPackageGeometryCodec.WriteGeometry(point, Srid);

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "INSERT INTO knoten_lage (t_id, lage) VALUES (@tid, @geom)";
        cmd.Parameters.AddWithValue("@tid", tid);
        cmd.Parameters.AddWithValue("@geom", blob);
        cmd.ExecuteNonQuery();

        if (hasDetailgeometrie)
        {
            using var knotenCmd = connection.CreateCommand();
            knotenCmd.CommandText = "INSERT INTO knoten (t_id, funktion, detailgeometrie) VALUES (@tid, @funktion, @geom)";
            knotenCmd.Parameters.AddWithValue("@tid", tid);
            knotenCmd.Parameters.AddWithValue("@funktion", "Pumpwerk");
            knotenCmd.Parameters.AddWithValue("@geom", blob);
            knotenCmd.ExecuteNonQuery();
        }
    }

    private static void InsertLeitung(SqliteConnection connection, long tid, long? vonRef, long? nachRef, params Coordinate[] vertices)
    {
        var line = Factory.CreateLineString(vertices);
        var blob = GeoPackageGeometryCodec.WriteGeometry(line, Srid);

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "INSERT INTO leitung (t_id, knoten_vonref, knoten_nachref, verlauf) VALUES (@tid, @von, @nach, @geom)";
        cmd.Parameters.AddWithValue("@tid", tid);
        cmd.Parameters.AddWithValue("@von", (object?)vonRef ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@nach", (object?)nachRef ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@geom", blob);
        cmd.ExecuteNonQuery();
    }

    private static void InsertAggregat(SqliteConnection connection, long tid, long? knotenRef, long? nachRef)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "INSERT INTO ueberlauf_foerderaggregat (t_id, knotenref, knoten_nachref) VALUES (@tid, @kref, @nref)";
        cmd.Parameters.AddWithValue("@tid", tid);
        cmd.Parameters.AddWithValue("@kref", (object?)knotenRef ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@nref", (object?)nachRef ?? DBNull.Value);
        cmd.ExecuteNonQuery();
    }

#pragma warning disable CA2100
    private static int GetCount(SqliteConnection connection, string table)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = $"SELECT COUNT(*) FROM \"{table}\"";
        return Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture);
    }

    private static int GetScalar(SqliteConnection connection, string sql)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        return Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture);
    }

#pragma warning restore CA2100

    private static (double MinX, double MinY, double MaxX, double MaxY) GetExtent(SqliteConnection connection, string table)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT min_x, min_y, max_x, max_y FROM gpkg_contents WHERE table_name = @t";
        cmd.Parameters.AddWithValue("@t", table);
        using var reader = cmd.ExecuteReader();
        Assert.IsTrue(reader.Read(), $"Expected gpkg_contents row for {table}");
        return (reader.GetDouble(0), reader.GetDouble(1), reader.GetDouble(2), reader.GetDouble(3));
    }

#pragma warning disable CA2100
    private static (double MinX, double MinY, double MaxX, double MaxY) ComputeGeomBbox(SqliteConnection connection, string table)
    {
        var envelope = new Envelope();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = $"SELECT geom FROM \"{table}\" WHERE geom IS NOT NULL";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var geom = GeoPackageGeometryCodec.ReadGeometry((byte[])reader.GetValue(0), out _);
            envelope.ExpandToInclude(geom.EnvelopeInternal);
        }

        return (envelope.MinX, envelope.MinY, envelope.MaxX, envelope.MaxY);
    }
#pragma warning restore CA2100

    private static void AssertNetworkRow(SqliteConnection connection, long srcTid, double? expectedDiffStart, double? expectedDiffEnd, string expectedLinetype)
    {
        var row = GetNetworkRow(connection, srcTid);
        Assert.AreEqual(expectedLinetype, row.Linetype);
        Assert.AreEqual(expectedDiffStart, row.DiffStart);
        Assert.AreEqual(expectedDiffEnd, row.DiffEnd);
    }

    private static NetworkRow GetNetworkRow(SqliteConnection connection, long srcTid)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT diff_start, diff_end, linetype, geom, knoten_vonref, knoten_nachref FROM ca_topo_network_edges WHERE src_tid = @t";
        cmd.Parameters.AddWithValue("@t", srcTid);
        using var reader = cmd.ExecuteReader();
        Assert.IsTrue(reader.Read(), $"Expected row with src_tid={srcTid}");
        return new NetworkRow(
            DiffStart: reader.IsDBNull(0) ? null : reader.GetDouble(0),
            DiffEnd: reader.IsDBNull(1) ? null : reader.GetDouble(1),
            Linetype: reader.GetString(2),
            Geom: (byte[])reader.GetValue(3),
            VonRef: reader.IsDBNull(4) ? null : reader.GetInt64(4),
            NachRef: reader.IsDBNull(5) ? null : reader.GetInt64(5));
    }

    private sealed record NetworkRow(double? DiffStart, double? DiffEnd, string Linetype, byte[] Geom, long? VonRef, long? NachRef);
}
