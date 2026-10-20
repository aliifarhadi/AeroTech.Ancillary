namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Selection
{
    public sealed record OfferInteractionDto(string Mode, bool RequiresDetails, IReadOnlyList<string> ReasonCodes);
}
