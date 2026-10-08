using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;
using AeroTech.Ancillary.Query.AncillaryPricingAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryPricingAggregate.Queries.GetAncillaryPricingsPaginated.Backoffice;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;
using AeroTech.Ancillary.Query._Shared.Enums;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;
using AeroTech.Messages.Core.Enums;
using System.Globalization;
using Xunit;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.P1Commands;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.V12Commands;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Families;

[Collection(DatabaseCollection.Name)]
public class V12PriceStressAcceptanceTests
{
    private readonly TestDatabase _database;
    private readonly FixedClock _clock = new();
    private readonly FamilyProof _proof;

    public V12PriceStressAcceptanceTests(TestDatabase database)
    {
        _database = database;
        _proof = new FamilyProof(database, _clock);
    }

    private const long AirlineOffice = 9200000000000001;
    private const long AgencyOffice = 1551571720488353792;

    private static DateOnly Date(int year, int month, int day) => new(year, month, day);

    private static TValue[] Values<TValue>(IReadOnlyList<BackofficeProvisionConditionRowDto<TValue>> rows)
        => rows.Select(row => row.Value).ToArray();

    private static string[] Names(IReadOnlyList<BackofficeProvisionConditionRowDto<EnumValueDto>> rows)
        => rows.Select(row => row.Value.Name).ToArray();

    private static (DateOnly Start, DateOnly End)[] Periods(IReadOnlyList<BackofficeProvisionDatePeriodDto> rows)
        => rows.Select(row => (row.StartDate, row.EndDate)).ToArray();

    private static (string Day, int? From, int? To, string Effect)[] DayTimes(BackofficeProvisionDto provision)
        => provision.Travel.DayTimeRestrictions
            .Select(row => (row.DayOfWeek.Name, row.StartTime?.Hour, row.EndTime?.Hour, row.Effect.Name))
            .ToArray();

    private static (string? Passenger, int? AgeFrom, int? AgeTo, decimal Base, decimal Total)[] Rates(BackofficePricingDto pricing)
        => pricing.Rates
            .Select(rate => (rate.PassengerTypeCode?.Name, rate.AgeFromInclusive, rate.AgeToExclusive, rate.BaseAmount, rate.TotalAmount))
            .ToArray();

    private Task<PublishedRule> PricedAsync(TestDefineProvisionCommand provision, int currencyId, params PricingLineInput[] priceLines)
        => _proof.RuleAsync(provision, provisionId => Pricing(provisionId, currencyId, priceLines));

    private async Task<(int Sequence, string Filed)[]> FiledAsync(long serviceDefinitionId)
    {
        var filed = new List<(int, string)>();

        await using var reader = new AncillaryScope(_database, _clock);

        foreach (var provision in await _proof.ListedProvisionsAsync(serviceDefinitionId))
        {
            var active = (await reader.GetPricingsPaginated.ExecuteAsync(
                    new BackofficeGetAncillaryPricingsPaginatedQuery
                    {
                        AncillaryProvisionId = long.Parse(provision.Id, CultureInfo.InvariantCulture),
                        Status = PricingStatus.Active
                    }))
                .Results.SingleOrDefault();

            if (active is null)
            {
                filed.Add((provision.Sequence, provision.Disposition.Name));
                continue;
            }

            var pricing = await reader.GetPricingById.ExecuteAsync(long.Parse(active.Id, CultureInfo.InvariantCulture));

            filed.Add((
                provision.Sequence,
                $"{pricing.Currency} {string.Join("/", pricing.Rates.Select(rate => rate.TotalAmount.ToString("0.00", CultureInfo.InvariantCulture)))}"));
        }

        return filed.ToArray();
    }

