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
public class V12FamilyAcceptanceTests
{
    private readonly TestDatabase _database;
    private readonly FixedClock _clock = new();
    private readonly FamilyProof _proof;

    public V12FamilyAcceptanceTests(TestDatabase database)
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

    private static FamilyPricingProof Flat(PricingUnit pricingUnit, decimal draft, decimal edited, string name)
        => new(
            provisionId => Pricing(provisionId, Eur, Base(draft, name: name)),
            provisionId => Pricing(provisionId, Eur, Base(edited, name: name)),
            pricing => Assert.Equal(
                (pricingUnit.ToString(), "EUR", name, draft, draft),
                (pricing.PricingUnit!.Name, pricing.Currency, pricing.PriceLines.Single().Name, pricing.Rates.Single().BaseAmount, pricing.Rates.Single().TotalAmount)),
            pricing => Assert.Equal(
                (pricingUnit.ToString(), "EUR", name, edited, edited),
                (pricing.PricingUnit!.Name, pricing.Currency, pricing.PriceLines.Single().Name, pricing.Rates.Single().BaseAmount, pricing.Rates.Single().TotalAmount)));

    private Task RefusedAsync(int code, int httpStatus, Func<AncillaryScope, Task> request)
        => BusinessAssert.ThrowsAsync(code, httpStatus, async () =>
        {
            await using var scope = new AncillaryScope(_database, _clock);

            await request(scope);
        });

    [Fact]
    public async Task V12_FAM01_extra_baggage_is_filed_per_piece_and_per_kilogram_as_separate_services_with_every_outcome()
    {
        var airlineId = _database.NextAirlineId();
        var supplierId = await _proof.SupplierAsync(M1Commands.LocalSupplier(airlineId));
        var winter = Period(Date(2026, 12, 20), Date(2026, 12, 31));
        var spring = Period(Date(2027, 3, 15), Date(2027, 4, 5));

        var piece = await _proof.ProveAsync(
            FirstExcessBagDefinition(airlineId, supplierId),
            definitionId => Provision(
                definitionId,
                10,
                coverageScope: ServiceCoverageScope.Journey,
                quantityUnit: AncillaryQuantityUnit.Piece,
                maxQuantity: 3,
                passenger: Passengers(PassengerTypeCode.ADT),
                sales: new ProvisionSalesCriteriaInput(PointOfSaleIds: [AgencyOffice]),
                travel: new ProvisionTravelCriteriaInput(RoutePairs: [Pair(Thr, Ist)], FlightIds: [81234], SeasonalPeriods: [winter]),
                fare: new ProvisionFareCriteriaInput(FareFamilyIds: [5]),
                application: Baggage(23m, 1, 1, BaggageTravelApplication.AllSectors)),
            draft => draft with
            {
                Travel = draft.Travel! with { SeasonalPeriods = [winter, spring] },
                Application = Baggage(25m, 1, 1, BaggageTravelApplication.AllSectors)
            },
            definition =>
            {
                Assert.Equal(("Industry", "0CC", "C", "BG", "B1"), (definition.SubCodeSource.Name, definition.ServiceSubCode, definition.ServiceTypeCode, definition.GroupCode, definition.Description1Code));
                Assert.Equal(("EmdAssociated", "C", "0CC"), (definition.DocumentType.Name, definition.DocumentRfic, definition.DocumentRfisc));
                Assert.Equal(("Ssr", "XBAG", "PerPiece"), (definition.BookingMethod.Name, definition.BookingSsrCode, definition.PricingUnit!.Name));
            },
            draft =>
            {
                Assert.Equal(("Baggage", "Journey", "Piece", 1, 3, "Paid"), (draft.ApplicationType.Name, draft.CoverageScope.Name, draft.QuantityUnit.Name, draft.MinQuantity, draft.MaxQuantity, draft.Disposition.Name));
                Assert.Equal((1, 1, 23m, "Kg", "AllSectors", "Prepaid"), (draft.Baggage!.FirstExcessPiece, draft.Baggage.LastExcessPiece, draft.Baggage.Weight, draft.Baggage.WeightUnit.Name, draft.Baggage.TravelApplication!.Name, draft.Baggage.PurchaseApplication.Name));
                Assert.Equal(new[] { "ADT" }, Names(draft.Passenger.PassengerTypes));
                Assert.Equal(new[] { AgencyOffice }, Values(draft.Sales.PointsOfSale));
                Assert.Equal((Thr, Ist, "Directional"), draft.Travel.RoutePairs.Select(pair => (pair.OriginAirportId, pair.DestinationAirportId, pair.Direction.Name)).Single());
                Assert.Equal(new long[] { 81234 }, Values(draft.Travel.Flights));
                Assert.Equal(new[] { (Date(2026, 12, 20), Date(2026, 12, 31)) }, Periods(draft.Travel.SeasonalPeriods));
                Assert.Equal(new long[] { 5 }, Values(draft.Fare.FareFamilies));
            },
            edited =>
            {
                Assert.Equal(25m, edited.Baggage!.Weight);
                Assert.Equal(
                    new[] { (Date(2026, 12, 20), Date(2026, 12, 31)), (Date(2027, 3, 15), Date(2027, 4, 5)) },
                    Periods(edited.Travel.SeasonalPeriods));
                Assert.Equal(new long[] { 81234 }, Values(edited.Travel.Flights));
                Assert.Equal(new[] { "ADT" }, Names(edited.Passenger.PassengerTypes));
            },
            new FamilyPricingProof(
                provisionId => Pricing(provisionId, Eur, Base(30m, name: "Extra bag")),
                provisionId => Pricing(provisionId, Eur, Base(32m, name: "Extra bag"), Tax("VAT", 3.2m, countryId: 1, name: "Value added tax")),
                pricing => Assert.Equal(("PerPiece", "EUR", 30m, 30m), (pricing.PricingUnit!.Name, pricing.Currency, pricing.Rates.Single().BaseAmount, pricing.Rates.Single().TotalAmount)),
                pricing => Assert.Equal(("PerPiece", 32m, 3.2m, 35.2m), (pricing.PricingUnit!.Name, pricing.Rates.Single().BaseAmount, pricing.Rates.Single().TaxAmount, pricing.Rates.Single().TotalAmount))));

        var kilogram = await _proof.DefinitionAsync(
            CarrierDefinition(airlineId, supplierId, "XBAG_KILO", "XKG", "C", "BG", "Extra baggage per kilogram", Ssr("XBAG"), pricingUnit: PricingUnit.PerKilogram));
        var perKilogram = await _proof.RuleAsync(
            Provision(
                kilogram.Id,
                10,
                coverageScope: ServiceCoverageScope.Journey,
                quantityUnit: AncillaryQuantityUnit.Kilogram,
                minQuantity: 5,
                maxQuantity: 30,
                travel: new ProvisionTravelCriteriaInput(
                    RoutePairs: [Pair(Thr, Ist, RoutePairDirection.BothDirections)],
                    SeasonalPeriods: [winter],
                    FlightNumbers: ["W5112"]),
                application: Baggage(30m)),
            provisionId => Pricing(provisionId, Eur, Base(4.5m, name: "Extra kilogram")));
        var free = await _proof.RuleAsync(
            Provision(
                piece.ServiceDefinitionId,
                20,
                CommercialDisposition.Free,
                ServiceCoverageScope.Journey,
                AncillaryQuantityUnit.Piece,
                fare: new ProvisionFareCriteriaInput(FareFamilyIds: [6]),
                application: Baggage(23m, 1, 1)));
        var notAvailable = await _proof.RuleAsync(
            Provision(
                piece.ServiceDefinitionId,
                30,
                CommercialDisposition.NotAvailable,
                ServiceCoverageScope.Journey,
                AncillaryQuantityUnit.Piece,
                passenger: Passengers(PassengerTypeCode.INF),
                application: Baggage(23m, 1, 1)));

        Assert.NotEqual(piece.ServiceDefinitionId, kilogram.Id);
        Assert.Equal(("CarrierDefined", "XKG", "PerKilogram", "Active"), (kilogram.SubCodeSource.Name, kilogram.ServiceSubCode, kilogram.PricingUnit!.Name, kilogram.Status.Name));
        Assert.Equal(("Active", "Kilogram", 5, 30, 30m), (perKilogram.Provision.Status.Name, perKilogram.Provision.QuantityUnit.Name, perKilogram.Provision.MinQuantity, perKilogram.Provision.MaxQuantity, perKilogram.Provision.Baggage!.Weight));
        Assert.Null(perKilogram.Provision.Baggage.FirstExcessPiece);
        Assert.Equal(new[] { "W5112" }, Values(perKilogram.Provision.Travel.FlightNumbers));
        Assert.Equal(("PerKilogram", "Active", 4.5m), (perKilogram.Pricing!.PricingUnit!.Name, perKilogram.Pricing.Status.Name, perKilogram.Pricing.Rates.Single().TotalAmount));
        Assert.Equal(("Active", "Free"), (free.Provision.Status.Name, free.Provision.Disposition.Name));
        Assert.Equal(new long[] { 6 }, Values(free.Provision.Fare.FareFamilies));
        Assert.Equal(("Active", "NotAvailable"), (notAvailable.Provision.Status.Name, notAvailable.Provision.Disposition.Name));
        Assert.Equal(new[] { "INF" }, Names(notAvailable.Provision.Passenger.PassengerTypes));
        Assert.Equal(
            new[] { (10, "Paid", 2), (20, "Free", 0), (30, "NotAvailable", 0) },
            (await _proof.ListedProvisionsAsync(piece.ServiceDefinitionId)).Select(row => (row.Sequence, row.Disposition.Name, row.SeasonalPeriodCount)));
        await RefusedAsync(16508, 409, scope => scope.DefinePricing.DefineAsync(Pricing(perKilogram.Provision.Id, Eur, Base(4.5m, PassengerTypeCode.ADT))));
    }

