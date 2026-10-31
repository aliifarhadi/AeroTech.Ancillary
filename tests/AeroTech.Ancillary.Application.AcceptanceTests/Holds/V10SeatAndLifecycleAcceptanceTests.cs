using System.Reflection;
using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.CancelAncillaryHold;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.ConfirmAncillaryHold;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.ExpireAncillaryHolds;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldSeatSelections;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldSeatSelections.Service;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.ReleaseAncillaryHold;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Services;
using AeroTech.Ancillary.Application.FlightOffers;
using AeroTech.Ancillary.Application.Orders;
using AeroTech.Ancillary.Persistence.AncillaryReservationAggregate;
using AeroTech.Ancillary.Providers.AirInfo.Services;
using AeroTech.Ancillary.Providers.FlightFlow.Services;
using AeroTech.Ancillary.Query.AncillaryReservationAggregate.Queries.GetAncillaryHoldById;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.VerifyHoldSelections;
using AeroTech.Ancillary.Query.Shopping.SeatMaps;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.VerifySeatSelections;
using AeroTech.Ancillary.Shopping.Engine;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.P2Commands;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Holds;

[Collection(DatabaseCollection.Name)]
public class V10SeatAndLifecycleAcceptanceTests
{
    private const long Flight = 301;
    private const long PricingId = 901;

    private readonly TestDatabase _database;
    private readonly FixedClock _clock = new();
    private readonly InventoryHarness _harness;
    private readonly TestOnlyHoldLock _lock = new();

    public V10SeatAndLifecycleAcceptanceTests(TestDatabase database)
    {
        _database = database;
        _harness = new InventoryHarness(database, _clock);
    }

    private static long NextOrderId => Interlocked.Increment(ref _lastOrderId);

    private static long _lastOrderId = 970_000;

    private sealed record Product(long DefinitionId, long ProvisionId);

    private sealed record Catalog(InventoryFixture Fixture, Product Seat, Product Bag, Product Meal, long BagSource);

    private async Task<Product> ProductAsync(int airlineId, long supplierId, string reference)
    {
        var definition = await _harness.Proof.DefinitionAsync(M1Commands.LoungeDefinition(airlineId, supplierId, reference));
        var rule = await _harness.Proof.RuleAsync(M1Commands.LoungeProvision(definition.Id));

        return new Product(definition.Id, rule.Provision.Id);
    }

    private async Task<Catalog> CatalogAsync()
    {
        var (fixture, airlineId, supplierId) = await _harness.OperatorAsync();

        fixture.Flights!.Add(Flight);
        fixture.Resources!.Add((InventoryResourceKind.FlightCount, OversizeResource));

        var seat = await ProductAsync(airlineId, supplierId, "G3_SEAT");
        var bag = await ProductAsync(airlineId, supplierId, "G3_BAG");
        var meal = await ProductAsync(airlineId, supplierId, "G3_MEAL");

        await _harness.ActivePolicyAsync(fixture, new TestDefineInventoryPolicyCommand(
            airlineId, "G3_BAG", bag.DefinitionId, InventoryAuthority.Local, LocalInventoryPattern.FlightCount, CountConsumption: Count(OversizeResource, 1, InventoryCountUnit.Item)));
        await _harness.ActivePolicyAsync(fixture, new TestDefineInventoryPolicyCommand(airlineId, "G3_MEAL", meal.DefinitionId, InventoryAuthority.Unlimited));

        var source = await _harness.RequestAsync(fixture, scope => scope.DefineFlightCountInventory.DefineAsync(
            new TestDefineFlightCountInventoryCommand(airlineId, Flight, OversizeResource, InventoryCountUnit.Item, 5)));
        await _harness.RequestAsync(fixture, scope => scope.ActivateFlightCountInventory.ActivateAsync(new TestFlightCountLifecycleCommand(source.Id, 1)));

        return new Catalog(fixture, seat, bag, meal, source.Id);
    }

