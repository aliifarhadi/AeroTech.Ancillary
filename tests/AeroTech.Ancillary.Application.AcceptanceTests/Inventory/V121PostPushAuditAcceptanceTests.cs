using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing;
using AeroTech.Ancillary.ReferenceData.ReadModels;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.P1Commands;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.P2Commands;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Inventory;

[Collection(DatabaseCollection.Name)]
public class V121PostPushAuditAcceptanceTests
{
    private const long Flight = 81234;
    private const int ProbeCurrency = 9901;

    private readonly TestDatabase _database;
    private readonly FixedClock _clock = new();
    private readonly InventoryHarness _harness;

    public V121PostPushAuditAcceptanceTests(TestDatabase database)
    {
        _database = database;
        _harness = new InventoryHarness(database, _clock);
    }

    private async Task<TResult> RequestAsync<TResult>(Func<AncillaryScope, Task<TResult>> request)
    {
        await using var scope = new AncillaryScope(_database, _clock);

        return await request(scope);
    }

    private Task RefusedAsync(int code, int httpStatus, Func<AncillaryScope, Task> request)
        => BusinessAssert.ThrowsAsync(code, httpStatus, async () =>
        {
            await using var scope = new AncillaryScope(_database, _clock);

            await request(scope);
        });

    private async Task SetProbeDecimalPlacesAsync(int? decimalPlaces)
    {
        await using var reference = _database.NewReferenceContext();
        var stored = await reference.Currencies.FirstOrDefaultAsync(currency => currency.Id == ProbeCurrency);

        if (stored is not null)
            reference.Currencies.Remove(stored);

        if (decimalPlaces is { } places)
            reference.Currencies.Add(new CurrencyReadModel { Id = ProbeCurrency, Code = "XTS", DecimalPlaces = places, RoundingFactor = 0 });

        await reference.SaveChangesAsync();
    }

    private async Task<long> NextVersionAsync(long definitionId)
    {
        await using var author = new AncillaryScope(_database, _clock);
        var revisionId = (await author.ReviseServiceDefinition.ReviseAsync(new TestServiceDefinitionLifecycleCommand(definitionId))).Id;

        await author.RetireServiceDefinition.RetireAsync(new TestServiceDefinitionLifecycleCommand(definitionId));
        await author.ActivateServiceDefinition.ActivateAsync(new TestActivateServiceDefinitionCommand(revisionId));

        return revisionId;
    }

    [Fact]
    public async Task F01_the_amount_scale_follows_the_currency_reference_only_and_a_wrong_or_missing_reference_fails_closed()
    {
        var airlineId = _database.NextAirlineId();
        var supplierId = await _harness.Proof.SupplierAsync(M1Commands.LocalSupplier(airlineId));
        var definition = await _harness.Proof.DefinitionAsync(CarrierDefinition(airlineId, supplierId, "SERVICE", "SVC", "F", "TS", "Service", pricingUnit: PricingUnit.PerItem));
        var provisionId = (await RequestAsync(scope => scope.DefineProvision.DefineAsync(Provision(definition.Id, 10)))).Id;
        var price = new TestDefinePricingRatesCommand(provisionId, [new PricingRateInput(null, null, null, new MoneyInput(7.5m, ProbeCurrency), null)]);

        try
        {
            await SetProbeDecimalPlacesAsync(null);
            await RefusedAsync(16511, 422, scope => scope.DefinePricing.DefineAsync(price));

            await SetProbeDecimalPlacesAsync(0);
            await RefusedAsync(16512, 422, scope => scope.DefinePricing.DefineAsync(price));

            await SetProbeDecimalPlacesAsync(2);

            var draft = await RequestAsync(scope => scope.DefinePricing.DefineAsync(price));

            await SetProbeDecimalPlacesAsync(0);
            await RefusedAsync(16512, 422, scope => scope.PublishProvision.PublishAsync(new TestPublishProvisionCommand(provisionId, draft.Id)));

            await SetProbeDecimalPlacesAsync(null);
            await RefusedAsync(16511, 422, scope => scope.PublishProvision.PublishAsync(new TestPublishProvisionCommand(provisionId, draft.Id)));

            await SetProbeDecimalPlacesAsync(2);
            await RequestAsync(scope => scope.PublishProvision.PublishAsync(new TestPublishProvisionCommand(provisionId, draft.Id)));

            Assert.Equal(
                new[] { $"{ProbeCurrency} 7.500000 2" },
                await _harness.RowsAsync($"""
                    SELECT CONCAT(rate.CurrencyId, ' ', rate.BaseAmount, ' ', pricing.Status) AS Value
                    FROM Ancillary.AncillaryPricingRates AS rate JOIN Ancillary.AncillaryPricings AS pricing ON pricing.Id = rate.AncillaryPricingId
                    WHERE pricing.AncillaryProvisionId = {provisionId}
                    """));
        }
        finally
        {
            await SetProbeDecimalPlacesAsync(null);
        }
    }

