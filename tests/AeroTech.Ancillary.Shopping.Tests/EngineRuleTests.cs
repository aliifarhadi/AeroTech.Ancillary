using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Ancillary.Domain.ConformanceTests.Oracle;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Ancillary.Shopping.Selection;
using AeroTech.Ancillary.Shopping.Tests.Fixtures;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;
using AeroTech.Messages.Core.Enums;
using Xunit;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.ShoppingLab;

namespace AeroTech.Ancillary.Shopping.Tests;

public class EngineRuleTests
{
    private static readonly DateTimeOffset Departure = new(2026, 12, 1, 10, 0, 0, Trip.TehranOffset);

    private static ServiceSpecificationInput Plain(ServiceSpecificationInput input)
        => input with { Priority = input.Priority! with { AirportIds = [], FareBenefitRef = null } };

    private static ProvisionFareApplicationArgs Fares(
        long[]? fares = null,
        AirFareType[]? types = null,
        long[]? families = null,
        string[]? bases = null,
        int[]? cabins = null,
        long[]? rbds = null)
        => new(fares ?? [], types ?? [], families ?? [], bases ?? [], cabins ?? [], rbds ?? []);

    private static ProvisionFlightApplicationArgs Flights(int[]? marketing = null, int[]? operating = null, string[]? numbers = null, long[]? ids = null, int[]? aircraft = null)
        => new(marketing ?? [], operating ?? [], numbers ?? [], ids ?? [], aircraft ?? []);

    private static ProvisionGeographyArgs Geography(
        int[]? origins = null,
        int[]? destinations = null,
        int[]? via = null,
        ProvisionRoutePairArgs[]? pairs = null,
        int[]? countries = null)
        => new(origins ?? [], destinations ?? [], via ?? [], pairs ?? [], [], countries ?? []);

    private static ProvisionSalesRestrictionsArgs Sales(DateTimeOffset? from = null, DateTimeOffset? until = null, long[]? customers = null, CustomerType[]? types = null)
        => new(from, until, [Pos], customers ?? [], types ?? []);

    private static async Task<(EligibilityStatus Status, CanonicalAncillaryOfferCandidate? Candidate)> VerdictAsync(
        ProvisionRulesArgs rules,
        AncillaryShoppingContext? context = null,
        ServiceCoverageScope scope = ServiceCoverageScope.Sector)
    {
        var lab = new ShoppingLab();

        lab.Provision(lab.Definition("A23", specification: Plain), rules: rules, scope: scope);

        var result = await lab.Engine.ShopAsync(context ?? Trip.Context(), new ShoppingFilter { IncludeNonSellable = true });
        var candidate = result.Candidates.FirstOrDefault(row => row.Eligibility.Status != EligibilityStatus.Eligible) ?? result.Candidates.FirstOrDefault();

        return (candidate?.Eligibility.Status ?? EligibilityStatus.NotEligible, candidate);
    }

    private static AncillaryShoppingContext With(AncillaryShoppingContext context, Func<TravellerFareFacts, TravellerFareFacts> fare)
        => context with { TravellerFareFacts = context.TravellerFareFacts.Select(fare).ToList() };

    private static AncillaryShoppingContext Leaving(DateTimeOffset departure, string? zone = Trip.Tehran, ShoppingTraveller? traveller = null)
        => Trip.Context([Trip.Flight(departure: departure, zone: zone)], travellers: traveller is null ? null : [traveller]);

    [Fact]
    public async Task RULE_01_passenger_types_and_age_bands_use_the_age_on_the_local_service_date()
    {
        var children = Rules(passengers: new([PassengerTypeCode.CHD], [new(2, 12)]));
        var leapling = Trip.Child("T1", new DateOnly(2016, 2, 29));
        var turningTwelve = Trip.Child("T1", new DateOnly(2014, 12, 2));

        Assert.Equal(EligibilityStatus.NotEligible, (await VerdictAsync(children)).Status);
        Assert.Equal(EligibilityStatus.Eligible, (await VerdictAsync(children, Trip.Context(travellers: [Trip.Child("T1")]))).Status);
        Assert.Equal(EligibilityStatus.Eligible, (await VerdictAsync(children, Leaving(Departure, traveller: turningTwelve))).Status);
        Assert.Equal(EligibilityStatus.NotEligible, (await VerdictAsync(children, Leaving(Departure.AddDays(1), traveller: turningTwelve))).Status);

        var tenOnly = Rules(passengers: new([], [new(10, 11)]));

        Assert.Equal(EligibilityStatus.NotEligible, (await VerdictAsync(tenOnly, Leaving(new DateTimeOffset(2026, 2, 27, 10, 0, 0, Trip.TehranOffset), traveller: leapling))).Status);
        Assert.Equal(EligibilityStatus.Eligible, (await VerdictAsync(tenOnly, Leaving(new DateTimeOffset(2026, 2, 28, 10, 0, 0, Trip.TehranOffset), traveller: leapling))).Status);

        var unknownAge = await VerdictAsync(children, Trip.Context(travellers: [Trip.Child("T1") with { DateOfBirth = null }]));

        Assert.Equal(EligibilityStatus.InsufficientContext, unknownAge.Status);
        Assert.Contains(ShoppingReasonCodes.AgeNotVerified, unknownAge.Candidate!.ReasonCodes);
        Assert.Contains("Travellers[T1].DateOfBirth", unknownAge.Candidate.Eligibility.MissingContextFields);

        var verifiedAge = Trip.Child("T1") with { DateOfBirth = null, VerifiedAgeAtTravel = 8, AgeEvidenceAsOfDate = new DateOnly(2026, 12, 1) };

        Assert.Equal(EligibilityStatus.Eligible, (await VerdictAsync(children, Trip.Context(travellers: [verifiedAge]))).Status);
        Assert.Equal(
            EligibilityStatus.InsufficientContext,
            (await VerdictAsync(children, Trip.Context(travellers: [verifiedAge with { AgeEvidenceAsOfDate = new DateOnly(2026, 11, 30) }]))).Status);
        Assert.Equal(EligibilityStatus.Eligible, (await VerdictAsync(Rules(passengers: new([PassengerTypeCode.CHD], [])), Trip.Context(travellers: [verifiedAge]))).Status);
    }

