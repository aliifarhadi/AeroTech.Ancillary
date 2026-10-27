using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldSeatSelections;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Services;
using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers;
using AeroTech.Ancillary.Domain.AncillaryReservationAggregate;
using AeroTech.Ancillary.Query.Shopping.SeatMaps;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.P1Commands;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Holds;

[Collection(DatabaseCollection.Name)]
public class V11LifecycleAcceptanceTests
{
    private const long Flight = 301;
    private const long PricingId = 902;

    private readonly TestDatabase _database;
    private readonly FixedClock _clock = new();
    private readonly InventoryHarness _harness;

    public V11LifecycleAcceptanceTests(TestDatabase database)
    {
        _database = database;
        _harness = new InventoryHarness(database, _clock);
    }

    private static long NextOrderId => Interlocked.Increment(ref _lastOrderId);

    private static long _lastOrderId = 1_130_000;

    private sealed record Product(long DefinitionId, long ProvisionId);

    private async Task<IReadOnlyDictionary<string, Product>> CatalogAsync(params string[] codes)
    {
        var (fixture, airlineId, supplierId) = await _harness.OperatorAsync();
        var products = new Dictionary<string, Product>(StringComparer.Ordinal);

        foreach (var code in codes)
            products[code] = await PublishedAsync(fixture, airlineId, supplierId, V122Catalog.Case(code));

        return products;
    }

    private async Task<Product> PublishedAsync(InventoryFixture fixture, int airlineId, long supplierId, VariantCase variant)
    {
        var definition = await _harness.Proof.DefinitionAsync(V122Catalog.Define(variant, airlineId, supplierId) with
        {
            Booking = variant.Booking with { ConfirmationRequirement = ConfirmationRequirement.Immediate }
        });
        var rule = await _harness.Proof.RuleAsync(
            V122Catalog.Rule(variant, definition.Id) with { QuoteProviderKey = null },
            variant.Disposition == CommercialDisposition.Paid
                ? provisionId => new TestDefinePricingRatesCommand(provisionId, [new(null, null, null, new MoneyInput(25m, Eur), null)])
                : null);

        if (variant.Profile != AncillaryProfile.Seat)
            await _harness.ActivePolicyAsync(fixture, new TestDefineInventoryPolicyCommand(airlineId, variant.Reference, definition.Id, InventoryAuthority.Unlimited));

        return new Product(definition.Id, rule.Provision.Id);
    }

    private static HoldSelection Unit(string code, Product product, long orderServiceId, long travellerId = 71, ISelectionOptions? options = null, int? quantity = null)
        => new(V122Catalog.ProfileOf(code), code, product.ProvisionId, orderServiceId, travellerId, [Flight], quantity, options);

    private TestHoldAncillaryServicesCommand Hold(long orderId, string key, params HoldSelection[] selections)
        => M1Commands.Hold($"V11-L-{orderId}-{key}", orderId, _clock.Now.AddMinutes(30), selections);

    private static long UnitOf(AncillaryHoldReceipt hold, long orderServiceId) => hold.Units.Single(unit => unit.OrderServiceId == orderServiceId).UnitId;

