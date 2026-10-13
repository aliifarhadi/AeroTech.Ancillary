using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.ChangeInventoryPolicy.Backoffice;
using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.DefineInventoryPolicy;
using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.DefineInventoryPolicy.Backoffice;
using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.AdjustFlightCountInventory.Backoffice;
using AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.DefineFlightWeightInventory.Backoffice;
using AeroTech.Ancillary.Providers.FlightFlow.Services;
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
    public async Task P2_C01_unlimited_policy_creates_no_stock_rows()
    {
        var (fixture, airlineId, supplierId) = await _harness.OperatorAsync();
        var product = await _harness.ProductAsync(airlineId, supplierId, "PRIORITY_BASIC");
        var draft = await DefineAsync(fixture, new TestDefineInventoryPolicyCommand(airlineId, "PRIORITY_BASIC", product.Id, InventoryAuthority.Unlimited));

        await _harness.RefusedAsync(16602, 422, fixture, scope => scope.ChangeInventoryPolicy.ChangeAsync(
            new TestChangeInventoryPolicyCommand(draft.Id, product.Id, InventoryAuthority.Unlimited, 1, CountConsumption: Count())));

        var active = await ActivateAsync(fixture, draft.Id, 1);
        var snapshot = await _harness.SnapshotAsync(fixture, "PRIORITY_BASIC");

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
        var closed = await _harness.SnapshotAsync(fixture, "PRIORITY_BASIC");

        Assert.Equal((InventoryRecordStatus.Suspended, 3L), (suspended.Status, suspended.Version));
        Assert.Equal(("ClosedForSale", "PolicySuspended", true), (closed.State.Name, closed.ReasonCode, closed.ClosedForSale!.Value));

        await ActivateAsync(fixture, draft.Id, 3);

        var retired = await _harness.RequestAsync(fixture, scope => scope.RetireInventoryPolicy.RetireAsync(new TestInventoryPolicyLifecycleCommand(draft.Id, 4)));

        Assert.Equal((InventoryRecordStatus.Retired, 5L), (retired.Status, retired.Version));
        Assert.Equal("NotConfigured", (await _harness.SnapshotAsync(fixture, "PRIORITY_BASIC")).State.Name);

        var successor = await DefineAsync(fixture, new TestDefineInventoryPolicyCommand(airlineId, "PRIORITY_BASIC", product.Id, InventoryAuthority.Unlimited));

        Assert.NotEqual(draft.Id, successor.Id);
        Assert.Equal(
            new[] { $"{draft.Id}|4|5", $"{successor.Id}|1|1" },
            await _harness.RowsAsync($"SELECT CONCAT(Id, '|', Status, '|', Version) AS Value FROM Ancillary.AncillaryInventoryPolicies WHERE OwnerAirlineId = {airlineId} ORDER BY Id"));
        Assert.Equal("0|0|0|0|0|0", await SourceRowsAsync(airlineId));
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