    [Fact]
    public async Task RULE_02_sales_window_customer_and_customer_type_cannot_be_bypassed_by_leaving_facts_out()
    {
        Assert.Equal(EligibilityStatus.Eligible, (await VerdictAsync(Rules(sales: Sales(from: Now, until: Now.AddTicks(1))))).Status);
        Assert.Equal(EligibilityStatus.NotEligible, (await VerdictAsync(Rules(sales: Sales(from: Now.AddTicks(1))))).Status);
        Assert.Equal(EligibilityStatus.NotEligible, (await VerdictAsync(Rules(sales: Sales(until: Now)))).Status);

        var corporate = Rules(sales: Sales(customers: [77]));
        var agencies = Rules(sales: Sales(types: [(CustomerType)1]));

        Assert.Equal(EligibilityStatus.InsufficientContext, (await VerdictAsync(corporate)).Status);
        Assert.Equal(EligibilityStatus.Eligible, (await VerdictAsync(corporate, Trip.Context() with { CustomerId = 77 })).Status);
        Assert.Equal(EligibilityStatus.NotEligible, (await VerdictAsync(corporate, Trip.Context() with { CustomerId = 78 })).Status);
        Assert.Equal(EligibilityStatus.InsufficientContext, (await VerdictAsync(agencies)).Status);
        Assert.Equal(EligibilityStatus.Eligible, (await VerdictAsync(agencies, Trip.Context() with { CustomerType = (CustomerType)1 })).Status);
        Assert.Equal(EligibilityStatus.NotEligible, (await VerdictAsync(agencies, Trip.Context() with { CustomerType = (CustomerType)2 })).Status);
        Assert.Equal(EligibilityStatus.NotEligible, (await VerdictAsync(Rules(), Trip.Context(pointOfSaleId: OtherPos))).Status);
    }

    [Fact]
    public async Task RULE_02_definition_sales_dates_are_decided_only_when_the_calendar_day_is_the_same_everywhere()
    {
        async Task<EligibilityStatus> OnAsync(DateOnly? from, DateOnly? until)
        {
            var lab = new ShoppingLab();

            lab.Provision(lab.Definition("A23", specification: Plain, salesFrom: from, salesUntil: until));

            var result = await lab.Engine.ShopAsync(Trip.Context(), new ShoppingFilter { IncludeNonSellable = true });

            return result.Candidates.SingleOrDefault()?.Eligibility.Status ?? EligibilityStatus.NotEligible;
        }

        Assert.Equal(EligibilityStatus.Eligible, await OnAsync(new DateOnly(2026, 9, 30), new DateOnly(2026, 10, 2)));
        Assert.Equal(EligibilityStatus.NotEligible, await OnAsync(new DateOnly(2026, 10, 3), null));
        Assert.Equal(EligibilityStatus.NotEligible, await OnAsync(null, new DateOnly(2026, 9, 29)));
        Assert.Equal(EligibilityStatus.InsufficientContext, await OnAsync(new DateOnly(2026, 10, 1), null));
        Assert.Equal(EligibilityStatus.InsufficientContext, await OnAsync(null, new DateOnly(2026, 10, 1)));
    }

