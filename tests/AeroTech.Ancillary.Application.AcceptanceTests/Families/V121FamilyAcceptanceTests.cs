using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;
using AeroTech.Messages.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.P1Commands;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.V121Commands;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.V12Commands;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Families;

[Collection(DatabaseCollection.Name)]
public class V121FamilyAcceptanceTests
{
    private const long AirlineOffice = 9200000000000001;
    private const long AgencyOffice = 1551571720488353792;
    private const int Turkey = 90;
    private const int Iran = 98;
    private const int Tehran = 7;

    private readonly TestDatabase _database;
    private readonly FixedClock _clock = new();
    private readonly FamilyProof _proof;

    public V121FamilyAcceptanceTests(TestDatabase database)
    {
        _database = database;
        _proof = new FamilyProof(database, _clock);
    }

    private Task RefusedAsync(int code, int httpStatus, Func<AncillaryScope, Task> request)
        => BusinessAssert.ThrowsAsync(code, httpStatus, async () =>
        {
            await using var scope = new AncillaryScope(_database, _clock);

            await request(scope);
        });

    private async Task<(int AirlineId, long SupplierId)> SupplierAsync()
    {
        var airlineId = _database.NextAirlineId();

        return (airlineId, await _proof.SupplierAsync(M1Commands.LocalSupplier(airlineId)));
    }

    private static Action<BackofficeServiceDefinitionDto> Definition(PricingUnit pricingUnit, ServiceDateBasis basis, string bookingMethod = "NoBookingProcessRequired", string? ssrCode = null)
        => definition => Assert.Equal(
            (pricingUnit.ToString(), basis.ToString(), bookingMethod, ssrCode),
            (definition.PricingUnit!.Name, definition.ServiceDateBasis!.Name, definition.BookingMethod.Name, definition.BookingSsrCode));

    private static Action<BackofficeProvisionDto> Rules(string expected, string disposition = "Paid")
        => provision => Assert.Equal((expected, disposition), (RuleText.Of(provision), provision.Disposition.Name));

    private static FamilyPricingProof Priced(
        string draft,
        string edited,
        Func<long, TestDefinePricingCommand> draftPricing,
        Func<long, TestDefinePricingCommand> editedPricing)
        => new(draftPricing, editedPricing, pricing => Assert.Equal(draft, RuleText.Of(pricing)), pricing => Assert.Equal(edited, RuleText.Of(pricing)));

    private static FamilyPricingProof Flat(PricingUnit pricingUnit, decimal draft, decimal edited)
        => Priced(
            FormattableString.Invariant($"{pricingUnit} EUR *[-]={draft:0.##}+0+0={draft:0.##}"),
            FormattableString.Invariant($"{pricingUnit} EUR *[-]={edited:0.##}+0+0={edited:0.##}"),
            provisionId => Pricing(provisionId, Eur, Base(draft)),
            provisionId => Pricing(provisionId, Eur, Base(edited)));

    private async Task<(int Sequence, string Disposition, string Status, string Rules)[]> FiledAsync(long serviceDefinitionId)
    {
        var rows = await _proof.ListedProvisionsAsync(serviceDefinitionId);
        var filed = new List<(int, string, string, string)>();

        await using var reader = new AncillaryScope(_database, _clock);

        foreach (var row in rows.Where(row => row.Status.Name == "Active").OrderBy(row => row.Sequence))
            filed.Add((row.Sequence, row.Disposition.Name, row.Status.Name, RuleText.Of(await reader.GetProvisionById.ExecuteAsync(long.Parse(row.Id)))));

        return filed.ToArray();
    }

