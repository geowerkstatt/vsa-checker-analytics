namespace VsaCheckerAnalytics.Ili2Gpkg;

/// <summary>
/// File-drop client for the ili2gpkg worker service. See the worker README for the
/// shared-folder protocol this implementation follows.
/// </summary>
public interface IIli2GpkgClient
{
    /// <summary>
    /// Imports the transfer file from <paramref name="transferFileInput"/> into the GeoPackage read from
    /// <paramref name="geoPackageInput"/>. On success, the populated GeoPackage is written to
    /// <paramref name="geoPackageOutput"/>.
    /// </summary>
    /// <param name="geoPackageInput">Stream that produces the input GeoPackage bytes.</param>
    /// <param name="transferFileInput">Stream that produces the INTERLIS transfer file bytes.</param>
    /// <param name="geoPackageOutput">Stream that receives the populated GeoPackage on success. Not written on failure.</param>
    /// <param name="args">Additional ili2gpkg arguments.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>An <see cref="Ili2GpkgImportResult"/> indicating success and the worker log content.</returns>
    Task<Ili2GpkgImportResult> ImportToGeoPackageAsync(
        Stream geoPackageInput,
        Stream transferFileInput,
        Stream geoPackageOutput,
        Ili2GpkgArgs args,
        CancellationToken cancellationToken = default);
}

/// <summary>Outcome of a single ili2gpkg import job.</summary>
/// <param name="Success">Whether the import succeeded.</param>
/// <param name="Log">Content of the worker-produced log (success.log or error.log). Empty if neither was produced.</param>
public sealed record Ili2GpkgImportResult(bool Success, string Log);