    private AncillaryScope Scope(Catalog catalog)
    {
        var scope = new AncillaryScope(_database, _clock, _lock);

        scope.SeatSelections.Catalog[PricingId] = (catalog.Seat.DefinitionId, catalog.Seat.ProvisionId);

        return scope;
    }

    private static string SeatId(string travellerRef, string seatNumber, long pricingId = PricingId)
        => SeatMapSelectionId.Encode(travellerRef, Flight, pricingId, seatNumber, 1);

    private static SeatHoldSelectionInput Seat(long orderServiceId, long travellerId, string seatMapSelectionId)
        => new(
            orderServiceId,
            travellerId,
            seatMapSelectionId,
            PassengerTypeCode.ADT,
            null,
            new SeatHoldFlightInput("111", 1, 1, 2, 6, new DateTimeOffset(2026, 12, 1, 10, 0, 0, TimeSpan.Zero), new DateTimeOffset(2026, 12, 1, 13, 0, 0, TimeSpan.Zero), 1, 1, 11),
            null);

    private static TestHoldSeatSelectionsCommand SeatHold(long orderId, string key, DateTimeOffset expiresAt, long? holdId, params SeatHoldSelectionInput[] seats)
        => new(key, orderId, 1, $"ORD-{orderId}", expiresAt, 1, M1Commands.Currency, null, seats, holdId);

    private static TestHoldAncillaryServicesCommand Hold(long orderId, DateTimeOffset expiresAt, params HoldSelection[] services)
        => M1Commands.Hold($"G3-{orderId}", orderId, expiresAt, services);

    private static HoldSelection Service(Product product, long orderServiceId, long travellerId)
        => M1Commands.Lounge(product.ProvisionId, orderServiceId, travellerId, [Flight]);

    private static long UnitOf(AncillaryHoldReceipt hold, long orderServiceId) => hold.Units.Single(unit => unit.OrderServiceId == orderServiceId).UnitId;

    private static IEnumerable<(long, AncillaryReservationUnitStatus)> Statuses(AncillaryHoldReceipt hold)
        => hold.Units.OrderBy(unit => unit.OrderServiceId).Select(unit => (unit.OrderServiceId, unit.Status));

    [Fact]
    public async Task V10_S01_S08_S09_S15_a_seat_only_hold_has_one_sector_unit_per_seat_and_no_allocation()
    {
        var catalog = await CatalogAsync();
        await using var scope = Scope(catalog);
        var orderId = NextOrderId;
        var seatId = SeatId("ADT-1", "3A");

        var hold = await scope.HoldSeatSelections.HoldAsync(SeatHold(orderId, $"G3-SEAT-{orderId}", _clock.Now.AddMinutes(30), null, Seat(orderId * 10 + 1, 71, seatId)));
        var unit = Assert.Single((await scope.Reservations.GetAsync(hold.HoldId))!.Units);

        Assert.Equal(AncillaryReservationStatus.Held, hold.Status);
        Assert.Equal(
            (1, ServiceCoverageScope.Sector, Flight, "3A", PricingId, seatId, (string?)null),
            (unit.Quantity, unit.CoverageScope, unit.CoveredFlightIds.Single(), unit.SeatNumber!, unit.PricingId!.Value, unit.SeatMapSelectionId!, unit.AppendIdempotencyKey));
        Assert.Empty(unit.Allocations);
        Assert.Empty(await _harness.AllocationsAsync(orderId));
    }

