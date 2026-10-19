namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Selection
{
    public sealed record OfferSelectionContractDto(
        string Kind,
        IReadOnlyList<OfferSelectionFieldDto> Fields);
}
