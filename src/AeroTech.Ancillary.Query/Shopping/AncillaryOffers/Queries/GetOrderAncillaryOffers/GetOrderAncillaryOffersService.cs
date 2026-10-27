using AeroTech.Ancillary.Domain.SelectionSnapshotAggregate;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.Shopping.Common.Orders;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Ancillary.Shopping.Offers;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetOrderAncillaryOffers
{
    public sealed class GetOrderAncillaryOffersService : IGetOrderAncillaryOffersService
    {
        private readonly IOrderShoppingSource _orders;
        private readonly IAncillaryOfferListing _offers;

        public GetOrderAncillaryOffersService(IOrderShoppingSource orders, IAncillaryOfferListing offers)
        {
            _orders = orders;
            _offers = offers;
        }

        public Task<OrderAncillaryOffersSnapshot> ExecuteAsync(IOrderAncillaryOffersQuery query, AncillarySalesScope scope, CancellationToken cancellationToken = default)
            => ListAsync(query.OrderId, scope, null, cancellationToken);

        public Task<OrderAncillaryOffersSnapshot> ExecuteAsync(SelectionSnapshot selection, AncillarySalesScope scope, CancellationToken cancellationToken = default)
            => ListAsync(selection.OrderId, scope, selection, cancellationToken);

        private async Task<OrderAncillaryOffersSnapshot> ListAsync(
            long? orderId,
            AncillarySalesScope scope,
            SelectionSnapshot? selection,
            CancellationToken cancellationToken)
        {
            var order = await _orders.LoadAsync(orderId, scope, cancellationToken);

            return new OrderAncillaryOffersSnapshot(
                order.Order.OrderId,
                order.Order.CommercialVersion,
                await _offers.ListServicesAsync(order.Shopping, selection, cancellationToken));
        }
    }
}
