using System.Buffers.Text;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using AeroTech.Ancillary.Application.FlightOffers;

namespace AeroTech.Ancillary.Query.Shopping.Common.FlightOffers
{
    public static class FlightOfferFingerprint
    {
        private const char Separator = '\n';

        public static string Of(FlightOffer offer)
        {
            var parts = new List<string>
            {
                offer.OfferId,
                offer.PricedAt.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),
                offer.CurrencyId.ToString(CultureInfo.InvariantCulture),
                offer.BaseAmount.ToString(CultureInfo.InvariantCulture),
                offer.ChargeAmount.ToString(CultureInfo.InvariantCulture),
                offer.TotalAmount.ToString(CultureInfo.InvariantCulture)
            };

            foreach (var bound in offer.Bounds.OrderBy(bound => bound.Sequence))
            {
                parts.Add(bound.BoundId);

                foreach (var flight in bound.Flights.OrderBy(flight => flight.Sequence))
                {
                    parts.Add(flight.FlightId.ToString(CultureInfo.InvariantCulture));
                    parts.Add(flight.FlightCapacityId.ToString(CultureInfo.InvariantCulture));
                    parts.Add(flight.FlightVersion.ToString(CultureInfo.InvariantCulture));
                    parts.Add(flight.AircraftId?.ToString(CultureInfo.InvariantCulture) ?? string.Empty);
                    parts.Add(flight.CabinClassId?.ToString(CultureInfo.InvariantCulture) ?? string.Empty);
                    parts.Add(flight.RbdId?.ToString(CultureInfo.InvariantCulture) ?? string.Empty);
                    parts.Add(flight.BookingClass ?? string.Empty);
                    parts.Add(flight.OriginAirportId.ToString(CultureInfo.InvariantCulture));
                    parts.Add(flight.DestinationAirportId.ToString(CultureInfo.InvariantCulture));
                    parts.Add(flight.DepartureAt.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture));
                    parts.Add(flight.ArrivalAt.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture));
                }
            }

            foreach (var component in offer.FareComponents.OrderBy(component => component.BoundId, StringComparer.Ordinal).ThenBy(component => component.AirFareId))
            {
                parts.Add(component.BoundId);
                parts.Add(component.AirFareId.ToString(CultureInfo.InvariantCulture));
                parts.Add(component.FareBasis ?? string.Empty);
                parts.Add(component.FareFamily ?? string.Empty);
                parts.Add(component.FareType?.ToString() ?? string.Empty);
            }

            foreach (var ticket in offer.Tickets.OrderBy(ticket => ticket.TravellerIndex))
            {
                parts.Add(ticket.TravellerRef);
                parts.Add(ticket.PassengerTypeCode.ToString());
            }

            return Base64Url.EncodeToString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(Separator, parts))));
        }
    }
}