    [Fact]
    public async Task V121_F01_extra_baggage_is_per_piece_with_route_flight_date_and_prepaid_descriptors_and_every_outcome()
    {
        var (airlineId, supplierId) = await SupplierAsync();
        var winter = Period(Day(2026, 12, 20), Day(2026, 12, 31));
        var spring = Period(Day(2027, 3, 15), Day(2027, 4, 5));

        TestDefineProvisionCommand Bag(long definitionId, int sequence, CommercialDisposition disposition = CommercialDisposition.Paid)
            => Provision(definitionId, sequence, disposition, ServiceCoverageScope.Journey, AncillaryQuantityUnit.Piece, maxQuantity: 3, applicationType: ProvisionApplicationType.Baggage) with
            {
                BaggageApplication = Baggage(23m, 1, 1, BaggageTravelApplication.AllSectors)
            };

        var piece = await _proof.ProveAsync(
            FirstExcessBagDefinition(airlineId, supplierId),
            definitionId => Bag(definitionId, 10) with
            {
                PassengerEligibility = Passengers(PassengerTypeCode.ADT),
                SalesRestrictions = new(AllowedPointOfSaleIds: [AgencyOffice]),
                Geography = new(AllowedRoutePairs: [Pair(Thr, Ist)]),
                FlightApplication = new(AllowedFlightIds: [81234]),
                FareApplication = new(AllowedFareFamilyIds: [5]),
                TravelDate = Dates([winter])
            },
            draft => draft with { TravelDate = Dates([winter, spring]), BaggageApplication = Baggage(25m, 1, 1, BaggageTravelApplication.AllSectors) },
            definition =>
            {
                Definition(PricingUnit.PerPiece, ServiceDateBasis.FlightDeparture, "Ssr", "XBAG")(definition);
                Assert.Equal(("Industry", "0CC", "EmdAssociated", "C", "0CC"), (definition.SubCodeSource.Name, definition.ServiceSubCode, definition.DocumentType.Name, definition.DocumentRfic, definition.DocumentRfisc));
            },
            Rules($"PTC=ADT; POS={AgencyOffice}; ROUTE=1>6; FLT=81234; FAMILY=5; PERMIT=2026-12-20..2026-12-31; BAG=1-1:23Kg:Prepaid"),
            Rules($"PTC=ADT; POS={AgencyOffice}; ROUTE=1>6; FLT=81234; FAMILY=5; PERMIT=2026-12-20..2026-12-31,2027-03-15..2027-04-05; BAG=1-1:25Kg:Prepaid"),
            Priced(
                "PerPiece EUR *[-]=30+0+0=30",
                "PerPiece EUR *[-]=32+3.2+0=35.2",
                provisionId => Pricing(provisionId, Eur, Base(30m, name: "Extra bag")),
                provisionId => Pricing(provisionId, Eur, Base(32m, name: "Extra bag"), Tax("VAT", 3.2m, countryId: Iran, name: "Value added tax"))));

        var free = await _proof.RuleAsync(Bag(piece.ServiceDefinitionId, 20, CommercialDisposition.Free) with { FareApplication = new(AllowedFareFamilyIds: [6]) });
        var notAvailable = await _proof.RuleAsync(Bag(piece.ServiceDefinitionId, 5, CommercialDisposition.NotAvailable) with { PassengerEligibility = Passengers(PassengerTypeCode.INF) });

        Assert.Null(free.Pricing);
        Assert.Null(notAvailable.Pricing);
        Assert.Equal(
            new[]
            {
                (5, "NotAvailable", "Active", "PTC=INF; BAG=1-1:23Kg:Prepaid"),
                (10, "Paid", "Active", $"PTC=ADT; POS={AgencyOffice}; ROUTE=1>6; FLT=81234; FAMILY=5; PERMIT=2026-12-20..2026-12-31,2027-03-15..2027-04-05; BAG=1-1:25Kg:Prepaid"),
                (20, "Free", "Active", "FAMILY=6; BAG=1-1:23Kg:Prepaid")
            },
            await FiledAsync(piece.ServiceDefinitionId));
        Assert.Equal(
            new[] { (5, 0), (10, 2), (20, 0) },
            (await _proof.ListedProvisionsAsync(piece.ServiceDefinitionId)).OrderBy(row => row.Sequence).Select(row => (row.Sequence, row.PermittedPeriodCount)));

        var kilogram = await _proof.DefinitionAsync(
            CarrierDefinition(airlineId, supplierId, "XBAG_KILO", "XKG", "C", "BG", "Extra baggage per kilogram", Ssr("XBAG"), pricingUnit: PricingUnit.PerKilogram));
        var perKilogram = await _proof.RuleAsync(
            Provision(kilogram.Id, 10, coverageScope: ServiceCoverageScope.Journey, quantityUnit: AncillaryQuantityUnit.Kilogram, minQuantity: 5, maxQuantity: 30, applicationType: ProvisionApplicationType.Baggage) with
            {
                Geography = new(AllowedRoutePairs: [Pair(Thr, Ist, RoutePairDirection.BothDirections)]),
                FlightApplication = new(AllowedFlightNumbers: ["W5112"]),
                TravelDate = Dates([winter]),
                BaggageApplication = Baggage(30m, chargeKind: BaggageChargeKind.Overweight, allowanceConcept: null)
            },
            provisionId => Pricing(provisionId, Eur, Base(4.5m, name: "Extra kilogram")));

        Assert.NotEqual(piece.ServiceDefinitionId, kilogram.Id);
        Assert.Equal(("Kilogram", 5, 30, "Active"), (perKilogram.Provision.QuantityUnit.Name, perKilogram.Provision.MinQuantity, perKilogram.Provision.MaxQuantity, perKilogram.Provision.Status.Name));
        Assert.Equal("ROUTE=1<>6; FLTNO=W5112; PERMIT=2026-12-20..2026-12-31; BAG=-:30Kg:Prepaid", RuleText.Of(perKilogram.Provision));
        Assert.Equal("PerKilogram EUR *[-]=4.5+0+0=4.5", RuleText.Of(perKilogram.Pricing));
        await RefusedAsync(16508, 409, scope => scope.DefinePricing.DefineAsync(Pricing(perKilogram.Provision.Id, Eur, Base(4.5m, PassengerTypeCode.ADT))));
    }

