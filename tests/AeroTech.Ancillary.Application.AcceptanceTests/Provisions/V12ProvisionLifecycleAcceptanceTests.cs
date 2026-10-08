using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.ChangeAncillaryPricing.Backoffice;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing.Backoffice;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.SwitchActiveAncillaryPricing.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionDayTimeRestriction.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionTravelDate.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeAncillaryProvision.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionSeasonalPeriod.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.PublishAncillaryProvision.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RemoveProvisionTravelDate.Backoffice;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.AssignAncillaryServiceDefinitionPricingUnit.Backoffice;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetAncillaryProvisionsPaginated.Backoffice;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;
using AeroTech.Messages.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.P1Commands;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.V12Commands;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Provisions;

[Collection(DatabaseCollection.Name)]
public class V12ProvisionLifecycleAcceptanceTests
{
    private readonly TestDatabase _database;
    private readonly FixedClock _clock = new();
    private readonly FamilyProof _proof;

    public V12ProvisionLifecycleAcceptanceTests(TestDatabase database)
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

    private static TestDefineProvisionCommand FullyRestricted(long definitionId)
        => Provision(
            definitionId,
            10,
            coverageScope: ServiceCoverageScope.Journey,
            quantityUnit: AncillaryQuantityUnit.Piece,
            maxQuantity: 3,
            salesEffectiveFrom: new DateTimeOffset(2026, 11, 1, 0, 0, 0, TimeSpan.FromMinutes(210)),
            salesDiscontinueAt: new DateTimeOffset(2026, 12, 1, 0, 0, 0, TimeSpan.FromMinutes(210)),
            passenger: Passengers(PassengerTypeCode.ADT, PassengerTypeCode.CHD),
            sales: new ProvisionSalesCriteriaInput([501, 502], [9001], [CustomerType.TravelAgency, CustomerType.Organization]),
            travel: new ProvisionTravelCriteriaInput(
                [Thr],
                [Ist],
                [Mhd],
                [Pair(Thr, Ist), Pair(Mhd, Ist, RoutePairDirection.BothDirections)],
                [new DateOnly(2026, 12, 24), new DateOnly(2026, 12, 25)],
                [Period(new DateOnly(2026, 12, 20), new DateOnly(2026, 12, 31))],
                [Period(new DateOnly(2026, 12, 31), new DateOnly(2026, 12, 31))],
                [DayTime(DayOfWeek.Monday, 8, 12), DayTime(DayOfWeek.Monday, 9, 10, DayTimeRestrictionEffect.Deny)],
                [1, 2],
                [3],
                [" w5112", "W5116"],
                [81234, 81240],
                [1, 2]),
            fare: new ProvisionFareCriteriaInput([7001], [AirFareType.Public, AirFareType.Private], [5, 6], ["y26lt"], [2], [41, 42]),
            advancePurchase: new ProvisionAdvancePurchaseInput(3, TimeUnit.Days),
            application: Baggage(
                23.50m,
                1,
                2,
                BaggageTravelApplication.MostSignificantSector,
                BaggagePurchaseApplication.PrepaidAndCheckIn,
                BaggageRuleDeference.OperatingCarrier,
                0,
                WeightUnit.Lbs));

    private static PricingLineInput[] FullPrice() =>
    [
        Base(30m, name: "Extra bag"),
        Tax("VAT", 2.7m, countryId: 98, stationAirportId: Thr, name: "Value added tax"),
        Fee("HDL", 1.05m)
    ];