    [Fact]
    public async Task V12_FAM02_sports_equipment_has_its_own_identity_with_permitted_dates_an_allowance_and_a_price()
    {
        var airlineId = _database.NextAirlineId();
        var supplierId = await _proof.SupplierAsync(M1Commands.LocalSupplier(airlineId));
        DateOnly[] saturdays = [Date(2027, 1, 9), Date(2027, 1, 16), Date(2027, 1, 23)];

        var ski = await _proof.ProveAsync(
            CarrierDefinition(airlineId, supplierId, "SPORT_SKI", "SKI", "C", "SB", "Ski equipment", Ssr("SPEQ"), subGroupCode: "SK", pricingUnit: PricingUnit.PerPiece),
            definitionId => Provision(
                definitionId,
                10,
                coverageScope: ServiceCoverageScope.Journey,
                quantityUnit: AncillaryQuantityUnit.Piece,
                maxQuantity: 2,
                travel: new ProvisionTravelCriteriaInput(
                    RoutePairs: [Pair(Thr, Ist, RoutePairDirection.BothDirections)],
                    TravelDates: saturdays,
                    AircraftIds: [1, 2]),
                application: Baggage(32m, purchaseApplication: BaggagePurchaseApplication.PrepaidAndCheckIn)),
            draft => draft with
            {
                Travel = draft.Travel! with { TravelDates = [.. saturdays, Date(2027, 1, 30)] },
                Application = Baggage(30m, purchaseApplication: BaggagePurchaseApplication.CheckIn)
            },
            definition =>
            {
                Assert.Equal(("CarrierDefined", "SKI", "C", "SB", "SK", "PerPiece"), (definition.SubCodeSource.Name, definition.ServiceSubCode, definition.ServiceTypeCode, definition.GroupCode, definition.SubGroupCode, definition.PricingUnit!.Name));
                Assert.Equal(("Ssr", "SPEQ", "None"), (definition.BookingMethod.Name, definition.BookingSsrCode, definition.DocumentType.Name));
            },
            draft =>
            {
                Assert.Equal(("Baggage", 32m, "PrepaidAndCheckIn", 2), (draft.ApplicationType.Name, draft.Baggage!.Weight, draft.Baggage.PurchaseApplication.Name, draft.MaxQuantity));
                Assert.Equal((Thr, Ist, "BothDirections"), draft.Travel.RoutePairs.Select(pair => (pair.OriginAirportId, pair.DestinationAirportId, pair.Direction.Name)).Single());
                Assert.Equal(saturdays, Values(draft.Travel.TravelDates));
                Assert.Equal(new[] { 1, 2 }, Values(draft.Travel.Aircraft));
            },
            edited =>
            {
                Assert.Equal((30m, "CheckIn"), (edited.Baggage!.Weight, edited.Baggage.PurchaseApplication.Name));
                Assert.Equal(saturdays.Append(Date(2027, 1, 30)), Values(edited.Travel.TravelDates));
            },
            Flat(PricingUnit.PerPiece, 45m, 48m, "Ski equipment"));

        var golf = await _proof.DefinitionAsync(
            CarrierDefinition(airlineId, supplierId, "SPORT_GOLF", "GLF", "C", "SB", "Golf bag", Ssr("SPEQ"), subGroupCode: "GF", pricingUnit: PricingUnit.PerPiece));
        var golfRule = await _proof.RuleAsync(
            Provision(
                golf.Id,
                10,
                coverageScope: ServiceCoverageScope.Journey,
                quantityUnit: AncillaryQuantityUnit.Piece,
                travel: new ProvisionTravelCriteriaInput(TravelDates: [Date(2027, 5, 1), Date(2027, 5, 2)]),
                application: Baggage(15m)),
            provisionId => Pricing(provisionId, Eur, Base(40m, name: "Golf bag")));

        Assert.NotEqual(ski.ServiceDefinitionId, golf.Id);
        Assert.Equal(("GLF", "GF", "PerPiece", "Active"), (golf.ServiceSubCode, golf.SubGroupCode, golf.PricingUnit!.Name, golf.Status.Name));
        Assert.Equal(("Active", 15m, 2), (golfRule.Provision.Status.Name, golfRule.Provision.Baggage!.Weight, golfRule.Provision.Travel.TravelDates.Count));
        Assert.Equal(("PerPiece", "Active", 40m), (golfRule.Pricing!.PricingUnit!.Name, golfRule.Pricing.Status.Name, golfRule.Pricing.Rates.Single().TotalAmount));
        Assert.Equal(4, (await _proof.ListedProvisionsAsync(ski.ServiceDefinitionId)).Single().TravelDateCount);
    }

    [Fact]
    public async Task V12_FAM03_wheelchair_assistance_uses_ssr_metadata_is_free_or_not_available_and_never_fabricates_an_industry_code()
    {
        var airlineId = _database.NextAirlineId();
        var supplierId = await _proof.SupplierAsync(M1Commands.LocalSupplier(airlineId));

        var ramp = await _proof.ProveAsync(
            CarrierDefinition(airlineId, supplierId, "WHEELCHAIR_RAMP", "WCR", "F", "AS", "Wheelchair to aircraft door", Ssr("WCHR")),
            definitionId => Provision(
                definitionId,
                10,
                CommercialDisposition.Free,
                travel: new ProvisionTravelCriteriaInput(OriginAirportIds: [Ika, Thr])),
            draft => draft with { Travel = new ProvisionTravelCriteriaInput(OriginAirportIds: [Ika, Thr, Mhd]), Outcome = draft.Outcome with { BookingRequired = true } },
            definition =>
            {
                Assert.Equal(("CarrierDefined", "WCR", "F", "AS"), (definition.SubCodeSource.Name, definition.ServiceSubCode, definition.ServiceTypeCode, definition.GroupCode));
                Assert.Equal(("Ssr", "WCHR", "None"), (definition.BookingMethod.Name, definition.BookingSsrCode, definition.DocumentType.Name));
            },
            draft =>
            {
                Assert.Equal(("Standard", "Free"), (draft.ApplicationType.Name, draft.Disposition.Name));
                Assert.Equal(new[] { Ika, Thr }, Values(draft.Travel.OriginAirports));
                Assert.False(draft.BookingRequired);
            },
            edited =>
            {
                Assert.Equal(new[] { Ika, Thr, Mhd }, Values(edited.Travel.OriginAirports));
                Assert.True(edited.BookingRequired);
                Assert.Equal("Free", edited.Disposition.Name);
            });

        Assert.Null(ramp.PricingId);
        await RefusedAsync(16505, 409, scope => scope.DefinePricing.DefineAsync(Pricing(ramp.ProvisionId, Eur, Base(20m))));

        foreach (var (reference, subCode, ssr, name) in new[]
                 {
                     ("WHEELCHAIR_STEPS", "WCS", "WCHS", "Wheelchair up the steps"),
                     ("WHEELCHAIR_CABIN", "WCC", "WCHC", "Wheelchair to the cabin seat")
                 })
        {
            var definition = await _proof.DefinitionAsync(CarrierDefinition(airlineId, supplierId, reference, subCode, "F", "AS", name, Ssr(ssr)));
            var notAvailable = await _proof.RuleAsync(
                Provision(definition.Id, 10, CommercialDisposition.NotAvailable, travel: new ProvisionTravelCriteriaInput(AircraftIds: [4])));
            var free = await _proof.RuleAsync(Provision(definition.Id, 100, CommercialDisposition.Free, bookingRequired: true));

            Assert.Equal((ssr, "CarrierDefined", "Active"), (definition.BookingSsrCode, definition.SubCodeSource.Name, definition.Status.Name));
            Assert.Equal(("NotAvailable", "Active"), (notAvailable.Provision.Disposition.Name, notAvailable.Provision.Status.Name));
            Assert.Equal(new[] { 4 }, Values(notAvailable.Provision.Travel.Aircraft));
            Assert.Equal(("Free", "Active", true), (free.Provision.Disposition.Name, free.Provision.Status.Name, free.Provision.BookingRequired));
            Assert.Null(notAvailable.Pricing);
            Assert.Null(free.Pricing);
        }

        await BusinessAssert.ThrowsAsync(16207, 422, () => _proof.DefinitionAsync(
            CarrierDefinition(airlineId, supplierId, "WHEELCHAIR_FAKE", "0WC", "F", "AS", "Wheelchair", Ssr("WCHR")) with { SubCodeSource = ServiceSubCodeSource.Industry },
            activate: false));
    }