    [Fact]
    public async Task V10_S02_S05_S09_S11_E08_a_seat_appended_to_a_mixed_hold_keeps_the_hold_id_and_every_partial_step_keeps_the_stock_exact()
    {
        var catalog = await CatalogAsync();
        await using var scope = Scope(catalog);
        var orderId = NextOrderId;
        var expiresAt = _clock.Now.AddMinutes(30);
        var (firstBag, secondBag, meal, seat) = (orderId * 10 + 1, orderId * 10 + 2, orderId * 10 + 3, orderId * 10 + 4);

        var hold = await scope.HoldAncillaryServices.HoldAsync(Hold(orderId, expiresAt, Service(catalog.Bag, firstBag, 71), Service(catalog.Bag, secondBag, 72), Service(catalog.Meal, meal, 71)));

        Assert.Equal("2.000", await _harness.UsageAsync(InventoryResourceKind.FlightCount, catalog.BagSource));

        var append = SeatHold(orderId, $"G3-SEAT-{orderId}", expiresAt, hold.HoldId, Seat(seat, 71, SeatId("ADT-1", "3A")));
        var appended = await scope.HoldSeatSelections.HoldAsync(append);
        var replayed = await scope.HoldSeatSelections.HoldAsync(append);
        var stored = (await scope.Reservations.GetAsync(hold.HoldId))!.Units.Single(unit => unit.OrderServiceId == seat);

        Assert.Equal((hold.HoldId, hold.HoldId, 4, 4), (appended.HoldId, replayed.HoldId, appended.Units.Count, replayed.Units.Count));
        Assert.Equal(appended.Units.Select(unit => unit.UnitId), replayed.Units.Select(unit => unit.UnitId));
        Assert.Single(scope.SeatSelections.Verified);
        Assert.Equal(($"G3-SEAT-{orderId}", 0), (stored.AppendIdempotencyKey!, stored.Allocations.Count));
        Assert.Equal(
            new[] { $"{(int)InventoryResourceKind.FlightCount}:{catalog.BagSource}:1.000", $"{(int)InventoryResourceKind.FlightCount}:{catalog.BagSource}:1.000" },
            await _harness.AllocationsAsync(orderId));

        var released = await scope.ReleaseAncillaryHold.ReleaseAsync(new TestReleaseAncillaryHoldCommand(hold.HoldId, [UnitOf(appended, meal)]));
        var confirmed = await scope.ConfirmAncillaryHold.ConfirmAsync(new TestConfirmAncillaryHoldCommand(hold.HoldId));

        Assert.Equal("2.000", await _harness.UsageAsync(InventoryResourceKind.FlightCount, catalog.BagSource));

        var cancelled = await scope.CancelAncillaryHold.CancelAsync(new TestCancelAncillaryHoldCommand(hold.HoldId, "CUSTOMER_REQUEST", [UnitOf(appended, firstBag)]));
        var read = await scope.GetHoldById.ExecuteAsync(hold.HoldId);

        Assert.Equal(
            (AncillaryReservationStatus.Held, AncillaryReservationStatus.Confirmed, AncillaryReservationStatus.Confirmed),
            (released.Status, confirmed.Status, cancelled.Status));
        Assert.Equal(
            new[]
            {
                (firstBag, AncillaryReservationUnitStatus.Cancelled),
                (secondBag, AncillaryReservationUnitStatus.Confirmed),
                (meal, AncillaryReservationUnitStatus.Released),
                (seat, AncillaryReservationUnitStatus.Confirmed)
            },
            Statuses(cancelled));
        Assert.Equal(Statuses(cancelled), read.Units().OrderBy(unit => unit.OrderServiceId).Select(unit => (unit.OrderServiceId, unit.Status)));
        Assert.Equal("1.000", await _harness.UsageAsync(InventoryResourceKind.FlightCount, catalog.BagSource));
        Assert.Equal("5|2", (await _harness.RowsAsync($"SELECT CONCAT(TotalCapacity, '|', Version) AS Value FROM Ancillary.FlightCountInventories WHERE Id = {catalog.BagSource}")).Single());
    }