    [Fact]
    public async Task V12_PS1_lounge_is_25_eur_for_an_adult_15_eur_for_a_child_and_not_available_or_free_for_an_infant()
    {
        var airlineId = _database.NextAirlineId();
        var supplierId = await _proof.SupplierAsync(M1Commands.LocalSupplier(airlineId));
        var lounge = await _proof.DefinitionAsync(M1Commands.LoungeDefinition(airlineId, supplierId));

        var priced = await PricedAsync(
            Provision(lounge.Id, 10, passenger: Passengers(PassengerTypeCode.ADT, PassengerTypeCode.CHD)),
            Eur,
            Base(25m, PassengerTypeCode.ADT),
            Base(15m, PassengerTypeCode.CHD));
        var infant = await _proof.RuleAsync(
            Provision(lounge.Id, 30, CommercialDisposition.NotAvailable, passenger: Passengers(PassengerTypeCode.INF)));

        Assert.All(new[] { priced.Provision, infant.Provision }, provision => Assert.Equal((lounge.Id, "Active"), (provision.ServiceDefinitionId, provision.Status.Name)));
        Assert.Equal(new (string?, int?, int?, decimal, decimal)[] { ("ADT", null, null, 25m, 25m), ("CHD", null, null, 15m, 15m) }, Rates(priced.Pricing!));
        Assert.Equal((Eur, "EUR", "Active"), (priced.Pricing!.CurrencyId, priced.Pricing.Currency, priced.Pricing.Status.Name));
        Assert.Equal(("NotAvailable", new[] { "INF" }.Single()), (infant.Provision.Disposition.Name, Names(infant.Provision.Passenger.PassengerTypes).Single()));
        Assert.Null(infant.Pricing);
        Assert.Equal(new[] { (10, "EUR 25.00/15.00"), (30, "NotAvailable") }, await FiledAsync(lounge.Id));

        var otherAirline = _database.NextAirlineId();
        var otherSupplier = await _proof.SupplierAsync(M1Commands.LocalSupplier(otherAirline));
        var otherLounge = await _proof.DefinitionAsync(M1Commands.LoungeDefinition(otherAirline, otherSupplier));
        var freeInfant = await _proof.RuleAsync(
            Provision(otherLounge.Id, 30, CommercialDisposition.Free, passenger: Passengers(PassengerTypeCode.INF)));

        Assert.Equal(("Free", "Active"), (freeInfant.Provision.Disposition.Name, freeInfant.Provision.Status.Name));
        Assert.Equal(new[] { (30, "Free") }, await FiledAsync(otherLounge.Id));
        Assert.DoesNotContain(
            typeof(BackofficeProvisionDto).GetProperties().Concat(typeof(BackofficePricingDto).GetProperties()).Concat(typeof(BackofficePricingLineDto).GetProperties()),
            property => property.Name.Contains("Adult", StringComparison.Ordinal)
                        || property.Name.Contains("Child", StringComparison.Ordinal)
                        || property.Name.Contains("Infant", StringComparison.Ordinal));
    }