    [Fact]
    public async Task F03_the_availability_check_flag_follows_the_current_definition_version_and_never_a_retired_one()
    {
        var (fixture, airlineId, supplierId) = await _harness.OperatorAsync(connected: false);
        var first = await _harness.ProductAsync(airlineId, supplierId, "ASSIST_WCHR");
        var retiredRule = await _harness.Proof.RuleAsync(Provision(first.Id, 10, CommercialDisposition.Free) with { Availability = new(true) });
        var policy = await _harness.RequestAsync(fixture, scope => scope.DefineInventoryPolicy.DefineAsync(
            new TestDefineInventoryPolicyCommand(airlineId, "ASSIST_WCHR", first.Id, InventoryAuthority.Unlimited)));

        await _harness.RequestAsync(fixture, scope => scope.ActivateInventoryPolicy.ActivateAsync(new TestInventoryPolicyLifecycleCommand(policy.Id, 1)));

        var before = await _harness.SnapshotAsync(fixture, "ASSIST_WCHR", Flight);
        long secondId;

        await using (var author = new AncillaryScope(_database, _clock))
            secondId = (await author.ReviseServiceDefinition.ReviseAsync(new TestServiceDefinitionLifecycleCommand(first.Id))).Id;

        var whileDrafted = await _harness.SnapshotAsync(fixture, "ASSIST_WCHR", Flight);

        await using (var publisher = new AncillaryScope(_database, _clock))
        {
            await publisher.RetireServiceDefinition.RetireAsync(new TestServiceDefinitionLifecycleCommand(first.Id));
            await publisher.ActivateServiceDefinition.ActivateAsync(new TestActivateServiceDefinitionCommand(secondId));
        }

        await _harness.Proof.RuleAsync(Provision(secondId, 10, CommercialDisposition.Free));

        var after = await _harness.SnapshotAsync(fixture, "ASSIST_WCHR", Flight);

        Assert.Equal((first.Id, true, false), (before.CurrentServiceDefinitionId!.Value, before.RequiresAvailabilityCheck, before.IsGuaranteed));
        Assert.Equal((first.Id, true), (whileDrafted.CurrentServiceDefinitionId!.Value, whileDrafted.RequiresAvailabilityCheck));
        Assert.Equal(
            new[] { $"{first.Id} 4 {retiredRule.Provision.Id} 2 1" },
            await _harness.RowsAsync($"""
                SELECT CONCAT(definition.Id, ' ', definition.Status, ' ', provision.Id, ' ', provision.Status, ' ', provision.MustCheckAvailability) AS Value
                FROM Ancillary.AncillaryProvisions AS provision JOIN Ancillary.AncillaryServiceDefinitions AS definition ON definition.Id = provision.ServiceDefinitionId
                WHERE provision.Id = {retiredRule.Provision.Id}
                """));
        Assert.Equal(
            (secondId, "Unlimited", false, false, policy.Id),
            (after.CurrentServiceDefinitionId!.Value, after.State.Name, after.RequiresAvailabilityCheck, after.IsGuaranteed, after.PolicyId!.Value));

        await _harness.Proof.RuleAsync(Provision(secondId, 20, CommercialDisposition.Free) with { Availability = new(true) });

        var current = await _harness.SnapshotAsync(fixture, "ASSIST_WCHR", Flight);

        Assert.Equal((secondId, true, false), (current.CurrentServiceDefinitionId!.Value, current.RequiresAvailabilityCheck, current.IsGuaranteed));
    }

