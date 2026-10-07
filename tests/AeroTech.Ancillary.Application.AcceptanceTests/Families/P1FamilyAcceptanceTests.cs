using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;
using AeroTech.Messages.Core.Enums;
using Xunit;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.P1Commands;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Families;

[Collection(DatabaseCollection.Name)]
public class P1FamilyAcceptanceTests
{
    private readonly TestDatabase _database;
    private readonly FixedClock _clock = new();
    private readonly FamilyProof _proof;

    public P1FamilyAcceptanceTests(TestDatabase database)
    {
        _database = database;
        _proof = new FamilyProof(database, _clock);
    }

    private static decimal Amount(BackofficeProvisionDto provision) => provision.PriceLines.Sum(line => line.UnitAmount);

    private static string[] Passengers(BackofficeProvisionDto provision)
        => provision.Passenger.PassengerTypeCodes.Select(code => code.Name).ToArray();

    [Fact]
    public async Task P1_FAM01_extra_prepaid_baggage_is_authored_as_piece_and_weight_packages_with_every_outcome()
    {
        var airlineId = _database.NextAirlineId();
        var supplierId = await _proof.SupplierAsync(M1Commands.LocalSupplier(airlineId));

        var proof = await _proof.ProveAsync(
            FirstExcessBagDefinition(airlineId, supplierId),
            definitionId => Provision(
                definitionId,
                10,
                30m,
                coverageScope: ServiceCoverageScope.Journey,
                quantityUnit: AncillaryQuantityUnit.Piece,
                maxQuantity: 3,
                passenger: P1Commands.Passengers(PassengerTypeCode.ADT),
                sales: new ProvisionSalesCriteriaInput(PointOfSaleIds: [501]),
                travel: new ProvisionTravelCriteriaInput(
                    RoutePairs: [Pair(Thr, Ist)],
                    FlightIds: [81234],
                    TravelFrom: new DateOnly(2026, 12, 20),
                    TravelTo: new DateOnly(2026, 12, 31)),
                fare: new ProvisionFareCriteriaInput(FareFamilyIds: [5]),
                application: Baggage(23m, 1, 1, BaggageTravelApplication.AllSectors)),
            draft => draft with
            {
                Fee = draft.Fee! with { PriceLines = Price(32m, "Extra bag") },
                Travel = draft.Travel! with { TravelTo = new DateOnly(2027, 1, 10) },
                Application = Baggage(25m, 1, 1, BaggageTravelApplication.AllSectors)
            },
            definition =>
            {
                Assert.Equal(("Industry", "0CC", "C", "BG", "B1"), (definition.SubCodeSource.Name, definition.ServiceSubCode, definition.ServiceTypeCode, definition.GroupCode, definition.Description1Code));
                Assert.Equal(("EmdAssociated", "C", "0CC"), (definition.DocumentType.Name, definition.DocumentRfic, definition.DocumentRfisc));
                Assert.Equal(("Ssr", "XBAG"), (definition.BookingMethod.Name, definition.BookingSsrCode));
            },
            draft =>
            {
                Assert.Equal(("Baggage", "Journey", "Piece", 1, 3), (draft.ApplicationType.Name, draft.CoverageScope.Name, draft.QuantityUnit.Name, draft.MinQuantity, draft.MaxQuantity));
                Assert.Equal((1, 1, 23m, "Kg", "AllSectors", "Prepaid"), (draft.Baggage!.FirstExcessPiece, draft.Baggage.LastExcessPiece, draft.Baggage.Weight, draft.Baggage.WeightUnit.Name, draft.Baggage.TravelApplication!.Name, draft.Baggage.PurchaseApplication.Name));
                Assert.Equal(new[] { "ADT" }, Passengers(draft));
                Assert.Equal(new long[] { 501 }, draft.Sales.PointOfSaleIds);
                Assert.Equal((Thr, Ist), draft.Travel.RoutePairs.Select(pair => (pair.OriginAirportId, pair.DestinationAirportId)).Single());
                Assert.Equal(new long[] { 81234 }, draft.Travel.FlightIds);
                Assert.Equal((new DateOnly(2026, 12, 20), new DateOnly(2026, 12, 31)), (draft.Travel.TravelFrom!.Value, draft.Travel.TravelTo!.Value));
                Assert.Equal(new long[] { 5 }, draft.Fare.FareFamilyIds);
                Assert.Equal(("Paid", 30m, Eur), (draft.Disposition.Name, Amount(draft), draft.FeeCurrencyId));
            },
            edited =>
            {
                Assert.Equal((32m, 25m, new DateOnly(2027, 1, 10)), (Amount(edited), edited.Baggage!.Weight, edited.Travel.TravelTo!.Value));
                Assert.Equal(new long[] { 81234 }, edited.Travel.FlightIds);
                Assert.Equal(new[] { "ADT" }, Passengers(edited));
            });

        var weightDefinition = await _proof.DefinitionAsync(
            CarrierDefinition(airlineId, supplierId, "XBAG_20KG", "XK2", "C", "BG", "Extra baggage 20 kg", Ssr("XBAG")),
            activate: true);
        var weightPackage = await _proof.ProvisionAsync(
            Provision(
                weightDefinition.Id,
                10,
                45m,
                coverageScope: ServiceCoverageScope.Journey,
                quantityUnit: AncillaryQuantityUnit.Kilogram,
                minQuantity: 20,
                maxQuantity: 20,
                application: Baggage(20m)),
            activate: true);
        var free = await _proof.ProvisionAsync(
            Provision(
                proof.ServiceDefinitionId,
                20,
                disposition: CommercialDisposition.Free,
                coverageScope: ServiceCoverageScope.Journey,
                quantityUnit: AncillaryQuantityUnit.Piece,
                fare: new ProvisionFareCriteriaInput(FareFamilyIds: [6]),
                application: Baggage(23m, 1, 1)),
            activate: true);
        var notAvailable = await _proof.ProvisionAsync(
            Provision(
                proof.ServiceDefinitionId,
                30,
                disposition: CommercialDisposition.NotAvailable,
                coverageScope: ServiceCoverageScope.Journey,
                quantityUnit: AncillaryQuantityUnit.Piece,
                passenger: P1Commands.Passengers(PassengerTypeCode.INF),
                application: Baggage(23m, 1, 1)),
            activate: true);

        Assert.Equal(("CarrierDefined", "XK2", "Active"), (weightDefinition.SubCodeSource.Name, weightDefinition.ServiceSubCode, weightDefinition.Status.Name));
        Assert.Equal(("Active", "Kilogram", 20, 20, 20m, 45m), (weightPackage.Status.Name, weightPackage.QuantityUnit.Name, weightPackage.MinQuantity, weightPackage.MaxQuantity, weightPackage.Baggage!.Weight, Amount(weightPackage)));
        Assert.Null(weightPackage.Baggage.FirstExcessPiece);
        Assert.Equal(("Active", "Free", (int?)null, 0), (free.Status.Name, free.Disposition.Name, free.FeeCurrencyId, free.PriceLines.Count));
        Assert.Equal(new long[] { 6 }, free.Fare.FareFamilyIds);
        Assert.Equal(("Active", "NotAvailable", 0), (notAvailable.Status.Name, notAvailable.Disposition.Name, notAvailable.PriceLines.Count));
        Assert.Equal(new[] { "INF" }, Passengers(notAvailable));
        Assert.Equal(
            new[] { (10, "Paid"), (20, "Free"), (30, "NotAvailable") },
            (await _proof.ListedProvisionsAsync(proof.ServiceDefinitionId)).Select(row => (row.Sequence, row.Disposition.Name)));
    }

