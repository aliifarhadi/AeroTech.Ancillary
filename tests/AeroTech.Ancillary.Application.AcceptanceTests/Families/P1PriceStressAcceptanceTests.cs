using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;
using AeroTech.Messages.Core.Enums;
using Xunit;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.P1Commands;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Families;

[Collection(DatabaseCollection.Name)]
public class P1PriceStressAcceptanceTests
{
    private readonly TestDatabase _database;
    private readonly FixedClock _clock = new();
    private readonly FamilyProof _proof;

    public P1PriceStressAcceptanceTests(TestDatabase database)
    {
        _database = database;
        _proof = new FamilyProof(database, _clock);
    }

    private static decimal? Amount(BackofficeProvisionDto provision)
        => provision.PriceLines.Count == 0 ? null : provision.PriceLines.Sum(line => line.UnitAmount);

    private static string[] Passengers(BackofficeProvisionDto provision)
        => provision.Passenger.PassengerTypeCodes.Select(code => code.Name).ToArray();

    [Fact]
    public async Task P1_PS1_D01_D02_lounge_is_25_eur_for_an_adult_15_eur_for_a_child_and_not_available_for_an_infant()
    {
        var airlineId = _database.NextAirlineId();
        var supplierId = await _proof.SupplierAsync(M1Commands.LocalSupplier(airlineId));
        var lounge = await _proof.DefinitionAsync(M1Commands.LoungeDefinition(airlineId, supplierId), activate: true);

        var adult = await _proof.ProvisionAsync(Provision(lounge.Id, 10, 25m, passenger: P1Commands.Passengers(PassengerTypeCode.ADT)), activate: true);
        var child = await _proof.ProvisionAsync(Provision(lounge.Id, 20, 15m, passenger: P1Commands.Passengers(PassengerTypeCode.CHD)), activate: true);
        var infant = await _proof.ProvisionAsync(
            Provision(lounge.Id, 30, disposition: CommercialDisposition.NotAvailable, passenger: P1Commands.Passengers(PassengerTypeCode.INF)),
            activate: true);

        Assert.All(new[] { adult, child, infant }, provision => Assert.Equal((lounge.Id, "Active"), (provision.ServiceDefinitionId, provision.Status.Name)));
        Assert.Equal((new[] { "ADT" }.Single(), 25m, (int?)Eur, "Paid"), (Passengers(adult).Single(), Amount(adult), adult.FeeCurrencyId, adult.Disposition.Name));
        Assert.Equal((new[] { "CHD" }.Single(), 15m, (int?)Eur, "Paid"), (Passengers(child).Single(), Amount(child), child.FeeCurrencyId, child.Disposition.Name));
        Assert.Equal(("INF", (decimal?)null, (int?)null, "NotAvailable"), (Passengers(infant).Single(), Amount(infant), infant.FeeCurrencyId, infant.Disposition.Name));

        var listed = await _proof.ListedProvisionsAsync(lounge.Id);

        Assert.Equal(new[] { 10, 20, 30 }, listed.Select(row => row.Sequence));
        Assert.Equal(new[] { "25.00", "15.00", null }, listed.Select(row => row.FiledAmount));
        Assert.Equal(new[] { "EUR", "EUR", null }, listed.Select(row => row.Currency));
        Assert.DoesNotContain(
            typeof(BackofficeProvisionDto).GetProperties(),
            property => property.Name.Contains("Adult", StringComparison.Ordinal)
                        || property.Name.Contains("Child", StringComparison.Ordinal)
                        || property.Name.Contains("Infant", StringComparison.Ordinal));
    }

    [Fact]
    public async Task P1_PS1_D02_an_infant_can_instead_be_free_through_its_own_provision()
    {
        var airlineId = _database.NextAirlineId();
        var supplierId = await _proof.SupplierAsync(M1Commands.LocalSupplier(airlineId));
        var lounge = await _proof.DefinitionAsync(M1Commands.LoungeDefinition(airlineId, supplierId), activate: true);

        var infant = await _proof.ProvisionAsync(
            Provision(lounge.Id, 30, disposition: CommercialDisposition.Free, passenger: P1Commands.Passengers(PassengerTypeCode.INF)),
            activate: true);

        Assert.Equal(("Free", "Active", (decimal?)null), (infant.Disposition.Name, infant.Status.Name, Amount(infant)));
        Assert.Equal(new[] { "INF" }, Passengers(infant));
    }