    [Fact]
    public async Task V12_C02_a_draft_edit_is_refused_when_it_is_malformed_or_the_provision_does_not_exist()
    {
        var definitionId = await DefinitionAsync();
        var command = Provision(definitionId, 10, passenger: Passengers(PassengerTypeCode.ADT));
        var draft = await RequestAsync(scope => scope.DefineProvision.DefineAsync(command));

        await RefusedAsync(16302, 422, scope => scope.ChangeProvision.ChangeAsync(Change(draft.Id, command with { Sequence = 0 })));
        await RefusedAsync(16302, 422, scope => scope.ChangeProvision.ChangeAsync(Change(draft.Id, command with { Application = Seat(["12A"], null) })));
        await RefusedAsync(16302, 422, scope => scope.ChangeProvision.ChangeAsync(
            Change(draft.Id, command with { Travel = new ProvisionTravelCriteriaInput(RoutePairs: [Pair(Thr, Thr)]) })));
        await RefusedAsync(16302, 422, scope => scope.ChangeProvision.ChangeAsync(
            Change(draft.Id, command with { Travel = new ProvisionTravelCriteriaInput(RoutePairs: [Pair(Thr, Ist), Pair(Ist, Thr, RoutePairDirection.BothDirections)]) })));
        await RefusedAsync(16302, 422, scope => scope.ChangeProvision.ChangeAsync(
            Change(draft.Id, command with { Passenger = Passengers(PassengerTypeCode.ADT, PassengerTypeCode.ADT) })));
        await RefusedAsync(16301, 404, scope => scope.ChangeProvision.ChangeAsync(Change(999_999_999, command)));
        await RefusedAsync(16304, 422, scope => scope.DefineProvision.DefineAsync(command with { ServiceDefinitionId = 999_999_999 }));

        var detail = await RequestAsync(scope => scope.GetProvisionById.ExecuteAsync(draft.Id));

        Assert.Equal(("Draft", 10, "Standard"), (detail.Status.Name, detail.Sequence, detail.ApplicationType.Name));
        Assert.Equal(new[] { "ADT" }, detail.Passenger.PassengerTypes.Select(row => row.Value.Name));
        Assert.Empty(detail.Travel.RoutePairs);
    }