    [Fact]
    public async Task RULE_03_geography_matches_origin_destination_via_directional_pairs_and_verified_countries()
    {
        var connection = Trip.Context([
            Trip.Flight("F1", 101, Trip.Thr, Trip.Ika),
            Trip.Flight("F2", 102, Trip.Ika, Trip.Ist, Departure.AddHours(5))
        ]);

        Assert.Equal(EligibilityStatus.Eligible, (await VerdictAsync(Rules(geography: Geography(origins: [Trip.Thr], destinations: [Trip.Ist])))).Status);
        Assert.Equal(EligibilityStatus.NotEligible, (await VerdictAsync(Rules(geography: Geography(origins: [Trip.Mhd])))).Status);
        Assert.Equal(EligibilityStatus.NotEligible, (await VerdictAsync(Rules(geography: Geography(destinations: [Trip.Mhd])))).Status);
        Assert.Equal(EligibilityStatus.Eligible, (await VerdictAsync(Rules(geography: Geography(pairs: [new(Trip.Thr, Trip.Ist, RoutePairDirection.Directional)])))).Status);
        Assert.Equal(EligibilityStatus.NotEligible, (await VerdictAsync(Rules(geography: Geography(pairs: [new(Trip.Ist, Trip.Thr, RoutePairDirection.Directional)])))).Status);
        Assert.Equal(EligibilityStatus.Eligible, (await VerdictAsync(Rules(geography: Geography(pairs: [new(Trip.Ist, Trip.Thr, RoutePairDirection.BothDirections)])))).Status);
        Assert.Equal(EligibilityStatus.Eligible, (await VerdictAsync(Rules(geography: Geography(via: [Trip.Ika])), connection, ServiceCoverageScope.Portion)).Status);
        Assert.Equal(EligibilityStatus.NotEligible, (await VerdictAsync(Rules(geography: Geography(via: [Trip.Mhd])), connection, ServiceCoverageScope.Portion)).Status);
        Assert.Equal(EligibilityStatus.NotEligible, (await VerdictAsync(Rules(geography: Geography(via: [Trip.Ika])))).Status);
        Assert.Equal(EligibilityStatus.Eligible, (await VerdictAsync(Rules(geography: Geography(countries: [200])))).Status);
        Assert.Equal(EligibilityStatus.NotEligible, (await VerdictAsync(Rules(geography: Geography(countries: [300])))).Status);

        var unknownVia = await VerdictAsync(Rules(geography: Geography(via: [Trip.Ika])), Trip.Context([Trip.Flight() with { ViaAirportIds = null }]));
        var unknownCountry = await VerdictAsync(Rules(geography: Geography(countries: [200])), Trip.Context([Trip.Flight() with { DestinationCountryId = null }]));

        Assert.Equal((EligibilityStatus.InsufficientContext, true), (unknownVia.Status, unknownVia.Candidate!.ReasonCodes.Contains(ShoppingReasonCodes.ViaNotVerified)));
        Assert.Equal((EligibilityStatus.InsufficientContext, true), (unknownCountry.Status, unknownCountry.Candidate!.ReasonCodes.Contains(ShoppingReasonCodes.CountryNotVerified)));
        Assert.Equal(
            EligibilityStatus.InsufficientContext,
            (await VerdictAsync(Rules(geography: new([], [], [], [], [new(ServiceLocationType.Airport, Trip.Thr)], [])))).Status);
    }

    [Fact]
    public async Task RULE_04_marketing_operating_number_flight_and_aircraft_are_separate_filters_without_fallback()
    {
        var codeshare = Trip.Context([Trip.Flight() with { MarketingAirlineId = 3, OperatingAirlineId = 1 }]);

        Assert.Equal(EligibilityStatus.Eligible, (await VerdictAsync(Rules(flights: Flights(marketing: [3], operating: [1])), codeshare)).Status);
        Assert.Equal(EligibilityStatus.NotEligible, (await VerdictAsync(Rules(flights: Flights(marketing: [1])), codeshare)).Status);
        Assert.Equal(EligibilityStatus.NotEligible, (await VerdictAsync(Rules(flights: Flights(operating: [3])), codeshare)).Status);
        Assert.Equal(
            EligibilityStatus.InsufficientContext,
            (await VerdictAsync(Rules(flights: Flights(marketing: [1])), Trip.Context([Trip.Flight() with { MarketingAirlineId = null }]))).Status);
        Assert.Equal(EligibilityStatus.Eligible, (await VerdictAsync(Rules(flights: Flights(numbers: ["111"], ids: [101])))).Status);
        Assert.Equal(EligibilityStatus.NotEligible, (await VerdictAsync(Rules(flights: Flights(numbers: ["111"], ids: [999])))).Status);
        Assert.Equal(EligibilityStatus.NotEligible, (await VerdictAsync(Rules(flights: Flights(numbers: ["999"])))).Status);
        Assert.Equal(EligibilityStatus.Eligible, (await VerdictAsync(Rules(flights: Flights(aircraft: [1, 4])))).Status);
        Assert.Equal(EligibilityStatus.NotEligible, (await VerdictAsync(Rules(flights: Flights(aircraft: [4])))).Status);

        var missing = await VerdictAsync(Rules(flights: Flights(aircraft: [1])), Trip.Context([Trip.Flight() with { AircraftId = null }]));

        Assert.Equal(EligibilityStatus.InsufficientContext, missing.Status);
        Assert.Contains("Flights[F1].AircraftId", missing.Candidate!.Eligibility.MissingContextFields);
    }

    [Fact]
    public async Task RULE_05_fare_filters_read_the_coupon_of_that_traveller_and_flight_and_never_the_fare_family_text()
    {
        var all = Rules(fares: Fares(fares: [501], types: [(AirFareType)1], families: [5], bases: ["YOW"], cabins: [1], rbds: [11]));

        Assert.Equal(EligibilityStatus.Eligible, (await VerdictAsync(all)).Status);
        Assert.Equal(EligibilityStatus.NotEligible, (await VerdictAsync(all, With(Trip.Context(), fare => fare with { AirFareId = 502 }))).Status);
        Assert.Equal(EligibilityStatus.NotEligible, (await VerdictAsync(all, With(Trip.Context(), fare => fare with { AirFareType = (AirFareType)2 }))).Status);
        Assert.Equal(EligibilityStatus.NotEligible, (await VerdictAsync(all, With(Trip.Context(), fare => fare with { FareBasisCode = "QOW" }))).Status);
        Assert.Equal(EligibilityStatus.NotEligible, (await VerdictAsync(all, With(Trip.Context(), fare => fare with { CabinClassId = 2 }))).Status);
        Assert.Equal(EligibilityStatus.NotEligible, (await VerdictAsync(all, With(Trip.Context(), fare => fare with { RbdId = 12 }))).Status);

        var family = Rules(fares: Fares(families: [5]));
        var textOnly = await VerdictAsync(family, With(Trip.Context(), fare => fare with { FareFamilyId = null, FareFamilyCodeOrName = "5" }));

        Assert.Equal(EligibilityStatus.InsufficientContext, textOnly.Status);
        Assert.Contains(ShoppingReasonCodes.FareFamilyNotVerified, textOnly.Candidate!.ReasonCodes);
        Assert.Equal(EligibilityStatus.InsufficientContext, (await VerdictAsync(family, Trip.Context() with { TravellerFareFacts = [] })).Status);

        var lab = new ShoppingLab();
        var definition = lab.Definition("A23", specification: Plain);

        lab.Provision(definition, rules: family);

        var mixed = Trip.Context(travellers: [Trip.Adult("T1"), Trip.Adult("T2")]);
        var result = await lab.Engine.ShopAsync(
            mixed with { TravellerFareFacts = mixed.TravellerFareFacts.Select(fare => fare.TravellerRef == "T2" ? fare with { FareFamilyId = 9 } : fare).ToList() });

        Assert.Equal("T1", Assert.Single(result.One().Travellers));
    }