    [Fact]
    public async Task V10_S03_S04_L23_E07_an_append_that_does_not_match_a_live_held_hold_changes_nothing()
    {
        var catalog = await CatalogAsync();
        await using var scope = Scope(catalog);
        var orderId = NextOrderId;
        var expiresAt = _clock.Now.AddMinutes(30);
        var hold = await scope.HoldAncillaryServices.HoldAsync(Hold(orderId, expiresAt, Service(catalog.Meal, orderId * 10 + 1, 71)));
        var seat = Seat(orderId * 10 + 2, 71, SeatId("ADT-1", "3A"));

        await BusinessAssert.ThrowsAsync(16402, 422, () => scope.HoldSeatSelections.HoldAsync(SeatHold(orderId + 1, $"G3-A-{orderId}", expiresAt, hold.HoldId, seat) with { Reference = $"ORD-{orderId}" }));
        await BusinessAssert.ThrowsAsync(16402, 422, () => scope.HoldSeatSelections.HoldAsync(SeatHold(orderId, $"G3-B-{orderId}", expiresAt, hold.HoldId, seat) with { Reference = "ORD-OTHER" }));
        await BusinessAssert.ThrowsAsync(16402, 422, () => scope.HoldSeatSelections.HoldAsync(SeatHold(orderId, $"G3-C-{orderId}", expiresAt.AddMinutes(10), hold.HoldId, seat)));
        await BusinessAssert.ThrowsAsync(16415, 422, () => scope.HoldSeatSelections.HoldAsync(SeatHold(orderId, $"G3-D-{orderId}", expiresAt, hold.HoldId, Seat(orderId * 10 + 2, 71, SeatId("ADT-1", "3A", 999)))));
        await BusinessAssert.ThrowsAsync(16401, 404, () => scope.HoldSeatSelections.HoldAsync(SeatHold(orderId, $"G3-E-{orderId}", expiresAt, 999_999_999, seat)));

        var untouched = await scope.GetHoldById.ExecuteAsync(hold.HoldId);

        Assert.Equal((AncillaryReservationStatus.Held, 1, expiresAt), (untouched.Status, untouched.Units().Count, untouched.ExpiresAt!.Value));

        var confirmedOrder = NextOrderId;
        var confirmed = await scope.HoldAncillaryServices.HoldAsync(Hold(confirmedOrder, expiresAt, Service(catalog.Meal, confirmedOrder * 10 + 1, 71)));
        await scope.ConfirmAncillaryHold.ConfirmAsync(new TestConfirmAncillaryHoldCommand(confirmed.HoldId));

        await BusinessAssert.ThrowsAsync(16411, 409, () => scope.HoldSeatSelections.HoldAsync(
            SeatHold(confirmedOrder, $"G3-F-{confirmedOrder}", expiresAt, confirmed.HoldId, Seat(confirmedOrder * 10 + 2, 71, SeatId("ADT-1", "3A")))));

        _clock.Now = expiresAt;

        await BusinessAssert.ThrowsAsync(16407, 409, () => scope.HoldSeatSelections.HoldAsync(SeatHold(orderId, $"G3-G-{orderId}", expiresAt, hold.HoldId, seat)));
        Assert.Single((await scope.Reservations.GetAsync(hold.HoldId))!.Units);
    }

