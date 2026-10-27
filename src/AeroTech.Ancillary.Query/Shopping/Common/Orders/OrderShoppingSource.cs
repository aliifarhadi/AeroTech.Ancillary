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

        public async Task<OrderShopping> LoadAsync(long? orderId, AncillarySalesScope scope, CancellationToken cancellationToken = default)
        {
            if (orderId is not > 0)
                throw ExceptionFactory.ShoppingContextIsInvalid("orderId is required");

            var order = await _orders.FindAsync(orderId.Value, cancellationToken) ?? throw ExceptionFactory.ShoppingCallerNotAuthorized();

            OrderScopeGuard.EnsureSellable(order, scope);

            var shopping = await _flightOffers.ContextualizeAsync(order.Air, scope, OrderSourceFingerprint.Of(order), cancellationToken);

            return new OrderShopping(
                order,
                shopping with
                {
                    PointsOfSale = shopping.PointsOfSale
                        .Select(pointOfSale => pointOfSale with { Context = OrderShoppingContextMapper.Map(pointOfSale.Context, order) })
                        .ToList()
                });
        }
    }
}
