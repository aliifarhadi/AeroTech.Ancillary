namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.OtaPanel
{
    public sealed record OtaPanelPriceBreakdownDto(
        OtaPanelMoneyDto? Base,
        IReadOnlyList<OtaPanelFeeDto> AddedTaxes,
        IReadOnlyList<OtaPanelFeeDto> IncludedTaxes,
        IReadOnlyList<OtaPanelFeeDto> UnitFees,
        IReadOnlyList<OtaPanelFeeDto> OrderLevelFees,
        OtaPanelMoneyDto? UnitPrice);
}
