using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using NetTopologySuite.Geometries;
using System.Globalization;
using VsaCheckerAnalytics.TestHelpers;

namespace VsaCheckerAnalytics.Processors.NetworkTopologyPatcher;

[TestClass]
public sealed class NetworkTopologyPatcherTest
{
    private const int Srid = 2056;

    private static readonly GeometryFactory Factory =
        NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory();

    [TestMethod]
    public void ComputeLeitungEdgesReturnsUnalteredLeitungWhenNoConnectorsRequired()
    {
        var verlauf = Factory.CreateLineString(new[] { new Coordinate(0, 0), new Coordinate(10, 0) });
        var row = new LeitungRow(Tid: 1, VonRef: 1, NachRef: 2, Verlauf: verlauf);
        var knotenIndex = new Dictionary<long, KnotenInfo>
        {
            [1] = new KnotenInfo(new Coordinate(0, 0), "Pumpwerk"),
            [2] = new KnotenInfo(new Coordinate(10, 0), "Pumpwerk"),
        };

        var result = NetworkTopologyPatcher.ComputeLeitungEdges(row, knotenIndex, Factory);

        Assert.AreEqual(1L, result.NetworkEdge.SrcTid);
        Assert.AreEqual(1L, result.NetworkEdge.VonRef);
        Assert.AreEqual(2L, result.NetworkEdge.NachRef);
        Assert.AreEqual("topologielinie", result.NetworkEdge.Linetype);
        Assert.AreEqual(0d, result.NetworkEdge.DiffStart);
        Assert.AreEqual(0d, result.NetworkEdge.DiffEnd);
        Assert.AreEqual(new Coordinate(0, 0), result.NetworkEdge.Geom.StartPoint.Coordinate);
        Assert.AreEqual(new Coordinate(10, 0), result.NetworkEdge.Geom.EndPoint.Coordinate);
        Assert.IsEmpty(result.ExtraEdges);
    }

    [TestMethod]
    public void ComputeLeitungEdgesCreatesStartAndEndConnectors()
    {
        var verlauf = Factory.CreateLineString(new[] { new Coordinate(5, 0), new Coordinate(10, 5), new Coordinate(15, 0) });
        var row = new LeitungRow(Tid: 1, VonRef: 1, NachRef: 2, Verlauf: verlauf);
        var knotenIndex = new Dictionary<long, KnotenInfo>
        {
            [1] = new KnotenInfo(new Coordinate(0, 0), "Pumpwerk"),
            [2] = new KnotenInfo(new Coordinate(20, 0), "Pumpwerk"),
        };

        var result = NetworkTopologyPatcher.ComputeLeitungEdges(row, knotenIndex, Factory);

        Assert.AreEqual(1L, result.NetworkEdge.SrcTid);
        Assert.AreEqual(1L, result.NetworkEdge.VonRef);
        Assert.AreEqual(2L, result.NetworkEdge.NachRef);
        Assert.AreEqual("topologielinie", result.NetworkEdge.Linetype);
        Assert.AreEqual(5.0, result.NetworkEdge.DiffStart!.Value, 1e-9);
        Assert.AreEqual(5.0, result.NetworkEdge.DiffEnd!.Value, 1e-9);
        Assert.HasCount(2, result.ExtraEdges);
        Assert.AreEqual(5.0, result.ExtraEdges[0].Diff, 1e-9);
        Assert.AreEqual(5.0, result.ExtraEdges[1].Diff, 1e-9);
        Assert.AreEqual(1L, result.ExtraEdges[0].SrcTid);
        Assert.AreEqual(1L, result.ExtraEdges[0].TidPipe);
        Assert.AreEqual("topologielinie", result.ExtraEdges[0].Linetype);

        // Start connector goes from VonRef Knoten to the original verlauf start vertex.
        Assert.AreEqual(new Coordinate(0, 0), result.ExtraEdges[0].Geom.StartPoint.Coordinate);
        Assert.AreEqual(new Coordinate(5, 0), result.ExtraEdges[0].Geom.EndPoint.Coordinate);

        // End connector goes from the original verlauf end vertex to the NachRef Knoten.
        Assert.AreEqual(new Coordinate(15, 0), result.ExtraEdges[1].Geom.StartPoint.Coordinate);
        Assert.AreEqual(new Coordinate(20, 0), result.ExtraEdges[1].Geom.EndPoint.Coordinate);

        CollectionAssert.AreEqual(
            new[] { new Coordinate(0, 0), new Coordinate(5, 0), new Coordinate(10, 5), new Coordinate(15, 0), new Coordinate(20, 0) },
            result.NetworkEdge.Geom.Coordinates);
    }

