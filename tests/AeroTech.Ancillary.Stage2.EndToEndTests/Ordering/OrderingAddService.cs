using System.Globalization;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace AeroTech.Ancillary.Stage2.EndToEndTests.Ordering;

public sealed class OrderingAddService(TestOrderStore store, HttpClient ancillary)
{
    public const string ResolveRoute = "Service/v1/Ancillaries/Selections/Resolve";

    public async Task<(int Status, JsonObject Body)> AddAsync(long orderId, AddAncillariesRequest request, CancellationToken cancellationToken = default)
    {
        if (store.Find(orderId) is not { } order)
            return Problem(404, "OrderNotFound");

        if (request is not { ExpectedOrderCommercialVersion: { } expectedVersion, PricedSelectionRefs.Count: > 0, IdempotencyKey: { Length: > 0 } key })
            return Problem(422, "AddRequestIncomplete");

        var refs = request.PricedSelectionRefs!;
        var digest = Digest(expectedVersion, refs);

        await order.AddGate.WaitAsync(cancellationToken);

        try
        {
            if (order.AddsByIdempotencyKey.TryGetValue(key, out var earlier))
                return string.Equals(earlier.PayloadDigest, digest, StringComparison.Ordinal)
                    ? (earlier.Status, Replayed(earlier.Result))
                    : Problem(409, "IdempotencyKeyReusedWithAnotherPayload");

            int currentVersion;

            lock (order)
                currentVersion = order.CommercialVersion;

            if (currentVersion != expectedVersion)
                return Problem(409, "OrderVersionConflict", new JsonObject { ["currentCommercialVersion"] = currentVersion });

            using var response = await ancillary.PostAsJsonAsync(
                ResolveRoute,
                new { orderId, expectedOrderCommercialVersion = expectedVersion, pricedSelectionRefs = refs, correlationId = key },
                cancellationToken);
            var body = JsonNode.Parse(await response.Content.ReadAsStringAsync(cancellationToken))?.AsObject();

            if ((int)response.StatusCode != 200 || body?["data"] is not JsonObject data)
                return Problem(502, "AncillaryResolveFailed", new JsonObject { ["resolveStatus"] = (int)response.StatusCode, ["resolveBody"] = body?.DeepClone() });

            var problems = Validate(order, expectedVersion, refs, data);

            if (problems.Count > 0)
                return Problem(409, "SelectionsNotCommittable", new JsonObject { ["problems"] = problems, ["resolve"] = data.DeepClone() });

            var result = Apply(order, data);

            order.AddsByIdempotencyKey[key] = new TestAddRecord(digest, 201, result);

            return (201, (JsonObject)result.DeepClone());
        }
        finally
        {
            order.AddGate.Release();
        }
    }

    private static JsonArray Validate(TestOrder order, int expectedVersion, IReadOnlyList<string> refs, JsonObject data)
    {
        var problems = new JsonArray();

        lock (order)
        {
            if (Long(data["orderId"]) != order.OrderId)
                problems.Add(Issue(null, "ResolvedForAnotherOrder"));

            if (data["orderCommercialVersion"]?.GetValue<int>() != expectedVersion || order.CommercialVersion != expectedVersion)
                problems.Add(Issue(null, "OrderVersionMoved"));

            if (data["allValid"]?.GetValue<bool>() != true)
                problems.Add(Issue(null, "NotAllValid"));

            var results = data["results"]?.AsArray() ?? [];

            if (results.Count != refs.Count)
                problems.Add(Issue(null, "ResultCountMismatch"));

            var seatsTaken = order.Seats.Select(seat => (seat.FlightId, seat.SeatNumber.ToUpperInvariant()))
                .Concat(order.Services.Where(service => service.SeatNumber is not null).SelectMany(service => service.FlightIds.Select(flightId => (flightId, service.SeatNumber!.ToUpperInvariant()))))
                .ToHashSet();

            for (var index = 0; index < results.Count; index++)
            {
                var result = results[index]!.AsObject();
                var reference = result["pricedSelectionRef"]?.GetValue<string>();

                if (index < refs.Count && !string.Equals(reference, refs[index], StringComparison.Ordinal))
                    problems.Add(Issue(reference, "ResultOrderMismatch"));

                if (result["status"]?.GetValue<string>() != "Valid")
                {
                    problems.Add(Issue(reference, $"Status:{result["status"]?.GetValue<string>()}", string.Join(',', (result["reasonCodes"]?.AsArray() ?? []).Select(code => code!.GetValue<string>()))));
                    continue;
                }

                switch (result["service"], result["includedSeat"])
                {
                    case (JsonObject service, null):
                        ValidateService(order, reference, service, seatsTaken, problems);
                        break;
                    case (null, JsonObject seat):
                        ValidateIncludedSeat(order, reference, seat, seatsTaken, problems);
                        break;
                    default:
                        problems.Add(Issue(reference, "ServiceOrSeatExpected"));
                        break;
                }
            }
        }

        return problems;
    }

