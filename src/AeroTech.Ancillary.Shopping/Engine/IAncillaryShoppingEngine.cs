using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Ancillary.Shopping.Selection;

namespace AeroTech.Ancillary.Shopping.Engine
{
    public interface IAncillaryShoppingEngine
    {
        Task<CanonicalAncillaryOfferResult> ShopAsync(
            AncillaryShoppingContext context,
            ShoppingFilter? filter = null,
            CancellationToken cancellationToken = default);

        Task<CanonicalAncillarySelectionEvaluation> EvaluateSelectionAsync(
            AncillaryShoppingContext context,
            CandidateIdentity candidate,
            AncillarySelection selection,
            CancellationToken cancellationToken = default);
    }
}
