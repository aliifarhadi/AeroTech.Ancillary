using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.DefineInventoryPolicy;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionsPaginated.Backoffice;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.P1Commands;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.P2Commands;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Families;

[Collection(DatabaseCollection.Name)]
public class V122JourneyAcceptanceTests
{
    private const long Flight = 81234;
    private const string FlightFlowKey = "FlightFlow";
    private const string WeightFamily = "XBAG_WEIGHT";

    private static readonly DateTimeOffset At = Utc(10, 15);

    private readonly TestDatabase _database;
    private readonly FixedClock _clock = new();
    private readonly InventoryHarness _harness;

    public V122JourneyAcceptanceTests(TestDatabase database)
    {
        _database = database;
        _harness = new InventoryHarness(database, _clock);
    }

    private static Func<long, IDefineAncillaryPricingCommand> Euro(decimal amount)
        => provisionId => new TestDefinePricingRatesCommand(provisionId, [new(null, null, null, new MoneyInput(amount, Eur), null)]);

    private static TestDefineProvisionCommand Rule(string code, long definitionId, int sequence = 10) => V122Catalog.Rule(V122Catalog.Case(code), definitionId, sequence);

    private static TestDefineProvisionCommand Unavailable(TestDefineProvisionCommand rule)
        => rule with { Outcome = new(CommercialDisposition.NotAvailable, false, false), Origin = PriceOrigin.NotAvailable, QuoteProviderKey = null };

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

    private async Task<(int AirlineId, long SupplierId)> OperatorAsync(bool external = false)
    {
        var airlineId = _database.NextAirlineId();

        return (airlineId, await _harness.Proof.SupplierAsync(
            external ? M1Commands.ExternalSupplier(airlineId, V122Catalog.QuoteProvider) : M1Commands.LocalSupplier(airlineId)));
    }

    private Task<BackofficeServiceDefinitionDto> ProductAsync(
        string code,
        int airlineId,
        long supplierId,
        Func<TestDefineServiceDefinitionCommand, TestDefineServiceDefinitionCommand>? change = null)
    {
        var command = V122Catalog.Define(V122Catalog.Case(code), airlineId, supplierId);

        return _harness.Proof.DefinitionAsync(change?.Invoke(command) ?? command);
    }

    private async Task<string> SourceRowsAsync(int airlineId)
        => (await _harness.RowsAsync($"""
            SELECT CONCAT(
                (SELECT COUNT(*) FROM Ancillary.FlightCountInventories WHERE OwnerAirlineId = {airlineId}), '|',
                (SELECT COUNT(*) FROM Ancillary.FlightWeightInventories WHERE OwnerAirlineId = {airlineId}), '|',
                (SELECT COUNT(*) FROM Ancillary.AirportSlotInventories WHERE OwnerAirlineId = {airlineId})) AS Value
            """)).Single();

    private async Task<long> PolicyAsync(InventoryFixture fixture, TestDefineInventoryPolicyCommand command)
        => (await _harness.RequestAsync(fixture, scope => scope.DefineInventoryPolicy.DefineAsync(command))).Id;

    private Task ActivateAsync(InventoryFixture fixture, long policyId)
        => _harness.RequestAsync(fixture, scope => scope.ActivateInventoryPolicy.ActivateAsync(new TestInventoryPolicyLifecycleCommand(policyId, 1)));