    [Fact]
    public async Task RULE_06_travel_dates_are_inclusive_local_dates_blackout_wins_and_every_covered_flight_counts()
    {
        var april = Rules(dates: new([new(new DateOnly(2027, 4, 1), new DateOnly(2027, 4, 30))], [new(new DateOnly(2027, 4, 10), new DateOnly(2027, 4, 12))]));

        DateTimeOffset On(int month, int day, int hour = 12) => new(2027, month, day, hour, 0, 0, Trip.TehranOffset);

        Assert.Equal(EligibilityStatus.NotEligible, (await VerdictAsync(april, Leaving(On(3, 31)))).Status);
        Assert.Equal(EligibilityStatus.Eligible, (await VerdictAsync(april, Leaving(On(4, 1, 0)))).Status);
        Assert.Equal(EligibilityStatus.Eligible, (await VerdictAsync(april, Leaving(On(4, 9)))).Status);
        Assert.Equal(EligibilityStatus.NotEligible, (await VerdictAsync(april, Leaving(On(4, 11)))).Status);
        Assert.Equal(EligibilityStatus.Eligible, (await VerdictAsync(april, Leaving(On(4, 30, 23)))).Status);
        Assert.Equal(EligibilityStatus.NotEligible, (await VerdictAsync(april, Leaving(On(5, 1, 0)))).Status);
        Assert.Equal(EligibilityStatus.Eligible, (await VerdictAsync(april, Leaving(new DateTimeOffset(2027, 3, 31, 21, 0, 0, TimeSpan.Zero)))).Status);
        Assert.Equal(EligibilityStatus.InsufficientContext, (await VerdictAsync(april, Leaving(On(4, 9), zone: null))).Status);
        Assert.Equal(EligibilityStatus.InsufficientContext, (await VerdictAsync(april, Leaving(On(4, 9), zone: "+03:30"))).Status);

        var straddling = Trip.Context([
            Trip.Flight("F1", 101, Trip.Thr, Trip.Ika, On(4, 9, 20)),
            Trip.Flight("F2", 102, Trip.Ika, Trip.Ist, On(4, 10, 2))
        ]);

        Assert.Equal(EligibilityStatus.NotEligible, (await VerdictAsync(april, straddling, ServiceCoverageScope.Portion)).Status);
        Assert.Equal(new[] { "F1" }, (await VerdictAsyncAll(april, straddling)).Select(candidate => Assert.Single(candidate.FlightRefs)));
    }

    private static async Task<IReadOnlyList<CanonicalAncillaryOfferCandidate>> VerdictAsyncAll(ProvisionRulesArgs rules, AncillaryShoppingContext context)
    {
        var lab = new ShoppingLab();

        lab.Provision(lab.Definition("A23", specification: Plain), rules: rules);

        return (await lab.Engine.ShopAsync(context)).Candidates;
    }

