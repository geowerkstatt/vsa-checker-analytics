using System.Net;

namespace VsaCheckerAnalytics.TestHelpers;

/// <summary>
/// HTTP message handler that returns preconfigured responses for test URLs.
/// </summary>
internal sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly Dictionary<string, byte[]> responses = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Registers a response body for the given URL.
    /// </summary>
    public void Register(string url, string content)
    {
        responses[url] = System.Text.Encoding.UTF8.GetBytes(content);
    }

    /// <inheritdoc/>
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var url = request.RequestUri?.ToString() ?? string.Empty;
        if (responses.TryGetValue(url, out var body))
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(body),
            });
        }

        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
    }
}
