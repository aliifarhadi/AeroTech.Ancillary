using System.Collections.Concurrent;
using System.Text.Json.Nodes;
using AeroTech.Ancillary.Shopping.Tests.Fixtures;

namespace AeroTech.Ancillary.Stage2.EndToEndTests.Host;

public sealed class TestAirAvail
{
    public const int BackofficeChannel = 2;
    public const int PartnerApiChannel = 3;

    private const string Journey = "1|1|0;9000000000000000001~9000000000000000101_9000000000000000002~9000000000000000102;";
    private const string Currency = "70";
    private const string SalesUnix = "1791706372";

    private readonly ConcurrentDictionary<string, Func<JsonObject, JsonObject>> _offers = new(StringComparer.Ordinal);

    public bool Unavailable { get; set; }

    public int Calls { get; private set; }

    public static string OfferId(int channel, long? customerId, long? countryId = null)
        => $"{Journey}{channel}|{customerId}|{countryId}|{Currency}|{SalesUnix}";

    public string Publish(int channel, long? customerId, long? countryId = null)
    {
        var offerId = OfferId(channel, customerId, countryId);

        _offers[offerId] = data => data;

        return offerId;
    }

    public void Change(string offerId, Action<JsonObject> change)
    {
        var previous = _offers[offerId];

        _offers[offerId] = data =>
        {
            var changed = previous(data);

            change(changed);

            return changed;
        };
    }

    public (int Status, string Body) Answer(string? offerId)
    {
        Calls++;

        if (Unavailable)
            return (503, """{"errors":[{"title":"AirAvail is unavailable"}]}""");

        if (offerId is null || !_offers.TryGetValue(offerId, out var shape))
            return (404, """{"errors":[{"title":"Offer not found","detail":"the offer is unknown or expired"}]}""");

        var root = JsonNode.Parse(OfferWire.Captured())!.AsObject();
        var data = shape(root["data"]!.AsObject());

        data["offerId"] = offerId;

        return (200, root.ToJsonString());
    }
}