    [TestMethod]
    public void ComputeLeitungEdgesCreatesOnlyStartConnectorWhenEndFitsExactly()
    {
        var verlauf = Factory.CreateLineString(new[] { new Coordinate(5, 0), new Coordinate(10, 0) });
        var row = new LeitungRow(Tid: 1, VonRef: 1, NachRef: 2, Verlauf: verlauf);
        var knotenIndex = new Dictionary<long, KnotenInfo>
        {
            [1] = new KnotenInfo(new Coordinate(0, 0), "Pumpwerk"),
            [2] = new KnotenInfo(new Coordinate(10, 0), "Pumpwerk"),
        };

        var result = NetworkTopologyPatcher.ComputeLeitungEdges(row, knotenIndex, Factory);

        Assert.HasCount(1, result.ExtraEdges);
    }

    [TestMethod]
    public void ComputeLeitungEdgesCreatesOnlyEndConnectorWhenStartFitsExactly()
    {
        var verlauf = Factory.CreateLineString(new[] { new Coordinate(0, 0), new Coordinate(15, 0) });
        var row = new LeitungRow(Tid: 1, VonRef: 1, NachRef: 2, Verlauf: verlauf);
        var knotenIndex = new Dictionary<long, KnotenInfo>
        {
            [1] = new KnotenInfo(new Coordinate(0, 0), "Pumpwerk"),
            [2] = new KnotenInfo(new Coordinate(20, 0), "Pumpwerk"),
        };

        var result = NetworkTopologyPatcher.ComputeLeitungEdges(row, knotenIndex, Factory);

        Assert.HasCount(1, result.ExtraEdges);
    }

    [TestMethod]
    public void ComputeLeitungEdgesDiscardsConnectorsBelowMinimumLength()
    {
        var verlauf = Factory.CreateLineString(new[] { new Coordinate(0.05, 0), new Coordinate(9.95, 0) });
        var row = new LeitungRow(Tid: 1, VonRef: 1, NachRef: 2, Verlauf: verlauf);
        var knotenIndex = new Dictionary<long, KnotenInfo>
        {
            [1] = new KnotenInfo(new Coordinate(0, 0), "Pumpwerk"),
            [2] = new KnotenInfo(new Coordinate(10, 0), "Pumpwerk"),
        };

        var result = NetworkTopologyPatcher.ComputeLeitungEdges(row, knotenIndex, Factory);

        Assert.AreEqual(1L, result.NetworkEdge.SrcTid);
        Assert.AreEqual(1L, result.NetworkEdge.VonRef);
        Assert.AreEqual(2L, result.NetworkEdge.NachRef);
        Assert.AreEqual("topologielinie", result.NetworkEdge.Linetype);
        Assert.AreEqual(0d, result.NetworkEdge.DiffStart);
        Assert.AreEqual(0d, result.NetworkEdge.DiffEnd);
        Assert.AreEqual(new Coordinate(0.05, 0), result.NetworkEdge.Geom.StartPoint.Coordinate);
        Assert.AreEqual(new Coordinate(9.95, 0), result.NetworkEdge.Geom.EndPoint.Coordinate);
        Assert.IsEmpty(result.ExtraEdges);
    }

    [TestMethod]
    public void ComputeLeitungEdgesReturnsUnalteredLeitungWhenBothRefsAreMissing()
    {
        var verlauf = Factory.CreateLineString(new[] { new Coordinate(0, 0), new Coordinate(10, 0) });
        var row = new LeitungRow(Tid: 1, VonRef: null, NachRef: null, Verlauf: verlauf);
        var knotenIndex = new Dictionary<long, KnotenInfo>
        {
            [1] = new KnotenInfo(new Coordinate(-5, 0), "Pumpwerk"),
            [2] = new KnotenInfo(new Coordinate(20, 0), "Pumpwerk"),
        };

        var result = NetworkTopologyPatcher.ComputeLeitungEdges(row, knotenIndex, Factory);

        Assert.AreSame(verlauf, result.NetworkEdge.Geom);
        Assert.AreEqual(0d, result.NetworkEdge.DiffStart);
        Assert.AreEqual(0d, result.NetworkEdge.DiffEnd);
        Assert.AreEqual(1L, result.NetworkEdge.SrcTid);
        Assert.IsNull(result.NetworkEdge.VonRef);
        Assert.IsNull(result.NetworkEdge.NachRef);
        Assert.AreEqual("topologielinie", result.NetworkEdge.Linetype);
        Assert.IsEmpty(result.ExtraEdges);
    }