    [Fact]
    public async Task RULE_07_day_and_time_windows_use_the_zone_of_the_occurrence_and_deny_beats_allow()
    {
        var weekdayMornings = Rules(dayTime: new([new(31, new TimeOnly(6, 0), new TimeOnly(12, 0), DayTimeRestrictionEffect.Allow)]));

        DateTimeOffset Tehran(int day, int hour, int minute = 0) => new(2026, 12, day, hour, minute, 0, Trip.TehranOffset);

        Assert.Equal(EligibilityStatus.Eligible, (await VerdictAsync(weekdayMornings, Leaving(Tehran(1, 6)))).Status);
        Assert.Equal(EligibilityStatus.Eligible, (await VerdictAsync(weekdayMornings, Leaving(Tehran(1, 11, 59)))).Status);
        Assert.Equal(EligibilityStatus.NotEligible, (await VerdictAsync(weekdayMornings, Leaving(Tehran(1, 12)))).Status);
        Assert.Equal(EligibilityStatus.NotEligible, (await VerdictAsync(weekdayMornings, Leaving(Tehran(1, 5, 59)))).Status);
        Assert.Equal(EligibilityStatus.NotEligible, (await VerdictAsync(weekdayMornings, Leaving(Tehran(5, 8)))).Status);

        var withDeny = Rules(dayTime: new([
            new(127, null, null, DayTimeRestrictionEffect.Allow),
            new(2, new TimeOnly(9, 0), new TimeOnly(11, 0), DayTimeRestrictionEffect.Deny)
        ]));

        Assert.Equal(EligibilityStatus.NotEligible, (await VerdictAsync(withDeny, Leaving(Tehran(1, 10)))).Status);
        Assert.Equal(EligibilityStatus.Eligible, (await VerdictAsync(withDeny, Leaving(Tehran(1, 11)))).Status);
        Assert.Equal(EligibilityStatus.Eligible, (await VerdictAsync(withDeny, Leaving(Tehran(2, 10)))).Status);

        var parisNight = Rules(dayTime: new([new(64, new TimeOnly(3, 0), new TimeOnly(4, 0), DayTimeRestrictionEffect.Allow)]));
        var afterSpringForward = new DateTimeOffset(2027, 3, 28, 1, 30, 0, TimeSpan.Zero);

        Assert.Equal(EligibilityStatus.Eligible, (await VerdictAsync(parisNight, Leaving(afterSpringForward, Trip.Paris))).Status);
        Assert.Equal(EligibilityStatus.NotEligible, (await VerdictAsync(parisNight, Leaving(afterSpringForward.AddHours(-1), Trip.Paris))).Status);

        var unknown = await VerdictAsync(parisNight, Leaving(afterSpringForward, null));

        Assert.Equal(EligibilityStatus.InsufficientContext, unknown.Status);
        Assert.Contains(ShoppingReasonCodes.TimeZoneNotVerified, unknown.Candidate!.ReasonCodes);
        Assert.Equal(EligibilityStatus.InsufficientContext, (await VerdictAsync(parisNight, Leaving(afterSpringForward, "Mars/Olympus"))).Status);
    }

    [Fact]
    public async Task RULE_08_advance_purchase_compares_exact_cutoffs_in_each_unit_and_needs_the_ticket_time_when_asked()
    {
        DateTimeOffset In(TimeSpan lead) => Now + lead;

        var twoHours = Rules(advance: new(120, TimeUnit.Minutes, false));
        var day = Rules(advance: new(1, TimeUnit.Days, false));
        var window = Rules(advance: new(2, TimeUnit.Days, false, 30));
        var months = Rules(advance: new(1, TimeUnit.Months, false));

        Assert.Equal(EligibilityStatus.Eligible, (await VerdictAsync(twoHours, Leaving(In(TimeSpan.FromMinutes(120))))).Status);
        Assert.Equal(EligibilityStatus.NotEligible, (await VerdictAsync(twoHours, Leaving(In(TimeSpan.FromMinutes(120) - TimeSpan.FromSeconds(1))))).Status);
        Assert.Equal(EligibilityStatus.Eligible, (await VerdictAsync(Rules(advance: new(2, TimeUnit.Hours, false)), Leaving(In(TimeSpan.FromHours(2))))).Status);
        Assert.Equal(EligibilityStatus.Eligible, (await VerdictAsync(day, Leaving(new DateTimeOffset(2026, 10, 2, 0, 5, 0, Trip.TehranOffset)))).Status);
        Assert.Equal(EligibilityStatus.NotEligible, (await VerdictAsync(day, Leaving(new DateTimeOffset(2026, 10, 1, 23, 55, 0, Trip.TehranOffset)))).Status);
        Assert.Equal(EligibilityStatus.Eligible, (await VerdictAsync(window, Leaving(new DateTimeOffset(2026, 10, 31, 9, 0, 0, Trip.TehranOffset)))).Status);
        Assert.Equal(EligibilityStatus.NotEligible, (await VerdictAsync(window, Leaving(new DateTimeOffset(2026, 11, 1, 9, 0, 0, Trip.TehranOffset)))).Status);
        Assert.Equal(EligibilityStatus.NotEligible, (await VerdictAsync(window, Leaving(new DateTimeOffset(2026, 10, 2, 20, 0, 0, Trip.TehranOffset)))).Status);
        Assert.Equal(EligibilityStatus.Eligible, (await VerdictAsync(months, Leaving(new DateTimeOffset(2026, 11, 1, 9, 0, 0, Trip.TehranOffset)))).Status);
        Assert.Equal(EligibilityStatus.NotEligible, (await VerdictAsync(months, Leaving(new DateTimeOffset(2026, 10, 31, 9, 0, 0, Trip.TehranOffset)))).Status);
        Assert.Equal(EligibilityStatus.InsufficientContext, (await VerdictAsync(day, Leaving(Departure, zone: null))).Status);

        var withTicket = Rules(advance: new(0, TimeUnit.Days, true));
        var noTicketTime = await VerdictAsync(withTicket);

        Assert.Equal(EligibilityStatus.InsufficientContext, noTicketTime.Status);
        Assert.Contains(ShoppingReasonCodes.TicketTimeNotVerified, noTicketTime.Candidate!.ReasonCodes);

        AncillaryShoppingContext Ticketed(DateTimeOffset at)
            => Trip.Context() with { SourceIdentity = new SourceIdentityContext { SourceKind = ShoppingSourceKind.Offer, TicketedAtUtc = at } };

        Assert.Equal(EligibilityStatus.Eligible, (await VerdictAsync(withTicket, Ticketed(Now))).Status);
        Assert.Equal(EligibilityStatus.NotEligible, (await VerdictAsync(withTicket, Ticketed(Now.AddMinutes(-5)))).Status);
    }

