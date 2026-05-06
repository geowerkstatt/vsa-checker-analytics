using Geopilot.PipelineCore.Pipeline;

namespace VsaCheckerAnalytics.TestHelpers;

/// <summary>
/// Factory for creating temporary test files backed by <see cref="TestPipelineFile"/>.
/// </summary>
internal sealed class TestFileFactory : IDisposable
{
    private readonly string baseDirectory;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestFileFactory"/> class.
    /// </summary>
    public TestFileFactory()
    {
        baseDirectory = Path.Combine(Path.GetTempPath(), "vsa-testfiles-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(baseDirectory);
    }

    /// <summary>
    /// Creates a test XTF file with an INTERLIS 2.4 header declaring the given model names.
    /// </summary>
    public IPipelineFile CreateXtf24(string fileName, params string[] modelNames)
    {
        var models = string.Join(
            Environment.NewLine,
            modelNames.Select(m => $"      <ili:model>{m}</ili:model>"));

        var xml = $"""
            <?xml version="1.0" encoding="UTF-8"?>
            <ili:transfer xmlns:ili="http://www.interlis.ch/xtf/2.4/INTERLIS">
              <ili:headersection>
                <ili:models>
            {models}
                </ili:models>
                <ili:sender>Test</ili:sender>
              </ili:headersection>
              <ili:datasection/>
            </ili:transfer>
            """;

        return CreateFile(fileName, xml);
    }

    /// <summary>
    /// Creates a test XTF file with an INTERLIS 2.3 header declaring the given model names.
    /// </summary>
    public IPipelineFile CreateXtf23(string fileName, params string[] modelNames)
    {
        var models = string.Join(
            Environment.NewLine,
            modelNames.Select(m => $"      <MODEL NAME=\"{m}\"/>"));

        var xml = $"""
            <?xml version="1.0" encoding="UTF-8"?>
            <TRANSFER xmlns="http://www.interlis.ch/xtf/2.3/INTERLIS">
              <HEADERSECTION>
                <MODELS>
            {models}
                </MODELS>
              </HEADERSECTION>
            </TRANSFER>
            """;

        return CreateFile(fileName, xml);
    }

    /// <summary>
    /// Creates a test CSV file with the given content and optional relative path.
    /// </summary>
    public IPipelineFile CreateCsv(string fileName, string originalRelativePath = "", string content = "col1;col2\nval1;val2")
    {
        return CreateFile(fileName, content, originalRelativePath);
    }

    /// <summary>
    /// Creates a test file with arbitrary content and returns a pipeline file.
    /// </summary>
    public IPipelineFile CreateFile(string fileName, string content, string originalRelativePath = "")
    {
        var filePath = Path.Combine(baseDirectory, fileName);
        File.WriteAllText(filePath, content);
        return new TestPipelineFile(filePath, originalRelativePath);
    }

    /// <summary>
    /// Creates a resource file on disk and returns its path.
    /// </summary>
    public string CreateResourceFile(string fileName, string content = "resource-content")
    {
        var filePath = Path.Combine(baseDirectory, "resources", fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        File.WriteAllText(filePath, content);
        return filePath;
    }

    /// <summary>
    /// Removes the temporary directory and all test files.
    /// </summary>
    public void Dispose()
    {
        if (Directory.Exists(baseDirectory))
            Directory.Delete(baseDirectory, recursive: true);
    }
}
