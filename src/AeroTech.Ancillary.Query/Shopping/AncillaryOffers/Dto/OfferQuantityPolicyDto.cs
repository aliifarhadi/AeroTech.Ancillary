using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto
{
    public sealed record OfferQuantityPolicyDto(bool Selectable, int Min, int Max, AncillaryQuantityUnit Unit);
}
