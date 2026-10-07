using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeAncillaryProvision.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision.Backoffice;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetAncillaryProvisionsPaginated.Backoffice;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;
using AeroTech.Messages.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Provisions;

[Collection(DatabaseCollection.Name)]
public class P1ProvisionAcceptanceTests
{
    private readonly TestDatabase _database;
    private readonly FixedClock _clock = new();

    public P1ProvisionAcceptanceTests(TestDatabase database) => _database = database;

    private async Task<long> DefinitionAsync(AncillaryScope scope)
    {
        var airlineId = _database.NextAirlineId();
        var supplier = await scope.RegisterSupplier.RegisterAsync(M1Commands.LocalSupplier(airlineId));
        var definition = await scope.DefineServiceDefinition.DefineAsync(P1Commands.FirstExcessBagDefinition(airlineId, supplier.Id));
        await scope.ActivateServiceDefinition.ActivateAsync(new TestActivateServiceDefinitionCommand(definition.Id));

        return definition.Id;
    }

    private static TestDefineProvisionCommand FullyRestricted(long definitionId)
        => P1Commands.Provision(
            definitionId,
            10,
            coverageScope: ServiceCoverageScope.Journey,
            quantityUnit: AncillaryQuantityUnit.Piece,
            maxQuantity: 3,
            salesEffectiveFrom: new DateTimeOffset(2026, 11, 1, 0, 0, 0, TimeSpan.FromMinutes(210)),
            salesDiscontinueAt: new DateTimeOffset(2026, 12, 1, 0, 0, 0, TimeSpan.FromMinutes(210)),
            passenger: P1Commands.Passengers(PassengerTypeCode.ADT, PassengerTypeCode.CHD),
            sales: new ProvisionSalesCriteriaInput([501, 502], [9001], [CustomerType.TravelAgency, CustomerType.Organization]),
            travel: new ProvisionTravelCriteriaInput(
                [P1Commands.Thr],
                [P1Commands.Ist],
                [P1Commands.Mhd],
                [P1Commands.Pair(P1Commands.Thr, P1Commands.Ist), P1Commands.Pair(P1Commands.Mhd, P1Commands.Ist, RoutePairDirection.BothDirections)],
                new DateOnly(2026, 12, 20),
                new DateOnly(2026, 12, 31),
                [DayOfWeek.Monday, DayOfWeek.Friday],
                new TimeOnly(22, 0),
                new TimeOnly(2, 0),
                [1, 2],
                [3],
                [" w5112", "W5116"],
                [81234, 81240],
                [320, 321]),
            fare: new ProvisionFareCriteriaInput([7001], [AirFareType.Public, AirFareType.Private], [5, 6], ["y26lt"], [2], [41, 42]),
            advancePurchase: new ProvisionAdvancePurchaseInput(3, TimeUnit.Days),
            application: P1Commands.Baggage(
                23.50m,
                1,
                2,
                BaggageTravelApplication.MostSignificantSector,
                BaggagePurchaseApplication.PrepaidAndCheckIn,
                BaggageRuleDeference.OperatingCarrier,
                0,
                WeightUnit.Lbs),
            priceLines:
            [
                new ProvisionPriceLineInput(AncillaryPriceLineCategory.Ancillary, null, "Extra bag", 30.00m),
                new ProvisionPriceLineInput(AncillaryPriceLineCategory.Tax, "VAT", "Value added tax", 2.70m, 98, P1Commands.Thr),
                new ProvisionPriceLineInput(AncillaryPriceLineCategory.Fee, null, "Handling", 1.05m)
            ]);

