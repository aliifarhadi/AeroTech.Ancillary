using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.ValueObjects;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.ValueObjects;
using AeroTech.Ancillary.Domain.ConformanceTests.Fakes;
using AeroTech.Ancillary.Domain.SupplierAggregate;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;
using AeroTech.Messages.Core.Enums;

namespace AeroTech.Ancillary.Domain.ConformanceTests.Fixtures;

public static class P1Fixtures
{
    public const int Airline = 1;
    public const int Eur = 47;
    public const int Thr = 1;
    public const int Mhd = 3;
    public const int Ist = 6;

    public static readonly DateTimeOffset Now = new(2026, 10, 1, 10, 0, 0, TimeSpan.Zero);

    public static Supplier Supplier(long id = 2001, string name = "Dot Air")
        => SupplierAggregate.Supplier.Register(id, Airline, name, SupplierFulfillmentKind.Local, null, Now);

    public static AncillaryServiceDefinition CarrierDefinition(
        long id = 1001,
        long supplierId = 2001,
        string reference = "MEAL_VGML",
        int version = 1)
        => AncillaryServiceDefinition.Define(
            id,
            Airline,
            supplierId,
            reference,
            version,
            "MVG",
            ServiceSubCodeSource.CarrierDefined,
            new ServiceDefinitionClassificationArgs("F", "ML", "VG", null, null),
            "Vegetarian meal",
            "Pre-ordered vegetarian meal",
            DocumentDefinition.Create(AncillaryDocumentType.EmdAssociated, "G", "MVG"),
            BookingDefinition.Create(BookingMethod.Ssr, "VGML", null),
            new DateOnly(2026, 10, 1),
            new DateOnly(2027, 3, 31),
            Now);

    public static AncillaryProvision Provision(
        long id = 501,
        int sequence = 10,
        decimal amount = 25m,
        CommercialDisposition disposition = CommercialDisposition.Paid,
        ServiceCoverageScope coverageScope = ServiceCoverageScope.Sector,
        PassengerCriteria? passenger = null,
        SalesCriteria? sales = null,
        TravelCriteria? travel = null,
        IReadOnlyList<ProvisionRoutePairArgs>? routePairs = null,
        FareCriteria? fare = null,
        AdvancePurchaseCriteria? advancePurchase = null,
        ProvisionApplication? application = null,
        FeeApplicationUnit feeApplicationUnit = FeeApplicationUnit.Item,
        IReadOnlyList<ProvisionPriceLineArgs>? priceLines = null)
    {
        var paid = disposition == CommercialDisposition.Paid;

        return AncillaryProvision.Define(
            id,
            1001,
            sequence,
            null,
            null,
            coverageScope,
            passenger ?? Passengers(),
            sales ?? Sales(),
            travel ?? Travel(),
            routePairs ?? [],
            fare ?? Fare(),
            advancePurchase,
            QuantityRule.Create(AncillaryQuantityUnit.Each, 1, 9),
            application ?? Standard(),
            CommercialOutcome.Create(disposition, paid, false),
            paid ? FeeDefinition.Create(Eur, feeApplicationUnit) : null,
            paid ? priceLines ?? [new ProvisionPriceLineArgs(AncillaryPriceLineCategory.Ancillary, null, "Service", amount)] : [],
            SettlementDefinition.Create(ReissueRefundPolicy.NonRefundable, null, false, false),
            AvailabilityDefinition.Create(false),
            FulfillmentDefinition.Create("Ancillary"),
            new SequentialIdGenerator(),
            Now);
    }

    public static PassengerCriteria Passengers(params PassengerTypeCode[] passengerTypeCodes)
        => PassengerCriteria.Create(passengerTypeCodes);

    public static SalesCriteria Sales(
        long[]? pointOfSaleIds = null,
        long[]? customerIds = null,
        CustomerType[]? customerTypes = null)
        => SalesCriteria.Create(pointOfSaleIds, customerIds, customerTypes);

    public static TravelCriteria Travel(
        int[]? originAirportIds = null,
        int[]? destinationAirportIds = null,
        int[]? viaAirportIds = null,
        DateOnly? travelFrom = null,
        DateOnly? travelTo = null,
        DayOfWeek[]? daysOfWeek = null,
        TimeOnly? timeFrom = null,
        TimeOnly? timeTo = null,
        int[]? marketingAirlineIds = null,
        int[]? operatingAirlineIds = null,
        string[]? flightNumbers = null,
        long[]? flightIds = null,
        int[]? aircraftIds = null)
        => TravelCriteria.Create(
            originAirportIds,
            destinationAirportIds,
            viaAirportIds,
            travelFrom,
            travelTo,
            daysOfWeek,
            timeFrom,
            timeTo,
            marketingAirlineIds,
            operatingAirlineIds,
            flightNumbers,
            flightIds,
            aircraftIds);

    public static FareCriteria Fare(
        long[]? airFareIds = null,
        AirFareType[]? airFareTypes = null,
        long[]? fareFamilyIds = null,
        string[]? fareBasisCodes = null,
        int[]? cabinClassIds = null,
        long[]? rbdIds = null)
        => FareCriteria.Create(airFareIds, airFareTypes, fareFamilyIds, fareBasisCodes, cabinClassIds, rbdIds);

    public static ProvisionRoutePairArgs Pair(
        int originAirportId,
        int destinationAirportId,
        RoutePairDirection direction = RoutePairDirection.Directional)
        => new(originAirportId, destinationAirportId, direction);

    public static ProvisionApplication Standard()
        => ProvisionApplication.Create(ProvisionApplicationType.Standard, null, null);

    public static ProvisionApplication Baggage()
        => ProvisionApplication.Create(
            ProvisionApplicationType.Baggage,
            BaggageApplication.Create(
                null,
                1,
                1,
                23m,
                WeightUnit.Kg,
                BaggageTravelApplication.AllSectors,
                BaggagePurchaseApplication.Prepaid,
                null),
            null);

    public static ProvisionApplication Seat(string[]? seatNumbers, string[]? seatCharacteristicCodes)
        => ProvisionApplication.Create(
            ProvisionApplicationType.Seat,
            null,
            SeatApplication.Create(seatNumbers, seatCharacteristicCodes));

    public static string[] PropertiesOf<T>()
        => typeof(T).GetProperties()
            .Where(property => property.DeclaringType == typeof(T))
            .Select(property => property.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
}
