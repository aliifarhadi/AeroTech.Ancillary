using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.ChangeInventoryPolicy.Backoffice;
using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.DefineInventoryPolicy;
using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.DefineInventoryPolicy.Backoffice;
using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.AdjustFlightCountInventory.Backoffice;
using AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.DefineFlightWeightInventory.Backoffice;
using AeroTech.Ancillary.Providers.InventoryReferences;
using AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Queries.GetInventoryPoliciesPaginated.Backoffice;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.P1Commands;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.P2Commands;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Inventory;

[Collection(DatabaseCollection.Name)]
public class P2InventoryPolicyAcceptanceTests
{
    private readonly TestDatabase _database;
    private readonly FixedClock _clock = new();
    private readonly InventoryHarness _harness;

    public P2InventoryPolicyAcceptanceTests(TestDatabase database)
    {
        _database = database;
        _harness = new InventoryHarness(database, _clock);
    }

    private static readonly string[] OperationalTerms = ["Available", "Held", "Confirmed", "Sold", "Reserved", "Remaining", "Free"];

    private Task<InventoryPolicyResult> DefineAsync(InventoryFixture fixture, TestDefineInventoryPolicyCommand command)
        => _harness.RequestAsync(fixture, scope => scope.DefineInventoryPolicy.DefineAsync(command));

    private Task<InventoryPolicyResult> ActivateAsync(InventoryFixture fixture, long policyId, long expectedVersion)
        => _harness.RequestAsync(fixture, scope => scope.ActivateInventoryPolicy.ActivateAsync(new TestInventoryPolicyLifecycleCommand(policyId, expectedVersion)));

    private async Task<string> SourceRowsAsync(int airlineId)
        => (await _harness.RowsAsync($"""
            SELECT CONCAT(
                (SELECT COUNT(*) FROM Ancillary.FlightCountInventories WHERE OwnerAirlineId = {airlineId}), '|',
                (SELECT COUNT(*) FROM Ancillary.FlightWeightInventories WHERE OwnerAirlineId = {airlineId}), '|',
                (SELECT COUNT(*) FROM Ancillary.AirportSlotInventories WHERE OwnerAirlineId = {airlineId}), '|',
                (SELECT COUNT(*) FROM ReadModel.FlightCountInventories WHERE OwnerAirlineId = {airlineId}), '|',
                (SELECT COUNT(*) FROM ReadModel.FlightWeightInventories WHERE OwnerAirlineId = {airlineId}), '|',
                (SELECT COUNT(*) FROM ReadModel.AirportSlotInventories WHERE OwnerAirlineId = {airlineId})) AS Value
            """)).Single();

    [Fact]
    public async Task P2_C02_unmapped_legacy_product_is_not_configured_not_unlimited()
    {
        var (fixture, airlineId, supplierId) = await _harness.OperatorAsync();
        var product = await _harness.ProductAsync(airlineId, supplierId, "MEAL_CHML");

        await _harness.Proof.RuleAsync(Provision(product.Id, 10, CommercialDisposition.Free));

        var snapshot = await _harness.SnapshotAsync(fixture, "MEAL_CHML", 81234);

        Assert.Equal(("NotConfigured", "PolicyNotConfigured", false), (snapshot.State.Name, snapshot.ReasonCode, snapshot.IsGuaranteed));
        Assert.Null(snapshot.PolicyId);
        Assert.Null(snapshot.Authority);
        Assert.Null(snapshot.ConfiguredCount);
        Assert.Null(snapshot.ConfiguredKg);
        Assert.Null(snapshot.Resource);
        Assert.Equal(("MEAL_CHML", airlineId, _clock.Now), (snapshot.ServiceDefinitionRef, snapshot.OwnerAirlineId, snapshot.ObservedAt));
        await _harness.RefusedAsync(16601, 404, fixture, scope => scope.GetInventoryPolicyByServiceIdentity.ExecuteAsync("MEAL_CHML"));
        Assert.Equal("0", (await _harness.RowsAsync($"SELECT CAST(COUNT(*) AS varchar(10)) AS Value FROM Ancillary.AncillaryInventoryPolicies WHERE OwnerAirlineId = {airlineId}")).Single());

        var draft = await DefineAsync(fixture, new TestDefineInventoryPolicyCommand(airlineId, "MEAL_CHML", product.Id, InventoryAuthority.Unlimited));
        var inert = await _harness.SnapshotAsync(fixture, "MEAL_CHML", 81234);

        Assert.Equal((InventoryRecordStatus.Draft, 1L), (draft.Status, draft.Version));
        Assert.Equal(("NotConfigured", "PolicyNotActive", draft.Id), (inert.State.Name, inert.ReasonCode, inert.PolicyId!.Value));
        Assert.Equal("NotConfigured", (await _harness.SnapshotAsync(fixture, "UNKNOWN_PRODUCT")).State.Name);
    }