    [Fact]
    public async Task P1_PS2_baggage_has_a_flight_and_date_rule_a_route_and_fare_family_rule_and_a_default_rule()
    {
        var airlineId = _database.NextAirlineId();
        var supplierId = await _proof.SupplierAsync(M1Commands.LocalSupplier(airlineId));
        var bag = await _proof.DefinitionAsync(
            CarrierDefinition(airlineId, supplierId, "XBAG_20KG", "XK2", "C", "BG", "Extra bag 20KG", Ssr("XBAG")),
            activate: true);

        var selectedFlight = await _proof.ProvisionAsync(
            Provision(
                bag.Id,
                10,
                30m,
                travel: new ProvisionTravelCriteriaInput(
                    FlightIds: [81234],
                    TravelFrom: new DateOnly(2026, 12, 20),
                    TravelTo: new DateOnly(2026, 12, 31)),
                application: Baggage(20m)),
            activate: true);
        var routeAndFareFamily = await _proof.ProvisionAsync(
            Provision(
                bag.Id,
                20,
                25m,
                travel: new ProvisionTravelCriteriaInput(RoutePairs: [Pair(Thr, Ist)]),
                fare: new ProvisionFareCriteriaInput(FareFamilyIds: [5]),
                application: Baggage(20m)),
            activate: true);
        var broadDefault = await _proof.ProvisionAsync(Provision(bag.Id, 100, 20m, application: Baggage(20m)), activate: true);

        Assert.Equal((10, 30m), (selectedFlight.Sequence, Amount(selectedFlight)));
        Assert.Equal(new long[] { 81234 }, selectedFlight.Travel.FlightIds);
        Assert.Equal((new DateOnly(2026, 12, 20), new DateOnly(2026, 12, 31)), (selectedFlight.Travel.TravelFrom!.Value, selectedFlight.Travel.TravelTo!.Value));
        Assert.Empty(selectedFlight.Travel.RoutePairs);
        Assert.Empty(selectedFlight.Fare.FareFamilyIds);

        Assert.Equal((20, 25m), (routeAndFareFamily.Sequence, Amount(routeAndFareFamily)));
        Assert.Equal((Thr, Ist, "Directional"), routeAndFareFamily.Travel.RoutePairs.Select(pair => (pair.OriginAirportId, pair.DestinationAirportId, pair.Direction.Name)).Single());
        Assert.Equal(new long[] { 5 }, routeAndFareFamily.Fare.FareFamilyIds);
        Assert.Empty(routeAndFareFamily.Travel.FlightIds);
        Assert.Null(routeAndFareFamily.Travel.TravelFrom);

        Assert.Equal((100, 20m), (broadDefault.Sequence, Amount(broadDefault)));
        Assert.Empty(broadDefault.Travel.FlightIds);
        Assert.Empty(broadDefault.Travel.RoutePairs);
        Assert.Empty(broadDefault.Fare.FareFamilyIds);
        Assert.Empty(broadDefault.Passenger.PassengerTypeCodes);

        var listed = await _proof.ListedProvisionsAsync(bag.Id);

        Assert.Equal(new[] { (10, "30.00", "EUR", "Active"), (20, "25.00", "EUR", "Active"), (100, "20.00", "EUR", "Active") },
            listed.Select(row => (row.Sequence, row.FiledAmount!, row.Currency!, row.Status.Name)));
    }