    [Fact]
    public async Task V10_S06_S07_L21_a_reused_append_key_a_seat_held_twice_or_a_live_order_service_is_refused()
    {
        var catalog = await CatalogAsync();
        await using var scope = Scope(catalog);
        var orderId = NextOrderId;
        var expiresAt = _clock.Now.AddMinutes(30);
        var hold = await scope.HoldAncillaryServices.HoldAsync(Hold(orderId, expiresAt, Service(catalog.Meal, orderId * 10 + 1, 71)));
        var key = $"G3-SEAT-{orderId}";

        await scope.HoldSeatSelections.HoldAsync(SeatHold(orderId, key, expiresAt, hold.HoldId, Seat(orderId * 10 + 2, 71, SeatId("ADT-1", "3A"))));

        await BusinessAssert.ThrowsAsync(16403, 409, () => scope.HoldSeatSelections.HoldAsync(SeatHold(orderId, key, expiresAt, hold.HoldId, Seat(orderId * 10 + 2, 71, SeatId("ADT-1", "3B")))));
        await BusinessAssert.ThrowsAsync(16402, 422, () => scope.HoldSeatSelections.HoldAsync(SeatHold(orderId, $"{key}-2", expiresAt, hold.HoldId, Seat(orderId * 10 + 3, 72, SeatId("ADT-2", "3A")))));
        await BusinessAssert.ThrowsAsync(16406, 409, () => scope.HoldSeatSelections.HoldAsync(SeatHold(orderId, $"{key}-3", expiresAt, hold.HoldId, Seat(orderId * 10 + 1, 71, SeatId("ADT-1", "4A")))));

        var otherOrder = NextOrderId;
        var other = await scope.HoldAncillaryServices.HoldAsync(Hold(otherOrder, expiresAt, Service(catalog.Meal, otherOrder * 10 + 1, 71)));

        await BusinessAssert.ThrowsAsync(16406, 409, () => scope.HoldSeatSelections.HoldAsync(SeatHold(otherOrder, $"G3-SEAT-{otherOrder}", expiresAt, other.HoldId, Seat(orderId * 10 + 2, 71, SeatId("ADT-1", "5A")))));
        Assert.Equal(
            new[] { $"{hold.HoldId}|2", $"{other.HoldId}|1" },
            await _harness.RowsAsync($"SELECT CONCAT(AncillaryReservationId, '|', COUNT(*)) AS Value FROM Ancillary.AncillaryReservationUnits WHERE AncillaryReservationId IN ({hold.HoldId}, {other.HoldId}) GROUP BY AncillaryReservationId ORDER BY AncillaryReservationId"));
    }

    [Fact]
    public async Task V10_E05_a_seat_replay_returns_the_stored_hold_without_verifying_again_even_when_the_seat_would_no_longer_sell()
    {
        var catalog = await CatalogAsync();
        await using var scope = Scope(catalog);
        var orderId = NextOrderId;
        var expiresAt = _clock.Now.AddMinutes(30);
        var create = SeatHold(orderId, $"G3-SEAT-{orderId}", expiresAt, null, Seat(orderId * 10 + 1, 71, SeatId("ADT-1", "3A")));
        var created = await scope.HoldSeatSelections.HoldAsync(create);
        var append = SeatHold(orderId, $"G3-APPEND-{orderId}", expiresAt, created.HoldId, Seat(orderId * 10 + 2, 72, SeatId("ADT-2", "3B")));
        var appended = await scope.HoldSeatSelections.HoldAsync(append);

        scope.SeatSelections.Catalog.Clear();

        var createReplay = await scope.HoldSeatSelections.HoldAsync(create);
        var appendReplay = await scope.HoldSeatSelections.HoldAsync(append);

        Assert.Equal(2, scope.SeatSelections.Verified.Count);
        Assert.Equal((created.HoldId, created.HoldId), (createReplay.HoldId, appendReplay.HoldId));
        Assert.Equal(appended.Units.Select(unit => unit.UnitId), appendReplay.Units.Select(unit => unit.UnitId));
        await BusinessAssert.ThrowsAsync(16403, 409, () => scope.HoldSeatSelections.HoldAsync(create with { Seats = [Seat(orderId * 10 + 1, 71, SeatId("ADT-1", "3C"))] }));
        Assert.Equal(2, scope.SeatSelections.Verified.Count);
    }