    [Fact]
    public async Task V121_F02_sports_baggage_has_an_aircraft_restriction_a_weight_descriptor_and_a_blackout()
    {
        var (airlineId, supplierId) = await SupplierAsync();

        var sports = await _proof.ProveAsync(
            CarrierDefinition(airlineId, supplierId, "SPORT_EQUIPMENT", "SPQ", "C", "BG", "Sports equipment", Ssr("SPEQ"), pricingUnit: PricingUnit.PerPiece, variant: "A06"),
            definitionId => Provision(definitionId, 10, coverageScope: ServiceCoverageScope.Journey, quantityUnit: AncillaryQuantityUnit.Piece, maxQuantity: 2, applicationType: ProvisionApplicationType.Baggage) with
            {
                FlightApplication = new(AllowedAircraftIds: [1, 2]),
                TravelDate = Dates(blackout: [Period(Day(2027, 3, 20), Day(2027, 4, 2))]),
                BaggageApplication = Baggage(32m, purchaseApplication: BaggagePurchaseApplication.PrepaidAndCheckIn, chargeKind: BaggageChargeKind.SpecialEquipment, allowanceConcept: null)
            },
            draft => draft with
            {
                FlightApplication = new(AllowedAircraftIds: [1, 2, 3]),
                TravelDate = Dates(blackout: [Period(Day(2027, 3, 20), Day(2027, 4, 2)), Period(Day(2027, 12, 24), Day(2027, 12, 26))]),
                BaggageApplication = Baggage(30m, purchaseApplication: BaggagePurchaseApplication.PrepaidAndCheckIn, chargeKind: BaggageChargeKind.SpecialEquipment, allowanceConcept: null)
            },
            Definition(PricingUnit.PerPiece, ServiceDateBasis.FlightDeparture, "Ssr", "SPEQ"),
            Rules("ACFT=1,2; BLACKOUT=2027-03-20..2027-04-02; BAG=-:32Kg:PrepaidAndCheckIn"),
            Rules("ACFT=1,2,3; BLACKOUT=2027-03-20..2027-04-02,2027-12-24..2027-12-26; BAG=-:30Kg:PrepaidAndCheckIn"),
            Flat(PricingUnit.PerPiece, 60m, 65m));

        Assert.Equal((0, 2), (await _proof.ListedProvisionsAsync(sports.ServiceDefinitionId)).Select(row => (row.PermittedPeriodCount, row.BlackoutPeriodCount)).Single());
        await RefusedAsync(16302, 422, scope => scope.DefineProvision.DefineAsync(
            Provision(sports.ServiceDefinitionId, 20, quantityUnit: AncillaryQuantityUnit.Piece, applicationType: ProvisionApplicationType.Baggage)));
        await RefusedAsync(16302, 422, scope => scope.DefineProvision.DefineAsync(
            Provision(sports.ServiceDefinitionId, 20, quantityUnit: AncillaryQuantityUnit.Piece, applicationType: ProvisionApplicationType.Baggage) with { BaggageApplication = Baggage(0m) }));
    }

