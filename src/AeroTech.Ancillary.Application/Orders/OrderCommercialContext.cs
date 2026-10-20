using AeroTech.Ancillary.Application.FlightOffers;

namespace AeroTech.Ancillary.Application.Orders
{
    public sealed record OrderCommercialContext(
        long OrderId,
        int CommercialVersion,
        bool AcceptsChanges,
        DateTimeOffset? TicketedAtUtc,
        FlightOffer Air,
        IReadOnlyList<OrderTravellerFacts> Travellers,
        IReadOnlyList<OrderServiceFacts> Services);
}
