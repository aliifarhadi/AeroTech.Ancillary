using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.ChangeAncillaryPricing.Backoffice;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing.Backoffice;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.SwitchActiveAncillaryPricing.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeAncillaryProvision.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.PublishAncillaryProvision.Backoffice;
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
