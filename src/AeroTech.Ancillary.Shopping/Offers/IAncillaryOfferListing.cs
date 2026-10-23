using AeroTech.Ancillary.Shopping.Context;

namespace AeroTech.Ancillary.Shopping.Offers
{
    public interface IAncillaryOfferListing
    {
        Task<IReadOnlyList<AncillaryOfferRow>> ListAsync(
            IReadOnlyList<AncillaryShoppingContext> contexts,
            ShoppingFilter filter,
            CancellationToken cancellationToken = default);
    }
}
