using System.Collections.Concurrent;
using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.DefineInventoryPolicy;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Services;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;
using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers;
using AeroTech.Ancillary.Persistence;
using AeroTech.Ancillary.Persistence.AirportSlotInventoryAggregate;
using AeroTech.Ancillary.Persistence.AncillaryInventoryPolicyAggregate;
using AeroTech.Ancillary.Persistence.AncillaryReservationAggregate;
using AeroTech.Ancillary.Persistence.FlightCountInventoryAggregate;
using AeroTech.Ancillary.Persistence.FlightWeightInventoryAggregate;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.VerifyHoldSelections;
using AeroTech.Ancillary.Shopping.Reading.Sql;
using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.P1Commands;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.P2Commands;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Holds;

[Collection(DatabaseCollection.Name)]
public class V10InventoryAllocationAcceptanceTests
{
    private const long Flight = 301;
    private const long NextFlight = 302;
    private const InventoryResourceKind FlightCount = InventoryResourceKind.FlightCount;
    private const InventoryResourceKind FlightWeight = InventoryResourceKind.FlightWeight;
    private const InventoryResourceKind AirportSlot = InventoryResourceKind.AirportSlot;

    private readonly TestDatabase _database;
    private readonly FixedClock _clock = new();
    private readonly InventoryHarness _harness;

    public V10InventoryAllocationAcceptanceTests(TestDatabase database)
    {
        _database = database;
        _harness = new InventoryHarness(database, _clock);
    }

    private static long NextOrderId => Interlocked.Increment(ref _lastOrderId);

    private static long _lastOrderId = 950_000;

    private sealed record Product(long DefinitionId, long ProvisionId, long? PolicyId = null);

    private async Task<(InventoryFixture Fixture, int AirlineId, long SupplierId)> OperatorAsync()
    {
        var connected = await _harness.OperatorAsync();

        connected.Fixture.Flights!.UnionWith([Flight, NextFlight]);
        connected.Fixture.Resources!.UnionWith(
        [
            (InventoryResourceKind.FlightCount, MealResource),
            (InventoryResourceKind.FlightCount, OversizeResource),
            (InventoryResourceKind.FlightWeight, BagWeightResource)
        ]);
        connected.Fixture.Airports.Add(Thr);
        connected.Fixture.Facilities![Lounge] = (Thr, "Asia/Tehran");

        return connected;
    }

    private static TestDefineInventoryPolicyCommand Local(
        int airlineId,
        string reference,
        long definitionId,
        LocalInventoryPattern pattern,
        FlightCountConsumptionInput? count = null,
        FlightWeightConsumptionInput? weight = null,
        AirportSlotConsumptionInput? slot = null)
        => new(airlineId, reference, definitionId, InventoryAuthority.Local, pattern, CountConsumption: count, WeightConsumption: weight, SlotConsumption: slot);

    private async Task<Product> ProductAsync(
        InventoryFixture fixture,
        int airlineId,
        long supplierId,
        string reference,
        Func<long, TestDefineInventoryPolicyCommand>? policy,
        int maxQuantity = 2,
        ServiceCoverageScope coverageScope = ServiceCoverageScope.Sector)
    {
        var definition = await _harness.Proof.DefinitionAsync(M1Commands.LoungeDefinition(airlineId, supplierId, reference));
        var rule = await _harness.Proof.RuleAsync(M1Commands.LoungeProvision(definition.Id, maxQuantity: maxQuantity) with { CoverageScope = coverageScope });

        long? policyId = policy is null ? null : await _harness.ActivePolicyAsync(fixture, policy(definition.Id));

        return new Product(definition.Id, rule.Provision.Id, policyId);
    }

    private async Task<Product> LoungeSlotProductAsync(InventoryFixture fixture, int airlineId, long supplierId)
    {
        var variant = V122Catalog.Case("A20");
        var definition = await _harness.Proof.DefinitionAsync(V122Catalog.Define(variant, airlineId, supplierId) with
        {
            TypedSpecification = new ServiceSpecificationInput(AirportService: variant.Specification.AirportService! with { MaxGuestsPerPrimary = 2, RequiresSpecificAppointment = true })
        });
        var rule = await _harness.Proof.RuleAsync(
            V122Catalog.Rule(variant, definition.Id) with
            {
                AirportServiceRule = new ProvisionAirportServiceRuleInput("T1", AirportServiceDirection.Departure, new TimeOnly(6, 0), new TimeOnly(20, 0), null, 2)
            },
            provisionId => new TestDefinePricingRatesCommand(provisionId, [new(null, null, null, new MoneyInput(25m, Eur), null)]));

        await _harness.ActivePolicyAsync(fixture, Local(airlineId, variant.Reference, definition.Id, LocalInventoryPattern.AirportSlot, slot: SlotUse()));

        return new Product(definition.Id, rule.Provision.Id);
    }

    private async Task<long> CountSourceAsync(InventoryFixture fixture, int airlineId, long resourceId, int total, long flightId = Flight)
    {
        var source = await _harness.RequestAsync(fixture, scope => scope.DefineFlightCountInventory.DefineAsync(
            new TestDefineFlightCountInventoryCommand(airlineId, flightId, resourceId, InventoryCountUnit.Item, total)));

        return (await _harness.RequestAsync(fixture, scope => scope.ActivateFlightCountInventory.ActivateAsync(new TestFlightCountLifecycleCommand(source.Id, 1)))).Id;
    }

    private async Task<long> WeightSourceAsync(InventoryFixture fixture, int airlineId, decimal capacityKg)
    {
        var source = await _harness.RequestAsync(fixture, scope => scope.DefineFlightWeightInventory.DefineAsync(
            new TestDefineFlightWeightInventoryCommand(airlineId, Flight, BagWeightResource, capacityKg)));

        return (await _harness.RequestAsync(fixture, scope => scope.ActivateFlightWeightInventory.ActivateAsync(new TestFlightWeightLifecycleCommand(source.Id, 1)))).Id;
    }

    private async Task<long> SlotSourceAsync(InventoryFixture fixture, int airlineId, DateTimeOffset startUtc, DateTimeOffset endUtc, int capacity)
    {
        var source = await _harness.RequestAsync(fixture, scope => scope.DefineAirportSlotInventory.DefineAsync(
            new TestDefineAirportSlotInventoryCommand(airlineId, Thr, Lounge, startUtc, endUtc, capacity)));

        return (await _harness.RequestAsync(fixture, scope => scope.ActivateAirportSlotInventory.ActivateAsync(new TestAirportSlotLifecycleCommand(source.Id, 1)))).Id;
    }

