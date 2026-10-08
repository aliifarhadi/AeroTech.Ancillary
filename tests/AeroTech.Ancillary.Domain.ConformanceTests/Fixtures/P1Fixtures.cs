using AeroTech.Ancillary.Domain.AncillaryPricingAggregate;
using AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Arguments;
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
        int version = 1,
        PricingUnit pricingUnit = PricingUnit.PerPassenger)
        => AncillaryServiceDefinition.Define(
            id,
            Airline,
            supplierId,
            reference,
            version,
            "MVG",
            ServiceSubCodeSource.CarrierDefined,
            new ServiceDefinitionClassificationArgs("F", "ML", "VG", null, null),
            pricingUnit,
            "Vegetarian meal",
            "Pre-ordered vegetarian meal",
            DocumentDefinition.Create(AncillaryDocumentType.EmdAssociated, "G", "MVG"),
            BookingDefinition.Create(BookingMethod.Ssr, "VGML", null),
            new DateOnly(2026, 10, 1),
            new DateOnly(2027, 3, 31),
            Now);

    public static AncillaryProvision Provision(
        ProvisionConditionsArgs? conditions = null,
        ProvisionApplicationType applicationType = ProvisionApplicationType.Standard,
        CommercialDisposition disposition = CommercialDisposition.Paid,
        long id = 5001,
        int sequence = 10,
        SequentialIdGenerator? ids = null)
        => AncillaryProvision.Define(
            id,
            1001,
            sequence,
            null,
            null,
            ServiceCoverageScope.Sector,
            null,
            QuantityRule.Create(AncillaryQuantityUnit.Each, 1, 1),
            ProvisionApplication.Create(applicationType, applicationType == ProvisionApplicationType.Baggage ? Baggage() : null),
            CommercialOutcome.Create(disposition, disposition == CommercialDisposition.Paid, false),
            SettlementDefinition.Create(ReissueRefundPolicy.NonRefundable, null, false, false),
            AvailabilityDefinition.Create(false),
            FulfillmentDefinition.Create("Ancillary"),
            conditions ?? ProvisionConditionsArgs.Unrestricted,
            ids ?? new SequentialIdGenerator(),
            Now);

    public static void Replace(AncillaryProvision provision, ProvisionConditionsArgs conditions, SequentialIdGenerator ids)
        => provision.Change(
            provision.Sequence,
            provision.SalesEffectiveFrom,
            provision.SalesDiscontinueAt,
            provision.CoverageScope,
            provision.AdvancePurchase,
            provision.Quantity,
            provision.Application,
            provision.Outcome,
            provision.Settlement,
            provision.Availability,
            provision.Fulfillment,
            conditions,
            ids);

    public static BaggageApplication Baggage()
        => BaggageApplication.Create(
            null,
            1,
            1,
            23m,
            WeightUnit.Kg,
            BaggageTravelApplication.AllSectors,
            BaggagePurchaseApplication.Prepaid,
            null);

    public static ProvisionRoutePairArgs Pair(int origin, int destination, RoutePairDirection direction = RoutePairDirection.Directional)
        => new(origin, destination, direction);

    public static AncillaryPricing Pricing(
        PricingUnit pricingUnit,
        IReadOnlyList<AncillaryPricingLineArgs> priceLines,
        FeeApplicationUnit? feeApplicationUnit = FeeApplicationUnit.Item,
        long id = 7001,
        int version = 1,
        SequentialIdGenerator? ids = null)
        => AncillaryPricing.Define(id, 5001, pricingUnit, version, Eur, feeApplicationUnit, priceLines, ids ?? new SequentialIdGenerator(), Now);

    public static AncillaryPricingLineArgs Base(
        decimal amount,
        PassengerTypeCode? passengerTypeCode = null,
        int? ageFromInclusive = null,
        int? ageToExclusive = null)
        => new(passengerTypeCode, ageFromInclusive, ageToExclusive, AncillaryPriceLineCategory.Ancillary, null, "Service", null, null, amount);

    public static AncillaryPricingLineArgs Component(
        AncillaryPriceLineCategory category,
        string? code,
        decimal amount,
        PassengerTypeCode? passengerTypeCode = null,
        int? ageFromInclusive = null,
        int? ageToExclusive = null,
        int? countryId = null,
        int? stationAirportId = null)
        => new(passengerTypeCode, ageFromInclusive, ageToExclusive, category, code, null, countryId, stationAirportId, amount);

    public static string[] PropertiesOf<T>()
        => typeof(T).GetProperties()
            .Where(property => property.DeclaringType == typeof(T))
            .Select(property => property.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
}