    [Fact]
    public async Task F04_an_active_policy_reports_the_current_definition_version_beside_its_stored_pointer_and_keeps_its_physical_binding()
    {
        var (fixture, airlineId, supplierId) = await _harness.OperatorAsync();

        fixture.Flights!.Add(Flight);
        fixture.Resources!.Add((InventoryResourceKind.FlightCount, PetResource));

        var first = await _harness.ProductAsync(airlineId, supplierId, "PET_IN_CABIN", PricingUnit.PerItem);
        var policy = await _harness.RequestAsync(fixture, scope => scope.DefineInventoryPolicy.DefineAsync(
            new TestDefineInventoryPolicyCommand(airlineId, "PET_IN_CABIN", first.Id, InventoryAuthority.Local, LocalInventoryPattern.FlightCount, CountConsumption: Count())));

        await _harness.RequestAsync(fixture, scope => scope.ActivateInventoryPolicy.ActivateAsync(new TestInventoryPolicyLifecycleCommand(policy.Id, 1)));

        var source = await _harness.RequestAsync(fixture, scope => scope.DefineFlightCountInventory.DefineAsync(
            new TestDefineFlightCountInventoryCommand(airlineId, Flight, PetResource, InventoryCountUnit.AnimalCarrier, 2)));

        await _harness.RequestAsync(fixture, scope => scope.ActivateFlightCountInventory.ActivateAsync(new TestFlightCountLifecycleCommand(source.Id, 1)));

        var published = await _harness.RequestAsync(fixture, scope => scope.GetInventoryPolicyById.ExecuteAsync(policy.Id));
        var secondId = await NextVersionAsync(first.Id);
        var byId = await _harness.RequestAsync(fixture, scope => scope.GetInventoryPolicyById.ExecuteAsync(policy.Id));
        var byIdentity = await _harness.RequestAsync(fixture, scope => scope.GetInventoryPolicyByServiceIdentity.ExecuteAsync("PET_IN_CABIN"));
        var snapshot = await _harness.SnapshotAsync(fixture, "PET_IN_CABIN", Flight);

        Assert.Equal((first.Id, first.Id, 2L), (published.ServiceDefinitionId, published.CurrentServiceDefinitionId!.Value, published.Version));
        Assert.NotEqual(first.Id, secondId);
        Assert.Equal((policy.Id, first.Id, secondId, "Active", 2L), (byId.Id, byId.ServiceDefinitionId, byId.CurrentServiceDefinitionId!.Value, byId.Status.Name, byId.Version));
        Assert.Equal(byId, byIdentity with { PassengerUsageLimits = byId.PassengerUsageLimits });
        Assert.Equal(
            (policy.Id, secondId, "ConfiguredNotGuaranteed", 2, false, Flight, PetResource),
            (snapshot.PolicyId!.Value, snapshot.CurrentServiceDefinitionId!.Value, snapshot.State.Name, snapshot.ConfiguredCount!.Value, snapshot.IsGuaranteed, snapshot.Resource!.FlightId!.Value,
                snapshot.Resource.ResourceId!.Value));
        Assert.Equal(
            new[] { $"{policy.Id}|{first.Id}|2|2|{source.Id}|2|2" },
            await _harness.RowsAsync($"""
                SELECT CONCAT(policy.Id, '|', policy.ServiceDefinitionId, '|', policy.Status, '|', policy.Version, '|', source.Id, '|', source.TotalCapacity, '|', source.Version) AS Value
                FROM ReadModel.AncillaryInventoryPolicies AS policy
                JOIN Ancillary.FlightCountInventories AS source ON source.OwnerAirlineId = policy.OwnerAirlineId AND source.ResourceId = policy.CountResourceId
                WHERE policy.OwnerAirlineId = {airlineId}
                """));

        await using (var author = new AncillaryScope(_database, _clock))
            await author.RetireServiceDefinition.RetireAsync(new TestServiceDefinitionLifecycleCommand(secondId));

        var orphaned = await _harness.RequestAsync(fixture, scope => scope.GetInventoryPolicyById.ExecuteAsync(policy.Id));
        var orphanedSnapshot = await _harness.SnapshotAsync(fixture, "PET_IN_CABIN", Flight);
        var suspended = await _harness.RequestAsync(fixture, scope => scope.SuspendInventoryPolicy.SuspendAsync(new TestInventoryPolicyLifecycleCommand(policy.Id, 2)));

        Assert.Equal((first.Id, (long?)null), (orphaned.ServiceDefinitionId, orphaned.CurrentServiceDefinitionId));
        Assert.Equal(((long?)null, false, false), (orphanedSnapshot.CurrentServiceDefinitionId, orphanedSnapshot.RequiresAvailabilityCheck, orphanedSnapshot.IsGuaranteed));
        await _harness.RefusedAsync(16617, 422, fixture, scope => scope.ActivateInventoryPolicy.ActivateAsync(new TestInventoryPolicyLifecycleCommand(policy.Id, suspended.Version)));
    }
}