    [Fact]
    public async Task P1_K04_every_typed_criterion_round_trips_through_the_command_store()
    {
        await using var scope = new AncillaryScope(_database, _clock);
        var definitionId = await DefinitionAsync(scope);
        var draft = await scope.DefineProvision.DefineAsync(FullyRestricted(definitionId));

        await using var reader = new AncillaryScope(_database, _clock);
        var provision = (await reader.Provisions.GetAsync(draft.Id))!;

        Assert.Equal(new[] { PassengerTypeCode.ADT, PassengerTypeCode.CHD }, provision.Passenger.PassengerTypeCodes);
        Assert.Equal(new long[] { 501, 502 }, provision.Sales.PointOfSaleIds);
        Assert.Equal(new long[] { 9001 }, provision.Sales.CustomerIds);
        Assert.Equal(new[] { CustomerType.TravelAgency, CustomerType.Organization }, provision.Sales.CustomerTypes);
        Assert.Equal(new[] { P1Commands.Thr }, provision.Travel.OriginAirportIds);
        Assert.Equal(new[] { P1Commands.Ist }, provision.Travel.DestinationAirportIds);
        Assert.Equal(new[] { P1Commands.Mhd }, provision.Travel.ViaAirportIds);
        Assert.Equal(
            new[]
            {
                (P1Commands.Thr, P1Commands.Ist, RoutePairDirection.Directional),
                (P1Commands.Mhd, P1Commands.Ist, RoutePairDirection.BothDirections)
            },
            provision.RoutePairs.OrderBy(pair => pair.Id).Select(pair => (pair.OriginAirportId, pair.DestinationAirportId, pair.Direction)));
        Assert.Equal((new DateOnly(2026, 12, 20), new DateOnly(2026, 12, 31)), (provision.Travel.TravelFrom!.Value, provision.Travel.TravelTo!.Value));
        Assert.Equal(new[] { DayOfWeek.Monday, DayOfWeek.Friday }, provision.Travel.DaysOfWeek);
        Assert.Equal((new TimeOnly(22, 0), new TimeOnly(2, 0)), (provision.Travel.TimeFrom!.Value, provision.Travel.TimeTo!.Value));
        Assert.Equal(new[] { 1, 2 }, provision.Travel.MarketingAirlineIds);
        Assert.Equal(new[] { 3 }, provision.Travel.OperatingAirlineIds);
        Assert.Equal(new[] { "W5112", "W5116" }, provision.Travel.FlightNumbers);
        Assert.Equal(new long[] { 81234, 81240 }, provision.Travel.FlightIds);
        Assert.Equal(new[] { 320, 321 }, provision.Travel.AircraftIds);
        Assert.Equal(new long[] { 7001 }, provision.Fare.AirFareIds);
        Assert.Equal(new[] { AirFareType.Public, AirFareType.Private }, provision.Fare.AirFareTypes);
        Assert.Equal(new long[] { 5, 6 }, provision.Fare.FareFamilyIds);
        Assert.Equal(new[] { "Y26LT" }, provision.Fare.FareBasisCodes);
        Assert.Equal(new[] { 2 }, provision.Fare.CabinClassIds);
        Assert.Equal(new long[] { 41, 42 }, provision.Fare.RbdIds);
        Assert.Equal((3, TimeUnit.Days), (provision.AdvancePurchase!.Period, provision.AdvancePurchase.Unit));
        Assert.Equal(ProvisionApplicationType.Baggage, provision.Application.Type);
        Assert.Null(provision.Application.Seat);

        var baggage = provision.Application.Baggage!;

        Assert.Equal((0, 1, 2, 23.50m, WeightUnit.Lbs), (baggage.FreePieces, baggage.FirstExcessPiece, baggage.LastExcessPiece, baggage.Weight, baggage.WeightUnit));
        Assert.Equal(
            (BaggageTravelApplication.MostSignificantSector, BaggagePurchaseApplication.PrepaidAndCheckIn, BaggageRuleDeference.OperatingCarrier),
            (baggage.TravelApplication, baggage.PurchaseApplication, baggage.RuleDeference));
        Assert.Equal("Ancillary", provision.Fulfillment.FulfillmentProviderKey);
        Assert.Equal(
            new[] { ("Ancillary", (string?)null, (int?)null, (int?)null, 30.00m), ("Tax", "VAT", 98, P1Commands.Thr, 2.70m), ("Fee", null, null, null, 1.05m) },
            provision.PriceLines.OrderBy(line => line.Id)
                .Select(line => (line.Category.ToString(), line.Code, line.CountryId, line.StationAirportId, line.UnitAmount)));
    }