    [Fact]
    public async Task P2_C01_unlimited_policy_creates_no_stock_rows()
    {
        var (fixture, airlineId, supplierId) = await _harness.OperatorAsync();
        var product = await _harness.ProductAsync(airlineId, supplierId, "INS_TRAVEL_BASIC", basis: ServiceDateBasis.CoverageStart);
        var draft = await DefineAsync(fixture, new TestDefineInventoryPolicyCommand(airlineId, "INS_TRAVEL_BASIC", product.Id, InventoryAuthority.Unlimited));

        await _harness.RefusedAsync(16602, 422, fixture, scope => scope.ChangeInventoryPolicy.ChangeAsync(
            new TestChangeInventoryPolicyCommand(draft.Id, product.Id, InventoryAuthority.Unlimited, 1, CountConsumption: Count())));

        var active = await ActivateAsync(fixture, draft.Id, 1);
        var snapshot = await _harness.SnapshotAsync(fixture, "INS_TRAVEL_BASIC");

        Assert.Equal((InventoryRecordStatus.Active, 2L), (active.Status, active.Version));
        Assert.Equal(("Unlimited", "Unlimited", false), (snapshot.State.Name, snapshot.Authority!.Name, snapshot.IsGuaranteed));
        Assert.Null(snapshot.ConfiguredCount);
        Assert.Null(snapshot.ConfiguredKg);
        Assert.Null(snapshot.ClosedForSale);
        Assert.Null(snapshot.Pattern);
        Assert.Equal("0|0|0|0|0|0", await SourceRowsAsync(airlineId));
        await _harness.RefusedAsync(16603, 409, fixture, scope => scope.ChangeInventoryPolicy.ChangeAsync(
            new TestChangeInventoryPolicyCommand(draft.Id, product.Id, InventoryAuthority.Unlimited, 2)));

        var suspended = await _harness.RequestAsync(fixture, scope => scope.SuspendInventoryPolicy.SuspendAsync(new TestInventoryPolicyLifecycleCommand(draft.Id, 2)));
        var closed = await _harness.SnapshotAsync(fixture, "INS_TRAVEL_BASIC");

        Assert.Equal((InventoryRecordStatus.Suspended, 3L), (suspended.Status, suspended.Version));
        Assert.Equal(("ClosedForSale", "PolicySuspended", true), (closed.State.Name, closed.ReasonCode, closed.ClosedForSale!.Value));

        await ActivateAsync(fixture, draft.Id, 3);

        var retired = await _harness.RequestAsync(fixture, scope => scope.RetireInventoryPolicy.RetireAsync(new TestInventoryPolicyLifecycleCommand(draft.Id, 4)));

        Assert.Equal((InventoryRecordStatus.Retired, 5L), (retired.Status, retired.Version));
        Assert.Equal("NotConfigured", (await _harness.SnapshotAsync(fixture, "INS_TRAVEL_BASIC")).State.Name);

        var successor = await DefineAsync(fixture, new TestDefineInventoryPolicyCommand(airlineId, "INS_TRAVEL_BASIC", product.Id, InventoryAuthority.Unlimited));

        Assert.NotEqual(draft.Id, successor.Id);
        Assert.Equal(
            new[] { $"{draft.Id}|4|5", $"{successor.Id}|1|1" },
            await _harness.RowsAsync($"SELECT CONCAT(Id, '|', Status, '|', Version) AS Value FROM Ancillary.AncillaryInventoryPolicies WHERE OwnerAirlineId = {airlineId} ORDER BY Id"));
        Assert.Equal("0|0|0|0|0|0", await SourceRowsAsync(airlineId));
    }

    [Fact]
    public async Task P2_C03_H05_X11_a_supplier_managed_policy_keeps_no_local_stock_and_answers_delegated_check_required()
    {
        var (fixture, airlineId, localSupplierId) = await _harness.OperatorAsync();
        var hotelId = await _harness.Proof.SupplierAsync(new TestRegisterSupplierCommand(airlineId, "City Hotels", SupplierFulfillmentKind.External, "HotelPartnerA"));
        var room = await _harness.ProductAsync(airlineId, hotelId, "HOTEL_ROOM_STD", PricingUnit.PerRoom, ServiceDateBasis.CheckIn);
        var own = await _harness.ProductAsync(airlineId, localSupplierId, "LOUNGE_OWN", basis: ServiceDateBasis.ServiceStart);
        var wrongKey = await DefineAsync(fixture, new TestDefineInventoryPolicyCommand(airlineId, "HOTEL_ROOM_STD", room.Id, InventoryAuthority.Supplier, ProviderKey: "OtherPartner"));

        await _harness.RefusedAsync(16606, 409, fixture, scope => scope.ActivateInventoryPolicy.ActivateAsync(new TestInventoryPolicyLifecycleCommand(wrongKey.Id, 1)));

        var corrected = await _harness.RequestAsync(fixture, scope => scope.ChangeInventoryPolicy.ChangeAsync(
            new TestChangeInventoryPolicyCommand(wrongKey.Id, room.Id, InventoryAuthority.Supplier, 1, ProviderKey: "HotelPartnerA")));
        var active = await ActivateAsync(fixture, wrongKey.Id, corrected.Version);
        var snapshot = await _harness.SnapshotAsync(fixture, "HOTEL_ROOM_STD");

        Assert.Equal((InventoryRecordStatus.Active, 3L, "HotelPartnerA"), (active.Status, active.Version, active.ProviderKey));
        Assert.Equal(("DelegatedCheckRequired", "Supplier", "DelegatedSourceNotConnected", false), (snapshot.State.Name, snapshot.Authority!.Name, snapshot.ReasonCode, snapshot.IsGuaranteed));
        Assert.Null(snapshot.ConfiguredCount);
        Assert.Null(snapshot.ConfiguredKg);
        Assert.Null(snapshot.StaleAfter);
        Assert.Equal("0|0|0|0|0|0", await SourceRowsAsync(airlineId));
        await _harness.RefusedAsync(16602, 422, fixture, scope => scope.DefineInventoryPolicy.DefineAsync(
            new TestDefineInventoryPolicyCommand(airlineId, "LOUNGE_OWN", own.Id, InventoryAuthority.Supplier, LocalInventoryPattern.RoomNight, "HotelPartnerA")));

        var localSupplier = await DefineAsync(fixture, new TestDefineInventoryPolicyCommand(airlineId, "LOUNGE_OWN", own.Id, InventoryAuthority.Supplier, ProviderKey: "HotelPartnerA"));

        await _harness.RefusedAsync(16606, 409, fixture, scope => scope.ActivateInventoryPolicy.ActivateAsync(new TestInventoryPolicyLifecycleCommand(localSupplier.Id, 1)));
        Assert.Equal("NotConfigured", (await _harness.SnapshotAsync(fixture, "LOUNGE_OWN")).State.Name);
    }

