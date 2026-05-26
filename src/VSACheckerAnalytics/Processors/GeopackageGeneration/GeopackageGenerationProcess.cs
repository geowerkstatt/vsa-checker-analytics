using Geopilot.PipelineCore.Pipeline;
using Geopilot.PipelineCore.Pipeline.Process;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text;
using VsaCheckerAnalytics.Ili2Gpkg;

namespace VsaCheckerAnalytics.Processors.GeopackageGeneration;

/// <summary>
/// Imports INTERLIS transfer files into a schema-only GeoPackage via ili2gpkg, then enriches
/// the result with checker CSV data, an error matrix, and analytical SQL views. Each enrichment
/// step produces a new copy of the GeoPackage so that intermediate states remain inspectable.
/// </summary>
public sealed class GeopackageGenerationProcess
{
    private const string GeneratedGpkgOutputKey = "generatedGeopackage";
    private const string GeneratedGeopackageName = "generated";

    private static readonly string[] CsvColumns =
        ["Module", "ErrorId", "Category", "Group", "Description", "Model", "Topic", "Bid", "Class", "Tid", "Line", "CharPos", "Geom1", "Geom2", "UserAttributes", "Profiles"];

    private static readonly string[] ErrorMatrixColumns =
        ["cid", "ccat", "cmsg_de", "cmsg_fr", "class_de", "class_fr", "checkmodel", "model", "prio_uc", "prio_gsp", "sub_project_gsp_de", "sub_project_gsp_fr", "required_action_de", "required_action_fr", "action_context_de", "action_context_fr"];

    private static readonly string[] CsvJoinIndexColumns = ["ErrorId", "Model", "Class"];
    private static readonly string[] ErrorMatrixJoinIndexColumns = ["cid", "model", "class_de"];

#pragma warning disable CA1859 // Use concrete types when possible for improved performance
    private readonly IIli2GpkgClient ili2GpkgClient;
#pragma warning restore CA1859 // Use concrete types when possible for improved performance
    private readonly IPipelineFileManager pipelineFileManager;
    private readonly ILogger logger;