    [Fact]
    public async Task V12_PS2_baggage_has_a_flight_and_date_rule_a_route_and_fare_family_rule_and_a_default_rule_each_with_its_own_price()
    {
        var airlineId = _database.NextAirlineId();
        var supplierId = await _proof.SupplierAsync(M1Commands.LocalSupplier(airlineId));
        var bag = await _proof.DefinitionAsync(
            CarrierDefinition(airlineId, supplierId, "XBAG_20KG", "XK2", "C", "BG", "Extra bag 20KG", Ssr("XBAG"), pricingUnit: PricingUnit.PerPiece));

        var selectedFlight = await PricedAsync(
            Provision(
                bag.Id,
                10,
                travel: new ProvisionTravelCriteriaInput(SeasonalPeriods: [Period(Date(2026, 12, 20), Date(2026, 12, 31))], FlightIds: [81234]),
                application: Baggage(20m)),
            Eur,
            Base(30m));
        var routeAndFareFamily = await PricedAsync(
            Provision(
                bag.Id,
                20,
                travel: new ProvisionTravelCriteriaInput(RoutePairs: [Pair(Thr, Ist)]),
                fare: new ProvisionFareCriteriaInput(FareFamilyIds: [5]),
                application: Baggage(20m)),
            Eur,
            Base(25m));
        var broadDefault = await PricedAsync(Provision(bag.Id, 100, application: Baggage(20m)), Eur, Base(20m));

        Assert.Equal(new long[] { 81234 }, Values(selectedFlight.Provision.Travel.Flights));
        Assert.Equal(new[] { (Date(2026, 12, 20), Date(2026, 12, 31)) }, Periods(selectedFlight.Provision.Travel.SeasonalPeriods));
        Assert.Empty(selectedFlight.Provision.Travel.RoutePairs);
        Assert.Empty(selectedFlight.Provision.Fare.FareFamilies);
        Assert.Equal((Thr, Ist, "Directional"), routeAndFareFamily.Provision.Travel.RoutePairs.Select(pair => (pair.OriginAirportId, pair.DestinationAirportId, pair.Direction.Name)).Single());
        Assert.Equal(new long[] { 5 }, Values(routeAndFareFamily.Provision.Fare.FareFamilies));
        Assert.Empty(routeAndFareFamily.Provision.Travel.Flights);
        Assert.Empty(routeAndFareFamily.Provision.Travel.SeasonalPeriods);
        Assert.Empty(broadDefault.Provision.Travel.Flights);
        Assert.Empty(broadDefault.Provision.Travel.RoutePairs);
        Assert.Empty(broadDefault.Provision.Fare.FareFamilies);
        Assert.Empty(broadDefault.Provision.Passenger.PassengerTypes);
        Assert.Equal(new[] { (10, "EUR 30.00"), (20, "EUR 25.00"), (100, "EUR 20.00") }, await FiledAsync(bag.Id));
    }

    [Fact]
    public async Task V12_PS3_two_lounge_suppliers_have_distinct_definition_ids_and_prices()
    {
        var airlineId = _database.NextAirlineId();
        var airlineSupplierId = await _proof.SupplierAsync(M1Commands.LocalSupplier(airlineId, "Dot Air"));
        var partnerSupplierId = await _proof.SupplierAsync(M1Commands.LocalSupplier(airlineId, "IKA CIP Lounge"));
        var own = await _proof.DefinitionAsync(M1Commands.LoungeDefinition(airlineId, airlineSupplierId, "LNG_DOTAIR"));
        var partner = await _proof.DefinitionAsync(M1Commands.LoungeDefinition(airlineId, partnerSupplierId, "LNG_CIP"));

        var ownPrice = await PricedAsync(Provision(own.Id, 100), Eur, Base(25m));
        var partnerPrice = await PricedAsync(Provision(partner.Id, 100), Eur, Base(45m));

        Assert.NotEqual(own.Id, partner.Id);
        Assert.NotEqual(own.SupplierId, partner.SupplierId);
        Assert.NotEqual(ownPrice.Pricing!.Id, partnerPrice.Pricing!.Id);
        Assert.Equal(("0BX", "0BX"), (own.ServiceSubCode, partner.ServiceSubCode));
        Assert.Equal(("Dot Air", "IKA CIP Lounge"), (own.SupplierName, partner.SupplierName));
        Assert.Equal((own.Id, ownPrice.Provision.Id), (ownPrice.Provision.ServiceDefinitionId, ownPrice.Pricing.AncillaryProvisionId));
        Assert.Equal((partner.Id, partnerPrice.Provision.Id), (partnerPrice.Provision.ServiceDefinitionId, partnerPrice.Pricing.AncillaryProvisionId));
        Assert.Equal(new[] { (100, "EUR 25.00") }, await FiledAsync(own.Id));
        Assert.Equal(new[] { (100, "EUR 45.00") }, await FiledAsync(partner.Id));
    }