    private TestHoldAncillaryServicesCommand Hold(long orderId, params HoldSelection[] services)
        => M1Commands.Hold($"G2-{orderId}", orderId, _clock.Now.AddMinutes(30), services);

    private static HoldSelection Service(
        Product product,
        long orderId,
        int quantity = 1,
        IReadOnlyList<long>? flights = null,
        AirportServiceSelectionOptions? appointment = null)
        => M1Commands.Lounge(product.ProvisionId, orderId * 10 + 1, 71, flights ?? [Flight], quantity, appointment);

    private async Task<AncillaryHoldReceipt> HoldAsync(TestHoldAncillaryServicesCommand command)
    {
        await using var scope = new AncillaryScope(_database, _clock);

        return await scope.HoldAncillaryServices.HoldAsync(command);
    }

    private Task RefusedAsync(int code, int httpStatus, TestHoldAncillaryServicesCommand command)
        => BusinessAssert.ThrowsAsync(code, httpStatus, () => HoldAsync(command));

    private async Task<AncillaryHoldReceipt> LifecycleAsync(Func<AncillaryScope, Task<AncillaryHoldReceipt>> change)
    {
        await using var scope = new AncillaryScope(_database, _clock);

        return await change(scope);
    }

    private static string Uses(InventoryResourceKind kind, long sourceId, string amount) => $"{(int)kind}:{sourceId}:{amount}";

    private async Task<string> CapacityAsync(InventoryResourceKind kind, long sourceId)
        => (await (kind switch
        {
            InventoryResourceKind.FlightCount => _harness.RowsAsync($"SELECT CONCAT(TotalCapacity, '|', Version) AS Value FROM Ancillary.FlightCountInventories WHERE Id = {sourceId}"),
            InventoryResourceKind.FlightWeight => _harness.RowsAsync($"SELECT CONCAT(CapacityKg, '|', Version) AS Value FROM Ancillary.FlightWeightInventories WHERE Id = {sourceId}"),
            _ => _harness.RowsAsync($"SELECT CONCAT(CapacityPersons, '|', Version) AS Value FROM Ancillary.AirportSlotInventories WHERE Id = {sourceId}")
        })).Single();

    private async Task<string> ReservationsAsync(params long[] orderIds)
    {
        var (reservations, units) = (0, 0);

        foreach (var orderId in orderIds)
        {
            var row = (await _harness.RowsAsync($"""
                SELECT CONCAT(COUNT(DISTINCT reservation.Id), '|', COUNT(unit.Id)) AS Value
                FROM Ancillary.AncillaryReservations AS reservation
                LEFT JOIN Ancillary.AncillaryReservationUnits AS unit ON unit.AncillaryReservationId = reservation.Id
                WHERE reservation.OrderId = {orderId}
                """)).Single().Split('|');

            reservations += int.Parse(row[0]);
            units += int.Parse(row[1]);
        }

        return $"{reservations}|{units}";
    }

    [Fact]
    public async Task V10_I01_I02_I03_I04_I05_I12_only_an_active_unlimited_or_supported_local_policy_reaches_a_hold()
    {
        var (fixture, airlineId, supplierId) = await OperatorAsync();
        var unlimited = await ProductAsync(fixture, airlineId, supplierId, "G2_UNLIMITED", id => new(airlineId, "G2_UNLIMITED", id, InventoryAuthority.Unlimited));
        var missing = await ProductAsync(fixture, airlineId, supplierId, "G2_MISSING", null);
        var suspended = await ProductAsync(fixture, airlineId, supplierId, "G2_SUSPENDED", id => new(airlineId, "G2_SUSPENDED", id, InventoryAuthority.Unlimited));
        var daily = await ProductAsync(fixture, airlineId, supplierId, "G2_DAILY", null);
        var dailyPolicy = await _harness.RequestAsync(fixture, scope => scope.DefineInventoryPolicy.DefineAsync(
            new TestDefineInventoryPolicyCommand(airlineId, "G2_DAILY", daily.DefinitionId, InventoryAuthority.Local, LocalInventoryPattern.DailyCount)));
        await _harness.RefusedAsync(16607, 409, fixture, scope => scope.ActivateInventoryPolicy.ActivateAsync(new TestInventoryPolicyLifecycleCommand(dailyPolicy.Id, 1)));
        await _harness.RequestAsync(fixture, scope => scope.SuspendInventoryPolicy.SuspendAsync(new TestInventoryPolicyLifecycleCommand(suspended.PolicyId!.Value, 2)));

        var externalSupplier = await _harness.Proof.SupplierAsync(M1Commands.ExternalSupplier(airlineId));
        var external = await ProductAsync(fixture, airlineId, externalSupplier, "G2_EXTERNAL", null);
        var kilo = await _harness.ProductAsync(airlineId, supplierId, "G2_KILO", PricingUnit.PerKilogram);
        var kiloRule = await _harness.Proof.RuleAsync(V122Catalog.Fit(
            M1Commands.LoungeProvision(kilo.Id) with { Quantity = new ProvisionQuantityInput(AncillaryQuantityUnit.Kilogram, 1, 32) },
            PricingUnit.PerKilogram));
        await _harness.ActivePolicyAsync(fixture, Local(airlineId, "G2_KILO", kilo.Id, LocalInventoryPattern.FlightWeight, weight: AcceptedKg()));
        var weightSource = await WeightSourceAsync(fixture, airlineId, 100m);

        var heldOrder = NextOrderId;
        var held = await HoldAsync(Hold(heldOrder, Service(unlimited, heldOrder, quantity: 2)));
        var refused = Enumerable.Range(0, 5).Select(_ => NextOrderId).ToArray();

        await RefusedAsync(16419, 409, Hold(refused[0], Service(missing, refused[0])));
        await RefusedAsync(16419, 409, Hold(refused[1], Service(suspended, refused[1])));
        await RefusedAsync(16419, 409, Hold(refused[2], Service(daily, refused[2])));
        await RefusedAsync(16104, 422, Hold(refused[3], Service(external, refused[3])));
        await RefusedAsync(16419, 409, Hold(refused[4], new HoldSelection(
            AncillaryProfile.Baggage,
            V122Catalog.VariantFor(PricingUnit.PerKilogram, ServiceDateBasis.FlightDeparture),
            kiloRule.Provision.Id,
            refused[4] * 10 + 1,
            71,
            [Flight],
            1,
            new BaggageSelectionOptions("B-1", 27m, null, null))));

        await using var reader = new AncillaryScope(_database, _clock);

        Assert.Equal((AncillaryReservationStatus.Held, 2), (held.Status, Assert.Single((await reader.Reservations.GetAsync(held.HoldId))!.Units).Quantity));
        Assert.Empty(await _harness.AllocationsAsync(heldOrder));
        Assert.Equal("0|0", await ReservationsAsync(refused));
        Assert.Equal("0.000", await _harness.UsageAsync(FlightWeight, weightSource));
    }