    [Fact]
    public async Task V12_FAM04_a_meal_is_filed_for_adults_and_children_by_flight_and_cabin_with_its_booking_metadata()
    {
        var airlineId = _database.NextAirlineId();
        var supplierId = await _proof.SupplierAsync(M1Commands.LocalSupplier(airlineId, "Sky Catering"));

        var meal = await _proof.ProveAsync(
            CarrierDefinition(
                airlineId,
                supplierId,
                "MEAL_VGML",
                "MVG",
                "F",
                "ML",
                "Vegetarian meal",
                Ssr("VGML"),
                new ServiceDefinitionDocumentInput(AncillaryDocumentType.EmdAssociated, "G", "MVG"),
                "VG"),
            definitionId => Provision(
                definitionId,
                10,
                passenger: Passengers(PassengerTypeCode.ADT, PassengerTypeCode.CHD),
                travel: new ProvisionTravelCriteriaInput(
                    SeasonalPeriods: [Period(Date(2026, 12, 1), Date(2027, 2, 28))],
                    FlightNumbers: ["W5112"],
                    FlightIds: [81234, 81240]),
                fare: new ProvisionFareCriteriaInput(FareFamilyIds: [5, 6], CabinClassIds: [2]),
                advancePurchase: new ProvisionAdvancePurchaseInput(24, TimeUnit.Hours),
                bookingRequired: true),
            draft => draft with
            {
                Fare = new ProvisionFareCriteriaInput(FareFamilyIds: [5], CabinClassIds: [1, 2]),
                AdvancePurchase = new ProvisionAdvancePurchaseInput(2, TimeUnit.Days)
            },
            definition =>
            {
                Assert.Equal(("MVG", "ML", "VG", "Ssr", "VGML"), (definition.ServiceSubCode, definition.GroupCode, definition.SubGroupCode, definition.BookingMethod.Name, definition.BookingSsrCode));
                Assert.Equal(("EmdAssociated", "G", "MVG", "PerPassenger"), (definition.DocumentType.Name, definition.DocumentRfic, definition.DocumentRfisc, definition.PricingUnit!.Name));
            },
            draft =>
            {
                Assert.Equal(new[] { "ADT", "CHD" }, Names(draft.Passenger.PassengerTypes));
                Assert.Equal(new long[] { 81234, 81240 }, Values(draft.Travel.Flights));
                Assert.Equal(new[] { "W5112" }, Values(draft.Travel.FlightNumbers));
                Assert.Equal(new[] { (Date(2026, 12, 1), Date(2027, 2, 28)) }, Periods(draft.Travel.SeasonalPeriods));
                Assert.Equal(new long[] { 5, 6 }, Values(draft.Fare.FareFamilies));
                Assert.Equal(new[] { 2 }, Values(draft.Fare.CabinClasses));
                Assert.Equal((24, "Hours", true), (draft.AdvancePurchase!.Period, draft.AdvancePurchase.Unit.Name, draft.BookingRequired));
            },
            edited =>
            {
                Assert.Equal(new long[] { 5 }, Values(edited.Fare.FareFamilies));
                Assert.Equal(new[] { 1, 2 }, Values(edited.Fare.CabinClasses).OrderBy(id => id));
                Assert.Equal((2, "Days"), (edited.AdvancePurchase!.Period, edited.AdvancePurchase.Unit.Name));
                Assert.Equal(new[] { "ADT", "CHD" }, Names(edited.Passenger.PassengerTypes));
            },
            new FamilyPricingProof(
                provisionId => Pricing(provisionId, Eur, Base(12m, PassengerTypeCode.ADT, name: "Vegetarian meal"), Base(8m, PassengerTypeCode.CHD, name: "Vegetarian meal")),
                provisionId => Pricing(provisionId, Eur, Base(13.5m, PassengerTypeCode.ADT, name: "Vegetarian meal"), Base(8m, PassengerTypeCode.CHD, name: "Vegetarian meal")),
                pricing => Assert.Equal(new (string?, int?, int?, decimal, decimal)[] { ("ADT", null, null, 12m, 12m), ("CHD", null, null, 8m, 8m) }, Rates(pricing)),
                pricing => Assert.Equal(new (string?, int?, int?, decimal, decimal)[] { ("ADT", null, null, 13.5m, 13.5m), ("CHD", null, null, 8m, 8m) }, Rates(pricing))));

        var childMeal = await _proof.DefinitionAsync(
            CarrierDefinition(airlineId, supplierId, "MEAL_CHML", "MCH", "F", "ML", "Child meal", Ssr("CHML"), subGroupCode: "CH"));
        var childRule = await _proof.RuleAsync(
            Provision(
                childMeal.Id,
                10,
                passenger: Passengers(PassengerTypeCode.CHD, PassengerTypeCode.INS),
                fare: new ProvisionFareCriteriaInput(CabinClassIds: [1, 2]),
                bookingRequired: true),
            provisionId => Pricing(provisionId, Eur, Base(6m, PassengerTypeCode.CHD), Base(6m, PassengerTypeCode.INS)));

        Assert.NotEqual(meal.ServiceDefinitionId, childMeal.Id);
        Assert.Equal(("CHML", "MCH", "Ssr"), (childMeal.BookingSsrCode, childMeal.ServiceSubCode, childMeal.BookingMethod.Name));
        Assert.Equal(new[] { "CHD", "INS" }, Names(childRule.Provision.Passenger.PassengerTypes));
        Assert.Equal(new (string?, int?, int?, decimal, decimal)[] { ("CHD", null, null, 6m, 6m), ("INS", null, null, 6m, 6m) }, Rates(childRule.Pricing!));
    }

