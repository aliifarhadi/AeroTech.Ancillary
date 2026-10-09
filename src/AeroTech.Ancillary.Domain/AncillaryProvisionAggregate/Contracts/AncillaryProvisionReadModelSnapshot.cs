using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;
using AeroTech.Messages.Core.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Contracts
{
    public sealed record AncillaryProvisionReadModelSnapshot(
        long ProvisionId,
        long ServiceDefinitionId,
        int Sequence,
        ProvisionStatus Status,
        ServiceCoverageScope CoverageScope,
        PurchaseStage PurchaseStage,
        AncillaryQuantityUnit QuantityUnit,
        int MinQuantity,
        int MaxQuantity,
        ProvisionApplicationType ApplicationType,
        CommercialDisposition Disposition,
        bool DocumentRequired,
        bool BookingRequired,
        ReissueRefundPolicy ReissueRefund,
        FormOfRefund? FormOfRefund,
        bool Commissionable,
        bool InterlineSettlement,
        bool MustCheckAvailability,
        string FulfillmentProviderKey,
        DateTimeOffset CreatedAt,
        DateTimeOffset? ActivatedAt,
        DateTimeOffset? SuspendedAt,
        DateTimeOffset? RetiredAt,
        ProvisionPassengerEligibilityReadModelSnapshot? PassengerEligibility,
        ProvisionSalesRestrictionsReadModelSnapshot? SalesRestrictions,
        ProvisionGeographyReadModelSnapshot? Geography,
        ProvisionFlightApplicationReadModelSnapshot? FlightApplication,
        ProvisionFareApplicationReadModelSnapshot? FareApplication,
        ProvisionTravelDateReadModelSnapshot? TravelDate,
        ProvisionDayTimeApplicationReadModelSnapshot? DayTimeApplication,
        ProvisionAdvancePurchaseReadModelSnapshot? AdvancePurchase,
        ProvisionBaggageApplicationReadModelSnapshot? BaggageApplication,
        ProvisionSeatApplicationReadModelSnapshot? SeatApplication);

    public sealed record ProvisionRuleRowReadModelSnapshot<TValue>(
        long RowId,
        TValue Value);

    public sealed record ProvisionPassengerEligibilityReadModelSnapshot(
        long RuleId,
        IReadOnlyList<ProvisionRuleRowReadModelSnapshot<PassengerTypeCode>> PassengerTypes,
        IReadOnlyList<ProvisionAgeBandReadModelSnapshot> AgeBands);

    public sealed record ProvisionSalesRestrictionsReadModelSnapshot(
        long RuleId,
        DateTimeOffset? SalesEffectiveFrom,
        DateTimeOffset? SalesDiscontinueAt,
        IReadOnlyList<ProvisionRuleRowReadModelSnapshot<long>> PointsOfSale,
        IReadOnlyList<ProvisionRuleRowReadModelSnapshot<long>> Customers,
        IReadOnlyList<ProvisionRuleRowReadModelSnapshot<CustomerType>> CustomerTypes);

    public sealed record ProvisionGeographyReadModelSnapshot(
        long RuleId,
        IReadOnlyList<ProvisionRuleRowReadModelSnapshot<int>> OriginAirports,
        IReadOnlyList<ProvisionRuleRowReadModelSnapshot<int>> DestinationAirports,
        IReadOnlyList<ProvisionRuleRowReadModelSnapshot<int>> ViaAirports,
        IReadOnlyList<ProvisionRuleRowReadModelSnapshot<int>> CoverageCountries,
        IReadOnlyList<ProvisionRoutePairReadModelSnapshot> RoutePairs,
        IReadOnlyList<ProvisionServiceLocationReadModelSnapshot> ServiceLocations);

    public sealed record ProvisionFlightApplicationReadModelSnapshot(
        long RuleId,
        IReadOnlyList<ProvisionRuleRowReadModelSnapshot<int>> MarketingAirlines,
        IReadOnlyList<ProvisionRuleRowReadModelSnapshot<int>> OperatingAirlines,
        IReadOnlyList<ProvisionRuleRowReadModelSnapshot<string>> FlightNumbers,
        IReadOnlyList<ProvisionRuleRowReadModelSnapshot<long>> Flights,
        IReadOnlyList<ProvisionRuleRowReadModelSnapshot<int>> Aircraft);

    public sealed record ProvisionFareApplicationReadModelSnapshot(
        long RuleId,
        IReadOnlyList<ProvisionRuleRowReadModelSnapshot<long>> AirFares,
        IReadOnlyList<ProvisionRuleRowReadModelSnapshot<AirFareType>> AirFareTypes,
        IReadOnlyList<ProvisionRuleRowReadModelSnapshot<long>> FareFamilies,
        IReadOnlyList<ProvisionRuleRowReadModelSnapshot<string>> FareBases,
        IReadOnlyList<ProvisionRuleRowReadModelSnapshot<int>> CabinClasses,
        IReadOnlyList<ProvisionRuleRowReadModelSnapshot<long>> Rbds);

    public sealed record ProvisionTravelDateReadModelSnapshot(
        long RuleId,
        IReadOnlyList<ProvisionDatePeriodReadModelSnapshot> PermittedPeriods,
        IReadOnlyList<ProvisionDatePeriodReadModelSnapshot> BlackoutPeriods);

    public sealed record ProvisionDayTimeApplicationReadModelSnapshot(
        long RuleId,
        IReadOnlyList<ProvisionDayTimeWindowReadModelSnapshot> Windows);

    public sealed record ProvisionAdvancePurchaseReadModelSnapshot(
        long RuleId,
        int MinimumPeriod,
        TimeUnit Unit,
        bool SameTimeAsTicketed,
        int? MaximumPeriod);

    public sealed record ProvisionBaggageApplicationReadModelSnapshot(
        long RuleId,
        int? FreePieces,
        int? FirstExcessPiece,
        int? LastExcessPiece,
        decimal? Weight,
        WeightUnit WeightUnit,
        BaggageTravelApplication? TravelApplication,
        BaggagePurchaseApplication PurchaseApplication,
        BaggageRuleDeference? RuleDeference,
        BaggageChargeKind? ChargeKind,
        BaggageAllowanceConcept? AllowanceConcept);

    public sealed record ProvisionSeatApplicationReadModelSnapshot(
        long RuleId,
        IReadOnlyList<ProvisionRuleRowReadModelSnapshot<string>> SeatNumbers,
        IReadOnlyList<ProvisionRuleRowReadModelSnapshot<string>> SeatCharacteristics);

    public sealed record ProvisionAgeBandReadModelSnapshot(
        long RowId,
        int AgeFromInclusive,
        int? AgeToExclusive);

    public sealed record ProvisionRoutePairReadModelSnapshot(
        long RowId,
        int OriginAirportId,
        int DestinationAirportId,
        RoutePairDirection Direction);

    public sealed record ProvisionServiceLocationReadModelSnapshot(
        long RowId,
        ServiceLocationType LocationType,
        int LocationId);

    public sealed record ProvisionDatePeriodReadModelSnapshot(
        long RowId,
        DateOnly StartDate,
        DateOnly EndDate);

    public sealed record ProvisionDayTimeWindowReadModelSnapshot(
        long RowId,
        byte DaysOfWeekMask,
        TimeOnly? StartLocalTime,
        TimeOnly? EndLocalTime,
        DayTimeRestrictionEffect Effect);
}