    [Fact]
    public async Task V10_I06_I08_I09_I25_L04_L12_count_demand_is_quantity_times_the_binding_and_only_live_units_consume_it()
    {
        var (fixture, airlineId, supplierId) = await OperatorAsync();
        var meal = await ProductAsync(fixture, airlineId, supplierId, "G2_MEAL",
            id => Local(airlineId, "G2_MEAL", id, LocalInventoryPattern.FlightCount, count: Count(MealResource, 3, InventoryCountUnit.Item)));
        var source = await CountSourceAsync(fixture, airlineId, MealResource, 10);
        var (first, second, third, fourth) = (NextOrderId, NextOrderId, NextOrderId, NextOrderId);

        var confirmed = await HoldAsync(Hold(first, Service(meal, first, quantity: 2)));
        await LifecycleAsync(scope => scope.ConfirmAncillaryHold.ConfirmAsync(new TestConfirmAncillaryHoldCommand(confirmed.HoldId)));

        await RefusedAsync(16417, 409, Hold(second, Service(meal, second, quantity: 2)));
        var held = await HoldAsync(Hold(third, Service(meal, third)));
        await RefusedAsync(16417, 409, Hold(fourth, Service(meal, fourth)));

        Assert.Equal(new[] { Uses(FlightCount, source, "6.000") }, await _harness.AllocationsAsync(first));
        Assert.Equal(new[] { Uses(FlightCount, source, "3.000") }, await _harness.AllocationsAsync(third));
        Assert.Equal(("9.000", "10|2"), (await _harness.UsageAsync(FlightCount, source), await CapacityAsync(FlightCount, source)));

        await LifecycleAsync(scope => scope.ReleaseAncillaryHold.ReleaseAsync(new TestReleaseAncillaryHoldCommand(held.HoldId)));
        var retried = await HoldAsync(Hold(fourth, Service(meal, fourth)));

        Assert.Equal(AncillaryReservationStatus.Held, retried.Status);
        Assert.Equal("9.000", await _harness.UsageAsync(FlightCount, source));

        await LifecycleAsync(scope => scope.CancelAncillaryHold.CancelAsync(new TestCancelAncillaryHoldCommand(confirmed.HoldId, "CUSTOMER")));

        Assert.Equal(("3.000", "10|2"), (await _harness.UsageAsync(FlightCount, source), await CapacityAsync(FlightCount, source)));
        Assert.Equal(new[] { Uses(FlightCount, source, "6.000") }, await _harness.AllocationsAsync(first));
    }

    [Fact]
    public async Task V10_I07_I10_two_products_share_one_source_and_a_logically_expired_hold_frees_it_before_the_poller()
    {
        var (fixture, airlineId, supplierId) = await OperatorAsync();
        var vegetarian = await ProductAsync(fixture, airlineId, supplierId, "G2_VGML",
            id => Local(airlineId, "G2_VGML", id, LocalInventoryPattern.FlightCount, count: Count(MealResource, 1, InventoryCountUnit.Item)));
        var child = await ProductAsync(fixture, airlineId, supplierId, "G2_CHML",
            id => Local(airlineId, "G2_CHML", id, LocalInventoryPattern.FlightCount, count: Count(MealResource, 1, InventoryCountUnit.Item)));
        var source = await CountSourceAsync(fixture, airlineId, MealResource, 2);
        var (first, second, third) = (NextOrderId, NextOrderId, NextOrderId);

        await HoldAsync(Hold(first, Service(vegetarian, first)));
        await HoldAsync(Hold(second, Service(child, second)));
        await RefusedAsync(16417, 409, Hold(third, Service(vegetarian, third)));

        Assert.Equal("2.000", await _harness.UsageAsync(FlightCount, source));

        _clock.Now = _clock.Now.AddMinutes(31);

        var afterExpiry = await HoldAsync(Hold(third, Service(vegetarian, third)));

        Assert.Equal(AncillaryReservationStatus.Held, afterExpiry.Status);
        Assert.Equal("1.000", await _harness.UsageAsync(FlightCount, source));
        Assert.Equal(
            new[] { $"{(int)AncillaryReservationStatus.Held}" },
            (await _harness.RowsAsync($"SELECT CONCAT(Status, '') AS Value FROM Ancillary.AncillaryReservations WHERE OrderId IN ({first}, {second})")).Distinct());
    }

    [Fact]
    public async Task V10_I11_I14_I15_count_plus_weight_is_allocated_together_or_not_at_all()
    {
        var (fixture, airlineId, supplierId) = await OperatorAsync();
        var bike = await ProductAsync(fixture, airlineId, supplierId, "G2_BIKE",
            id => Local(airlineId, "G2_BIKE", id, LocalInventoryPattern.FlightCountPlusWeight, count: Count(OversizeResource, 1, InventoryCountUnit.Item), weight: FixedKg(12.5m)));
        var count = await CountSourceAsync(fixture, airlineId, OversizeResource, 5);
        var weight = await WeightSourceAsync(fixture, airlineId, 30m);
        var (first, second) = (NextOrderId, NextOrderId);

        await HoldAsync(Hold(first, Service(bike, first, quantity: 2)));
        await RefusedAsync(16417, 409, Hold(second, Service(bike, second)));

        Assert.Equal(new[] { Uses(FlightCount, count, "2.000"), Uses(FlightWeight, weight, "25.000") }, await _harness.AllocationsAsync(first));
        Assert.Empty(await _harness.AllocationsAsync(second));
        Assert.Equal("0|0", await ReservationsAsync(second));
        Assert.Equal(
            ("2.000", "25.000", "5|2", "30.000|2"),
            (await _harness.UsageAsync(FlightCount, count), await _harness.UsageAsync(FlightWeight, weight), await CapacityAsync(FlightCount, count), await CapacityAsync(FlightWeight, weight)));
    }

