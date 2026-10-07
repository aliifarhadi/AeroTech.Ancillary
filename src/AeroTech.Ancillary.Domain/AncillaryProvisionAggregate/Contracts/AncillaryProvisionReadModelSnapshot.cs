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
        ProvisionCriteriaReadModelSnapshot Criteria,
        AncillaryQuantityUnit QuantityUnit,
        int MinQuantity,
        int MaxQuantity,
        ProvisionApplicationReadModelSnapshot Application,
        CommercialDisposition Disposition,
        bool DocumentRequired,
        bool BookingRequired,
        int? FeeCurrencyId,
        FeeApplicationUnit? FeeApplicationUnit,
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
        IReadOnlyList<ProvisionRoutePairReadModelSnapshot> RoutePairs,
        IReadOnlyList<ProvisionPriceLineReadModelSnapshot> PriceLines);

    public sealed record ProvisionCriteriaReadModelSnapshot(
        IReadOnlyList<PassengerTypeCode> PassengerTypeCodes,
        IReadOnlyList<long> PointOfSaleIds,
        IReadOnlyList<long> CustomerIds,
        IReadOnlyList<CustomerType> CustomerTypes,
        IReadOnlyList<int> OriginAirportIds,
        IReadOnlyList<int> DestinationAirportIds,
        IReadOnlyList<int> ViaAirportIds,
        DateOnly? TravelFrom,
        DateOnly? TravelTo,
        IReadOnlyList<DayOfWeek> DaysOfWeek,
        TimeOnly? TimeFrom,
        TimeOnly? TimeTo,
        IReadOnlyList<int> MarketingAirlineIds,
        IReadOnlyList<int> OperatingAirlineIds,
        IReadOnlyList<string> FlightNumbers,
        IReadOnlyList<long> FlightIds,
        IReadOnlyList<int> AircraftIds,
        IReadOnlyList<long> AirFareIds,
        IReadOnlyList<AirFareType> AirFareTypes,
        IReadOnlyList<long> FareFamilyIds,
        IReadOnlyList<string> FareBasisCodes,
        IReadOnlyList<int> CabinClassIds,
        IReadOnlyList<long> RbdIds,
        int? AdvancePurchasePeriod,
        TimeUnit? AdvancePurchaseUnit);

    public sealed record ProvisionApplicationReadModelSnapshot(
        ProvisionApplicationType Type,
        int? BaggageFreePieces,
        int? BaggageFirstExcessPiece,
        int? BaggageLastExcessPiece,
        decimal? BaggageWeight,
        WeightUnit? BaggageWeightUnit,
        BaggageTravelApplication? BaggageTravelApplication,
        BaggagePurchaseApplication? BaggagePurchaseApplication,
        BaggageRuleDeference? BaggageRuleDeference,
        IReadOnlyList<string> SeatNumbers,
        IReadOnlyList<string> SeatCharacteristicCodes);

    public sealed record ProvisionRoutePairReadModelSnapshot(
        long RoutePairId,
        int OriginAirportId,
        int DestinationAirportId,
        RoutePairDirection Direction);

    public sealed record ProvisionPriceLineReadModelSnapshot(
        long PriceLineId,
        AncillaryPriceLineCategory Category,
        string? Code,
        string? Name,
        int? CountryId,
        int? StationAirportId,
        decimal UnitAmount);
}
