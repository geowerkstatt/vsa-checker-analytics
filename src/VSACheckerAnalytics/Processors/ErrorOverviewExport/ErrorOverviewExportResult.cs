using Geopilot.PipelineCore.Pipeline;

namespace VsaCheckerAnalytics.Processors.ErrorOverviewExport;

/// <summary>
/// Result of the <see cref="ErrorOverviewExportProcess"/>.
/// </summary>
public sealed class ErrorOverviewExportResult
{
    /// <summary>
    /// The exported Excel workbook containing the error overview.
    /// </summary>
    public required IPipelineFile ErrorOverview { get; init; }

    /// <summary>
    /// A localized status message describing the outcome.
    /// </summary>
    public required LocalizedText StatusMessage { get; init; }
}
