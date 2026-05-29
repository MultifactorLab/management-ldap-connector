using System.Net;

namespace Infra.Tests.Features.BackgroundWorker.Helpers;

internal sealed class CaptureBodyHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode;
    private readonly Action<string> _capture;
 
    public CaptureBodyHandler(HttpStatusCode statusCode, Action<string> capture)
    {
        _statusCode = statusCode;
        _capture = capture;
    }
 
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var body = request.Content is not null
            ? await request.Content.ReadAsStringAsync(cancellationToken)
            : string.Empty;
        _capture(body);
        return new HttpResponseMessage(_statusCode);
    }
}