    [Fact]
    public async Task RULE_09_baggage_tiers_follow_the_verified_allowance_and_the_pieces_already_bought()
    {
        ProvisionBaggageApplicationArgs Tier(int first, int last, int? free = null)
            => new(free, first, last, null, WeightUnit.Kg, null, BaggagePurchaseApplication.Prepaid, null, BaggageChargeKind.ExtraPiece, BaggageAllowanceConcept.Piece);

        var lab = new ShoppingLab();
        var bag = lab.Definition("A01");
        var firstTier = lab.Provision(bag, sequence: 10, rules: Rules(baggage: Tier(1, 1, 1)), maxQuantity: 1);
        var secondTier = lab.Provision(bag, sequence: 20, rules: Rules(baggage: Tier(2, 3, 1)), maxQuantity: 2);

        lab.Price(bag, firstTier, Rate(40m));
        lab.Price(bag, secondTier, Rate(60m));

        ExistingAncillaryServiceFacts Bought(int quantity, ExistingServiceCommercialState state = ExistingServiceCommercialState.Active)
            => new() { ServiceRef = bag.ServiceDefinitionRef, TravellerRef = "T1", FlightRefs = ["F1"], Quantity = quantity, CommercialState = state };

        var none = (await lab.Engine.ShopAsync(Trip.Context())).One();
        var one = (await lab.Engine.ShopAsync(Trip.Context() with { ExistingServiceFacts = [Bought(1)] })).One();
        var cancelled = (await lab.Engine.ShopAsync(Trip.Context() with { ExistingServiceFacts = [Bought(1, ExistingServiceCommercialState.Cancelled)] })).One();
        var three = await lab.Engine.ShopAsync(Trip.Context() with { ExistingServiceFacts = [Bought(3)] });

        Assert.Equal((firstTier.Id, 40m, OfferReadiness.Selectable), (none.ProvisionId, none.Price.CompleteUnitTotal, none.OfferReadiness));
        Assert.Equal((1, 1), (none.Quantity.MinPerSelection, none.Quantity.MaxPerSelection));
        Assert.Equal((secondTier.Id, 60m), (one.ProvisionId, one.Price.CompleteUnitTotal));
        Assert.Equal(firstTier.Id, cancelled.ProvisionId);
        Assert.Empty(three.Candidates);

        var twoMore = await lab.Engine.EvaluateSelectionAsync(Trip.Context() with { ExistingServiceFacts = [Bought(1)] }, one.CandidateIdentity, new AncillarySelection { Quantity = 2 });
        var tooMany = await lab.Engine.EvaluateSelectionAsync(Trip.Context() with { ExistingServiceFacts = [Bought(2)] }, one.CandidateIdentity, new AncillarySelection { Quantity = 2 });

        Assert.Equal((SelectionEvaluationStatus.Accepted, 120m), (twoMore.Status, twoMore.Candidate.Price.RequestedQuantityTotal));
        Assert.Equal(SelectionEvaluationStatus.Rejected, tooMany.Status);

        var otherAllowance = await lab.Engine.ShopAsync(Trip.Context() with { TravellerBaggageFacts = [Trip.Baggage("T1", "F1", "P1", checkedPieces: 2)] });
        var unknownAllowance = (await lab.Engine.ShopAsync(
            Trip.Context() with { TravellerBaggageFacts = [Trip.Baggage("T1", "F1", "P1") with { SourceCompleteness = FactEvidence.Partial }] })).One();
        var unknownPurchases = (await lab.Engine.ShopAsync(
            Trip.Context() with { CoverageCompleteness = Trip.Context().CoverageCompleteness with { ExistingServicesComplete = false } })).One();

        Assert.Empty(otherAllowance.Candidates);
        Assert.Equal(OfferReadiness.NeedsVerification, unknownAllowance.OfferReadiness);
        Assert.Contains(ShoppingReasonCodes.BaggageAllowanceNotVerified, unknownAllowance.ReasonCodes);
        Assert.Contains(ShoppingReasonCodes.ExistingServicesNotVerified, unknownPurchases.ReasonCodes);

        var checkInOnly = new ShoppingLab();
        var counter = checkInOnly.Definition("A04");

        checkInOnly.Price(
            counter,
            checkInOnly.Provision(
                counter,
                rules: Rules(baggage: new(null, null, null, null, WeightUnit.Kg, null, BaggagePurchaseApplication.CheckIn, null, BaggageChargeKind.Oversize, null))),
            Rate(70m));

        Assert.Empty((await checkInOnly.Engine.ShopAsync(Trip.Context())).Candidates);
    }

