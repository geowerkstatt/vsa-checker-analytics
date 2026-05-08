using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VsaCheckerAnalytics.Ili2Gpkg;

/// <summary>
/// File-drop client for the ili2gpkg worker service. See the worker README for the
/// shared-folder protocol this implementation follows.
/// </summary>
public sealed class Ili2GpkgClient : IIli2GpkgClient
{
    private const string ArgsFileName = "args.json";
    private const string DataFileName = "data.xtf";
    private const string DbFileName = "dbfile.gpkg";
    private const string InputReadyFileName = "input.ready";
    private const string OutputReadyFileName = "output.ready";
    private const string SuccessLogFileName = "success.log";
    private const string ErrorLogFileName = "error.log";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        WriteIndented = false,
    };

    private readonly Ili2GpkgClientOptions options;
    private readonly ILogger logger;

    /// <summary>Initializes a new <see cref="Ili2GpkgClient"/>.</summary>
    public Ili2GpkgClient(Ili2GpkgClientOptions options, ILogger logger)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (string.IsNullOrWhiteSpace(options.JobsDirectory))
        {
            throw new ArgumentException($"{nameof(Ili2GpkgClientOptions.JobsDirectory)} must be set.", nameof(options));
        }
    }

    /// <inheritdoc />
    public async Task<bool> ImportToGeoPackageAsync(
        string geoPackagePath,
        string transferFilePath,
        string logFilePath,
        Ili2GpkgArgs args,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(geoPackagePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(transferFilePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(logFilePath);
        ArgumentNullException.ThrowIfNull(args);

        Directory.CreateDirectory(options.JobsDirectory);

        var jobId = Guid.NewGuid().ToString("N");
        var jobDir = Path.Combine(options.JobsDirectory, jobId);
        Directory.CreateDirectory(jobDir);

        var jobDbFile = Path.Combine(jobDir, DbFileName);
        var jobDataFile = Path.Combine(jobDir, DataFileName);
        var jobArgsFile = Path.Combine(jobDir, ArgsFileName);
        var jobInputReady = Path.Combine(jobDir, InputReadyFileName);
        var jobOutputReady = Path.Combine(jobDir, OutputReadyFileName);
        var jobSuccessLog = Path.Combine(jobDir, SuccessLogFileName);
        var jobErrorLog = Path.Combine(jobDir, ErrorLogFileName);

        using var timeoutCts = new CancellationTokenSource(options.Timeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
        var token = linkedCts.Token;

        try
        {
            logger.LogDebug("Submitting ili2gpkg import job {JobId} in {JobDir}.", jobId, jobDir);

            await CopyFileAsync(geoPackagePath, jobDbFile, token).ConfigureAwait(false);
            await CopyFileAsync(transferFilePath, jobDataFile, token).ConfigureAwait(false);

            var payload = new ArgsPayload
            {
                Operation = "import",
                DoSchemaImport = args.DoSchemaImport,
                SqlEnableNull = args.SqlEnableNull,
                SkipReferenceErrors = args.SkipReferenceErrors,
                SkipGeometryErrors = args.SkipGeometryErrors,
                DisableValidation = args.DisableValidation,
                ImportTid = args.ImportTid,
            };
            await using (var argsStream = new FileStream(jobArgsFile, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                await JsonSerializer.SerializeAsync(argsStream, payload, JsonOptions, token).ConfigureAwait(false);
                await argsStream.FlushAsync(token).ConfigureAwait(false);
            }

            // Create empty sentinel file
            await using (new FileStream(jobInputReady, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
            }

            while (!File.Exists(jobOutputReady))
            {
                await Task.Delay(options.PollInterval, token).ConfigureAwait(false);
            }

            if (File.Exists(jobSuccessLog))
            {
                File.Copy(jobSuccessLog, logFilePath, overwrite: true);
                File.Copy(jobDbFile, geoPackagePath, overwrite: true);
                logger.LogDebug("ili2gpkg import job {JobId} succeeded.", jobId);
                return true;
            }

            if (File.Exists(jobErrorLog))
            {
                File.Copy(jobErrorLog, logFilePath, overwrite: true);
                logger.LogWarning("ili2gpkg import job {JobId} failed; see {LogFilePath}.", jobId, logFilePath);
                return false;
            }

            logger.LogWarning("ili2gpkg import job {JobId} produced no log file.", jobId);
            await File.WriteAllTextAsync(
                logFilePath,
                $"ili2gpkg worker reported completion for job {jobId} but produced neither success.log nor error.log.",
                token).ConfigureAwait(false);
            return false;
        }
        finally
        {
            try
            {
                Directory.Delete(jobDir, recursive: true);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                logger.LogDebug(ex, "Failed to clean up ili2gpkg job folder {JobDir}; orphan sweep will reclaim it.", jobDir);
            }
        }
    }

    private static async Task CopyFileAsync(string source, string destination, CancellationToken token)
    {
        await using var src = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read);
        await using var dst = new FileStream(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        await src.CopyToAsync(dst, token).ConfigureAwait(false);
        await dst.FlushAsync(token).ConfigureAwait(false);
    }

    private sealed class ArgsPayload
    {
        public string Operation { get; set; } = string.Empty;

        public bool DoSchemaImport { get; set; }

        public bool SqlEnableNull { get; set; }

        public bool SkipReferenceErrors { get; set; }

        public bool SkipGeometryErrors { get; set; }

        public bool DisableValidation { get; set; }

        public bool ImportTid { get; set; }
    }
}