    [Fact]
    public async Task V11_L02_L03_V12_W02_W03_Q1_a_replay_in_another_order_returns_the_same_hold_and_a_changed_option_quantity_family_or_variant_conflicts_without_a_write()
    {
        var catalog = await CatalogAsync("A03", "A11", "A15");
        var orderId = NextOrderId;
        var service = orderId * 10;
        await using var scope = new AncillaryScope(_database, _clock);
        HoldSelection[] selections =
        [
            Unit("A11", catalog["A11"], service + 1),
            Unit("A11", catalog["A11"], service + 2, travellerId: 72),
            Unit("A15", catalog["A15"], service + 3, options: new AssistedTravelHoldSelectionOptions("WCHR")),
            Unit("A03", catalog["A03"], service + 4, options: new BaggageSelectionOptions("B-32", 27.5m, null, null), quantity: 1)
        ];

        var hold = await scope.HoldAncillaryServices.HoldAsync(Hold(orderId, "SET", selections));
        var replay = await scope.HoldAncillaryServices.HoldAsync(Hold(orderId, "SET", Enumerable.Reverse(selections).ToArray()));

        Assert.Equal(hold.HoldId, replay.HoldId);
        Assert.Equal(
            hold.Units.Select(unit => (unit.OrderServiceId, unit.UnitId)).Order(),
            replay.Units.Select(unit => (unit.OrderServiceId, unit.UnitId)).Order());

        var bagRef = selections.ToArray();
        var ssr = selections.ToArray();
        var quantity = selections.ToArray();
        var variant = selections.ToArray();
        var family = selections.ToArray();
        bagRef[3] = bagRef[3] with { Options = new BaggageSelectionOptions("B-33", 27.5m, null, null) };
        ssr[2] = ssr[2] with { Options = new AssistedTravelHoldSelectionOptions("BLND") };
        quantity[3] = quantity[3] with { Quantity = 2 };
        variant[1] = variant[1] with { VariantCode = "A12" };
        family[1] = family[1] with { Family = AncillaryProfile.Upgrade };

        foreach (var changed in new[] { bagRef, ssr, quantity, variant, family })
            await BusinessAssert.ThrowsAsync(16403, 409, () => scope.HoldAncillaryServices.HoldAsync(Hold(orderId, "SET", changed)));

        Assert.Equal(
            new[] { "1|4" },
            await _harness.RowsAsync($"""
                SELECT CONCAT(COUNT(DISTINCT reservation.Id), '|', COUNT(unit.Id)) AS Value
                FROM Ancillary.AncillaryReservations AS reservation
                JOIN Ancillary.AncillaryReservationUnits AS unit ON unit.AncillaryReservationId = reservation.Id
                WHERE reservation.OrderId = {orderId}
                """));
    }

    [Fact]
    public async Task V12_W01_W02_W03_W04_W05_W06_the_hold_route_binds_strict_json_over_http_holds_on_sql_and_replays_only_the_same_request()
    {
        var catalog = await CatalogAsync("A01", "A11", "A15", "A20");
        var orderId = NextOrderId;
        var service = orderId * 10;
        await using var host = await ReservationHttpHost.StartAsync(_database, _clock);
        var body = new JsonObject
        {
            ["idempotencyKey"] = $"V12-HTTP-{orderId}",
            ["orderId"] = orderId,
            ["reference"] = $"ORD-{orderId}",
            ["requestedExpiresAt"] = _clock.Now.AddMinutes(30),
            ["baggages"] = Family("A01", catalog["A01"], service + 1, new JsonObject { ["quantity"] = 1 }),
            ["meals"] = Family("A11", catalog["A11"], service + 2),
            ["assistedTravel"] = Family("A15", catalog["A15"], service + 3, Options("assistedTravel", new JsonObject { ["assistanceSsrCode"] = "WCHR" })),
            ["airportServices"] = Family("A20", catalog["A20"], service + 4, Options("airportService", new JsonObject { ["guestCount"] = 1 }))
        };
        var reordered = new JsonObject(Enumerable.Reverse(body).Select(member => KeyValuePair.Create(member.Key, member.Value?.DeepClone())));
        var changedSsr = Changed(body, json => json["assistedTravel"]![0]!["products"]![0]!["units"]![0]!["selected"]!["options"]!["assistedTravel"]!["assistanceSsrCode"] = "BLND");
        var refused = new[]
        {
            Changed(body, json => json["baggages"]![0]!["products"]![0]!["units"]![0]!["coverageScope"] = "Sector"),
            Changed(body, json => json["meals"]![0]!["products"]![0]!["serviceDefinitionId"] = 1),
            Changed(body, json => json["inventoryFacts"] = new JsonObject()),
            Changed(body, json => json["meals"]![0]!["products"]![0]!["units"]![0]!["selected"] = Options("baggage", new JsonObject { ["bagRef"] = "B-1" })),
            Changed(body, json => json["pets"] = new JsonArray())
        };

        var created = await PostAsync(host, body);
        var replayed = await PostAsync(host, reordered);
        var conflicted = await PostAsync(host, changedSsr);
        var shapes = new List<HttpStatusCode>();

        foreach (var wrong in refused)
            shapes.Add((await PostAsync(host, wrong)).Status);

        Assert.Equal((HttpStatusCode.Created, 4), (created.Status, created.Body["data"]!["units"]!.AsArray().Count));
        Assert.Equal((HttpStatusCode.Created, created.Body["data"]!["holdId"]!.GetValue<string>()), (replayed.Status, replayed.Body["data"]!["holdId"]!.GetValue<string>()));
        Assert.Equal(HttpStatusCode.Conflict, conflicted.Status);
        Assert.Contains("16403", conflicted.Body.ToJsonString(), StringComparison.Ordinal);
        Assert.All(shapes, status => Assert.Equal(HttpStatusCode.BadRequest, status));
        Assert.Equal(
            new[] { "1|4" },
            await _harness.RowsAsync($"""
                SELECT CONCAT(COUNT(DISTINCT reservation.Id), '|', COUNT(unit.Id)) AS Value
                FROM Ancillary.AncillaryReservations AS reservation
                JOIN Ancillary.AncillaryReservationUnits AS unit ON unit.AncillaryReservationId = reservation.Id
                WHERE reservation.OrderId = {orderId}
                """));
    }