    [Fact]
    public async Task RULE_10_a_seat_rule_waits_for_the_selected_seat_and_needs_verified_seat_map_evidence()
    {
        var lab = new ShoppingLab();
        var seat = lab.Definition("A07");
        var exitRows = lab.Provision(seat, sequence: 10, rules: Rules(flights: Flights(aircraft: [1]), seats: new(["12A", "12F"], [])));
        var windows = lab.Provision(seat, sequence: 20, rules: Rules(seats: new([], ["W"])));

        lab.Price(seat, exitRows, Rate(50m));
        lab.Price(seat, windows, Rate(20m));

        var shown = (await lab.Engine.ShopAsync(Trip.Context())).One();

        Assert.Equal((exitRows.Id, OfferReadiness.NeedsSelection, EligibilityStatus.InsufficientContext), (shown.ProvisionId, shown.OfferReadiness, shown.Eligibility.Status));
        Assert.Contains(ShoppingReasonCodes.SeatSelectionPending, shown.ReasonCodes);

        Task<CanonicalAncillarySelectionEvaluation> ChooseAsync(string number, params string[]? characteristics)
            => lab.Engine.EvaluateSelectionAsync(Trip.Context(), shown.CandidateIdentity, new AncillarySelection { SeatNumber = number, VerifiedSeatCharacteristicCodes = characteristics });

        var exit = await ChooseAsync("12a", "E");
        var window = await ChooseAsync("20A", "W");
        var aisle = await ChooseAsync("20C", "A");
        var unverified = await ChooseAsync("20A", null);

        Assert.Equal((SelectionEvaluationStatus.Accepted, exitRows.Id, 50m, false), (exit.Status, exit.Candidate.ProvisionId, exit.Candidate.Price.CompleteUnitTotal, exit.ProvisionChangedBySelection));
        Assert.Equal((SelectionEvaluationStatus.Accepted, windows.Id, 20m, true), (window.Status, window.Candidate.ProvisionId, window.Candidate.Price.CompleteUnitTotal, window.ProvisionChangedBySelection));
        Assert.Contains(ShoppingReasonCodes.ProvisionChangedBySelection, window.Candidate.ReasonCodes);
        Assert.Equal(SelectionEvaluationStatus.Rejected, aisle.Status);
        Assert.Contains(aisle.FieldIssues, issue => issue is { Field: "SeatNumber", ReasonCode: ShoppingReasonCodes.SeatNumberNotAllowed });
        Assert.Equal(OfferReadiness.NeedsVerification, unverified.Candidate.OfferReadiness);
        Assert.Contains(ShoppingReasonCodes.SeatMapNotVerified, unverified.Candidate.ReasonCodes);
        Assert.False(unverified.Candidate.Availability.IsGuaranteed);

        var meal = lab.Definition("A12");

        lab.Price(meal, lab.Provision(meal), Rate(9m));

        var mealCandidate = (await lab.Engine.ShopAsync(Trip.Context(), new ShoppingFilter { VariantCodes = ["A12"] })).One();
        var foreign = await lab.Engine.EvaluateSelectionAsync(Trip.Context(), mealCandidate.CandidateIdentity, new AncillarySelection { MenuItemRef = "MENU_PASTA_01", Quantity = 1, SeatNumber = "12A" });

        Assert.Equal(SelectionEvaluationStatus.Rejected, foreign.Status);
        Assert.Contains(foreign.FieldIssues, issue => issue is { Field: "SeatNumber", ReasonCode: ShoppingReasonCodes.SelectionFieldNotAllowed });
    }

    public static IEnumerable<object[]> OracleRules()
    {
        (string Name, ProvisionRulesArgs Rules)[] sets =
        [
            ("none", Rules()),
            ("adult", Rules(passengers: new([PassengerTypeCode.ADT], []))),
            ("child band", Rules(passengers: new([PassengerTypeCode.CHD], [new(2, 12)]))),
            ("age band", Rules(passengers: new([], [new(18, null)]))),
            ("sales open", Rules(sales: Sales(from: Now.AddDays(-1), until: Now.AddDays(1)))),
            ("sales closed", Rules(sales: Sales(until: Now))),
            ("customer", Rules(sales: Sales(customers: [77]))),
            ("customer type", Rules(sales: Sales(types: [(CustomerType)1]))),
            ("origin", Rules(geography: Geography(origins: [Trip.Thr]))),
            ("destination", Rules(geography: Geography(destinations: [Trip.Mhd]))),
            ("via", Rules(geography: Geography(via: [Trip.Ika]))),
            ("pair", Rules(geography: Geography(pairs: [new(Trip.Ist, Trip.Thr, RoutePairDirection.BothDirections)]))),
            ("country", Rules(geography: Geography(countries: [200]))),
            ("marketing", Rules(flights: Flights(marketing: [1]))),
            ("operating", Rules(flights: Flights(operating: [2]))),
            ("number and id", Rules(flights: Flights(numbers: ["111"], ids: [101]))),
            ("aircraft", Rules(flights: Flights(aircraft: [1]))),
            ("fare", Rules(fares: Fares(fares: [501], families: [5]))),
            ("fare basis cabin rbd", Rules(fares: Fares(bases: ["YOW"], cabins: [1], rbds: [11]))),
            ("fare type", Rules(fares: Fares(types: [(AirFareType)2]))),
            ("dates", Rules(dates: new([new(new DateOnly(2026, 12, 1), new DateOnly(2026, 12, 31))], [new(new DateOnly(2026, 12, 24), new DateOnly(2026, 12, 26))]))),
            ("day time", Rules(dayTime: new([new(127, new TimeOnly(6, 0), new TimeOnly(12, 0), DayTimeRestrictionEffect.Allow), new(4, null, null, DayTimeRestrictionEffect.Deny)]))),
            ("advance days", Rules(advance: new(30, TimeUnit.Days, false))),
            ("advance hours", Rules(advance: new(48, TimeUnit.Hours, false))),
            ("with ticket", Rules(advance: new(0, TimeUnit.Days, true))),
            ("mixed", Rules(passengers: new([PassengerTypeCode.ADT], []), flights: Flights(aircraft: [1]), fares: Fares(families: [5]), dates: new([], [new(new DateOnly(2026, 12, 2), new DateOnly(2026, 12, 2))])))
        ];

        return sets.Select(set => new object[] { set.Name, set.Rules });
    }

