using VsaCheckerAnalytics.Ili2Gpkg;

namespace VsaCheckerAnalytics.TestHelpers;

/// <summary>Hand-rolled <see cref="IIli2GpkgClient"/> test double recording invocations.</summary>
public sealed class FakeIli2GpkgClient : IIli2GpkgClient
{
    public sealed record Invocation(string GeoPackagePath, string TransferFilePath, string LogFilePath, Ili2GpkgArgs Args);

    private readonly List<Invocation> invocations = new();

    public IReadOnlyList<Invocation> Invocations => invocations;

    public Func<Invocation, bool> ResultSelector { get; set; } = _ => true;

    public Action<Invocation>? OnInvocation { get; set; }

    public Task<bool> ImportToGeoPackageAsync(
        string geoPackagePath,
        string transferFilePath,
        string logFilePath,
        Ili2GpkgArgs args,
        CancellationToken cancellationToken = default)
    {
        var invocation = new Invocation(geoPackagePath, transferFilePath, logFilePath, args);
        invocations.Add(invocation);
        OnInvocation?.Invoke(invocation);
        return Task.FromResult(ResultSelector(invocation));
    }
}