    [Fact]
    public async Task V12_FAM05_travel_insurance_is_filed_per_passenger_with_distinct_rates_below_and_from_sixty_five()
    {
        var airlineId = _database.NextAirlineId();
        var insurerId = await _proof.SupplierAsync(new TestRegisterSupplierCommand(airlineId, "SafeTrip Insurance", SupplierFulfillmentKind.External, "InsurancePartnerA"));

        var basic = await _proof.ProveAsync(
            CarrierDefinition(
                airlineId,
                insurerId,
                "INS_BASIC",
                "INB",
                "M",
                "IN",
                "Travel insurance basic",
                salesEffectiveFrom: Date(2026, 10, 1),
                salesDiscontinueOn: Date(2027, 9, 30)),
            definitionId => Provision(
                definitionId,
                10,
                coverageScope: ServiceCoverageScope.Journey,
                salesEffectiveFrom: new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.Zero),
                salesDiscontinueAt: new DateTimeOffset(2027, 1, 1, 0, 0, 0, TimeSpan.Zero),
                sales: new ProvisionSalesCriteriaInput(PointOfSaleIds: [AirlineOffice, AgencyOffice], CustomerTypes: [CustomerType.Individual]),
                travel: new ProvisionTravelCriteriaInput(SeasonalPeriods: [Period(Date(2026, 10, 1), Date(2026, 12, 31))])),
            draft => draft with
            {
                Sales = new ProvisionSalesCriteriaInput(PointOfSaleIds: [AirlineOffice], CustomerTypes: [CustomerType.Individual, CustomerType.Organization]),
                Travel = new ProvisionTravelCriteriaInput(SeasonalPeriods: [Period(Date(2026, 10, 1), Date(2027, 1, 31))]),
                SalesDiscontinueAt = new DateTimeOffset(2027, 2, 1, 0, 0, 0, TimeSpan.Zero)
            },
            definition =>
            {
                Assert.Equal((insurerId, "SafeTrip Insurance", "INB", "M", "IN", "PerPassenger"), (definition.SupplierId, definition.SupplierName, definition.ServiceSubCode, definition.ServiceTypeCode, definition.GroupCode, definition.PricingUnit!.Name));
                Assert.Equal((Date(2026, 10, 1), Date(2027, 9, 30)), (definition.SalesEffectiveFrom!.Value, definition.SalesDiscontinueOn!.Value));
                Assert.Equal(("None", "NoBookingProcessRequired"), (definition.DocumentType.Name, definition.BookingMethod.Name));
            },
            draft =>
            {
                Assert.Equal(("Standard", "Journey"), (draft.ApplicationType.Name, draft.CoverageScope.Name));
                Assert.Empty(draft.Passenger.PassengerTypes);
                Assert.Equal(new[] { AirlineOffice, AgencyOffice }, Values(draft.Sales.PointsOfSale));
                Assert.Equal(new[] { "Individual" }, Names(draft.Sales.CustomerTypes));
                Assert.Equal(new[] { (Date(2026, 10, 1), Date(2026, 12, 31)) }, Periods(draft.Travel.SeasonalPeriods));
                Assert.Equal(new DateTimeOffset(2027, 1, 1, 0, 0, 0, TimeSpan.Zero), draft.SalesDiscontinueAt);
            },
            edited =>
            {
                Assert.Equal(new[] { AirlineOffice }, Values(edited.Sales.PointsOfSale));
                Assert.Equal(new[] { "Individual", "Organization" }, Names(edited.Sales.CustomerTypes));
                Assert.Equal(new[] { (Date(2026, 10, 1), Date(2027, 1, 31)) }, Periods(edited.Travel.SeasonalPeriods));
                Assert.Equal(new DateTimeOffset(2027, 2, 1, 0, 0, 0, TimeSpan.Zero), edited.SalesDiscontinueAt);
            },
            new FamilyPricingProof(
                provisionId => Pricing(provisionId, Eur, Base(9m, null, 0, 65), Base(22m, null, 65)),
                provisionId => Pricing(provisionId, Eur, Base(9.5m, null, 0, 65), Base(23m, null, 65)),
                pricing => Assert.Equal(new (string?, int?, int?, decimal, decimal)[] { (null, 0, 65, 9m, 9m), (null, 65, null, 22m, 22m) }, Rates(pricing)),
                pricing => Assert.Equal(new (string?, int?, int?, decimal, decimal)[] { (null, 0, 65, 9.5m, 9.5m), (null, 65, null, 23m, 23m) }, Rates(pricing))));

        var plus = await _proof.DefinitionAsync(CarrierDefinition(airlineId, insurerId, "INS_PLUS", "INP", "M", "IN", "Travel insurance plus"));
        var plusRule = await _proof.RuleAsync(
            Provision(
                plus.Id,
                10,
                coverageScope: ServiceCoverageScope.Order,
                travel: new ProvisionTravelCriteriaInput(SeasonalPeriods: [Period(Date(2026, 10, 1), Date(2027, 9, 30))])),
            provisionId => Pricing(
                provisionId,
                Eur,
                Base(25m, PassengerTypeCode.ADT, 0, 65),
                Base(55m, PassengerTypeCode.ADT, 65),
                Base(12m, PassengerTypeCode.CHD),
                Base(5m, PassengerTypeCode.INF)));

