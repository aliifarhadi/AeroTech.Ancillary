using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.ResolveGroupedSelections
{
    public static class GroupedHoldCapability
    {
        public static string? Blocker(CanonicalAncillaryOfferCandidate candidate)
            => candidate.Profile == AncillaryProfile.Seat
                ? ShoppingReasonCodes.SeatSelectionNotOffered
                : candidate.Availability.InventoryAuthority switch
                {
                    null => ShoppingReasonCodes.InventoryNotConfigured,
                    InventoryAuthority.Supplier => ShoppingReasonCodes.InventoryDelegatedCheckRequired,
                    InventoryAuthority.Local => ShoppingReasonCodes.InventorySourceNotConfigured,
                    _ => candidate.Availability.RequiresCheckAtReserve ? ShoppingReasonCodes.AvailabilityMustBeChecked : null
                };
    }
}
