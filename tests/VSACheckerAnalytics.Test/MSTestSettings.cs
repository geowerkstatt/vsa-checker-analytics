using System.IO.Packaging;

[assembly: Parallelize(Scope = ExecutionScope.MethodLevel)]

namespace VsaCheckerAnalytics;

[TestClass]
public static class MSTestSettings
{
    [AssemblyInitialize]
    public static void RegisterPackUriScheme(TestContext context)
    {
        // Each TestPipelineHost loads the plugin, and with it a private System.IO.Packaging, into its own
        // AssemblyLoadContext. Their PackUriHelper type initializers race to register the process-wide
        // "pack" URI scheme when parallel tests open an XLSX at once; registering it up front makes them skip it.
        _ = PackUriHelper.UriSchemePack;
    }
}
