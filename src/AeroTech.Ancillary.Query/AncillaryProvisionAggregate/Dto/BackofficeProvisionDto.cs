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
        int? FeeCurrencyId,
        EnumValueDto? FeeApplicationUnit,
        EnumValueDto ReissueRefund,
        EnumValueDto? FormOfRefund,
        bool Commissionable,
        bool InterlineSettlement,
        bool MustCheckAvailability,
        string FulfillmentProviderKey,
        DateTimeOffset CreatedAt,
        DateTimeOffset? ActivatedAt,
        DateTimeOffset? SuspendedAt,
        DateTimeOffset? RetiredAt,
        IReadOnlyList<BackofficeProvisionPriceLineDto> PriceLines);

    public sealed record BackofficeProvisionPassengerCriteriaDto(IReadOnlyList<EnumValueDto> PassengerTypeCodes);

    public sealed record BackofficeProvisionSalesCriteriaDto(
        IReadOnlyList<long> PointOfSaleIds,
        IReadOnlyList<long> CustomerIds,
        IReadOnlyList<EnumValueDto> CustomerTypes);

    public sealed record BackofficeProvisionTravelCriteriaDto(
        IReadOnlyList<int> OriginAirportIds,
        IReadOnlyList<int> DestinationAirportIds,
        IReadOnlyList<int> ViaAirportIds,
        IReadOnlyList<BackofficeProvisionRoutePairDto> RoutePairs,
        DateOnly? TravelFrom,
        DateOnly? TravelTo,
        IReadOnlyList<EnumValueDto> DaysOfWeek,
        TimeOnly? TimeFrom,
        TimeOnly? TimeTo,
        IReadOnlyList<int> MarketingAirlineIds,
        IReadOnlyList<int> OperatingAirlineIds,
        IReadOnlyList<string> FlightNumbers,
        IReadOnlyList<long> FlightIds,
        IReadOnlyList<int> AircraftIds);

    public sealed record BackofficeProvisionRoutePairDto(
        long Id,
        int OriginAirportId,
        int DestinationAirportId,
        EnumValueDto Direction);

    public sealed record BackofficeProvisionFareCriteriaDto(
        IReadOnlyList<long> AirFareIds,
        IReadOnlyList<EnumValueDto> AirFareTypes,
        IReadOnlyList<long> FareFamilyIds,
        IReadOnlyList<string> FareBasisCodes,
        IReadOnlyList<int> CabinClassIds,
        IReadOnlyList<long> RbdIds);

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

    public sealed record BackofficeProvisionSeatApplicationDto(
        IReadOnlyList<string> SeatNumbers,
        IReadOnlyList<string> SeatCharacteristicCodes);

    public sealed record BackofficeProvisionPriceLineDto(
        long Id,
        EnumValueDto Category,
        string? Code,
        string? Name,
        int? CountryId,
        int? StationAirportId,
        decimal UnitAmount);
}
