using System.Net;
using System.Text;

namespace Wayaquick.Tests.Helpers;

/// <summary>Records the last outgoing request so tests can assert on headers and body.</summary>
internal sealed class CapturingHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _status;
    private readonly string _body;

    public HttpRequestMessage? LastRequest { get; private set; }
    public string? LastBody { get; private set; }

    internal CapturingHandler(HttpStatusCode status, string body)
    {
        _status = status;
        _body   = body;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequest = request;
        LastBody    = request.Content is not null
            ? await request.Content.ReadAsStringAsync(cancellationToken)
            : null;

        return new HttpResponseMessage(_status)
        {
            Content = new StringContent(_body, Encoding.UTF8, "application/json"),
        };
    }
}