    [Fact]
    public async Task P1_K04_the_backoffice_detail_returns_every_typed_criterion()
    {
        await using var scope = new AncillaryScope(_database, _clock);
        var definitionId = await DefinitionAsync(scope);
        var draft = await scope.DefineProvision.DefineAsync(FullyRestricted(definitionId));

        await using var reader = new AncillaryScope(_database, _clock);
        var detail = await reader.GetProvisionById.ExecuteAsync(draft.Id);

        Assert.Equal(("Draft", 10, "Journey", "Piece", 1, 3), (detail.Status.Name, detail.Sequence, detail.CoverageScope.Name, detail.QuantityUnit.Name, detail.MinQuantity, detail.MaxQuantity));
        Assert.Equal(
            (new DateTimeOffset(2026, 11, 1, 0, 0, 0, TimeSpan.FromMinutes(210)), new DateTimeOffset(2026, 12, 1, 0, 0, 0, TimeSpan.FromMinutes(210))),
            (detail.SalesEffectiveFrom!.Value, detail.SalesDiscontinueAt!.Value));
        Assert.Equal(new[] { "ADT", "CHD" }, detail.Passenger.PassengerTypeCodes.Select(code => code.Name));
        Assert.Equal(new long[] { 501, 502 }, detail.Sales.PointOfSaleIds);
        Assert.Equal(new long[] { 9001 }, detail.Sales.CustomerIds);
        Assert.Equal(new[] { "TravelAgency", "Organization" }, detail.Sales.CustomerTypes.Select(type => type.Name));
        Assert.Equal(new[] { P1Commands.Thr }, detail.Travel.OriginAirportIds);
        Assert.Equal(new[] { P1Commands.Ist }, detail.Travel.DestinationAirportIds);
        Assert.Equal(new[] { P1Commands.Mhd }, detail.Travel.ViaAirportIds);
        Assert.Equal(
            new[] { (P1Commands.Thr, P1Commands.Ist, "Directional"), (P1Commands.Mhd, P1Commands.Ist, "BothDirections") },
            detail.Travel.RoutePairs.Select(pair => (pair.OriginAirportId, pair.DestinationAirportId, pair.Direction.Name)));
        Assert.Equal((new DateOnly(2026, 12, 20), new DateOnly(2026, 12, 31)), (detail.Travel.TravelFrom!.Value, detail.Travel.TravelTo!.Value));
        Assert.Equal(new[] { "Monday", "Friday" }, detail.Travel.DaysOfWeek.Select(day => day.Name));
        Assert.Equal((new TimeOnly(22, 0), new TimeOnly(2, 0)), (detail.Travel.TimeFrom!.Value, detail.Travel.TimeTo!.Value));
        Assert.Equal(new[] { 1, 2 }, detail.Travel.MarketingAirlineIds);
        Assert.Equal(new[] { 3 }, detail.Travel.OperatingAirlineIds);
        Assert.Equal(new[] { "W5112", "W5116" }, detail.Travel.FlightNumbers);
        Assert.Equal(new long[] { 81234, 81240 }, detail.Travel.FlightIds);
        Assert.Equal(new[] { 320, 321 }, detail.Travel.AircraftIds);
        Assert.Equal(new long[] { 7001 }, detail.Fare.AirFareIds);
        Assert.Equal(new[] { "Public", "Private" }, detail.Fare.AirFareTypes.Select(type => type.Name));
        Assert.Equal(new long[] { 5, 6 }, detail.Fare.FareFamilyIds);
        Assert.Equal(new[] { "Y26LT" }, detail.Fare.FareBasisCodes);
        Assert.Equal(new[] { 2 }, detail.Fare.CabinClassIds);
        Assert.Equal(new long[] { 41, 42 }, detail.Fare.RbdIds);
        Assert.Equal((3, "Days"), (detail.AdvancePurchase!.Period, detail.AdvancePurchase.Unit.Name));
        Assert.Equal("Baggage", detail.ApplicationType.Name);
        Assert.Null(detail.Seat);
        Assert.Equal((0, 1, 2, 23.50m, "Lbs"), (detail.Baggage!.FreePieces, detail.Baggage.FirstExcessPiece, detail.Baggage.LastExcessPiece, detail.Baggage.Weight, detail.Baggage.WeightUnit.Name));
        Assert.Equal(
            ("MostSignificantSector", "PrepaidAndCheckIn", "OperatingCarrier"),
            (detail.Baggage.TravelApplication!.Name, detail.Baggage.PurchaseApplication.Name, detail.Baggage.RuleDeference!.Name));
        Assert.Equal(("Paid", P1Commands.Eur, "Item", "Ancillary"), (detail.Disposition.Name, detail.FeeCurrencyId, detail.FeeApplicationUnit!.Name, detail.FulfillmentProviderKey));
        Assert.Equal(
            new[] { ("Ancillary", (string?)null, (int?)null, (int?)null, 30.00m), ("Tax", "VAT", 98, P1Commands.Thr, 2.70m), ("Fee", null, null, null, 1.05m) },
            detail.PriceLines.Select(line => (line.Category.Name, line.Code, line.CountryId, line.StationAirportId, line.UnitAmount)));
        Assert.Equal((_clock.Now, (DateTimeOffset?)null, (DateTimeOffset?)null, (DateTimeOffset?)null), (detail.CreatedAt, detail.ActivatedAt, detail.SuspendedAt, detail.RetiredAt));
    }

    [Fact]
    public async Task P1_K04_a_provision_without_criteria_round_trips_as_unrestricted()
    {
        await using var scope = new AncillaryScope(_database, _clock);
        var definitionId = await DefinitionAsync(scope);
        var draft = await scope.DefineProvision.DefineAsync(P1Commands.Provision(definitionId, 100, 20m));

        await using var reader = new AncillaryScope(_database, _clock);
        var provision = (await reader.Provisions.GetAsync(draft.Id))!;
        var detail = await reader.GetProvisionById.ExecuteAsync(draft.Id);
        var row = await reader.Query.AncillaryProvisions.AsNoTracking().SingleAsync(readModel => readModel.Id == draft.Id);

        Assert.Empty(provision.Passenger.PassengerTypeCodes);
        Assert.Empty(provision.Sales.CustomerTypes);
        Assert.Empty(provision.Travel.FlightNumbers);
        Assert.Empty(provision.Fare.FareBasisCodes);
        Assert.Empty(provision.RoutePairs);
        Assert.Null(provision.Travel.TravelFrom);
        Assert.Null(provision.Travel.TimeFrom);
        Assert.Null(provision.AdvancePurchase);
        Assert.Null(provision.Application.Baggage);
        Assert.Null(provision.Application.Seat);
        Assert.Empty(detail.Passenger.PassengerTypeCodes);
        Assert.Empty(detail.Travel.RoutePairs);
        Assert.Empty(detail.Travel.DaysOfWeek);
        Assert.Empty(detail.Fare.RbdIds);
        Assert.Null(detail.AdvancePurchase);
        Assert.Null(detail.Baggage);
        Assert.Null(detail.Seat);
        Assert.Equal("Standard", detail.ApplicationType.Name);
        Assert.Empty(row.PointOfSaleIds);
        Assert.Empty(row.SeatNumbers);
        Assert.Null(row.BaggageWeightUnit);
        Assert.Null(row.AdvancePurchaseUnit);
    }

