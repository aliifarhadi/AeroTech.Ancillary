using System.Net;
using System.Text;
using System.Text.Json.Nodes;

namespace AeroTech.Ancillary.Stage2.EndToEndTests.Host;

public sealed class AirAvailHandler(TestAirAvail airAvail) : HttpMessageHandler
{
    public const string DetailsPath = "/service/v1/FlightOffers/Details";

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Method != HttpMethod.Post || !string.Equals(request.RequestUri?.AbsolutePath, DetailsPath, StringComparison.Ordinal))
            return new HttpResponseMessage(HttpStatusCode.NotFound);

        var body = request.Content is null ? null : JsonNode.Parse(await request.Content.ReadAsStringAsync(cancellationToken))?.AsObject();
        var offerId = body?.FirstOrDefault(property => string.Equals(property.Key, "offerId", StringComparison.OrdinalIgnoreCase)).Value?.GetValue<string>();
        var (status, json) = airAvail.Answer(offerId);

        return new HttpResponseMessage((HttpStatusCode)status) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
    }
}