    [Fact]
    public async Task V121_F03_wheelchair_assistance_uses_ssr_metadata_is_free_or_not_available_and_never_fabricates_an_industry_code()
    {
        var (airlineId, supplierId) = await SupplierAsync();

        var ramp = await _proof.ProveAsync(
            CarrierDefinition(airlineId, supplierId, "WHEELCHAIR_RAMP", "WCR", "F", "AS", "Wheelchair to aircraft door", Ssr("WCHR"), variant: "A15"),
            definitionId => Provision(definitionId, 100, CommercialDisposition.Free) with { Geography = new(AllowedOriginAirportIds: [Ika, Thr]) },
            draft => draft with { Geography = new(AllowedOriginAirportIds: [Ika, Thr, Mhd]), Outcome = draft.Outcome with { BookingRequired = true } },
            definition =>
            {
                Definition(PricingUnit.PerPassenger, ServiceDateBasis.FlightDeparture, "Ssr", "WCHR")(definition);
                Assert.Equal(("CarrierDefined", "WCR", "None"), (definition.SubCodeSource.Name, definition.ServiceSubCode, definition.DocumentType.Name));
            },
            draft =>
            {
                Rules("ORG=2,1", "Free")(draft);
                Assert.False(draft.BookingRequired);
            },
            edited =>
            {
                Rules("ORG=2,1,3", "Free")(edited);
                Assert.Equal((true, false), (edited.BookingRequired, edited.DocumentRequired));
            });
        var notAvailable = await _proof.RuleAsync(Provision(ramp.ServiceDefinitionId, 10, CommercialDisposition.NotAvailable) with { FlightApplication = new(AllowedAircraftIds: [4]) });

        Assert.Null(ramp.PricingId);
        Assert.Null(notAvailable.Pricing);
        Assert.Equal(
            new[] { (10, "NotAvailable", "Active", "ACFT=4"), (100, "Free", "Active", "ORG=2,1,3") },
            await FiledAsync(ramp.ServiceDefinitionId));
        await RefusedAsync(16505, 409, scope => scope.DefinePricing.DefineAsync(Pricing(ramp.ProvisionId, Eur, Base(20m))));
        await BusinessAssert.ThrowsAsync(16207, 422, () => _proof.DefinitionAsync(
            CarrierDefinition(airlineId, supplierId, "WHEELCHAIR_FAKE", "0WC", "F", "AS", "Wheelchair", Ssr("WCHR"), variant: "A15") with { SubCodeSource = ServiceSubCodeSource.Industry },
            activate: false));
    }

    [Fact]
    public async Task V121_F04_a_meal_is_filed_by_passenger_type_fare_cabin_and_flight_with_its_booking_metadata()
    {
        var (airlineId, supplierId) = await SupplierAsync();

        var meal = await _proof.ProveAsync(
            CarrierDefinition(airlineId, supplierId, "MEAL_VGML", "VGM", "F", "ML", "Vegetarian meal", Ssr("VGML"), variant: "A12"),
            definitionId => Provision(definitionId, 10, bookingRequired: true) with
            {
                PassengerEligibility = Passengers(PassengerTypeCode.ADT, PassengerTypeCode.CHD),
                FlightApplication = new(AllowedFlightIds: [81234]),
                FareApplication = new(AllowedCabinClassIds: [1])
            },
            draft => draft with
            {
                FlightApplication = new(AllowedFlightIds: [81234, 81240]),
                FareApplication = new(AllowedFareFamilyIds: [5], AllowedCabinClassIds: [1, 2]),
                AdvancePurchase = new(24, TimeUnit.Hours)
            },
            Definition(PricingUnit.PerPassenger, ServiceDateBasis.FlightDeparture, "Ssr", "VGML"),
            draft =>
            {
                Rules("PTC=ADT,CHD; FLT=81234; CABIN=1")(draft);
                Assert.True(draft.BookingRequired);
            },
            Rules("PTC=ADT,CHD; FLT=81234,81240; FAMILY=5; CABIN=1,2; ADVANCE=24Hours"),
            Priced(
                "PerPassenger EUR ADT[-]=12+0+0=12 | CHD[-]=8+0+0=8",
                "PerPassenger EUR ADT[-]=13+0+0=13 | CHD[-]=8.5+0+0=8.5",
                provisionId => Pricing(provisionId, Eur, Base(12m, PassengerTypeCode.ADT), Base(8m, PassengerTypeCode.CHD)),
                provisionId => Pricing(provisionId, Eur, Base(13m, PassengerTypeCode.ADT), Base(8.5m, PassengerTypeCode.CHD))));
        var business = await _proof.RuleAsync(
            Provision(meal.ServiceDefinitionId, 5, CommercialDisposition.Free, bookingRequired: true) with { FareApplication = new(AllowedCabinClassIds: [3]) });

        Assert.Equal(("Free", true), (business.Provision.Disposition.Name, business.Provision.BookingRequired));
        Assert.Equal(new[] { 5, 10 }, (await FiledAsync(meal.ServiceDefinitionId)).Select(row => row.Sequence));
    }

