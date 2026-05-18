using System.Text;
using VsaCheckerAnalytics.Ili2Gpkg;

namespace VsaCheckerAnalytics.TestHelpers;

/// <summary>Hand-rolled <see cref="IIli2GpkgClient"/> test double recording invocations.</summary>
public sealed class FakeIli2GpkgClient : IIli2GpkgClient
{
    public sealed record Invocation(string GeoPackageText, string TransferFileText, Ili2GpkgArgs Args);

    private readonly List<Invocation> invocations = new();

    public IReadOnlyList<Invocation> Invocations => invocations;

    public Func<Invocation, bool> ResultSelector { get; set; } = _ => true;

    public Action<Invocation>? OnInvocation { get; set; }

    /// <summary>Text written to the output stream when <see cref="ResultSelector"/> returns true. Defaults to a constant marker.</summary>
    public Func<Invocation, string> OutputSelector { get; set; } = _ => "populated-gpkg";

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

        using var gpkgReader = new StreamReader(geoPackageInput, Encoding.UTF8, leaveOpen: true);
        var gpkgText = await gpkgReader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);

        using var xtfReader = new StreamReader(transferFileInput, Encoding.UTF8, leaveOpen: true);
        var xtfText = await xtfReader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);

        var invocation = new Invocation(gpkgText, xtfText, args);
        invocations.Add(invocation);
        OnInvocation?.Invoke(invocation);

        var success = ResultSelector(invocation);
        if (success)
        {
            var bytes = Encoding.UTF8.GetBytes(OutputSelector(invocation));
            await geoPackageOutput.WriteAsync(bytes, cancellationToken).ConfigureAwait(false);
        }

        return new Ili2GpkgImportResult(success, success ? "ok" : "fail");
    }
}