    [Theory]
    [MemberData(nameof(OracleRules))]
    public async Task RULE_ORACLE_the_engine_decides_like_the_phase_one_reference_oracle(string name, ProvisionRulesArgs rules)
    {
        (string Name, AncillaryShoppingContext Context)[] contexts =
        [
            ("default", Trip.Context()),
            ("child", Trip.Context(travellers: [Trip.Child("T1")])),
            ("no birth date", Trip.Context(travellers: [Trip.Adult() with { DateOfBirth = null }])),
            ("no zone", Trip.Context([Trip.Flight(zone: null)])),
            ("no fare facts", Trip.Context() with { TravellerFareFacts = [] }),
            ("customer", Trip.Context() with { CustomerId = 77, CustomerType = (CustomerType)1 }),
            ("other route", Trip.Context([Trip.Flight(origin: Trip.Ist, destination: Trip.Mhd) with { ViaAirportIds = [Trip.Ika], OperatingAirlineId = 2, AircraftId = null, DestinationCountryId = null }])),
            ("christmas noon", Trip.Context([Trip.Flight(departure: new DateTimeOffset(2026, 12, 25, 12, 0, 0, Trip.TehranOffset))])),
            ("wednesday morning", Trip.Context([Trip.Flight(departure: new DateTimeOffset(2026, 12, 2, 8, 0, 0, Trip.TehranOffset))])),
            ("tomorrow", Trip.Context([Trip.Flight(departure: Now.AddHours(30).ToOffset(Trip.TehranOffset))])),
            ("ticketed now", Trip.Context() with { SourceIdentity = new SourceIdentityContext { SourceKind = ShoppingSourceKind.Offer, TicketedAtUtc = Now } }),
            ("other fare", With(Trip.Context(), fare => fare with { FareFamilyId = 9, AirFareType = (AirFareType)2, RbdId = null }))
        ];

        foreach (var (disposition, sequence) in new[] { (CommercialDisposition.Free, 10), (CommercialDisposition.NotAvailable, 5) })
        {
            var lab = new ShoppingLab();
            var definition = lab.Definition("A23", specification: Plain);
            var broad = lab.Provision(definition, sequence: 10, disposition: CommercialDisposition.Free);
            var specific = sequence == 10 ? broad : lab.Provision(definition, sequence: sequence, rules: rules, disposition: disposition);

            if (sequence == 10)
            {
                lab.Catalog.Provisions.Clear();
                specific = lab.Provision(definition, sequence: 10, rules: rules, disposition: disposition);
            }

            foreach (var (label, context) in contexts)
            {
                var expected = ProvisionRuleOracle.Select(lab.Catalog.Provisions, Oracle(context));
                var result = await lab.Engine.ShopAsync(context, new ShoppingFilter { IncludeNonSellable = true });
                var candidate = result.Candidates.SingleOrDefault();
                var actual = candidate switch
                {
                    null => OracleOutcome.NoMatch,
                    { Eligibility.Status: EligibilityStatus.InsufficientContext } => OracleOutcome.UnsupportedContext,
                    { OfferReadiness: OfferReadiness.Unavailable } => OracleOutcome.NotAvailable,
                    _ => OracleOutcome.Free
                };

                Assert.True(expected.Outcome == actual, $"{name} / {disposition} / {label}: oracle {expected.Outcome}, engine {actual}");
                Assert.Equal(expected.ProvisionId, candidate?.ProvisionId);
            }

            Assert.NotNull(specific);
        }
    }

    private static OracleContext Oracle(AncillaryShoppingContext context)
    {
        var flight = context.Flights.Single();
        var traveller = context.Travellers.Single();
        var fare = context.TravellerFareFacts.SingleOrDefault();

        return new OracleContext
        {
            SaleInstant = context.EvaluatedAtUtc,
            TicketedAt = context.SourceIdentity.TicketedAtUtc,
            Occurrence = OracleOccurrence.AtInstant(flight.DepartureAt, flight.OriginIanaTimeZoneId is null ? null : TimeZoneInfo.FindSystemTimeZoneById(flight.OriginIanaTimeZoneId)),
            PassengerType = traveller.PassengerTypeCode,
            DateOfBirth = traveller.DateOfBirth,
            PointOfSaleId = context.PointOfSaleId,
            CustomerId = context.CustomerId,
            CustomerType = context.CustomerType,
            OriginAirportId = flight.OriginAirportId,
            DestinationAirportId = flight.DestinationAirportId,
            ViaAirportIds = flight.ViaAirportIds,
            CoverageCountryId = flight.DestinationCountryId,
            MarketingAirlineId = flight.MarketingAirlineId,
            OperatingAirlineId = flight.OperatingAirlineId,
            FlightNumber = flight.FlightNumber,
            FlightId = flight.FlightId,
            AircraftId = flight.AircraftId,
            AirFareId = fare?.AirFareId,
            AirFareType = fare?.AirFareType,
            FareFamilyId = fare?.FareFamilyId,
            FareBasisCode = fare?.FareBasisCode,
            CabinClassId = fare?.CabinClassId ?? (fare is null ? null : flight.CabinClassId),
            RbdId = fare?.RbdId ?? (fare is null ? null : flight.RbdId)
        };
    }
}