    [Fact]
    public async Task P2_C04_X12_a_flightflow_managed_policy_never_mirrors_seat_capacity_and_needs_a_connected_delegation()
    {
        var (fixture, airlineId, supplierId) = await _harness.OperatorAsync(connected: false);
        var seat = await _harness.ProductAsync(airlineId, supplierId, "SEAT_SELECTION", PricingUnit.PerSeat);

        await _harness.RefusedAsync(16602, 422, fixture, scope => scope.DefineInventoryPolicy.DefineAsync(
            new TestDefineInventoryPolicyCommand(airlineId, "SEAT_SELECTION", seat.Id, InventoryAuthority.FlightFlow, LocalInventoryPattern.FlightCount, "FlightFlow", Count())));

        var policy = await DefineAsync(fixture, new TestDefineInventoryPolicyCommand(airlineId, "SEAT_SELECTION", seat.Id, InventoryAuthority.FlightFlow, ProviderKey: "FlightFlow"));

        await _harness.RefusedAsync(16608, 409, fixture, scope => scope.ActivateInventoryPolicy.ActivateAsync(new TestInventoryPolicyLifecycleCommand(policy.Id, 1)));
        Assert.Equal(InventoryReferenceCheck.SourceUnavailable, await new NotConnectedFlightFlowDelegationReference().CheckAsync(airlineId, "FlightFlow"));
        Assert.Equal(InventoryReferenceCheck.SourceUnavailable, await new NotConnectedFlightOccurrenceReference().CheckAsync(airlineId, 81234));
        Assert.Equal(InventoryReferenceCheck.SourceUnavailable, await new NotConnectedInventoryResourceReference().CheckAsync(airlineId, InventoryResourceKind.FlightCount, PetResource));
        Assert.Equal(InventoryReferenceCheck.SourceUnavailable, (await new NotConnectedAirportFacilityReference().CheckAsync(airlineId, Lounge)).Result);
        Assert.Equal(InventoryReferenceCheck.SourceUnavailable, await new NotConnectedCountingFamilyReference().CheckAsync(airlineId, "MEAL"));
        Assert.Equal("NotConfigured", (await _harness.SnapshotAsync(fixture, "SEAT_SELECTION", 81234)).State.Name);

        fixture.FlightFlowProviderKeys = [];

        await _harness.RefusedAsync(16609, 422, fixture, scope => scope.ActivateInventoryPolicy.ActivateAsync(new TestInventoryPolicyLifecycleCommand(policy.Id, 1)));

        fixture.FlightFlowProviderKeys.Add("FlightFlow");

        var active = await ActivateAsync(fixture, policy.Id, 1);
        var snapshot = await _harness.SnapshotAsync(fixture, "SEAT_SELECTION", 81234);

        Assert.Equal(InventoryRecordStatus.Active, active.Status);
        Assert.Equal(("DelegatedCheckRequired", "FlightFlow", false), (snapshot.State.Name, snapshot.Authority!.Name, snapshot.IsGuaranteed));
        Assert.Null(snapshot.ConfiguredCount);
        Assert.Null(snapshot.Resource);
        Assert.Equal("0|0|0|0|0|0", await SourceRowsAsync(airlineId));
    }

    [Fact]
    public async Task P2_X09_legacy_definition_authority_is_not_inferred()
    {
        var (fixture, airlineId, supplierId) = await _harness.OperatorAsync();
        var checkedProduct = await _harness.ProductAsync(airlineId, supplierId, "PET_IN_CABIN", PricingUnit.PerItem);
        var plain = await _harness.ProductAsync(airlineId, supplierId, "WIFI_FULL_FLIGHT", PricingUnit.PerItem);

        await _harness.Proof.RuleAsync(Provision(checkedProduct.Id, 10, CommercialDisposition.Free) with { Availability = new(true) });
        await _harness.Proof.RuleAsync(Provision(plain.Id, 10, CommercialDisposition.Free));

        Assert.Equal("0", (await _harness.RowsAsync($"SELECT CAST(COUNT(*) AS varchar(10)) AS Value FROM Ancillary.AncillaryInventoryPolicies WHERE OwnerAirlineId = {airlineId}")).Single());
        Assert.Equal(
            new[] { "NotConfigured", "NotConfigured" },
            new[] { (await _harness.SnapshotAsync(fixture, "PET_IN_CABIN", 81234)).State.Name, (await _harness.SnapshotAsync(fixture, "WIFI_FULL_FLIGHT", 81234)).State.Name });

        var unlimited = await DefineAsync(fixture, new TestDefineInventoryPolicyCommand(airlineId, "PET_IN_CABIN", checkedProduct.Id, InventoryAuthority.Unlimited));

        var authored = await _harness.SnapshotAsync(fixture, "PET_IN_CABIN", 81234);

        Assert.Equal(("NotConfigured", "PolicyNotActive", true, false), (authored.State.Name, authored.ReasonCode, authored.RequiresAvailabilityCheck, authored.IsGuaranteed));
        Assert.Equal(InventoryRecordStatus.Active, (await ActivateAsync(fixture, unlimited.Id, 1)).Status);

        var declared = await _harness.SnapshotAsync(fixture, "PET_IN_CABIN", 81234);

        Assert.Equal(("Unlimited", true, false), (declared.State.Name, declared.RequiresAvailabilityCheck, declared.IsGuaranteed));
        await _harness.RefusedAsync(16617, 422, fixture, scope => scope.DefineInventoryPolicy.DefineAsync(
            new TestDefineInventoryPolicyCommand(airlineId, "WIFI_MESSAGING", plain.Id, InventoryAuthority.Unlimited)));
        await _harness.RefusedAsync(16617, 422, fixture, scope => scope.DefineInventoryPolicy.DefineAsync(
            new TestDefineInventoryPolicyCommand(airlineId, "WIFI_FULL_FLIGHT", 999_999_999, InventoryAuthority.Unlimited)));

        var wifi = await DefineAsync(fixture, new TestDefineInventoryPolicyCommand(airlineId, "WIFI_FULL_FLIGHT", plain.Id, InventoryAuthority.Unlimited));

        Assert.Equal(InventoryRecordStatus.Active, (await ActivateAsync(fixture, wifi.Id, 1)).Status);
        Assert.Equal("Free", (await _harness.RequestAsync(fixture, scope => scope.Query.AncillaryProvisions.AsNoTracking().Where(row => row.ServiceDefinitionId == plain.Id).Select(row => row.Disposition).SingleAsync())).ToString());
    }

