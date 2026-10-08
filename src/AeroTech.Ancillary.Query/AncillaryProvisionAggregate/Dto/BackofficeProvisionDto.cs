using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto
{
    public sealed record BackofficeProvisionDto(
        long Id,
        long ServiceDefinitionId,
        int Sequence,
        EnumValueDto Status,
        DateTimeOffset? SalesEffectiveFrom,
        DateTimeOffset? SalesDiscontinueAt,
        EnumValueDto CoverageScope,
        BackofficeProvisionPassengerCriteriaDto Passenger,
        BackofficeProvisionSalesCriteriaDto Sales,
        BackofficeProvisionTravelCriteriaDto Travel,
        BackofficeProvisionFareCriteriaDto Fare,
        BackofficeProvisionAdvancePurchaseDto? AdvancePurchase,
        EnumValueDto QuantityUnit,
        int MinQuantity,
        int MaxQuantity,
        EnumValueDto ApplicationType,
        BackofficeProvisionBaggageApplicationDto? Baggage,
        BackofficeProvisionSeatApplicationDto? Seat,
        EnumValueDto Disposition,
        bool DocumentRequired,
        bool BookingRequired,
        EnumValueDto ReissueRefund,
        EnumValueDto? FormOfRefund,
        bool Commissionable,
        bool InterlineSettlement,
        bool MustCheckAvailability,
        string FulfillmentProviderKey,
        DateTimeOffset CreatedAt,
        DateTimeOffset? ActivatedAt,
        DateTimeOffset? SuspendedAt,
        DateTimeOffset? RetiredAt);

    public sealed record BackofficeProvisionConditionRowDto<TValue>(
        long Id,
        TValue Value);

    public sealed record BackofficeProvisionPassengerCriteriaDto(
        IReadOnlyList<BackofficeProvisionConditionRowDto<EnumValueDto>> PassengerTypes);

    public sealed record BackofficeProvisionSalesCriteriaDto(
        IReadOnlyList<BackofficeProvisionConditionRowDto<long>> PointsOfSale,
        IReadOnlyList<BackofficeProvisionConditionRowDto<long>> Customers,
        IReadOnlyList<BackofficeProvisionConditionRowDto<EnumValueDto>> CustomerTypes);

    public sealed record BackofficeProvisionTravelCriteriaDto(
        IReadOnlyList<BackofficeProvisionConditionRowDto<int>> OriginAirports,
        IReadOnlyList<BackofficeProvisionConditionRowDto<int>> DestinationAirports,
        IReadOnlyList<BackofficeProvisionConditionRowDto<int>> ViaAirports,
        IReadOnlyList<BackofficeProvisionRoutePairDto> RoutePairs,
        IReadOnlyList<BackofficeProvisionConditionRowDto<DateOnly>> TravelDates,
        IReadOnlyList<BackofficeProvisionDatePeriodDto> SeasonalPeriods,
        IReadOnlyList<BackofficeProvisionDatePeriodDto> BlackoutPeriods,
        IReadOnlyList<BackofficeProvisionDayTimeRestrictionDto> DayTimeRestrictions,
        IReadOnlyList<BackofficeProvisionConditionRowDto<int>> MarketingAirlines,
        IReadOnlyList<BackofficeProvisionConditionRowDto<int>> OperatingAirlines,
        IReadOnlyList<BackofficeProvisionConditionRowDto<string>> FlightNumbers,
        IReadOnlyList<BackofficeProvisionConditionRowDto<long>> Flights,
        IReadOnlyList<BackofficeProvisionConditionRowDto<int>> Aircraft);

    public sealed record BackofficeProvisionFareCriteriaDto(
        IReadOnlyList<BackofficeProvisionConditionRowDto<long>> AirFares,
        IReadOnlyList<BackofficeProvisionConditionRowDto<EnumValueDto>> AirFareTypes,
        IReadOnlyList<BackofficeProvisionConditionRowDto<long>> FareFamilies,
        IReadOnlyList<BackofficeProvisionConditionRowDto<string>> FareBases,
        IReadOnlyList<BackofficeProvisionConditionRowDto<int>> CabinClasses,
        IReadOnlyList<BackofficeProvisionConditionRowDto<long>> Rbds);

    public sealed record BackofficeProvisionSeatApplicationDto(
        IReadOnlyList<BackofficeProvisionConditionRowDto<string>> SeatNumbers,
        IReadOnlyList<BackofficeProvisionConditionRowDto<string>> SeatCharacteristics);

    public sealed record BackofficeProvisionRoutePairDto(
        long Id,
        int OriginAirportId,
        int DestinationAirportId,
        EnumValueDto Direction);

    public sealed record BackofficeProvisionDatePeriodDto(
        long Id,
        DateOnly StartDate,
        DateOnly EndDate);

    public sealed record BackofficeProvisionDayTimeRestrictionDto(
        long Id,
        EnumValueDto DayOfWeek,
        TimeOnly? StartTime,
        TimeOnly? EndTime,
        EnumValueDto Effect);

    public sealed record BackofficeProvisionAdvancePurchaseDto(
        int Period,
        EnumValueDto Unit);

    public sealed record BackofficeProvisionBaggageApplicationDto(
        int? FreePieces,
        int? FirstExcessPiece,
        int? LastExcessPiece,
        decimal? Weight,
        EnumValueDto WeightUnit,
        EnumValueDto? TravelApplication,
        EnumValueDto PurchaseApplication,
        EnumValueDto? RuleDeference);
}