    [Fact]
    public async Task P1_K05_a_draft_update_round_trips_every_field()
    {
        await using var scope = new AncillaryScope(_database, _clock);
        var definitionId = await DefinitionAsync(scope);
        var draft = await scope.DefineProvision.DefineAsync(FullyRestricted(definitionId));

        var seatRule = P1Commands.Provision(
            definitionId,
            20,
            15m,
            P1Commands.Irr,
            coverageScope: ServiceCoverageScope.Sector,
            quantityUnit: AncillaryQuantityUnit.Each,
            passenger: P1Commands.Passengers(PassengerTypeCode.INF),
            sales: new ProvisionSalesCriteriaInput(CustomerIds: [9002, 9003]),
            travel: new ProvisionTravelCriteriaInput(
                RoutePairs: [P1Commands.Pair(P1Commands.Ika, P1Commands.Ist)],
                TravelFrom: new DateOnly(2027, 1, 1),
                MarketingAirlineIds: [4],
                AircraftIds: [737]),
            fare: new ProvisionFareCriteriaInput(RbdIds: [51]),
            application: P1Commands.Seat(["12A", "12B"], ["W"]),
            priceLines: [new ProvisionPriceLineInput(AncillaryPriceLineCategory.Ancillary, "ST", "Seat", 15m, null, P1Commands.Ika)]);

        await using var editor = new AncillaryScope(_database, _clock);
        var changed = await editor.ChangeProvision.ChangeAsync(P1Commands.Change(draft.Id, seatRule));

        Assert.Equal((draft.Id, 20, ProvisionStatus.Draft, ServiceCoverageScope.Sector), (changed.Id, changed.Sequence, changed.Status, changed.CoverageScope));

        await using var reader = new AncillaryScope(_database, _clock);
        var provision = (await reader.Provisions.GetAsync(draft.Id))!;
        var detail = await reader.GetProvisionById.ExecuteAsync(draft.Id);

        Assert.Equal((20, (DateTimeOffset?)null, (DateTimeOffset?)null), (provision.Sequence, provision.SalesEffectiveFrom, provision.SalesDiscontinueAt));
        Assert.Equal(new[] { PassengerTypeCode.INF }, provision.Passenger.PassengerTypeCodes);
        Assert.Empty(provision.Sales.PointOfSaleIds);
        Assert.Equal(new long[] { 9002, 9003 }, provision.Sales.CustomerIds);
        Assert.Empty(provision.Sales.CustomerTypes);
        Assert.Empty(provision.Travel.OriginAirportIds);
        Assert.Empty(provision.Travel.FlightNumbers);
        Assert.Empty(provision.Travel.DaysOfWeek);
        Assert.Equal((new DateOnly(2027, 1, 1), (DateOnly?)null, (TimeOnly?)null), (provision.Travel.TravelFrom!.Value, provision.Travel.TravelTo, provision.Travel.TimeFrom));
        Assert.Equal(new[] { 4 }, provision.Travel.MarketingAirlineIds);
        Assert.Equal(new[] { 737 }, provision.Travel.AircraftIds);
        Assert.Equal((P1Commands.Ika, P1Commands.Ist), provision.RoutePairs.Select(pair => (pair.OriginAirportId, pair.DestinationAirportId)).Single());
        Assert.Empty(provision.Fare.FareFamilyIds);
        Assert.Equal(new long[] { 51 }, provision.Fare.RbdIds);
        Assert.Null(provision.AdvancePurchase);
        Assert.Equal(ProvisionApplicationType.Seat, provision.Application.Type);
        Assert.Null(provision.Application.Baggage);
        Assert.Equal(new[] { "12A", "12B" }, provision.Application.Seat!.SeatNumbers);
        Assert.Equal(new[] { "W" }, provision.Application.Seat.SeatCharacteristicCodes);
        Assert.Equal((AncillaryQuantityUnit.Each, 1, 1), (provision.Quantity.Unit, provision.Quantity.MinQuantity, provision.Quantity.MaxQuantity));
        Assert.Equal(P1Commands.Irr, provision.Fee!.CurrencyId);
        Assert.Equal(("ST", (int?)null, P1Commands.Ika, 15m), provision.PriceLines.Select(line => (line.Code, line.CountryId, line.StationAirportId!.Value, line.UnitAmount)).Single());

        Assert.Equal(("Draft", 20, "Sector", "Seat"), (detail.Status.Name, detail.Sequence, detail.CoverageScope.Name, detail.ApplicationType.Name));
        Assert.Equal(new[] { "INF" }, detail.Passenger.PassengerTypeCodes.Select(code => code.Name));
        Assert.Equal(new long[] { 9002, 9003 }, detail.Sales.CustomerIds);
        Assert.Equal((P1Commands.Ika, P1Commands.Ist, "Directional"), detail.Travel.RoutePairs.Select(pair => (pair.OriginAirportId, pair.DestinationAirportId, pair.Direction.Name)).Single());
        Assert.Equal(new[] { 737 }, detail.Travel.AircraftIds);
        Assert.Empty(detail.Travel.FlightIds);
        Assert.Null(detail.Travel.TimeTo);
        Assert.Equal(new long[] { 51 }, detail.Fare.RbdIds);
        Assert.Empty(detail.Fare.FareBasisCodes);
        Assert.Null(detail.AdvancePurchase);
        Assert.Null(detail.Baggage);
        Assert.Equal(new[] { "12A", "12B" }, detail.Seat!.SeatNumbers);
        Assert.Equal(P1Commands.Irr, detail.FeeCurrencyId);
        Assert.Equal(("ST", "Seat", P1Commands.Ika, 15m), detail.PriceLines.Select(line => (line.Code, line.Name, line.StationAirportId!.Value, line.UnitAmount)).Single());

        Assert.Equal(1, await reader.Command.Set<AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities.ProvisionPriceLine>().CountAsync(line => line.AncillaryProvisionId == draft.Id));
        Assert.Equal(1, await reader.Command.Set<AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities.ProvisionRoutePair>().CountAsync(pair => pair.AncillaryProvisionId == draft.Id));
        Assert.Equal(1, await reader.Query.AncillaryProvisionPriceLines.CountAsync(line => line.AncillaryProvisionId == draft.Id));
        Assert.Equal(1, await reader.Query.AncillaryProvisionRoutePairs.CountAsync(pair => pair.AncillaryProvisionId == draft.Id));

        var free = P1Commands.Provision(definitionId, 30, disposition: CommercialDisposition.Free, advancePurchase: new ProvisionAdvancePurchaseInput(2, TimeUnit.Months));

        await using var secondEditor = new AncillaryScope(_database, _clock);
        await secondEditor.ChangeProvision.ChangeAsync(P1Commands.Change(draft.Id, free));

        await using var secondReader = new AncillaryScope(_database, _clock);
        var freeDetail = await secondReader.GetProvisionById.ExecuteAsync(draft.Id);

        Assert.Equal(("Free", (int?)null, 30, "Standard"), (freeDetail.Disposition.Name, freeDetail.FeeCurrencyId, freeDetail.Sequence, freeDetail.ApplicationType.Name));
        Assert.Null(freeDetail.FeeApplicationUnit);
        Assert.Empty(freeDetail.PriceLines);
        Assert.Empty(freeDetail.Travel.RoutePairs);
        Assert.Empty(freeDetail.Passenger.PassengerTypeCodes);
        Assert.Null(freeDetail.Seat);
        Assert.Equal((2, "Months"), (freeDetail.AdvancePurchase!.Period, freeDetail.AdvancePurchase.Unit.Name));
        Assert.Equal(0, await secondReader.Query.AncillaryProvisionPriceLines.CountAsync(line => line.AncillaryProvisionId == draft.Id));
        Assert.Equal(0, await secondReader.Query.AncillaryProvisionRoutePairs.CountAsync(pair => pair.AncillaryProvisionId == draft.Id));

        await using var thirdEditor = new AncillaryScope(_database, _clock);
        await thirdEditor.ChangeProvision.ChangeAsync(P1Commands.Change(draft.Id, FullyRestricted(definitionId)));

        await using var thirdReader = new AncillaryScope(_database, _clock);
        var restored = await thirdReader.GetProvisionById.ExecuteAsync(draft.Id);

        Assert.Equal(("Paid", "Baggage", 10, 3), (restored.Disposition.Name, restored.ApplicationType.Name, restored.Sequence, restored.PriceLines.Count));
        Assert.Equal(2, restored.Travel.RoutePairs.Count);
        Assert.Equal("OperatingCarrier", restored.Baggage!.RuleDeference!.Name);
        Assert.Equal((3, "Days"), (restored.AdvancePurchase!.Period, restored.AdvancePurchase.Unit.Name));
    }