    [Fact]
    public async Task P2_X10_one_current_policy_per_service_identity_even_under_one_hundred_concurrent_requests()
    {
        var (fixture, airlineId, supplierId) = await _harness.OperatorAsync();
        var product = await _harness.ProductAsync(airlineId, supplierId, "MEAL_VGML");
        var command = new TestDefineInventoryPolicyCommand(airlineId, "MEAL_VGML", product.Id, InventoryAuthority.Unlimited);
        var defined = await _harness.RaceAsync(Enumerable.Range(0, 100).Select(_ => (Func<Task>)(() => DefineAsync(fixture, command))));

        Assert.Equal(1, defined.Succeeded);
        Assert.Equal(new[] { 16604 }, defined.Codes);

        var policyId = long.Parse((await _harness.RowsAsync($"SELECT CAST(Id AS varchar(30)) AS Value FROM Ancillary.AncillaryInventoryPolicies WHERE OwnerAirlineId = {airlineId}")).Single());

        await _harness.RefusedAsync(16604, 409, fixture, scope => scope.DefineInventoryPolicy.DefineAsync(command));

        var scopes = Enumerable.Range(0, 100).Select(_ => new InventoryScope(_database, _clock, fixture)).ToList();

        foreach (var scope in scopes)
            await scope.Policies.GetAsync(policyId);

        var activated = await _harness.RaceAsync(scopes.Select(scope => (Func<Task>)(() => scope.ActivateInventoryPolicy.ActivateAsync(new TestInventoryPolicyLifecycleCommand(policyId, 1)))));

        foreach (var scope in scopes)
            await scope.DisposeAsync();

        Assert.Equal(1, activated.Succeeded);
        Assert.Equal(new[] { 16605 }, activated.Codes);
        Assert.Equal(
            new[] { $"{policyId}|2|2|2|2" },
            await _harness.RowsAsync($"SELECT CONCAT(command.Id, '|', command.Status, '|', command.Version, '|', model.Status, '|', model.Version) AS Value FROM Ancillary.AncillaryInventoryPolicies AS command JOIN ReadModel.AncillaryInventoryPolicies AS model ON model.Id = command.Id WHERE command.OwnerAirlineId = {airlineId}"));
    }

