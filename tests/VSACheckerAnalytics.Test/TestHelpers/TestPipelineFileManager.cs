using Geopilot.PipelineCore.Pipeline;

namespace VsaCheckerAnalytics.TestHelpers;

/// <summary>
/// Test double for <see cref="IPipelineFileManager"/> that creates files in a temporary directory.
/// </summary>
internal sealed class TestPipelineFileManager : IPipelineFileManager, IDisposable
{
    private readonly string baseDirectory;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestPipelineFileManager"/> class.
    /// </summary>
    public TestPipelineFileManager()
    {
        baseDirectory = Path.Combine(Path.GetTempPath(), "vsa-test-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(baseDirectory);
    }

    /// <inheritdoc/>
    public IPipelineFile GeneratePipelineFile(string originalFileName, string fileExtension)
    {
        var fileName = $"{originalFileName}.{fileExtension}";
        var filePath = Path.Combine(baseDirectory, fileName);
        return new TestPipelineFile(filePath);
    }

    /// <inheritdoc/>
    public IPipelineFile GeneratePipelineFile(string originalRelativePath, string originalFileName, string fileExtension)
    {
        return GeneratePipelineFile(originalFileName, fileExtension);
    }

    /// <inheritdoc/>
    public async Task<IPipelineFile> CreateWritableCopyAsync(IPipelineFile source, string name, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        var target = GeneratePipelineFile(source.OriginalRelativePath, name, source.FileExtension);
        using var sourceStream = await source.OpenReadAsync(cancellationToken);
        using var targetStream = target.OpenWriteFileStream();
        await sourceStream.CopyToAsync(targetStream, cancellationToken);
        return target;
    }

    /// <summary>
    /// Removes the temporary directory and all generated files.
    /// </summary>
    public void Dispose()
    {
        if (Directory.Exists(baseDirectory))
            Directory.Delete(baseDirectory, recursive: true);
    }
}
