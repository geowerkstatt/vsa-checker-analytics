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
    public async Task<Ili2GpkgImportResult> ImportToGeoPackageAsync(
        Stream geoPackageInput,
        Stream transferFileInput,
        Stream geoPackageOutput,
        Ili2GpkgArgs args,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(geoPackageInput);
        ArgumentNullException.ThrowIfNull(transferFileInput);
        ArgumentNullException.ThrowIfNull(geoPackageOutput);
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

            await WriteStreamToFileAsync(geoPackageInput, jobDbFile, token).ConfigureAwait(false);
            await WriteStreamToFileAsync(transferFileInput, jobDataFile, token).ConfigureAwait(false);

            var payload = new ArgsPayload
            {
                Operation = "import",
                DoSchemaImport = args.DoSchemaImport,
                SqlEnableNull = args.SqlEnableNull,
                SkipReferenceErrors = args.SkipReferenceErrors,
                SkipGeometryErrors = args.SkipGeometryErrors,
                DisableValidation = args.DisableValidation,
                ImportTid = args.ImportTid,
                StrokeArcs = args.StrokeArcs,
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
                await using var src = new FileStream(jobDbFile, FileMode.Open, FileAccess.Read, FileShare.Read);
                await src.CopyToAsync(geoPackageOutput, token).ConfigureAwait(false);

                var log = await File.ReadAllTextAsync(jobSuccessLog, token).ConfigureAwait(false);
                logger.LogDebug("ili2gpkg import job {JobId} succeeded.", jobId);
                return new Ili2GpkgImportResult(true, log);
            }

            if (File.Exists(jobErrorLog))
            {
                var log = await File.ReadAllTextAsync(jobErrorLog, token).ConfigureAwait(false);
                logger.LogWarning("ili2gpkg import job {JobId} failed.", jobId);
                return new Ili2GpkgImportResult(false, log);
            }

            logger.LogWarning("ili2gpkg import job {JobId} produced no log file.", jobId);
            return new Ili2GpkgImportResult(
                false,
                $"ili2gpkg worker reported completion for job {jobId} but produced neither success.log nor error.log.");
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

    private static async Task WriteStreamToFileAsync(Stream source, string destination, CancellationToken token)
    {
        await using var dst = new FileStream(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        await source.CopyToAsync(dst, token).ConfigureAwait(false);
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

        public bool StrokeArcs { get; set; }
    }
}
