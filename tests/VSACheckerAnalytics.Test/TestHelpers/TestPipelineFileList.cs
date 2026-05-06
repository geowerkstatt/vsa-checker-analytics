using Geopilot.PipelineCore.Pipeline;
using System.Text.RegularExpressions;

namespace VsaCheckerAnalytics.TestHelpers;

/// <summary>
/// Test double for <see cref="IPipelineFileList"/> backed by a simple list.
/// </summary>
internal sealed class TestPipelineFileList : IPipelineFileList
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestPipelineFileList"/> class.
    /// </summary>
    public TestPipelineFileList(IEnumerable<IPipelineFile> files)
    {
        Files = files.ToList();
    }

    /// <inheritdoc/>
    public ICollection<IPipelineFile> Files { get; }

    /// <inheritdoc/>
    public IPipelineFileList WithExtensions(HashSet<string> extensions)
    {
        var filtered = Files.Where(f =>
            extensions.Any(ext => f.FileExtension.Equals(ext, StringComparison.OrdinalIgnoreCase)));
        return new TestPipelineFileList(filtered);
    }

    /// <inheritdoc/>
    public IPipelineFileList WithMatchingName(string namePattern)
    {
        var filtered = Files.Where(f => Regex.IsMatch(f.OriginalFileName, namePattern, RegexOptions.IgnoreCase));
        return new TestPipelineFileList(filtered);
    }

    /// <inheritdoc/>
    public IPipelineFileList Matches(Func<IPipelineFile, bool> predicate)
    {
        return new TestPipelineFileList(Files.Where(predicate));
    }
}