    [Fact]
    public async Task P2_C07_a_unit_mismatch_between_the_service_and_its_consumption_is_refused_at_activation()
    {
        var (fixture, airlineId, supplierId) = await _harness.OperatorAsync();
        var perItem = await _harness.ProductAsync(airlineId, supplierId, "XBAG_WEIGHT_5KG", PricingUnit.PerItem);
        var perKilo = await _harness.ProductAsync(airlineId, supplierId, "XBAG_KILO", PricingUnit.PerKilogram);

        fixture.Resources!.Add((InventoryResourceKind.FlightWeight, BagWeightResource));
        fixture.Resources.Add((InventoryResourceKind.FlightCount, PetResource));

        var acceptedOnItem = await DefineAsync(fixture, new TestDefineInventoryPolicyCommand(airlineId, "XBAG_WEIGHT_5KG", perItem.Id, InventoryAuthority.Local, LocalInventoryPattern.FlightWeight, WeightConsumption: AcceptedKg()));
        var countOnKilo = await DefineAsync(fixture, new TestDefineInventoryPolicyCommand(airlineId, "XBAG_KILO", perKilo.Id, InventoryAuthority.Local, LocalInventoryPattern.FlightCount, CountConsumption: Count()));

        await _harness.RefusedAsync(16618, 409, fixture, scope => scope.ActivateInventoryPolicy.ActivateAsync(new TestInventoryPolicyLifecycleCommand(acceptedOnItem.Id, 1)));
        await _harness.RefusedAsync(16618, 409, fixture, scope => scope.ActivateInventoryPolicy.ActivateAsync(new TestInventoryPolicyLifecycleCommand(countOnKilo.Id, 1)));
        await _harness.RefusedAsync(16602, 422, fixture, scope => scope.ChangeInventoryPolicy.ChangeAsync(
            new TestChangeInventoryPolicyCommand(acceptedOnItem.Id, perItem.Id, InventoryAuthority.Local, 1, LocalInventoryPattern.FlightWeight, WeightConsumption: new(BagWeightResource, FlightWeightConsumptionMode.AcceptedWeightKg, 5m))));
        await _harness.RefusedAsync(16602, 422, fixture, scope => scope.ChangeInventoryPolicy.ChangeAsync(
            new TestChangeInventoryPolicyCommand(acceptedOnItem.Id, perItem.Id, InventoryAuthority.Local, 1, LocalInventoryPattern.FlightWeight, WeightConsumption: new(BagWeightResource, FlightWeightConsumptionMode.FixedKgPerAcceptedUnit, null))));

        var bundle = await _harness.RequestAsync(fixture, scope => scope.ChangeInventoryPolicy.ChangeAsync(
            new TestChangeInventoryPolicyCommand(acceptedOnItem.Id, perItem.Id, InventoryAuthority.Local, 1, LocalInventoryPattern.FlightWeight, WeightConsumption: FixedKg(5m))));
        var byWeight = await _harness.RequestAsync(fixture, scope => scope.ChangeInventoryPolicy.ChangeAsync(
            new TestChangeInventoryPolicyCommand(countOnKilo.Id, perKilo.Id, InventoryAuthority.Local, 1, LocalInventoryPattern.FlightWeight, WeightConsumption: AcceptedKg())));

        Assert.Equal(InventoryRecordStatus.Active, (await ActivateAsync(fixture, bundle.Id, bundle.Version)).Status);
        Assert.Equal(InventoryRecordStatus.Active, (await ActivateAsync(fixture, byWeight.Id, byWeight.Version)).Status);

        var detail = await _harness.RequestAsync(fixture, scope => scope.GetInventoryPolicyById.ExecuteAsync(bundle.Id));

        Assert.Equal(("FixedKgPerAcceptedUnit", (decimal?)5m, BagWeightResource), (detail.WeightConsumption!.ConsumptionMode.Name, detail.WeightConsumption.FixedKgPerUnit, detail.WeightConsumption.WeightResourceId));
        Assert.Null(detail.CountConsumption);
    }

    [Fact]
    public async Task P2_P01_A07_passenger_usage_limits_are_policy_rows_beside_the_binding_and_need_a_registered_family()
    {
        var (fixture, airlineId, supplierId) = await _harness.OperatorAsync();
        var meal = await _harness.ProductAsync(airlineId, supplierId, "MEAL_PREORDER_HOT");

        fixture.Resources!.Add((InventoryResourceKind.FlightCount, MealResource));

        var draft = await DefineAsync(fixture, new TestDefineInventoryPolicyCommand(
            airlineId,
            "MEAL_PREORDER_HOT",
            meal.Id,
            InventoryAuthority.Local,
            LocalInventoryPattern.FlightCount,
            CountConsumption: Count(MealResource, 1, InventoryCountUnit.Item),
            PassengerUsageLimits: [Limit(PassengerUsageLimitScope.PerFlightOccurrence, 1, "meal"), Limit(PassengerUsageLimitScope.PerOrder, 4, "MEAL")]));
        var detail = await _harness.RequestAsync(fixture, scope => scope.GetInventoryPolicyById.ExecuteAsync(draft.Id));

        Assert.Equal(
            new[] { ("PerFlightOccurrence", 1, "MEAL"), ("PerOrder", 4, "MEAL") },
            detail.PassengerUsageLimits.Select(limit => (limit.LimitScope.Name, limit.MaxUnits, limit.CountingFamilyCode)));
        Assert.Equal((MealResource, 1, "Item"), (detail.CountConsumption!.ResourceId, detail.CountConsumption.CountPerAcceptedUnit, detail.CountConsumption.CountUnit.Name));
        await _harness.RefusedAsync(16609, 422, fixture, scope => scope.ActivateInventoryPolicy.ActivateAsync(new TestInventoryPolicyLifecycleCommand(draft.Id, 1)));

        fixture.CountingFamilies!.Add("MEAL");

        var changed = await _harness.RequestAsync(fixture, scope => scope.ChangeInventoryPolicy.ChangeAsync(new TestChangeInventoryPolicyCommand(
            draft.Id,
            meal.Id,
            InventoryAuthority.Local,
            1,
            LocalInventoryPattern.FlightCount,
            CountConsumption: Count(MealResource, 1, InventoryCountUnit.Item),
            PassengerUsageLimits: [Limit(PassengerUsageLimitScope.PerFlightOccurrence, 1, "MEAL")])));
        var active = await ActivateAsync(fixture, draft.Id, changed.Version);
        var stored = await _harness.RequestAsync(fixture, scope => scope.GetInventoryPolicyByServiceIdentity.ExecuteAsync("MEAL_PREORDER_HOT"));

        Assert.Equal((InventoryRecordStatus.Active, 3L), (active.Status, active.Version));
        Assert.Equal((detail.PassengerUsageLimits[0].Id, "PerFlightOccurrence"), stored.PassengerUsageLimits.Select(limit => (limit.Id, limit.LimitScope.Name)).Single());
        Assert.Equal(
            new[] { "1|1" },
            await _harness.RowsAsync($"SELECT CONCAT((SELECT COUNT(*) FROM Ancillary.InventoryPassengerUsageLimits WHERE InventoryPolicyId = {draft.Id}), '|', (SELECT COUNT(*) FROM ReadModel.AncillaryInventoryPassengerUsageLimits WHERE InventoryPolicyId = {draft.Id})) AS Value"));
        Assert.Empty(await _harness.RowsAsync($"SELECT t.name AS Value FROM sys.tables AS t WHERE t.name LIKE '%Usage%' AND t.name NOT LIKE '%PassengerUsageLimits'"));
    }

