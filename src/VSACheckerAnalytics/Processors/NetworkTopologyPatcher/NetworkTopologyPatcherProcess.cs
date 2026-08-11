using Geopilot.PipelineCore.Pipeline;
using Geopilot.PipelineCore.Pipeline.Process;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Globalization;

namespace VsaCheckerAnalytics.Processors.NetworkTopologyPatcher;

/// <summary>
/// Pipeline process that takes a prepared GeoPackage and produces an enriched copy with
/// two additional feature layers reconstructing the sewer network topology:
/// <list type="bullet">
///   <item><c>ca_topo_network_edges</c>: one continuous line per input edge, joining
///   the referenced upstream and downstream nodes.</item>
///   <item><c>ca_topo_extra_edges</c>: only the newly synthesised connector segments
///   (plus the pump/weir connectors from <c>ueberlauf_foerderaggregat</c>).</item>
/// </list>
/// The input file is left untouched; all writes go into a fresh copy.
/// </summary>
public sealed class NetworkTopologyPatcherProcess
{
    private const string PatchedGeopackageName = "gpkg-with-topology";

    private static readonly LocalizedText TopologyStatusMessageFormat = new Dictionary<string, string>
    {
        { "de", "Netzwerk-Topologie aufgebaut: {0} Kanten erstellt, {1} übersprungen." },
        { "fr", "Topologie du réseau construite : {0} arêtes créées, {1} ignorées." },
        { "it", "Topologia di rete costruita: {0} archi creati, {1} ignorati." },
        { "en", "Network topology built: {0} edges created, {1} skipped." },
    };

    private readonly IPipelineFileManager pipelineFileManager;
    private readonly ILogger logger;

    /// <summary>
    /// Initializes a new <see cref="NetworkTopologyPatcherProcess"/>.
    /// </summary>
    /// <param name="pipelineFileManager">Pipeline file manager used to allocate the output GeoPackage.</param>
    /// <param name="logger">Logger.</param>
    public NetworkTopologyPatcherProcess(
        IPipelineFileManager pipelineFileManager,
        ILogger logger)
    {
        this.pipelineFileManager = pipelineFileManager ?? throw new ArgumentNullException(nameof(pipelineFileManager));
        this.logger = logger ?? NullLogger.Instance;
    }

    /// <summary>
    /// Copies <paramref name="geoPackage"/> and writes the two topology layers into the copy.
    /// The original input file is left untouched.
    /// </summary>
    /// <param name="geoPackage">Prepared input GeoPackage containing <c>leitung</c>, <c>knoten_lage</c>, and (optionally) <c>ueberlauf_foerderaggregat</c>.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="NetworkTopologyPatcherResult"/> with the enriched GeoPackage copy and a localized status message.</returns>
    [PipelineProcessRun]
    public async Task<NetworkTopologyPatcherResult> RunAsync(
        IPipelineFile geoPackage,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(geoPackage);

        var target = await pipelineFileManager.CreateWritableCopyAsync(geoPackage, PatchedGeopackageName, cancellationToken);
        var path = await target.GetLocalPathAsync(cancellationToken);

        NetworkTopologyResult topologyResult;
        using (var connection = OpenGeoPackage(path))
        {
            var patcher = new NetworkTopologyPatcher(connection, logger);
            topologyResult = await patcher.RunAsync(cancellationToken).ConfigureAwait(false);
        }

        logger.LogInformation("NetworkTopologyPatcherProcess produced GeoPackage <{FileName}>.", target.OriginalFileName);

        var statusMessage = TopologyStatusMessageFormat
            .Map(msg => string.Format(CultureInfo.InvariantCulture, msg, topologyResult.EdgesBuilt, topologyResult.LeitungenSkipped));

        return new NetworkTopologyPatcherResult
        {
            PatchedGeopackage = target,
            StatusMessage = statusMessage,
        };
    }

    private static SqliteConnection OpenGeoPackage(string path)
    {
        var connection = new SqliteConnection($"Data Source={path};Pooling=false");
        connection.Open();
        return connection;
    }
}