    [Fact]
    public async Task P1_PS3_B01_two_lounge_suppliers_have_distinct_definition_ids_and_prices()
    {
        var airlineId = _database.NextAirlineId();
        var airlineSupplierId = await _proof.SupplierAsync(M1Commands.LocalSupplier(airlineId, "Dot Air"));
        var partnerSupplierId = await _proof.SupplierAsync(M1Commands.LocalSupplier(airlineId, "IKA CIP Lounge"));
        var own = await _proof.DefinitionAsync(M1Commands.LoungeDefinition(airlineId, airlineSupplierId, "LNG_DOTAIR"), activate: true);
        var partner = await _proof.DefinitionAsync(M1Commands.LoungeDefinition(airlineId, partnerSupplierId, "LNG_CIP"), activate: true);

        var ownPrice = await _proof.ProvisionAsync(Provision(own.Id, 100, 25m), activate: true);
        var partnerPrice = await _proof.ProvisionAsync(Provision(partner.Id, 100, 45m), activate: true);

        Assert.NotEqual(own.Id, partner.Id);
        Assert.NotEqual(own.SupplierId, partner.SupplierId);
        Assert.Equal(("0BX", "0BX"), (own.ServiceSubCode, partner.ServiceSubCode));
        Assert.Equal(("Dot Air", "IKA CIP Lounge"), (own.SupplierName, partner.SupplierName));
        Assert.Equal((own.Id, 25m), (ownPrice.ServiceDefinitionId, Amount(ownPrice)));
        Assert.Equal((partner.Id, 45m), (partnerPrice.ServiceDefinitionId, Amount(partnerPrice)));
        Assert.Equal("25.00", (await _proof.ListedProvisionsAsync(own.Id)).Single().FiledAmount);
        Assert.Equal("45.00", (await _proof.ListedProvisionsAsync(partner.Id)).Single().FiledAmount);
    }

    [Fact]
    public async Task P1_PS4_a_meal_price_differs_by_passenger_type_and_cabin()
    {
        var airlineId = _database.NextAirlineId();
        var supplierId = await _proof.SupplierAsync(M1Commands.LocalSupplier(airlineId, "Sky Catering"));
        var meal = await _proof.DefinitionAsync(
            CarrierDefinition(airlineId, supplierId, "MEAL_HOT", "MHT", "F", "ML", "Hot meal", Ssr("SPML")),
            activate: true);

        (int Sequence, PassengerTypeCode Passenger, int Cabin, decimal Price)[] matrix =
        [
            (10, PassengerTypeCode.ADT, 1, 18m),
            (20, PassengerTypeCode.ADT, 2, 12m),
            (30, PassengerTypeCode.CHD, 1, 11m),
            (40, PassengerTypeCode.CHD, 2, 8m)
        ];

        foreach (var row in matrix)
        {
            var authored = await _proof.ProvisionAsync(
                Provision(
                    meal.Id,
                    row.Sequence,
                    row.Price,
                    passenger: P1Commands.Passengers(row.Passenger),
                    fare: new ProvisionFareCriteriaInput(CabinClassIds: [row.Cabin])),
                activate: true);

            Assert.Equal((row.Passenger.ToString(), row.Cabin, (decimal?)row.Price), (Passengers(authored).Single(), authored.Fare.CabinClassIds.Single(), Amount(authored)));
        }

        Assert.Equal(
            new[] { "18.00", "12.00", "11.00", "8.00" },
            (await _proof.ListedProvisionsAsync(meal.Id)).Select(row => row.FiledAmount));
    }

    [Fact]
    public async Task P1_PS5_an_insurance_plan_price_differs_by_passenger_type_and_travel_window()
    {
        var airlineId = _database.NextAirlineId();
        var insurerId = await _proof.SupplierAsync(M1Commands.ExternalSupplier(airlineId, "InsurancePartnerA"));
        var plan = await _proof.DefinitionAsync(
            CarrierDefinition(airlineId, insurerId, "INS_BASIC", "INB", "M", "IN", "Travel insurance basic"),
            activate: true);

        var winter = (From: new DateOnly(2026, 12, 15), To: new DateOnly(2027, 1, 15));
        var spring = (From: new DateOnly(2027, 3, 15), To: new DateOnly(2027, 4, 5));

        (int Sequence, PassengerTypeCode Passenger, (DateOnly From, DateOnly To) Window, decimal Price)[] matrix =
        [
            (10, PassengerTypeCode.ADT, winter, 14m),
            (20, PassengerTypeCode.CHD, winter, 9m),
            (30, PassengerTypeCode.ADT, spring, 12m),
            (40, PassengerTypeCode.CHD, spring, 7m)
        ];

        foreach (var row in matrix)
        {
            var authored = await _proof.ProvisionAsync(
                Provision(
                    plan.Id,
                    row.Sequence,
                    row.Price,
                    coverageScope: ServiceCoverageScope.Journey,
                    passenger: P1Commands.Passengers(row.Passenger),
                    travel: new ProvisionTravelCriteriaInput(TravelFrom: row.Window.From, TravelTo: row.Window.To)),
                activate: true);

            Assert.Equal(
                (row.Passenger.ToString(), row.Window.From, row.Window.To, (decimal?)row.Price),
                (Passengers(authored).Single(), authored.Travel.TravelFrom!.Value, authored.Travel.TravelTo!.Value, Amount(authored)));
        }

        var broad = await _proof.ProvisionAsync(Provision(plan.Id, 100, 10m, coverageScope: ServiceCoverageScope.Journey), activate: true);

        Assert.Null(broad.Travel.TravelFrom);
        Assert.Equal(5, (await _proof.ListedProvisionsAsync(plan.Id)).Count);
    }

