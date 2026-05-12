using Geopilot.Api.Pipeline;
using Geopilot.PipelineCore.Pipeline;
using Geopilot.PipelineCore.Pipeline.Process;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Threading;
using VsaCheckerAnalytics.Ili2Gpkg;

namespace VsaCheckerAnalytics.Process.VsaGeopackageGeneration;

/// <summary>
/// Imports the three INTERLIS transfer files (default organisation table, optional user organisation table,
/// and the GEP/DSS Mini transfer file) into a schema-only input GeoPackage by delegating to
/// <see cref="IIli2GpkgClient"/>. Produces a single populated GeoPackage as output under the dictionary key <c>gpkg</c>.
/// </summary>
public sealed class VsaGeopackageGenerationProcess : IDisposable
{
    private const string GeneratedGpkgOutputKey = "generatedGeopackage";
    private const string GeneratedGeopackageName = "generated";

#pragma warning disable CA1859 // Use concrete types when possible for improved performance
    private readonly IIli2GpkgClient ili2GpkgClient;
#pragma warning restore CA1859 // Use concrete types when possible for improved performance
    private readonly IPipelineFileManager pipelineFileManager;
    private readonly ILogger logger;
    private readonly string workDir;

    private bool disposed;

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

        workDir = Path.Combine(Path.GetTempPath(), "vsa-gpkg-import-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(workDir);
    }

    /// <summary>Internal accessor exposing the per-instance scratch directory for tests.</summary>
    internal string WorkDirectory => workDir;

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
        ObjectDisposedException.ThrowIf(disposed, this);

        var outputGpkg = await CreateGpkgWithImports(geoPackage, dssMiniXtf, defaultOrgsXtf, userOrgsXtf, cancellationToken);

        return new Dictionary<string, object?>
        {
            { GeneratedGpkgOutputKey, outputGpkg },
        };
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        try
        {
            if (Directory.Exists(workDir))
            {
                Directory.Delete(workDir, recursive: true);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            logger.LogDebug(ex, "Failed to clean up VsaGeopackageImportProcessor work directory <{WorkDir}>.", workDir);
        }
        finally
        {
            disposed = true;
        }
    }

    private async Task<IPipelineFile?> CreateGpkgWithImports(
        IPipelineFile geoPackage,
        IPipelineFile dssMiniXtf,
        IPipelineFile defaultOrgsXtf,
        IPipelineFile? userOrgsXtf,
        CancellationToken cancellationToken)
    {
        var workingGpkgPath = await CopyPipelineFileToWorkdir(geoPackage, "input.gpkg", cancellationToken).ConfigureAwait(false);
        var defaultOrgsPath = await CopyPipelineFileToWorkdir(defaultOrgsXtf, "defaultOrgs.xtf", cancellationToken).ConfigureAwait(false);
        var dssMiniPath = await CopyPipelineFileToWorkdir(dssMiniXtf, "dssMini.xtf", cancellationToken).ConfigureAwait(false);
        string? userOrgsPath = userOrgsXtf is null
            ? null
            : await CopyPipelineFileToWorkdir(userOrgsXtf, "userOrgs.xtf", cancellationToken).ConfigureAwait(false);

        var args = new Ili2GpkgArgs
        {
            SkipReferenceErrors = true,
            SkipGeometryErrors = true,
            DisableValidation = true,
            ImportTid = true,
        };

        try
        {
            await ImportOrThrowAsync(workingGpkgPath, defaultOrgsPath, "defaultOrgs", args, cancellationToken).ConfigureAwait(false);

            if (userOrgsPath is not null)
            {
                await ImportOrThrowAsync(workingGpkgPath, userOrgsPath, "userOrgs", args, cancellationToken).ConfigureAwait(false);
            }

            await ImportOrThrowAsync(workingGpkgPath, dssMiniPath, "dssMini", args, cancellationToken).ConfigureAwait(false);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogDebug(ex.Message);
            return null;
        }

        var outputGpkg = pipelineFileManager.GeneratePipelineFile(GeneratedGeopackageName, "gpkg");
        await using (var src = new FileStream(workingGpkgPath, FileMode.Open, FileAccess.Read, FileShare.Read))
        await using (var dst = outputGpkg.OpenWriteFileStream())
        {
            await src.CopyToAsync(dst, cancellationToken).ConfigureAwait(false);
        }

        logger.LogInformation("VsaGeopackageImportProcessor produced populated GeoPackage <{FileName}>.", outputGpkg.OriginalFileName);

        return outputGpkg;
    }

    private async Task<string> CopyPipelineFileToWorkdir(IPipelineFile file, string localName, CancellationToken cancellationToken)
    {
        var localPath = Path.Combine(workDir, localName);
        await using var src = file.OpenReadFileStream();
        await using var dst = new FileStream(localPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        await src.CopyToAsync(dst, cancellationToken).ConfigureAwait(false);
        await dst.FlushAsync(cancellationToken).ConfigureAwait(false);
        return localPath;
    }

    private async Task ImportOrThrowAsync(string geoPackagePath, string transferFilePath, string label, Ili2GpkgArgs args, CancellationToken cancellationToken)
    {
        var logPath = Path.Combine(workDir, $"{label}.log");
        logger.LogDebug("Starting ili2gpkg import <{Label}> into <{Gpkg}>.", label, geoPackagePath);

        var success = await ili2GpkgClient
            .ImportToGeoPackageAsync(geoPackagePath, transferFilePath, logPath, args, cancellationToken)
            .ConfigureAwait(false);

        if (!success)
        {
            var detail = await TryReadLogAsync(logPath).ConfigureAwait(false);
            throw new InvalidOperationException(
                $"ili2gpkg import '{label}' failed. See log <{logPath}>.{(detail is null ? string.Empty : Environment.NewLine + detail)}");
        }
    }

    private static async Task<string?> TryReadLogAsync(string logPath)
    {
        try
        {
            if (!File.Exists(logPath))
            {
                return null;
            }

            return await File.ReadAllTextAsync(logPath).ConfigureAwait(false);
        }
        catch (IOException)
        {
            return null;
        }
    }
}
