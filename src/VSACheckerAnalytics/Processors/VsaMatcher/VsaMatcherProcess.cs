using Geopilot.PipelineCore.Pipeline;
using Geopilot.PipelineCore.Pipeline.Process;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace VsaCheckerAnalytics.Processors.VsaMatcher;

/// <summary>
/// Routes input files by semantic role, extracts metadata from the GEP transfer file,
/// and enriches the pipeline with application resources and VSA repository data.
/// </summary>
/// <remarks>
/// <para>
/// The VSA Matcher receives the original user upload (GEP transfer file and optional
/// organisation table) together with the unzipped GEP checker output. It identifies each
/// file by INTERLIS model name or filename pattern, extracts the data-model version
/// (2020 / 2020.1) from the GEP header, selects version-matching application resources
/// (GeoPackage template, error matrix, QGIS project), and fetches the standard organisation
/// table from the public VSA repository.
/// </para>
/// <para>
/// ILI model filters use exact name matching (case-insensitive). A file matches when its
/// declared models overlap with at least one entry in the corresponding filter set.
/// Checker CSV identification uses regex patterns matched against the filename without extension.
/// </para>
/// </remarks>
internal sealed class VsaMatcherProcess : IDisposable
{
    private const string CheckerDirectory = "check";
    private const string CheckerCsvPatternA = "_a_err$";
    private const string CheckerCsvPatternFp = "_fp_err$";
    private const string CheckerCsvPatternT = "_t_err$";
    private static readonly XNamespace Interlis24Namespace = "http://www.interlis.ch/xtf/2.4/INTERLIS";

    private static readonly LocalizedText GepFoundStatusMessageFormat = new Dictionary<string, string>()
    {
        { "de", "GEP-Datei erkannt (Modell {0}, {1}), {2} Checker-CSV(s) gefunden." },
        { "fr", "Fichier GEP identifié (modèle {0}, {1}), {2} CSV de vérification trouvé(s)." },
        { "it", "File GEP identificato (modello {0}, {1}), {2} CSV di verifica trovati." },
        { "en", "GEP file identified (model {0}, {1}), {2} checker CSV(s) found." },
    };

    private static readonly LocalizedText NoGepFoundStatusMessage = new Dictionary<string, string>()
    {
        { "de", "Keine GEP-Transferdatei in den hochgeladenen Dateien gefunden." },
        { "fr", "Aucun fichier de transfert GEP trouvé dans les fichiers téléchargés." },
        { "it", "Nessun file di trasferimento GEP trovato nei file caricati." },
        { "en", "No GEP transfer file found in uploads." },
    };

    private static readonly LocalizedText MultipleGepStatusMessageFormat = new Dictionary<string, string>()
    {
        { "de", "{0} GEP-Dateien gefunden ({1}), keine eindeutige Zuordnung möglich." },
        { "fr", "{0} fichiers GEP trouvés ({1}), attribution univoque impossible." },
        { "it", "{0} file GEP trovati ({1}), attribuzione univoca non possibile." },
        { "en", "{0} GEP files found ({1}), unambiguous assignment not possible." },
    };

