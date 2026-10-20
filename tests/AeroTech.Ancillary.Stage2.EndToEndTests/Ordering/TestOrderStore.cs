using System.Collections.Concurrent;
using AeroTech.Ancillary.Application.FlightOffers;
using AeroTech.Ancillary.Application.Orders;
using AeroTech.Ancillary.Query.Shopping.Common.FlightOffers;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Stage2.EndToEndTests.Ordering;

public sealed class TestOrderStore
{
    public const string AirTransportProviderKey = "FlightFlow";

    private readonly ConcurrentDictionary<long, TestOrder> _orders = new();
    private long _nextOrderId = 7000;
    private long _nextServiceId = 50000;

    public bool OrderingUnavailable { get; set; }

    public TestOrder Create(string offerId, FlightOffer air)
    {
        var order = new TestOrder { OrderId = Interlocked.Increment(ref _nextOrderId), SourceOfferId = offerId, Air = air };

        foreach (var ticket in air.Tickets)
        {
            var traveller = new TestOrderTraveller(
                ticket.TravellerRef,
                1000 + ticket.TravellerIndex,
                ticket.PassengerTypeCode.ToString(),
                new DateOnly(ticket.PassengerTypeCode == PassengerTypeCode.CHD ? 2018 : 1990, 5, 20));

            order.Travellers.Add(traveller);

            foreach (var coupon in ticket.Coupons)
            {
                var flight = air.Bounds.SelectMany(bound => bound.Flights).Single(row => row.FlightId == coupon.FlightId);

                order.AirServices.Add(new TestOrderAirService(
                    NextServiceId(),
                    traveller.OrderTravellerId,
                    traveller.TravellerRef,
                    coupon.BoundId,
                    flight.FlightId,
                    flight.FlightCapacityId,
                    AirTransportProviderKey));
            }
        }

        _orders[order.OrderId] = order;

        return order;
    }

    public TestOrder? Find(long orderId) => _orders.GetValueOrDefault(orderId);

    public long NextServiceId() => Interlocked.Increment(ref _nextServiceId);

    public OrderCommercialContext? ContextOf(long orderId)
    {
        if (Find(orderId) is not { } order)
            return null;

        lock (order)
        {
            var flightRefs = order.Air.Bounds
                .SelectMany(bound => bound.Flights.Select(flight => (flight.FlightId, Ref: FlightOfferShoppingContextMapper.FlightRef(bound, flight))))
                .ToDictionary(row => row.FlightId, row => row.Ref);

            return new OrderCommercialContext(
                order.OrderId,
                order.CommercialVersion,
                order.AcceptsChanges,
                order.TicketedAtUtc,
                order.Air,
                order.Travellers.Select(traveller => new OrderTravellerFacts(traveller.TravellerRef, traveller.OrderTravellerId, traveller.DateOfBirth, null)).ToList(),
                order.Services
                    .Select(service => new OrderServiceFacts(
                        service.OrderServiceId,
                        service.TravellerRef,
                        service.ServiceDefinitionRef,
                        service.PortionRefs.FirstOrDefault(),
                        service.FlightIds.Select(flightId => flightRefs[flightId]).ToList(),
                        service.Quantity ?? 1,
                        ExistingServiceCommercialState.Active))
                    .ToList());
        }
    }
}
