using AeroTech.Ancillary.Domain.SelectionSnapshotAggregate;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.Shopping.Common;
using AeroTech.Ancillary.Query.Shopping.Common.FlightOffers;
using AeroTech.Ancillary.Shopping.Offers;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers
{
    public static class AncillaryOfferListingExtensions
    {
        public static async Task<AncillaryOffersSnapshot> ListServicesAsync(
            this IAncillaryOfferListing listing,
            FlightOfferShopping shopping,
            SelectionSnapshot? selection,
            CancellationToken cancellationToken)
            => new(
                shopping.Offer.OfferId,
                shopping.CurrencyId,
                await listing.ListAsync(shopping.PointsOfSale.Select(pointOfSale => pointOfSale.Context).ToList(), OfferFamilyFilters.Services, selection, cancellationToken));
    }
}
