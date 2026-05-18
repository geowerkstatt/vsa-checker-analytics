using Geopilot.Api.Pipeline;
using Geopilot.PipelineCore.Pipeline;
using Geopilot.PipelineCore.Pipeline.Process;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using VsaCheckerAnalytics.Ili2Gpkg;

namespace VsaCheckerAnalytics.Process.VsaGeopackageGeneration;

/// <summary>
/// Imports the three INTERLIS transfer files (default organisation table, optional user organisation table,
/// and the GEP/DSS Mini transfer file) into a schema-only input GeoPackage by delegating to
/// <see cref="IIli2GpkgClient"/>. Produces a single populated GeoPackage as output under the dictionary key <c>gpkg</c>.
/// </summary>
public sealed class VsaGeopackageGenerationProcess
{
    private const string GeneratedGpkgOutputKey = "generatedGeopackage";
    private const string GeneratedGeopackageName = "generated";

#pragma warning disable CA1859 // Use concrete types when possible for improved performance
    private readonly IIli2GpkgClient ili2GpkgClient;
#pragma warning restore CA1859 // Use concrete types when possible for improved performance
    private readonly IPipelineFileManager pipelineFileManager;
    private readonly ILogger logger;

    /// <summary>
    /// Initializes a new <see cref="VsaGeopackageGenerationProcess"/>.
    /// </summary>
    /// <param name="jobsDirectory">Local path the ili2gpkg worker has mounted as <c>ILI2GPKG_JOBS_DIR</c>.</param>
    /// <param name="pipelineFileManager">Pipeline file manager used to allocate the output GeoPackage.</param>
    /// <param name="logger">Logger.</param>
    public VsaGeopackageGenerationProcess(
        string jobsDirectory,
        IPipelineFileManager pipelineFileManager,
        ILogger logger)
    {
        this.pipelineFileManager = pipelineFileManager ?? throw new ArgumentNullException(nameof(pipelineFileManager));
        this.logger = logger ?? NullLogger.Instance;

        var options = new Ili2GpkgClientOptions
        {
            JobsDirectory = jobsDirectory,
        };
        this.ili2GpkgClient = new Ili2GpkgClient(options, this.logger);
    }

    /// <summary>
    /// Imports the three transfer files into <paramref name="geoPackages"/> in the order
    /// defaultOrgs → (userOrgs if present) → dssMini, then returns the populated GeoPackage as a fresh
    /// <see cref="IPipelineFile"/> under the key <c>gpkg</c>.
    /// </summary>
    /// <param name="geoPackages">The schema-only input GeoPackage that the transfer files are imported into.</param>
    /// <param name="dssMiniXtfs">The GEP/DSS Mini INTERLIS transfer file.</param>
    /// <param name="defaultOrgsXtfs">The standard organisation table INTERLIS transfer file.</param>
    /// <param name="userOrgsXtfs">Optional user organisation table INTERLIS transfer file.</param>
    /// <param name="cancellationToken">Cancellation token forwarded to the ili2gpkg client.</param>
    /// <returns>A dictionary with a single entry, <c>gpkg</c>, holding the populated GeoPackage.</returns>
    [PipelineProcessRun]
    public async Task<Dictionary<string, object?>> RunAsync(
        IPipelineFile[] geoPackages,
        IPipelineFile[] dssMiniXtfs,
        IPipelineFile[] defaultOrgsXtfs,
        IPipelineFile?[] userOrgsXtfs,
        CancellationToken cancellationToken = default)
    {
        var geoPackage = geoPackages.FirstOrDefault();
        var dssMiniXtf = dssMiniXtfs.FirstOrDefault();
        var defaultOrgsXtf = defaultOrgsXtfs.FirstOrDefault();
        var userOrgsXtf = userOrgsXtfs.FirstOrDefault();

        ArgumentNullException.ThrowIfNull(geoPackage);
        ArgumentNullException.ThrowIfNull(dssMiniXtf);
        ArgumentNullException.ThrowIfNull(defaultOrgsXtf);

        var outputGpkg = await CreateGpkgWithImports(geoPackage, dssMiniXtf, defaultOrgsXtf, userOrgsXtf, cancellationToken);

        return new Dictionary<string, object?>
        {
            { GeneratedGpkgOutputKey, outputGpkg },
        };
    }

    private async Task<IPipelineFile?> CreateGpkgWithImports(
        IPipelineFile geoPackage,
        IPipelineFile dssMiniXtf,
        IPipelineFile defaultOrgsXtf,
        IPipelineFile? userOrgsXtf,
        CancellationToken cancellationToken)
    {
        var args = new Ili2GpkgArgs
        {
            SkipReferenceErrors = true,
            SkipGeometryErrors = true,
            DisableValidation = true,
            ImportTid = true,
        };

        var steps = new List<(string Label, IPipelineFile Xtf)>
        {
            ("defaultOrgs", defaultOrgsXtf),
        };
        if (userOrgsXtf is not null)
        {
            steps.Add(("userOrgs", userOrgsXtf));
        }

        steps.Add(("dssMini", dssMiniXtf));

        var current = geoPackage;
        try
        {
            for (var i = 0; i < steps.Count; i++)
            {
                var (label, xtf) = steps[i];
                var isFinal = i == steps.Count - 1;
                var next = isFinal
                    ? pipelineFileManager.GeneratePipelineFile(GeneratedGeopackageName, "gpkg")
                    : pipelineFileManager.GeneratePipelineFile($"gpkg-step-{label}", "gpkg");

                await RunImportStepAsync(current, xtf, next, label, args, cancellationToken).ConfigureAwait(false);
                current = next;
            }
        }
        catch (InvalidOperationException ex)
        {
            logger.LogDebug(ex.Message);
            return null;
        }

        logger.LogInformation("VsaGeopackageImportProcessor produced populated GeoPackage <{FileName}>.", current.OriginalFileName);
        return current;
    }

    private async Task RunImportStepAsync(
        IPipelineFile gpkgIn,
        IPipelineFile xtfIn,
        IPipelineFile gpkgOut,
        string label,
        Ili2GpkgArgs args,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Starting ili2gpkg import <{Label}>.", label);

        await using var gpkgInStream = gpkgIn.OpenReadFileStream();
        await using var xtfInStream = xtfIn.OpenReadFileStream();
        await using var gpkgOutStream = gpkgOut.OpenWriteFileStream();

        var result = await ili2GpkgClient
            .ImportToGeoPackageAsync(gpkgInStream, xtfInStream, gpkgOutStream, args, cancellationToken)
            .ConfigureAwait(false);

        if (!result.Success)
        {
            throw new InvalidOperationException(
                $"ili2gpkg import '{label}' failed.{(string.IsNullOrEmpty(result.Log) ? string.Empty : Environment.NewLine + result.Log)}");
        }
    }
}
