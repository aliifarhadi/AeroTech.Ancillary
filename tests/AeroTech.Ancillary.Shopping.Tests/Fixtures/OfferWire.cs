using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using AeroTech.Ancillary.Providers.Offer.Services;
using AeroTech.Ancillary.Application.FlightOffers;

namespace AeroTech.Ancillary.Shopping.Tests.Fixtures;

public static class OfferWire
{
    public const string OfferId = "1|1|0;9000000000000000001~9000000000000000101_9000000000000000002~9000000000000000102;3|9000000000000000900||70|1791706372";

    public static readonly Uri BaseAddress = new("https://flight-offers.test/service/");

    public static string Captured() => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "Wire", "flight-offer-details.json"));

    public static string Changed(Action<JsonObject> change)
    {
        var root = JsonNode.Parse(Captured())!.AsObject();

        change(root["data"]!.AsObject());

        return root.ToJsonString();
    }

    public static FlightOfferProvider Provider(StubHandler handler, bool configured = true)
        => new(new HttpClient(handler) { BaseAddress = configured ? BaseAddress : null });

    public static Task<FlightOffer> ReadAsync(string json, string offerId = OfferId)
        => Provider(StubHandler.Json(HttpStatusCode.OK, json)).GetByOfferIdAsync(offerId);

    public sealed class StubHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _answer;

        public StubHandler(Func<HttpRequestMessage, HttpResponseMessage> answer) => _answer = answer;

        public int Calls { get; private set; }

        public HttpRequestMessage? LastRequest { get; private set; }

        public string? LastBody { get; private set; }

        public static StubHandler Json(HttpStatusCode status, string body)
            => new(_ => new HttpResponseMessage(status) { Content = new StringContent(body, Encoding.UTF8, "application/json") });

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Calls++;
            LastRequest = request;
            LastBody = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);

            return _answer(request);
        }
    }
}