    [TestMethod]
    [DataRow(null, 2L, DisplayName = "VonRef is null: only end connector is built")]
    [DataRow(1L, null, DisplayName = "NachRef is null: only start connector is built")]
    public void ComputeLeitungEdgesBuildsConnectorOnSideWhereRefIsPresent(long? vonRefBoxed, long? nachRefBoxed)
    {
        var vonRef = vonRefBoxed;
        var nachRef = nachRefBoxed;
        var verlauf = Factory.CreateLineString(new[] { new Coordinate(0, 0), new Coordinate(10, 0) });
        var row = new LeitungRow(Tid: 1, VonRef: vonRef, NachRef: nachRef, Verlauf: verlauf);
        var knotenIndex = new Dictionary<long, KnotenInfo>
        {
            [1] = new KnotenInfo(new Coordinate(-5, 0), "Pumpwerk"),
            [2] = new KnotenInfo(new Coordinate(20, 0), "Pumpwerk"),
        };

        var result = NetworkTopologyPatcher.ComputeLeitungEdges(row, knotenIndex, Factory);

        Assert.AreEqual(1L, result.NetworkEdge.SrcTid);
        Assert.AreEqual(vonRef, result.NetworkEdge.VonRef);
        Assert.AreEqual(nachRef, result.NetworkEdge.NachRef);
        Assert.AreEqual("topologielinie", result.NetworkEdge.Linetype);
        Assert.HasCount(1, result.ExtraEdges);

        if (vonRef is not null)
        {
            // Start connector built from VonRef (-5, 0) to verlauf start (0, 0).
            Assert.AreEqual(5d, result.NetworkEdge.DiffStart);
            Assert.AreEqual(0d, result.NetworkEdge.DiffEnd);
            Assert.AreEqual(new Coordinate(-5, 0), result.NetworkEdge.Geom.StartPoint.Coordinate);
            Assert.AreEqual(new Coordinate(10, 0), result.NetworkEdge.Geom.EndPoint.Coordinate);
        }
        else
        {
            // End connector built from verlauf end (10, 0) to NachRef (20, 0).
            Assert.AreEqual(0d, result.NetworkEdge.DiffStart);
            Assert.AreEqual(10d, result.NetworkEdge.DiffEnd);
            Assert.AreEqual(new Coordinate(0, 0), result.NetworkEdge.Geom.StartPoint.Coordinate);
            Assert.AreEqual(new Coordinate(20, 0), result.NetworkEdge.Geom.EndPoint.Coordinate);
        }
    }

    [TestMethod]
    public void ComputeLeitungEdgesSkipsConnectorsWhenKnotenHaveInvalidFunktion()
    {
        var verlauf = Factory.CreateLineString(new[] { new Coordinate(5, 0), new Coordinate(15, 0) });
        var row = new LeitungRow(Tid: 1, VonRef: 1, NachRef: 2, Verlauf: verlauf);
        var knotenIndex = new Dictionary<long, KnotenInfo>
        {
            [1] = new KnotenInfo(new Coordinate(0, 0), "Kontroll_Einsteigschacht"),
            [2] = new KnotenInfo(new Coordinate(20, 0), "Leitungsknoten"),
        };

        var result = NetworkTopologyPatcher.ComputeLeitungEdges(row, knotenIndex, Factory);

        Assert.AreSame(verlauf, result.NetworkEdge.Geom);
        Assert.AreEqual(0d, result.NetworkEdge.DiffStart);
        Assert.AreEqual(0d, result.NetworkEdge.DiffEnd);
        Assert.IsEmpty(result.ExtraEdges);
    }

    [TestMethod]
    public void ComputeLeitungEdgesReturnsUnalteredLeitungWhenKnotenHasNullFunktion()
    {
        var verlauf = Factory.CreateLineString(new[] { new Coordinate(5, 0), new Coordinate(15, 0) });
        var row = new LeitungRow(Tid: 1, VonRef: 1, NachRef: 2, Verlauf: verlauf);
        var knotenIndex = new Dictionary<long, KnotenInfo>
        {
            [1] = new KnotenInfo(new Coordinate(0, 0), Funktion: null),
            [2] = new KnotenInfo(new Coordinate(20, 0), Funktion: null),
        };

        var result = NetworkTopologyPatcher.ComputeLeitungEdges(row, knotenIndex, Factory);

        Assert.AreSame(verlauf, result.NetworkEdge.Geom);
        Assert.IsEmpty(result.ExtraEdges);
    }

