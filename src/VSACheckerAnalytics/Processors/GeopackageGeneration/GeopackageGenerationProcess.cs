using Geopilot.PipelineCore.Ilitools;
using Geopilot.PipelineCore.Pipeline;
using Geopilot.PipelineCore.Pipeline.Process;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text;
using VsaCheckerAnalytics.Geopackage;

namespace VsaCheckerAnalytics.Processors.GeopackageGeneration;

/// <summary>
/// Imports INTERLIS transfer files into a schema-only GeoPackage via ili2gpkg, then enriches
/// the result with checker CSV data, an error matrix, and analytical SQL views. Each enrichment
/// step produces a new copy of the GeoPackage so that intermediate states remain inspectable.
/// </summary>
public sealed class GeopackageGenerationProcess
{
    private const string GeneratedGeopackageName = "generated";

    private static readonly LocalizedText GeneratedStatusMessage = new Dictionary<string, string>
    {
        { "de", "GeoPackage erstellt und mit Checker-Daten, Fehlermatrix und Analyse-Views angereichert." },
        { "fr", "GeoPackage créé et enrichi avec les données du vérificateur, la matrice d'erreurs et les vues d'analyse." },
        { "it", "GeoPackage creato e arricchito con i dati del controllo, la matrice degli errori e le viste di analisi." },
        { "en", "GeoPackage created and enriched with checker data, error matrix and analysis views." },
    };

    private static readonly LocalizedText ImportFailedStatusMessage = new Dictionary<string, string>
    {
        { "de", "GeoPackage konnte nicht erstellt werden: INTERLIS-Import fehlgeschlagen." },
        { "fr", "Le GeoPackage n'a pas pu être créé : l'import INTERLIS a échoué." },
        { "it", "Impossibile creare il GeoPackage: importazione INTERLIS non riuscita." },
        { "en", "GeoPackage could not be created: INTERLIS import failed." },
    };

    private static readonly string[] CsvColumns =
        ["Module", "ErrorId", "Category", "Description", "Model", "Topic", "Bid", "Class", "Tid", "Line", "CharPos", "Geom1", "Geom2", "UserAttributes", "Profiles"];

    private static readonly string[] ErrorMatrixColumns =
        ["cid", "ccat", "cmsg_de", "cmsg_fr", "class_de", "class_fr", "checkmodel", "model", "prio_uc", "prio_gsp", "sub_project_gsp_de", "sub_project_gsp_fr", "required_action_de", "required_action_fr", "action_context_de", "action_context_fr"];

    private static readonly string[] BaseErrorColumns =
        ["cid", "ccat", "cmsg_de", "cmsg_fr", "class_de", "class_fr", "checkmodel", "model", "prio_uc", "prio_gsp", "sub_project_gsp_de", "sub_project_gsp_fr", "required_action_de", "required_action_fr", "action_context_de", "action_context_fr", "cmsg_it", "error_type_de", "error_type_fr", "error_type_it", "required_action_it", "action_context_it"];

    private static readonly string[] CsvJoinIndexColumns = ["ErrorId", "Model", "Class"];

    private readonly IIli2GpkgClient ili2GpkgClient;
    private readonly IPipelineFileManager pipelineFileManager;
    private readonly ILogger logger;