    [Fact]
    public async Task V12_C02_the_lifecycle_moves_a_paid_provision_with_its_conditions_and_price_through_the_existing_states()
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
                (2, 2, 2, 1, 1, 2, 2, 2),
                (detail.Passenger.PassengerTypes.Count, detail.Sales.PointsOfSale.Count, detail.Travel.RoutePairs.Count, detail.Travel.SeasonalPeriods.Count,
                    detail.Travel.BlackoutPeriods.Count, detail.Travel.DayTimeRestrictions.Count, detail.Travel.TravelDates.Count, detail.Fare.Rbds.Count));
            Assert.Equal((23.5m, "Lbs", "PrepaidAndCheckIn"), (detail.Baggage!.Weight, detail.Baggage.WeightUnit.Name, detail.Baggage.PurchaseApplication.Name));
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
    public async Task V12_C02_a_suspended_provision_cannot_be_reactivated_while_its_sequence_has_another_active_row()
    {
        var definitionId = await DefinitionAsync();
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
    public async Task V12_C02_provisions_of_a_definition_are_filtered_by_status_coverage_sequence_and_sales_date()
    {
        var definitionId = await DefinitionAsync();
        var sector = await RequestAsync(scope => scope.DefineProvision.DefineAsync(
            Provision(
                definitionId,
                10,
                CommercialDisposition.Free,
                travel: new ProvisionTravelCriteriaInput(
                    TravelDates: [new DateOnly(2027, 1, 1), new DateOnly(2027, 1, 2), new DateOnly(2027, 1, 3)],
                    SeasonalPeriods: [Period(new DateOnly(2027, 1, 1), new DateOnly(2027, 1, 31))],
                    DayTimeRestrictions: [DayTime(DayOfWeek.Monday, 8, 12), DayTime(DayOfWeek.Tuesday, null, null)]))));
        var journey = await RequestAsync(scope => scope.DefineProvision.DefineAsync(
            Provision(
                definitionId,
                20,
                CommercialDisposition.Free,
                ServiceCoverageScope.Journey,
                salesEffectiveFrom: _clock.Now.AddDays(10),
                salesDiscontinueAt: _clock.Now.AddDays(20),
                travel: new ProvisionTravelCriteriaInput(BlackoutPeriods: [Period(new DateOnly(2027, 3, 21), new DateOnly(2027, 3, 21))]))));
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
            new[] { (3, 1, 0, 2), (0, 0, 1, 0), (0, 0, 0, 0) },
            rows.Select(row => (row.TravelDateCount, row.SeasonalPeriodCount, row.BlackoutPeriodCount, row.DayTimeRestrictionCount)));
    }

    [Fact]
    public async Task V12_C02_C03_weights_and_amounts_keep_their_precision_and_the_v11_price_columns_are_kept_beside_the_new_tables()
    {
        var columns = await RequestAsync(scope => scope.Command.Database
            .SqlQueryRaw<string>(
                "SELECT TABLE_SCHEMA + '.' + TABLE_NAME + '.' + COLUMN_NAME + ' ' + DATA_TYPE + '(' + " +
                "CAST(NUMERIC_PRECISION AS varchar(3)) + ',' + CAST(NUMERIC_SCALE AS varchar(3)) + ')' AS Value " +
                "FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME IN ('UnitAmount', 'BaggageWeight', 'Amount') " +
                "AND TABLE_SCHEMA IN ('Ancillary', 'ReadModel')")
            .ToListAsync());

        Assert.Equal(
            new[]
            {
                "Ancillary.AncillaryPricingLines.Amount decimal(18,2)",
                "Ancillary.AncillaryProvisions.BaggageWeight decimal(9,2)",
                "Ancillary.ProvisionPriceLines.UnitAmount decimal(18,2)",
                "ReadModel.AncillaryPricingLines.Amount decimal(18,2)",
                "ReadModel.AncillaryProvisionPriceLines.UnitAmount decimal(18,2)",
                "ReadModel.AncillaryProvisions.BaggageWeight decimal(9,2)"
            },
            columns.OrderBy(column => column, StringComparer.Ordinal));

        var legacy = await RequestAsync(scope => scope.Command.Database
            .SqlQueryRaw<string>(
                "SELECT TABLE_SCHEMA + '.' + TABLE_NAME + '.' + COLUMN_NAME AS Value FROM INFORMATION_SCHEMA.COLUMNS " +
                "WHERE TABLE_NAME = 'AncillaryProvisions' AND COLUMN_NAME IN ('FeeCurrencyId', 'FeeApplicationUnit', 'TravelFrom', 'TravelTo', 'TimeFrom', 'TimeTo')")
            .ToListAsync());

        Assert.Equal(12, legacy.Count);
    }

    [Fact]
    public void V12_C02_the_backoffice_validators_check_the_shape_of_every_authoring_input()
    {
        var template = FullyRestricted(1);

        string[] DefineErrors(TestDefineProvisionCommand command)
            => new BackofficeDefineAncillaryProvisionCommandValidator()
                .Validate(new BackofficeDefineAncillaryProvisionCommand(
                    command.ServiceDefinitionId,
                    command.Sequence,
                    command.SalesEffectiveFrom,
                    command.SalesDiscontinueAt,
                    command.CoverageScope,
                    command.Passenger,
                    command.Sales,
                    command.Travel,
                    command.Fare,
                    command.AdvancePurchase,
                    command.Quantity,
                    command.Application,
                    command.Outcome,
                    command.Settlement,
                    command.Availability,
                    command.Fulfillment))
                .Errors.Select(error => error.PropertyName).ToArray();

        string[] ChangeErrors(long provisionId, TestDefineProvisionCommand command)
            => new BackofficeChangeAncillaryProvisionCommandValidator()
                .Validate(new BackofficeChangeAncillaryProvisionCommand(
                    provisionId,
                    command.Sequence,
                    command.SalesEffectiveFrom,
                    command.SalesDiscontinueAt,
                    command.CoverageScope,
                    command.Passenger,
                    command.Sales,
                    command.Travel,
                    command.Fare,
                    command.AdvancePurchase,
                    command.Quantity,
                    command.Application,
                    command.Outcome,
                    command.Settlement,
                    command.Availability,
                    command.Fulfillment))
                .Errors.Select(error => error.PropertyName).ToArray();

        (string Property, TestDefineProvisionCommand Command)[] malformed =
        [
            ("Passenger.PassengerTypeCodes[0]", template with { Passenger = new([(PassengerTypeCode)(-1)]) }),
            ("Sales.CustomerTypes[0]", template with { Sales = new(CustomerTypes: [(CustomerType)99]) }),
            ("Travel.RoutePairs[0].Direction", template with { Travel = new(RoutePairs: [new(1, 6, (RoutePairDirection)9)]) }),
            ("Travel.DayTimeRestrictions[0].DayOfWeek", template with { Travel = new(DayTimeRestrictions: [new((DayOfWeek)9, null, null, DayTimeRestrictionEffect.Allow)]) }),
            ("Travel.DayTimeRestrictions[0].Effect", template with { Travel = new(DayTimeRestrictions: [new(DayOfWeek.Monday, null, null, (DayTimeRestrictionEffect)9)]) }),
            ("Travel.FlightNumbers[0]", template with { Travel = new(FlightNumbers: [new string('9', 17)]) }),
            ("Fare.AirFareTypes[0]", template with { Fare = new(AirFareTypes: [(AirFareType)99]) }),
            ("Fare.FareBasisCodes[0]", template with { Fare = new(FareBasisCodes: [new string('Y', 65)]) }),
            ("AdvancePurchase.Unit", template with { AdvancePurchase = new(3, (TimeUnit)99) }),
            ("Application.Type", template with { Application = new((ProvisionApplicationType)9) }),
            ("Application.Baggage.WeightUnit", template with { Application = Baggage(20m, weightUnit: (WeightUnit)9) }),
            ("Application.Baggage.TravelApplication", template with { Application = Baggage(20m, travelApplication: (BaggageTravelApplication)9) }),
            ("Application.Seat.SeatNumbers[0]", template with { Application = Seat([new string('1', 17)], null) }),
            ("Application.Seat.SeatCharacteristicCodes[0]", template with { Application = Seat(null, [new string('W', 26)]) }),
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
                .Validate(new BackofficeDefineAncillaryPricingCommand(provisionId, Eur, feeApplicationUnit, priceLines))
                .Errors.Select(error => error.PropertyName).ToArray();

        string[] PricingChangeErrors(long pricingId, params PricingLineInput[] priceLines)
            => new BackofficeChangeAncillaryPricingCommandValidator()
                .Validate(new BackofficeChangeAncillaryPricingCommand(pricingId, Eur, FeeApplicationUnit.Item, priceLines))
                .Errors.Select(error => error.PropertyName).ToArray();

        var wrongCategory = new PricingLineInput(null, null, null, (AncillaryPriceLineCategory)99, null, null, null, null, 1m);
        var wrongPassenger = Base(1m, (PassengerTypeCode)(-1));
        var longCode = Tax(new string('T', 11), 1m);
        var longName = Base(1m, name: new string('n', 101));

        Assert.Empty(PricingErrors(1, FeeApplicationUnit.Item, FullPrice()));
        Assert.Empty(PricingChangeErrors(1, FullPrice()));
        Assert.Contains("AncillaryProvisionId", PricingErrors(0, FeeApplicationUnit.Item, FullPrice()));
        Assert.Contains("FeeApplicationUnit", PricingErrors(1, (FeeApplicationUnit)99, FullPrice()));
        Assert.Contains("PricingId", PricingChangeErrors(0, FullPrice()));
        Assert.Equal(
            new[] { "PriceLines[0].Category", "PriceLines[1].PassengerTypeCode", "PriceLines[2].Code", "PriceLines[3].Name" },
            PricingErrors(1, null, wrongCategory, wrongPassenger, longCode, longName));
        Assert.Equal(
            new[] { "PriceLines[0].Category", "PriceLines[1].PassengerTypeCode", "PriceLines[2].Code", "PriceLines[3].Name" },
            PricingChangeErrors(1, wrongCategory, wrongPassenger, longCode, longName));

        Assert.Equal(
            new[] { "ProvisionId", "DayOfWeek", "Effect" },
            new BackofficeAddProvisionDayTimeRestrictionCommandValidator()
                .Validate(new BackofficeAddProvisionDayTimeRestrictionCommand(0, (DayOfWeek)9, null, null, (DayTimeRestrictionEffect)9))
                .Errors.Select(error => error.PropertyName));
        Assert.Equal(
            new[] { "ProvisionId" },
            new BackofficeAddProvisionTravelDateCommandValidator()
                .Validate(new BackofficeAddProvisionTravelDateCommand(0, new DateOnly(2027, 1, 1)))
                .Errors.Select(error => error.PropertyName));
        Assert.Equal(
            new[] { "ProvisionId", "RowId" },
            new BackofficeChangeProvisionSeasonalPeriodCommandValidator()
                .Validate(new BackofficeChangeProvisionSeasonalPeriodCommand(0, 0, new DateOnly(2027, 1, 1), new DateOnly(2027, 1, 2)))
                .Errors.Select(error => error.PropertyName));
        Assert.Equal(
            new[] { "ProvisionId", "RowId" },
            new BackofficeRemoveProvisionTravelDateCommandValidator()
                .Validate(new BackofficeRemoveProvisionTravelDateCommand(0, 0))
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
        Assert.Empty(
            new BackofficeSwitchActiveAncillaryPricingCommandValidator()
                .Validate(new BackofficeSwitchActiveAncillaryPricingCommand(1, 2, null))
                .Errors);
        Assert.Equal(
            new[] { "ServiceDefinitionId", "PricingUnit" },
            new BackofficeAssignAncillaryServiceDefinitionPricingUnitCommandValidator()
                .Validate(new BackofficeAssignAncillaryServiceDefinitionPricingUnitCommand(0, (PricingUnit)99))
                .Errors.Select(error => error.PropertyName));
    }
}
