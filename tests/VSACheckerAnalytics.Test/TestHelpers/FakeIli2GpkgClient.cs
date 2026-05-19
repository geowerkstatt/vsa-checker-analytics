using Microsoft.Data.Sqlite;
using System.Text;
using VsaCheckerAnalytics.Ili2Gpkg;

namespace VsaCheckerAnalytics.TestHelpers;

/// <summary>Hand-rolled <see cref="IIli2GpkgClient"/> test double recording invocations.</summary>
public sealed class FakeIli2GpkgClient : IIli2GpkgClient
{
    public sealed record Invocation(string GeoPackageText, string TransferFileText, Ili2GpkgArgs Args);

    private static readonly Lazy<byte[]> MinimalSqliteDb = new(() =>
    {
        var path = Path.Combine(Path.GetTempPath(), $"fake-gpkg-{Guid.NewGuid():N}.db");
        try
        {
            using (var conn = new SqliteConnection($"Data Source={path};Pooling=false"))
            {
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "PRAGMA user_version = 0";
                cmd.ExecuteNonQuery();
            }

            return File.ReadAllBytes(path);
        }
        finally
        {
            File.Delete(path);
        }
    });

    private readonly List<Invocation> invocations = new();

    public IReadOnlyList<Invocation> Invocations => invocations;

    public Func<Invocation, bool> ResultSelector { get; set; } = _ => true;

    public Action<Invocation>? OnInvocation { get; set; }

    /// <summary>Bytes written to the output stream on success. Defaults to an empty SQLite database.</summary>
    public Func<Invocation, byte[]> OutputSelector { get; set; } = _ => MinimalSqliteDb.Value;

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
            await geoPackageOutput.WriteAsync(OutputSelector(invocation), cancellationToken).ConfigureAwait(false);
        }

        return new Ili2GpkgImportResult(success, success ? "ok" : "fail");
    }
}