    [Theory]
    [InlineData(LocalInventoryPattern.DailyCount)]
    [InlineData(LocalInventoryPattern.RoomNight)]
    [InlineData(LocalInventoryPattern.AssignedAsset)]
    public async Task P2_D04_H06_R03_a_pattern_without_a_verified_source_model_is_reported_unsupported_and_never_unlimited(LocalInventoryPattern pattern)
    {
        var (fixture, airlineId, supplierId) = await _harness.OperatorAsync();
        var product = await _harness.ProductAsync(airlineId, supplierId, $"TOUR_{pattern}".ToUpperInvariant(), PricingUnit.PerItem, ServiceDateBasis.ServiceStart);
        var draft = await DefineAsync(fixture, new TestDefineInventoryPolicyCommand(airlineId, product.ServiceDefinitionRef, product.Id, InventoryAuthority.Local, pattern));

        await _harness.RefusedAsync(16607, 409, fixture, scope => scope.ActivateInventoryPolicy.ActivateAsync(new TestInventoryPolicyLifecycleCommand(draft.Id, 1)));

        var snapshot = await _harness.SnapshotAsync(fixture, product.ServiceDefinitionRef, 81234);

        Assert.Equal(("UnsupportedPattern", "PatternNotSupported", pattern.ToString(), false), (snapshot.State.Name, snapshot.ReasonCode, snapshot.Pattern!.Name, snapshot.IsGuaranteed));
        Assert.Null(snapshot.ConfiguredCount);
        Assert.Equal("Draft", (await _harness.RequestAsync(fixture, scope => scope.GetInventoryPolicyById.ExecuteAsync(draft.Id))).Status.Name);
        Assert.Empty(await _harness.RowsAsync($"SELECT t.name AS Value FROM sys.tables AS t WHERE t.name LIKE '%Daily%' OR t.name LIKE '%RoomNight%' OR t.name LIKE '%Asset%' OR t.name LIKE '%StockPool%'"));
    }

    [Fact]
    public async Task P2_X02_inventory_is_managed_only_by_an_airline_caller_of_the_operator_and_never_across_owners()
    {
        var (fixture, airlineId, supplierId) = await _harness.OperatorAsync();
        var (other, otherAirlineId, otherSupplierId) = await _harness.OperatorAsync();
        var mine = await _harness.ProductAsync(airlineId, supplierId, "FAST_TRACK", basis: ServiceDateBasis.ServiceStart);
        var theirs = await _harness.ProductAsync(otherAirlineId, otherSupplierId, "FAST_TRACK", basis: ServiceDateBasis.ServiceStart);
        var theirPolicy = await DefineAsync(other, new TestDefineInventoryPolicyCommand(otherAirlineId, "FAST_TRACK", theirs.Id, InventoryAuthority.Unlimited));

        await _harness.RefusedAsync(16610, 403, fixture, scope => scope.DefineInventoryPolicy.DefineAsync(
            new TestDefineInventoryPolicyCommand(otherAirlineId, "FAST_TRACK", theirs.Id, InventoryAuthority.Unlimited)));
        await _harness.RefusedAsync(16617, 422, fixture, scope => scope.DefineInventoryPolicy.DefineAsync(
            new TestDefineInventoryPolicyCommand(airlineId, "FAST_TRACK", theirs.Id, InventoryAuthority.Unlimited)));
        await _harness.RefusedAsync(16601, 404, fixture, scope => scope.GetInventoryPolicyById.ExecuteAsync(theirPolicy.Id));
        await _harness.RefusedAsync(16601, 404, fixture, scope => scope.ActivateInventoryPolicy.ActivateAsync(new TestInventoryPolicyLifecycleCommand(theirPolicy.Id, 1)));
        await _harness.RefusedAsync(16601, 404, fixture, scope => scope.RetireInventoryPolicy.RetireAsync(new TestInventoryPolicyLifecycleCommand(theirPolicy.Id, 1)));
        await _harness.RefusedAsync(16601, 404, fixture, scope => scope.GetInventoryPolicyByServiceIdentity.ExecuteAsync("FAST_TRACK"));

        var minePolicy = await DefineAsync(fixture, new TestDefineInventoryPolicyCommand(airlineId, "FAST_TRACK", mine.Id, InventoryAuthority.Unlimited));
        var listed = await _harness.RequestAsync(fixture, scope => scope.GetInventoryPoliciesPaginated.ExecuteAsync(new BackofficeGetInventoryPoliciesPaginatedQuery { PageSize = 50 }));

        Assert.Equal(new[] { minePolicy.Id.ToString() }, listed.Results.Select(row => row.Id));
        Assert.Equal(1, listed.TotalCount);
        Assert.Equal(
            new[] { "Airline", "Service", "Authority", "Pattern", "Provider Key", "Status", "Version", "Updated" },
            listed.Metadata.Fields.Select(field => field.Title));

        foreach (var caller in new[]
                 {
                     new InventoryFixture(airlineId) { ContextType = CallerContextType.TravelAgency },
                     new InventoryFixture(airlineId) { ContextType = CallerContextType.None },
                     new InventoryFixture(airlineId) { ContextType = CallerContextType.Service },
                     new InventoryFixture(airlineId) { HomeAirlineId = null },
                     new InventoryFixture(airlineId) { HomeAirlineId = otherAirlineId }
                 })
        {
            await _harness.RefusedAsync(16610, 403, caller, scope => scope.DefineInventoryPolicy.DefineAsync(
                new TestDefineInventoryPolicyCommand(airlineId, "FAST_TRACK", mine.Id, InventoryAuthority.Unlimited)));
            await _harness.RefusedAsync(caller.HomeAirlineId == otherAirlineId ? 16601 : 16610, caller.HomeAirlineId == otherAirlineId ? 404 : 403, caller, scope => scope.GetInventoryPolicyById.ExecuteAsync(minePolicy.Id));
        }

        Assert.Equal("NotConfigured", (await _harness.SnapshotAsync(other, "MEAL_CHML")).State.Name);
        Assert.Equal(otherAirlineId, (await _harness.SnapshotAsync(other, "FAST_TRACK")).OwnerAirlineId);
    }