    [Fact]
    public async Task P1_K05_an_edit_is_refused_once_the_provision_left_draft_or_when_it_is_malformed()
    {
        await using var scope = new AncillaryScope(_database, _clock);
        var definitionId = await DefinitionAsync(scope);
        var command = P1Commands.Provision(definitionId, 10, 30m, passenger: P1Commands.Passengers(PassengerTypeCode.ADT));
        var draft = await scope.DefineProvision.DefineAsync(command);

        await BusinessAssert.ThrowsAsync(16302, 422, () => scope.ChangeProvision.ChangeAsync(
            P1Commands.Change(draft.Id, command with { Sequence = 0 })));
        await BusinessAssert.ThrowsAsync(16302, 422, () => scope.ChangeProvision.ChangeAsync(
            P1Commands.Change(draft.Id, command with { Application = P1Commands.Seat(["12A"], null) })));
        await BusinessAssert.ThrowsAsync(16302, 422, () => scope.ChangeProvision.ChangeAsync(
            P1Commands.Change(draft.Id, command with { Travel = new ProvisionTravelCriteriaInput(RoutePairs: [P1Commands.Pair(P1Commands.Thr, P1Commands.Thr)]) })));
        await BusinessAssert.ThrowsAsync(16301, 404, () => scope.ChangeProvision.ChangeAsync(P1Commands.Change(999_999_999, command)));

        await using var activator = new AncillaryScope(_database, _clock);
        await activator.ActivateProvision.ActivateAsync(new TestActivateProvisionCommand(draft.Id));

        await using var editor = new AncillaryScope(_database, _clock);

        await BusinessAssert.ThrowsAsync(16303, 409, () => editor.ChangeProvision.ChangeAsync(
            P1Commands.Change(draft.Id, command with { Sequence = 20, Passenger = P1Commands.Passengers(PassengerTypeCode.CHD) })));

        await using var reader = new AncillaryScope(_database, _clock);
        var detail = await reader.GetProvisionById.ExecuteAsync(draft.Id);

        Assert.Equal(("Active", 10), (detail.Status.Name, detail.Sequence));
        Assert.Equal(new[] { "ADT" }, detail.Passenger.PassengerTypeCodes.Select(code => code.Name));
        Assert.Equal(30m, detail.PriceLines.Single().UnitAmount);
    }