    private static readonly HashSet<string> GepIliModels2020De = new(["VSADSSMINI_2020_LV95"], StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<string> GepIliModels2020Fr = new(["VSASDEEMINI_2020_LV95"], StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<string> GepIliModels20201De = new(["VSADSSMINI_2020_1_LV95"], StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<string> GepIliModels20201Fr = new(["VSASDEEMINI_2020_1_LV95"], StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<string> OrgTableIliModels = new(
        ["SIA405_Base_Abwasser_LV95", "SIA405_Base_Abwasser_1_LV95", "SIA405_Base_Eaux_usees_LV95", "SIA405_Base_Eaux_usees_1_LV95"],
        StringComparer.OrdinalIgnoreCase);

    private readonly ILogger logger;
    private readonly IPipelineFileManager pipelineFileManager;
    private readonly string geoPackageTemplatePath2020;
    private readonly string geoPackageTemplatePath20201;
    private readonly string vsaOrgTableUrl2020;
    private readonly string vsaOrgTableUrl20201;
    private HttpClient httpClient = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="VsaMatcherProcess"/> class.
    /// </summary>
    /// <param name="logger">Logger instance injected by the pipeline framework.</param>
    /// <param name="pipelineFileManager">File manager for creating pipeline output files.</param>
    /// <param name="geoPackageTemplatePath2020">Path to the GeoPackage template for model version 2020.</param>
    /// <param name="geoPackageTemplatePath20201">Path to the GeoPackage template for model version 2020.1.</param>
    /// <param name="vsaOrgTableUrl2020">URL of the standard organisation table (2020) on the VSA repository.</param>
    /// <param name="vsaOrgTableUrl20201">URL of the standard organisation table (2020.1) on the VSA repository.</param>
    public VsaMatcherProcess(
        ILogger logger,
        IPipelineFileManager pipelineFileManager,
        string geoPackageTemplatePath2020,
        string geoPackageTemplatePath20201,
        string vsaOrgTableUrl2020,
        string vsaOrgTableUrl20201)
    {
        this.logger = logger;
        this.pipelineFileManager = pipelineFileManager;
        this.geoPackageTemplatePath2020 = geoPackageTemplatePath2020;
        this.geoPackageTemplatePath20201 = geoPackageTemplatePath20201;
        this.vsaOrgTableUrl2020 = vsaOrgTableUrl2020;
        this.vsaOrgTableUrl20201 = vsaOrgTableUrl20201;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        httpClient.Dispose();
    }

    /// <summary>
    /// Runs the VSA matcher: identifies input files, extracts metadata, loads resources, and
    /// provides named output channels for downstream processors. A localized
    /// <c>status_message</c> summarises the identification result.
    /// </summary>
    /// <param name="files">The originally uploaded files (GEP transfer file, optional org table, ZIP).</param>
    /// <param name="unzippedFiles">Files extracted from the GEP checker ZIP by a preceding unzip step.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="VsaMatcherResult"/> with the routed files, extracted metadata, loaded resources, and a localized status message.</returns>
    [PipelineProcessRun]
    public async Task<VsaMatcherResult> RunAsync(
        IPipelineFile[] uploadFiles,
        IPipelineFile[] unzippedFiles,
        CancellationToken cancellationToken)
    {
        var gepMatches = FindGepFiles(files);
        var userOrgTables = FindOrgTables(files);

        var checkerCsvsA = FindCheckerCsvs(unzippedFiles, CheckerCsvPatternA, "ARA (a)");
        var checkerCsvsFp = FindCheckerCsvs(unzippedFiles, CheckerCsvPatternFp, "Fachprüfungen (FP)");
        var checkerCsvsT = FindCheckerCsvs(unzippedFiles, CheckerCsvPatternT, "Trägerschaft (T)");

        var (modelVersion, language) = gepMatches.Length == 1
            ? (gepMatches[0].Version, gepMatches[0].Language)
            : ((ModelVersion?)null, (Language?)null);

        IPipelineFile? gpkgTemplate = null;
        IPipelineFile? standardOrgTable = null;

        if (modelVersion != null)
        {
            var templatePath = modelVersion == ModelVersion.V20201 ? geoPackageTemplatePath20201 : geoPackageTemplatePath2020;
            var orgTableUrl = modelVersion == ModelVersion.V20201 ? vsaOrgTableUrl20201 : vsaOrgTableUrl2020;

            gpkgTemplate = await CopyResourceToPipelineFileAsync(templatePath, cancellationToken).ConfigureAwait(false);
            standardOrgTable = await FetchToPipelineFileAsync(orgTableUrl, cancellationToken).ConfigureAwait(false);
        }

        var totalCheckerCsvs = checkerCsvsA.Length + checkerCsvsFp.Length + checkerCsvsT.Length;
        LocalizedText statusMessage;
        if (gepMatches.Length == 1)
        {
            statusMessage = GepFoundStatusMessageFormat
                .Map(msg => string.Format(CultureInfo.InvariantCulture, msg, ModelVersionToString(modelVersion), LanguageToString(language), totalCheckerCsvs));
        }
        else if (gepMatches.Length > 1)
        {
            var matchDetails = string.Join(", ", gepMatches.Select(m => $"{ModelVersionToString(m.Version)} {LanguageToString(m.Language)}"));
            statusMessage = MultipleGepStatusMessageFormat
                .Map(msg => string.Format(CultureInfo.InvariantCulture, msg, gepMatches.Length, matchDetails));
        }
        else
        {
            statusMessage = NoGepFoundStatusMessage;
        }

        return new VsaMatcherResult
        {
            Gep = gepMatches.Select(m => m.File).ToArray(),
            ModelVersion = ModelVersionToString(modelVersion),
            Language = LanguageToString(language),
            UserOrgTable = userOrgTables,
            CheckerCsvA = checkerCsvsA,
            CheckerCsvFp = checkerCsvsFp,
            CheckerCsvT = checkerCsvsT,
            GpkgTemplate = gpkgTemplate,
            StandardOrgTable = standardOrgTable,
            StatusMessage = statusMessage,
        };
    }

    /// <summary>
    /// Finds all GEP transfer files among the uploaded XTF files. Each match carries its own
    /// model version and language. Checks version 2020.1 first (more specific) before 2020.
    /// </summary>
    private GepMatch[] FindGepFiles(IPipelineFile[] files)
    {
        var xtfFiles = files.WithExtensions(new HashSet<string> { "xtf" });
        var matches = new List<GepMatch>();

        foreach (var file in xtfFiles)
        {
            var models = ExtractIliModels(file);

            if (GepIliModels20201De.Overlaps(models))
            {
                logger.LogInformation("Identified GEP '{FileName}' as model version 2020.1 (de).", file.OriginalFileName);
                matches.Add(new GepMatch(file, ModelVersion.V20201, Language.De));
            }
            else if (GepIliModels20201Fr.Overlaps(models))
            {
                logger.LogInformation("Identified GEP '{FileName}' as model version 2020.1 (fr).", file.OriginalFileName);
                matches.Add(new GepMatch(file, ModelVersion.V20201, Language.Fr));
            }
            else if (GepIliModels2020De.Overlaps(models))
            {
                logger.LogInformation("Identified GEP '{FileName}' as model version 2020 (de).", file.OriginalFileName);
                matches.Add(new GepMatch(file, ModelVersion.V2020, Language.De));
            }
            else if (GepIliModels2020Fr.Overlaps(models))
            {
                logger.LogInformation("Identified GEP '{FileName}' as model version 2020 (fr).", file.OriginalFileName);
                matches.Add(new GepMatch(file, ModelVersion.V2020, Language.Fr));
            }
        }

        if (matches.Count == 0)
            logger.LogWarning("No GEP transfer file found in uploads.");

        return matches.ToArray();
    }

    /// <summary>
    /// Finds all user-provided organisation tables among the uploaded XTF files.
    /// </summary>
    private IPipelineFile[] FindOrgTables(IPipelineFile[] files)
    {
        var xtfFiles = files.WithExtensions(new HashSet<string> { "xtf" });
        var orgTables = new List<IPipelineFile>();

        foreach (var file in xtfFiles)
        {
            var models = ExtractIliModels(file);

            if (OrgTableIliModels.Overlaps(models))
            {
                logger.LogInformation("Found user organisation table: '{FileName}'.", file.OriginalFileName);
                orgTables.Add(file);
            }
        }

        if (orgTables.Count == 0)
            logger.LogInformation("No user organisation table found in uploads.");

        return orgTables.ToArray();
    }

    /// <summary>
    /// Finds all checker CSV files from the unzipped files. Files must reside in the
    /// <see cref="CheckerDirectory"/> and match the given filename regex pattern.
    /// </summary>
    private IPipelineFile[] FindCheckerCsvs(IPipelineFile[] unzippedFiles, string pattern, string label)
    {
        var csvs = unzippedFiles
            .Where(f =>
                f.FileExtension.Equals("csv", StringComparison.OrdinalIgnoreCase) &&
                f.OriginalRelativePath.Equals(CheckerDirectory, StringComparison.OrdinalIgnoreCase) &&
                Regex.IsMatch(f.OriginalFileNameWithoutExtension, pattern, RegexOptions.IgnoreCase))
            .ToArray();

        if (csvs.Length == 0)
            logger.LogWarning("Checker CSV for {Label} not found (pattern: '{Pattern}').", label, pattern);
        else
            logger.LogInformation("Found {Count} checker CSV(s) for {Label}.", csvs.Length, label);

        return csvs;
    }

    private static string? LanguageToString(Language? language) => language switch
    {
        Language.De => "DE",
        Language.Fr => "FR",
        _ => null,
    };

    private static string? ModelVersionToString(ModelVersion? version) => version switch
    {
        ModelVersion.V2020 => "2020",
        ModelVersion.V20201 => "2020.1",
        _ => null,
    };

    /// <summary>
    /// Reads the INTERLIS model names declared in the XTF header of the given file.
    /// Supports INTERLIS 2.4 (namespace-qualified elements) and INTERLIS 2.3
    /// (default-namespace uppercase elements with <c>NAME</c> attribute).
    /// Returns an empty set if the file cannot be parsed or matches neither format.
    /// </summary>
    private static HashSet<string> ExtractIliModels(IPipelineFile file)
    {
        try
        {
            using var stream = file.OpenReadFileStream();
            var doc = XDocument.Load(stream);
            var root = doc.Root;
            if (root == null)
                return new HashSet<string>();

            if (root.Name.Namespace == Interlis24Namespace)
            {
                return root
                    .Element(Interlis24Namespace + "headersection")?
                    .Element(Interlis24Namespace + "models")?
                    .Elements(Interlis24Namespace + "model")
                    .Select(e => e.Value)
                    .ToHashSet() ?? new HashSet<string>();
            }

            var defaultNamespace = root.GetDefaultNamespace();
            return root
                .Element(defaultNamespace + "HEADERSECTION")?
                .Element(defaultNamespace + "MODELS")?
                .Elements(defaultNamespace + "MODEL")
                .Select(e => e.Attribute("NAME")?.Value)
                .OfType<string>()
                .ToHashSet() ?? new HashSet<string>();
        }
        catch
        {
            return new HashSet<string>();
        }
    }

    /// <summary>
    /// Copies a local application resource file into a pipeline-managed file.
    /// </summary>
    private async Task<IPipelineFile> CopyResourceToPipelineFileAsync(string sourcePath, CancellationToken cancellationToken)
    {
        var fileName = Path.GetFileNameWithoutExtension(sourcePath);
        var extension = Path.GetExtension(sourcePath).TrimStart('.');
        var pipelineFile = pipelineFileManager.GeneratePipelineFile(fileName, extension);

        using var source = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var target = pipelineFile.OpenWriteFileStream();
        await source.CopyToAsync(target, cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Copied resource '{SourcePath}' to pipeline.", sourcePath);
        return pipelineFile;
    }

    /// <summary>
    /// Downloads a file from the given URL and stores it as a pipeline-managed file.
    /// </summary>
    private async Task<IPipelineFile> FetchToPipelineFileAsync(string url, CancellationToken cancellationToken)
    {
        var uri = new Uri(url);
        var fileName = Path.GetFileNameWithoutExtension(uri.LocalPath);
        var extension = Path.GetExtension(uri.LocalPath).TrimStart('.');
        var pipelineFile = pipelineFileManager.GeneratePipelineFile(fileName, extension);

        using var response = await httpClient.GetAsync(uri, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        using var target = pipelineFile.OpenWriteFileStream();
        await response.Content.CopyToAsync(target, cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Fetched '{Url}' to pipeline.", url);
        return pipelineFile;
    }

    /// <summary>
    /// A GEP file match with its associated model version and language.
    /// </summary>
    private sealed record GepMatch(IPipelineFile File, ModelVersion Version, Language Language);
}
