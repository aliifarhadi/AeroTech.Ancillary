using System.Globalization;
using AeroTech.Ancillary.Application.FlightOffers;
using AeroTech.Ancillary.Query.Shopping.Common.FlightOffers;

namespace AeroTech.Ancillary.Query.Shopping.Common
{
    public static class ApiOfferIdentifiers
    {
        public static string Id(int number) => number.ToString(CultureInfo.InvariantCulture);

        public static IReadOnlyDictionary<string, string> TravelerIds(FlightOffer offer)
            => offer.Tickets.ToDictionary(ticket => ticket.TravellerRef, ticket => Id(ticket.TravellerIndex), StringComparer.Ordinal);

        public static IReadOnlyList<ApiSegmentLeg> Segments(FlightOffer offer)
        {
            var segments = new List<ApiSegmentLeg>();

            foreach (var bound in offer.Bounds.OrderBy(bound => bound.Sequence))
            {
                foreach (var flight in bound.Flights.OrderBy(flight => flight.Sequence))
                {
                    foreach (var leg in flight.Legs.OrderBy(leg => leg.Sequence))
                        segments.Add(new ApiSegmentLeg(Id(segments.Count + 1), FlightOfferShoppingContextMapper.FlightRef(bound, flight), flight, leg));
                }
            }

            return segments;
        }
    }

    public sealed record ApiSegmentLeg(string Id, string FlightRef, FlightOfferFlight Flight, FlightOfferLeg Leg);
}
