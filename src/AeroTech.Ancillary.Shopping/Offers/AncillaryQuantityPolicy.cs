using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Shopping.Offers
{
    public sealed record AncillaryQuantityPolicy(bool Selectable, int Min, int Max, AncillaryQuantityUnit Unit);
}