    [Fact]
    public async Task V12_PS4_a_meal_price_differs_by_passenger_type_and_cabin()
    {
        var airlineId = _database.NextAirlineId();
        var supplierId = await _proof.SupplierAsync(M1Commands.LocalSupplier(airlineId, "Sky Catering"));
        var meal = await _proof.DefinitionAsync(CarrierDefinition(airlineId, supplierId, "MEAL_HOT", "MHT", "F", "ML", "Hot meal", Ssr("SPML")));

        (int Sequence, int Cabin, decimal Adult, decimal Child)[] matrix = [(10, 1, 18m, 11m), (20, 2, 12m, 8m)];

        foreach (var row in matrix)
        {
            var authored = await PricedAsync(
                Provision(
                    meal.Id,
                    row.Sequence,
                    passenger: Passengers(PassengerTypeCode.ADT, PassengerTypeCode.CHD),
                    fare: new ProvisionFareCriteriaInput(CabinClassIds: [row.Cabin])),
                Eur,
                Base(row.Adult, PassengerTypeCode.ADT),
                Base(row.Child, PassengerTypeCode.CHD));

            Assert.Equal(row.Cabin, Values(authored.Provision.Fare.CabinClasses).Single());
            Assert.Equal(
                new (string?, int?, int?, decimal, decimal)[] { ("ADT", null, null, row.Adult, row.Adult), ("CHD", null, null, row.Child, row.Child) },
                Rates(authored.Pricing!));
        }

        Assert.Equal(new[] { (10, "EUR 18.00/11.00"), (20, "EUR 12.00/8.00") }, await FiledAsync(meal.Id));
    }

    [Fact]
    public async Task V12_PS5_an_insurance_plan_price_differs_by_passenger_type_and_travel_window()
    {
        var airlineId = _database.NextAirlineId();
        var insurerId = await _proof.SupplierAsync(M1Commands.ExternalSupplier(airlineId, "InsurancePartnerA"));
        var plan = await _proof.DefinitionAsync(CarrierDefinition(airlineId, insurerId, "INS_BASIC", "INB", "M", "IN", "Travel insurance basic"));

        (int Sequence, DateOnly From, DateOnly To, decimal Adult, decimal Child)[] matrix =
        [
            (10, Date(2026, 12, 15), Date(2027, 1, 15), 14m, 9m),
            (20, Date(2027, 3, 15), Date(2027, 4, 5), 12m, 7m)
        ];

        foreach (var row in matrix)
        {
            var authored = await PricedAsync(
                Provision(
                    plan.Id,
                    row.Sequence,
                    coverageScope: ServiceCoverageScope.Journey,
                    travel: new ProvisionTravelCriteriaInput(SeasonalPeriods: [Period(row.From, row.To)])),
                Eur,
                Base(row.Adult, PassengerTypeCode.ADT),
                Base(row.Child, PassengerTypeCode.CHD));

            Assert.Equal(new[] { (row.From, row.To) }, Periods(authored.Provision.Travel.SeasonalPeriods));
            Assert.Equal(
                new (string?, int?, int?, decimal, decimal)[] { ("ADT", null, null, row.Adult, row.Adult), ("CHD", null, null, row.Child, row.Child) },
                Rates(authored.Pricing!));
        }

        var broad = await PricedAsync(Provision(plan.Id, 100, coverageScope: ServiceCoverageScope.Journey), Eur, Base(10m));

        Assert.Empty(broad.Provision.Travel.SeasonalPeriods);
        Assert.Equal(new[] { (10, "EUR 14.00/9.00"), (20, "EUR 12.00/7.00"), (100, "EUR 10.00") }, await FiledAsync(plan.Id));
    }