    [Fact]
    public async Task P1_PS6_a_seat_rule_differs_by_aircraft_and_characteristic()
    {
        var airlineId = _database.NextAirlineId();
        var supplierId = await _proof.SupplierAsync(M1Commands.LocalSupplier(airlineId));
        var seat = await _proof.DefinitionAsync(
            CarrierDefinition(airlineId, supplierId, "SEAT_SELECT", "STS", "F", "ST", "Seat selection"),
            activate: true);

        (int Sequence, int Aircraft, string Characteristic, decimal Price)[] matrix =
        [
            (10, 320, "LS", 22m),
            (20, 320, "W", 9m),
            (30, 321, "LS", 28m),
            (40, 321, "W", 11m)
        ];

        foreach (var row in matrix)
        {
            var authored = await _proof.ProvisionAsync(
                Provision(
                    seat.Id,
                    row.Sequence,
                    row.Price,
                    travel: new ProvisionTravelCriteriaInput(AircraftIds: [row.Aircraft]),
                    application: Seat(null, [row.Characteristic])),
                activate: true);

            Assert.Equal(
                ("Seat", row.Aircraft, row.Characteristic, (decimal?)row.Price),
                (authored.ApplicationType.Name, authored.Travel.AircraftIds.Single(), authored.Seat!.SeatCharacteristicCodes.Single(), Amount(authored)));
        }

        var exitRow = await _proof.ProvisionAsync(
            Provision(seat.Id, 5, 35m, travel: new ProvisionTravelCriteriaInput(AircraftIds: [321]), application: Seat(["12A", "12F"], null)),
            activate: true);

        Assert.Equal(new[] { "12A", "12F" }, exitRow.Seat!.SeatNumbers);
        Assert.Equal(
            new[] { (5, "35.00"), (10, "22.00"), (20, "9.00"), (30, "28.00"), (40, "11.00") },
            (await _proof.ListedProvisionsAsync(seat.Id)).Select(row => (row.Sequence, row.FiledAmount!)));
    }