    [Fact]
    public async Task P1_FAM02_sports_and_special_baggage_are_separate_definitions_with_fixed_prices()
    {
        var airlineId = _database.NextAirlineId();
        var supplierId = await _proof.SupplierAsync(M1Commands.LocalSupplier(airlineId));

        await _proof.ProveAsync(
            CarrierDefinition(airlineId, supplierId, "SPORT_SKI", "SKI", "C", "SB", "Ski equipment", Ssr("SPEQ"), subGroupCode: "SK"),
            definitionId => Provision(
                definitionId,
                10,
                45m,
                coverageScope: ServiceCoverageScope.Journey,
                quantityUnit: AncillaryQuantityUnit.Piece,
                travel: new ProvisionTravelCriteriaInput(RoutePairs: [Pair(Thr, Ist, RoutePairDirection.BothDirections)], AircraftIds: [320, 321]),
                application: Baggage(32m, purchaseApplication: BaggagePurchaseApplication.PrepaidAndCheckIn)),
            draft => draft with { Fee = draft.Fee! with { PriceLines = Price(48m, "Ski equipment") }, Application = Baggage(30m, purchaseApplication: BaggagePurchaseApplication.CheckIn) },
            definition =>
            {
                Assert.Equal(("CarrierDefined", "SKI", "C", "SB", "SK"), (definition.SubCodeSource.Name, definition.ServiceSubCode, definition.ServiceTypeCode, definition.GroupCode, definition.SubGroupCode));
                Assert.Equal(("Ssr", "SPEQ", "None"), (definition.BookingMethod.Name, definition.BookingSsrCode, definition.DocumentType.Name));
            },
            draft =>
            {
                Assert.Equal(("Baggage", 32m, "PrepaidAndCheckIn", 45m), (draft.ApplicationType.Name, draft.Baggage!.Weight, draft.Baggage.PurchaseApplication.Name, Amount(draft)));
                Assert.Equal((Thr, Ist, "BothDirections"), draft.Travel.RoutePairs.Select(pair => (pair.OriginAirportId, pair.DestinationAirportId, pair.Direction.Name)).Single());
                Assert.Equal(new[] { 320, 321 }, draft.Travel.AircraftIds);
            },
            edited => Assert.Equal((30m, "CheckIn", 48m), (edited.Baggage!.Weight, edited.Baggage.PurchaseApplication.Name, Amount(edited))));

        var golf = await _proof.DefinitionAsync(
            CarrierDefinition(airlineId, supplierId, "SPORT_GOLF", "GLF", "C", "SB", "Golf bag", Ssr("SPEQ"), subGroupCode: "GF"),
            activate: true);
        var golfPrice = await _proof.ProvisionAsync(Provision(golf.Id, 10, 40m, coverageScope: ServiceCoverageScope.Journey), activate: true);

        Assert.Equal(("GLF", "GF", "Active"), (golf.ServiceSubCode, golf.SubGroupCode, golf.Status.Name));
        Assert.Equal(("Standard", "Active", 40m), (golfPrice.ApplicationType.Name, golfPrice.Status.Name, Amount(golfPrice)));
        Assert.Null(golfPrice.Baggage);
    }

