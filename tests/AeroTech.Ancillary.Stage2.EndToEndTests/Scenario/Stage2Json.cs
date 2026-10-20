using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace AeroTech.Ancillary.Stage2.EndToEndTests.Scenario;

public static class Stage2Json
{
    public static IReadOnlyList<ListedService> ApiServices(JsonNode data)
        => Entries(data["included"]?["bags"])
            .Concat(Entries(data["included"]?["other-services"]))
            .Select(entry => new ListedService(
                "api",
                entry.Key,
                entry.Value["ancillary"]!["service"]!["variantCode"]!.GetValue<string>(),
                Strings(entry.Value["travelerIds"]),
                entry.Value["ancillary"]!["canPurchase"]!.GetValue<bool>(),
                entry.Value))
            .ToList();

    public static IReadOnlyList<ListedService> BackofficeServices(JsonNode data)
        => (data["offers"]?.AsArray() ?? [])
            .Concat(data["blockedOffers"]?.AsArray() ?? [])
            .Select(node => node!.AsObject())
            .Select(offer => new ListedService(
                "backoffice",
                offer["offerItemRef"]!.GetValue<string>(),
                offer["service"]!["variantCode"]!.GetValue<string>(),
                Strings(offer["coverage"]!["travellerRefs"]),
                offer["canPurchase"]!.GetValue<bool>(),
                offer))
            .ToList();

    public static IReadOnlyList<ListedService> OtaPanelServices(JsonNode data)
        => (data["travellerGroups"]?.AsArray() ?? [])
            .Select(group => group!.AsObject())
            .SelectMany(group => (group["flightGroups"]?.AsArray() ?? [])
                .SelectMany(flight => flight!["services"]?.AsArray() ?? [])
                .Select(service => (Traveller: group["travellerRef"]!.GetValue<string>(), Service: service!.AsObject())))
            .Concat((data["orderScopedServices"]?.AsArray() ?? []).Select(service => (Traveller: string.Empty, Service: service!.AsObject())))
            .GroupBy(row => row.Service["offerItemRef"]!.GetValue<string>(), StringComparer.Ordinal)
            .Select(group => new ListedService(
                "otapanel",
                group.Key,
                group.First().Service["variantCode"]!.GetValue<string>(),
                group.Select(row => row.Traveller).Where(traveller => traveller.Length > 0).Distinct(StringComparer.Ordinal).ToList(),
                group.First().Service["canPurchase"]!.GetValue<bool>(),
                group.First().Service))
            .ToList();

    public static IEnumerable<(string Number, JsonObject Pricing)> SeatOffers(JsonNode seatMaps, int mapIndex)
        => seatMaps["data"]![mapIndex]!["decks"]!.AsArray()
            .SelectMany(deck => deck!["cabins"]!.AsArray())
            .SelectMany(cabin => cabin!["items"]!.AsArray())
            .Where(item => item!["number"] is not null)
            .SelectMany(item => item!["travelerPricing"]!.AsArray().Select(pricing => (item["number"]!.GetValue<string>(), pricing!.AsObject())));

    public static long Long(JsonNode? node)
        => node is JsonValue value && value.GetValueKind() == JsonValueKind.String
            ? long.Parse(value.GetValue<string>(), CultureInfo.InvariantCulture)
            : node!.GetValue<long>();

    public static IReadOnlyList<string> Strings(JsonNode? node) => (node?.AsArray() ?? []).Select(item => item!.GetValue<string>()).ToList();

    private static IEnumerable<KeyValuePair<string, JsonObject>> Entries(JsonNode? node)
        => (node?.AsObject() ?? []).Select(entry => new KeyValuePair<string, JsonObject>(entry.Key, entry.Value!.AsObject()));
}
