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
        DateTimeOffset? SalesEffectiveFrom,
        DateTimeOffset? SalesDiscontinueAt,
        ServiceCoverageScope CoverageScope,
        int? AdvancePurchasePeriod,
        TimeUnit? AdvancePurchaseUnit,
        AncillaryQuantityUnit QuantityUnit,
        int MinQuantity,
        int MaxQuantity,
        ProvisionApplicationType ApplicationType,
        int? BaggageFreePieces,
        int? BaggageFirstExcessPiece,
        int? BaggageLastExcessPiece,
        decimal? BaggageWeight,
        WeightUnit? BaggageWeightUnit,
        BaggageTravelApplication? BaggageTravelApplication,
        BaggagePurchaseApplication? BaggagePurchaseApplication,
        BaggageRuleDeference? BaggageRuleDeference,
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
        ProvisionConditionsReadModelSnapshot Conditions);

    public sealed record ProvisionConditionsReadModelSnapshot(
        IReadOnlyList<ProvisionConditionRowReadModelSnapshot<PassengerTypeCode>> PassengerTypes,
        IReadOnlyList<ProvisionConditionRowReadModelSnapshot<long>> PointsOfSale,
        IReadOnlyList<ProvisionConditionRowReadModelSnapshot<long>> Customers,
        IReadOnlyList<ProvisionConditionRowReadModelSnapshot<CustomerType>> CustomerTypes,
        IReadOnlyList<ProvisionConditionRowReadModelSnapshot<int>> OriginAirports,
        IReadOnlyList<ProvisionConditionRowReadModelSnapshot<int>> DestinationAirports,
        IReadOnlyList<ProvisionConditionRowReadModelSnapshot<int>> ViaAirports,
        IReadOnlyList<ProvisionRoutePairReadModelSnapshot> RoutePairs,
        IReadOnlyList<ProvisionConditionRowReadModelSnapshot<int>> MarketingAirlines,
        IReadOnlyList<ProvisionConditionRowReadModelSnapshot<int>> OperatingAirlines,
        IReadOnlyList<ProvisionConditionRowReadModelSnapshot<string>> FlightNumbers,
        IReadOnlyList<ProvisionConditionRowReadModelSnapshot<long>> Flights,
        IReadOnlyList<ProvisionConditionRowReadModelSnapshot<int>> Aircraft,
        IReadOnlyList<ProvisionConditionRowReadModelSnapshot<long>> AirFares,
        IReadOnlyList<ProvisionConditionRowReadModelSnapshot<AirFareType>> AirFareTypes,
        IReadOnlyList<ProvisionConditionRowReadModelSnapshot<long>> FareFamilies,
        IReadOnlyList<ProvisionConditionRowReadModelSnapshot<string>> FareBases,
        IReadOnlyList<ProvisionConditionRowReadModelSnapshot<int>> CabinClasses,
        IReadOnlyList<ProvisionConditionRowReadModelSnapshot<long>> Rbds,
        IReadOnlyList<ProvisionConditionRowReadModelSnapshot<DateOnly>> TravelDates,
        IReadOnlyList<ProvisionDatePeriodReadModelSnapshot> SeasonalPeriods,
        IReadOnlyList<ProvisionDatePeriodReadModelSnapshot> BlackoutPeriods,
        IReadOnlyList<ProvisionDayTimeRestrictionReadModelSnapshot> DayTimeRestrictions,
        IReadOnlyList<ProvisionConditionRowReadModelSnapshot<string>> SeatNumbers,
        IReadOnlyList<ProvisionConditionRowReadModelSnapshot<string>> SeatCharacteristics);

    public sealed record ProvisionConditionRowReadModelSnapshot<TValue>(
        long RowId,
        TValue Value);

    public sealed record ProvisionRoutePairReadModelSnapshot(
        long RowId,
        int OriginAirportId,
        int DestinationAirportId,
        RoutePairDirection Direction);

    public sealed record ProvisionDatePeriodReadModelSnapshot(
        long RowId,
        DateOnly StartDate,
        DateOnly EndDate);

    public sealed record ProvisionDayTimeRestrictionReadModelSnapshot(
        long RowId,
        DayOfWeek DayOfWeek,
        TimeOnly? StartTime,
        TimeOnly? EndTime,
        DayTimeRestrictionEffect Effect);
}