    [Fact]
    public async Task V10_I16_I28_a_portion_takes_every_flight_or_nothing_and_never_another_airlines_source()
    {
        var (fixture, airlineId, supplierId) = await OperatorAsync();
        var (otherFixture, otherAirlineId, _) = await OperatorAsync();
        var portion = await ProductAsync(fixture, airlineId, supplierId, "G2_PORTION",
            id => Local(airlineId, "G2_PORTION", id, LocalInventoryPattern.FlightCount, count: Count(MealResource, 1, InventoryCountUnit.Item)),
            maxQuantity: 1,
            coverageScope: ServiceCoverageScope.Portion);
        var outbound = await CountSourceAsync(fixture, airlineId, MealResource, 5);
        var foreign = await CountSourceAsync(otherFixture, otherAirlineId, MealResource, 5, NextFlight);
        var orderId = NextOrderId;
        var command = Hold(orderId, Service(portion, orderId, flights: [Flight, NextFlight]));

        await RefusedAsync(16419, 409, command);

        Assert.Equal("0|0", await ReservationsAsync(orderId));
        Assert.Equal(("0.000", "0.000"), (await _harness.UsageAsync(FlightCount, outbound), await _harness.UsageAsync(FlightCount, foreign)));

        var inbound = await CountSourceAsync(fixture, airlineId, MealResource, 5, NextFlight);
        await HoldAsync(command);

        Assert.Equal(
            new[] { Uses(FlightCount, outbound, "1.000"), Uses(FlightCount, inbound, "1.000") }.Order(),
            (await _harness.AllocationsAsync(orderId)).Order());
        Assert.Equal("0.000", await _harness.UsageAsync(FlightCount, foreign));
    }

    [Fact]
    public async Task V10_I17_I18_I19_I20_L19_L20_V12_D07_W03_slots_are_taken_per_contiguous_slice_with_guests_counted_as_persons_and_the_appointment_keeps_its_offset()
    {
        var (fixture, airlineId, supplierId) = await OperatorAsync();
        var lounge = await LoungeSlotProductAsync(fixture, airlineId, supplierId);
        var morning = await SlotSourceAsync(fixture, airlineId, Utc(10), Utc(11), 4);
        var noon = await SlotSourceAsync(fixture, airlineId, Utc(11), Utc(12), 4);
        await SlotSourceAsync(fixture, airlineId, Utc(13), Utc(14), 4);
        var (first, gap, crowd, closed) = (NextOrderId, NextOrderId, NextOrderId, NextOrderId);
        var tehran = TimeSpan.FromHours(3.5);
        var command = Hold(first, Service(lounge, first, appointment: new AirportServiceSelectionOptions(Utc(10).ToOffset(tehran), 2)));

        var hold = await HoldAsync(command);
        var replay = await HoldAsync(command);

        Assert.Equal(hold.HoldId, replay.HoldId);
        Assert.Equal(new[] { Uses(AirportSlot, morning, "3.000"), Uses(AirportSlot, noon, "3.000") }.Order(), (await _harness.AllocationsAsync(first)).Order());

        await using (var reader = new AncillaryScope(_database, _clock))
        {
            var unit = Assert.Single(Assert.Single(Assert.Single((await reader.GetHoldById.ExecuteAsync(hold.HoldId)).AirportServices!).Products).Units);
            var appointment = unit.Selected.Options!.AirportService;

            Assert.Equal((tehran, Utc(10), (int?)2), (appointment.TimeWithOffset!.Value.Offset, appointment.TimeWithOffset.Value, appointment.GuestCount));
        }

        await RefusedAsync(16403, 409, Hold(first, Service(lounge, first, appointment: new AirportServiceSelectionOptions(Utc(10).ToOffset(tehran), 1))));
        await RefusedAsync(16403, 409, Hold(first, Service(lounge, first, appointment: new AirportServiceSelectionOptions(Utc(10, 30).ToOffset(tehran), 2))));
        await RefusedAsync(16419, 409, Hold(gap, Service(lounge, gap, appointment: new AirportServiceSelectionOptions(Utc(11, 30), 0))));
        await RefusedAsync(16417, 409, Hold(crowd, Service(lounge, crowd, appointment: new AirportServiceSelectionOptions(Utc(10), 1))));
        await RefusedAsync(16402, 422, Hold(crowd, Service(lounge, crowd)));

        await _harness.RequestAsync(fixture, scope => scope.CloseAirportSlotInventoryForSale.CloseAsync(new TestAirportSlotLifecycleCommand(morning, 2)));
        await RefusedAsync(16419, 409, Hold(closed, Service(lounge, closed, appointment: new AirportServiceSelectionOptions(Utc(10), 0))));
        await LifecycleAsync(scope => scope.ReleaseAncillaryHold.ReleaseAsync(new TestReleaseAncillaryHoldCommand(hold.HoldId)));

        Assert.Equal(
            ("0.000", "0.000", "4|3", "4|2"),
            (await _harness.UsageAsync(AirportSlot, morning), await _harness.UsageAsync(AirportSlot, noon), await CapacityAsync(AirportSlot, morning), await CapacityAsync(AirportSlot, noon)));
        Assert.Equal("0|0", await ReservationsAsync(gap, closed));
    }

