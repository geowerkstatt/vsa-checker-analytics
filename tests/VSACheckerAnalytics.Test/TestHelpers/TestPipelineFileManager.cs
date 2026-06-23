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
    public IPipelineFile CreateWritableCopy(IPipelineFile source, string name)
    {
        ArgumentNullException.ThrowIfNull(source);
        var target = GeneratePipelineFile(source.OriginalRelativePath, name, source.FileExtension);
        using var sourceStream = source.OpenReadFileStream();
        using var targetStream = target.OpenWriteFileStream();
        sourceStream.CopyTo(targetStream);
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