    [Fact]
    public async Task P1_K06_the_lifecycle_moves_a_provision_through_the_existing_states()
    {
        var start = _clock.Now;
        await using var scope = new AncillaryScope(_database, _clock);
        var definitionId = await DefinitionAsync(scope);
        var draft = await scope.DefineProvision.DefineAsync(FullyRestricted(definitionId));
        var command = new TestProvisionLifecycleCommand(draft.Id);

        await BusinessAssert.ThrowsAsync(16303, 409, () => scope.SuspendProvision.SuspendAsync(command));
        await BusinessAssert.ThrowsAsync(16303, 409, () => scope.ReactivateProvision.ReactivateAsync(command));

        async Task<(string Status, DateTimeOffset? ActivatedAt, DateTimeOffset? SuspendedAt, DateTimeOffset? RetiredAt)> ReadAsync()
        {
            await using var reader = new AncillaryScope(_database, _clock);
            var detail = await reader.GetProvisionById.ExecuteAsync(draft.Id);
            var row = await reader.Query.AncillaryProvisions.AsNoTracking().SingleAsync(provision => provision.Id == draft.Id);

            Assert.Equal(detail.Status.Name, row.Status.ToString());
            Assert.Equal(detail.Status.Name, (await reader.Provisions.GetAsync(draft.Id))!.Status.ToString());
            Assert.Equal(2, detail.Travel.RoutePairs.Count);
            Assert.Equal(3, detail.PriceLines.Count);
            Assert.Equal(new[] { "ADT", "CHD" }, detail.Passenger.PassengerTypeCodes.Select(code => code.Name));

            return (detail.Status.Name, detail.ActivatedAt, detail.SuspendedAt, detail.RetiredAt);
        }

        async Task<ProvisionStatus> ActAsync(Func<AncillaryScope, Task<ProvisionResult>> act)
        {
            _clock.Now = _clock.Now.AddHours(1);
            await using var writer = new AncillaryScope(_database, _clock);

            return (await act(writer)).Status;
        }

        Assert.Equal(ProvisionStatus.Active, await ActAsync(writer => writer.ActivateProvision.ActivateAsync(new TestActivateProvisionCommand(draft.Id))));
        Assert.Equal(("Active", start.AddHours(1), null, null), await ReadAsync());

        Assert.Equal(ProvisionStatus.Suspended, await ActAsync(writer => writer.SuspendProvision.SuspendAsync(command)));
        Assert.Equal(("Suspended", start.AddHours(1), start.AddHours(2), null), await ReadAsync());

        Assert.Equal(ProvisionStatus.Active, await ActAsync(writer => writer.ReactivateProvision.ReactivateAsync(command)));
        Assert.Equal(("Active", start.AddHours(1), null, null), await ReadAsync());

        Assert.Equal(ProvisionStatus.Retired, await ActAsync(writer => writer.RetireProvision.RetireAsync(command)));
        Assert.Equal(("Retired", start.AddHours(1), null, start.AddHours(4)), await ReadAsync());

        await using var late = new AncillaryScope(_database, _clock);

        await BusinessAssert.ThrowsAsync(16303, 409, () => late.RetireProvision.RetireAsync(command));
        await BusinessAssert.ThrowsAsync(16303, 409, () => late.ReactivateProvision.ReactivateAsync(command));
        await BusinessAssert.ThrowsAsync(16303, 409, () => late.ActivateProvision.ActivateAsync(new TestActivateProvisionCommand(draft.Id)));
        await BusinessAssert.ThrowsAsync(16301, 404, () => late.SuspendProvision.SuspendAsync(new TestProvisionLifecycleCommand(999_999_999)));
    }

    [Fact]
    public async Task P1_K06_a_suspended_provision_cannot_be_reactivated_while_its_sequence_has_another_active_row()
    {
        await using var scope = new AncillaryScope(_database, _clock);
        var definitionId = await DefinitionAsync(scope);
        var first = await scope.DefineProvision.DefineAsync(P1Commands.Provision(definitionId, 10, 30m));
        await scope.ActivateProvision.ActivateAsync(new TestActivateProvisionCommand(first.Id));
        await scope.SuspendProvision.SuspendAsync(new TestProvisionLifecycleCommand(first.Id));
        var replacement = await scope.DefineProvision.DefineAsync(P1Commands.Provision(definitionId, 10, 32m));
        await scope.ActivateProvision.ActivateAsync(new TestActivateProvisionCommand(replacement.Id));

        await using var writer = new AncillaryScope(_database, _clock);

        await BusinessAssert.ThrowsAsync(16306, 409, () => writer.ReactivateProvision.ReactivateAsync(new TestProvisionLifecycleCommand(first.Id)));

        await using var reader = new AncillaryScope(_database, _clock);

        Assert.Equal("Suspended", (await reader.GetProvisionById.ExecuteAsync(first.Id)).Status.Name);
        Assert.Equal("Active", (await reader.GetProvisionById.ExecuteAsync(replacement.Id)).Status.Name);

        await reader.RetireProvision.RetireAsync(new TestProvisionLifecycleCommand(replacement.Id));

        await using var retry = new AncillaryScope(_database, _clock);

        Assert.Equal(ProvisionStatus.Active, (await retry.ReactivateProvision.ReactivateAsync(new TestProvisionLifecycleCommand(first.Id))).Status);
    }