    [TestMethod]
    [DataRow(null, 2L, DisplayName = "KnotenRef is null")]
    [DataRow(1L, null, DisplayName = "NachRef is null")]
    [DataRow(999L, 2L, DisplayName = "KnotenRef references unknown Knoten")]
    [DataRow(1L, 999L, DisplayName = "NachRef references unknown Knoten")]
    public void ComputeUeberlaufFoerderaggregatEdgesReturnsNullWhenRefDoesNotResolve(long? knotenRef, long? nachRef)
    {
        var row = new UeberlaufFoerderaggregatRow(Tid: 100, KnotenRef: knotenRef, NachRef: nachRef);
        var knotenIndex = new Dictionary<long, KnotenInfo>
        {
            [1] = new KnotenInfo(new Coordinate(0, 0), "Pumpwerk"),
            [2] = new KnotenInfo(new Coordinate(10, 0), "Pumpwerk"),
        };

        var result = NetworkTopologyPatcher.ComputeUeberlaufFoerderaggregatEdges(row, knotenIndex, Factory);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void ComputeUeberlaufFoerderaggregatEdgesBuildsDirectSegmentBetweenKnoten()
    {
        var row = new UeberlaufFoerderaggregatRow(Tid: 100, KnotenRef: 1, NachRef: 2);
        var knotenIndex = new Dictionary<long, KnotenInfo>
        {
            [1] = new KnotenInfo(new Coordinate(0, 0), "Pumpwerk"),
            [2] = new KnotenInfo(new Coordinate(10, 0), "Pumpwerk"),
        };

        var result = NetworkTopologyPatcher.ComputeUeberlaufFoerderaggregatEdges(row, knotenIndex, Factory);

        Assert.IsNotNull(result);
        Assert.AreEqual("ueberlauf_foerderaggregat", result!.NetworkEdge.Linetype);
        Assert.AreEqual(100L, result.NetworkEdge.SrcTid);
        Assert.AreEqual(new Coordinate(0, 0), result.NetworkEdge.Geom.StartPoint.Coordinate);
        Assert.AreEqual(new Coordinate(10, 0), result.NetworkEdge.Geom.EndPoint.Coordinate);
        Assert.IsNull(result.NetworkEdge.DiffStart);
        Assert.IsNull(result.NetworkEdge.DiffEnd);
        Assert.HasCount(1, result.ExtraEdges);
        Assert.AreEqual(10.0, result.ExtraEdges[0].Diff, 1e-9);
        Assert.AreEqual(100, result.ExtraEdges[0].SrcTid);
        Assert.IsNull(result.ExtraEdges[0].TidPipe);
        Assert.AreEqual("ueberlauf_foerderaggregat", result.ExtraEdges[0].Linetype);
    }

    [TestMethod]
    public async Task RunAsyncProcessesOneLeitungEndToEnd()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        MinimalNetworkTopologyGeoPackage.CreateSchema(connection);
        InsertKnoten(connection, tid: 1, x: 0, y: 0);
        InsertKnoten(connection, tid: 2, x: 10, y: 0);
        InsertLeitung(connection, tid: 10, vonRef: 1, nachRef: 2, new Coordinate(0, 0), new Coordinate(10, 0));

        var patcher = new NetworkTopologyPatcher(connection, NullLogger.Instance);
        await patcher.RunAsync(CancellationToken.None);

        Assert.AreEqual(1, GetCount(connection, TopologyOutputTables.NetworkEdgesTable));
        Assert.AreEqual(0, GetCount(connection, TopologyOutputTables.ExtraEdgesTable));

        // The new layers must be registred in the correct geopackage metadata tables
        Assert.AreEqual(1, GetScalar(connection, $"SELECT COUNT(*) FROM gpkg_contents WHERE table_name = '{TopologyOutputTables.NetworkEdgesTable}' AND data_type = 'features'"));
        Assert.AreEqual(1, GetScalar(connection, $"SELECT COUNT(*) FROM gpkg_contents WHERE table_name = '{TopologyOutputTables.ExtraEdgesTable}' AND data_type = 'features'"));
        Assert.AreEqual(1, GetScalar(connection, $"SELECT COUNT(*) FROM gpkg_geometry_columns WHERE table_name = '{TopologyOutputTables.NetworkEdgesTable}' AND geometry_type_name = 'LINESTRING'"));
        Assert.AreEqual(1, GetScalar(connection, $"SELECT COUNT(*) FROM gpkg_geometry_columns WHERE table_name = '{TopologyOutputTables.ExtraEdgesTable}' AND geometry_type_name = 'LINESTRING'"));
    }

