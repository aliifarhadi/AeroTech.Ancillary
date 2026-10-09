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
        PricingUnit pricingUnit = PricingUnit.PerPassenger,
        ServiceDateBasis serviceDateBasis = ServiceDateBasis.FlightDeparture)
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
            serviceDateBasis,
            "Vegetarian meal",
            "Pre-ordered vegetarian meal",
            DocumentDefinition.Create(AncillaryDocumentType.EmdAssociated, "G", "MVG"),
            BookingDefinition.Create(BookingMethod.Ssr, "VGML", null),
            new DateOnly(2026, 10, 1),
            new DateOnly(2027, 3, 31),
            Now);

    public static AncillaryProvision Provision(
        ProvisionRulesArgs? rules = null,
        ProvisionApplicationType applicationType = ProvisionApplicationType.Standard,
        CommercialDisposition disposition = CommercialDisposition.Paid,
        long id = 5001,
        int sequence = 10,
        SequentialIdGenerator? ids = null,
        AncillaryQuantityUnit quantityUnit = AncillaryQuantityUnit.Each)
        => AncillaryProvision.Define(
            id,
            1001,
            sequence,
            ServiceCoverageScope.Sector,
            PurchaseStage.Both,
            QuantityRule.Create(quantityUnit, 1, 1),
            applicationType,
            CommercialOutcome.Create(disposition, disposition == CommercialDisposition.Paid, false),
            SettlementDefinition.Create(ReissueRefundPolicy.NonRefundable, null, false, false),
            AvailabilityDefinition.Create(false),
            FulfillmentDefinition.Create("Ancillary"),
            Rules(rules, applicationType),
            ids ?? new SequentialIdGenerator(),
            Now);

    public static void Replace(AncillaryProvision provision, ProvisionRulesArgs rules, SequentialIdGenerator ids)
        => provision.Change(
            provision.Sequence,
            provision.CoverageScope,
            provision.PurchaseStage,
            provision.Quantity,
            provision.ApplicationType,
            provision.Outcome,
            provision.Settlement,
            provision.Availability,
            provision.Fulfillment,
            Rules(rules, provision.ApplicationType),
            ids);

    public static ProvisionRulesArgs Rules(ProvisionRulesArgs? rules, ProvisionApplicationType applicationType)
    {
        var requested = rules ?? ProvisionRulesArgs.Unrestricted;

        return applicationType == ProvisionApplicationType.Baggage && requested.BaggageApplication is null
            ? requested with { BaggageApplication = Baggage() }
            : requested;
    }

    public static ProvisionBaggageApplicationArgs Baggage()
        => new(
            null,
            1,
            1,
            23m,
            WeightUnit.Kg,
            BaggageTravelApplication.AllSectors,
            BaggagePurchaseApplication.Prepaid,
            null,
            BaggageChargeKind.ExtraPiece,
            BaggageAllowanceConcept.Piece);

    public static ProvisionDatePeriodArgs Period(DateOnly startDate, DateOnly endDate) => new(startDate, endDate);

    public static ProvisionDayTimeWindowArgs Window(
        byte daysOfWeekMask,
        int? fromHour = null,
        int? toHour = null,
        DayTimeRestrictionEffect effect = DayTimeRestrictionEffect.Allow)
        => new(
            daysOfWeekMask,
            fromHour is null ? null : new TimeOnly(fromHour.Value, 0),
            toHour is null ? null : new TimeOnly(toHour.Value, 0),
            effect);

    public static ProvisionRoutePairArgs Pair(int origin, int destination, RoutePairDirection direction = RoutePairDirection.Directional)
        => new(origin, destination, direction);

    public static AncillaryPricing Pricing(
        PricingUnit pricingUnit,
        IReadOnlyList<FiledLine> priceLines,
        FeeApplicationUnit? feeApplicationUnit = FeeApplicationUnit.Item,
        long id = 7001,
        int version = 1,
        SequentialIdGenerator? ids = null)
        => AncillaryPricing.Define(id, 5001, pricingUnit, version, FiledPrice.ToRates(priceLines, Eur, feeApplicationUnit), FiledPrice.Scales, ids ?? new SequentialIdGenerator(), Now);

    public static FiledLine Base(
        decimal amount,
        PassengerTypeCode? passengerTypeCode = null,
        int? ageFromInclusive = null,
        int? ageToExclusive = null)
        => new(passengerTypeCode, ageFromInclusive, ageToExclusive, AncillaryPriceLineCategory.Ancillary, null, "Service", null, null, amount);

    public static FiledLine Component(
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