    [Fact]
    public async Task P1_K03_provisions_of_a_definition_are_filtered_by_status_coverage_sequence_and_sales_date()
    {
        await using var scope = new AncillaryScope(_database, _clock);
        var definitionId = await DefinitionAsync(scope);
        var sector = await scope.DefineProvision.DefineAsync(P1Commands.Provision(definitionId, 10, 30m));
        var journey = await scope.DefineProvision.DefineAsync(P1Commands.Provision(
            definitionId,
            20,
            25m,
            coverageScope: ServiceCoverageScope.Journey,
            salesEffectiveFrom: _clock.Now.AddDays(10),
            salesDiscontinueAt: _clock.Now.AddDays(20)));
        var free = await scope.DefineProvision.DefineAsync(P1Commands.Provision(definitionId, 30, disposition: CommercialDisposition.Free));
        await scope.ActivateProvision.ActivateAsync(new TestActivateProvisionCommand(sector.Id));
        await scope.ActivateProvision.ActivateAsync(new TestActivateProvisionCommand(journey.Id));
        await scope.SuspendProvision.SuspendAsync(new TestProvisionLifecycleCommand(journey.Id));

        async Task<string[]> IdsAsync(BackofficeGetAncillaryProvisionsPaginatedQuery query)
        {
            query.ServiceDefinitionId = definitionId;

            return (await scope.GetProvisionsPaginated.ExecuteAsync(query)).Results.Select(row => row.Id).ToArray();
        }

        Assert.Equal(new[] { sector.Id.ToString(), journey.Id.ToString(), free.Id.ToString() }, await IdsAsync(new()));
        Assert.Equal(new[] { sector.Id.ToString() }, await IdsAsync(new() { Status = ProvisionStatus.Active }));
        Assert.Equal(new[] { journey.Id.ToString() }, await IdsAsync(new() { Status = ProvisionStatus.Suspended }));
        Assert.Equal(new[] { free.Id.ToString() }, await IdsAsync(new() { Status = ProvisionStatus.Draft }));
        Assert.Equal(new[] { journey.Id.ToString() }, await IdsAsync(new() { CoverageScope = ServiceCoverageScope.Journey }));
        Assert.Equal(new[] { sector.Id.ToString(), free.Id.ToString() }, await IdsAsync(new() { CoverageScope = ServiceCoverageScope.Sector }));
        Assert.Equal(new[] { journey.Id.ToString() }, await IdsAsync(new() { Sequence = 20 }));
        Assert.Equal(new[] { sector.Id.ToString(), free.Id.ToString() }, await IdsAsync(new() { SalesDate = _clock.Now }));
        Assert.Equal(3, (await IdsAsync(new() { SalesDate = _clock.Now.AddDays(15) })).Length);
        Assert.Equal(2, (await IdsAsync(new() { PageSize = 2 })).Length);

        var rows = (await scope.GetProvisionsPaginated.ExecuteAsync(
            new BackofficeGetAncillaryProvisionsPaginatedQuery { ServiceDefinitionId = definitionId })).Results.ToList();

        Assert.Equal(new[] { "30.00", "25.00", null }, rows.Select(row => row.FiledAmount));
        Assert.Equal(new[] { "EUR", "EUR", null }, rows.Select(row => row.Currency));
        Assert.Equal(new[] { "Paid", "Paid", "Free" }, rows.Select(row => row.Disposition.Name));
    }

    [Fact]
    public async Task P1_I03_amounts_are_stored_as_decimal_18_2_and_the_baggage_weight_as_decimal_9_2()
    {
        await using var scope = new AncillaryScope(_database, _clock);

        var columns = await scope.Command.Database
            .SqlQueryRaw<string>(
                "SELECT TABLE_SCHEMA + '.' + TABLE_NAME + '.' + COLUMN_NAME + ' ' + DATA_TYPE + '(' + " +
                "CAST(NUMERIC_PRECISION AS varchar(3)) + ',' + CAST(NUMERIC_SCALE AS varchar(3)) + ')' AS Value " +
                "FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME IN ('UnitAmount', 'BaggageWeight') " +
                "AND TABLE_SCHEMA IN ('Ancillary', 'ReadModel')")
            .ToListAsync();

        Assert.Equal(
            new[]
            {
                "Ancillary.AncillaryProvisions.BaggageWeight decimal(9,2)",
                "Ancillary.ProvisionPriceLines.UnitAmount decimal(18,2)",
                "ReadModel.AncillaryProvisionPriceLines.UnitAmount decimal(18,2)",
                "ReadModel.AncillaryProvisions.BaggageWeight decimal(9,2)"
            },
            columns.OrderBy(column => column, StringComparer.Ordinal));
    }

