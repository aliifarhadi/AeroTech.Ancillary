namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.OtaPanel
{
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
        bool CanPurchase,
        string SelectionKind,
        string AvailabilityState,
        string OfferReadiness,
        IReadOnlyList<string> ReasonCodes,
        IReadOnlyList<OtaPanelFeeDto> Taxes,
        IReadOnlyList<OtaPanelFeeDto> Fees);
}
