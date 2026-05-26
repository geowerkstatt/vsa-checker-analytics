using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Linemerge;
using System.Diagnostics.CodeAnalysis;

namespace VsaCheckerAnalytics.Processors.NetworkTopologyPatcher;

/// <summary>
/// Topological completion of the sewer network: ensures every <c>leitung</c> linestring
/// runs exactly from its referenced upstream <c>knoten_lage</c> point to its downstream
/// one, and materialises pump/weir aggregates (<c>ueberlauf_foerderaggregat</c>) as
/// direct line segments. Writes results to <c>ca_topo_network_edges</c> and
/// <c>ca_topo_extra_edges</c>.
/// </summary>
internal sealed class NetworkTopologyPatcher
{
    /// <summary>Minimum connector length in metres — shorter connectors are discarded.</summary>
    internal const double MinConnectorLength = 0.10;

    /// <summary>SRID of all processed VSA data — Swiss LV95 (EPSG:2056).</summary>
    internal const int Srid = 2056;

    /// <summary>
    /// Knoten <c>funktion</c> values that are considered valid connector targets. Connectors from
    /// <c>leitung</c> endpoints are only created when the referenced Knoten's <c>funktion</c>
    /// is in this set. Otherwise the verlauf endpoint is kept as-is on that side.
    /// </summary>
    internal static readonly IReadOnlySet<string> ValidConnectorFunktionen = new HashSet<string>(StringComparer.Ordinal)
    {
        "abflussloseGrube",
        "Absturzbauwerk",
        "Abwasserfaulraum",
        "Duekerkammer",
        "Duekeroberhaupt",
        "Faulgrube",
        "Gelaendemulde",
        "Geschiebefang",
        "Guellegrube",
        "Klaergrube",
        "Regenbecken_Durchlaufbecken",
        "Regenbecken_Fangbecken",
        "Regenbecken_Fangkanal",
        "Regenbecken_Regenklaerbecken",
        "Regenbecken_Regenrueckhaltebecken",
        "Regenbecken_Regenrueckhaltekanal",
        "Regenbecken_Stauraumkanal",
        "Regenbecken_Verbundbecken",
        "Wirbelfallschacht",
        "Pumpwerk",
        "Trennbauwerk",
        "Regenueberlauf",
    };

    private readonly SqliteConnection connection;
    private readonly ILogger logger;
    private readonly GeometryFactory geometryFactory = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory();

    internal NetworkTopologyPatcher(SqliteConnection connection, ILogger logger)
    {
        this.connection = connection;
        this.logger = logger;
    }

    internal async Task RunAsync(CancellationToken cancellationToken)
    {
        TopologyOutputTables.Create(connection, Srid);

        var knotenIndex = LoadKnotenIndex();
        logger.LogDebug("Loaded {KnotenCount} Knoten from knoten_lage (SRID {Srid}).", knotenIndex.Count, Srid);

        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        using var insertNetworkEdgeCommand = CreateInsertNetworkCommand();
        using var insertExtraEdgeCommand = CreateInsertExtraCommand();

        var leitungCount = 0;
        foreach (var row in ReadLeitungen())
        {
            cancellationToken.ThrowIfCancellationRequested();
            var computedEdges = ComputeLeitungEdges(row, knotenIndex, geometryFactory);
            WriteComputedEdgesToGeoPackage(insertNetworkEdgeCommand, insertExtraEdgeCommand, computedEdges);
            leitungCount++;
        }

        var aggregatCount = 0;
        foreach (var row in ReadUeberlaufFoerderaggregate())
        {
            cancellationToken.ThrowIfCancellationRequested();
            var computedEdges = ComputeUeberlaufFoerderaggregatEdges(row, knotenIndex, geometryFactory);
            if (computedEdges is null)
            {
                continue;
            }

            WriteComputedEdgesToGeoPackage(insertNetworkEdgeCommand, insertExtraEdgeCommand, computedEdges);
            aggregatCount++;
        }

        await transaction.CommitAsync(cancellationToken);

        logger.LogInformation(
            "Network topology patch complete: {Leitungen} leitung rows + {Aggregate} ueberlauf_foerderaggregat rows processed.",
            leitungCount,
            aggregatCount);
    }

