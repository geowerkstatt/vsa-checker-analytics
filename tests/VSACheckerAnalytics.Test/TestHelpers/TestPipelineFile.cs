using Geopilot.PipelineCore.Pipeline;

namespace VsaCheckerAnalytics.TestHelpers;

/// <summary>
/// Test double for <see cref="IPipelineFile"/> backed by a temporary file on disk.
/// </summary>
internal sealed class TestPipelineFile : IPipelineFile
{
    private readonly string filePath;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestPipelineFile"/> class.
    /// </summary>
    public TestPipelineFile(string filePath, string originalRelativePath = "")
    {
        this.filePath = filePath;
        OriginalFileName = Path.GetFileName(filePath);
        OriginalFileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
        FileExtension = Path.GetExtension(filePath).TrimStart('.');
        OriginalRelativePath = originalRelativePath;
    }

    /// <summary>Gets or sets the value.</summary>
    public string OriginalFileNameWithoutExtension { get; }

    /// <summary>Gets or sets the value.</summary>
    public string OriginalFileName { get; }

    /// <summary>Gets or sets the value.</summary>
    public string FileExtension { get; }

    /// <summary>Gets or sets the value.</summary>
    public string OriginalRelativePath { get; }

    /// <summary>Gets or sets the value.</summary>
    public FileStream OpenReadFileStream() => new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

    /// <summary>Gets or sets the value.</summary>
    public FileStream OpenWriteFileStream() => new(filePath, FileMode.CreateNew, FileAccess.Write);
}