    [Fact]
    public async Task V10_I21_I22_I23_concurrent_orders_for_the_last_unit_kilogram_and_person_get_exactly_one_hold()
    {
        var (fixture, airlineId, supplierId) = await OperatorAsync();
        var meal = await ProductAsync(fixture, airlineId, supplierId, "G2_LAST_MEAL",
            id => Local(airlineId, "G2_LAST_MEAL", id, LocalInventoryPattern.FlightCount, count: Count(MealResource, 1, InventoryCountUnit.Item)));
        var bag = await ProductAsync(fixture, airlineId, supplierId, "G2_LAST_BAG",
            id => Local(airlineId, "G2_LAST_BAG", id, LocalInventoryPattern.FlightWeight, weight: FixedKg(12.5m)));
        var lounge = await LoungeSlotProductAsync(fixture, airlineId, supplierId);
        var count = await CountSourceAsync(fixture, airlineId, MealResource, 1);
        var weight = await WeightSourceAsync(fixture, airlineId, 12.5m);
        var slot = await SlotSourceAsync(fixture, airlineId, Utc(10), Utc(12), 1);

        var races = new (Func<long, HoldSelection> Service, InventoryResourceKind Kind, long Source, string Capacity)[]
        {
            (orderId => Service(meal, orderId), FlightCount, count, "1.000"),
            (orderId => Service(bag, orderId), FlightWeight, weight, "12.500"),
            (orderId => Service(lounge, orderId, appointment: new AirportServiceSelectionOptions(Utc(10), 0)), AirportSlot, slot, "1.000")
        };

        foreach (var race in races)
        {
            var orders = Enumerable.Range(0, 4).Select(_ => NextOrderId).ToArray();
            var raced = await _harness.RaceAsync(orders.Select(orderId => (Func<Task>)(() => HoldAsync(Hold(orderId, race.Service(orderId))))));

            Assert.Equal(1, raced.Succeeded);
            Assert.Equal(new[] { 16417 }, raced.Codes);
            Assert.Equal(race.Capacity, await _harness.UsageAsync(race.Kind, race.Source));
        }
    }

    [Fact]
    public async Task V12_I05_the_same_hold_sent_concurrently_creates_one_hold_and_every_answer_is_that_hold_or_a_busy_order()
    {
        var (fixture, airlineId, supplierId) = await OperatorAsync();
        var meal = await ProductAsync(fixture, airlineId, supplierId, "V12_SAME_KEY",
            id => Local(airlineId, "V12_SAME_KEY", id, LocalInventoryPattern.FlightCount, count: Count(MealResource, 1, InventoryCountUnit.Item)));
        var source = await CountSourceAsync(fixture, airlineId, MealResource, 10);
        var used = 0;

        foreach (var sharedOrderLock in new[] { true, false })
        {
            var orderId = NextOrderId;
            var command = Hold(orderId, Service(meal, orderId));
            var orderLock = new TestOnlyHoldLock();
            var receipts = new ConcurrentBag<AncillaryHoldReceipt>();

            var raced = await _harness.RaceAsync(Enumerable.Range(0, 4).Select(_ => (Func<Task>)(async () =>
            {
                await using var scope = new AncillaryScope(_database, _clock, sharedOrderLock ? orderLock : null);
                receipts.Add(await scope.HoldAncillaryServices.HoldAsync(command));
            })));
            var retried = await HoldAsync(command);
            int[] refusals = !sharedOrderLock || raced.Succeeded == 4 ? [] : [16416];
            used++;

            Assert.InRange(raced.Succeeded, sharedOrderLock ? 1 : 4, 4);
            Assert.Equal(refusals, raced.Codes);
            Assert.Single(receipts.Append(retried).Select(receipt => (receipt.HoldId, string.Join(',', receipt.Units.Select(unit => unit.UnitId)))).Distinct());
            Assert.Equal(
                ("1|1", "1|1.000", $"{used}.000"),
                (await ReservationsAsync(orderId), await AllocationRowsAsync(retried.HoldId), await _harness.UsageAsync(FlightCount, source)));
        }
    }

    [Fact]
    public async Task V10_I24_capacity_never_drops_below_live_usage_and_an_adjustment_serializes_with_a_hold()
    {
        var (fixture, airlineId, supplierId) = await OperatorAsync();
        var meal = await ProductAsync(fixture, airlineId, supplierId, "G2_ADJUST",
            id => Local(airlineId, "G2_ADJUST", id, LocalInventoryPattern.FlightCount, count: Count(MealResource, 1, InventoryCountUnit.Item)));
        var source = await CountSourceAsync(fixture, airlineId, MealResource, 3);
        var (first, second) = (NextOrderId, NextOrderId);

        await HoldAsync(Hold(first, Service(meal, first, quantity: 2)));
        await _harness.RefusedAsync(16621, 409, fixture, scope => scope.AdjustFlightCountInventory.AdjustAsync(
            new TestAdjustFlightCountInventoryCommand(source, 1, "AIRCRAFT_CHANGE", "corr-below", 2)));

        Assert.Equal(("3|2", "2.000"), (await CapacityAsync(FlightCount, source), await _harness.UsageAsync(FlightCount, source)));

        var raced = await _harness.RaceAsync(
        [
            () => HoldAsync(Hold(second, Service(meal, second))),
            () => _harness.RequestAsync(fixture, scope => scope.AdjustFlightCountInventory.AdjustAsync(
                new TestAdjustFlightCountInventoryCommand(source, 2, "AIRCRAFT_CHANGE", "corr-race", 2)))
        ]);
        var outcome = (await CapacityAsync(FlightCount, source), await _harness.UsageAsync(FlightCount, source));

        Assert.Equal(1, raced.Succeeded);
        Assert.Contains((raced.Codes.Single(), outcome), new[] { (16621, ("3|2", "3.000")), (16417, ("2|3", "2.000")) });
    }

    private async Task<string> StateAsync(long sourceId)
        => (await _harness.RowsAsync($"SELECT CONCAT(Status, '|', ClosedForSale) AS Value FROM Ancillary.FlightCountInventories WHERE Id = {sourceId}")).Single();

    private async Task<int> RaceAgainstHoldAsync(InventoryFixture fixture, Product product, Func<InventoryScope, Task> change)
    {
        var (racing, after) = (NextOrderId, NextOrderId);
        var raced = await _harness.RaceAsync(
        [
            () => HoldAsync(Hold(racing, Service(product, racing))),
            () => _harness.RequestAsync(fixture, async scope =>
            {
                await change(scope);
                return racing;
            })
        ]);

        Assert.InRange(raced.Succeeded, 1, 2);
        Assert.Equal(raced.Succeeded == 2 ? Array.Empty<int>() : new[] { 16419 }, raced.Codes);
        await RefusedAsync(16419, 409, Hold(after, Service(product, after)));

        return raced.Succeeded - 1;
    }