    [Fact]
    public async Task J1_CT10_an_asymmetric_trip_files_each_direction_as_its_own_rule_with_its_own_cap_and_price_and_no_baggage_stock()
    {
        var (airlineId, supplierId) = await OperatorAsync();
        var bag = await ProductAsync("A01", airlineId, supplierId);
        var heavy = await ProductAsync("A03", airlineId, supplierId);
        var oversize = await ProductAsync("A04", airlineId, supplierId);
        var outbound = await _harness.Proof.RuleAsync(Rule("A01", bag.Id) with { Geography = new(AllowedRoutePairs: [Pair(Thr, Ist)]) }, Euro(120m));
        var inbound = await _harness.Proof.RuleAsync(
            Rule("A01", bag.Id, 20) with { Geography = new(AllowedRoutePairs: [Pair(Ist, Thr)]), Quantity = new(AncillaryQuantityUnit.Piece, 1, 2) },
            Euro(95m));
        var heavyInbound = await _harness.Proof.RuleAsync(Rule("A03", heavy.Id) with { Geography = new(AllowedRoutePairs: [Pair(Ist, Thr)]) }, Euro(60m));
        var oversizeOutbound = await _harness.Proof.RuleAsync(Unavailable(Rule("A04", oversize.Id)) with { Geography = new(AllowedRoutePairs: [Pair(Thr, Ist)]) });

        Assert.Equal(
            new[] { (10, "Active", 1, 6, Thr, Ist, 120m), (20, "Active", 1, 2, Ist, Thr, 95m) },
            new[] { outbound, inbound }.Select(rule => (
                rule.Provision.Sequence,
                rule.Provision.Status.Name,
                rule.Provision.MinQuantity,
                rule.Provision.MaxQuantity,
                rule.Provision.Geography!.AllowedRoutePairs.Single().OriginAirportId,
                rule.Provision.Geography.AllowedRoutePairs.Single().DestinationAirportId,
                rule.Pricing!.Rates.Single().UnitTotal.Amount)));
        Assert.Equal(("Active", "Paid", "Overweight", 60m), (heavyInbound.Provision.Status.Name, heavyInbound.Provision.Disposition.Name, heavy.Specification!.Baggage!.ChargeKind.Name, heavyInbound.Pricing!.Rates.Single().UnitTotal.Amount));
        Assert.Equal(("Active", "NotAvailable", "NotAvailable"), (oversizeOutbound.Provision.Status.Name, oversizeOutbound.Provision.Disposition.Name, oversizeOutbound.Provision.PriceOrigin.Name));
        Assert.Null(oversizeOutbound.Pricing);
        Assert.Equal(("QuantityChoice", true), (bag.SelectionContract!.Kind.Name, bag.SelectionContract.ZeroQuantityMeansNoSelection));
        Assert.Equal("0|0|0", await SourceRowsAsync(airlineId));

        var unconfigured = await _harness.SnapshotAsync(new InventoryFixture(airlineId), bag.ServiceDefinitionRef, Flight, At);

        Assert.Equal(("NotConfigured", false), (unconfigured.State.Name, unconfigured.IsGuaranteed));
    }