    private static JsonArray Family(string code, Product product, long orderServiceId, JsonObject? selected = null)
    {
        var unit = new JsonObject
        {
            ["orderServiceId"] = orderServiceId,
            ["unit"] = new JsonObject { ["travellerId"] = 71 },
            ["coverage"] = new JsonObject { ["flightIds"] = new JsonArray(Flight) }
        };

        if (selected is not null)
            unit["selected"] = selected;

        return new JsonArray(new JsonObject
        {
            ["variantCode"] = code,
            ["products"] = new JsonArray(new JsonObject { ["provisionId"] = product.ProvisionId, ["units"] = new JsonArray(unit) })
        });
    }

    private static JsonObject Options(string family, JsonObject options) => new() { ["options"] = new JsonObject { [family] = options } };

    private static JsonObject Changed(JsonObject body, Action<JsonObject> change)
    {
        var copy = body.DeepClone().AsObject();

        change(copy);

        return copy;
    }

    private static async Task<(HttpStatusCode Status, JsonNode Body)> PostAsync(ReservationHttpHost host, JsonObject body)
    {
        var response = await host.Client.PostAsync("Service/v1/Ancillaries/Hold", new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json"));
        var text = await response.Content.ReadAsStringAsync();

        return (response.StatusCode, JsonNode.Parse(text is { Length: > 0 } ? text : "{}")!);
    }

    [Fact]
    public async Task V11_L09_a_cancelled_and_an_expired_unit_keep_their_typed_selection_and_reason_in_the_get()
    {
        var catalog = await CatalogAsync("A03", "A15");
        var (cancelledOrder, expiredOrder) = (NextOrderId, NextOrderId);
        await using var scope = new AncillaryScope(_database, _clock);

        var cancelled = await scope.HoldAncillaryServices.HoldAsync(Hold(
            cancelledOrder,
            "CANCEL",
            Unit("A15", catalog["A15"], cancelledOrder * 10 + 1, options: new AssistedTravelHoldSelectionOptions("WCHR")),
            Unit("A03", catalog["A03"], cancelledOrder * 10 + 2, options: new BaggageSelectionOptions("B-32", 27.5m, null, null), quantity: 1)));
        var expired = await scope.HoldAncillaryServices.HoldAsync(Hold(
            expiredOrder,
            "EXPIRE",
            Unit("A03", catalog["A03"], expiredOrder * 10 + 1, options: new BaggageSelectionOptions("B-40", 30m, null, null), quantity: 1)));

        await scope.ConfirmAncillaryHold.ConfirmAsync(new TestConfirmAncillaryHoldCommand(cancelled.HoldId));
        await scope.CancelAncillaryHold.CancelAsync(new TestCancelAncillaryHoldCommand(cancelled.HoldId, "CUSTOMER_REQUEST", [UnitOf(cancelled, cancelledOrder * 10 + 1)]));
        _clock.Now = _clock.Now.AddMinutes(31);

        var afterCancel = await scope.GetHoldById.ExecuteAsync(cancelled.HoldId);
        var wheelchair = Assert.Single(Assert.Single(Assert.Single(afterCancel.AssistedTravel!).Products).Units);
        var bag = Assert.Single(Assert.Single(Assert.Single(afterCancel.Baggages!).Products).Units);

        Assert.Equal(
            (AncillaryReservationUnitStatus.Cancelled, "CUSTOMER_REQUEST", "WCHR"),
            (wheelchair.Status, wheelchair.CancellationReasonCode, wheelchair.Selected.Options!.AssistedTravel.AssistanceSsrCode));
        Assert.Equal((AncillaryReservationUnitStatus.Confirmed, "B-32"), (bag.Status, bag.Selected.Options!.Baggage.BagRef));

        string beforePoller;

        await using (var early = new AncillaryScope(_database, _clock))
            beforePoller = JsonSerializer.Serialize(await early.GetHoldById.ExecuteAsync(expired.HoldId), HostJson.Options);

        Assert.Equal(1, await scope.ExpireAncillaryHolds.ExpireAsync(expiredOrder));

        await using var reader = new AncillaryScope(_database, _clock);
        var afterPoller = await reader.GetHoldById.ExecuteAsync(expired.HoldId);
        var expiredBag = Assert.Single(Assert.Single(Assert.Single(afterPoller.Baggages!).Products).Units);

        Assert.Equal((AncillaryReservationStatus.Expired, AncillaryReservationUnitStatus.Expired), (afterPoller.Status, expiredBag.Status));
        Assert.Equal(("B-40", (decimal?)30m), (expiredBag.Selected.Options!.Baggage.BagRef, expiredBag.Selected.Options.Baggage.MeasuredWeightKg));
        Assert.Equal(beforePoller, JsonSerializer.Serialize(afterPoller, HostJson.Options));
    }

    [Fact]
    public async Task V11_L04_L05_L07_L12_L13_a_baggage_meal_and_seat_hold_keeps_every_family_through_partial_steps()
    {
        var catalog = await CatalogAsync("A03", "A11", "A07");
        var orderId = NextOrderId;
        var (bagService, mealService, seatService) = (orderId * 10 + 1, orderId * 10 + 2, orderId * 10 + 3);
        var expiresAt = _clock.Now.AddMinutes(30);
        await using var scope = new AncillaryScope(_database, _clock);
        scope.SeatSelections.Catalog[PricingId] = (catalog["A07"].DefinitionId, catalog["A07"].ProvisionId);

        var hold = await scope.HoldAncillaryServices.HoldAsync(M1Commands.Hold(
            $"V11-L-{orderId}",
            orderId,
            expiresAt,
            Unit("A03", catalog["A03"], bagService, options: new BaggageSelectionOptions("B-32", 27.5m, null, null), quantity: 1),
            Unit("A11", catalog["A11"], mealService)));
        var seat = new SeatHoldSelectionInput(
            seatService,
            71,
            SeatMapSelectionId.Encode("ADT-1", Flight, PricingId, "12A"),
            PassengerTypeCode.ADT,
            null,
            new SeatHoldFlightInput("111", 1, 1, 2, 6, new DateTimeOffset(2026, 12, 1, 10, 0, 0, TimeSpan.Zero), new DateTimeOffset(2026, 12, 1, 13, 0, 0, TimeSpan.Zero), 1, 1, 11),
            null);
        var append = new TestHoldSeatSelectionsCommand($"V11-L-SEAT-{orderId}", orderId, 1, $"ORD-{orderId}", expiresAt, 1, M1Commands.Currency, null, [seat], hold.HoldId);
        var appended = await scope.HoldSeatSelections.HoldAsync(append);
        var replayed = await scope.HoldSeatSelections.HoldAsync(append);

        Assert.Equal((hold.HoldId, hold.HoldId, 3), (appended.HoldId, replayed.HoldId, appended.Units.Count));
        Assert.Equal(appended.Units.Select(unit => unit.UnitId), replayed.Units.Select(unit => unit.UnitId));
        Assert.Single(scope.SeatSelections.Verified);

        await scope.ReleaseAncillaryHold.ReleaseAsync(new TestReleaseAncillaryHoldCommand(hold.HoldId, [UnitOf(appended, mealService)]));
        await scope.ConfirmAncillaryHold.ConfirmAsync(new TestConfirmAncillaryHoldCommand(hold.HoldId));
        var cancelled = await scope.CancelAncillaryHold.CancelAsync(new TestCancelAncillaryHoldCommand(hold.HoldId, "CUSTOMER_REQUEST", [UnitOf(appended, bagService)]));

        await using var reader = new AncillaryScope(_database, _clock);
        var read = await reader.GetHoldById.ExecuteAsync(hold.HoldId);
        var bag = Assert.Single(Assert.Single(Assert.Single(read.Baggages!).Products).Units);
        var meal = Assert.Single(Assert.Single(read.Meals!).Products);
        var seatVariant = Assert.Single(read.Seats!);
        var seatUnit = Assert.Single(Assert.Single(seatVariant.Products).Units);

        Assert.Equal(AncillaryReservationStatus.Confirmed, cancelled.Status);
        Assert.Equal((AncillaryReservationUnitStatus.Cancelled, "CUSTOMER_REQUEST", "B-32"), (bag.Status, bag.CancellationReasonCode, bag.Selected.Options!.Baggage.BagRef));
        Assert.Equal((AncillaryReservationUnitStatus.Released, "VGML"), (Assert.Single(meal.Units).Status, meal.Details!.MealCode));
        Assert.Equal(
            ("A07", catalog["A07"].ProvisionId, AncillaryReservationUnitStatus.Confirmed, "12A", seat.SeatMapSelectionId),
            (seatVariant.VariantCode, Assert.Single(seatVariant.Products).ProvisionId, seatUnit.Status, seatUnit.SeatNumber, seatUnit.SeatMapSelectionId));
        Assert.Empty(await _harness.AllocationsAsync(orderId));
        Assert.Null(read.AssistedTravel);
    }

    [Fact]
    public void V11_L21_one_reservation_aggregate_and_one_service_per_operation_serve_every_family()
    {
        var roots = typeof(AncillaryReservation).Assembly.GetTypes()
            .Where(type => type.Namespace?.StartsWith("AeroTech.Ancillary.Domain.AncillaryReservationAggregate", StringComparison.Ordinal) == true)
            .Where(type => type.BaseType is { IsGenericType: true } baseType && baseType.Name.StartsWith("AggregateRoot", StringComparison.Ordinal))
            .ToList();
        var services = typeof(HoldAncillaryServicesService).Assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false }
                           && type.Namespace?.StartsWith("AeroTech.Ancillary.Application.AncillaryReservationAggregate", StringComparison.Ordinal) == true
                           && type.Name.EndsWith("Service", StringComparison.Ordinal))
            .Select(type => type.Name)
            .Order()
            .ToArray();

        Assert.Equal(typeof(AncillaryReservation), Assert.Single(roots));
        Assert.Equal(
            new[]
            {
                "CancelAncillaryHoldService",
                "ConfirmAncillaryHoldService",
                "ExpireAncillaryHoldsService",
                "HoldAncillaryServicesService",
                "HoldSeatSelectionsService",
                "ReleaseAncillaryHoldService"
            },
            services);
    }
}