    [Fact]
    public async Task P1_FAM03_wheelchair_assistance_uses_ssr_booking_metadata_with_free_and_paid_configurations()
    {
        var airlineId = _database.NextAirlineId();
        var supplierId = await _proof.SupplierAsync(M1Commands.LocalSupplier(airlineId));

        await _proof.ProveAsync(
            CarrierDefinition(airlineId, supplierId, "WHEELCHAIR_RAMP", "WCR", "F", "AS", "Wheelchair to aircraft door", Ssr("WCHR")),
            definitionId => Provision(
                definitionId,
                10,
                disposition: CommercialDisposition.Free,
                travel: new ProvisionTravelCriteriaInput(OriginAirportIds: [Ika, Thr])),
            draft => draft with { Travel = new ProvisionTravelCriteriaInput(OriginAirportIds: [Ika, Thr, Mhd]), Outcome = draft.Outcome with { BookingRequired = true } },
            definition =>
            {
                Assert.Equal(("CarrierDefined", "WCR", "F", "AS"), (definition.SubCodeSource.Name, definition.ServiceSubCode, definition.ServiceTypeCode, definition.GroupCode));
                Assert.Equal(("Ssr", "WCHR", "None"), (definition.BookingMethod.Name, definition.BookingSsrCode, definition.DocumentType.Name));
            },
            draft =>
            {
                Assert.Equal(("Standard", "Free", 0), (draft.ApplicationType.Name, draft.Disposition.Name, draft.PriceLines.Count));
                Assert.Equal(new[] { Ika, Thr }, draft.Travel.OriginAirportIds);
                Assert.False(draft.BookingRequired);
            },
            edited =>
            {
                Assert.Equal(new[] { Ika, Thr, Mhd }, edited.Travel.OriginAirportIds);
                Assert.True(edited.BookingRequired);
                Assert.Equal("Free", edited.Disposition.Name);
            });

        foreach (var (reference, subCode, ssr, name) in new[]
                 {
                     ("WHEELCHAIR_STEPS", "WCS", "WCHS", "Wheelchair up the steps"),
                     ("WHEELCHAIR_CABIN", "WCC", "WCHC", "Wheelchair to the cabin seat")
                 })
        {
            var definition = await _proof.DefinitionAsync(
                CarrierDefinition(airlineId, supplierId, reference, subCode, "F", "AS", name, Ssr(ssr)),
                activate: true);
            var paid = await _proof.ProvisionAsync(
                Provision(definition.Id, 10, 20m, sales: new ProvisionSalesCriteriaInput(PointOfSaleIds: [601])),
                activate: true);
            var free = await _proof.ProvisionAsync(Provision(definition.Id, 100, disposition: CommercialDisposition.Free), activate: true);

            Assert.Equal((ssr, "Active"), (definition.BookingSsrCode, definition.Status.Name));
            Assert.Equal(("Paid", 20m, "Active"), (paid.Disposition.Name, Amount(paid), paid.Status.Name));
            Assert.Equal(("Free", "Active"), (free.Disposition.Name, free.Status.Name));
        }
    }

    [Fact]
    public async Task P1_FAM04_meals_are_separate_definitions_priced_by_passenger_type_flight_date_cabin_and_fare_family()
    {
        var airlineId = _database.NextAirlineId();
        var supplierId = await _proof.SupplierAsync(M1Commands.LocalSupplier(airlineId, "Sky Catering"));

        var proof = await _proof.ProveAsync(
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
                12m,
                passenger: P1Commands.Passengers(PassengerTypeCode.ADT),
                travel: new ProvisionTravelCriteriaInput(
                    FlightIds: [81234, 81240],
                    FlightNumbers: ["W5112"],
                    TravelFrom: new DateOnly(2026, 12, 1),
                    TravelTo: new DateOnly(2027, 2, 28)),
                fare: new ProvisionFareCriteriaInput(FareFamilyIds: [5, 6], CabinClassIds: [2]),
                advancePurchase: new ProvisionAdvancePurchaseInput(24, TimeUnit.Hours)),
            draft => draft with
            {
                Fee = draft.Fee! with { PriceLines = Price(13.50m, "Vegetarian meal") },
                Fare = new ProvisionFareCriteriaInput(FareFamilyIds: [5], CabinClassIds: [1, 2]),
                AdvancePurchase = new ProvisionAdvancePurchaseInput(2, TimeUnit.Days)
            },
            definition =>
            {
                Assert.Equal(("MVG", "ML", "VG", "Ssr", "VGML"), (definition.ServiceSubCode, definition.GroupCode, definition.SubGroupCode, definition.BookingMethod.Name, definition.BookingSsrCode));
                Assert.Equal(("EmdAssociated", "G", "MVG"), (definition.DocumentType.Name, definition.DocumentRfic, definition.DocumentRfisc));
            },
            draft =>
            {
                Assert.Equal(new[] { "ADT" }, Passengers(draft));
                Assert.Equal(new long[] { 81234, 81240 }, draft.Travel.FlightIds);
                Assert.Equal(new[] { "W5112" }, draft.Travel.FlightNumbers);
                Assert.Equal((new DateOnly(2026, 12, 1), new DateOnly(2027, 2, 28)), (draft.Travel.TravelFrom!.Value, draft.Travel.TravelTo!.Value));
                Assert.Equal(new long[] { 5, 6 }, draft.Fare.FareFamilyIds);
                Assert.Equal(new[] { 2 }, draft.Fare.CabinClassIds);
                Assert.Equal((24, "Hours", 12m), (draft.AdvancePurchase!.Period, draft.AdvancePurchase.Unit.Name, Amount(draft)));
            },
            edited =>
            {
                Assert.Equal(new long[] { 5 }, edited.Fare.FareFamilyIds);
                Assert.Equal(new[] { 1, 2 }, edited.Fare.CabinClassIds);
                Assert.Equal((2, "Days", 13.50m), (edited.AdvancePurchase!.Period, edited.AdvancePurchase.Unit.Name, Amount(edited)));
            });

        var child = await _proof.ProvisionAsync(
            Provision(proof.ServiceDefinitionId, 20, 8m, passenger: P1Commands.Passengers(PassengerTypeCode.CHD)),
            activate: true);
        var childMeal = await _proof.DefinitionAsync(
            CarrierDefinition(airlineId, supplierId, "MEAL_CHML", "MCH", "F", "ML", "Child meal", Ssr("CHML"), subGroupCode: "CH"),
            activate: true);
        var childMealPrice = await _proof.ProvisionAsync(
            Provision(childMeal.Id, 10, 6m, passenger: P1Commands.Passengers(PassengerTypeCode.CHD, PassengerTypeCode.INS)),
            activate: true);