    [Fact]
    public async Task V121_F06_P06_a_paid_seat_is_per_seat_by_aircraft_and_seat_characteristic_and_owns_no_occupancy()
    {
        var (airlineId, supplierId) = await SupplierAsync();

        TestDefineProvisionCommand SeatRule(long definitionId, int sequence, CommercialDisposition disposition = CommercialDisposition.Paid)
            => Provision(definitionId, sequence, disposition, applicationType: ProvisionApplicationType.Seat);

        var seat = await _proof.ProveAsync(
            CarrierDefinition(airlineId, supplierId, "SEAT_CHOICE", "SEA", "F", "SA", "Seat selection", pricingUnit: PricingUnit.PerSeat, variant: "A07"),
            definitionId => SeatRule(definitionId, 10) with { FlightApplication = new(AllowedAircraftIds: [1]), SeatApplication = Seat(null, ["W", "LS"]) },
            draft => draft with { FlightApplication = new(AllowedAircraftIds: [1, 2]), SeatApplication = Seat(["12a", "12F"], ["W"]) },
            Definition(PricingUnit.PerSeat, ServiceDateBasis.FlightDeparture),
            draft =>
            {
                Rules("ACFT=1; SEATCHAR=W,LS")(draft);
                Assert.Equal(("Seat", false), (draft.ApplicationType.Name, draft.MustCheckAvailability));
            },
            Rules("ACFT=1,2; SEAT=12A,12F; SEATCHAR=W"),
            Flat(PricingUnit.PerSeat, 15m, 17m));
        var blocked = await _proof.RuleAsync(SeatRule(seat.ServiceDefinitionId, 5, CommercialDisposition.NotAvailable) with { SeatApplication = Seat(null, ["E"]) });

        Assert.Equal("SEATCHAR=E", RuleText.Of(blocked.Provision));
        await RefusedAsync(16302, 422, scope => scope.DefineProvision.DefineAsync(SeatRule(seat.ServiceDefinitionId, 20) with { SeatApplication = Seat(["14C"], null) }));
        await RefusedAsync(16302, 422, scope => scope.DefineProvision.DefineAsync(SeatRule(seat.ServiceDefinitionId, 20)));
        await RefusedAsync(16508, 409, scope => scope.DefinePricing.DefineAsync(Pricing(seat.ProvisionId, Eur, Base(15m, PassengerTypeCode.ADT))));
        await RefusedAsync(16505, 409, scope => scope.DefinePricing.DefineAsync(Pricing(blocked.Provision.Id, Eur, Base(15m))));
    }

    [Fact]
    public async Task V121_F07_an_airport_lounge_is_filed_by_service_location_date_and_local_time_with_passenger_rates()
    {
        var (airlineId, supplierId) = await SupplierAsync();

        var lounge = await _proof.ProveAsync(
            M1Commands.LoungeDefinition(airlineId, supplierId),
            definitionId => Provision(definitionId, 10) with
            {
                Geography = new(ServiceLocations: [Location(ServiceLocationType.Airport, Ika)]),
                TravelDate = Dates([Period(Day(2027, 1, 1), Day(2027, 12, 31))]),
                DayTimeApplication = DayTime(Window(Weekdays, 6, 22))
            },
            draft => draft with
            {
                TravelDate = Dates([Period(Day(2027, 1, 1), Day(2027, 12, 31))], [Period(Day(2027, 3, 20), Day(2027, 3, 21))]),
                DayTimeApplication = DayTime(Window(EveryDay, 5, 23), Window(Friday, 12, 14, DayTimeRestrictionEffect.Deny))
            },
            definition =>
            {
                Definition(PricingUnit.PerPassenger, ServiceDateBasis.ServiceStart)(definition);
                Assert.Equal(("Industry", "0BX", "EmdStandalone"), (definition.SubCodeSource.Name, definition.ServiceSubCode, definition.DocumentType.Name));
            },
            Rules("AT=Airport:2; PERMIT=2027-01-01..2027-12-31; TIME=31:6-22:Allow"),
            Rules("AT=Airport:2; PERMIT=2027-01-01..2027-12-31; BLACKOUT=2027-03-20..2027-03-21; TIME=127:5-23:Allow,16:12-14:Deny"),
            Priced(
                "PerPassenger EUR ADT[-]=25+2.5+0=27.5 | CHD[-]=15+1.5+0=16.5",
                "PerPassenger EUR ADT[-]=26+2.6+1=29.6 | CHD[-]=15+1.5+1=17.5",
                provisionId => Pricing(provisionId, Eur, Base(25m, PassengerTypeCode.ADT), Tax("VAT", 2.5m, PassengerTypeCode.ADT), Base(15m, PassengerTypeCode.CHD), Tax("VAT", 1.5m, PassengerTypeCode.CHD)),
                provisionId => Pricing(
                    provisionId,
                    Eur,
                    Base(26m, PassengerTypeCode.ADT),
                    Tax("VAT", 2.6m, PassengerTypeCode.ADT),
                    Fee("SVC", 1m, PassengerTypeCode.ADT),
                    Base(15m, PassengerTypeCode.CHD),
                    Tax("VAT", 1.5m, PassengerTypeCode.CHD),
                    Fee("SVC", 1m, PassengerTypeCode.CHD))));
        var infants = await _proof.RuleAsync(Provision(lounge.ServiceDefinitionId, 5, CommercialDisposition.Free) with { PassengerEligibility = Passengers(PassengerTypeCode.INF) });

        Assert.Equal(("Free", "PTC=INF"), (infants.Provision.Disposition.Name, RuleText.Of(infants.Provision)));
        Assert.Equal((1, 1, 2), (await _proof.ListedProvisionsAsync(lounge.ServiceDefinitionId)).Where(row => row.Sequence == 10).Select(row => (row.PermittedPeriodCount, row.BlackoutPeriodCount, row.DayTimeWindowCount)).Single());
    }

