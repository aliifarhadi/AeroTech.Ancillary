using AeroTech.Ancillary.Application.Orders;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.Shopping.Common.FlightOffers;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;

namespace AeroTech.Ancillary.Query.Shopping.Common.Orders
{
    public sealed class OrderShoppingSource : IOrderShoppingSource
    {
        private readonly IOrderCommercialContextReader _orders;
        private readonly IFlightOfferShoppingSource _flightOffers;

        public OrderShoppingSource(IOrderCommercialContextReader orders, IFlightOfferShoppingSource flightOffers)
        {
            _orders = orders;
            _flightOffers = flightOffers;
        }

        public async Task<FlightOfferShopping> LoadAsync(long orderId, AncillarySalesScope scope, CancellationToken cancellationToken = default)
        {
            var order = await _orders.FindAsync(orderId, cancellationToken) ?? throw ExceptionFactory.ShoppingCallerNotAuthorized();

            if (!order.AcceptsChanges)
                throw ExceptionFactory.ShoppingContextIsInvalid($"order {orderId} does not accept new services");

            if (string.IsNullOrWhiteSpace(order.Air.OfferId))
                throw ExceptionFactory.ShoppingContextIsInvalid($"order {orderId} carries no source offer id");

            var shopping = await _flightOffers.ContextualizeAsync(order.Air, scope, OrderSourceFingerprint.Of(order), cancellationToken);

            return shopping with
            {
                PointsOfSale = shopping.PointsOfSale
                    .Select(pointOfSale => pointOfSale with { Context = OrderShoppingContextMapper.Map(pointOfSale.Context, order) })
                    .ToList()
            };
        }
    }
}