    /// <summary>
    /// Initializes a new <see cref="GeopackageGenerationProcess"/>.
    /// </summary>
    /// <param name="ili2GpkgClient">The ili2gpkg client.</param>
    /// <param name="pipelineFileManager">Pipeline file manager used to allocate the output GeoPackage.</param>
    /// <param name="logger">Logger.</param>
    public GeopackageGenerationProcess(
        IIli2GpkgClient ili2GpkgClient,
        IPipelineFileManager pipelineFileManager,
        ILogger logger)
    {
        this.ili2GpkgClient = ili2GpkgClient ?? throw new ArgumentNullException(nameof(ili2GpkgClient));
        this.pipelineFileManager = pipelineFileManager ?? throw new ArgumentNullException(nameof(pipelineFileManager));
        this.logger = logger ?? NullLogger.Instance;
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
    /// <param name="modelVersion">Data model version (<c>"2020"</c> or <c>"2020.1"</c>) selecting the statistics views.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="GeopackageGenerationResult"/> with the populated GeoPackage and a localized status message.</returns>
    [PipelineProcessRun]
    public async Task<GeopackageGenerationResult> RunAsync(
        IPipelineFile geoPackage,
        IPipelineFile dssMiniXtf,
        IPipelineFile defaultOrgsXtf,
        IPipelineFile? userOrgsXtf,
        IPipelineFile checkerCsvT,
        IPipelineFile checkerCsvA,
        IPipelineFile checkerCsvFp,
        IPipelineFile errorMatrix,
        string language,
        string modelVersion,
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
        ArgumentNullException.ThrowIfNull(modelVersion);

        var outputGpkg = await CreateGpkgWithImports(geoPackage, dssMiniXtf, defaultOrgsXtf, userOrgsXtf, cancellationToken);

        if (outputGpkg is not null)
        {
            outputGpkg = await ImportCheckerCsvsAsync(outputGpkg, checkerCsvT, checkerCsvA, checkerCsvFp, cancellationToken);
            outputGpkg = await ImportErrorMatrixAsync(outputGpkg, errorMatrix, cancellationToken);
            outputGpkg = await CreateAnalyticsAsync(outputGpkg, language, modelVersion, cancellationToken);
        }

        return new GeopackageGenerationResult
        {
            GeneratedGeopackage = outputGpkg,
            StatusMessage = outputGpkg is not null ? GeneratedStatusMessage : ImportFailedStatusMessage,
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

        var result = await ili2GpkgClient
            .ImportAsync(args, gpkgIn, gpkgOut, [xtfIn], cancellationToken)
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

        var target = pipelineFileManager.CreateWritableCopy(sourceGpkg, "gpkg-with-csvs");

        using var connection = OpenGeoPackage(target.GetLocalPath());
        var csvImporter = new CsvImporter(connection, Encoding.GetEncoding(1252), logger);

        await ImportCsvAsync(csvImporter, checkerCsvT, "checker_csv_t", cancellationToken);
        GeopackageMetadata.RegisterAttributes(connection, "checker_csv_t");
        await ImportCsvAsync(csvImporter, checkerCsvA, "checker_csv_a", cancellationToken);
        GeopackageMetadata.RegisterAttributes(connection, "checker_csv_a");
        await ImportCsvAsync(csvImporter, checkerCsvFp, "checker_csv_fp", cancellationToken);
        GeopackageMetadata.RegisterAttributes(connection, "checker_csv_fp");

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
        var target = pipelineFileManager.CreateWritableCopy(sourceGpkg, "gpkg-with-error-matrix");

        using var connection = OpenGeoPackage(target.GetLocalPath());

        EmbeddedSql.Execute(connection, "ErrorMatrixSchema.sql");

        var importer = new ErrorMatrixImporter(connection, logger);

        await using var stream = errorMatrix.OpenReadFileStream();
        await importer.ImportAsync(stream, "error_matrix", ErrorMatrixColumns, cancellationToken)
            .ConfigureAwait(false);

        using var baseStream = EmbeddedResource.OpenRead("errorMatrixBaseError.xlsx");
        await importer.ImportAsync(baseStream, "error_matrix", BaseErrorColumns, cancellationToken)
            .ConfigureAwait(false);

        GeopackageMetadata.RegisterAttributes(connection, "error_matrix");

        new ReaderErrorRulesInitializer(connection, logger).Initialize();

        logger.LogInformation("Imported error matrix into GeoPackage.");
        return target;
    }

    private async Task<IPipelineFile> CreateAnalyticsAsync(
        IPipelineFile sourceGpkg,
        string language,
        string modelVersion,
        CancellationToken cancellationToken)
    {
        var target = pipelineFileManager.CreateWritableCopy(sourceGpkg, GeneratedGeopackageName);

        using var connection = OpenGeoPackage(target.GetLocalPath());
        var viewCreator = new ViewCreator(connection);

        viewCreator.CreateCheckerCsvUnionView(
            "v_checker_csv_all",
            ["checker_csv_t", "checker_csv_a", "checker_csv_fp"],
            ["T", "A", "FP"]);

        viewCreator.CreateCheckerCsvClassifiedView("v_checker_csv_classified", "v_checker_csv_all", "error_matrix", language);
        viewCreator.CreateAdditionalViews(modelVersion);
        viewCreator.AddGpkgContents();

        new StatisticsMaterializer(connection, logger)
            .Materialize("ca_statistics_attribute", "v_statistics_attribute");

        var materializer = new ErrorDataMaterializer(connection, logger);
        materializer.CreateBuildView("v_ca_error_data_build", "v_checker_csv_classified", "error_matrix", "reader_error_rules", language);
        await materializer.MaterializeAsync("v_ca_error_data_build", cancellationToken);

        new OrphanInspector(connection, logger)
            .MaterializeOrphans("ca_error_orphans", "v_checker_csv_classified");

        logger.LogInformation("Created analytics in GeoPackage.");
        return target;
    }

    private static SqliteConnection OpenGeoPackage(string path)
    {
        var connection = new SqliteConnection($"Data Source={path};Pooling=false");
        connection.Open();
        return connection;
    }
}
