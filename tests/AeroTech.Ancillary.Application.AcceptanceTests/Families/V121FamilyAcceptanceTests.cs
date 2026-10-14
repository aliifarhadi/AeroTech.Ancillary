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
