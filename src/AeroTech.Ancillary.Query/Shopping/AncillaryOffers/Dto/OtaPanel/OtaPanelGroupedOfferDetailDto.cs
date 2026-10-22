namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.OtaPanel
{
    public sealed record OtaPanelGroupedOfferDetailDto(
        string OfferItemRef,
        string VariantCode,
        string Readiness,
        OtaPanelGroupedQuantityDto? Quantity,
        IReadOnlyList<OtaPanelGroupedRequiredFieldDto> RequiredFields,
        IReadOnlyList<OtaPanelGroupedOptionGroupDto> Options,
        OtaPanelGroupedSelectedDto? Selected,
        OtaPanelGroupedPriceDto Price,
        string? SelectionId,
        IReadOnlyList<string> ReasonCodes);
}
