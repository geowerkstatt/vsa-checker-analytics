using Geopilot.Pipeline;
using Geopilot.Pipeline.Config;
using Geopilot.Pipeline.Ilitools;
using Geopilot.Pipeline.Process;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace VsaCheckerAnalytics.TestHelpers;

/// <summary>
/// Builds a pipeline from the repository's real <c>Pipelines/vsaCheckerAnalytics.yaml</c>, the same way
/// the geopilot host does. Tests get the production wiring (step ids, <c>${...}</c> input expressions and
/// <c>default_config</c>) instead of a hand-assembled copy of it, so a change to the definition breaks the
/// tests rather than only the deployment.
/// </summary>
internal sealed class TestPipelineHost : IDisposable
{
    /// <summary>The id of the only pipeline in the definition.</summary>
    public const string PipelineId = "vsa_checker_analytics";

    private readonly PipelineProcessFactory processFactory;
    private readonly PipelineFactory pipelineFactory;
    private bool disposed;

    private TestPipelineHost(PipelineProcessFactory processFactory, PipelineFactory pipelineFactory, string workingDirectory)
    {
        this.processFactory = processFactory;
        this.pipelineFactory = pipelineFactory;
        WorkingDirectory = workingDirectory;
    }

    /// <summary>The temporary directory holding this host's pipeline working directories.</summary>
    public string WorkingDirectory { get; }

    /// <summary>The repository root, located by walking up from the test output directory.</summary>
    public static string RepoRoot { get; } = FindRepoRoot();

    /// <summary>The deployment resources root that <c>${file(...)}</c> references resolve against.</summary>
    public static string ResourcesDirectory { get; } = Path.Combine(RepoRoot, "Resources");

    /// <summary>The pipeline definition shipped with this plugin.</summary>
    public static string DefinitionPath { get; } = Path.Combine(RepoRoot, "Pipelines", "vsaCheckerAnalytics.yaml");

    /// <summary>The fully qualified name of the VSA matcher, the key of its app-settings config layer.</summary>
    public const string VsaMatcherImplementation = "VsaCheckerAnalytics.Processors.VsaMatcher.VsaMatcherProcess";

    /// <summary>The organisation table URL the matcher is configured with for the 2020 model. Unroutable on purpose.</summary>
    public const string VsaOrgTableUrl2020 = "http://vsa-org-table.invalid/org_2020.xtf";

    /// <summary>The organisation table URL the matcher is configured with for the 2020.1 model. Unroutable on purpose.</summary>
    public const string VsaOrgTableUrl20201 = "http://vsa-org-table.invalid/org_2020_1.xtf";

    /// <summary>
    /// Creates a host with the plugin assembly registered as a process plugin.
    /// </summary>
    /// <param name="processConfigOverrides">
    /// Entries replacing the default <c>Pipeline:ProcessConfigs</c> layer for the given implementation. The
    /// defaults already satisfy every process, so this is only needed when a test cares about the values, for
    /// example to point the VSA matcher at a stubbed organisation table URL.
    /// </param>
    /// <remarks>
    /// Every step of the definition is constructed, not just the one under test, so the config layer must
    /// satisfy all of them. Not all parameters come from the definition: the VSA matcher takes its template
    /// paths and organisation table URLs from app settings, which is what the defaults stand in for.
    /// </remarks>
    public static TestPipelineHost Create(Dictionary<string, Parameterization>? processConfigOverrides = null)
    {
        var workingDirectory = Path.Combine(Path.GetTempPath(), "vsa-pipeline-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(workingDirectory);

        var processConfigs = DefaultProcessConfigs();
        foreach (var (implementation, parameterization) in processConfigOverrides ?? [])
            processConfigs[implementation] = parameterization;

        var pipelineOptions = new PipelineOptions
        {
            Definition = DefinitionPath,
            Plugins = [typeof(Processors.VsaMatcher.VsaMatcherProcess).Assembly.Location],
            ProcessConfigs = processConfigs,
        };

        var processFactory = new PipelineProcessFactory(
            Options.Create(pipelineOptions),
            Options.Create(new IlitoolsOptions { IlitoolsWrapperAddress = "http://localhost:5555" }),
            NullLoggerFactory.Instance);

        var pipelineFactory = PipelineFactory.Builder()
            .File(DefinitionPath)
            .PipelineProcessFactory(processFactory)
            .LoggerFactory(NullLoggerFactory.Instance)
            .PipelineTempDirectory(workingDirectory)
            .ResourcesDirectory(ResourcesDirectory)
            .Build();

        return new TestPipelineHost(processFactory, pipelineFactory, workingDirectory);
    }

    /// <summary>
    /// Creates the pipeline. The caller owns the returned instance and must dispose it, which also removes
    /// its per-job working directory.
    /// </summary>
    public IPipeline CreatePipeline() => pipelineFactory.CreatePipeline(PipelineId, Guid.NewGuid());

    /// <summary>
    /// Replaces a private field of a step's process instance, to substitute a framework dependency such as
    /// the matcher's <c>HttpClient</c> or the GeoPackage generation's <c>IIli2GpkgClient</c>.
    /// </summary>
    /// <remarks>
    /// Reflection is required here, not merely convenient. The pipeline runtime constructs processes itself
    /// and offers no seam for substituting their dependencies, and the process is loaded into its own
    /// <see cref="System.Runtime.Loader.AssemblyLoadContext"/>, so its type is not identical to the directly
    /// referenced one and a cast would fail even though the type name and assembly path match.
    /// </remarks>
    public static void ReplaceDependency(IPipelineStep step, string fieldName, object replacement)
    {
        var field = step.Process.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException($"Process <{step.Process.GetType().Name}> has no private field <{fieldName}>.");

        field.SetValue(step.Process, replacement);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (disposed)
            return;

        processFactory.Dispose();

        if (Directory.Exists(WorkingDirectory))
        {
            try
            {
                Directory.Delete(WorkingDirectory, recursive: true);
            }
            catch (IOException)
            {
                // A process may still hold a handle in the working directory; the temp folder is cleaned up by the OS.
            }
        }

        disposed = true;
    }

    /// <summary>
    /// The app-settings config layer a deployment supplies. The template paths point at the resources shipped
    /// with the plugin; the organisation table URLs are unroutable on purpose, so a test that does not stub
    /// them fails loudly instead of reaching the real VSA repository.
    /// </summary>
    private static Dictionary<string, Parameterization> DefaultProcessConfigs() => new()
    {
        [VsaMatcherImplementation] = new Parameterization
        {
            { "geoPackageTemplatePath2020", Path.Combine(ResourcesDirectory, "template_ca_dssmini_2020_d.gpkg") },
            { "geoPackageTemplatePath20201", Path.Combine(ResourcesDirectory, "template_ca_dssmini_2020_1_d.gpkg") },
            { "vsaOrgTableUrl2020", VsaOrgTableUrl2020 },
            { "vsaOrgTableUrl20201", VsaOrgTableUrl20201 },
        },
    };

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Pipelines", "vsaCheckerAnalytics.yaml")))
            directory = directory.Parent;

        return directory?.FullName
            ?? throw new InvalidOperationException($"Repository root not found above {AppContext.BaseDirectory}.");
    }
}