    [Fact]
    public async Task V121_F08_priority_boarding_is_dated_by_the_flight_departure_on_selected_flights_and_windows()
    {
        var (airlineId, supplierId) = await SupplierAsync();

        var priority = await _proof.ProveAsync(
            CarrierDefinition(airlineId, supplierId, "PRIORITY_BOARDING", "PRB", "F", "TS", "Priority boarding", variant: "A23"),
            definitionId => Provision(definitionId, 10) with
            {
                FlightApplication = new(AllowedFlightIds: [81234, 81240]),
                TravelDate = Dates([Period(Day(2027, 6, 1), Day(2027, 8, 31))]),
                DayTimeApplication = DayTime(Window((byte)(Monday | Friday), 6, 10))
            },
            draft => draft with
            {
                FlightApplication = new(AllowedMarketingAirlineIds: [1], AllowedFlightNumbers: ["W5112"], AllowedFlightIds: [81234, 81240]),
                DayTimeApplication = DayTime(Window((byte)(Monday | Friday), 6, 10), Window(Saturday, 22), Window(Sunday, toHour: 2))
            },
            Definition(PricingUnit.PerPassenger, ServiceDateBasis.FlightDeparture),
            Rules("FLT=81234,81240; PERMIT=2027-06-01..2027-08-31; TIME=17:6-10:Allow"),
            Rules("MKT=1; FLTNO=W5112; FLT=81234,81240; PERMIT=2027-06-01..2027-08-31; TIME=17:6-10:Allow,32:22-:Allow,64:-2:Allow"),
            Flat(PricingUnit.PerPassenger, 8m, 9m));
        var included = await _proof.RuleAsync(Provision(priority.ServiceDefinitionId, 5, CommercialDisposition.Free) with { FareApplication = new(AllowedFareFamilyIds: [9, 10]) });

        Assert.Equal(("Free", "FAMILY=9,10"), (included.Provision.Disposition.Name, RuleText.Of(included.Provision)));
    }

    [Fact]
    public async Task V121_F09_fast_track_is_dated_by_the_service_start_and_filed_by_point_of_sale_with_a_blackout()
    {
        var (airlineId, supplierId) = await SupplierAsync();

        var fastTrack = await _proof.ProveAsync(
            CarrierDefinition(airlineId, supplierId, "FAST_TRACK", "FST", "F", "TS", "Fast track security", serviceDateBasis: ServiceDateBasis.ServiceStart, variant: "A21"),
            definitionId => Provision(definitionId, 10) with
            {
                SalesRestrictions = new(AllowedPointOfSaleIds: [AirlineOffice]),
                Geography = new(ServiceLocations: [Location(ServiceLocationType.Airport, Thr)]),
                TravelDate = Dates(blackout: [Period(Day(2027, 3, 20), Day(2027, 4, 2))])
            },
            draft => draft with
            {
                SalesRestrictions = new(AllowedPointOfSaleIds: [AgencyOffice], AllowedCustomerTypes: [CustomerType.Individual, CustomerType.TravelAgency]),
                TravelDate = Dates(blackout: [Period(Day(2027, 3, 20), Day(2027, 4, 2)), Period(Day(2027, 4, 3), Day(2027, 4, 5))])
            },
            Definition(PricingUnit.PerPassenger, ServiceDateBasis.ServiceStart),
            Rules($"POS={AirlineOffice}; AT=Airport:1; BLACKOUT=2027-03-20..2027-04-02"),
            Rules($"POS={AgencyOffice}; CUSTTYPE=Individual,TravelAgency; AT=Airport:1; BLACKOUT=2027-03-20..2027-04-05"),
            Flat(PricingUnit.PerPassenger, 10m, 12m));

        Assert.Equal((0, 1), (await _proof.ListedProvisionsAsync(fastTrack.ServiceDefinitionId)).Select(row => (row.PermittedPeriodCount, row.BlackoutPeriodCount)).Single());
    }

