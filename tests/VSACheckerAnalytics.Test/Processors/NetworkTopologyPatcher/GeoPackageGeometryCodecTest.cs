using NetTopologySuite.Geometries;
using VsaCheckerAnalytics.Processors.NetworkTopologyPatcher;

namespace VsaCheckerAnalytics.Processors.NetworkTopologyPatcher;

[TestClass]
public sealed class GeoPackageGeometryCodecTest
{
    private static readonly GeometryFactory Factory =
        NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory();

    [TestMethod]
    public void RoundTripLineStringPreservesCoordinatesAndSrid()
    {
        var line = Factory.CreateLineString(new[]
        {
            new Coordinate(2_600_000.5, 1_200_000.25),
            new Coordinate(2_600_100.5, 1_200_050.25),
            new Coordinate(2_600_200.5, 1_200_100.25),
        });

        var blob = GeoPackageGeometryCodec.WriteGeometry(line, 2056);
        var decoded = GeoPackageGeometryCodec.ReadGeometry(blob, out var srid);

        Assert.AreEqual(2056, srid);
        Assert.IsInstanceOfType<LineString>(decoded);
        var decodedLine = (LineString)decoded;
        Assert.AreEqual(line.NumPoints, decodedLine.NumPoints);
        for (var i = 0; i < line.NumPoints; i++)
        {
            Assert.AreEqual(line.Coordinates[i].X, decodedLine.Coordinates[i].X, 1e-9);
            Assert.AreEqual(line.Coordinates[i].Y, decodedLine.Coordinates[i].Y, 1e-9);
        }
    }

    [TestMethod]
    public void RoundTripPointPreservesCoordinatesAndSrid()
    {
        var point = Factory.CreatePoint(new Coordinate(2_600_123.45, 1_200_678.9));

        var blob = GeoPackageGeometryCodec.WriteGeometry(point, 2056);
        var decoded = GeoPackageGeometryCodec.ReadGeometry(blob, out var srid);

        Assert.AreEqual(2056, srid);
        Assert.IsInstanceOfType<Point>(decoded);
        Assert.AreEqual(point.X, ((Point)decoded).X, 1e-9);
        Assert.AreEqual(point.Y, ((Point)decoded).Y, 1e-9);
    }

    [TestMethod]
    public void ReadGeometryRejectsBlobWithoutMagic()
    {
        var bogus = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        Assert.ThrowsExactly<InvalidDataException>(
            () => GeoPackageGeometryCodec.ReadGeometry(bogus, out _));
    }
}