        Assert.Equal((8m, "Active"), (Amount(child), child.Status.Name));
        Assert.Equal(new[] { "CHD" }, Passengers(child));
        Assert.Equal(("CHML", "MCH"), (childMeal.BookingSsrCode, childMeal.ServiceSubCode));
        Assert.Equal(new[] { "CHD", "INS" }, Passengers(childMealPrice));
    }

    [Fact]
    public async Task P1_FAM05_insurance_plans_are_separate_definitions_with_journey_and_order_coverage()
    {
        var airlineId = _database.NextAirlineId();
        var insurerId = await _proof.SupplierAsync(new TestRegisterSupplierCommand(airlineId, "SafeTrip Insurance", SupplierFulfillmentKind.External, "InsurancePartnerA"));

        var proof = await _proof.ProveAsync(
            CarrierDefinition(
                airlineId,
                insurerId,
                "INS_BASIC",
                "INB",
                "M",
                "IN",
                "Travel insurance basic",
                salesEffectiveFrom: new DateOnly(2026, 10, 1),
                salesDiscontinueOn: new DateOnly(2027, 9, 30)),
            definitionId => Provision(
                definitionId,
                10,
                9m,
                coverageScope: ServiceCoverageScope.Journey,
                salesEffectiveFrom: new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.Zero),
                salesDiscontinueAt: new DateTimeOffset(2027, 1, 1, 0, 0, 0, TimeSpan.Zero),
                passenger: P1Commands.Passengers(PassengerTypeCode.ADT),
                sales: new ProvisionSalesCriteriaInput(PointOfSaleIds: [501, 502], CustomerTypes: [CustomerType.Individual]),
                travel: new ProvisionTravelCriteriaInput(TravelFrom: new DateOnly(2026, 10, 1), TravelTo: new DateOnly(2026, 12, 31))),
            draft => draft with
            {
                Fee = draft.Fee! with { PriceLines = Price(9.50m, "Travel insurance basic") },
                Sales = new ProvisionSalesCriteriaInput(PointOfSaleIds: [501], CustomerTypes: [CustomerType.Individual, CustomerType.Organization]),
                SalesDiscontinueAt = new DateTimeOffset(2027, 2, 1, 0, 0, 0, TimeSpan.Zero)
            },
            definition =>
            {
                Assert.Equal((insurerId, "SafeTrip Insurance", "INB", "M", "IN"), (definition.SupplierId, definition.SupplierName, definition.ServiceSubCode, definition.ServiceTypeCode, definition.GroupCode));
                Assert.Equal((new DateOnly(2026, 10, 1), new DateOnly(2027, 9, 30)), (definition.SalesEffectiveFrom!.Value, definition.SalesDiscontinueOn!.Value));
                Assert.Equal(("None", "NoBookingProcessRequired"), (definition.DocumentType.Name, definition.BookingMethod.Name));
            },
            draft =>
            {
                Assert.Equal(("Standard", "Journey", 9m), (draft.ApplicationType.Name, draft.CoverageScope.Name, Amount(draft)));
                Assert.Equal(new[] { "ADT" }, Passengers(draft));
                Assert.Equal(new long[] { 501, 502 }, draft.Sales.PointOfSaleIds);
                Assert.Equal(new[] { "Individual" }, draft.Sales.CustomerTypes.Select(type => type.Name));
                Assert.Equal((new DateOnly(2026, 10, 1), new DateOnly(2026, 12, 31)), (draft.Travel.TravelFrom!.Value, draft.Travel.TravelTo!.Value));
                Assert.Equal(new DateTimeOffset(2027, 1, 1, 0, 0, 0, TimeSpan.Zero), draft.SalesDiscontinueAt);
            },
            edited =>
            {
                Assert.Equal(new long[] { 501 }, edited.Sales.PointOfSaleIds);
                Assert.Equal(new[] { "Individual", "Organization" }, edited.Sales.CustomerTypes.Select(type => type.Name));
                Assert.Equal((9.50m, new DateTimeOffset(2027, 2, 1, 0, 0, 0, TimeSpan.Zero)), (Amount(edited), edited.SalesDiscontinueAt!.Value));
            });

        var child = await _proof.ProvisionAsync(
            Provision(proof.ServiceDefinitionId, 20, 5m, coverageScope: ServiceCoverageScope.Journey, passenger: P1Commands.Passengers(PassengerTypeCode.CHD)),
            activate: true);
        var plus = await _proof.DefinitionAsync(
            CarrierDefinition(airlineId, insurerId, "INS_PLUS", "INP", "M", "IN", "Travel insurance plus"),
            activate: true);
        var plusPrice = await _proof.ProvisionAsync(Provision(plus.Id, 10, 25m, coverageScope: ServiceCoverageScope.Order), activate: true);

        Assert.Equal((5m, "Journey"), (Amount(child), child.CoverageScope.Name));
        Assert.NotEqual(proof.ServiceDefinitionId, plus.Id);
        Assert.Equal(("INP", insurerId), (plus.ServiceSubCode, plus.SupplierId));
        Assert.Equal(("Order", 25m, "Active"), (plusPrice.CoverageScope.Name, Amount(plusPrice), plusPrice.Status.Name));
    }

    [Fact]
    public async Task P1_FAM06_paid_seat_rules_are_authored_by_aircraft_seat_number_and_characteristic()
    {
        var airlineId = _database.NextAirlineId();
        var supplierId = await _proof.SupplierAsync(M1Commands.LocalSupplier(airlineId));

        var proof = await _proof.ProveAsync(
            CarrierDefinition(airlineId, supplierId, "SEAT_SELECT", "STS", "F", "ST", "Seat selection"),
            definitionId => Provision(
                definitionId,
                10,
                40m,
                travel: new ProvisionTravelCriteriaInput(AircraftIds: [320]),
                fare: new ProvisionFareCriteriaInput(CabinClassIds: [1]),
                application: Seat([" 1a", "1B", "1C"], null)),
            draft => draft with
            {
                Fee = draft.Fee! with { PriceLines = Price(42m, "Front row seat") },
                Application = Seat(["1A", "1B", "1C", "1D"], ["W"])
            },
            definition => Assert.Equal(("STS", "F", "ST"), (definition.ServiceSubCode, definition.ServiceTypeCode, definition.GroupCode)),
            draft =>
            {
                Assert.Equal(("Seat", 40m), (draft.ApplicationType.Name, Amount(draft)));
                Assert.Equal(new[] { "1A", "1B", "1C" }, draft.Seat!.SeatNumbers);
                Assert.Empty(draft.Seat.SeatCharacteristicCodes);
                Assert.Equal(new[] { 320 }, draft.Travel.AircraftIds);
                Assert.Equal(new[] { 1 }, draft.Fare.CabinClassIds);
                Assert.Null(draft.Baggage);
            },
            edited =>
            {
                Assert.Equal(new[] { "1A", "1B", "1C", "1D" }, edited.Seat!.SeatNumbers);
                Assert.Equal(new[] { "W" }, edited.Seat.SeatCharacteristicCodes);
                Assert.Equal(42m, Amount(edited));
            });

        var byCharacteristic = await _proof.ProvisionAsync(
            Provision(
                proof.ServiceDefinitionId,
                20,
                15m,
                passenger: P1Commands.Passengers(PassengerTypeCode.ADT),
                sales: new ProvisionSalesCriteriaInput(PointOfSaleIds: [501]),
                travel: new ProvisionTravelCriteriaInput(
                    AircraftIds: [320, 321],
                    TravelFrom: new DateOnly(2026, 12, 1),
                    TravelTo: new DateOnly(2027, 3, 31)),
                fare: new ProvisionFareCriteriaInput(FareFamilyIds: [5], CabinClassIds: [2], RbdIds: [41, 42]),
                application: Seat(null, ["W", "LS"])),
            activate: true);

        Assert.Equal(("Seat", "Active", 15m), (byCharacteristic.ApplicationType.Name, byCharacteristic.Status.Name, Amount(byCharacteristic)));
        Assert.Empty(byCharacteristic.Seat!.SeatNumbers);
        Assert.Equal(new[] { "W", "LS" }, byCharacteristic.Seat.SeatCharacteristicCodes);
        Assert.Equal(new long[] { 41, 42 }, byCharacteristic.Fare.RbdIds);
        Assert.Equal(new[] { 320, 321 }, byCharacteristic.Travel.AircraftIds);
    }

    [Fact]
    public async Task P1_FAM07_airport_lounge_is_defined_by_several_suppliers_with_their_own_prices()
    {
        var airlineId = _database.NextAirlineId();
        var airlineSupplierId = await _proof.SupplierAsync(M1Commands.LocalSupplier(airlineId, "Dot Air"));
        var partnerSupplierId = await _proof.SupplierAsync(M1Commands.ExternalSupplier(airlineId));

        var own = await _proof.ProveAsync(
            M1Commands.LoungeDefinition(airlineId, airlineSupplierId, "LNG_IKA_DOTAIR"),
            definitionId => Provision(
                definitionId,
                10,
                25m,
                passenger: P1Commands.Passengers(PassengerTypeCode.ADT),
                sales: new ProvisionSalesCriteriaInput(PointOfSaleIds: [501]),
                travel: new ProvisionTravelCriteriaInput(
                    OriginAirportIds: [Ika],
                    RoutePairs: [Pair(Ika, Ist)],
                    TravelFrom: new DateOnly(2026, 12, 1))),
            draft => draft with { Fee = draft.Fee! with { PriceLines = Price(27m, "Lounge access") } },
            definition =>
            {
                Assert.Equal(("Industry", "0BX", "F", "LG"), (definition.SubCodeSource.Name, definition.ServiceSubCode, definition.ServiceTypeCode, definition.GroupCode));
                Assert.Equal(("EmdStandalone", "E", "0BX"), (definition.DocumentType.Name, definition.DocumentRfic, definition.DocumentRfisc));
                Assert.Equal((airlineSupplierId, "Dot Air"), (definition.SupplierId, definition.SupplierName));
            },
            draft =>
            {
                Assert.Equal(new[] { Ika }, draft.Travel.OriginAirportIds);
                Assert.Equal((Ika, Ist), draft.Travel.RoutePairs.Select(pair => (pair.OriginAirportId, pair.DestinationAirportId)).Single());
                Assert.Equal(25m, Amount(draft));
            },
            edited => Assert.Equal(27m, Amount(edited)));

        var partner = await _proof.DefinitionAsync(M1Commands.LoungeDefinition(airlineId, partnerSupplierId, "LNG_IKA_PARTNER"), activate: true);
        var partnerPrice = await _proof.ProvisionAsync(
            Provision(
                partner.Id,
                10,
                45m,
                passenger: P1Commands.Passengers(PassengerTypeCode.ADT),
                sales: new ProvisionSalesCriteriaInput(PointOfSaleIds: [501]),
                travel: new ProvisionTravelCriteriaInput(OriginAirportIds: [Ika], RoutePairs: [Pair(Ika, Ist)])),
            activate: true);

        Assert.NotEqual(own.ServiceDefinitionId, partner.Id);
        Assert.Equal((partnerSupplierId, "Partner Lounge", "0BX", "Active"), (partner.SupplierId, partner.SupplierName, partner.ServiceSubCode, partner.Status.Name));
        Assert.Equal((45m, "Active"), (Amount(partnerPrice), partnerPrice.Status.Name));
        Assert.Equal(new[] { Ika }, partnerPrice.Travel.OriginAirportIds);
    }

    [Fact]
    public async Task P1_FAM08_priority_boarding_is_paid_free_or_not_available_by_fare_family_cabin_passenger_and_point_of_sale()
    {
        var airlineId = _database.NextAirlineId();
        var supplierId = await _proof.SupplierAsync(M1Commands.LocalSupplier(airlineId));

        var proof = await _proof.ProveAsync(
            CarrierDefinition(airlineId, supplierId, "PRIORITY_BOARDING", "PRB", "F", "PB", "Priority boarding"),
            definitionId => Provision(
                definitionId,
                10,
                disposition: CommercialDisposition.Free,
                fare: new ProvisionFareCriteriaInput(FareFamilyIds: [6], CabinClassIds: [1])),
            draft => draft with { Fare = new ProvisionFareCriteriaInput(FareFamilyIds: [6, 7], CabinClassIds: [1]) },
            definition => Assert.Equal(("PRB", "PB", "CarrierDefined"), (definition.ServiceSubCode, definition.GroupCode, definition.SubCodeSource.Name)),
            draft =>
            {
                Assert.Equal(("Free", 0), (draft.Disposition.Name, draft.PriceLines.Count));
                Assert.Equal(new long[] { 6 }, draft.Fare.FareFamilyIds);
                Assert.Equal(new[] { 1 }, draft.Fare.CabinClassIds);
            },
            edited => Assert.Equal(new long[] { 6, 7 }, edited.Fare.FareFamilyIds));

        var notAvailable = await _proof.ProvisionAsync(
            Provision(proof.ServiceDefinitionId, 20, disposition: CommercialDisposition.NotAvailable, passenger: P1Commands.Passengers(PassengerTypeCode.INF)),
            activate: true);
        var agencyPrice = await _proof.ProvisionAsync(
            Provision(proof.ServiceDefinitionId, 30, 7m, sales: new ProvisionSalesCriteriaInput(PointOfSaleIds: [501])),
            activate: true);
        var standard = await _proof.ProvisionAsync(Provision(proof.ServiceDefinitionId, 100, 9m), activate: true);

        Assert.Equal(("NotAvailable", "Active"), (notAvailable.Disposition.Name, notAvailable.Status.Name));
        Assert.Equal(new[] { "INF" }, Passengers(notAvailable));
        Assert.Equal((7m, 9m), (Amount(agencyPrice), Amount(standard)));
        Assert.Equal(new long[] { 501 }, agencyPrice.Sales.PointOfSaleIds);
        Assert.Equal(
            new[] { (10, "Free"), (20, "NotAvailable"), (30, "Paid"), (100, "Paid") },
            (await _proof.ListedProvisionsAsync(proof.ServiceDefinitionId)).Select(row => (row.Sequence, row.Disposition.Name)));
    }

    [Fact]
    public async Task P1_FAM09_fast_track_is_authored_by_airport_route_date_and_point_of_sale()
    {
        var airlineId = _database.NextAirlineId();
        var supplierId = await _proof.SupplierAsync(M1Commands.LocalSupplier(airlineId, "IKA Airport Services"));

        await _proof.ProveAsync(
            CarrierDefinition(airlineId, supplierId, "FASTTRACK_IKA", "FTK", "F", "FT", "Fast track security"),
            definitionId => Provision(
                definitionId,
                10,
                12m,
                sales: new ProvisionSalesCriteriaInput(PointOfSaleIds: [501, 601]),
                travel: new ProvisionTravelCriteriaInput(
                    OriginAirportIds: [Ika],
                    RoutePairs: [Pair(Ika, Ist), Pair(Ika, Mhd, RoutePairDirection.BothDirections)],
                    TravelFrom: new DateOnly(2026, 12, 20),
                    TravelTo: new DateOnly(2027, 1, 5),
                    DaysOfWeek: [DayOfWeek.Thursday, DayOfWeek.Friday],
                    TimeFrom: new TimeOnly(5, 0),
                    TimeTo: new TimeOnly(11, 0))),
            draft => draft with
            {
                Travel = draft.Travel! with { RoutePairs = [Pair(Ika, Ist)], TimeFrom = new TimeOnly(22, 0), TimeTo = new TimeOnly(2, 0) },
                Sales = new ProvisionSalesCriteriaInput(PointOfSaleIds: [501])
            },
            definition => Assert.Equal(("FTK", "FT", "IKA Airport Services"), (definition.ServiceSubCode, definition.GroupCode, definition.SupplierName)),
            draft =>
            {
                Assert.Equal(new[] { Ika }, draft.Travel.OriginAirportIds);
                Assert.Equal(
                    new[] { (Ika, Ist, "Directional"), (Ika, Mhd, "BothDirections") },
                    draft.Travel.RoutePairs.Select(pair => (pair.OriginAirportId, pair.DestinationAirportId, pair.Direction.Name)));
                Assert.Equal((new DateOnly(2026, 12, 20), new DateOnly(2027, 1, 5)), (draft.Travel.TravelFrom!.Value, draft.Travel.TravelTo!.Value));
                Assert.Equal(new[] { "Thursday", "Friday" }, draft.Travel.DaysOfWeek.Select(day => day.Name));
                Assert.Equal((new TimeOnly(5, 0), new TimeOnly(11, 0)), (draft.Travel.TimeFrom!.Value, draft.Travel.TimeTo!.Value));
                Assert.Equal(new long[] { 501, 601 }, draft.Sales.PointOfSaleIds);
            },
            edited =>
            {
                Assert.Equal((Ika, Ist), edited.Travel.RoutePairs.Select(pair => (pair.OriginAirportId, pair.DestinationAirportId)).Single());
                Assert.Equal((new TimeOnly(22, 0), new TimeOnly(2, 0)), (edited.Travel.TimeFrom!.Value, edited.Travel.TimeTo!.Value));
                Assert.Equal(new long[] { 501 }, edited.Sales.PointOfSaleIds);
                Assert.Equal(new[] { "Thursday", "Friday" }, edited.Travel.DaysOfWeek.Select(day => day.Name));
            });
    }

    [Fact]
    public async Task P1_FAM10_wifi_is_priced_by_aircraft_flight_cabin_and_fare_family()
    {
        var airlineId = _database.NextAirlineId();
        var supplierId = await _proof.SupplierAsync(M1Commands.LocalSupplier(airlineId));

        var proof = await _proof.ProveAsync(
            CarrierDefinition(airlineId, supplierId, "WIFI_FULL_FLIGHT", "WFF", "F", "WF", "Full flight Wi-Fi"),
            definitionId => Provision(
                definitionId,
                10,
                10m,
                travel: new ProvisionTravelCriteriaInput(
                    AircraftIds: [321],
                    FlightNumbers: ["w5112", "W5114"],
                    MarketingAirlineIds: [1],
                    OperatingAirlineIds: [1]),
                fare: new ProvisionFareCriteriaInput(FareFamilyIds: [5], CabinClassIds: [2])),
            draft => draft with
            {
                Travel = draft.Travel! with { AircraftIds = [321, 330] },
                Fee = draft.Fee! with { PriceLines = Price(11m, "Full flight Wi-Fi") }
            },
            definition => Assert.Equal(("WFF", "WF"), (definition.ServiceSubCode, definition.GroupCode)),
            draft =>
            {
                Assert.Equal(new[] { 321 }, draft.Travel.AircraftIds);
                Assert.Equal(new[] { "W5112", "W5114" }, draft.Travel.FlightNumbers);
                Assert.Equal(new[] { 1 }, draft.Travel.MarketingAirlineIds);
                Assert.Equal(new[] { 1 }, draft.Travel.OperatingAirlineIds);
                Assert.Equal(new long[] { 5 }, draft.Fare.FareFamilyIds);
                Assert.Equal(new[] { 2 }, draft.Fare.CabinClassIds);
                Assert.Equal(10m, Amount(draft));
            },
            edited =>
            {
                Assert.Equal(new[] { 321, 330 }, edited.Travel.AircraftIds);
                Assert.Equal(new[] { "W5112", "W5114" }, edited.Travel.FlightNumbers);
                Assert.Equal(11m, Amount(edited));
            });

        var included = await _proof.ProvisionAsync(
            Provision(
                proof.ServiceDefinitionId,
                20,
                disposition: CommercialDisposition.Free,
                travel: new ProvisionTravelCriteriaInput(AircraftIds: [321]),
                fare: new ProvisionFareCriteriaInput(FareFamilyIds: [6], CabinClassIds: [1])),
            activate: true);

        Assert.Equal(("Free", "Active"), (included.Disposition.Name, included.Status.Name));
        Assert.Equal(new[] { 1 }, included.Fare.CabinClassIds);
    }

    [Fact]
    public async Task P1_FAM11_pet_service_carries_booking_metadata_with_sector_and_journey_prices()
    {
        var airlineId = _database.NextAirlineId();
        var supplierId = await _proof.SupplierAsync(M1Commands.LocalSupplier(airlineId));

        var proof = await _proof.ProveAsync(
            CarrierDefinition(airlineId, supplierId, "PET_IN_CABIN", "PTC", "F", "PT", "Pet in cabin", Ssr("PETC")),
            definitionId => Provision(
                definitionId,
                10,
                50m,
                coverageScope: ServiceCoverageScope.Sector,
                travel: new ProvisionTravelCriteriaInput(AircraftIds: [320, 321])),
            draft => draft with { Fee = draft.Fee! with { PriceLines = Price(55m, "Pet in cabin") }, Outcome = draft.Outcome with { BookingRequired = true } },
            definition => Assert.Equal(("PTC", "PT", "Ssr", "PETC"), (definition.ServiceSubCode, definition.GroupCode, definition.BookingMethod.Name, definition.BookingSsrCode)),
            draft =>
            {
                Assert.Equal(("Sector", 50m), (draft.CoverageScope.Name, Amount(draft)));
                Assert.Equal(new[] { 320, 321 }, draft.Travel.AircraftIds);
            },
            edited =>
            {
                Assert.Equal(("Sector", 55m), (edited.CoverageScope.Name, Amount(edited)));
                Assert.True(edited.BookingRequired);
            });

        var journey = await _proof.ProvisionAsync(
            Provision(proof.ServiceDefinitionId, 20, 90m, coverageScope: ServiceCoverageScope.Journey),
            activate: true);
        var hold = await _proof.DefinitionAsync(
            CarrierDefinition(airlineId, supplierId, "PET_IN_HOLD", "PTH", "F", "PT", "Pet in hold", Ssr("AVIH")),
            activate: true);
        var holdPrice = await _proof.ProvisionAsync(Provision(hold.Id, 10, 120m, coverageScope: ServiceCoverageScope.Journey), activate: true);

        Assert.Equal(("Journey", 90m, "Active"), (journey.CoverageScope.Name, Amount(journey), journey.Status.Name));
        Assert.Equal(("AVIH", "PTH"), (hold.BookingSsrCode, hold.ServiceSubCode));
        Assert.Equal(120m, Amount(holdPrice));
    }

    [Fact]
    public async Task P1_FAM12_meet_and_assist_is_authored_by_airport_route_passenger_point_of_sale_and_date_for_two_suppliers()
    {
        var airlineId = _database.NextAirlineId();
        var firstSupplierId = await _proof.SupplierAsync(M1Commands.LocalSupplier(airlineId, "IKA CIP Services"));
        var secondSupplierId = await _proof.SupplierAsync(M1Commands.ExternalSupplier(airlineId, "MeetAssistPartner"));

        var first = await _proof.ProveAsync(
            CarrierDefinition(airlineId, firstSupplierId, "MAAS_IKA_CIP", "MAS", "F", "MA", "Meet and assist CIP", Ssr("MAAS")),
            definitionId => Provision(
                definitionId,
                10,
                60m,
                passenger: P1Commands.Passengers(PassengerTypeCode.ADT),
                sales: new ProvisionSalesCriteriaInput(PointOfSaleIds: [501], CustomerIds: [9001]),
                travel: new ProvisionTravelCriteriaInput(
                    OriginAirportIds: [Ika],
                    DestinationAirportIds: [Ist],
                    RoutePairs: [Pair(Ika, Ist)],
                    TravelFrom: new DateOnly(2026, 12, 1),
                    TravelTo: new DateOnly(2027, 3, 31))),
            draft => draft with
            {
                Passenger = P1Commands.Passengers(PassengerTypeCode.ADT, PassengerTypeCode.CHD),
                Sales = new ProvisionSalesCriteriaInput(PointOfSaleIds: [501], CustomerIds: [9001, 9002])
            },
            definition => Assert.Equal(("MAS", "MA", "MAAS", firstSupplierId), (definition.ServiceSubCode, definition.GroupCode, definition.BookingSsrCode, definition.SupplierId)),
            draft =>
            {
                Assert.Equal(new[] { "ADT" }, Passengers(draft));
                Assert.Equal(new long[] { 9001 }, draft.Sales.CustomerIds);
                Assert.Equal(new[] { Ika }, draft.Travel.OriginAirportIds);
                Assert.Equal(new[] { Ist }, draft.Travel.DestinationAirportIds);
                Assert.Equal((new DateOnly(2026, 12, 1), new DateOnly(2027, 3, 31)), (draft.Travel.TravelFrom!.Value, draft.Travel.TravelTo!.Value));
                Assert.Equal(60m, Amount(draft));
            },
            edited =>
            {
                Assert.Equal(new[] { "ADT", "CHD" }, Passengers(edited));
                Assert.Equal(new long[] { 9001, 9002 }, edited.Sales.CustomerIds);
            });

        var second = await _proof.DefinitionAsync(
            CarrierDefinition(airlineId, secondSupplierId, "MAAS_IKA_PARTNER", "MAS", "F", "MA", "Meet and assist partner", Ssr("MAAS")),
            activate: true);
        var secondPrice = await _proof.ProvisionAsync(
            Provision(second.Id, 10, 75m, travel: new ProvisionTravelCriteriaInput(OriginAirportIds: [Ika], DestinationAirportIds: [Ist])),
            activate: true);

        Assert.NotEqual(first.ServiceDefinitionId, second.Id);
        Assert.Equal((secondSupplierId, "MAS", "Active"), (second.SupplierId, second.ServiceSubCode, second.Status.Name));
        Assert.Equal(75m, Amount(secondPrice));
    }

    [Fact]
    public async Task P1_FAM13_unaccompanied_minor_handling_uses_passenger_type_and_booking_metadata_without_age_fields()
    {
        var airlineId = _database.NextAirlineId();
        var supplierId = await _proof.SupplierAsync(M1Commands.LocalSupplier(airlineId));

        await _proof.ProveAsync(
            CarrierDefinition(airlineId, supplierId, "UMNR_HANDLING", "UMN", "F", "UM", "Unaccompanied minor handling", Ssr("UMNR")),
            definitionId => Provision(
                definitionId,
                10,
                35m,
                coverageScope: ServiceCoverageScope.Journey,
                passenger: P1Commands.Passengers(PassengerTypeCode.UNN),
                sales: new ProvisionSalesCriteriaInput(CustomerTypes: [CustomerType.Individual, CustomerType.TravelAgency]),
                travel: new ProvisionTravelCriteriaInput(
                    RoutePairs: [Pair(Thr, Mhd, RoutePairDirection.BothDirections)],
                    TravelFrom: new DateOnly(2026, 12, 1),
                    OperatingAirlineIds: [1])),
            draft => draft with
            {
                Passenger = P1Commands.Passengers(PassengerTypeCode.UNN, PassengerTypeCode.CHD),
                Travel = draft.Travel! with { RoutePairs = [Pair(Thr, Mhd, RoutePairDirection.BothDirections), Pair(Thr, Ist)] }
            },
            definition => Assert.Equal(("UMN", "UM", "Ssr", "UMNR"), (definition.ServiceSubCode, definition.GroupCode, definition.BookingMethod.Name, definition.BookingSsrCode)),
            draft =>
            {
                Assert.Equal(new[] { "UNN" }, Passengers(draft));
                Assert.Equal(new[] { "Individual", "TravelAgency" }, draft.Sales.CustomerTypes.Select(type => type.Name));
                Assert.Equal((Thr, Mhd, "BothDirections"), draft.Travel.RoutePairs.Select(pair => (pair.OriginAirportId, pair.DestinationAirportId, pair.Direction.Name)).Single());
                Assert.Equal(new[] { 1 }, draft.Travel.OperatingAirlineIds);
                Assert.Equal(("Journey", 35m), (draft.CoverageScope.Name, Amount(draft)));
            },
            edited =>
            {
                Assert.Equal(new[] { "UNN", "CHD" }, Passengers(edited));
                Assert.Equal(2, edited.Travel.RoutePairs.Count);
            });

        Assert.DoesNotContain(
            typeof(BackofficeProvisionDto).GetProperties().Concat(typeof(BackofficeProvisionPassengerCriteriaDto).GetProperties()),
            property => property.Name.Contains("Age", StringComparison.Ordinal) || property.Name.Contains("Birth", StringComparison.Ordinal));
    }
}