    [Fact]
    public async Task P2_X05_the_policy_read_model_equals_the_command_store_after_every_step()
    {
        var (fixture, airlineId, supplierId) = await _harness.OperatorAsync();
        var bike = await _harness.ProductAsync(airlineId, supplierId, "SPORT_BIKE", PricingUnit.PerPiece);

        fixture.Resources!.Add((InventoryResourceKind.FlightCount, OversizeResource));
        fixture.Resources.Add((InventoryResourceKind.FlightWeight, BagWeightResource));
        fixture.CountingFamilies!.Add("SPORT");

        async Task<List<string>> DifferencesAsync() => await _harness.RowsAsync($"""
            SELECT CONCAT('policy ', COALESCE(command.Id, model.Id)) AS Value
            FROM Ancillary.AncillaryInventoryPolicies AS command
            FULL JOIN ReadModel.AncillaryInventoryPolicies AS model ON model.Id = command.Id
            WHERE COALESCE(command.OwnerAirlineId, model.OwnerAirlineId) = {airlineId}
              AND (command.Id IS NULL OR model.Id IS NULL
                OR command.ServiceDefinitionRef <> model.ServiceDefinitionRef OR command.ServiceDefinitionId <> model.ServiceDefinitionId
                OR command.Authority <> model.Authority OR ISNULL(command.LocalPattern, 0) <> ISNULL(model.LocalPattern, 0)
                OR ISNULL(command.ProviderKey, '') <> ISNULL(model.ProviderKey, '')
                OR ISNULL(command.CountResourceId, 0) <> ISNULL(model.CountResourceId, 0) OR ISNULL(command.CountPerAcceptedUnit, 0) <> ISNULL(model.CountPerAcceptedUnit, 0)
                OR ISNULL(command.CountUnit, 0) <> ISNULL(model.CountUnit, 0) OR ISNULL(command.WeightResourceId, 0) <> ISNULL(model.WeightResourceId, 0)
                OR ISNULL(command.WeightConsumptionMode, 0) <> ISNULL(model.WeightConsumptionMode, 0)
                OR ISNULL(command.WeightFixedKgPerUnit, -1) <> ISNULL(model.WeightFixedKgPerUnit, -1)
                OR ISNULL(command.SlotFacilityId, 0) <> ISNULL(model.SlotFacilityId, 0)
                OR command.Status <> model.Status OR command.Version <> model.Version
                OR command.CreatedAt <> model.CreatedAt OR command.UpdatedAt <> model.UpdatedAt
                OR ISNULL(command.ActivatedAt, '1900-01-01') <> ISNULL(model.ActivatedAt, '1900-01-01')
                OR ISNULL(command.SuspendedAt, '1900-01-01') <> ISNULL(model.SuspendedAt, '1900-01-01')
                OR ISNULL(command.RetiredAt, '1900-01-01') <> ISNULL(model.RetiredAt, '1900-01-01'))
            UNION ALL
            SELECT CONCAT('limit ', COALESCE(command.Id, model.Id))
            FROM Ancillary.InventoryPassengerUsageLimits AS command
            FULL JOIN ReadModel.AncillaryInventoryPassengerUsageLimits AS model ON model.Id = command.Id
            WHERE command.Id IS NULL OR model.Id IS NULL OR command.LimitScope <> model.LimitScope OR command.MaxUnits <> model.MaxUnits
               OR command.CountingFamilyCode <> model.CountingFamilyCode OR command.InventoryPolicyId <> model.InventoryPolicyId
            """);

        var draft = await DefineAsync(fixture, new TestDefineInventoryPolicyCommand(
            airlineId,
            "SPORT_BIKE",
            bike.Id,
            InventoryAuthority.Local,
            LocalInventoryPattern.FlightCountPlusWeight,
            CountConsumption: Count(OversizeResource, 1, InventoryCountUnit.Equipment),
            WeightConsumption: FixedKg(12.5m),
            PassengerUsageLimits: [Limit(PassengerUsageLimitScope.PerOrder, 2, "SPORT")]));

        Assert.Empty(await DifferencesAsync());

        _clock.Now = _clock.Now.AddMinutes(5);

        var changed = await _harness.RequestAsync(fixture, scope => scope.ChangeInventoryPolicy.ChangeAsync(new TestChangeInventoryPolicyCommand(
            draft.Id,
            bike.Id,
            InventoryAuthority.Local,
            1,
            LocalInventoryPattern.FlightCountPlusWeight,
            CountConsumption: Count(OversizeResource, 1, InventoryCountUnit.Equipment),
            WeightConsumption: FixedKg(12.125m))));

        Assert.Empty(await DifferencesAsync());

        _clock.Now = _clock.Now.AddMinutes(5);
        await ActivateAsync(fixture, draft.Id, changed.Version);
        Assert.Empty(await DifferencesAsync());

        _clock.Now = _clock.Now.AddMinutes(5);
        await _harness.RequestAsync(fixture, scope => scope.SuspendInventoryPolicy.SuspendAsync(new TestInventoryPolicyLifecycleCommand(draft.Id, 3)));
        Assert.Empty(await DifferencesAsync());

        var detail = await _harness.RequestAsync(fixture, scope => scope.GetInventoryPolicyById.ExecuteAsync(draft.Id));

        Assert.Equal(
            ("Suspended", 4L, "FlightCountPlusWeight", 12.125m, _clock.Now, _clock.Now.AddMinutes(-5)),
            (detail.Status.Name, detail.Version, detail.LocalPattern!.Name, detail.WeightConsumption!.FixedKgPerUnit!.Value, detail.SuspendedAt!.Value, detail.ActivatedAt!.Value));
        Assert.Empty(detail.PassengerUsageLimits);
    }