    [Fact]
    public async Task V10_L03_L06_L11_L16_every_lifecycle_operation_takes_the_order_lock_and_a_busy_order_changes_nothing()
    {
        var catalog = await CatalogAsync();
        await using var scope = Scope(catalog);
        var orderId = NextOrderId;
        var expiresAt = _clock.Now.AddMinutes(30);
        var hold = await scope.HoldAncillaryServices.HoldAsync(Hold(orderId, expiresAt, Service(catalog.Bag, orderId * 10 + 1, 71)));

        await using (await _lock.AcquireAsync(orderId))
        {
            await BusinessAssert.ThrowsAsync(16416, 409, () => scope.ConfirmAncillaryHold.ConfirmAsync(new TestConfirmAncillaryHoldCommand(hold.HoldId)));
            await BusinessAssert.ThrowsAsync(16416, 409, () => scope.ReleaseAncillaryHold.ReleaseAsync(new TestReleaseAncillaryHoldCommand(hold.HoldId)));
            await BusinessAssert.ThrowsAsync(16416, 409, () => scope.CancelAncillaryHold.CancelAsync(new TestCancelAncillaryHoldCommand(hold.HoldId, "CUSTOMER_REQUEST")));
            await BusinessAssert.ThrowsAsync(16416, 409, () => scope.ExpireAncillaryHolds.ExpireAsync(orderId));
            await BusinessAssert.ThrowsAsync(16416, 409, () => scope.HoldSeatSelections.HoldAsync(
                SeatHold(orderId, $"G3-SEAT-{orderId}", expiresAt, hold.HoldId, Seat(orderId * 10 + 2, 71, SeatId("ADT-1", "3A")))));
        }

        Assert.Equal((AncillaryReservationStatus.Held, 1), ((await scope.GetHoldById.ExecuteAsync(hold.HoldId)).Status, (await scope.GetHoldById.ExecuteAsync(hold.HoldId)).Units().Count));

        _clock.Now = expiresAt;

        Assert.Equal("0.000", await _harness.UsageAsync(InventoryResourceKind.FlightCount, catalog.BagSource));
        Assert.Contains(orderId, await scope.ExpireAncillaryHolds.ListDueOrderIdsAsync(1000));
        Assert.Equal((1, 0), (await scope.ExpireAncillaryHolds.ExpireAsync(orderId), await scope.ExpireAncillaryHolds.ExpireAsync(orderId)));
        Assert.DoesNotContain(orderId, await scope.ExpireAncillaryHolds.ListDueOrderIdsAsync(1000));
        Assert.Equal(AncillaryReservationStatus.Expired, (await scope.GetHoldById.ExecuteAsync(hold.HoldId)).Status);
        Assert.Equal(
            $"{(int)AncillaryReservationStatus.Expired}|{(int)AncillaryReservationUnitStatus.Expired}",
            (await _harness.RowsAsync($"SELECT CONCAT(reservation.Status, '|', unit.Status) AS Value FROM Ancillary.AncillaryReservations AS reservation JOIN Ancillary.AncillaryReservationUnits AS unit ON unit.AncillaryReservationId = reservation.Id WHERE reservation.Id = {hold.HoldId}")).Single());
    }

    [Fact]
    public async Task V10_S02_concurrent_appends_to_one_hold_are_serialized_by_the_order_lock()
    {
        var catalog = await CatalogAsync();
        var orderId = NextOrderId;
        var expiresAt = _clock.Now.AddMinutes(30);
        AncillaryHoldReceipt hold;

        await using (var scope = Scope(catalog))
            hold = await scope.HoldAncillaryServices.HoldAsync(Hold(orderId, expiresAt, Service(catalog.Meal, orderId * 10 + 1, 71)));

        async Task AppendAsync(TestHoldSeatSelectionsCommand command)
        {
            await using var scope = Scope(catalog);
            await scope.HoldSeatSelections.HoldAsync(command);
        }

        var distinct = await _harness.RaceAsync(
        [
            () => AppendAsync(SeatHold(orderId, $"G3-A-{orderId}", expiresAt, hold.HoldId, Seat(orderId * 10 + 2, 71, SeatId("ADT-1", "3A")))),
            () => AppendAsync(SeatHold(orderId, $"G3-B-{orderId}", expiresAt, hold.HoldId, Seat(orderId * 10 + 3, 72, SeatId("ADT-2", "3B"))))
        ]);
        var same = SeatHold(orderId, $"G3-C-{orderId}", expiresAt, hold.HoldId, Seat(orderId * 10 + 4, 73, SeatId("ADT-3", "3C")));
        var repeated = await _harness.RaceAsync([() => AppendAsync(same), () => AppendAsync(same)]);
        var units = await _harness.RowsAsync($"SELECT CONCAT(OrderServiceId, '|', COALESCE(SeatNumber, '-')) AS Value FROM Ancillary.AncillaryReservationUnits WHERE AncillaryReservationId = {hold.HoldId} ORDER BY OrderServiceId");

        Assert.InRange(distinct.Succeeded, 1, 2);
        Assert.Equal(distinct.Succeeded == 2 ? Array.Empty<int>() : new[] { 16416 }, distinct.Codes);
        Assert.InRange(repeated.Succeeded, 1, 2);
        Assert.Equal(repeated.Succeeded == 2 ? Array.Empty<int>() : new[] { 16416 }, repeated.Codes);
        Assert.Equal(2 + distinct.Succeeded, units.Count);
        Assert.Equal(units.Count, units.Distinct().Count());
        Assert.Single(units, unit => unit.EndsWith("|3C", StringComparison.Ordinal));
    }