    [Fact]
    public async Task J2_CT11_five_and_ten_kilogram_packages_share_one_kilogram_family_cap_and_a_piece_counter_cannot_join_it()
    {
        var (airlineId, supplierId) = await OperatorAsync();
        var fixture = InventoryFixture.Connected(airlineId);
        var ten = await ProductAsync("A02", airlineId, supplierId);
        var five = await ProductAsync("A02", airlineId, supplierId, command => command with
        {
            ServiceDefinitionRef = "WEIGHT_PACK_5",
            ServiceSubCode = "XW5",
            Document = command.Document with { Rfisc = "XW5" },
            TypedSpecification = command.TypedSpecification! with { Baggage = command.TypedSpecification.Baggage! with { PackageWeightKg = 5m } }
        });
        var piece = await ProductAsync("A01", airlineId, supplierId);
        var tenRule = await _harness.Proof.RuleAsync(Rule("A02", ten.Id), Euro(50m));
        var fiveRule = await _harness.Proof.RuleAsync(
            Rule("A02", five.Id) with { BaggageApplication = Baggage(5m, chargeKind: BaggageChargeKind.WeightPackage, allowanceConcept: BaggageAllowanceConcept.Weight) },
            Euro(30m));

        fixture.CountingFamilies!.Add(WeightFamily);

        var tenPolicy = await PolicyAsync(fixture, new TestDefineInventoryPolicyCommand(
            airlineId, ten.ServiceDefinitionRef, ten.Id, InventoryAuthority.Unlimited,
            PassengerUsageLimits: [new(PassengerUsageLimitScope.PerPortion, 20, WeightFamily, UsageConsumptionUnit.Kilogram, 10m)]));
        var fivePolicy = await PolicyAsync(fixture, new TestDefineInventoryPolicyCommand(
            airlineId, five.ServiceDefinitionRef, five.Id, InventoryAuthority.Unlimited,
            PassengerUsageLimits: [new(PassengerUsageLimitScope.PerPortion, 20, WeightFamily, UsageConsumptionUnit.Kilogram, 5m)]));
        var piecePolicy = await PolicyAsync(fixture, new TestDefineInventoryPolicyCommand(
            airlineId, piece.ServiceDefinitionRef, piece.Id, InventoryAuthority.Unlimited,
            PassengerUsageLimits: [new(PassengerUsageLimitScope.PerPortion, 2, WeightFamily)]));

        await ActivateAsync(fixture, tenPolicy);
        await ActivateAsync(fixture, fivePolicy);
        await _harness.RefusedAsync(16606, 409, fixture, scope => scope.ActivateInventoryPolicy.ActivateAsync(new TestInventoryPolicyLifecycleCommand(piecePolicy, 1)));

        var limits = new[]
        {
            (await _harness.RequestAsync(fixture, scope => scope.GetInventoryPolicyById.ExecuteAsync(tenPolicy))).PassengerUsageLimits.Single(),
            (await _harness.RequestAsync(fixture, scope => scope.GetInventoryPolicyById.ExecuteAsync(fivePolicy))).PassengerUsageLimits.Single()
        };
        var snapshot = await _harness.SnapshotAsync(fixture, ten.ServiceDefinitionRef, Flight, At);

        Assert.Equal(
            new[] { ("PerPortion", 20, WeightFamily, "Kilogram", (decimal?)10m), ("PerPortion", 20, WeightFamily, "Kilogram", (decimal?)5m) },
            limits.Select(limit => (limit.LimitScope.Name, limit.MaxUnits, limit.CountingFamilyCode, limit.ConsumptionUnit.Name, limit.UnitsPerPurchase)));
        Assert.Equal((10m, 5m), (ten.Specification!.Baggage!.PackageWeightKg!.Value, five.Specification!.Baggage!.PackageWeightKg!.Value));
        Assert.Equal(
            new[] { ("PerItem", 50m, 2), ("PerItem", 30m, 2) },
            new[] { tenRule, fiveRule }.Select(rule => (rule.Pricing!.PricingUnit!.Name, rule.Pricing.Rates.Single().UnitTotal.Amount, rule.Provision.MaxQuantity)));
        Assert.Equal(("Unlimited", false), (snapshot.State.Name, snapshot.IsGuaranteed));
        Assert.Null(snapshot.ConfiguredKg);
        Assert.Equal("0|0|0", await SourceRowsAsync(airlineId));
        Assert.Equal("Draft", (await _harness.RequestAsync(fixture, scope => scope.GetInventoryPolicyById.ExecuteAsync(piecePolicy))).Status.Name);
    }