    [SuppressMessage("Security", "CA2100", Justification = "SQL is built from internal table-name constants, not user input.")]
    private SqliteCommand CreateInsertNetworkCommand()
    {
        var command = connection.CreateCommand();
        command.CommandText = $"""
            INSERT INTO {TopologyOutputTables.NetworkEdgesTable}
                (src_tid, knoten_vonref, knoten_nachref, diff_start, diff_end, linetype, geom)
            VALUES (@src_tid, @knoten_vonref, @knoten_nachref, @diff_start, @diff_end, @linetype, @geom)
            """;
        command.Parameters.Add("@src_tid", SqliteType.Integer);
        command.Parameters.Add("@knoten_vonref", SqliteType.Integer);
        command.Parameters.Add("@knoten_nachref", SqliteType.Integer);
        command.Parameters.Add("@diff_start", SqliteType.Real);
        command.Parameters.Add("@diff_end", SqliteType.Real);
        command.Parameters.Add("@linetype", SqliteType.Text);
        command.Parameters.Add("@geom", SqliteType.Blob);
        return command;
    }

    [SuppressMessage("Security", "CA2100", Justification = "SQL is built from internal table-name constants, not user input.")]
    private SqliteCommand CreateInsertExtraCommand()
    {
        var command = connection.CreateCommand();
        command.CommandText = $"""
            INSERT INTO {TopologyOutputTables.ExtraEdgesTable}
                (src_tid, tid_pipe, diff, linetype, geom)
            VALUES (@src_tid, @tid_pipe, @diff, @linetype, @geom)
            """;
        command.Parameters.Add("@src_tid", SqliteType.Integer);
        command.Parameters.Add("@tid_pipe", SqliteType.Integer);
        command.Parameters.Add("@diff", SqliteType.Real);
        command.Parameters.Add("@linetype", SqliteType.Text);
        command.Parameters.Add("@geom", SqliteType.Blob);
        return command;
    }

    private void WriteComputedEdgesToGeoPackage(SqliteCommand insertNetwork, SqliteCommand insertExtra, ComputedEdges edges)
    {
        WriteNetworkEdges(insertNetwork, edges.NetworkEdge);
        foreach (var extra in edges.ExtraEdges)
        {
            WriteExtraEdges(insertExtra, extra);
        }
    }

    private void WriteNetworkEdges(SqliteCommand insertNetwork, NetworkEdge edge)
    {
        insertNetwork.Parameters["@src_tid"].Value = edge.SrcTid;
        insertNetwork.Parameters["@knoten_vonref"].Value = (object?)edge.VonRef ?? DBNull.Value;
        insertNetwork.Parameters["@knoten_nachref"].Value = (object?)edge.NachRef ?? DBNull.Value;
        insertNetwork.Parameters["@diff_start"].Value = (object?)edge.DiffStart ?? DBNull.Value;
        insertNetwork.Parameters["@diff_end"].Value = (object?)edge.DiffEnd ?? DBNull.Value;
        insertNetwork.Parameters["@linetype"].Value = edge.Linetype;
        insertNetwork.Parameters["@geom"].Value = GeoPackageGeometryCodec.WriteGeometry(edge.Geom, Srid);
        insertNetwork.ExecuteNonQuery();
    }

    private void WriteExtraEdges(SqliteCommand insertExtra, ExtraEdge edge)
    {
        insertExtra.Parameters["@src_tid"].Value = edge.SrcTid;
        insertExtra.Parameters["@tid_pipe"].Value = (object?)edge.TidPipe ?? DBNull.Value;
        insertExtra.Parameters["@diff"].Value = edge.Diff;
        insertExtra.Parameters["@linetype"].Value = edge.Linetype;
        insertExtra.Parameters["@geom"].Value = GeoPackageGeometryCodec.WriteGeometry(edge.Geom, Srid);
        insertExtra.ExecuteNonQuery();
    }

