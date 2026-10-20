using AeroTech.Ancillary.Application.FlightOffers;
using AeroTech.Ancillary.Application.Orders;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Shopping.Tests.Fixtures;

public static class OrderDesk
{
    public const long OrderId = 7001;

    public sealed class FixedOrders : IOrderCommercialContextReader
    {
        private readonly Dictionary<long, OrderCommercialContext> _orders = [];

        public FixedOrders(params OrderCommercialContext[] orders)
        {
            foreach (var order in orders)
                _orders[order.OrderId] = order;
        }

        public List<long> Asked { get; } = [];

        public void Put(OrderCommercialContext order) => _orders[order.OrderId] = order;

        public Task<OrderCommercialContext?> FindAsync(long orderId, CancellationToken cancellationToken = default)
        {
            Asked.Add(orderId);

            return Task.FromResult(_orders.GetValueOrDefault(orderId));
        }
    }

    public static OrderCommercialContext OrderOf(
        FlightOffer air,
        int commercialVersion = 1,
        DateTimeOffset? ticketedAtUtc = null,
        bool acceptsChanges = true,
        params OrderServiceFacts[] services)
        => new(
            OrderId,
            commercialVersion,
            acceptsChanges,
            ticketedAtUtc,
            air,
            air.Tickets
                .Select(ticket => new OrderTravellerFacts(ticket.TravellerRef, 100 + ticket.TravellerIndex, new DateOnly(ticket.PassengerTypeCode == PassengerTypeCode.CHD ? 2018 : 1990, 5, 20), null))
                .ToList(),
            services);

    public static FlightOffer Ticketed(FlightOffer air, Func<FlightOfferTicket, FlightOfferCoupon, bool> openForUse)
        => air with
        {
            Tickets = air.Tickets
                .Select(ticket => ticket with { Coupons = ticket.Coupons.Select(coupon => coupon with { IsOpenForUse = openForUse(ticket, coupon) }).ToList() })
                .ToList()
        };

    public static OrderServiceFacts Service(long orderServiceId, string travellerRef, string serviceRef, string portionRef, params string[] flightRefs)
        => new(orderServiceId, travellerRef, serviceRef, portionRef, flightRefs, 1, ExistingServiceCommercialState.Active);
}