    [Fact]
    public async Task J3_CT13_CT16_a_pet_is_refused_outbound_sold_on_the_return_and_a_delegated_supplier_is_never_a_guarantee()
    {
        var (airlineId, supplierId) = await OperatorAsync(external: true);
        var fixture = new InventoryFixture(airlineId);
        var pet = await ProductAsync("A13", airlineId, supplierId);
        var outbound = await _harness.Proof.RuleAsync(Unavailable(Rule("A13", pet.Id)) with { Geography = new(AllowedRoutePairs: [Pair(Thr, Ist)]) });
        var inbound = await _harness.Proof.RuleAsync(Rule("A13", pet.Id, 20) with { Geography = new(AllowedRoutePairs: [Pair(Ist, Thr)]) }, Euro(70m));
        var widened = await RequestAsync(scope => scope.DefineProvision.DefineAsync(Unavailable(Rule("A13", pet.Id, 30)) with
        {
            PetRule = new ProvisionPetRuleInput(null, null, 10m, ConfirmationRequirement.SubjectToConfirmation)
        }));
        var policy = await PolicyAsync(fixture, new TestDefineInventoryPolicyCommand(airlineId, pet.ServiceDefinitionRef, pet.Id, InventoryAuthority.Supplier, ProviderKey: V122Catalog.QuoteProvider));

        await RefusedAsync(16321, 409, scope => scope.ActivateProvision.ActivateAsync(new TestActivateProvisionCommand(widened.Id)));
        await ActivateAsync(fixture, policy);

        var snapshot = await _harness.SnapshotAsync(fixture, pet.ServiceDefinitionRef, Flight, At);
        var specification = SpecText.Of(pet.Specification);

        Assert.Equal(("NotAvailable", "Paid", 70m), (outbound.Provision.Disposition.Name, inbound.Provision.Disposition.Name, inbound.Pricing!.Rates.Single().UnitTotal.Amount));
        Assert.Equal((6m, 16, "SubjectToConfirmation"), (inbound.Provision.PetRule!.MaxCombinedKgOverride!.Value, inbound.Provision.PetRule.MinAnimalAgeWeeksOverride!.Value, inbound.Provision.PetRule.AcceptanceMode.Name));
        Assert.Equal(("TypedForm", "SubjectToConfirmation"), (pet.SelectionContract!.Kind.Name, pet.BookingConfirmationRequirement.Name));
        Assert.Equal(
            new[] { "AnimalType", "CombinedWeightKg", "CarrierDimensions", "DocumentAcknowledgements", "TravellerRef", "BoundRef" },
            pet.SelectionContract.Fields.Select(field => field.Name));
        Assert.DoesNotContain("\"CombinedWeightKg\"", specification, StringComparison.Ordinal);
        Assert.Contains("\"MaxCombinedWeightKg\":\"8\"", specification, StringComparison.Ordinal);
        Assert.Equal(("DelegatedCheckRequired", "Supplier", false), (snapshot.State.Name, snapshot.Authority!.Name, snapshot.IsGuaranteed));
        Assert.Null(snapshot.ConfiguredCount);
        Assert.Equal("0|0|0", await SourceRowsAsync(airlineId));
    }

    [Fact]
    public async Task J4_CT09_CT14_CT15_a_free_wheelchair_has_no_price_and_a_paid_minor_service_is_one_unit_per_portion_without_guardian_data()
    {
        var (airlineId, supplierId) = await OperatorAsync();
        var fixture = new InventoryFixture(airlineId);
        var wheelchair = await ProductAsync("A15", airlineId, supplierId);
        var minor = await ProductAsync("A19", airlineId, supplierId);
        var assisted = await _harness.Proof.RuleAsync(Rule("A15", wheelchair.Id));
        var escorted = await _harness.Proof.RuleAsync(Rule("A19", minor.Id), Euro(80m));
        var policy = await PolicyAsync(fixture, new TestDefineInventoryPolicyCommand(airlineId, wheelchair.ServiceDefinitionRef, wheelchair.Id, InventoryAuthority.Unlimited));

        await RefusedAsync(16505, 409, scope => scope.DefinePricing.DefineAsync(Euro(1m)(assisted.Provision.Id)));
        await RefusedAsync(16321, 409, async scope =>
        {
            var paid = await scope.DefineProvision.DefineAsync(Rule("A15", wheelchair.Id, 30) with { Outcome = new(CommercialDisposition.Paid, false, false), Origin = PriceOrigin.Filed });
            var price = await scope.DefinePricing.DefineAsync(Euro(10m)(paid.Id));

            await scope.PublishProvision.PublishAsync(new TestPublishProvisionCommand(paid.Id, price.Id));
        });
        await ActivateAsync(fixture, policy);

        var snapshot = await _harness.SnapshotAsync(fixture, wheelchair.ServiceDefinitionRef, Flight, At);
        var specification = SpecText.Of(minor.Specification);

        Assert.Equal(("Active", "Free", "Free", true), (assisted.Provision.Status.Name, assisted.Provision.Disposition.Name, assisted.Provision.PriceOrigin.Name, assisted.Provision.MustCheckAvailability));
        Assert.Null(assisted.Pricing);
        Assert.Equal(("NoAncillaryDocument", "None", "WCHR"), (wheelchair.DocumentRouting!.Name, wheelchair.DocumentType.Name, wheelchair.BookingSsrCode));
        Assert.Equal(("Unlimited", false, true), (snapshot.State.Name, snapshot.IsGuaranteed, snapshot.RequiresAvailabilityCheck));
        Assert.Equal(("Portion", 1, 1, 80m), (escorted.Provision.CoverageScope.Name, escorted.Provision.MinQuantity, escorted.Provision.MaxQuantity, Assert.Single(escorted.Pricing!.Rates).UnitTotal.Amount));
        Assert.Equal(("DirectOnly", 1440), (escorted.Provision.AssistedTravelRule!.ConnectionPolicy!.Name, escorted.Provision.AssistedTravelRule.MinimumLeadTimeMinutes!.Value));
        Assert.Equal(
            new[] { "GuardianHandoffContact:Text:True", "GuardianPickupContact:Text:True", "ChildRef:Reference:True", "BoundRef:Reference:True" },
            minor.SelectionContract!.Fields.Select(field => $"{field.Name}:{field.Type.Name}:{field.Required}"));
        Assert.DoesNotContain("Handoff", specification, StringComparison.Ordinal);
        Assert.DoesNotContain("Pickup", specification, StringComparison.Ordinal);
        Assert.Contains("\"GuardianContactRequired\":true", specification, StringComparison.Ordinal);
    }

