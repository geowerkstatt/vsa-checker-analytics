using Microsoft.Data.Sqlite;
using System.Text;
using VsaCheckerAnalytics.Ili2Gpkg;

namespace VsaCheckerAnalytics.TestHelpers;

/// <summary>Hand-rolled <see cref="IIli2GpkgClient"/> test double recording invocations.</summary>
public sealed class FakeIli2GpkgClient : IIli2GpkgClient
{
    public sealed record Invocation(string GeoPackageText, string TransferFileText, Ili2GpkgArgs Args);

    private static readonly Lazy<byte[]> MinimalGeoPackage = new(() =>
    {
        var path = Path.Combine(Path.GetTempPath(), $"fake-gpkg-{Guid.NewGuid():N}.gpkg");
        try
        {
            using (var conn = new SqliteConnection($"Data Source={path};Pooling=false"))
            {
                conn.Open();
                using var cmd = conn.CreateCommand();

                // A real ili2gpkg output is a valid GeoPackage: it always carries the OGC system
                // tables. Mirror that here (with SRS 2056 used by the VSA views) so downstream
                // steps that register layers in gpkg_contents/gpkg_geometry_columns can run.
                cmd.CommandText = """
                    PRAGMA application_id = 1196444487;
                    PRAGMA user_version = 10301;

                    CREATE TABLE gpkg_spatial_ref_sys (
                        srs_name TEXT NOT NULL,
                        srs_id INTEGER NOT NULL PRIMARY KEY,
                        organization TEXT NOT NULL,
                        organization_coordsys_id INTEGER NOT NULL,
                        definition TEXT NOT NULL,
                        description TEXT);

                    INSERT INTO gpkg_spatial_ref_sys VALUES
                        ('Undefined cartesian SRS', -1, 'NONE', -1, 'undefined', NULL),
                        ('Undefined geographic SRS', 0, 'NONE', 0, 'undefined', NULL),
                        ('WGS 84 geodetic', 4326, 'EPSG', 4326, 'geographic', NULL),
                        ('CH1903+ / LV95', 2056, 'EPSG', 2056, 'projected', NULL);

                    CREATE TABLE gpkg_contents (
                        table_name TEXT NOT NULL PRIMARY KEY,
                        data_type TEXT NOT NULL,
                        identifier TEXT UNIQUE,
                        description TEXT DEFAULT '',
                        last_change DATETIME NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ','now')),
                        min_x DOUBLE, min_y DOUBLE, max_x DOUBLE, max_y DOUBLE,
                        srs_id INTEGER,
                        CONSTRAINT fk_gc_r_srs_id FOREIGN KEY (srs_id) REFERENCES gpkg_spatial_ref_sys(srs_id));

                    CREATE TABLE gpkg_geometry_columns (
                        table_name TEXT NOT NULL,
                        column_name TEXT NOT NULL,
                        geometry_type_name TEXT NOT NULL,
                        srs_id INTEGER NOT NULL,
                        z TINYINT NOT NULL,
                        m TINYINT NOT NULL,
                        CONSTRAINT pk_geom_cols PRIMARY KEY (table_name, column_name),
                        CONSTRAINT fk_gc_tn FOREIGN KEY (table_name) REFERENCES gpkg_contents(table_name));
                    """;
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

    /// <summary>Bytes written to the output stream on success. Defaults to a minimal valid GeoPackage.</summary>
    public Func<Invocation, byte[]> OutputSelector { get; set; } = _ => MinimalGeoPackage.Value;

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
