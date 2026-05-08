namespace VsaCheckerAnalytics.Ili2Gpkg;

/// <summary>
/// File-drop client for the ili2gpkg worker service. See the worker README for the
/// shared-folder protocol this implementation follows.
/// </summary>
public interface IIli2GpkgClient
{
    /// <summary>
    /// Imports the transfer file under <paramref name="transferFilePath"/> into the GeoPackage under <paramref name="geoPackagePath"/>.
    /// </summary>
    /// <param name="geoPackagePath">The path to the GeoPackage to import into.</param>
    /// <param name="transferFilePath">The path to the INTERLIS transfer file to import.</param>
    /// <param name="logFilePath">The path to a file where the ili2gpkg log should be written.</param>
    /// <param name="args">Additional ili2gpkg arguments.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns><c>true</c> if the import was successful, <c>false</c> if not.</returns>
    Task<bool> ImportToGeoPackageAsync(string geoPackagePath, string transferFilePath, string logFilePath, Ili2GpkgArgs args, CancellationToken cancellationToken = default);
}
