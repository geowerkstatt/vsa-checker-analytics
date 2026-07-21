using Geopilot.PipelineCore.Pipeline;

namespace VsaCheckerAnalytics.Processors.NetworkTopologyPatcher;

/// <summary>
/// Result of the <see cref="NetworkTopologyPatcherProcess"/>.
/// </summary>
public sealed class NetworkTopologyPatcherResult
{
    /// <summary>
    /// The GeoPackage copy enriched with the reconstructed network topology layers,
    /// or <see langword="null"/> when the reconstruction failed.
    /// </summary>
    public IPipelineFile? PatchedGeopackage { get; set; }

    /// <summary>
    /// A localized status message describing the outcome.
    /// </summary>
    public required LocalizedText StatusMessage { get; set; }
}