    [Fact]
    public void P2_X08_snapshot_cannot_claim_booking_guarantee()
    {
        var properties = typeof(InventoryConfigurationSnapshotDto).GetProperties().Select(property => property.Name).ToArray();

        Assert.Equal(
            new[]
            {
                "Authority", "ClosedForSale", "ConfiguredCount", "ConfiguredKg", "CurrentServiceDefinitionId", "IsGuaranteed", "ObservedAt", "OwnerAirlineId", "Pattern", "PolicyId", "ReasonCode",
                "RequiresAvailabilityCheck", "Resource", "ResourceKind", "ServiceDefinitionRef", "StaleAfter", "State"
            },
            properties.OrderBy(name => name, StringComparer.Ordinal));
        Assert.DoesNotContain(properties, name => OperationalTerms.Any(term => name.Contains(term, StringComparison.Ordinal)));
        Assert.Equal(
            new[] { "NotConfigured", "Unlimited", "ConfiguredNotGuaranteed", "ClosedForSale", "DelegatedCheckRequired", "Unknown", "UnsupportedPattern" },
            Enum.GetNames<InventoryCapacityReadState>());
        Assert.DoesNotContain(Enum.GetNames<InventoryCapacityReadState>(), name => name is "Available" or "SoldOut" or "Reserved");
    }

    [Fact]
    public void P2_X02_the_backoffice_validators_check_the_shape_of_every_inventory_input()
    {
        string[] DefineErrors(BackofficeDefineInventoryPolicyCommand command)
            => new BackofficeDefineInventoryPolicyCommandValidator().Validate(command).Errors.Select(error => error.PropertyName).ToArray();

        var valid = new BackofficeDefineInventoryPolicyCommand(
            1,
            "SPORT_BIKE",
            1001,
            InventoryAuthority.Local,
            LocalInventoryPattern.FlightCountPlusWeight,
            null,
            Count(OversizeResource, 1, InventoryCountUnit.Equipment),
            FixedKg(12.5m),
            null,
            [Limit(PassengerUsageLimitScope.PerOrder, 2, "SPORT")]);

        Assert.Empty(DefineErrors(valid));
        Assert.Equal(new[] { "OwnerAirlineId", "ServiceDefinitionRef", "ServiceDefinitionId", "Authority" }, DefineErrors(valid with { OwnerAirlineId = 0, ServiceDefinitionRef = "", ServiceDefinitionId = 0, Authority = (InventoryAuthority)9 }));
        Assert.Contains("LocalPattern", DefineErrors(valid with { LocalPattern = (LocalInventoryPattern)99 }));
        Assert.Contains("ProviderKey", DefineErrors(valid with { ProviderKey = new string('k', 51) }));
        Assert.Contains("CountConsumption.CountUnit", DefineErrors(valid with { CountConsumption = Count(unit: (InventoryCountUnit)99) }));
        Assert.Contains("WeightConsumption.ConsumptionMode", DefineErrors(valid with { WeightConsumption = new(BagWeightResource, (FlightWeightConsumptionMode)9, null) }));
        Assert.Contains("PassengerUsageLimits[0].LimitScope", DefineErrors(valid with { PassengerUsageLimits = [Limit((PassengerUsageLimitScope)9, 1, "SPORT")] }));
        Assert.Empty(DefineErrors(valid with { CountConsumption = Count(perUnit: 0) }));
        Assert.Empty(DefineErrors(valid with { WeightConsumption = FixedKg(5.0005m) }));
        Assert.Empty(DefineErrors(valid with { SlotConsumption = SlotUse(occupancyMinutes: 0) }));
        Assert.Empty(DefineErrors(valid with { PassengerUsageLimits = [Limit(PassengerUsageLimitScope.PerOrder, 0, "SPORT")] }));
        Assert.Equal(
            new[] { "PolicyId", "ExpectedVersion", "ServiceDefinitionId" },
            new BackofficeChangeInventoryPolicyCommandValidator()
                .Validate(new BackofficeChangeInventoryPolicyCommand(0, 0, InventoryAuthority.Unlimited, null, null, null, null, null, null, 0))
                .Errors.Select(error => error.PropertyName));
        Assert.Equal(
            new[] { "InventoryId", "ExpectedVersion", "ReasonCode", "CorrelationId" },
            new BackofficeAdjustFlightCountInventoryCommandValidator()
                .Validate(new BackofficeAdjustFlightCountInventoryCommand(0, -1, "", "", 0))
                .Errors.Select(error => error.PropertyName).Distinct());
        Assert.Equal(
            new[] { "OwnerAirlineId", "FlightId", "WeightResourceId" },
            new BackofficeDefineFlightWeightInventoryCommandValidator()
                .Validate(new BackofficeDefineFlightWeightInventoryCommand(0, 0, 0, 10.0005m))
                .Errors.Select(error => error.PropertyName));
    }
}