    private static void ValidateService(TestOrder order, string? reference, JsonObject service, HashSet<(long, string)> seatsTaken, JsonArray problems)
    {
        var travellerIds = Longs(service["orderTravellerIds"]);
        var travellerRefs = Strings(service["travellerRefs"]);
        var flightIds = Longs(service["flightIds"]);
        var price = service["price"] as JsonObject;

        if (service["coverageScope"]?.GetValue<string>() == "Order" || travellerIds.Count == 0)
            problems.Add(Issue(reference, "OrderScopedServiceNotRepresentable", "Ordering OrderService.TravellerId is a non-nullable long"));
        else if (travellerIds.Count != 1 || travellerRefs.Count != 1)
            problems.Add(Issue(reference, "MultiBeneficiaryServiceNotRepresentable"));
        else if (order.Travellers.SingleOrDefault(traveller => traveller.OrderTravellerId == travellerIds[0]) is not { } traveller
                 || !string.Equals(traveller.TravellerRef, travellerRefs[0], StringComparison.Ordinal))
            problems.Add(Issue(reference, "BeneficiaryNotInOrder"));

        if (flightIds.Count == 0 || flightIds.Any(flightId => order.AirServices.All(air => air.FlightId != flightId)))
            problems.Add(Issue(reference, "FlightNotInOrder"));

        if (price is null || price["currencyId"]?.GetValue<int>() != order.Air.CurrencyId || price["lineTotal"] is null || price["isTotalComplete"]?.GetValue<bool>() != true)
            problems.Add(Issue(reference, "PriceEvidenceMissing"));

        if (string.IsNullOrWhiteSpace(service["fulfillmentProviderKey"]?.GetValue<string>()))
            problems.Add(Issue(reference, "FulfillmentProviderMissing"));

        if (service["seatNumber"]?.GetValue<string>() is { } seatNumber)
        {
            var flightCapacityId = Long(service["flightCapacityId"]);

            if (travellerIds.Count == 1 && flightIds.Count == 1
                && order.AirServices.All(air => air.OrderTravellerId != travellerIds[0] || air.FlightId != flightIds[0] || air.FlightCapacityId != flightCapacityId))
                problems.Add(Issue(reference, "SeatCapacityMismatch"));

            if (flightIds.Count == 1 && !seatsTaken.Add((flightIds[0], seatNumber.ToUpperInvariant())))
                problems.Add(Issue(reference, "SeatAlreadyAssigned"));
        }
    }

    private static void ValidateIncludedSeat(TestOrder order, string? reference, JsonObject seat, HashSet<(long, string)> seatsTaken, JsonArray problems)
    {
        var travellerId = Long(seat["orderTravellerId"]);
        var flightId = Long(seat["flightId"]);
        var flightCapacityId = Long(seat["flightCapacityId"]);
        var seatNumber = seat["seatNumber"]?.GetValue<string>();

        if (order.AirServices.SingleOrDefault(air => air.OrderTravellerId == travellerId && air.FlightId == flightId) is not { } air)
            problems.Add(Issue(reference, "SeatTravellerOrFlightNotInOrder"));
        else if (air.FlightCapacityId != flightCapacityId)
            problems.Add(Issue(reference, "SeatCapacityMismatch"));

        if (string.IsNullOrWhiteSpace(seatNumber) || !seatsTaken.Add((flightId ?? 0, seatNumber.ToUpperInvariant())))
            problems.Add(Issue(reference, "SeatAlreadyAssigned"));
    }