    [Fact]
    public async Task V121_F10_wifi_is_per_item_on_supported_flights_and_aircraft()
    {
        var (airlineId, supplierId) = await SupplierAsync();

        var wifi = await _proof.ProveAsync(
            CarrierDefinition(airlineId, supplierId, "WIFI_FULL", "WIF", "F", "IE", "Wi-Fi full flight", pricingUnit: PricingUnit.PerItem, variant: "A24"),
            definitionId => Provision(definitionId, 10, maxQuantity: 4) with { FlightApplication = new(AllowedFlightIds: [81234], AllowedAircraftIds: [3]) },
            draft => draft with { FlightApplication = new(AllowedOperatingAirlineIds: [1], AllowedFlightIds: [81234, 81240], AllowedAircraftIds: [3, 4]) },
            Definition(PricingUnit.PerItem, ServiceDateBasis.FlightDeparture),
            Rules("FLT=81234; ACFT=3"),
            Rules("OPR=1; FLT=81234,81240; ACFT=3,4"),
            Flat(PricingUnit.PerItem, 7m, 8m));

        await RefusedAsync(16508, 409, scope => scope.DefinePricing.DefineAsync(Pricing(wifi.ProvisionId, Eur, Base(7m, PassengerTypeCode.ADT))));
        await RefusedAsync(16508, 409, scope => scope.DefinePricing.DefineAsync(Pricing(wifi.ProvisionId, Eur, Base(7m, null, 0, 12), Base(9m, null, 12))));
    }

    [Fact]
    public async Task V121_F11_a_pet_service_is_per_item_with_a_route_a_flight_scope_and_booking_metadata()
    {
        var (airlineId, supplierId) = await SupplierAsync();

        await _proof.ProveAsync(
            CarrierDefinition(airlineId, supplierId, "PET_IN_CABIN", "PET", "C", "PT", "Pet in cabin", new ServiceDefinitionBookingInput(BookingMethod.Ssr, "PETC", null, ConfirmationRequirement.SubjectToConfirmation), pricingUnit: PricingUnit.PerItem, variant: "A13"),
            definitionId => Provision(definitionId, 10, coverageScope: ServiceCoverageScope.Journey, bookingRequired: true) with
            {
                Geography = new(AllowedRoutePairs: [Pair(Thr, Ist, RoutePairDirection.BothDirections)]),
                FlightApplication = new(AllowedFlightNumbers: ["w5112", "W5113"])
            },
            draft => draft with
            {
                Geography = new(AllowedRoutePairs: [Pair(Thr, Ist, RoutePairDirection.BothDirections), Pair(Thr, Mhd)]),
                FlightApplication = new(AllowedFlightNumbers: ["W5112", "W5113"], AllowedAircraftIds: [1]),
                AdvancePurchase = new(2, TimeUnit.Days)
            },
            Definition(PricingUnit.PerItem, ServiceDateBasis.FlightDeparture, "Ssr", "PETC"),
            draft =>
            {
                Rules("ROUTE=1<>6; FLTNO=W5112,W5113")(draft);
                Assert.Equal((true, "Journey"), (draft.BookingRequired, draft.CoverageScope.Name));
            },
            Rules("ROUTE=1<>6,1>3; FLTNO=W5112,W5113; ACFT=1; ADVANCE=2Days"),
            Flat(PricingUnit.PerItem, 50m, 55m));
    }

    [Fact]
    public async Task V121_F12_meet_and_assist_is_dated_by_the_service_start_at_a_service_airport_with_passenger_rates()
    {
        var (airlineId, supplierId) = await SupplierAsync();

        await _proof.ProveAsync(
            CarrierDefinition(airlineId, supplierId, "MEET_ASSIST", "MAS", "F", "TS", "Meet and assist", serviceDateBasis: ServiceDateBasis.ServiceStart, variant: "A22"),
            definitionId => Provision(definitionId, 10) with
            {
                PassengerEligibility = Passengers(PassengerTypeCode.ADT, PassengerTypeCode.CHD),
                Geography = new(ServiceLocations: [Location(ServiceLocationType.Airport, Ika)]),
                AdvancePurchase = new(12, TimeUnit.Hours)
            },
            draft => draft with
            {
                Geography = new(ServiceLocations: [Location(ServiceLocationType.Airport, Ika), Location(ServiceLocationType.Airport, Mhd)]),
                DayTimeApplication = DayTime(Window(EveryDay, 4, 23))
            },
            Definition(PricingUnit.PerPassenger, ServiceDateBasis.ServiceStart),
            Rules("PTC=ADT,CHD; AT=Airport:2; ADVANCE=12Hours"),
            Rules("PTC=ADT,CHD; AT=Airport:2,Airport:3; TIME=127:4-23:Allow; ADVANCE=12Hours"),
            Priced(
                "PerPassenger EUR ADT[-]=30+0+0=30 | CHD[-]=15+0+0=15",
                "PerPassenger EUR ADT[-]=32+0+0=32 | CHD[-]=16+0+0=16",
                provisionId => Pricing(provisionId, Eur, Base(30m, PassengerTypeCode.ADT), Base(15m, PassengerTypeCode.CHD)),
                provisionId => Pricing(provisionId, Eur, Base(32m, PassengerTypeCode.ADT), Base(16m, PassengerTypeCode.CHD))));
    }

