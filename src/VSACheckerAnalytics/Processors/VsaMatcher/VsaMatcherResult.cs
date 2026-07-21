using Geopilot.PipelineCore.Pipeline;

namespace VsaCheckerAnalytics.Processors.VsaMatcher;

/// <summary>
/// Result of the <see cref="VsaMatcherProcess"/>: the input files routed by semantic role,
/// the metadata extracted from the GEP transfer file, and the application resources loaded
/// for the matched model version.
/// </summary>
internal sealed class VsaMatcherResult
{
    /// <summary>
    /// The identified GEP transfer files. Exactly one entry indicates an unambiguous match.
    /// </summary>
    public required IPipelineFile[] Gep { get; set; }

    /// <summary>
    /// The data-model version of the GEP file (<c>"2020"</c> or <c>"2020.1"</c>), or
    /// <see langword="null"/> when no unambiguous match was found.
    /// </summary>
    public string? ModelVersion { get; set; }

    /// <summary>
    /// The language of the GEP file (<c>"DE"</c> or <c>"FR"</c>), or <see langword="null"/>
    /// when no unambiguous match was found.
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// The user-provided organisation tables found in the upload.
    /// </summary>
    public required IPipelineFile[] UserOrgTable { get; set; }

    /// <summary>
    /// The checker CSV files for the ARA catchment area (A).
    /// </summary>
    public required IPipelineFile[] CheckerCsvA { get; set; }

    /// <summary>
    /// The checker CSV files for technical inspections (FP).
    /// </summary>
    public required IPipelineFile[] CheckerCsvFp { get; set; }

    /// <summary>
    /// The checker CSV files for sponsorship (T).
    /// </summary>
    public required IPipelineFile[] CheckerCsvT { get; set; }

    /// <summary>
    /// The GeoPackage template matching the model version, or <see langword="null"/>
    /// when no unambiguous match was found.
    /// </summary>
    public IPipelineFile? GpkgTemplate { get; set; }

    /// <summary>
    /// The standard organisation table fetched from the VSA repository, or
    /// <see langword="null"/> when no unambiguous match was found.
    /// </summary>
    public IPipelineFile? StandardOrgTable { get; set; }

    /// <summary>
    /// A localized status message summarising the identification result.
    /// </summary>
    public required LocalizedText StatusMessage { get; set; }
}