    [Fact]
    public async Task V10_I19_close_and_suspend_serialize_with_a_racing_hold_and_admit_no_hold_after_them()
    {
        var (fixture, airlineId, supplierId) = await OperatorAsync();
        var meal = await ProductAsync(fixture, airlineId, supplierId, "G2_SALEABLE",
            id => Local(airlineId, "G2_SALEABLE", id, LocalInventoryPattern.FlightCount, count: Count(MealResource, 1, InventoryCountUnit.Item)));
        var source = await CountSourceAsync(fixture, airlineId, MealResource, 10);

        var admitted = await RaceAgainstHoldAsync(fixture, meal, scope => scope.CloseFlightCountInventoryForSale.CloseAsync(new TestFlightCountLifecycleCommand(source, 2)));

        Assert.Equal("2|1", await StateAsync(source));

        await _harness.RequestAsync(fixture, scope => scope.OpenFlightCountInventoryForSale.OpenAsync(new TestFlightCountLifecycleCommand(source, 3)));
        admitted += await RaceAgainstHoldAsync(fixture, meal, scope => scope.SuspendFlightCountInventory.SuspendAsync(new TestFlightCountLifecycleCommand(source, 4)));

        Assert.Equal(("3|0", $"{admitted}.000"), (await StateAsync(source), await _harness.UsageAsync(FlightCount, source)));
    }

    [Fact]
    public async Task V11_I15_I23_a_source_is_retired_only_without_live_usage_and_a_racing_hold_or_retire_wins_alone()
    {
        var (fixture, airlineId, supplierId) = await OperatorAsync();
        var meal = await ProductAsync(fixture, airlineId, supplierId, "V11_RETIRE",
            id => Local(airlineId, "V11_RETIRE", id, LocalInventoryPattern.FlightCount, count: Count(MealResource, 1, InventoryCountUnit.Item)));
        var source = await CountSourceAsync(fixture, airlineId, MealResource, 10);
        var first = NextOrderId;
        var held = await HoldAsync(Hold(first, Service(meal, first)));

        await _harness.RefusedAsync(16621, 409, fixture, scope => scope.RetireFlightCountInventory.RetireAsync(new TestFlightCountLifecycleCommand(source, 2)));
        await LifecycleAsync(scope => scope.ReleaseAncillaryHold.ReleaseAsync(new TestReleaseAncillaryHoldCommand(held.HoldId)));

        var racing = NextOrderId;
        var raced = await _harness.RaceAsync(
        [
            () => HoldAsync(Hold(racing, Service(meal, racing))),
            () => _harness.RequestAsync(fixture, scope => scope.RetireFlightCountInventory.RetireAsync(new TestFlightCountLifecycleCommand(source, 2)))
        ]);
        var lost = Assert.Single(raced.Codes);

        Assert.Equal(1, raced.Succeeded);
        Assert.Contains(lost, new[] { 16419, 16621 });
        Assert.Equal(
            lost == 16621 ? ("2|0", "1.000") : ("4|0", "0.000"),
            (await StateAsync(source), await _harness.UsageAsync(FlightCount, source)));
    }

    [Fact]
    public async Task V11_I15_a_weight_or_slot_source_with_a_live_hold_is_retired_only_after_the_hold_is_released()
    {
        var (fixture, airlineId, supplierId) = await OperatorAsync();
        var bag = await ProductAsync(fixture, airlineId, supplierId, "V11_RETIRE_BAG",
            id => Local(airlineId, "V11_RETIRE_BAG", id, LocalInventoryPattern.FlightWeight, weight: FixedKg(12.5m)));
        var lounge = await LoungeSlotProductAsync(fixture, airlineId, supplierId);
        var weight = await WeightSourceAsync(fixture, airlineId, 30m);
        var slot = await SlotSourceAsync(fixture, airlineId, Utc(10), Utc(12), 4);
        var (bagged, appointed) = (NextOrderId, NextOrderId);
        var bagHold = await HoldAsync(Hold(bagged, Service(bag, bagged)));
        var loungeHold = await HoldAsync(Hold(appointed, Service(lounge, appointed, appointment: new AirportServiceSelectionOptions(Utc(10), 0))));

        await _harness.RefusedAsync(16621, 409, fixture, scope => scope.RetireFlightWeightInventory.RetireAsync(new TestFlightWeightLifecycleCommand(weight, 2)));
        await _harness.RefusedAsync(16621, 409, fixture, scope => scope.RetireAirportSlotInventory.RetireAsync(new TestAirportSlotLifecycleCommand(slot, 2)));

        Assert.Equal(("12.500", "1.000"), (await _harness.UsageAsync(FlightWeight, weight), await _harness.UsageAsync(AirportSlot, slot)));

        await LifecycleAsync(scope => scope.ReleaseAncillaryHold.ReleaseAsync(new TestReleaseAncillaryHoldCommand(bagHold.HoldId)));
        await LifecycleAsync(scope => scope.ReleaseAncillaryHold.ReleaseAsync(new TestReleaseAncillaryHoldCommand(loungeHold.HoldId)));
        await _harness.RequestAsync(fixture, scope => scope.RetireFlightWeightInventory.RetireAsync(new TestFlightWeightLifecycleCommand(weight, 2)));
        await _harness.RequestAsync(fixture, scope => scope.RetireAirportSlotInventory.RetireAsync(new TestAirportSlotLifecycleCommand(slot, 2)));

        Assert.Equal(
            new[] { $"{(int)InventoryRecordStatus.Retired}|3", $"{(int)InventoryRecordStatus.Retired}|3" },
            await _harness.RowsAsync($"""
                SELECT CONCAT(Status, '|', Version) AS Value FROM Ancillary.FlightWeightInventories WHERE Id = {weight}
                UNION ALL
                SELECT CONCAT(Status, '|', Version) FROM Ancillary.AirportSlotInventories WHERE Id = {slot}
                """));
    }

