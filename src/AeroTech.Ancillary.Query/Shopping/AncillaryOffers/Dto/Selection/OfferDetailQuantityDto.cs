namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Selection
{
    public sealed record OfferDetailQuantityDto(
        string Unit,
        int Min,
        int Max,
        int Step,
        bool SelectionRequired);
}