    [Fact]
    public async Task P1_REQ_different_travel_dates_flights_routes_fare_families_cabins_and_rbds_carry_their_own_filed_price()
    {
        var airlineId = _database.NextAirlineId();
        var supplierId = await _proof.SupplierAsync(M1Commands.LocalSupplier(airlineId));
        var lounge = await _proof.DefinitionAsync(M1Commands.LoungeDefinition(airlineId, supplierId), activate: true);

        (int Sequence, decimal Price, ProvisionTravelCriteriaInput? Travel, ProvisionFareCriteriaInput? Fare)[] matrix =
        [
            (10, 31m, new ProvisionTravelCriteriaInput(TravelFrom: new DateOnly(2026, 12, 20), TravelTo: new DateOnly(2026, 12, 31)), null),
            (20, 32m, new ProvisionTravelCriteriaInput(TravelFrom: new DateOnly(2027, 1, 1), TravelTo: new DateOnly(2027, 1, 31)), null),
            (30, 33m, new ProvisionTravelCriteriaInput(FlightIds: [81234]), null),
            (40, 34m, new ProvisionTravelCriteriaInput(FlightIds: [81240]), null),
            (50, 35m, new ProvisionTravelCriteriaInput(RoutePairs: [Pair(Ika, Ist)]), null),
            (60, 36m, new ProvisionTravelCriteriaInput(RoutePairs: [Pair(Ika, Mhd)]), null),
            (70, 37m, null, new ProvisionFareCriteriaInput(FareFamilyIds: [5])),
            (80, 38m, null, new ProvisionFareCriteriaInput(FareFamilyIds: [6])),
            (90, 39m, null, new ProvisionFareCriteriaInput(CabinClassIds: [1])),
            (100, 40m, null, new ProvisionFareCriteriaInput(CabinClassIds: [2])),
            (110, 41m, null, new ProvisionFareCriteriaInput(RbdIds: [41])),
            (120, 42m, null, new ProvisionFareCriteriaInput(RbdIds: [42]))
        ];

        var authored = new List<BackofficeProvisionDto>();

        foreach (var row in matrix)
            authored.Add(await _proof.ProvisionAsync(Provision(lounge.Id, row.Sequence, row.Price, travel: row.Travel, fare: row.Fare), activate: true));

        Assert.All(authored, provision => Assert.Equal("Active", provision.Status.Name));
        Assert.Equal(matrix.Select(row => (decimal?)row.Price), authored.Select(Amount));
        Assert.Equal((new DateOnly(2026, 12, 20), new DateOnly(2027, 1, 1)), (authored[0].Travel.TravelFrom!.Value, authored[1].Travel.TravelFrom!.Value));
        Assert.Equal((81234L, 81240L), (authored[2].Travel.FlightIds.Single(), authored[3].Travel.FlightIds.Single()));
        Assert.Equal((Ist, Mhd), (authored[4].Travel.RoutePairs.Single().DestinationAirportId, authored[5].Travel.RoutePairs.Single().DestinationAirportId));
        Assert.Equal((5L, 6L), (authored[6].Fare.FareFamilyIds.Single(), authored[7].Fare.FareFamilyIds.Single()));
        Assert.Equal((1, 2), (authored[8].Fare.CabinClassIds.Single(), authored[9].Fare.CabinClassIds.Single()));
        Assert.Equal((41L, 42L), (authored[10].Fare.RbdIds.Single(), authored[11].Fare.RbdIds.Single()));
        Assert.Equal(
            matrix.Select(row => (row.Sequence, $"{row.Price:0.00}")),
            (await _proof.ListedProvisionsAsync(lounge.Id)).Select(row => (row.Sequence, row.FiledAmount!)));
    }

    [Fact]
    public async Task P1_REQ_different_points_of_sale_customers_and_customer_types_carry_their_own_filed_price_and_currency()
    {
        var airlineId = _database.NextAirlineId();
        var supplierId = await _proof.SupplierAsync(M1Commands.LocalSupplier(airlineId));
        var lounge = await _proof.DefinitionAsync(M1Commands.LoungeDefinition(airlineId, supplierId), activate: true);

        var tehranOffices = await _proof.ProvisionAsync(
            Provision(lounge.Id, 10, 15000000m, Irr, sales: new ProvisionSalesCriteriaInput(PointOfSaleIds: [501, 502])),
            activate: true);
        var namedCustomer = await _proof.ProvisionAsync(
            Provision(lounge.Id, 20, 18m, sales: new ProvisionSalesCriteriaInput(CustomerIds: [9001])),
            activate: true);
        var agencies = await _proof.ProvisionAsync(
            Provision(lounge.Id, 30, 22m, sales: new ProvisionSalesCriteriaInput(CustomerTypes: [CustomerType.TravelAgency])),
            activate: true);

        Assert.Equal(((int?)Irr, 15000000m), (tehranOffices.FeeCurrencyId, Amount(tehranOffices)));
        Assert.Equal(new long[] { 501, 502 }, tehranOffices.Sales.PointOfSaleIds);
        Assert.Equal(((int?)Eur, 18m), (namedCustomer.FeeCurrencyId, Amount(namedCustomer)));
        Assert.Equal(new long[] { 9001 }, namedCustomer.Sales.CustomerIds);
        Assert.Equal(new[] { "TravelAgency" }, agencies.Sales.CustomerTypes.Select(type => type.Name));
        Assert.Equal(
            new[] { ("15,000,000.00", "IRR"), ("18.00", "EUR"), ("22.00", "EUR") },
            (await _proof.ListedProvisionsAsync(lounge.Id)).Select(row => (row.FiledAmount!, row.Currency!)));
    }
}
