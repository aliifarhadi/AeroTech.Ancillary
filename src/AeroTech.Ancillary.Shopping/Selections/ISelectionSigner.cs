using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Results;

namespace AeroTech.Ancillary.Shopping.Selections
{
    public interface ISelectionSigner
    {
        string? SignDirect(AncillaryShoppingContext context, CanonicalAncillaryOfferCandidate candidate, AncillaryProvision provision, int minQuantity, int maxQuantity);

        string? SignReference(AncillaryShoppingContext context, CanonicalAncillaryOfferCandidate candidate);
    }
}