    [Fact]
    public async Task J5_CT19_seats_and_an_extra_seat_delegate_their_availability_to_flightflow_and_never_take_a_local_count()
    {
        var (airlineId, supplierId) = await OperatorAsync();
        var fixture = InventoryFixture.Connected(airlineId);
        var standard = await ProductAsync("A07", airlineId, supplierId);
        var preferred = await ProductAsync("A08", airlineId, supplierId);
        var extra = await ProductAsync("A09", airlineId, supplierId);
        var extraRule = await _harness.Proof.RuleAsync(Rule("A09", extra.Id), Euro(150m));

        fixture.FlightFlowProviderKeys!.Add(FlightFlowKey);
        await _harness.RefusedAsync(16602, 422, fixture, scope => scope.DefineInventoryPolicy.DefineAsync(new TestDefineInventoryPolicyCommand(
            airlineId, standard.ServiceDefinitionRef, standard.Id, InventoryAuthority.FlightFlow, LocalInventoryPattern.FlightCount, FlightFlowKey, Count())));

        foreach (var seat in new[] { standard, preferred, extra })
        {
            await ActivateAsync(fixture, await PolicyAsync(
                fixture,
                new TestDefineInventoryPolicyCommand(airlineId, seat.ServiceDefinitionRef, seat.Id, InventoryAuthority.FlightFlow, ProviderKey: FlightFlowKey)));

            var snapshot = await _harness.SnapshotAsync(fixture, seat.ServiceDefinitionRef, Flight, At);

            Assert.Equal(("DelegatedCheckRequired", "FlightFlow", false), (snapshot.State.Name, snapshot.Authority!.Name, snapshot.IsGuaranteed));
            Assert.Null(snapshot.ConfiguredCount);
            Assert.Equal("SeatMapSelection", seat.SelectionContract!.Kind.Name);
        }

        Assert.Equal(
            ("TicketOrExchange", "None", true, true, 1),
            (extra.DocumentRouting!.Name, extra.DocumentType.Name, extra.Specification!.Seat!.RequiresExternalTicketAction, extra.Specification.Seat.RequiresAdjacentSeat, extra.Specification.Seat.ExtraOccupiedSeatCount!.Value));
        Assert.Equal(("Active", "Filed", false, 150m), (extraRule.Provision.Status.Name, extraRule.Provision.PriceOrigin.Name, extraRule.Provision.DocumentRequired, extraRule.Pricing!.Rates.Single().UnitTotal.Amount));
        Assert.True(preferred.Specification!.Seat!.RequiresExitRowEligibility);
        Assert.Equal("0|0|0", await SourceRowsAsync(airlineId));
    }