    /// <summary>
    /// Initializes a new <see cref="GeopackageGenerationProcess"/>.
    /// </summary>
    /// <param name="jobsDirectory">Local path the ili2gpkg worker has mounted as <c>ILI2GPKG_JOBS_DIR</c>.</param>
    /// <param name="pipelineFileManager">Pipeline file manager used to allocate the output GeoPackage.</param>
    /// <param name="logger">Logger.</param>
    public GeopackageGenerationProcess(
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
    /// Imports INTERLIS transfer files into <paramref name="geoPackage"/>, then enriches the
    /// result with checker CSV data, an error matrix, and analytical views.
    /// </summary>
    /// <param name="geoPackage">The schema-only input GeoPackage that the transfer files are imported into.</param>
    /// <param name="dssMiniXtf">The GEP/DSS Mini INTERLIS transfer file.</param>
    /// <param name="defaultOrgsXtf">The standard organisation table INTERLIS transfer file.</param>
    /// <param name="userOrgsXtf">Optional user organisation table INTERLIS transfer file.</param>
    /// <param name="checkerCsvT">Checker CSV file for Trägerschaft (T).</param>
    /// <param name="checkerCsvA">Checker CSV file for ARA (A).</param>
    /// <param name="checkerCsvFp">Checker CSV file for Fachprüfungen (FP).</param>
    /// <param name="errorMatrix">Error matrix XLSX file.</param>
    /// <param name="language">Language code (<c>"DE"</c> or <c>"FR"</c>) for the error matrix join.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A dictionary with the populated GeoPackage under <c>generatedGeopackage</c>.</returns>
    [PipelineProcessRun]
    public async Task<Dictionary<string, object?>> RunAsync(
        IPipelineFile geoPackage,
        IPipelineFile dssMiniXtf,
        IPipelineFile defaultOrgsXtf,
        IPipelineFile? userOrgsXtf,
        IPipelineFile checkerCsvT,
        IPipelineFile checkerCsvA,
        IPipelineFile checkerCsvFp,
        IPipelineFile errorMatrix,
        string language,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(geoPackage);
        ArgumentNullException.ThrowIfNull(dssMiniXtf);
        ArgumentNullException.ThrowIfNull(defaultOrgsXtf);
        ArgumentNullException.ThrowIfNull(checkerCsvT);
        ArgumentNullException.ThrowIfNull(checkerCsvA);
        ArgumentNullException.ThrowIfNull(checkerCsvFp);
        ArgumentNullException.ThrowIfNull(errorMatrix);
        ArgumentNullException.ThrowIfNull(language);

        var outputGpkg = await CreateGpkgWithImports(geoPackage, dssMiniXtf, defaultOrgsXtf, userOrgsXtf, cancellationToken);

        if (outputGpkg is not null)
        {
            outputGpkg = await ImportCheckerCsvsAsync(outputGpkg, checkerCsvT, checkerCsvA, checkerCsvFp, cancellationToken);
            outputGpkg = await ImportErrorMatrixAsync(outputGpkg, errorMatrix, cancellationToken);
            outputGpkg = await CreateAnalyticsAsync(outputGpkg, language, cancellationToken);
        }

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
            StrokeArcs = true,
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
                var next = pipelineFileManager.GeneratePipelineFile($"gpkg-step-{label}", "gpkg");
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

    private async Task<IPipelineFile> ImportCheckerCsvsAsync(
        IPipelineFile sourceGpkg,
        IPipelineFile checkerCsvT,
        IPipelineFile checkerCsvA,
        IPipelineFile checkerCsvFp,
        CancellationToken cancellationToken)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var (target, path) = await CopyGeoPackageAsync(sourceGpkg, "gpkg-with-csvs", cancellationToken);

        using var connection = OpenGeoPackage(path);
        var csvImporter = new CsvImporter(connection, Encoding.GetEncoding(1252), logger);

        await ImportCsvAsync(csvImporter, checkerCsvT, "checker_csv_t", cancellationToken);
        await ImportCsvAsync(csvImporter, checkerCsvA, "checker_csv_a", cancellationToken);
        await ImportCsvAsync(csvImporter, checkerCsvFp, "checker_csv_fp", cancellationToken);

        logger.LogInformation("Imported checker CSVs into GeoPackage.");
        return target;
    }

    private static async Task ImportCsvAsync(
        CsvImporter csvImporter,
        IPipelineFile file,
        string tableName,
        CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadFileStream();
        await csvImporter.ImportAsync(stream, tableName, CsvColumns, cancellationToken, CsvJoinIndexColumns)
            .ConfigureAwait(false);
    }

    private async Task<IPipelineFile> ImportErrorMatrixAsync(
        IPipelineFile sourceGpkg,
        IPipelineFile errorMatrix,
        CancellationToken cancellationToken)
    {
        var (target, path) = await CopyGeoPackageAsync(sourceGpkg, "gpkg-with-error-matrix", cancellationToken);

        using var connection = OpenGeoPackage(path);
        var importer = new ErrorMatrixImporter(connection, logger);

        await using var stream = errorMatrix.OpenReadFileStream();
        await importer.ImportAsync(stream, "error_matrix", ErrorMatrixColumns, cancellationToken, ErrorMatrixJoinIndexColumns)
            .ConfigureAwait(false);

        logger.LogInformation("Imported error matrix into GeoPackage.");
        return target;
    }

    private async Task<IPipelineFile> CreateAnalyticsAsync(
        IPipelineFile sourceGpkg,
        string language,
        CancellationToken cancellationToken)
    {
        var (target, path) = await CopyGeoPackageAsync(sourceGpkg, GeneratedGeopackageName, cancellationToken);

        using var connection = OpenGeoPackage(path);
        var viewCreator = new ViewCreator(connection);

        viewCreator.CreateCheckerCsvUnionView(
            "v_checker_csv_all",
            ["checker_csv_t", "checker_csv_a", "checker_csv_fp"],
            ["T", "A", "FP"]);

        viewCreator.CreateCheckerErrorsView("v_checker_errors", "v_checker_csv_all", "error_matrix", language);
        viewCreator.CreateAdditionalViews();

        var materializer = new ErrorDataMaterializer(connection, logger);
        materializer.CreateBuildView("v_ca_error_data_build", "v_checker_errors", language);
        await materializer.MaterializeAsync("v_ca_error_data_build", cancellationToken);

        new OrphanInspector(connection, logger)
            .MaterializeOrphans("ca_error_orphans", "v_checker_csv_all", "v_checker_errors");

        logger.LogInformation("Created analytics in GeoPackage.");
        return target;
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
