using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System.Buffers.Binary;

namespace VsaCheckerAnalytics.Processors.NetworkTopologyPatcher;

/// <summary>
/// Reads and writes GeoPackage Binary (GPB) blobs: an 8–72 byte GPKG header followed by
/// standard OGC Well-Known Binary. The header carries the SRS ID (the WKB body does not).
/// </summary>
internal static class GeoPackageGeometryCodec
{
    private const byte MagicG = 0x47;
    private const byte MagicP = 0x50;

    /// <summary>
    /// Decodes a GPB blob into a NetTopologySuite geometry and returns the SRS ID from
    /// the header.
    /// </summary>
    /// <param name="blob">Raw bytes as stored in a GeoPackage geometry column.</param>
    /// <param name="srid">The SRS ID parsed from the GPB header.</param>
    /// <returns>The decoded geometry, with <see cref="Geometry.SRID"/> set.</returns>
    internal static Geometry ReadGeometry(byte[] blob, out int srid)
    {
        ArgumentNullException.ThrowIfNull(blob);
        if (blob.Length < 8 || blob[0] != MagicG || blob[1] != MagicP)
        {
            throw new InvalidDataException("Not a GeoPackage geometry blob (missing 'GP' magic).");
        }

        var flags = blob[3];
        var headerLittleEndian = (flags & 0x01) != 0;
        var envelopeCode = (flags >> 1) & 0x07;
        var envelopeBytes = envelopeCode switch
        {
            0 => 0,
            1 => 32,
            2 => 48,
            3 => 48,
            4 => 64,
            _ => throw new InvalidDataException($"Unsupported GPB envelope code {envelopeCode}."),
        };

        var sridSlice = blob.AsSpan(4, 4);
        srid = headerLittleEndian
            ? BinaryPrimitives.ReadInt32LittleEndian(sridSlice)
            : BinaryPrimitives.ReadInt32BigEndian(sridSlice);

        var wkbOffset = 8 + envelopeBytes;
        if (blob.Length < wkbOffset)
        {
            throw new InvalidDataException("GPB blob shorter than declared envelope size.");
        }

        var wkb = new byte[blob.Length - wkbOffset];
        Buffer.BlockCopy(blob, wkbOffset, wkb, 0, wkb.Length);

        var reader = new WKBReader();
        var geometry = reader.Read(wkb);
        geometry.SRID = srid;
        return geometry;
    }

    /// <summary>
    /// Encodes a geometry as a GeoPackage Binary blob using a minimal (envelope-less)
    /// little-endian header.
    /// </summary>
    /// <param name="geometry">The geometry to encode.</param>
    /// <param name="srid">The SRS ID to write into the header.</param>
    /// <returns>The GPB blob, ready to store in a GeoPackage geometry column.</returns>
    internal static byte[] WriteGeometry(Geometry geometry, int srid)
    {
        ArgumentNullException.ThrowIfNull(geometry);

        var writer = new WKBWriter(ByteOrder.LittleEndian);
        var wkb = writer.Write(geometry);

        var blob = new byte[8 + wkb.Length];
        blob[0] = MagicG;
        blob[1] = MagicP;
        blob[2] = 0;            // version
        blob[3] = 0x01;         // flags: little-endian header, envelope code 0, non-empty
        BinaryPrimitives.WriteInt32LittleEndian(blob.AsSpan(4, 4), srid);
        Buffer.BlockCopy(wkb, 0, blob, 8, wkb.Length);
        return blob;
    }
}
