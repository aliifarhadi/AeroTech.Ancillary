using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.Shopping.Common.FlightOffers;
using AeroTech.Ancillary.Query.Shopping.Common.Orders;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;

namespace AeroTech.Ancillary.Query.Shopping.Common
{
    public sealed class ShoppingSource : IShoppingSource
    {
        private readonly IFlightOfferShoppingSource _flightOffers;
        private readonly IOrderShoppingSource _orders;

        public ShoppingSource(IFlightOfferShoppingSource flightOffers, IOrderShoppingSource orders)
        {
            _flightOffers = flightOffers;
            _orders = orders;
        }

        public Task<FlightOfferShopping> LoadAsync(string? offerId, long? orderId, AncillarySalesScope scope, CancellationToken cancellationToken = default)
            => orderId switch
            {
                null => _flightOffers.LoadAsync(offerId, scope, cancellationToken),
                { } order when string.IsNullOrWhiteSpace(offerId) => _orders.LoadAsync(order, scope, cancellationToken),
                _ => throw ExceptionFactory.ShoppingContextIsInvalid("send either an offerId or an orderId, not both")
            };
    }
}