    [Fact]
    public async Task V12_PS6_a_seat_rule_differs_by_aircraft_and_characteristic()
    {
        var airlineId = _database.NextAirlineId();
        var supplierId = await _proof.SupplierAsync(M1Commands.LocalSupplier(airlineId));
        var seat = await _proof.DefinitionAsync(
            CarrierDefinition(airlineId, supplierId, "SEAT_SELECT", "STS", "F", "ST", "Seat selection", pricingUnit: PricingUnit.PerSeat));

        (int Sequence, int Aircraft, string Characteristic, decimal Price)[] matrix =
        [
            (10, 1, "LS", 22m),
            (20, 1, "W", 9m),
            (30, 2, "LS", 28m),
            (40, 2, "W", 11m)
        ];

        foreach (var row in matrix)
        {
            var authored = await PricedAsync(
                Provision(
                    seat.Id,
                    row.Sequence,
                    travel: new ProvisionTravelCriteriaInput(AircraftIds: [row.Aircraft]),
                    application: Seat(null, [row.Characteristic])),
                Eur,
                Base(row.Price));

            Assert.Equal(
                ("Seat", row.Aircraft, row.Characteristic, row.Price, "PerSeat"),
                (authored.Provision.ApplicationType.Name, Values(authored.Provision.Travel.Aircraft).Single(), Values(authored.Provision.Seat!.SeatCharacteristics).Single(), authored.Pricing!.Rates.Single().TotalAmount, authored.Pricing.PricingUnit!.Name));
        }

        var exitRow = await PricedAsync(
            Provision(seat.Id, 5, travel: new ProvisionTravelCriteriaInput(AircraftIds: [2]), application: Seat(["12A", "12F"], null)),
            Eur,
            Base(35m));

        Assert.Equal(new[] { "12A", "12F" }, Values(exitRow.Provision.Seat!.SeatNumbers));
        Assert.Equal(
            new[] { (5, "EUR 35.00"), (10, "EUR 22.00"), (20, "EUR 9.00"), (30, "EUR 28.00"), (40, "EUR 11.00") },
            await FiledAsync(seat.Id));
    }

    [Fact]
    public async Task V12_REQ_different_travel_dates_flights_routes_fare_families_cabins_and_rbds_carry_their_own_filed_price()
    {
        var airlineId = _database.NextAirlineId();
        var supplierId = await _proof.SupplierAsync(M1Commands.LocalSupplier(airlineId));
        var lounge = await _proof.DefinitionAsync(M1Commands.LoungeDefinition(airlineId, supplierId));

        (int Sequence, decimal Price, ProvisionTravelCriteriaInput? Travel, ProvisionFareCriteriaInput? Fare)[] matrix =
        [
            (10, 31m, new ProvisionTravelCriteriaInput(SeasonalPeriods: [Period(Date(2026, 12, 20), Date(2026, 12, 31))]), null),
            (20, 32m, new ProvisionTravelCriteriaInput(SeasonalPeriods: [Period(Date(2027, 1, 1), Date(2027, 1, 31))]), null),
            (30, 33m, new ProvisionTravelCriteriaInput(FlightIds: [81234]), null),
            (40, 34m, new ProvisionTravelCriteriaInput(FlightIds: [81240]), null),
            (50, 35m, new ProvisionTravelCriteriaInput(RoutePairs: [Pair(Ika, Ist)]), null),
            (60, 36m, new ProvisionTravelCriteriaInput(RoutePairs: [Pair(Ika, Mhd)]), null),
            (70, 37m, null, new ProvisionFareCriteriaInput(FareFamilyIds: [5])),
            (80, 38m, null, new ProvisionFareCriteriaInput(FareFamilyIds: [6])),
            (90, 39m, null, new ProvisionFareCriteriaInput(CabinClassIds: [1])),
            (100, 40m, null, new ProvisionFareCriteriaInput(CabinClassIds: [2])),
            (110, 41m, null, new ProvisionFareCriteriaInput(RbdIds: [41])),
            (120, 42m, null, new ProvisionFareCriteriaInput(RbdIds: [42])),
            (130, 43m, new ProvisionTravelCriteriaInput(TravelDates: [Date(2027, 3, 20)]), null),
            (140, 44m, new ProvisionTravelCriteriaInput(TravelDates: [Date(2027, 3, 21)]), null)
        ];

        var authored = new List<PublishedRule>();

        foreach (var row in matrix)
            authored.Add(await PricedAsync(Provision(lounge.Id, row.Sequence, travel: row.Travel, fare: row.Fare), Eur, Base(row.Price)));

        Assert.All(authored, rule => Assert.Equal(("Active", "Active"), (rule.Provision.Status.Name, rule.Pricing!.Status.Name)));
        Assert.Equal(matrix.Select(row => row.Price), authored.Select(rule => rule.Pricing!.Rates.Single().TotalAmount));
        Assert.Equal((Date(2026, 12, 20), Date(2027, 1, 1)), (authored[0].Provision.Travel.SeasonalPeriods.Single().StartDate, authored[1].Provision.Travel.SeasonalPeriods.Single().StartDate));
        Assert.Equal((81234L, 81240L), (Values(authored[2].Provision.Travel.Flights).Single(), Values(authored[3].Provision.Travel.Flights).Single()));
        Assert.Equal((Ist, Mhd), (authored[4].Provision.Travel.RoutePairs.Single().DestinationAirportId, authored[5].Provision.Travel.RoutePairs.Single().DestinationAirportId));
        Assert.Equal((5L, 6L), (Values(authored[6].Provision.Fare.FareFamilies).Single(), Values(authored[7].Provision.Fare.FareFamilies).Single()));
        Assert.Equal((1, 2), (Values(authored[8].Provision.Fare.CabinClasses).Single(), Values(authored[9].Provision.Fare.CabinClasses).Single()));
        Assert.Equal((41L, 42L), (Values(authored[10].Provision.Fare.Rbds).Single(), Values(authored[11].Provision.Fare.Rbds).Single()));
        Assert.Equal((Date(2027, 3, 20), Date(2027, 3, 21)), (Values(authored[12].Provision.Travel.TravelDates).Single(), Values(authored[13].Provision.Travel.TravelDates).Single()));
        Assert.Equal(
            matrix.Select(row => (row.Sequence, $"EUR {row.Price.ToString("0.00", CultureInfo.InvariantCulture)}")),
            await FiledAsync(lounge.Id));
    }

