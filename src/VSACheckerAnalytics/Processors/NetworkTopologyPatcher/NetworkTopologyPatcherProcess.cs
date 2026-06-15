using Geopilot.PipelineCore.Pipeline;
using Geopilot.PipelineCore.Pipeline.Process;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

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
    private const string PatchedGpkgOutputKey = "patchedGeopackage";
    private const string PatchedGeopackageName = "gpkg-with-topology";

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
    /// <returns>A dictionary with the enriched GeoPackage copy under <c>patchedGeopackage</c>.</returns>
    [PipelineProcessRun]
    public async Task<Dictionary<string, object?>> RunAsync(
        IPipelineFile geoPackage,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(geoPackage);

        var (target, path) = await CopyGeoPackageAsync(geoPackage, PatchedGeopackageName, cancellationToken).ConfigureAwait(false);

        using (var connection = OpenGeoPackage(path))
        {
            var patcher = new NetworkTopologyPatcher(connection, logger);
            await patcher.RunAsync(cancellationToken).ConfigureAwait(false);
        }

        logger.LogInformation("NetworkTopologyPatcherProcess produced GeoPackage <{FileName}>.", target.OriginalFileName);

        return new Dictionary<string, object?>
        {
            { PatchedGpkgOutputKey, target },
        };
    }

    private async Task<(IPipelineFile File, string Path)> CopyGeoPackageAsync(
        IPipelineFile source, string name, CancellationToken cancellationToken)
    {
        var target = pipelineFileManager.GeneratePipelineFile(name, "gpkg");
        string path;
        await using (var sourceStream = source.OpenReadFileStream())
        await using (var targetStream = target.OpenWriteFileStream())
        {
            path = targetStream.Name;
            await sourceStream.CopyToAsync(targetStream, cancellationToken).ConfigureAwait(false);
        }

        return (target, path);
    }

    private static SqliteConnection OpenGeoPackage(string path)
    {
        var connection = new SqliteConnection($"Data Source={path};Pooling=false");
        connection.Open();
        return connection;
    }
}