    [TestMethod]
    public async Task RunAsyncProcessesOneAggregatEndToEnd()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        MinimalNetworkTopologyGeoPackage.CreateSchema(connection);
        InsertKnoten(connection, tid: 1, x: 0, y: 0);
        InsertKnoten(connection, tid: 2, x: 10, y: 0);
        InsertUeberlaufFoerderaggregat(connection, tid: 100, knotenRef: 1, nachRef: 2);

        var patcher = new NetworkTopologyPatcher(connection, NullLogger.Instance);
        await patcher.RunAsync(CancellationToken.None);

        Assert.AreEqual(1, GetCount(connection, TopologyOutputTables.NetworkEdgesTable));
        Assert.AreEqual(1, GetCount(connection, TopologyOutputTables.ExtraEdgesTable));
    }

    [TestMethod]
    public async Task RunAsyncWritesConnectorToExtraEdgesTableForGappedLeitung()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        MinimalNetworkTopologyGeoPackage.CreateSchema(connection);
        InsertKnoten(connection, tid: 1, x: 0, y: 0);
        InsertKnoten(connection, tid: 2, x: 10, y: 0);
        InsertKnotenAttributes(connection, tid: 1, funktion: "Pumpwerk");
        InsertKnotenAttributes(connection, tid: 2, funktion: "Pumpwerk");
        InsertLeitung(connection, tid: 10, vonRef: 1, nachRef: 2, new Coordinate(5, 0), new Coordinate(10, 0));

        var patcher = new NetworkTopologyPatcher(connection, NullLogger.Instance);
        await patcher.RunAsync(CancellationToken.None);

        Assert.AreEqual(1, GetCount(connection, TopologyOutputTables.NetworkEdgesTable));
        Assert.AreEqual(1, GetCount(connection, TopologyOutputTables.ExtraEdgesTable));
    }

    [TestMethod]
    public async Task RunAsyncCreatesEmptyOutputTablesWhenNoInputRowsExist()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        MinimalNetworkTopologyGeoPackage.CreateSchema(connection);

        var patcher = new NetworkTopologyPatcher(connection, NullLogger.Instance);
        await patcher.RunAsync(CancellationToken.None);

        Assert.AreEqual(0, GetCount(connection, TopologyOutputTables.NetworkEdgesTable));
        Assert.AreEqual(0, GetCount(connection, TopologyOutputTables.ExtraEdgesTable));
        Assert.AreEqual(1, GetScalar(connection, $"SELECT COUNT(*) FROM gpkg_contents WHERE table_name = '{TopologyOutputTables.NetworkEdgesTable}' AND data_type = 'features'"));
        Assert.AreEqual(1, GetScalar(connection, $"SELECT COUNT(*) FROM gpkg_contents WHERE table_name = '{TopologyOutputTables.ExtraEdgesTable}' AND data_type = 'features'"));
    }

    private static void InsertKnotenAttributes(SqliteConnection connection, long tid, string? funktion)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "INSERT INTO knoten (t_id, funktion) VALUES (@tid, @funktion)";
        cmd.Parameters.AddWithValue("@tid", tid);
        cmd.Parameters.AddWithValue("@funktion", (object?)funktion ?? DBNull.Value);
        cmd.ExecuteNonQuery();
    }

#pragma warning disable CA2100
    private static int GetScalar(SqliteConnection connection, string sql)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        return Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture);
    }
#pragma warning restore CA2100

    private static void InsertKnoten(SqliteConnection connection, long tid, double x, double y)
    {
        var blob = GeoPackageGeometryCodec.WriteGeometry(Factory.CreatePoint(new Coordinate(x, y)), Srid);
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "INSERT INTO knoten_lage (t_id, lage) VALUES (@tid, @geom)";
        cmd.Parameters.AddWithValue("@tid", tid);
        cmd.Parameters.AddWithValue("@geom", blob);
        cmd.ExecuteNonQuery();
    }

    private static void InsertLeitung(SqliteConnection connection, long tid, long? vonRef, long? nachRef, params Coordinate[] vertices)
    {
        var blob = GeoPackageGeometryCodec.WriteGeometry(Factory.CreateLineString(vertices), Srid);
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "INSERT INTO leitung (t_id, knoten_vonref, knoten_nachref, verlauf) VALUES (@tid, @von, @nach, @geom)";
        cmd.Parameters.AddWithValue("@tid", tid);
        cmd.Parameters.AddWithValue("@von", (object?)vonRef ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@nach", (object?)nachRef ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@geom", blob);
        cmd.ExecuteNonQuery();
    }

    private static void InsertUeberlaufFoerderaggregat(SqliteConnection connection, long tid, long? knotenRef, long? nachRef)
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
#pragma warning restore CA2100
}