    /// <summary>
    /// Creates a lookup table (index) with all Knoten, keyed by the Tid of each Knoten.
    /// </summary>
    private Dictionary<long, KnotenInfo> LoadKnotenIndex()
    {
        var index = new Dictionary<long, KnotenInfo>();

        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT kl.t_id, kl.lage, k.funktion
            FROM knoten_lage kl
            LEFT JOIN knoten k ON k.t_id = kl.t_id
            WHERE kl.lage IS NOT NULL
            """;
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            if (reader.IsDBNull(0) || reader.IsDBNull(1))
            {
                continue;
            }

            var tid = reader.GetInt64(0);
            var blob = (byte[])reader.GetValue(1);
            Geometry geom;
            try
            {
                geom = GeoPackageGeometryCodec.ReadGeometry(blob, out _);
            }
            catch (InvalidDataException ex)
            {
                logger.LogWarning("knoten_lage T_Id={Tid}: failed to decode geometry blob — skipped ({Message})", tid, ex.Message);
                continue;
            }

            if (geom is not Point point)
            {
                logger.LogWarning("knoten_lage T_Id={Tid}: geometry is {Type}, expected Point — skipped", tid, geom.GeometryType);
                continue;
            }

            var funktion = reader.IsDBNull(2) ? null : reader.GetString(2);
            index[tid] = new KnotenInfo(point.Coordinate, funktion);
        }

        return index;
    }

    /// <summary>
    /// Streams rows from the <c>leitung</c> table with the <c>verlauf</c> blob already
    /// decoded into a <see cref="LineString"/>. Rows with a NULL key or geometry, an
    /// undecodable blob, or a non LineString geometry are logged and skipped.
    /// </summary>
    private IEnumerable<LeitungRow> ReadLeitungen()
    {
        using var select = connection.CreateCommand();
        select.CommandText = "SELECT t_id, knoten_vonref, knoten_nachref, verlauf FROM leitung";

        using var reader = select.ExecuteReader();
        while (reader.Read())
        {
            if (reader.IsDBNull(0) || reader.IsDBNull(3))
            {
                continue;
            }

            var tid = reader.GetInt64(0);
            var vonRef = reader.IsDBNull(1) ? (long?)null : reader.GetInt64(1);
            var nachRef = reader.IsDBNull(2) ? (long?)null : reader.GetInt64(2);
            var blob = (byte[])reader.GetValue(3);

            Geometry geom;
            try
            {
                geom = GeoPackageGeometryCodec.ReadGeometry(blob, out _);
            }
            catch (InvalidDataException ex)
            {
                logger.LogWarning("leitung T_Id={Tid}: failed to decode 'verlauf' — skipped ({Message})", tid, ex.Message);
                continue;
            }

            if (geom is not LineString line)
            {
                logger.LogWarning("leitung T_Id={Tid}: 'verlauf' is {Type}, expected LineString — skipped", tid, geom.GeometryType);
                continue;
            }

            yield return new LeitungRow(tid, vonRef, nachRef, line);
        }
    }

    /// <summary>
    /// Streams rows from the <c>ueberlauf_foerderaggregat</c> table. Yields nothing when
    /// the table is absent. Rows with a NULL key are logged and skipped.
    /// </summary>
    private IEnumerable<UeberlaufFoerderaggregatRow> ReadUeberlaufFoerderaggregate()
    {
        using var select = connection.CreateCommand();
        select.CommandText = "SELECT t_id, knotenref, knoten_nachref FROM ueberlauf_foerderaggregat";

        using var reader = select.ExecuteReader();
        while (reader.Read())
        {
            if (reader.IsDBNull(0))
            {
                continue;
            }

            var tid = reader.GetInt64(0);
            var knotenRef = reader.IsDBNull(1) ? (long?)null : reader.GetInt64(1);
            var nachRef = reader.IsDBNull(2) ? (long?)null : reader.GetInt64(2);

            yield return new UeberlaufFoerderaggregatRow(tid, knotenRef, nachRef);
        }
    }

    /// <summary>
    /// Looks at the referenced Knoten of the Leitung and builds connector segments if required.
    /// A connector on a given side is only built when the referenced Knoten resolves AND its
    /// <c>funktion</c> is in <see cref="ValidConnectorFunktionen"/>; otherwise the verlauf
    /// endpoint is kept as-is on that side.
    /// The created connector segments, or extra segments, are returned at <see cref="ComputedEdges.ExtraEdges"/>.
    /// A complete Leitung, consisting of the original Leitung and the extra segments merged into it, is also returned at <see cref="ComputedEdges.NetworkEdge"/>.
    /// </summary>
    internal static ComputedEdges ComputeLeitungEdges(
        LeitungRow row,
        Dictionary<long, KnotenInfo> knotenIndex,
        GeometryFactory geometryFactory)
    {
        const string Linetype = "topologielinie";

        var fromKnoten = ResolveKnoten(knotenIndex, row.VonRef);
        var toKnoten = ResolveKnoten(knotenIndex, row.NachRef);

        var startConnector = fromKnoten is not null && IsValidConnectorTarget(fromKnoten)
            ? BuildConnectorSegment(fromKnoten.Coord, row.Verlauf.StartPoint.Coordinate, geometryFactory)
            : null;
        var endConnector = toKnoten is not null && IsValidConnectorTarget(toKnoten)
            ? BuildConnectorSegment(row.Verlauf.EndPoint.Coordinate, toKnoten.Coord, geometryFactory)
            : null;

        var segments = new List<LineString>(3);
        var extras = new List<ExtraEdge>(2);

        if (startConnector is not null)
        {
            segments.Add(startConnector);
            extras.Add(new ExtraEdge(startConnector, row.Tid, row.Tid, startConnector.Length, Linetype));
        }

        segments.Add(row.Verlauf);

        if (endConnector is not null)
        {
            segments.Add(endConnector);
            extras.Add(new ExtraEdge(endConnector, row.Tid, row.Tid, endConnector.Length, Linetype));
        }

        var merged = MergeSegments(segments);

        return new ComputedEdges(
            new NetworkEdge(merged, row.Tid, row.VonRef, row.NachRef, startConnector?.Length ?? 0, endConnector?.Length ?? 0, Linetype),
            extras);
    }

    /// <summary>
    /// Pure topology computation for a single <see cref="UeberlaufFoerderaggregatRow"/>: resolves both
    /// referenced Knoten and materialises the pump/weir as a direct segment. Returns
    /// <see langword="null"/> when either Knoten reference is unresolved (caller skips the row).
    /// </summary>
    internal static ComputedEdges? ComputeUeberlaufFoerderaggregatEdges(
        UeberlaufFoerderaggregatRow row,
        Dictionary<long, KnotenInfo> knotenIndex,
        GeometryFactory geometryFactory)
    {
        const string Linetype = "ueberlauf_foerderaggregat";

        var fromKnoten = ResolveKnoten(knotenIndex, row.KnotenRef);
        var toKnoten = ResolveKnoten(knotenIndex, row.NachRef);
        if (fromKnoten is null || toKnoten is null)
        {
            return null;
        }

        var segment = geometryFactory.CreateLineString(new[] { fromKnoten.Coord, toKnoten.Coord });
        return new ComputedEdges(
            new NetworkEdge(segment, row.Tid, row.KnotenRef, row.NachRef, DiffStart: null, DiffEnd: null, Linetype),
            new[] { new ExtraEdge(segment, row.Tid, TidPipe: null, segment.Length, Linetype) });
    }

    /// <summary>
    /// Returns <see langword="true"/> when the Knoten's <c>funktion</c> is non-null and
    /// is in the allow-list (<see cref="ValidConnectorFunktionen"/>).
    /// </summary>
    private static bool IsValidConnectorTarget(KnotenInfo knoten) =>
        knoten.Funktion is not null && ValidConnectorFunktionen.Contains(knoten.Funktion);

    /// <summary>
    /// Builds a connector <see cref="LineString"/> between a Knoten and an original verlauf vertex,
    /// or returns <see langword="null"/> when the connector would be shorter than <see cref="MinConnectorLength"/>.
    /// </summary>
    private static LineString? BuildConnectorSegment(
        Coordinate a,
        Coordinate b,
        GeometryFactory geometryFactory)
    {
        var connector = geometryFactory.CreateLineString(new[] { a, b });
        return connector.Length >= MinConnectorLength ? connector : null;
    }

    private static LineString MergeSegments(List<LineString> segments)
    {
        if (segments.Count == 1)
        {
            return segments[0];
        }

        var merger = new LineMerger();
        foreach (var seg in segments)
        {
            merger.Add(seg);
        }

        var merged = merger.GetMergedLineStrings();
        if (merged.Count != 1)
        {
            throw new InvalidDataException($"The segments could not be merged into a single line.");
        }

        return (LineString)merged[0];
    }

    /// <summary>
    /// Resolves a Knoten reference to its <see cref="KnotenInfo"/>, or <see langword="null"/>
    /// when the reference is NULL or points to an unknown Knoten.
    /// </summary>
    private static KnotenInfo? ResolveKnoten(Dictionary<long, KnotenInfo> knotenIndex, long? refValue) =>
        refValue is long id && knotenIndex.TryGetValue(id, out var info) ? info : null;
}

/// <summary>
/// A Knoten entry in the in-memory index: its <c>knoten_lage</c> coordinate and (when available)
/// the <c>funktion</c> value joined from the <c>knoten</c> attribute table.
/// </summary>
internal sealed record KnotenInfo(Coordinate Coord, string? Funktion);

/// <summary>
/// One row of the <c>leitung</c> table with the <c>verlauf</c> blob already decoded
/// into a <see cref="LineString"/>.
/// </summary>
internal sealed record LeitungRow(long Tid, long? VonRef, long? NachRef, LineString Verlauf);

/// <summary>One row of the <c>ueberlauf_foerderaggregat</c> table.</summary>
internal sealed record UeberlaufFoerderaggregatRow(long Tid, long? KnotenRef, long? NachRef);

/// <summary>A row to insert into <c>ca_topo_network_edges</c>.</summary>
internal sealed record NetworkEdge(
    LineString Geom,
    long SrcTid,
    long? VonRef,
    long? NachRef,
    double? DiffStart,
    double? DiffEnd,
    string Linetype);

/// <summary>A row to insert into <c>ca_topo_extra_edges</c>.</summary>
internal sealed record ExtraEdge(
    LineString Geom,
    long SrcTid,
    long? TidPipe,
    double Diff,
    string Linetype);

/// <summary>
/// The result of computing one source row: the network edge plus any extra connector
/// edges.
/// </summary>
internal sealed record ComputedEdges(NetworkEdge NetworkEdge, IReadOnlyList<ExtraEdge> ExtraEdges);
