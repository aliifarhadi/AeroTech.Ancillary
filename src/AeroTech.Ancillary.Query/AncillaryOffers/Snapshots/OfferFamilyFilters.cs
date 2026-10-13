using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Snapshots
{
    public static class OfferFamilyFilters
    {
        public static readonly ShoppingFilter Seat = new() { Profiles = [AncillaryProfile.Seat] };

        public static readonly ShoppingFilter Services = new() { Profiles = Enum.GetValues<AncillaryProfile>().Where(profile => profile != AncillaryProfile.Seat).ToList() };
    }
}
