using Geopilot.PipelineCore.Pipeline;

namespace VsaCheckerAnalytics.Processors.GeopackageGeneration;

/// <summary>
/// Result of the <see cref="GeopackageGenerationProcess"/>.
/// </summary>
public sealed class GeopackageGenerationResult
{
    /// <summary>
    /// The generated GeoPackage, enriched with checker data, error matrix and analysis views,
    /// or <see langword="null"/> when the INTERLIS import failed.
    /// </summary>
    public IPipelineFile? GeneratedGeopackage { get; set; }

    /// <summary>
    /// A localized status message describing the outcome.
    /// </summary>
    public required LocalizedText StatusMessage { get; set; }
}