    [Fact]
    public async Task V11_I17_a_confirmed_unit_keeps_its_stock_after_the_hold_deadline()
    {
        var (fixture, airlineId, supplierId) = await OperatorAsync();
        var meal = await ProductAsync(fixture, airlineId, supplierId, "V11_CONFIRMED",
            id => Local(airlineId, "V11_CONFIRMED", id, LocalInventoryPattern.FlightCount, count: Count(MealResource, 1, InventoryCountUnit.Item)));
        var source = await CountSourceAsync(fixture, airlineId, MealResource, 1);
        var (confirmed, later) = (NextOrderId, NextOrderId);
        var hold = await HoldAsync(Hold(confirmed, Service(meal, confirmed)));

        await LifecycleAsync(scope => scope.ConfirmAncillaryHold.ConfirmAsync(new TestConfirmAncillaryHoldCommand(hold.HoldId)));
        _clock.Now = _clock.Now.AddMinutes(31);

        await RefusedAsync(16417, 409, Hold(later, Service(meal, later)));

        Assert.Equal(("1.000", "0|0"), (await _harness.UsageAsync(FlightCount, source), await ReservationsAsync(later)));
    }

    [Fact]
    public async Task V11_I22_two_products_on_one_source_in_one_hold_are_summed_before_the_shortage_check()
    {
        var (fixture, airlineId, supplierId) = await OperatorAsync();
        var vegetarian = await ProductAsync(fixture, airlineId, supplierId, "V11_VGML",
            id => Local(airlineId, "V11_VGML", id, LocalInventoryPattern.FlightCount, count: Count(MealResource, 1, InventoryCountUnit.Item)));
        var child = await ProductAsync(fixture, airlineId, supplierId, "V11_CHML",
            id => Local(airlineId, "V11_CHML", id, LocalInventoryPattern.FlightCount, count: Count(MealResource, 1, InventoryCountUnit.Item)));
        var source = await CountSourceAsync(fixture, airlineId, MealResource, 2);
        var (refused, held) = (NextOrderId, NextOrderId);

        await RefusedAsync(16417, 409, Hold(refused, Service(vegetarian, refused, quantity: 2), M1Commands.Lounge(child.ProvisionId, refused * 10 + 2, 72, [Flight], 1)));

        Assert.Equal(("0|0", "0.000"), (await ReservationsAsync(refused), await _harness.UsageAsync(FlightCount, source)));

        await HoldAsync(Hold(held, Service(vegetarian, held), M1Commands.Lounge(child.ProvisionId, held * 10 + 2, 72, [Flight], 1)));

        Assert.Equal(new[] { Uses(FlightCount, source, "1.000"), Uses(FlightCount, source, "1.000") }, await _harness.AllocationsAsync(held));
        Assert.Equal(("1|2", "2.000"), (await ReservationsAsync(held), await _harness.UsageAsync(FlightCount, source)));
    }

    [Fact]
    public async Task V11_I23_a_real_sql_lock_timeout_refuses_the_hold_without_a_write_and_the_same_request_then_holds_once()
    {
        var (fixture, airlineId, supplierId) = await OperatorAsync();
        var meal = await ProductAsync(fixture, airlineId, supplierId, "V11_LOCKED",
            id => Local(airlineId, "V11_LOCKED", id, LocalInventoryPattern.FlightCount, count: Count(MealResource, 1, InventoryCountUnit.Item)));
        var source = await CountSourceAsync(fixture, airlineId, MealResource, 1);
        var orderId = NextOrderId;
        var command = Hold(orderId, Service(meal, orderId));
        var resource = $"Ancillary.Inventory.{FlightCount}.{source}";

        await using (var blocker = _database.NewContext(_clock))
        {
            await using var transaction = await blocker.Database.BeginTransactionAsync();
            await blocker.Database.ExecuteSqlAsync($"EXEC sp_getapplock @Resource = {resource}, @LockMode = 'Exclusive', @LockOwner = 'Transaction'");

            await RefusedAsync(16622, 409, command);

            Assert.Equal(("0|0", "0.000"), (await ReservationsAsync(orderId), await _harness.UsageAsync(FlightCount, source)));
        }

        var held = await HoldAsync(command);
        var replay = await HoldAsync(command);

        Assert.Equal(held.HoldId, replay.HoldId);
        Assert.Equal(("1|1", "1.000"), (await ReservationsAsync(orderId), await _harness.UsageAsync(FlightCount, source)));
    }

    private async Task<string> UnitStatesAsync(long holdId)
        => string.Join('|', await _harness.RowsAsync($"SELECT CONCAT(Status, '') AS Value FROM Ancillary.AncillaryReservationUnits WHERE AncillaryReservationId = {holdId} ORDER BY OrderServiceId"));

    private async Task<string> AllocationRowsAsync(long holdId)
        => (await _harness.RowsAsync($"""
            SELECT CONCAT(COUNT(*), '|', COALESCE(SUM(allocation.Amount), 0)) AS Value
            FROM Ancillary.AncillaryReservationUnitAllocations AS allocation
            JOIN Ancillary.AncillaryReservationUnits AS unit ON unit.Id = allocation.AncillaryReservationUnitId
            WHERE unit.AncillaryReservationId = {holdId}
            """)).Single();