    [Fact]
    public async Task V121_F13_unaccompanied_minor_handling_is_for_children_free_or_not_available_in_a_local_time_period()
    {
        var (airlineId, supplierId) = await SupplierAsync();

        var minor = await _proof.ProveAsync(
            CarrierDefinition(airlineId, supplierId, "UMNR", "UMN", "F", "UN", "Unaccompanied minor", Ssr("UMNR"), variant: "A19"),
            definitionId => Provision(definitionId, 20, CommercialDisposition.Free, ServiceCoverageScope.Journey, bookingRequired: true) with
            {
                PassengerEligibility = new([PassengerTypeCode.CHD], [new(5, 12)]),
                DayTimeApplication = DayTime(Window(EveryDay, 6, 20))
            },
            draft => draft with { DayTimeApplication = DayTime(Window(EveryDay, 6, 20), Window(Friday, 12, 14, DayTimeRestrictionEffect.Deny)) },
            Definition(PricingUnit.PerPassenger, ServiceDateBasis.FlightDeparture, "Ssr", "UMNR"),
            Rules("PTC=CHD; AGE=5-12; TIME=127:6-20:Allow", "Free"),
            Rules("PTC=CHD; AGE=5-12; TIME=127:6-20:Allow,16:12-14:Deny", "Free"));
        var night = await _proof.RuleAsync(
            Provision(minor.ServiceDefinitionId, 10, CommercialDisposition.NotAvailable, ServiceCoverageScope.Journey) with
            {
                PassengerEligibility = Passengers(PassengerTypeCode.CHD),
                DayTimeApplication = DayTime(Window(EveryDay, 20), Window(EveryDay, toHour: 6))
            });

        Assert.Null(minor.PricingId);
        Assert.Equal(("NotAvailable", "PTC=CHD; TIME=127:20-:Allow,127:-6:Allow"), (night.Provision.Disposition.Name, RuleText.Of(night.Provision)));
        Assert.Equal(new[] { (10, "NotAvailable"), (20, "Free") }, (await FiledAsync(minor.ServiceDefinitionId)).Select(row => (row.Sequence, row.Disposition)));
    }

    [Theory]
    [InlineData("INS_BASIC", PricingUnit.PerPassenger, ServiceDateBasis.CoverageStart)]
    [InlineData("HOTEL_ROOM_STD", PricingUnit.PerRoom, ServiceDateBasis.CheckIn)]
    [InlineData("ESIM_TR_5GB", PricingUnit.PerItem, ServiceDateBasis.Activation)]
    [InlineData("TRANSFER_PRIVATE", PricingUnit.PerVehicle, ServiceDateBasis.ServiceStart)]
    [InlineData("TRANSFER_SHARED", PricingUnit.PerPassenger, ServiceDateBasis.ServiceStart)]
    public async Task V122_a_product_outside_the_nine_profiles_has_no_variant_and_cannot_be_authored(string reference, PricingUnit pricingUnit, ServiceDateBasis basis)
    {
        var (airlineId, supplierId) = await SupplierAsync();
        var command = CarrierDefinition(airlineId, supplierId, reference, "OUT", "M", "XX", reference, pricingUnit: pricingUnit, serviceDateBasis: basis, variant: "NONE");

        Assert.DoesNotContain(AncillaryVariant.All, variant => variant.Code == command.VariantCode);
        await RefusedAsync(16202, 422, scope => scope.DefineServiceDefinition.DefineAsync(command));

        await using var reader = new AncillaryScope(_database, _clock);

        Assert.False(await reader.Command.AncillaryServiceDefinitions.AnyAsync(definition => definition.OwnerAirlineId == airlineId));
    }
}
