namespace VsaCheckerAnalytics.Ili2Gpkg;

/// <summary>
/// Options for <see cref="Ili2GpkgClient"/>. The <see cref="JobsDirectory"/> must point at the
/// host-side path that the ili2gpkg worker has mounted as <c>ILI2GPKG_JOBS_DIR</c>.
/// </summary>
public sealed class Ili2GpkgClientOptions
{
    /// <summary>Local path to the folder where the ili2gpkg worker will process jobs.</summary>
    public string JobsDirectory { get; set; } = string.Empty;

    /// <summary>How often to poll for the worker's <c>output.ready</c> sentinel.</summary>
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromMilliseconds(250);

    /// <summary>Maximum time to wait for a single job to complete.</summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(10);
}