    [Fact]
    public async Task V12_REQ_different_points_of_sale_customers_and_customer_types_carry_their_own_filed_price_and_currency()
    {
        var airlineId = _database.NextAirlineId();
        var supplierId = await _proof.SupplierAsync(M1Commands.LocalSupplier(airlineId));
        var lounge = await _proof.DefinitionAsync(M1Commands.LoungeDefinition(airlineId, supplierId));

        var offices = await PricedAsync(
            Provision(lounge.Id, 10, sales: new ProvisionSalesCriteriaInput(PointOfSaleIds: [AirlineOffice, AgencyOffice])),
            Irr,
            Base(15000000m),
            Tax("VAT", 1500000m, countryId: 1));
        var namedCustomer = await PricedAsync(
            Provision(lounge.Id, 20, sales: new ProvisionSalesCriteriaInput(CustomerIds: [9001])),
            Eur,
            Base(18m));
        var agencies = await PricedAsync(
            Provision(lounge.Id, 30, sales: new ProvisionSalesCriteriaInput(CustomerTypes: [CustomerType.TravelAgency])),
            Usd,
            Base(22m));

        Assert.Equal((Irr, "IRR", 15000000m, 16500000m), (offices.Pricing!.CurrencyId, offices.Pricing.Currency, offices.Pricing.Rates.Single().BaseAmount, offices.Pricing.Rates.Single().TotalAmount));
        Assert.Equal(new[] { AirlineOffice, AgencyOffice }, Values(offices.Provision.Sales.PointsOfSale));
        Assert.Equal((Eur, 18m), (namedCustomer.Pricing!.CurrencyId, namedCustomer.Pricing.Rates.Single().TotalAmount));
        Assert.Equal(new long[] { 9001 }, Values(namedCustomer.Provision.Sales.Customers));
        Assert.Equal((Usd, "USD", 22m), (agencies.Pricing!.CurrencyId, agencies.Pricing.Currency, agencies.Pricing.Rates.Single().TotalAmount));
        Assert.Equal(new[] { "TravelAgency" }, Names(agencies.Provision.Sales.CustomerTypes));
        Assert.Equal(new[] { (10, "IRR 16500000.00"), (20, "EUR 18.00"), (30, "USD 22.00") }, await FiledAsync(lounge.Id));
    }
}
