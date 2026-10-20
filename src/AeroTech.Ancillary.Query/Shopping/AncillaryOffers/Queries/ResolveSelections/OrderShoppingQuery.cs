using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.ResolveSelections
{
    public sealed class OrderShoppingQuery : IAncillaryOffersQuery
    {
        public OrderShoppingQuery(long orderId) => OrderId = orderId;

        public string? OfferId => null;

        public long? OrderId { get; }
    }
}