    [Fact]
    public async Task V12_L01_L02_L03_L04_a_release_over_http_reads_only_unitIds_and_refuses_any_other_query_key_before_any_change()
    {
        var (fixture, airlineId, supplierId) = await OperatorAsync();
        var meal = await ProductAsync(fixture, airlineId, supplierId, "V12_RELEASE",
            id => Local(airlineId, "V12_RELEASE", id, LocalInventoryPattern.FlightCount, count: Count(MealResource, 1, InventoryCountUnit.Item)));
        var source = await CountSourceAsync(fixture, airlineId, MealResource, 10);
        var orderId = NextOrderId;
        var hold = await HoldAsync(Hold(
            orderId,
            M1Commands.Lounge(meal.ProvisionId, orderId * 10 + 1, 71, [Flight], 1),
            M1Commands.Lounge(meal.ProvisionId, orderId * 10 + 2, 72, [Flight], 1),
            M1Commands.Lounge(meal.ProvisionId, orderId * 10 + 3, 73, [Flight], 1)));
        var units = hold.Units.OrderBy(unit => unit.OrderServiceId).Select(unit => unit.UnitId).ToArray();
        var (held, released) = ($"{(int)AncillaryReservationUnitStatus.Held}", $"{(int)AncillaryReservationUnitStatus.Released}");
        var path = $"Service/v1/Ancillaries/Hold/{hold.HoldId}";
        await using var host = await ReservationHttpHost.StartAsync(_database, _clock);

        var before = (await UnitStatesAsync(hold.HoldId), await AllocationRowsAsync(hold.HoldId), await _harness.UsageAsync(FlightCount, source));

        foreach (var query in new[]
                 {
                     $"unitId={units[0]}", $"unitIds[]={units[0]}", $"unitIds[0]={units[0]}", "foo=1", $"unitIds={units[0]}&unitId={units[1]}",
                     "unitIds=", "unitIds=abc", "unitIds=0", $"unitIds={units[0]}&unitIds={units[0]}"
                 })
        {
            var refused = await host.Client.DeleteAsync($"{path}?{query}");

            Assert.Equal((query, System.Net.HttpStatusCode.BadRequest), (query, refused.StatusCode));
        }

        Assert.Equal(($"{held}|{held}|{held}", "3|3.000", "3.000"), before);
        Assert.Equal(before, (await UnitStatesAsync(hold.HoldId), await AllocationRowsAsync(hold.HoldId), await _harness.UsageAsync(FlightCount, source)));

        Assert.Equal(System.Net.HttpStatusCode.OK, (await host.Client.DeleteAsync($"{path}?unitIds={units[0]}")).StatusCode);
        Assert.Equal(($"{released}|{held}|{held}", "2.000"), (await UnitStatesAsync(hold.HoldId), await _harness.UsageAsync(FlightCount, source)));

        Assert.Equal(System.Net.HttpStatusCode.OK, (await host.Client.DeleteAsync($"{path}?UNITIDS={units[1]}")).StatusCode);
        Assert.Equal(($"{released}|{released}|{held}", "1.000"), (await UnitStatesAsync(hold.HoldId), await _harness.UsageAsync(FlightCount, source)));

        Assert.Equal(System.Net.HttpStatusCode.OK, (await host.Client.DeleteAsync(path)).StatusCode);
        Assert.Equal(($"{released}|{released}|{released}", "3|3.000", "0.000"), (await UnitStatesAsync(hold.HoldId), await AllocationRowsAsync(hold.HoldId), await _harness.UsageAsync(FlightCount, source)));
    }

    [Fact]
    public async Task V12_L05_get_confirm_and_cancel_answer_on_the_hold_route_an_empty_cancel_list_is_refused_and_the_old_route_is_gone()
    {
        var (fixture, airlineId, supplierId) = await OperatorAsync();
        var meal = await ProductAsync(fixture, airlineId, supplierId, "V12_ROUTE",
            id => Local(airlineId, "V12_ROUTE", id, LocalInventoryPattern.FlightCount, count: Count(MealResource, 1, InventoryCountUnit.Item)));
        await CountSourceAsync(fixture, airlineId, MealResource, 10);
        var orderId = NextOrderId;
        var hold = await HoldAsync(Hold(orderId, Service(meal, orderId)));
        var path = $"Service/v1/Ancillaries/Hold/{hold.HoldId}";
        await using var host = await ReservationHttpHost.StartAsync(_database, _clock);

        var read = await host.Client.GetAsync(path);
        var emptyCancel = await host.Client.PostAsync($"{path}/Cancellations", new StringContent("""{"reasonCode":"CUSTOMER_REQUEST","unitIds":[]}""", System.Text.Encoding.UTF8, "application/json"));
        var confirmed = await host.Client.PostAsync($"{path}/Confirmations", null);
        var oldRead = await host.Client.GetAsync($"Service/v1/Ancillaries/Service-Holds/{hold.HoldId}");
        var oldRelease = await host.Client.DeleteAsync($"Service/v1/Ancillaries/Service-Holds/{hold.HoldId}");

        Assert.Equal(
            (System.Net.HttpStatusCode.OK, System.Net.HttpStatusCode.BadRequest, System.Net.HttpStatusCode.OK, System.Net.HttpStatusCode.NotFound, System.Net.HttpStatusCode.NotFound),
            (read.StatusCode, emptyCancel.StatusCode, confirmed.StatusCode, oldRead.StatusCode, oldRelease.StatusCode));
        Assert.Equal($"{(int)AncillaryReservationUnitStatus.Confirmed}", await UnitStatesAsync(hold.HoldId));
        Assert.Contains("\"status\":\"Confirmed\"", await confirmed.Content.ReadAsStringAsync(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task V10_I26_a_save_that_fails_after_the_write_leaves_no_reservation_and_no_allocation()
    {
        var (fixture, airlineId, supplierId) = await OperatorAsync();
        var meal = await ProductAsync(fixture, airlineId, supplierId, "G2_ROLLBACK",
            id => Local(airlineId, "G2_ROLLBACK", id, LocalInventoryPattern.FlightCount, count: Count(MealResource, 1, InventoryCountUnit.Item)));
        var source = await CountSourceAsync(fixture, airlineId, MealResource, 5);
        var orderId = NextOrderId;
        var command = Hold(orderId, Service(meal, orderId));

        await using (var scope = new AncillaryScope(_database, _clock))
        {
            var service = new HoldAncillaryServicesService(
                scope.Reservations,
                new HoldSelectionVerifier(new SqlActiveAncillaryProvisionReader(scope.Command), new SqlActiveAncillaryDefinitionReader(scope.Command)),
                new AncillaryHoldAllocator(
                    scope.Definitions,
                    scope.Provisions,
                    scope.Suppliers,
                    new InventoryPolicyRepository(scope.Command),
                    new FlightCountInventoryRepository(scope.Command),
                    new FlightWeightInventoryRepository(scope.Command),
                    new AirportSlotInventoryRepository(scope.Command)),
                new InventoryLedger(scope.Command),
                new TestOnlyHoldLock(),
                new CommandThenFailUnitOfWork(scope.Command),
                _database.Ids,
                _clock);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.HoldAsync(command));
        }

        Assert.Equal(("0|0", "0.000"), (await ReservationsAsync(orderId), await _harness.UsageAsync(FlightCount, source)));

        await HoldAsync(command);

        Assert.Equal(new[] { Uses(FlightCount, source, "1.000") }, await _harness.AllocationsAsync(orderId));
    }

    private sealed class CommandThenFailUnitOfWork : IUnitOfWork
    {
        private readonly AncillaryDbContext _command;

        public CommandThenFailUnitOfWork(AncillaryDbContext command) => _command = command;

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _command.SaveChangesAsync(cancellationToken);

            throw new InvalidOperationException("The unit of work failed after the command write.");
        }
    }
}
