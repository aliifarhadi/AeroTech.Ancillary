using System.Text.Json.Nodes;
using AeroTech.Ancillary.Stage2.EndToEndTests.Host;
using AeroTech.Ancillary.Stage2.EndToEndTests.Ordering;

namespace AeroTech.Ancillary.Stage2.EndToEndTests.Scenario;

public static class Stage2Flows
{
    public const string Api = E2EIdentities.Api;
    public const string Backoffice = E2EIdentities.Backoffice;
    public const string OtaPanel = E2EIdentities.OtaPanel;
    public const string Adult = "ADT-1";
    public const string Child = "CHD-1";
    public const long FirstFlightId = 1558420526869774453;
    public const long SecondFlightId = 1550116065952923910;

    public static string Traveller(string surface, string travellerRef)
        => surface == Api ? (travellerRef == Adult ? "1" : "2") : travellerRef;

    public static string Prefix(string surface)
        => surface switch
        {
            Backoffice => "Backoffice",
            OtaPanel => "OtaPanel",
            _ => "Api"
        };

    public static object Source(string? offerId, long? orderId) => new { offerId, orderId };

    public static Task<E2EResponse> ListAsync(Stage2World world, string identity, string surface, string? offerId = null, long? orderId = null)
        => Stage2World.PostAsync(world.Surface(identity), $"{Prefix(surface)}/v1/AncillaryOffers", Source(offerId, orderId));

    public static async Task<IReadOnlyList<ListedService>> ServicesAsync(Stage2World world, string surface, string? offerId = null, long? orderId = null)
    {
        var list = await ListAsync(world, surface, surface, offerId, orderId);

        if (list.Status != 200)
            throw new InvalidOperationException($"List {surface} answered {list.Status}: {list.Body?.ToJsonString()}");

        return surface switch
        {
            Backoffice => Stage2Json.BackofficeServices(list.Data),
            OtaPanel => Stage2Json.OtaPanelServices(list.Data),
            _ => Stage2Json.ApiServices(list.Data)
        };
    }

    public static ListedService Pick(IReadOnlyList<ListedService> services, string variantCode, string traveller)
        => services.Single(service => service.VariantCode == variantCode && service.Travellers.SequenceEqual([traveller]) && IsFirstBound(service));

    public static Task<E2EResponse> DetailsAsync(Stage2World world, string surface, string offerItemRef, string? offerId = null, long? orderId = null)
        => Stage2World.PostAsync(world.Surface(surface), $"{Prefix(surface)}/v1/AncillaryOffers/Details", new { offerId, orderId, offerItemRef });

    public static Task<E2EResponse> EvaluateAsync(Stage2World world, string surface, string offerId, string offerItemRef, object selection)
        => Stage2World.PostAsync(world.Surface(surface), $"{Prefix(surface)}/v1/AncillaryOffers/Selections/Evaluate", new { offerId, offerItemRef, selection });

    public static async Task<BatchView> BatchAsync(Stage2World world, string surface, IReadOnlyList<object> selections, string? offerId = null, long? orderId = null, string? identity = null)
        => BatchView.Of(
            surface,
            await Stage2World.PostAsync(world.Surface(identity ?? surface), $"{Prefix(surface)}/v1/AncillaryOffers/Selections/EvaluateBatch", new { offerId, orderId, selections }));

    public static object AncillaryLine(string clientSelectionId, string offerItemRef, object selection, IReadOnlyDictionary<string, string>? selectedOptionRefs = null)
        => new { clientSelectionId, kind = "Ancillary", offerItemRef, selection, selectedOptionRefs };

    public static object SeatLine(string clientSelectionId, string seatMapSelectionId, bool? exitRowTermsAccepted = null)
        => new { clientSelectionId, kind = "SeatMap", seatMapSelectionId, exitRowTermsAccepted };

    public static Task<E2EResponse> SeatMapsAsync(Stage2World world, string surface, string? offerId = null, long? orderId = null)
        => Stage2World.PostAsync(world.Surface(surface), $"{Prefix(surface)}/v1/SeatMaps", Source(offerId, orderId));

    public static async Task<long> CreateOrderAsync(Stage2World world, string offerId)
    {
        var created = await world.OrderingPostAsync($"{OrderingSimulator.Prefix}/from-offer", new { offerId });

        return created.Status == 201
            ? created.Body!["orderId"]!.GetValue<long>()
            : throw new InvalidOperationException($"from-offer answered {created.Status}: {created.Body?.ToJsonString()}");
    }

    public static async Task<JsonNode> OrderAsync(Stage2World world, long orderId)
        => (await world.OrderingGetAsync($"{OrderingSimulator.Prefix}/{orderId}")).Body!;

    public static Task<E2EResponse> AddAsync(Stage2World world, long orderId, int expectedVersion, IReadOnlyList<string> refs, string idempotencyKey)
        => world.OrderingPostAsync(
            $"{OrderingSimulator.Prefix}/{orderId}/ancillaries",
            new { expectedOrderCommercialVersion = expectedVersion, pricedSelectionRefs = refs, idempotencyKey });

    public static Task<E2EResponse> MutateAsync(Stage2World world, long orderId, string kind, string? travellerRef = null, string? boundId = null, bool? openForUse = null)
        => world.OrderingPostAsync($"{OrderingSimulator.Prefix}/{orderId}/mutations", new { kind, travellerRef, boundId, openForUse });

    public static string? SeatId(JsonNode seatMaps, int mapIndex, string seatNumber, string travelerId)
        => Stage2Json.SeatOffers(seatMaps, mapIndex)
            .Where(row => row.Number == seatNumber && row.Pricing["travelerId"]!.GetValue<string>() == travelerId)
            .Select(row => row.Pricing["id"]?.GetValue<string>())
            .SingleOrDefault();

    public static string SeatStatus(JsonNode seatMaps, int mapIndex, string seatNumber, string travelerId)
        => Stage2Json.SeatOffers(seatMaps, mapIndex)
            .Single(row => row.Number == seatNumber && row.Pricing["travelerId"]!.GetValue<string>() == travelerId)
            .Pricing["seatAvailabilityStatus"]!.GetValue<string>();

    private static bool IsFirstBound(ListedService service)
        => service.Surface switch
        {
            "api" => Stage2Json.Strings(service.Node["segmentIds"]).SequenceEqual(["1"]),
            "backoffice" => Stage2Json.Strings(service.Node["coverage"]!["portionRefs"]).SequenceEqual(["B1"]),
            _ => Stage2Json.Strings(service.Node["coveredFlightRefs"]).SequenceEqual(["B1:F1"])
        };
}
