namespace AeroTech.Ancillary.Query.AncillaryOffers.Dto.OtaPanel
{
    public sealed record OtaPanelAncillaryOffersDto(
        string OfferId,
        int CurrencyId,
        string? CurrencyCode,
        IReadOnlyList<OtaPanelTravellerGroupDto> TravellerGroups,
        int BlockedCount,
        IReadOnlyList<OtaPanelRateOfExchangeDto> RatesOfExchange);

    public sealed record OtaPanelRateOfExchangeDto(string RateOfExchangeTableId, int FromCurrencyId, int ToCurrencyId, decimal Rate);

    public sealed record OtaPanelTravellerGroupDto(
        string TravellerRef,
        string PassengerTypeCode,
        IReadOnlyList<OtaPanelFlightGroupDto> FlightGroups);

    public sealed record OtaPanelFlightGroupDto(
        string FlightRef,
        string PortionRef,
        string? FlightNumber,
        int OriginAirportId,
        int DestinationAirportId,
        DateTimeOffset DepartureAt,
        OtaPanelIncludedBaggageDto? IncludedCheckedBaggage,
        OtaPanelIncludedBaggageDto? IncludedCabinBaggage,
        IReadOnlyList<OtaPanelServiceDto> Services);

    public sealed record OtaPanelIncludedBaggageDto(int? Pieces, decimal? Weight, string? WeightUnit);

    public sealed record OtaPanelServiceDto(
        string OfferItemRef,
        string Category,
        string VariantCode,
        string Title,
        string? Description,
        string CoverageScope,
        IReadOnlyList<string> CoveredFlightRefs,
        string PriceStatus,
        string? PriceUnit,
        OtaPanelMoneyDto? UnitPrice,
        string? RateOfExchangeTableId,
        IReadOnlyList<OtaPanelFeeDto> OrderLevelFees,
        string QuantityUnit,
        int MinQuantity,
        int MaxQuantity,
        IReadOnlyList<string> RequiredInputs,
        string DisplayStatus,
        bool RequiresRecheck,
        bool CanPurchase);

    public sealed record OtaPanelMoneyDto(decimal Amount, int CurrencyId);

    public sealed record OtaPanelFeeDto(string? Code, decimal Amount, int CurrencyId, string? AppliesPer);

    public sealed record OtaPanelAncillaryOfferDetailDto(
        string OfferId,
        string TravellerRef,
        OtaPanelServiceDto Service,
        OtaPanelPriceBreakdownDto PriceBreakdown,
        OtaPanelServiceSpecificationDto Specification,
        IReadOnlyList<OtaPanelIncludedAllowanceDto> IncludedAllowance,
        OtaPanelBaggageRuleDto? BaggageRule,
        OtaPanelSeatRuleDto? SeatRule,
        OtaPanelRestrictionsDto Restrictions,
        OtaPanelTermsDto Terms,
        IReadOnlyList<OtaPanelRateDto> PriceRows);

    public sealed record OtaPanelPriceBreakdownDto(
        OtaPanelMoneyDto? Base,
        IReadOnlyList<OtaPanelFeeDto> AddedTaxes,
        IReadOnlyList<OtaPanelFeeDto> IncludedTaxes,
        IReadOnlyList<OtaPanelFeeDto> UnitFees,
        IReadOnlyList<OtaPanelFeeDto> OrderLevelFees,
        OtaPanelMoneyDto? UnitPrice);

    public sealed record OtaPanelSelectionResultDto(
        string OfferId,
        string Status,
        string OfferItemRef,
        string CurrentOfferItemRef,
        bool ServiceChanged,
        OtaPanelServiceDto Service,
        OtaPanelMoneyDto? SelectedAmount,
        IReadOnlyList<OtaPanelInputIssueDto> InputIssues,
        bool CanPurchase);

    public sealed record OtaPanelInputIssueDto(string Field, string Reason);
}
