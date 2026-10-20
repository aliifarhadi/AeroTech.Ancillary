namespace AeroTech.Ancillary.Stage2.EndToEndTests.Host;

public sealed class BlockedNetworkHandler(IsolationGuard guard, string clientName) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        guard.Record($"HTTP {clientName}: {request.Method} {request.RequestUri}");

        throw new HttpRequestException($"E2E isolation: the outbound call of '{clientName}' to {request.RequestUri} was blocked.");
    }
}