    [Fact]
    public async Task CT37_only_an_on_board_connectivity_plan_is_sold_at_the_on_board_stage()
    {
        var (airlineId, supplierId) = await OperatorAsync();
        var onBoard = await ProductAsync("A24", airlineId, supplierId, command => command with
        {
            TypedSpecification = command.TypedSpecification! with { Connectivity = command.TypedSpecification.Connectivity! with { DeliveryStage = PurchaseStage.OnBoard } }
        });
        var priority = await ProductAsync("A23", airlineId, supplierId);
        var sold = await _harness.Proof.RuleAsync(Rule("A24", onBoard.Id) with { PurchaseStage = PurchaseStage.OnBoard }, Euro(9m));
        var preflight = await RequestAsync(scope => scope.DefineProvision.DefineAsync(Unavailable(Rule("A24", onBoard.Id, 20))));
        var misplaced = await RequestAsync(scope => scope.DefineProvision.DefineAsync(Unavailable(Rule("A23", priority.Id)) with { PurchaseStage = PurchaseStage.OnBoard }));

        await RefusedAsync(16323, 409, scope => scope.ActivateProvision.ActivateAsync(new TestActivateProvisionCommand(preflight.Id)));
        await RefusedAsync(16323, 409, scope => scope.ActivateProvision.ActivateAsync(new TestActivateProvisionCommand(misplaced.Id)));

        Assert.Equal(("Active", "OnBoard", 5), (sold.Provision.Status.Name, sold.Provision.PurchaseStage.Name, sold.Provision.PurchaseStage.Value));
        Assert.Equal("OnBoard", onBoard.Specification!.Connectivity!.DeliveryStage.Name);
    }

    [Fact]
    public async Task CT33_two_airlines_author_the_same_reference_with_their_own_specification_and_never_read_each_other()
    {
        var (firstAirline, firstSupplier) = await OperatorAsync(external: true);
        var (secondAirline, secondSupplier) = await OperatorAsync(external: true);
        var first = await ProductAsync("A13", firstAirline, firstSupplier);
        var second = await ProductAsync("A13", secondAirline, secondSupplier, command => command with
        {
            TypedSpecification = command.TypedSpecification! with { Pet = command.TypedSpecification.Pet! with { MaxCombinedWeightKg = 6m, RequiredDocumentCodes = ["RABIES_CERT"] } }
        });

        async Task<string[]> ListedAsync(int airlineId)
            => (await RequestAsync(scope => scope.GetServiceDefinitionsPaginated.ExecuteAsync(new BackofficeGetAncillaryServiceDefinitionsPaginatedQuery { OwnerAirlineId = airlineId })))
                .Results.Select(row => $"{row.Id}:{row.VariantCode}").ToArray();

        Assert.Equal((8m, 6m), (first.Specification!.Pet!.MaxCombinedWeightKg, second.Specification!.Pet!.MaxCombinedWeightKg));
        Assert.Equal(new[] { "ENTRY_RULES_ACK" }, first.Specification.Pet.RequiredDocumentCodes);
        Assert.Equal(new[] { "RABIES_CERT" }, second.Specification.Pet.RequiredDocumentCodes);
        Assert.Equal(new[] { $"{first.Id}:A13" }, await ListedAsync(firstAirline));
        Assert.Equal(new[] { $"{second.Id}:A13" }, await ListedAsync(secondAirline));
        Assert.Equal(
            new[] { $"{first.Id} 1", $"{second.Id} 1" },
            await _harness.RowsAsync($"SELECT CONCAT(AncillaryServiceDefinitionId, ' ', COUNT(*)) AS Value FROM Ancillary.PetSpecificationDocumentCodes WHERE AncillaryServiceDefinitionId IN ({first.Id}, {second.Id}) GROUP BY AncillaryServiceDefinitionId ORDER BY 1"));
    }
}