    private JsonObject Apply(TestOrder order, JsonObject data)
    {
        var services = new JsonArray();
        var seats = new JsonArray();

        lock (order)
        {
            var version = order.CommercialVersion + 1;

            foreach (var result in data["results"]!.AsArray().Select(node => node!.AsObject()))
            {
                if (result["service"] is JsonObject service)
                {
                    var travellerId = Longs(service["orderTravellerIds"])[0];
                    var price = service["price"]!.AsObject();
                    var added = new TestOrderAncillaryService(
                        store.NextServiceId(),
                        travellerId,
                        Strings(service["travellerRefs"])[0],
                        service["kind"]!.GetValue<string>(),
                        service["serviceDefinitionRef"]!.GetValue<string>(),
                        service["variantCode"]!.GetValue<string>(),
                        service["rfisc"]?.GetValue<string>(),
                        service["ssrCode"]?.GetValue<string>(),
                        service["coverageScope"]!.GetValue<string>(),
                        Longs(service["flightIds"]),
                        Strings(service["portionRefs"]),
                        service["seatNumber"]?.GetValue<string>(),
                        Long(service["flightCapacityId"]),
                        price["quantity"]?.GetValue<int>(),
                        price["currencyId"]!.GetValue<int>(),
                        price["lineTotal"]!.GetValue<decimal>(),
                        service["fulfillmentProviderKey"]!.GetValue<string>(),
                        service["selection"]?.DeepClone(),
                        version);

                    order.Services.Add(added);
                    services.Add(added.OrderServiceId.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    var seat = result["includedSeat"]!.AsObject();
                    var travellerId = Long(seat["orderTravellerId"])!.Value;
                    var flightId = Long(seat["flightId"])!.Value;
                    var air = order.AirServices.Single(row => row.OrderTravellerId == travellerId && row.FlightId == flightId);
                    var added = new TestOrderSeat(
                        store.NextServiceId(),
                        travellerId,
                        seat["travellerRef"]!.GetValue<string>(),
                        flightId,
                        Long(seat["flightCapacityId"])!.Value,
                        seat["seatNumber"]!.GetValue<string>(),
                        seat["currencyId"]!.GetValue<int>(),
                        seat["fee"]!.GetValue<decimal>(),
                        air.FulfillmentProviderKey,
                        version);

                    order.Seats.Add(added);
                    seats.Add(added.OrderSeatId.ToString(CultureInfo.InvariantCulture));
                }
            }

            order.CommercialVersion = version;

            return new JsonObject
            {
                ["orderId"] = order.OrderId.ToString(CultureInfo.InvariantCulture),
                ["commercialVersion"] = version,
                ["addedServiceIds"] = services,
                ["addedSeatIds"] = seats,
                ["replayed"] = false
            };
        }
    }

    private static JsonObject Replayed(JsonObject result)
    {
        var copy = (JsonObject)result.DeepClone();

        copy["replayed"] = true;

        return copy;
    }

    private static (int, JsonObject) Problem(int status, string code, JsonObject? details = null)
    {
        var body = new JsonObject { ["error"] = code };

        foreach (var (name, value) in details ?? [])
            body[name] = value?.DeepClone();

        return (status, body);
    }

    private static JsonObject Issue(string? reference, string code, string? detail = null)
        => new() { ["pricedSelectionRef"] = reference is null ? null : reference[..Math.Min(16, reference.Length)], ["code"] = code, ["detail"] = detail };

    private static string Digest(int expectedVersion, IReadOnlyList<string> refs)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{expectedVersion}\n{string.Join('\n', refs)}")));

    private static long? Long(JsonNode? node)
        => node switch
        {
            null => null,
            JsonValue value when value.GetValueKind() == JsonValueKind.String => long.Parse(value.GetValue<string>(), CultureInfo.InvariantCulture),
            JsonValue value => value.GetValue<long>(),
            _ => null
        };

    private static IReadOnlyList<long> Longs(JsonNode? node) => (node?.AsArray() ?? []).Select(item => Long(item)!.Value).ToList();

    private static IReadOnlyList<string> Strings(JsonNode? node) => (node?.AsArray() ?? []).Select(item => item!.GetValue<string>()).ToList();
}
