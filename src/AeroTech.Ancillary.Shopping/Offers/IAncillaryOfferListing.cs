using AeroTech.Ancillary.Domain.SelectionSnapshotAggregate;
using AeroTech.Ancillary.Shopping.Context;

namespace AeroTech.Ancillary.Shopping.Offers
{
    public interface IAncillaryOfferListing
    {
        Task<IReadOnlyList<AncillaryOfferRow>> ListAsync(
            IReadOnlyList<AncillaryShoppingContext> contexts,
            ShoppingFilter filter,
            SelectionSnapshot? selection,
            CancellationToken cancellationToken = default);
    }
}
