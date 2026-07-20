namespace VsaCheckerAnalytics.Processors.GeopackageGeneration;

/// <summary>
/// Opens files embedded in the assembly under the <c>VsaCheckerAnalytics.EmbeddedResources</c>
/// namespace as read-only streams.
/// </summary>
internal static class EmbeddedResource
{
    /// <summary>
    /// Opens the embedded resource <paramref name="fileName"/> (for example
    /// <c>errorMatrixBaseError.xlsx</c>) as a read-only stream. The caller owns and disposes it.
    /// </summary>
    /// <param name="fileName">File name of the embedded resource, without the namespace prefix.</param>
    /// <returns>A stream over the embedded resource.</returns>
    /// <exception cref="InvalidOperationException">The resource is not embedded in the assembly.</exception>
    internal static Stream OpenRead(string fileName)
    {
        var resourceName = $"VsaCheckerAnalytics.EmbeddedResources.{fileName}";
        var assembly = typeof(EmbeddedResource).Assembly;
        return assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException(
                $"Embedded resource '{resourceName}' not found. Available resources: " +
                $"[{string.Join(", ", assembly.GetManifestResourceNames())}].");
    }
}
