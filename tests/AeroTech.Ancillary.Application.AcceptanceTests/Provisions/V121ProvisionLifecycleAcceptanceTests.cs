using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.ChangeAncillaryPricing.Backoffice;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing.Backoffice;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.SwitchActiveAncillaryPricing.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionBlackoutPeriod.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionDayTimeWindow.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionPermittedTravelPeriod.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeAncillaryProvision.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionDayTimeApplication.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionDayTimeWindow.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionGeography.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionPermittedTravelPeriod.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionTravelDate.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.PublishAncillaryProvision.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RemoveProvisionBlackoutPeriod.Backoffice;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.AssignAncillaryServiceDefinitionPricingUnit.Backoffice;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.AssignAncillaryServiceDefinitionServiceDateBasis.Backoffice;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetAncillaryProvisionsPaginated.Backoffice;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;
using AeroTech.Messages.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.P1Commands;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.V121Commands;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.V12Commands;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Provisions;

[Collection(DatabaseCollection.Name)]
public class V121ProvisionLifecycleAcceptanceTests
{
    private readonly TestDatabase _database;
    private readonly FixedClock _clock = new();
    private readonly FamilyProof _proof;

    public V121ProvisionLifecycleAcceptanceTests(TestDatabase database)
    {
        _database = database;
        _proof = new FamilyProof(database, _clock);
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

    private async Task<long> DefinitionAsync()
    {
        var airlineId = _database.NextAirlineId();
        var supplierId = await _proof.SupplierAsync(M1Commands.LocalSupplier(airlineId));

        return (await _proof.DefinitionAsync(FirstExcessBagDefinition(airlineId, supplierId))).Id;
    }

    private async Task<long> DefinitionAsync(PricingUnit pricingUnit, ServiceDateBasis serviceDateBasis, string reference = "SERVICE")
    {
        var airlineId = _database.NextAirlineId();
        var supplierId = await _proof.SupplierAsync(M1Commands.LocalSupplier(airlineId));

        return (await _proof.DefinitionAsync(
            CarrierDefinition(airlineId, supplierId, reference, "SVC", "F", "TS", "Service", pricingUnit: pricingUnit, serviceDateBasis: serviceDateBasis))).Id;
    }

    private static TestDefineProvisionCommand FullyRestricted(long definitionId)
        => Provision(
            definitionId,
            10,
            coverageScope: ServiceCoverageScope.Journey,
            quantityUnit: AncillaryQuantityUnit.Piece,
            maxQuantity: 3,
            applicationType: ProvisionApplicationType.Baggage) with
        {
            PassengerEligibility = Passengers(PassengerTypeCode.ADT, PassengerTypeCode.CHD),
            SalesRestrictions = new(
                new DateTimeOffset(2026, 11, 1, 0, 0, 0, TimeSpan.FromMinutes(210)),
                new DateTimeOffset(2026, 12, 1, 0, 0, 0, TimeSpan.FromMinutes(210)),
                [501],
                [9001],
                [CustomerType.TravelAgency, CustomerType.Organization]),
            Geography = new([Thr], [Ist], [Mhd], [Pair(Thr, Ist), Pair(Mhd, Ist, RoutePairDirection.BothDirections)]),
            FlightApplication = new([1, 2], [3], [" w5112", "W5116"], [81234, 81240], [1, 2]),
            FareApplication = new([7001], [AirFareType.Public, AirFareType.Private], [5, 6], ["y26lt"], [2], [41, 42]),
            TravelDate = Dates([Period(Day(2026, 12, 20), Day(2026, 12, 31))], [Period(Day(2026, 12, 31), Day(2026, 12, 31))]),
            DayTimeApplication = DayTime(Window(Monday, 8, 12), Window(Monday, 9, 10, DayTimeRestrictionEffect.Deny)),
            AdvancePurchase = new(3, TimeUnit.Days),
            BaggageApplication = Baggage(
                23.50m,
                1,
                2,
                BaggageTravelApplication.MostSignificantSector,
                BaggagePurchaseApplication.PrepaidAndCheckIn,
                BaggageRuleDeference.OperatingCarrier,
                0,
                WeightUnit.Lbs)
        };

    private static PricingLineInput[] FullPrice() =>
    [
        Base(30m, name: "Extra bag"),
        Tax("VAT", 2.7m, countryId: 98, stationAirportId: Thr, name: "Value added tax"),
        Fee("HDL", 1.05m)
    ];

    [Fact]
    public async Task V121_M08_a_draft_edit_is_refused_when_it_is_malformed_or_the_provision_does_not_exist()
    {
        var definitionId = await DefinitionAsync(PricingUnit.PerPassenger, ServiceDateBasis.FlightDeparture);
        var command = Provision(definitionId, 10) with { PassengerEligibility = Passengers(PassengerTypeCode.ADT) };
        var draft = await RequestAsync(scope => scope.DefineProvision.DefineAsync(command));

        await RefusedAsync(16302, 422, scope => scope.ChangeProvision.ChangeAsync(Change(draft.Id, command with { Sequence = 0 })));
        await RefusedAsync(16302, 422, scope => scope.ChangeProvision.ChangeAsync(Change(draft.Id, command with { ApplicationType = ProvisionApplicationType.Seat })));
        await RefusedAsync(16302, 422, scope => scope.ChangeProvision.ChangeAsync(Change(draft.Id, command with { ApplicationType = ProvisionApplicationType.Seat, SeatApplication = Seat(["12A"], null) })));
        await RefusedAsync(16302, 422, scope => scope.ChangeProvision.ChangeAsync(Change(draft.Id, command with { BaggageApplication = Baggage(23m) })));
        await RefusedAsync(16302, 422, scope => scope.ChangeProvision.ChangeAsync(Change(draft.Id, command with { Geography = new(AllowedRoutePairs: [Pair(Thr, Thr)]) })));
        await RefusedAsync(16302, 422, scope => scope.ChangeProvision.ChangeAsync(
            Change(draft.Id, command with { Geography = new(AllowedRoutePairs: [Pair(Thr, Ist), Pair(Ist, Thr, RoutePairDirection.BothDirections)]) })));
        await RefusedAsync(16302, 422, scope => scope.ChangeProvision.ChangeAsync(
            Change(draft.Id, command with { PassengerEligibility = Passengers(PassengerTypeCode.ADT, PassengerTypeCode.ADT) })));
        await RefusedAsync(16302, 422, scope => scope.ChangeProvision.ChangeAsync(
            Change(draft.Id, command with { TravelDate = Dates([Period(Day(2027, 4, 30), Day(2027, 4, 1))]) })));
        await RefusedAsync(16302, 422, scope => scope.ChangeProvision.ChangeAsync(
            Change(draft.Id, command with { DayTimeApplication = DayTime(Window(0, 8, 12)) })));
        await RefusedAsync(16301, 404, scope => scope.ChangeProvision.ChangeAsync(Change(999_999_999, command)));
        await RefusedAsync(16304, 422, scope => scope.DefineProvision.DefineAsync(command with { ServiceDefinitionId = 999_999_999 }));

        var detail = await RequestAsync(scope => scope.GetProvisionById.ExecuteAsync(draft.Id));

        Assert.Equal(("Draft", 10, "Standard"), (detail.Status.Name, detail.Sequence, detail.ApplicationType.Name));
        Assert.Equal(new[] { "ADT" }, detail.PassengerEligibility!.AllowedPassengerTypes.Select(row => row.Value.Name));
        Assert.Null(detail.Geography);
        Assert.Null(detail.TravelDate);
    }

    [Fact]
    public async Task V121_M10_the_lifecycle_moves_a_paid_provision_with_its_rule_groups_and_price_through_the_existing_states()
    {
        var start = _clock.Now;
        var definitionId = await DefinitionAsync();
        var draft = await RequestAsync(scope => scope.DefineProvision.DefineAsync(FullyRestricted(definitionId)));
        var pricing = await RequestAsync(scope => scope.DefinePricing.DefineAsync(Pricing(draft.Id, Eur, FullPrice())));
        var command = new TestProvisionLifecycleCommand(draft.Id);

        await RefusedAsync(16303, 409, scope => scope.SuspendProvision.SuspendAsync(command));
        await RefusedAsync(16303, 409, scope => scope.ReactivateProvision.ReactivateAsync(command));

        async Task<(string Status, DateTimeOffset? ActivatedAt, DateTimeOffset? SuspendedAt, DateTimeOffset? RetiredAt)> ReadAsync()
        {
            await using var reader = new AncillaryScope(_database, _clock);
            var detail = await reader.GetProvisionById.ExecuteAsync(draft.Id);
            var row = await reader.Query.AncillaryProvisions.AsNoTracking().SingleAsync(provision => provision.Id == draft.Id);
            var price = await reader.GetPricingById.ExecuteAsync(pricing.Id);

            Assert.Equal(detail.Status.Name, row.Status.ToString());
            Assert.Equal(detail.Status.Name, (await reader.Provisions.GetAsync(draft.Id))!.Status.ToString());
            Assert.Equal(
                (2, 1, 2, 1, 1, 2, 2, 2),
                (detail.PassengerEligibility!.AllowedPassengerTypes.Count, detail.SalesRestrictions!.AllowedPointsOfSale.Count, detail.Geography!.AllowedRoutePairs.Count,
                    detail.TravelDate!.PermittedPeriods.Count, detail.TravelDate.BlackoutPeriods.Count, detail.DayTimeApplication!.Windows.Count,
                    detail.FlightApplication!.AllowedFlights.Count, detail.FareApplication!.AllowedRbds.Count));
            Assert.Equal((23.5m, "Lbs", "PrepaidAndCheckIn"), (detail.BaggageApplication!.Weight, detail.BaggageApplication.WeightUnit.Name, detail.BaggageApplication.PurchaseApplication.Name));
            Assert.Equal((3, "Days", false), (detail.AdvancePurchase!.MinimumPeriod, detail.AdvancePurchase.Unit.Name, detail.AdvancePurchase.SameTimeAsTicketed));
            Assert.Equal((3, 30m, 2.7m, 1.05m, 33.75m), (price.PriceLines.Count, price.Rates.Single().BaseAmount, price.Rates.Single().TaxAmount, price.Rates.Single().FeeAmount, price.Rates.Single().TotalAmount));

            return (detail.Status.Name, detail.ActivatedAt, detail.SuspendedAt, detail.RetiredAt);
        }

        async Task<ProvisionStatus> ActAsync(Func<AncillaryScope, Task<ProvisionResult>> act)
        {
            _clock.Now = _clock.Now.AddHours(1);
            await using var writer = new AncillaryScope(_database, _clock);

            return (await act(writer)).Status;
        }

        Assert.Equal(("Draft", null, null, null), await ReadAsync());

        Assert.Equal(ProvisionStatus.Active, await ActAsync(writer => writer.PublishProvision.PublishAsync(new TestPublishProvisionCommand(draft.Id, pricing.Id))));
        Assert.Equal(("Active", start.AddHours(1), null, null), await ReadAsync());

        Assert.Equal(ProvisionStatus.Suspended, await ActAsync(writer => writer.SuspendProvision.SuspendAsync(command)));
        Assert.Equal(("Suspended", start.AddHours(1), start.AddHours(2), null), await ReadAsync());

        Assert.Equal(ProvisionStatus.Active, await ActAsync(writer => writer.ReactivateProvision.ReactivateAsync(command)));
        Assert.Equal(("Active", start.AddHours(1), null, null), await ReadAsync());

        Assert.Equal(ProvisionStatus.Retired, await ActAsync(writer => writer.RetireProvision.RetireAsync(command)));
        Assert.Equal(("Retired", start.AddHours(1), null, start.AddHours(4)), await ReadAsync());

        await RefusedAsync(16303, 409, scope => scope.RetireProvision.RetireAsync(command));
        await RefusedAsync(16303, 409, scope => scope.ReactivateProvision.ReactivateAsync(command));
        await RefusedAsync(16303, 409, scope => scope.ActivateProvision.ActivateAsync(new TestActivateProvisionCommand(draft.Id)));
        await RefusedAsync(16303, 409, scope => scope.PublishProvision.PublishAsync(new TestPublishProvisionCommand(draft.Id, pricing.Id)));
        await RefusedAsync(16301, 404, scope => scope.SuspendProvision.SuspendAsync(new TestProvisionLifecycleCommand(999_999_999)));
        await RefusedAsync(16505, 409, scope => scope.DefinePricing.DefineAsync(Pricing(draft.Id, Eur, Base(31m))));

        Assert.Equal(PricingStatus.Active, (await RequestAsync(scope => scope.Pricings.GetAsync(pricing.Id)))!.Status);
        Assert.Equal(
            PricingStatus.Retired,
            (await RequestAsync(scope => scope.RetirePricing.RetireAsync(new TestPricingLifecycleCommand(pricing.Id)))).Status);
    }

    [Fact]
    public async Task V121_E17_a_suspended_provision_cannot_be_reactivated_while_its_sequence_has_another_active_row()
    {
        var definitionId = await DefinitionAsync(PricingUnit.PerPassenger, ServiceDateBasis.FlightDeparture);
        var first = await RequestAsync(scope => scope.DefineProvision.DefineAsync(Provision(definitionId, 10, CommercialDisposition.Free)));

        await RequestAsync(scope => scope.ActivateProvision.ActivateAsync(new TestActivateProvisionCommand(first.Id)));
        await RequestAsync(scope => scope.SuspendProvision.SuspendAsync(new TestProvisionLifecycleCommand(first.Id)));

        var replacement = await RequestAsync(scope => scope.DefineProvision.DefineAsync(Provision(definitionId, 10, CommercialDisposition.Free)));

        await RequestAsync(scope => scope.ActivateProvision.ActivateAsync(new TestActivateProvisionCommand(replacement.Id)));
        await RefusedAsync(16306, 409, scope => scope.ReactivateProvision.ReactivateAsync(new TestProvisionLifecycleCommand(first.Id)));

        Assert.Equal("Suspended", (await RequestAsync(scope => scope.GetProvisionById.ExecuteAsync(first.Id))).Status.Name);
        Assert.Equal("Active", (await RequestAsync(scope => scope.GetProvisionById.ExecuteAsync(replacement.Id))).Status.Name);

        await RequestAsync(scope => scope.RetireProvision.RetireAsync(new TestProvisionLifecycleCommand(replacement.Id)));

        Assert.Equal(
            ProvisionStatus.Active,
            (await RequestAsync(scope => scope.ReactivateProvision.ReactivateAsync(new TestProvisionLifecycleCommand(first.Id)))).Status);
    }

    [Fact]
    public async Task V121_M10_provisions_of_a_definition_are_filtered_by_status_coverage_sequence_and_sales_date_with_a_rule_summary()
    {
        var definitionId = await DefinitionAsync(PricingUnit.PerPassenger, ServiceDateBasis.FlightDeparture);
        var sector = await RequestAsync(scope => scope.DefineProvision.DefineAsync(
            Provision(definitionId, 10, CommercialDisposition.Free) with
            {
                TravelDate = Dates(
                    [Period(Day(2027, 1, 1), Day(2027, 1, 31)), Period(Day(2027, 3, 1), Day(2027, 3, 31)), Period(Day(2027, 5, 1), Day(2027, 5, 1))],
                    [Period(Day(2027, 1, 10), Day(2027, 1, 10))]),
                DayTimeApplication = DayTime(Window(Monday, 8, 12), Window(Tuesday))
            }));
        var journey = await RequestAsync(scope => scope.DefineProvision.DefineAsync(
            Provision(definitionId, 20, CommercialDisposition.Free, ServiceCoverageScope.Journey) with
            {
                SalesRestrictions = new(_clock.Now.AddDays(10), _clock.Now.AddDays(20), [V122Catalog.PointOfSale]),
                TravelDate = Dates(blackout: [Period(Day(2027, 3, 21), Day(2027, 3, 21))])
            }));
        var paid = await RequestAsync(scope => scope.DefineProvision.DefineAsync(Provision(definitionId, 30)));

        await RequestAsync(scope => scope.ActivateProvision.ActivateAsync(new TestActivateProvisionCommand(sector.Id)));
        await RequestAsync(scope => scope.ActivateProvision.ActivateAsync(new TestActivateProvisionCommand(journey.Id)));
        await RequestAsync(scope => scope.SuspendProvision.SuspendAsync(new TestProvisionLifecycleCommand(journey.Id)));

        async Task<string[]> IdsAsync(BackofficeGetAncillaryProvisionsPaginatedQuery query)
        {
            query.ServiceDefinitionId = definitionId;

            return (await RequestAsync(scope => scope.GetProvisionsPaginated.ExecuteAsync(query))).Results.Select(row => row.Id).ToArray();
        }

        Assert.Equal(new[] { sector.Id.ToString(), journey.Id.ToString(), paid.Id.ToString() }, await IdsAsync(new()));
        Assert.Equal(new[] { sector.Id.ToString() }, await IdsAsync(new() { Status = ProvisionStatus.Active }));
        Assert.Equal(new[] { journey.Id.ToString() }, await IdsAsync(new() { Status = ProvisionStatus.Suspended }));
        Assert.Equal(new[] { paid.Id.ToString() }, await IdsAsync(new() { Status = ProvisionStatus.Draft }));
        Assert.Equal(new[] { journey.Id.ToString() }, await IdsAsync(new() { CoverageScope = ServiceCoverageScope.Journey }));
        Assert.Equal(new[] { sector.Id.ToString(), paid.Id.ToString() }, await IdsAsync(new() { CoverageScope = ServiceCoverageScope.Sector }));
        Assert.Equal(new[] { journey.Id.ToString() }, await IdsAsync(new() { Sequence = 20 }));
        Assert.Equal(new[] { sector.Id.ToString(), paid.Id.ToString() }, await IdsAsync(new() { SalesDate = _clock.Now }));
        Assert.Equal(3, (await IdsAsync(new() { SalesDate = _clock.Now.AddDays(15) })).Length);
        Assert.Equal(2, (await IdsAsync(new() { PageSize = 2 })).Length);

        var rows = (await RequestAsync(scope => scope.GetProvisionsPaginated.ExecuteAsync(
            new BackofficeGetAncillaryProvisionsPaginatedQuery { ServiceDefinitionId = definitionId }))).Results.ToList();

        Assert.Equal(new[] { "Free", "Free", "Paid" }, rows.Select(row => row.Disposition.Name));
        Assert.Equal(
            new[] { (3, 1, 2), (0, 1, 0), (0, 0, 0) },
            rows.Select(row => (row.PermittedPeriodCount, row.BlackoutPeriodCount, row.DayTimeWindowCount)));
        Assert.Equal(
            new DateTimeOffset?[] { null, _clock.Now.AddDays(10), null },
            rows.Select(row => row.SalesEffectiveFrom));
    }

    [Theory]
    [InlineData(PricingUnit.PerPiece, AncillaryQuantityUnit.Kilogram)]
    [InlineData(PricingUnit.PerKilogram, AncillaryQuantityUnit.Piece)]
    [InlineData(PricingUnit.PerPassenger, AncillaryQuantityUnit.Kilogram)]
    public async Task V121_P07_P08_P09_publication_refuses_a_quantity_unit_that_conflicts_with_the_pricing_unit(PricingUnit pricingUnit, AncillaryQuantityUnit quantityUnit)
    {
        var definitionId = await DefinitionAsync(pricingUnit, ServiceDateBasis.FlightDeparture, $"UNIT_{pricingUnit}_{quantityUnit}".ToUpperInvariant());
        var paid = await RequestAsync(scope => scope.DefineProvision.DefineAsync(V122Catalog.Fit(Provision(definitionId, 10, quantityUnit: quantityUnit), pricingUnit)));
        var free = await RequestAsync(scope => scope.DefineProvision.DefineAsync(V122Catalog.Fit(Provision(definitionId, 20, CommercialDisposition.Free, quantityUnit: quantityUnit), pricingUnit)));
        var pricing = await RequestAsync(scope => scope.DefinePricing.DefineAsync(Pricing(paid.Id, Eur, Base(10m))));

        await RefusedAsync(16312, 409, scope => scope.PublishProvision.PublishAsync(new TestPublishProvisionCommand(paid.Id, pricing.Id)));
        await RefusedAsync(16312, 409, scope => scope.ActivateProvision.ActivateAsync(new TestActivateProvisionCommand(free.Id)));

        Assert.Equal("Draft", (await RequestAsync(scope => scope.GetProvisionById.ExecuteAsync(paid.Id))).Status.Name);
        Assert.Equal("Draft", (await RequestAsync(scope => scope.GetProvisionById.ExecuteAsync(free.Id))).Status.Name);
        Assert.Equal("Draft", (await RequestAsync(scope => scope.GetPricingById.ExecuteAsync(pricing.Id))).Status.Name);

        var compatible = pricingUnit switch
        {
            PricingUnit.PerPiece => AncillaryQuantityUnit.Piece,
            PricingUnit.PerKilogram => AncillaryQuantityUnit.Kilogram,
            _ => AncillaryQuantityUnit.Each
        };

        await RequestAsync(scope => scope.ChangeProvision.ChangeAsync(Change(paid.Id, V122Catalog.Fit(Provision(definitionId, 10, quantityUnit: compatible), pricingUnit))));

        Assert.Equal(
            ProvisionStatus.Active,
            (await RequestAsync(scope => scope.PublishProvision.PublishAsync(new TestPublishProvisionCommand(paid.Id, pricing.Id)))).Status);
    }

    [Fact]
    public async Task V121_A03_P18_E11_T06_publication_refuses_a_rule_the_service_basis_cannot_supply_or_that_can_never_match()
    {
        var simId = await DefinitionAsync(PricingUnit.PerPassenger, ServiceDateBasis.ServiceStart, "LOUNGE_TR");
        var flightId = await DefinitionAsync(PricingUnit.PerPassenger, ServiceDateBasis.FlightDeparture, "PRIORITY");

        async Task<long> DraftAsync(TestDefineProvisionCommand command) => (await RequestAsync(scope => scope.DefineProvision.DefineAsync(command))).Id;
        Task ActivationRefusedAsync(int code, int httpStatus, long provisionId)
            => RefusedAsync(code, httpStatus, scope => scope.ActivateProvision.ActivateAsync(new TestActivateProvisionCommand(provisionId)));

        var ticketed = await DraftAsync(Provision(simId, 10, CommercialDisposition.Free) with { AdvancePurchase = new(0, TimeUnit.Hours, true) });
        var fare = await DraftAsync(Provision(simId, 20, CommercialDisposition.Free) with { FareApplication = new(AllowedFareFamilyIds: [5]) });
        var flight = await DraftAsync(Provision(simId, 30, CommercialDisposition.Free) with { FlightApplication = new(AllowedFlightIds: [81234]) });
        var route = await DraftAsync(Provision(simId, 40, CommercialDisposition.Free) with { Geography = new(AllowedRoutePairs: [Pair(Thr, Ist)]) });
        var weeks = await DraftAsync(Provision(flightId, 10, CommercialDisposition.Free) with { AdvancePurchase = new(2, TimeUnit.Weeks) });
        var blackedOut = await DraftAsync(Provision(flightId, 20, CommercialDisposition.Free) with
        {
            TravelDate = Dates([Period(Day(2027, 4, 10), Day(2027, 4, 12))], [Period(Day(2027, 4, 1), Day(2027, 4, 30))])
        });
        var denied = await DraftAsync(Provision(flightId, 30, CommercialDisposition.Free) with
        {
            DayTimeApplication = DayTime(Window(Monday, 9, 10), Window(Monday, 8, 12, DayTimeRestrictionEffect.Deny))
        });
        var coverage = await DraftAsync(Provision(simId, 50, CommercialDisposition.Free) with
        {
            Geography = new(CoverageCountryIds: [90]),
            TravelDate = Dates([Period(Day(2027, 4, 1), Day(2027, 4, 30))]),
            AdvancePurchase = new(24, TimeUnit.Hours)
        });

        await ActivationRefusedAsync(16313, 409, ticketed);
        await ActivationRefusedAsync(16313, 409, fare);
        await ActivationRefusedAsync(16313, 409, flight);
        await ActivationRefusedAsync(16313, 409, route);
        await ActivationRefusedAsync(16314, 422, weeks);
        await ActivationRefusedAsync(16315, 409, blackedOut);
        await ActivationRefusedAsync(16315, 409, denied);

        Assert.Equal(ProvisionStatus.Active, (await RequestAsync(scope => scope.ActivateProvision.ActivateAsync(new TestActivateProvisionCommand(coverage)))).Status);
        Assert.All(
            new[] { ticketed, fare, flight, route, weeks, blackedOut, denied },
            provisionId => Assert.Equal(ProvisionStatus.Draft, RequestAsync(scope => scope.Provisions.GetAsync(provisionId)).GetAwaiter().GetResult()!.Status));

        await RequestAsync(scope => scope.ChangeAdvancePurchase.ChangeAsync(new TestChangeProvisionAdvancePurchaseCommand(weeks, new(14, TimeUnit.Days))));
        await RequestAsync(scope => scope.ChangeTravelDate.ChangeAsync(new TestChangeProvisionTravelDateCommand(blackedOut, Dates([Period(Day(2027, 4, 1), Day(2027, 4, 30))], [Period(Day(2027, 4, 10), Day(2027, 4, 12))]))));

        Assert.Equal(ProvisionStatus.Active, (await RequestAsync(scope => scope.ActivateProvision.ActivateAsync(new TestActivateProvisionCommand(weeks)))).Status);
        Assert.Equal(ProvisionStatus.Active, (await RequestAsync(scope => scope.ActivateProvision.ActivateAsync(new TestActivateProvisionCommand(blackedOut)))).Status);
    }

    [Fact]
    public async Task V121_M05_weights_and_amounts_keep_their_precision_and_the_superseded_columns_and_tables_are_gone_after_the_approved_cleanup()
    {
        var columns = await RequestAsync(scope => scope.Command.Database
            .SqlQueryRaw<string>(
                "SELECT TABLE_SCHEMA + '.' + TABLE_NAME + '.' + COLUMN_NAME + ' ' + DATA_TYPE + '(' + " +
                "CAST(NUMERIC_PRECISION AS varchar(3)) + ',' + CAST(NUMERIC_SCALE AS varchar(3)) + ')' AS Value " +
                "FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME IN ('UnitAmount', 'BaggageWeight', 'Amount', 'Weight') " +
                "AND TABLE_SCHEMA IN ('Ancillary', 'ReadModel')")
            .ToListAsync());

        Assert.Equal(
            new[]
            {
                "Ancillary.AncillaryPriceComponents.Amount decimal(19,6)",
                "Ancillary.ProvisionBaggageApplicationRules.Weight decimal(9,2)",
                "ReadModel.AncillaryPriceComponents.Amount decimal(19,6)",
                "ReadModel.AncillaryProvisions.BaggageWeight decimal(9,2)"
            },
            columns.OrderBy(column => column, StringComparer.Ordinal));

        var superseded = await RequestAsync(scope => scope.Command.Database
            .SqlQueryRaw<string>(
                "SELECT TABLE_SCHEMA + '.' + TABLE_NAME + '.' + COLUMN_NAME AS Value FROM INFORMATION_SCHEMA.COLUMNS " +
                "WHERE (TABLE_NAME = 'AncillaryProvisions' AND COLUMN_NAME IN ('FeeCurrencyId', 'FeeApplicationUnit', 'TravelFrom', 'TravelTo', 'TimeFrom', 'TimeTo')) " +
                "OR (TABLE_SCHEMA = 'Ancillary' AND TABLE_NAME = 'AncillaryProvisions' AND COLUMN_NAME IN ('SalesEffectiveFrom', 'SalesDiscontinueAt', 'AdvancePurchasePeriod', 'AdvancePurchaseUnit', 'BaggageWeight')) " +
                "OR (TABLE_NAME LIKE '%ProvisionTravelDates' AND COLUMN_NAME = 'TravelDate') " +
                "OR (TABLE_NAME LIKE '%ProvisionSeasonalPeriods' AND COLUMN_NAME = 'StartDate') " +
                "OR (TABLE_NAME LIKE '%ProvisionDayTimeRestrictions' AND COLUMN_NAME = 'DayOfWeek')")
            .ToListAsync());

        Assert.Empty(superseded);

        var mapped = await RequestAsync(scope => Task.FromResult(scope.Command.Model.GetEntityTypes()
            .Select(type => type.GetTableName())
            .Concat(scope.Query.Model.GetEntityTypes().Select(type => type.GetTableName()))
            .ToList()));

        Assert.DoesNotContain(mapped, table => table is not null && new[] { "TravelDates", "SeasonalPeriods", "DayTimeRestrictions" }.Any(table.EndsWith));
        Assert.DoesNotContain(
            await RequestAsync(scope => Task.FromResult(scope.Command.Model.FindEntityType(typeof(Domain.AncillaryProvisionAggregate.AncillaryProvision))!.GetProperties().Select(property => property.GetColumnName()).ToList())),
            column => column is "SalesEffectiveFrom" or "SalesDiscontinueAt" or "AdvancePurchasePeriod" or "BaggageWeight" or "TravelFrom" or "FeeCurrencyId");
    }

    [Fact]
    public void V121_M10_the_backoffice_validators_check_the_shape_of_every_authoring_input()
    {
        var template = FullyRestricted(1) with
        {
            Geography = new([Thr], [Ist], [Mhd], [Pair(Thr, Ist)], [Location(ServiceLocationType.Airport, Thr)], [98])
        };

        string[] DefineErrors(TestDefineProvisionCommand command)
            => new BackofficeDefineAncillaryProvisionCommandValidator()
                .Validate(new BackofficeDefineAncillaryProvisionCommand(
                    command.ServiceDefinitionId,
                    command.Sequence,
                    command.CoverageScope,
                    command.PurchaseStage,
                    command.PriceOrigin,
                    command.QuoteProviderKey,
                    command.Quantity,
                    command.ApplicationType,
                    command.Outcome,
                    command.Settlement,
                    command.Availability,
                    command.Fulfillment,
                    command.PassengerEligibility,
                    command.SalesRestrictions,
                    command.Geography,
                    command.FlightApplication,
                    command.FareApplication,
                    command.TravelDate,
                    command.DayTimeApplication,
                    command.AdvancePurchase,
                    command.BaggageApplication,
                    command.SeatApplication,
                    command.PetRule,
                    command.AssistedTravelRule,
                    command.AirportServiceRule))
                .Errors.Select(error => error.PropertyName).ToArray();

        string[] ChangeErrors(long provisionId, TestDefineProvisionCommand command)
            => new BackofficeChangeAncillaryProvisionCommandValidator()
                .Validate(new BackofficeChangeAncillaryProvisionCommand(
                    provisionId,
                    command.Sequence,
                    command.CoverageScope,
                    command.PurchaseStage,
                    command.PriceOrigin,
                    command.QuoteProviderKey,
                    command.Quantity,
                    command.ApplicationType,
                    command.Outcome,
                    command.Settlement,
                    command.Availability,
                    command.Fulfillment,
                    command.PassengerEligibility,
                    command.SalesRestrictions,
                    command.Geography,
                    command.FlightApplication,
                    command.FareApplication,
                    command.TravelDate,
                    command.DayTimeApplication,
                    command.AdvancePurchase,
                    command.BaggageApplication,
                    command.SeatApplication,
                    command.PetRule,
                    command.AssistedTravelRule,
                    command.AirportServiceRule))
                .Errors.Select(error => error.PropertyName).ToArray();

        (string Property, TestDefineProvisionCommand Command)[] malformed =
        [
            ("PassengerEligibility.AllowedPassengerTypes[0]", template with { PassengerEligibility = new([(PassengerTypeCode)(-1)]) }),
            ("SalesRestrictions.AllowedCustomerTypes[0]", template with { SalesRestrictions = new(AllowedCustomerTypes: [(CustomerType)99]) }),
            ("Geography.AllowedRoutePairs[0].Direction", template with { Geography = new(AllowedRoutePairs: [new(1, 6, (RoutePairDirection)9)]) }),
            ("Geography.ServiceLocations[0].LocationType", template with { Geography = new(ServiceLocations: [new((ServiceLocationType)9, 1)]) }),
            ("FlightApplication.AllowedFlightNumbers[0]", template with { FlightApplication = new(AllowedFlightNumbers: [new string('9', 17)]) }),
            ("FareApplication.AllowedAirFareTypes[0]", template with { FareApplication = new(AllowedAirFareTypes: [(AirFareType)99]) }),
            ("FareApplication.AllowedFareBasisCodes[0]", template with { FareApplication = new(AllowedFareBasisCodes: [new string('Y', 65)]) }),
            ("DayTimeApplication.Windows[0].DaysOfWeekMask", template with { DayTimeApplication = DayTime(Window(0)) }),
            ("DayTimeApplication.Windows[0].DaysOfWeekMask", template with { DayTimeApplication = DayTime(Window(128)) }),
            ("DayTimeApplication.Windows[0].Effect", template with { DayTimeApplication = DayTime(Window(Monday, effect: (DayTimeRestrictionEffect)9)) }),
            ("AdvancePurchase.Unit", template with { AdvancePurchase = new(3, (TimeUnit)99) }),
            ("AdvancePurchase.MinimumPeriod", template with { AdvancePurchase = new(-1, TimeUnit.Days) }),
            ("ApplicationType", template with { ApplicationType = (ProvisionApplicationType)9 }),
            ("BaggageApplication.WeightUnit", template with { BaggageApplication = Baggage(20m, weightUnit: (WeightUnit)9) }),
            ("BaggageApplication.TravelApplication", template with { BaggageApplication = Baggage(20m, travelApplication: (BaggageTravelApplication)9) }),
            ("SeatApplication.SeatNumbers[0]", template with { SeatApplication = Seat([new string('1', 17)], null) }),
            ("SeatApplication.SeatCharacteristicCodes[0]", template with { SeatApplication = Seat(null, [new string('W', 26)]) }),
            ("Fulfillment.FulfillmentProviderKey", template with { Fulfillment = new("") })
        ];

        Assert.Empty(DefineErrors(template));
        Assert.Empty(ChangeErrors(7, template));
        Assert.Contains("ProvisionId", ChangeErrors(0, template));

        foreach (var (property, command) in malformed)
        {
            Assert.Contains(property, DefineErrors(command));
            Assert.Contains(property, ChangeErrors(7, command));
        }

        string[] PricingErrors(long provisionId, FeeApplicationUnit? feeApplicationUnit, params PricingLineInput[] priceLines)
            => new BackofficeDefineAncillaryPricingCommandValidator()
                .Validate(new BackofficeDefineAncillaryPricingCommand(provisionId, FiledRates.Of(priceLines, Eur, feeApplicationUnit)))
                .Errors.Select(error => error.PropertyName).ToArray();

        string[] PricingChangeErrors(long pricingId, params PricingLineInput[] priceLines)
            => new BackofficeChangeAncillaryPricingCommandValidator()
                .Validate(new BackofficeChangeAncillaryPricingCommand(pricingId, FiledRates.Of(priceLines, Eur, FeeApplicationUnit.Item)))
                .Errors.Select(error => error.PropertyName).ToArray();

        var wrongCategory = new PricingLineInput(null, null, null, (AncillaryPriceLineCategory)99, null, null, null, null, 1m);
        var wrongPassenger = Base(1m, (PassengerTypeCode)(-1));
        var longCode = Tax(new string('T', 11), 1m);
        var longName = Base(1m, name: new string('n', 101));

        Assert.Empty(PricingErrors(1, FeeApplicationUnit.Item, FullPrice()));
        Assert.Empty(PricingChangeErrors(1, FullPrice()));
        Assert.Contains("AncillaryProvisionId", PricingErrors(0, FeeApplicationUnit.Item, FullPrice()));
        Assert.Contains(PricingErrors(1, (FeeApplicationUnit)99, FullPrice()), error => error.EndsWith(".FeeApplicationUnit", StringComparison.Ordinal));
        Assert.Contains("PricingId", PricingChangeErrors(0, FullPrice()));
        Assert.Equal(
            new[] { "Rates[0].Components[0].Category", "Rates[0].Components[0].Code", "Rates[0].Components[1].Code", "Rates[1].PassengerTypeCode" },
            PricingErrors(1, null, wrongCategory, wrongPassenger, longCode, longName));
        Assert.Equal(
            new[] { "Rates[0].Components[0].Category", "Rates[0].Components[0].Code", "Rates[0].Components[1].Code", "Rates[1].PassengerTypeCode" },
            PricingChangeErrors(1, wrongCategory, wrongPassenger, longCode, longName));

        Assert.Equal(
            new[] { "ProvisionId", "DaysOfWeekMask", "Effect" },
            new BackofficeAddProvisionDayTimeWindowCommandValidator()
                .Validate(new BackofficeAddProvisionDayTimeWindowCommand(0, 0, null, null, (DayTimeRestrictionEffect)9))
                .Errors.Select(error => error.PropertyName));
        Assert.Equal(
            new[] { "ProvisionId", "RowId", "DaysOfWeekMask" },
            new BackofficeChangeProvisionDayTimeWindowCommandValidator()
                .Validate(new BackofficeChangeProvisionDayTimeWindowCommand(0, 0, 200, null, null, DayTimeRestrictionEffect.Deny))
                .Errors.Select(error => error.PropertyName));
        Assert.Equal(
            new[] { "ProvisionId" },
            new BackofficeAddProvisionPermittedTravelPeriodCommandValidator()
                .Validate(new BackofficeAddProvisionPermittedTravelPeriodCommand(0, new DateOnly(2027, 1, 1), new DateOnly(2027, 1, 2)))
                .Errors.Select(error => error.PropertyName));
        Assert.Equal(
            new[] { "ProvisionId" },
            new BackofficeAddProvisionBlackoutPeriodCommandValidator()
                .Validate(new BackofficeAddProvisionBlackoutPeriodCommand(0, new DateOnly(2027, 1, 1), new DateOnly(2027, 1, 2)))
                .Errors.Select(error => error.PropertyName));
        Assert.Equal(
            new[] { "ProvisionId", "RowId" },
            new BackofficeChangeProvisionPermittedTravelPeriodCommandValidator()
                .Validate(new BackofficeChangeProvisionPermittedTravelPeriodCommand(0, 0, new DateOnly(2027, 1, 1), new DateOnly(2027, 1, 2)))
                .Errors.Select(error => error.PropertyName));
        Assert.Equal(
            new[] { "ProvisionId", "RowId" },
            new BackofficeRemoveProvisionBlackoutPeriodCommandValidator()
                .Validate(new BackofficeRemoveProvisionBlackoutPeriodCommand(0, 0))
                .Errors.Select(error => error.PropertyName));
        Assert.Equal(
            new[] { "ProvisionId", "Geography.AllowedRoutePairs[0].Direction", "Geography.ServiceLocations[0].LocationType" },
            new BackofficeChangeProvisionGeographyCommandValidator()
                .Validate(new BackofficeChangeProvisionGeographyCommand(0, new(AllowedRoutePairs: [new(1, 6, (RoutePairDirection)9)], ServiceLocations: [new((ServiceLocationType)9, 1)])))
                .Errors.Select(error => error.PropertyName));
        Assert.Empty(new BackofficeChangeProvisionGeographyCommandValidator().Validate(new BackofficeChangeProvisionGeographyCommand(7, null)).Errors);
        Assert.Empty(new BackofficeChangeProvisionTravelDateCommandValidator().Validate(new BackofficeChangeProvisionTravelDateCommand(7, Dates([Period(Day(2027, 1, 1), Day(2027, 1, 2))]))).Errors);
        Assert.Equal(
            new[] { "DayTimeApplication.Windows[1].DaysOfWeekMask" },
            new BackofficeChangeProvisionDayTimeApplicationCommandValidator()
                .Validate(new BackofficeChangeProvisionDayTimeApplicationCommand(7, DayTime(Window(Monday), Window(0))))
                .Errors.Select(error => error.PropertyName));
        Assert.Equal(
            new[] { "ProvisionId", "PricingId" },
            new BackofficePublishAncillaryProvisionCommandValidator()
                .Validate(new BackofficePublishAncillaryProvisionCommand(0, 0))
                .Errors.Select(error => error.PropertyName));
        Assert.Equal(
            new[] { "ProvisionId", "NewPricingId", "ExpectedOldPricingId" },
            new BackofficeSwitchActiveAncillaryPricingCommandValidator()
                .Validate(new BackofficeSwitchActiveAncillaryPricingCommand(0, 0, 0))
                .Errors.Select(error => error.PropertyName));
        Assert.Equal(
            new[] { "ServiceDefinitionId", "PricingUnit" },
            new BackofficeAssignAncillaryServiceDefinitionPricingUnitCommandValidator()
                .Validate(new BackofficeAssignAncillaryServiceDefinitionPricingUnitCommand(0, (PricingUnit)99))
                .Errors.Select(error => error.PropertyName));
        Assert.Equal(
            new[] { "ServiceDefinitionId", "ServiceDateBasis" },
            new BackofficeAssignAncillaryServiceDefinitionServiceDateBasisCommandValidator()
                .Validate(new BackofficeAssignAncillaryServiceDefinitionServiceDateBasisCommand(0, (ServiceDateBasis)99))
                .Errors.Select(error => error.PropertyName));
    }
}
