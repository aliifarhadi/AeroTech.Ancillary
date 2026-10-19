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
                Number(offer.BaseAmount),
                Number(offer.ChargeAmount),
                Number(offer.TotalAmount)
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
                parts.Add(ticket.TravellerIndex.ToString(CultureInfo.InvariantCulture));
                parts.Add(ticket.PassengerTypeCode.ToString());

                foreach (var coupon in ticket.Coupons.OrderBy(coupon => coupon.BoundId, StringComparer.Ordinal).ThenBy(coupon => coupon.FlightId))
                {
                    parts.Add(coupon.BoundId);
                    parts.Add(coupon.FlightId.ToString(CultureInfo.InvariantCulture));
                    parts.Add(Baggage(coupon.CheckedBaggage));
                    parts.Add(Baggage(coupon.CabinBaggage));
                    parts.Add(Flag(coupon.IsRefundable));
                    parts.Add(Flag(coupon.IsChangeable));
                    parts.Add(Flag(coupon.IsUpgradable));
                    parts.Add(string.Join(',', coupon.AirFareIds.Order().Select(id => id.ToString(CultureInfo.InvariantCulture))));
                    parts.AddRange(Lines(coupon.PriceLines));
                }
            }

            parts.AddRange(Lines(offer.OrderCharges));

            foreach (var rate in offer.RatesOfExchange
                         .OrderBy(rate => rate.RateOfExchangePeriodId, StringComparer.Ordinal)
                         .ThenBy(rate => rate.FromCurrencyId)
                         .ThenBy(rate => rate.ToCurrencyId))
            {
                parts.Add(rate.RateOfExchangePeriodId);
                parts.Add(rate.FromCurrencyId.ToString(CultureInfo.InvariantCulture));
                parts.Add(rate.ToCurrencyId.ToString(CultureInfo.InvariantCulture));
                parts.Add(Number(rate.Rate));
                parts.Add(rate.DecimalPlaces.ToString(CultureInfo.InvariantCulture));
                parts.Add(rate.RoundingFactor.ToString(CultureInfo.InvariantCulture));
            }

            return Base64Url.EncodeToString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(Separator, parts))));
        }

        private static string Flag(bool value) => value ? "1" : "0";

        private static string Number(decimal value) => value.ToString("0.############################", CultureInfo.InvariantCulture);

        private static string Baggage(FlightOfferBaggage? baggage)
            => baggage is null
                ? string.Empty
                : string.Join(
                    ',',
                    baggage.Pieces.ToString(CultureInfo.InvariantCulture),
                    Number(baggage.Weight),
                    baggage.Unit.ToString());

        private static IEnumerable<string> Lines(IEnumerable<FlightOfferPriceLine> lines)
            => lines
                .Select(line => string.Join(
                    ',',
                    line.Category.ToString(),
                    line.Code ?? string.Empty,
                    line.Reference ?? string.Empty,
                    Number(line.Amount),
                    line.CurrencyId.ToString(CultureInfo.InvariantCulture),
                    Flag(line.IsPercentage),
                    Number(line.EquivalentAmount),
                    line.EquivalentCurrencyId.ToString(CultureInfo.InvariantCulture),
                    line.RateOfExchangePeriodId ?? string.Empty))
                .Order(StringComparer.Ordinal);
    }
}