    [Fact]
    public void V10_S10_S11_V11_L15_L17_L18_no_type_reachable_from_a_reservation_operation_can_reach_flightflow_or_ordering()
    {
        Assembly[] solution =
        [
            typeof(HoldSeatSelectionsService).Assembly,
            typeof(SeatSelectionVerifier).Assembly,
            typeof(AncillaryShoppingEngine).Assembly,
            typeof(InventoryLedger).Assembly,
            typeof(FlightFlowOccupiedSeatProvider).Assembly
        ];
        var implementations = solution
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type is { IsClass: true, IsAbstract: false })
            .ToList();
        var reached = new HashSet<Type>();
        var pending = new Queue<Type>(
        [
            typeof(HoldSeatSelectionsService), typeof(ConfirmAncillaryHoldService), typeof(ReleaseAncillaryHoldService),
            typeof(CancelAncillaryHoldService), typeof(ExpireAncillaryHoldsService), typeof(ISeatSelectionVerifier),
            typeof(HoldAncillaryServicesService), typeof(GetAncillaryHoldByIdService)
        ]);

        while (pending.TryDequeue(out var type))
        {
            if (!reached.Add(type))
                continue;

            var next = type.IsInterface
                ? implementations.Where(type.IsAssignableFrom)
                : type.GetConstructors().SelectMany(constructor => constructor.GetParameters()).Select(parameter => parameter.ParameterType);

            foreach (var dependency in next.Where(dependency => dependency.Namespace?.StartsWith("AeroTech.Ancillary", StringComparison.Ordinal) == true))
                pending.Enqueue(dependency);
        }

        Assert.Contains(typeof(SeatSelectionVerifier), reached);
        Assert.Contains(typeof(AirInfoAircraftSeatMapProvider), reached);
        Assert.Contains(typeof(HoldAncillaryServicesService), reached);
        Assert.Contains(typeof(InventoryLedger), reached);
        Assert.Contains(typeof(HoldSelectionVerifier), reached);
        Assert.DoesNotContain(typeof(IOccupiedSeatProvider), reached);
        Assert.DoesNotContain(reached, type => type.Namespace!.Contains(".FlightFlow", StringComparison.Ordinal));
        Assert.DoesNotContain(typeof(IOrderCommercialContextReader), reached);
        Assert.DoesNotContain(reached, type => type.Namespace!.Contains(".Orders", StringComparison.Ordinal));
    }

    [Fact]
    public void V10_S02_the_seat_hold_contract_takes_an_optional_positive_hold_id_only()
    {
        var validator = new ServiceHoldSeatSelectionsCommandValidator();
        var seat = Seat(1, 101, SeatId("ADT-1", "3A"));
        ServiceHoldSeatSelectionsCommand Command(long? holdId)
            => new("KEY-1", 7001, 1, "ORD-7001", _clock.Now.AddMinutes(30), 1, M1Commands.Currency, null, [seat], holdId);

        Assert.True(validator.Validate(Command(null)).IsValid);
        Assert.True(validator.Validate(Command(9001)).IsValid);
        Assert.False(validator.Validate(Command(0)).IsValid);
        Assert.False(validator.Validate(Command(-1)).IsValid);
    }
}