        Assert.NotEqual(basic.ServiceDefinitionId, plus.Id);
        Assert.Equal(("INP", insurerId, "PerPassenger"), (plus.ServiceSubCode, plus.SupplierId, plus.PricingUnit!.Name));
        Assert.Equal(("Order", "Active"), (plusRule.Provision.CoverageScope.Name, plusRule.Provision.Status.Name));
        Assert.Equal(
            new (string?, int?, int?, decimal, decimal)[] { ("ADT", 0, 65, 25m, 25m), ("ADT", 65, null, 55m, 55m), ("CHD", null, null, 12m, 12m), ("INF", null, null, 5m, 5m) },
            Rates(plusRule.Pricing!));
        await RefusedAsync(16508, 409, scope => scope.DefinePricing.DefineAsync(
            Pricing(plusRule.Provision.Id, Eur, Base(25m, PassengerTypeCode.ADT, 0, 66), Base(55m, PassengerTypeCode.ADT, 65))));
    }

    [Fact]
    public async Task V12_FAM06_a_paid_seat_is_filed_per_seat_by_aircraft_seat_number_and_seat_characteristic()
    {
        var airlineId = _database.NextAirlineId();
        var supplierId = await _proof.SupplierAsync(M1Commands.LocalSupplier(airlineId));

        var seat = await _proof.ProveAsync(
            CarrierDefinition(airlineId, supplierId, "SEAT_SELECT", "STS", "F", "ST", "Seat selection", pricingUnit: PricingUnit.PerSeat),
            definitionId => Provision(
                definitionId,
                10,
                travel: new ProvisionTravelCriteriaInput(AircraftIds: [1]),
                fare: new ProvisionFareCriteriaInput(CabinClassIds: [1]),
                application: Seat([" 1a", "1B", "1C"], null)),
            draft => draft with { Application = Seat(["1A", "1B", "1C", "1D"], ["W"]) },
            definition => Assert.Equal(("STS", "F", "ST", "PerSeat"), (definition.ServiceSubCode, definition.ServiceTypeCode, definition.GroupCode, definition.PricingUnit!.Name)),
            draft =>
            {
                Assert.Equal("Seat", draft.ApplicationType.Name);
                Assert.Equal(new[] { "1A", "1B", "1C" }, Values(draft.Seat!.SeatNumbers));
                Assert.Empty(draft.Seat.SeatCharacteristics);
                Assert.Equal(new[] { 1 }, Values(draft.Travel.Aircraft));
                Assert.Equal(new[] { 1 }, Values(draft.Fare.CabinClasses));
                Assert.Null(draft.Baggage);
            },
            edited =>
            {
                Assert.Equal(new[] { "1A", "1B", "1C", "1D" }, Values(edited.Seat!.SeatNumbers));
                Assert.Equal(new[] { "W" }, Values(edited.Seat.SeatCharacteristics));
                Assert.Equal(new[] { 1 }, Values(edited.Travel.Aircraft));
            },
            Flat(PricingUnit.PerSeat, 40m, 42m, "Front row seat"));

        var byCharacteristic = await _proof.RuleAsync(
            Provision(
                seat.ServiceDefinitionId,
                20,
                passenger: Passengers(PassengerTypeCode.ADT),
                sales: new ProvisionSalesCriteriaInput(PointOfSaleIds: [AgencyOffice]),
                travel: new ProvisionTravelCriteriaInput(SeasonalPeriods: [Period(Date(2026, 12, 1), Date(2027, 3, 31))], AircraftIds: [1, 2]),
                fare: new ProvisionFareCriteriaInput(FareFamilyIds: [5], CabinClassIds: [2], RbdIds: [41, 42]),
                application: Seat(null, ["w", "LS"])),
            provisionId => Pricing(provisionId, Eur, Base(15m, name: "Window or legroom seat")));

        Assert.Equal(("Seat", "Active"), (byCharacteristic.Provision.ApplicationType.Name, byCharacteristic.Provision.Status.Name));
        Assert.Empty(byCharacteristic.Provision.Seat!.SeatNumbers);
        Assert.Equal(new[] { "W", "LS" }, Values(byCharacteristic.Provision.Seat.SeatCharacteristics));
        Assert.Equal(new long[] { 41, 42 }, Values(byCharacteristic.Provision.Fare.Rbds));
        Assert.Equal(new[] { 1, 2 }, Values(byCharacteristic.Provision.Travel.Aircraft));
        Assert.Equal(("PerSeat", "Active", 15m), (byCharacteristic.Pricing!.PricingUnit!.Name, byCharacteristic.Pricing.Status.Name, byCharacteristic.Pricing.Rates.Single().TotalAmount));
        await RefusedAsync(16302, 422, scope => scope.DefineProvision.DefineAsync(
            Provision(seat.ServiceDefinitionId, 30, application: Seat(["12A"], null))));
    }

    [Fact]
    public async Task V12_FAM07_an_airport_lounge_uses_the_verified_industry_code_and_files_passenger_rates_with_tax_and_fee()
    {
        var airlineId = _database.NextAirlineId();
        var airlineSupplierId = await _proof.SupplierAsync(M1Commands.LocalSupplier(airlineId, "Dot Air"));
        var partnerSupplierId = await _proof.SupplierAsync(M1Commands.ExternalSupplier(airlineId));

        var own = await _proof.ProveAsync(
            M1Commands.LoungeDefinition(airlineId, airlineSupplierId, "LNG_IKA_DOTAIR"),
            definitionId => Provision(
                definitionId,
                10,
                passenger: Passengers(PassengerTypeCode.ADT, PassengerTypeCode.CHD),
                sales: new ProvisionSalesCriteriaInput(PointOfSaleIds: [AgencyOffice]),
                travel: new ProvisionTravelCriteriaInput(
                    OriginAirportIds: [Ika],
                    RoutePairs: [Pair(Ika, Ist)],
                    SeasonalPeriods: [Period(Date(2026, 12, 1), Date(2027, 11, 30))])),
            draft => draft with { Travel = draft.Travel! with { RoutePairs = [Pair(Ika, Ist), Pair(Ika, Mhd)] } },
            definition =>
            {
                Assert.Equal(("Industry", "0BX", "F", "LG", "PerPassenger"), (definition.SubCodeSource.Name, definition.ServiceSubCode, definition.ServiceTypeCode, definition.GroupCode, definition.PricingUnit!.Name));
                Assert.Equal(("EmdStandalone", "E", "0BX"), (definition.DocumentType.Name, definition.DocumentRfic, definition.DocumentRfisc));
                Assert.Equal((airlineSupplierId, "Dot Air"), (definition.SupplierId, definition.SupplierName));
            },
            draft =>
            {
                Assert.Equal(new[] { "ADT", "CHD" }, Names(draft.Passenger.PassengerTypes));
                Assert.Equal(new[] { Ika }, Values(draft.Travel.OriginAirports));
                Assert.Equal((Ika, Ist), draft.Travel.RoutePairs.Select(pair => (pair.OriginAirportId, pair.DestinationAirportId)).Single());
            },
            edited => Assert.Equal(
                new[] { (Ika, Ist), (Ika, Mhd) },
                edited.Travel.RoutePairs.Select(pair => (pair.OriginAirportId, pair.DestinationAirportId))),
            new FamilyPricingProof(
                provisionId => Pricing(
                    provisionId,
                    Eur,
                    Base(25m, PassengerTypeCode.ADT, name: "Lounge access"),
                    Tax("VAT", 2.5m, PassengerTypeCode.ADT, countryId: 1, name: "Value added tax"),
                    Fee("SVC", 1m, PassengerTypeCode.ADT),
                    Base(15m, PassengerTypeCode.CHD, name: "Lounge access"),
                    Tax("VAT", 1.5m, PassengerTypeCode.CHD, countryId: 1, name: "Value added tax")),
                provisionId => Pricing(
                    provisionId,
                    Eur,
                    Base(27m, PassengerTypeCode.ADT, name: "Lounge access"),
                    Tax("VAT", 2.7m, PassengerTypeCode.ADT, countryId: 1, name: "Value added tax"),
                    Fee("SVC", 1m, PassengerTypeCode.ADT),
                    Base(15m, PassengerTypeCode.CHD, name: "Lounge access"),
                    Tax("VAT", 1.5m, PassengerTypeCode.CHD, countryId: 1, name: "Value added tax")),
                pricing =>
                {
                    Assert.Equal(new (string?, int?, int?, decimal, decimal)[] { ("ADT", null, null, 25m, 28.5m), ("CHD", null, null, 15m, 16.5m) }, Rates(pricing));
                    Assert.Equal(
                        new[] { ("Ancillary", (string?)null, 25m), ("Tax", "VAT", 2.5m), ("Fee", "SVC", 1m), ("Ancillary", null, 15m), ("Tax", "VAT", 1.5m) },
                        pricing.PriceLines.Select(line => (line.Category.Name, line.Code, line.Amount)));
                },
                pricing =>
                {
                    Assert.Equal(new (string?, int?, int?, decimal, decimal)[] { ("ADT", null, null, 27m, 30.7m), ("CHD", null, null, 15m, 16.5m) }, Rates(pricing));
                    Assert.Equal((2.7m, 1m, 1), (pricing.Rates[0].TaxAmount, pricing.Rates[0].FeeAmount, pricing.PriceLines.Where(line => line.Category.Name == "Tax").Select(line => line.CountryId).Distinct().Single()));
                }));

        var partner = await _proof.DefinitionAsync(M1Commands.LoungeDefinition(airlineId, partnerSupplierId, "LNG_IKA_PARTNER"));
        var partnerRule = await _proof.RuleAsync(
            Provision(
                partner.Id,
                10,
                passenger: Passengers(PassengerTypeCode.ADT),
                travel: new ProvisionTravelCriteriaInput(OriginAirportIds: [Ika])),
            provisionId => Pricing(provisionId, Eur, Base(45m, PassengerTypeCode.ADT, name: "Partner lounge")));

        Assert.NotEqual(own.ServiceDefinitionId, partner.Id);
        Assert.Equal((partnerSupplierId, "Partner Lounge", "0BX", "Active"), (partner.SupplierId, partner.SupplierName, partner.ServiceSubCode, partner.Status.Name));
        Assert.Equal(new (string?, int?, int?, decimal, decimal)[] { ("ADT", null, null, 45m, 45m) }, Rates(partnerRule.Pricing!));
        Assert.Equal(new[] { Ika }, Values(partnerRule.Provision.Travel.OriginAirports));

        await BusinessAssert.ThrowsAsync(16207, 422, () => _proof.DefinitionAsync(
            M1Commands.LoungeDefinition(airlineId, airlineSupplierId, "LNG_MHD") with
            {
                ServiceSubCode = "0ZZ",
                Document = new ServiceDefinitionDocumentInput(AncillaryDocumentType.None, null, null)
            },
            activate: false));

        var local = await _proof.DefinitionAsync(CarrierDefinition(airlineId, airlineSupplierId, "LNG_MHD", "LMH", "F", "LG", "Mashhad lounge"));

        Assert.Equal(("CarrierDefined", "LMH", "LG", "Active"), (local.SubCodeSource.Name, local.ServiceSubCode, local.GroupCode, local.Status.Name));
    }

    [Fact]
    public async Task V12_FAM08_priority_boarding_is_filed_per_passenger_with_day_and_time_windows_and_every_outcome()
    {
        var airlineId = _database.NextAirlineId();
        var supplierId = await _proof.SupplierAsync(M1Commands.LocalSupplier(airlineId));

        var paid = await _proof.ProveAsync(
            CarrierDefinition(airlineId, supplierId, "PRIORITY_BOARDING", "PRB", "F", "PB", "Priority boarding"),
            definitionId => Provision(
                definitionId,
                30,
                sales: new ProvisionSalesCriteriaInput(PointOfSaleIds: [AgencyOffice]),
                travel: new ProvisionTravelCriteriaInput(
                    DayTimeRestrictions: [DayTime(DayOfWeek.Thursday, 5, 11), DayTime(DayOfWeek.Friday, 5, 11)])),
            draft => draft with
            {
                Travel = new ProvisionTravelCriteriaInput(
                    DayTimeRestrictions:
                    [
                        DayTime(DayOfWeek.Thursday, 5, 11),
                        DayTime(DayOfWeek.Friday, 5, 11),
                        DayTime(DayOfWeek.Friday, 8, 9, DayTimeRestrictionEffect.Deny),
                        DayTime(DayOfWeek.Saturday, null, null)
                    ])
            },
            definition => Assert.Equal(("PRB", "PB", "CarrierDefined", "PerPassenger"), (definition.ServiceSubCode, definition.GroupCode, definition.SubCodeSource.Name, definition.PricingUnit!.Name)),
            draft =>
            {
                Assert.Equal(new (string, int?, int?, string)[] { ("Thursday", 5, 11, "Allow"), ("Friday", 5, 11, "Allow") }, DayTimes(draft));
                Assert.Equal(new[] { AgencyOffice }, Values(draft.Sales.PointsOfSale));
            },
            edited => Assert.Equal(
                new (string, int?, int?, string)[] { ("Thursday", 5, 11, "Allow"), ("Friday", 5, 11, "Allow"), ("Friday", 8, 9, "Deny"), ("Saturday", null, null, "Allow") },
                DayTimes(edited)),
            new FamilyPricingProof(
                provisionId => Pricing(provisionId, Eur, Base(7m, PassengerTypeCode.ADT), Base(7m, PassengerTypeCode.CHD)),
                provisionId => Pricing(provisionId, Eur, Base(9m, PassengerTypeCode.ADT), Base(7m, PassengerTypeCode.CHD)),
                pricing => Assert.Equal(new (string?, int?, int?, decimal, decimal)[] { ("ADT", null, null, 7m, 7m), ("CHD", null, null, 7m, 7m) }, Rates(pricing)),
                pricing => Assert.Equal(new (string?, int?, int?, decimal, decimal)[] { ("ADT", null, null, 9m, 9m), ("CHD", null, null, 7m, 7m) }, Rates(pricing))));

        var free = await _proof.RuleAsync(
            Provision(paid.ServiceDefinitionId, 10, CommercialDisposition.Free, fare: new ProvisionFareCriteriaInput(FareFamilyIds: [6, 7], CabinClassIds: [1])));
        var notAvailable = await _proof.RuleAsync(
            Provision(paid.ServiceDefinitionId, 20, CommercialDisposition.NotAvailable, passenger: Passengers(PassengerTypeCode.INF)));
        var standard = await _proof.RuleAsync(
            Provision(paid.ServiceDefinitionId, 100),
            provisionId => Pricing(provisionId, Eur, Base(9m, name: "Priority boarding")));

        Assert.Equal(new long[] { 6, 7 }, Values(free.Provision.Fare.FareFamilies));
        Assert.Equal(new[] { "INF" }, Names(notAvailable.Provision.Passenger.PassengerTypes));
        Assert.Equal(new (string?, int?, int?, decimal, decimal)[] { (null, null, null, 9m, 9m) }, Rates(standard.Pricing!));
        Assert.Equal(
            new[] { (10, "Free", 0), (20, "NotAvailable", 0), (30, "Paid", 4), (100, "Paid", 0) },
            (await _proof.ListedProvisionsAsync(paid.ServiceDefinitionId)).Select(row => (row.Sequence, row.Disposition.Name, row.DayTimeRestrictionCount)));
        await RefusedAsync(16302, 422, scope => scope.DefineProvision.DefineAsync(
            Provision(
                paid.ServiceDefinitionId,
                40,
                travel: new ProvisionTravelCriteriaInput(
                    DayTimeRestrictions: [new ProvisionDayTimeRestrictionInput(DayOfWeek.Monday, new TimeOnly(12, 0), new TimeOnly(8, 0), DayTimeRestrictionEffect.Allow)]))));
    }

    [Fact]
    public async Task V12_FAM09_fast_track_is_filed_by_point_of_sale_seasons_and_blackout_dates()
    {
        var airlineId = _database.NextAirlineId();
        var supplierId = await _proof.SupplierAsync(M1Commands.LocalSupplier(airlineId, "IKA Airport Services"));
        ProvisionDatePeriodInput[] seasons = [Period(Date(2026, 12, 20), Date(2027, 1, 5)), Period(Date(2027, 3, 15), Date(2027, 4, 5))];
        ProvisionDatePeriodInput[] blackouts = [Period(Date(2026, 12, 31), Date(2027, 1, 1)), Period(Date(2027, 3, 21), Date(2027, 3, 21))];

        var fastTrack = await _proof.ProveAsync(
            CarrierDefinition(airlineId, supplierId, "FASTTRACK_IKA", "FTK", "F", "FT", "Fast track security"),
            definitionId => Provision(
                definitionId,
                10,
                sales: new ProvisionSalesCriteriaInput(
                    PointOfSaleIds: [AirlineOffice, AgencyOffice],
                    CustomerTypes: [CustomerType.Individual, CustomerType.TravelAgency]),
                travel: new ProvisionTravelCriteriaInput(
                    OriginAirportIds: [Ika],
                    RoutePairs: [Pair(Ika, Ist), Pair(Ika, Mhd, RoutePairDirection.BothDirections)],
                    SeasonalPeriods: seasons,
                    BlackoutPeriods: blackouts,
                    DayTimeRestrictions: [DayTime(DayOfWeek.Thursday, 5, 11), DayTime(DayOfWeek.Friday, 5, 11)])),
            draft => draft with
            {
                Sales = new ProvisionSalesCriteriaInput(PointOfSaleIds: [AgencyOffice]),
                Travel = draft.Travel! with
                {
                    RoutePairs = [Pair(Ika, Ist)],
                    BlackoutPeriods = [.. blackouts, Period(Date(2027, 4, 1), Date(2027, 4, 2))]
                }
            },
            definition => Assert.Equal(("FTK", "FT", "IKA Airport Services", "PerPassenger"), (definition.ServiceSubCode, definition.GroupCode, definition.SupplierName, definition.PricingUnit!.Name)),
            draft =>
            {
                Assert.Equal(new[] { Ika }, Values(draft.Travel.OriginAirports));
                Assert.Equal(
                    new[] { (Ika, Ist, "Directional"), (Ika, Mhd, "BothDirections") },
                    draft.Travel.RoutePairs.Select(pair => (pair.OriginAirportId, pair.DestinationAirportId, pair.Direction.Name)));
                Assert.Equal(new[] { (Date(2026, 12, 20), Date(2027, 1, 5)), (Date(2027, 3, 15), Date(2027, 4, 5)) }, Periods(draft.Travel.SeasonalPeriods));
                Assert.Equal(new[] { (Date(2026, 12, 31), Date(2027, 1, 1)), (Date(2027, 3, 21), Date(2027, 3, 21)) }, Periods(draft.Travel.BlackoutPeriods));
                Assert.Equal(new (string, int?, int?, string)[] { ("Thursday", 5, 11, "Allow"), ("Friday", 5, 11, "Allow") }, DayTimes(draft));
                Assert.Equal(new[] { AirlineOffice, AgencyOffice }, Values(draft.Sales.PointsOfSale));
                Assert.Equal(new[] { "Individual", "TravelAgency" }, Names(draft.Sales.CustomerTypes));
            },
            edited =>
            {
                Assert.Equal((Ika, Ist), edited.Travel.RoutePairs.Select(pair => (pair.OriginAirportId, pair.DestinationAirportId)).Single());
                Assert.Equal(new[] { AgencyOffice }, Values(edited.Sales.PointsOfSale));
                Assert.Empty(edited.Sales.CustomerTypes);
                Assert.Equal(2, edited.Travel.SeasonalPeriods.Count);
                Assert.Equal(
                    new[] { (Date(2026, 12, 31), Date(2027, 1, 1)), (Date(2027, 3, 21), Date(2027, 3, 21)), (Date(2027, 4, 1), Date(2027, 4, 2)) },
                    Periods(edited.Travel.BlackoutPeriods));
            },
            Flat(PricingUnit.PerPassenger, 12m, 13m, "Fast track"));

        Assert.Equal(
            (2, 3, 2),
            (await _proof.ListedProvisionsAsync(fastTrack.ServiceDefinitionId))
            .Select(row => (row.SeasonalPeriodCount, row.BlackoutPeriodCount, row.DayTimeRestrictionCount))
            .Single());
        await RefusedAsync(16302, 422, scope => scope.DefineProvision.DefineAsync(
            Provision(
                fastTrack.ServiceDefinitionId,
                20,
                travel: new ProvisionTravelCriteriaInput(BlackoutPeriods: [Period(Date(2027, 1, 2), Date(2027, 1, 1))]))));
    }

    [Fact]
    public async Task V12_FAM10_wifi_is_filed_per_item_by_aircraft_flight_cabin_and_fare_family_without_an_activation_workflow()
    {
        var airlineId = _database.NextAirlineId();
        var supplierId = await _proof.SupplierAsync(M1Commands.LocalSupplier(airlineId));

        var wifi = await _proof.ProveAsync(
            CarrierDefinition(airlineId, supplierId, "WIFI_FULL_FLIGHT", "WFF", "F", "WF", "Full flight Wi-Fi", pricingUnit: PricingUnit.PerItem),
            definitionId => Provision(
                definitionId,
                10,
                travel: new ProvisionTravelCriteriaInput(
                    MarketingAirlineIds: [1],
                    OperatingAirlineIds: [1],
                    FlightNumbers: ["w5112", "W5114"],
                    AircraftIds: [2]),
                fare: new ProvisionFareCriteriaInput(FareFamilyIds: [5], CabinClassIds: [2])),
            draft => draft with { Travel = draft.Travel! with { AircraftIds = [2, 3] } },
            definition => Assert.Equal(("WFF", "WF", "PerItem", "NoBookingProcessRequired"), (definition.ServiceSubCode, definition.GroupCode, definition.PricingUnit!.Name, definition.BookingMethod.Name)),
            draft =>
            {
                Assert.Equal(new[] { 2 }, Values(draft.Travel.Aircraft));
                Assert.Equal(new[] { "W5112", "W5114" }, Values(draft.Travel.FlightNumbers));
                Assert.Equal(new[] { 1 }, Values(draft.Travel.MarketingAirlines));
                Assert.Equal(new[] { 1 }, Values(draft.Travel.OperatingAirlines));
                Assert.Equal(new long[] { 5 }, Values(draft.Fare.FareFamilies));
                Assert.Equal(new[] { 2 }, Values(draft.Fare.CabinClasses));
                Assert.Equal(("Ancillary", false), (draft.FulfillmentProviderKey, draft.MustCheckAvailability));
            },
            edited =>
            {
                Assert.Equal(new[] { 2, 3 }, Values(edited.Travel.Aircraft));
                Assert.Equal(new[] { "W5112", "W5114" }, Values(edited.Travel.FlightNumbers));
            },
            Flat(PricingUnit.PerItem, 10m, 11m, "Full flight Wi-Fi"));

        var included = await _proof.RuleAsync(
            Provision(
                wifi.ServiceDefinitionId,
                20,
                CommercialDisposition.Free,
                travel: new ProvisionTravelCriteriaInput(AircraftIds: [2]),
                fare: new ProvisionFareCriteriaInput(FareFamilyIds: [6], CabinClassIds: [1])));

        Assert.Equal(("Free", "Active"), (included.Provision.Disposition.Name, included.Provision.Status.Name));
        Assert.Equal(new[] { 1 }, Values(included.Provision.Fare.CabinClasses));
        Assert.Null(included.Pricing);
        Assert.DoesNotContain(
            typeof(BackofficeProvisionDto).GetProperties()
                .Concat(typeof(BackofficeServiceDefinitionDto).GetProperties())
                .Concat(typeof(BackofficePricingDto).GetProperties()),
            property => new[] { "Voucher", "Credential", "Token", "Device", "Session", "Activation" }
                .Any(term => property.Name.Contains(term, StringComparison.Ordinal)));
    }

    [Fact]
    public async Task V12_FAM11_a_pet_service_carries_booking_metadata_a_route_and_flight_scope_and_a_fixed_price()
    {
        var airlineId = _database.NextAirlineId();
        var supplierId = await _proof.SupplierAsync(M1Commands.LocalSupplier(airlineId));

        var cabin = await _proof.ProveAsync(
            CarrierDefinition(airlineId, supplierId, "PET_IN_CABIN", "PTC", "F", "PT", "Pet in cabin", Ssr("PETC"), pricingUnit: PricingUnit.PerItem),
            definitionId => Provision(
                definitionId,
                10,
                travel: new ProvisionTravelCriteriaInput(
                    RoutePairs: [Pair(Thr, Mhd, RoutePairDirection.BothDirections)],
                    FlightIds: [81234],
                    AircraftIds: [1, 2])),
            draft => draft with
            {
                Travel = draft.Travel! with { FlightIds = [81234, 81240] },
                Outcome = draft.Outcome with { BookingRequired = true }
            },
            definition => Assert.Equal(("PTC", "PT", "Ssr", "PETC", "PerItem"), (definition.ServiceSubCode, definition.GroupCode, definition.BookingMethod.Name, definition.BookingSsrCode, definition.PricingUnit!.Name)),
            draft =>
            {
                Assert.Equal(("Sector", false), (draft.CoverageScope.Name, draft.BookingRequired));
                Assert.Equal((Thr, Mhd, "BothDirections"), draft.Travel.RoutePairs.Select(pair => (pair.OriginAirportId, pair.DestinationAirportId, pair.Direction.Name)).Single());
                Assert.Equal(new long[] { 81234 }, Values(draft.Travel.Flights));
                Assert.Equal(new[] { 1, 2 }, Values(draft.Travel.Aircraft));
            },
            edited =>
            {
                Assert.Equal(("Sector", true), (edited.CoverageScope.Name, edited.BookingRequired));
                Assert.Equal(new long[] { 81234, 81240 }, Values(edited.Travel.Flights));
            },
            Flat(PricingUnit.PerItem, 50m, 55m, "Pet in cabin"));

        var journey = await _proof.RuleAsync(
            Provision(cabin.ServiceDefinitionId, 20, coverageScope: ServiceCoverageScope.Journey, bookingRequired: true),
            provisionId => Pricing(provisionId, Eur, Base(90m, name: "Pet in cabin")));
        var hold = await _proof.DefinitionAsync(
            CarrierDefinition(airlineId, supplierId, "PET_IN_HOLD", "PTH", "F", "PT", "Pet in hold", Ssr("AVIH"), pricingUnit: PricingUnit.PerItem));
        var holdRule = await _proof.RuleAsync(
            Provision(hold.Id, 10, coverageScope: ServiceCoverageScope.Journey, bookingRequired: true),
            provisionId => Pricing(provisionId, Eur, Base(120m, name: "Pet in hold")));

        Assert.Equal(("Journey", "Active", 90m), (journey.Provision.CoverageScope.Name, journey.Provision.Status.Name, journey.Pricing!.Rates.Single().TotalAmount));
        Assert.Equal(("AVIH", "PTH", "PerItem"), (hold.BookingSsrCode, hold.ServiceSubCode, hold.PricingUnit!.Name));
        Assert.Equal((120m, "Active"), (holdRule.Pricing!.Rates.Single().TotalAmount, holdRule.Pricing.Status.Name));
    }

    [Fact]
    public async Task V12_FAM12_meet_and_assist_is_filed_by_point_of_sale_airport_and_time_per_passenger_or_per_item()
    {
        var airlineId = _database.NextAirlineId();
        var firstSupplierId = await _proof.SupplierAsync(M1Commands.LocalSupplier(airlineId, "IKA CIP Services"));
        var secondSupplierId = await _proof.SupplierAsync(M1Commands.ExternalSupplier(airlineId, "MeetAssistPartner"));

        var first = await _proof.ProveAsync(
            CarrierDefinition(airlineId, firstSupplierId, "MAAS_IKA_CIP", "MAS", "F", "MA", "Meet and assist CIP", Ssr("MAAS")),
            definitionId => Provision(
                definitionId,
                10,
                passenger: Passengers(PassengerTypeCode.ADT),
                sales: new ProvisionSalesCriteriaInput(PointOfSaleIds: [AgencyOffice], CustomerIds: [9001]),
                travel: new ProvisionTravelCriteriaInput(
                    OriginAirportIds: [Ika],
                    DestinationAirportIds: [Ist],
                    RoutePairs: [Pair(Ika, Ist)],
                    SeasonalPeriods: [Period(Date(2026, 12, 1), Date(2027, 3, 31))],
                    DayTimeRestrictions: [DayTime(DayOfWeek.Monday, 6, 22), DayTime(DayOfWeek.Tuesday, 6, 22)]),
                bookingRequired: true),
            draft => draft with
            {
                Passenger = Passengers(PassengerTypeCode.ADT, PassengerTypeCode.CHD),
                Sales = new ProvisionSalesCriteriaInput(PointOfSaleIds: [AgencyOffice], CustomerIds: [9001, 9002])
            },
            definition => Assert.Equal(("MAS", "MA", "MAAS", firstSupplierId, "PerPassenger"), (definition.ServiceSubCode, definition.GroupCode, definition.BookingSsrCode, definition.SupplierId, definition.PricingUnit!.Name)),
            draft =>
            {
                Assert.Equal(new[] { "ADT" }, Names(draft.Passenger.PassengerTypes));
                Assert.Equal(new[] { AgencyOffice }, Values(draft.Sales.PointsOfSale));
                Assert.Equal(new long[] { 9001 }, Values(draft.Sales.Customers));
                Assert.Equal(new[] { Ika }, Values(draft.Travel.OriginAirports));
                Assert.Equal(new[] { Ist }, Values(draft.Travel.DestinationAirports));
                Assert.Equal(new[] { (Date(2026, 12, 1), Date(2027, 3, 31)) }, Periods(draft.Travel.SeasonalPeriods));
                Assert.Equal(new (string, int?, int?, string)[] { ("Monday", 6, 22, "Allow"), ("Tuesday", 6, 22, "Allow") }, DayTimes(draft));
            },
            edited =>
            {
                Assert.Equal(new[] { "ADT", "CHD" }, Names(edited.Passenger.PassengerTypes));
                Assert.Equal(new long[] { 9001, 9002 }, Values(edited.Sales.Customers));
                Assert.Equal(2, edited.Travel.DayTimeRestrictions.Count);
            },
            new FamilyPricingProof(
                provisionId => Pricing(provisionId, Eur, Base(60m, PassengerTypeCode.ADT)),
                provisionId => Pricing(provisionId, Eur, Base(60m, PassengerTypeCode.ADT), Base(30m, PassengerTypeCode.CHD)),
                pricing => Assert.Equal(new (string?, int?, int?, decimal, decimal)[] { ("ADT", null, null, 60m, 60m) }, Rates(pricing)),
                pricing => Assert.Equal(new (string?, int?, int?, decimal, decimal)[] { ("ADT", null, null, 60m, 60m), ("CHD", null, null, 30m, 30m) }, Rates(pricing))));

        var second = await _proof.DefinitionAsync(
            CarrierDefinition(airlineId, secondSupplierId, "MAAS_IKA_PARTNER", "MAS", "F", "MA", "Meet and assist partner", Ssr("MAAS"), pricingUnit: PricingUnit.PerItem));
        var secondRule = await _proof.RuleAsync(
            Provision(
                second.Id,
                10,
                travel: new ProvisionTravelCriteriaInput(
                    OriginAirportIds: [Ika],
                    DestinationAirportIds: [Ist],
                    DayTimeRestrictions: [DayTime(DayOfWeek.Friday, 0, 6, DayTimeRestrictionEffect.Deny)])),
            provisionId => Pricing(provisionId, Eur, Base(75m, name: "Meet and assist")));

        Assert.NotEqual(first.ServiceDefinitionId, second.Id);
        Assert.Equal((secondSupplierId, "MAS", "PerItem", "Active"), (second.SupplierId, second.ServiceSubCode, second.PricingUnit!.Name, second.Status.Name));
        Assert.Equal(new (string, int?, int?, string)[] { ("Friday", 0, 6, "Deny") }, DayTimes(secondRule.Provision));
        Assert.Equal(new (string?, int?, int?, decimal, decimal)[] { (null, null, null, 75m, 75m) }, Rates(secondRule.Pricing!));
    }

    [Fact]
    public async Task V12_FAM13_unaccompanied_minor_handling_is_filed_for_child_passengers_on_several_explicit_dates()
    {
        var airlineId = _database.NextAirlineId();
        var supplierId = await _proof.SupplierAsync(M1Commands.LocalSupplier(airlineId));
        DateOnly[] dates = [Date(2026, 12, 18), Date(2026, 12, 19), Date(2026, 12, 26), Date(2027, 1, 2), Date(2027, 1, 3)];

        var umnr = await _proof.ProveAsync(
            CarrierDefinition(airlineId, supplierId, "UMNR_HANDLING", "UMN", "F", "UM", "Unaccompanied minor handling", Ssr("UMNR")),
            definitionId => Provision(
                definitionId,
                10,
                coverageScope: ServiceCoverageScope.Journey,
                passenger: Passengers(PassengerTypeCode.UNN),
                sales: new ProvisionSalesCriteriaInput(CustomerTypes: [CustomerType.Individual, CustomerType.TravelAgency]),
                travel: new ProvisionTravelCriteriaInput(
                    RoutePairs: [Pair(Thr, Mhd, RoutePairDirection.BothDirections)],
                    TravelDates: dates,
                    OperatingAirlineIds: [1]),
                bookingRequired: true),
            draft => draft with
            {
                Passenger = Passengers(PassengerTypeCode.UNN, PassengerTypeCode.CHD),
                Travel = draft.Travel! with
                {
                    RoutePairs = [Pair(Thr, Mhd, RoutePairDirection.BothDirections), Pair(Thr, Ist)],
                    TravelDates = [.. dates, Date(2027, 3, 20), Date(2027, 3, 21)]
                }
            },
            definition => Assert.Equal(("UMN", "UM", "Ssr", "UMNR", "PerPassenger"), (definition.ServiceSubCode, definition.GroupCode, definition.BookingMethod.Name, definition.BookingSsrCode, definition.PricingUnit!.Name)),
            draft =>
            {
                Assert.Equal(new[] { "UNN" }, Names(draft.Passenger.PassengerTypes));
                Assert.Equal(new[] { "Individual", "TravelAgency" }, Names(draft.Sales.CustomerTypes));
                Assert.Equal((Thr, Mhd, "BothDirections"), draft.Travel.RoutePairs.Select(pair => (pair.OriginAirportId, pair.DestinationAirportId, pair.Direction.Name)).Single());
                Assert.Equal(new[] { 1 }, Values(draft.Travel.OperatingAirlines));
                Assert.Equal(dates, Values(draft.Travel.TravelDates));
                Assert.Equal(("Journey", true), (draft.CoverageScope.Name, draft.BookingRequired));
            },
            edited =>
            {
                Assert.Equal(new[] { "UNN", "CHD" }, Names(edited.Passenger.PassengerTypes));
                Assert.Equal(2, edited.Travel.RoutePairs.Count);
                Assert.Equal(dates.Concat([Date(2027, 3, 20), Date(2027, 3, 21)]), Values(edited.Travel.TravelDates));
                Assert.Equal(7, edited.Travel.TravelDates.Select(row => row.Id).Distinct().Count());
            },
            new FamilyPricingProof(
                provisionId => Pricing(provisionId, Eur, Base(35m, PassengerTypeCode.UNN, 5, 12)),
                provisionId => Pricing(
                    provisionId,
                    Eur,
                    Base(35m, PassengerTypeCode.UNN, 5, 12),
                    Base(45m, PassengerTypeCode.UNN, 12, 16),
                    Base(35m, PassengerTypeCode.CHD, 5, 12)),
                pricing => Assert.Equal(new (string?, int?, int?, decimal, decimal)[] { ("UNN", 5, 12, 35m, 35m) }, Rates(pricing)),
                pricing => Assert.Equal(
                    new (string?, int?, int?, decimal, decimal)[] { ("CHD", 5, 12, 35m, 35m), ("UNN", 5, 12, 35m, 35m), ("UNN", 12, 16, 45m, 45m) },
                    Rates(pricing))));

        Assert.Equal(7, (await _proof.ListedProvisionsAsync(umnr.ServiceDefinitionId)).Single().TravelDateCount);
        Assert.DoesNotContain(
            typeof(BackofficeProvisionDto).GetProperties().Concat(typeof(BackofficeProvisionPassengerCriteriaDto).GetProperties()),
            property => property.Name.Contains("Age", StringComparison.Ordinal) || property.Name.Contains("Birth", StringComparison.Ordinal));
        Assert.Contains(typeof(BackofficePricingLineDto).GetProperties(), property => property.Name == nameof(BackofficePricingLineDto.AgeFromInclusive));
        Assert.Contains(typeof(BackofficePricingLineDto).GetProperties(), property => property.Name == nameof(BackofficePricingLineDto.AgeToExclusive));
    }
}