    [Fact]
    public async Task P1_REQ_criteria_are_typed_columns_in_both_stores_and_no_generic_rule_document_exists()
    {
        await using var scope = new AncillaryScope(_database, _clock);

        var columns = await scope.Command.Database
            .SqlQueryRaw<string>(
                "SELECT TABLE_SCHEMA + '.' + COLUMN_NAME AS Value FROM INFORMATION_SCHEMA.COLUMNS " +
                "WHERE TABLE_NAME = 'AncillaryProvisions' AND TABLE_SCHEMA IN ('Ancillary', 'ReadModel')")
            .ToListAsync();

        string[] typed =
        [
            "PassengerTypeCodes", "PointOfSaleIds", "CustomerIds", "CustomerTypes", "OriginAirportIds", "DestinationAirportIds",
            "ViaAirportIds", "TravelFrom", "TravelTo", "DaysOfWeek", "TimeFrom", "TimeTo", "MarketingAirlineIds",
            "OperatingAirlineIds", "FlightNumbers", "FlightIds", "AircraftIds", "AirFareIds", "AirFareTypes", "FareFamilyIds",
            "FareBasisCodes", "CabinClassIds", "RbdIds", "AdvancePurchasePeriod", "AdvancePurchaseUnit", "ApplicationType",
            "BaggageFreePieces", "BaggageFirstExcessPiece", "BaggageLastExcessPiece", "BaggageWeight", "BaggageWeightUnit",
            "BaggageTravelApplication", "BaggagePurchaseApplication", "BaggageRuleDeference", "SeatNumbers",
            "SeatCharacteristicCodes", "FulfillmentProviderKey"
        ];

        foreach (var schema in new[] { "Ancillary", "ReadModel" })
        foreach (var column in typed)
            Assert.Contains($"{schema}.{column}", columns);

        foreach (var generic in new[] { "Criteria", "Rules", "Expression", "Condition", "Dsl", "Payload", "Json", "Metadata", "Attributes" })
            Assert.DoesNotContain(columns, column => column.Contains(generic, StringComparison.OrdinalIgnoreCase));

        var tables = await scope.Command.Database
            .SqlQueryRaw<string>(
                "SELECT TABLE_SCHEMA + '.' + TABLE_NAME AS Value FROM INFORMATION_SCHEMA.TABLES " +
                "WHERE TABLE_SCHEMA IN ('Ancillary', 'ReadModel')")
            .ToListAsync();

        Assert.Equal(
            new[]
            {
                "Ancillary.AncillaryProvisions", "Ancillary.ProvisionPriceLines", "Ancillary.ProvisionRoutePairs",
                "ReadModel.AncillaryProvisionPriceLines", "ReadModel.AncillaryProvisionRoutePairs", "ReadModel.AncillaryProvisions"
            },
            tables.Where(table => table.Contains("Provision", StringComparison.Ordinal)).OrderBy(table => table, StringComparer.Ordinal));

        foreach (var generic in new[] { "Criteria", "Criterion", "Rule", "Attribute", "Metadata", "Matrix", "StockPool", "Evaluation" })
            Assert.DoesNotContain(tables, table => table.Contains(generic, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void P1_REQ_the_backoffice_validators_check_the_shape_of_the_criteria_inputs()
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
                    command.Fee,
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
                    command.Fee,
                    command.Settlement,
                    command.Availability,
                    command.Fulfillment))
                .Errors.Select(error => error.PropertyName).ToArray();

        (string Property, TestDefineProvisionCommand Command)[] malformed =
        [
            ("Passenger.PassengerTypeCodes[0]", template with { Passenger = new([(PassengerTypeCode)(-1)]) }),
            ("Sales.CustomerTypes[0]", template with { Sales = new(CustomerTypes: [(CustomerType)99]) }),
            ("Travel.DaysOfWeek[0]", template with { Travel = new(DaysOfWeek: [(DayOfWeek)9]) }),
            ("Travel.RoutePairs[0].Direction", template with { Travel = new(RoutePairs: [new(1, 6, (RoutePairDirection)9)]) }),
            ("Travel.FlightNumbers[0]", template with { Travel = new(FlightNumbers: [new string('9', 17)]) }),
            ("Fare.AirFareTypes[0]", template with { Fare = new(AirFareTypes: [(AirFareType)99]) }),
            ("Fare.FareBasisCodes[0]", template with { Fare = new(FareBasisCodes: [new string('Y', 65)]) }),
            ("AdvancePurchase.Unit", template with { AdvancePurchase = new(3, (TimeUnit)99) }),
            ("Application.Type", template with { Application = new((ProvisionApplicationType)9) }),
            ("Application.Baggage.WeightUnit", template with { Application = P1Commands.Baggage(20m, weightUnit: (WeightUnit)9) }),
            ("Application.Baggage.TravelApplication", template with { Application = P1Commands.Baggage(20m, travelApplication: (BaggageTravelApplication)9) }),
            ("Application.Seat.SeatNumbers[0]", template with { Application = P1Commands.Seat([new string('1', 17)], null) }),
            ("Application.Seat.SeatCharacteristicCodes[0]", template with { Application = P1Commands.Seat(null, [new string('W', 26)]) }),
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
    }
}
