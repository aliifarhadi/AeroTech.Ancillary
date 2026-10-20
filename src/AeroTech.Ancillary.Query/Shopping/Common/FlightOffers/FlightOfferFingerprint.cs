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
                Instant(offer.PricedAt),
                offer.CurrencyId.ToString(CultureInfo.InvariantCulture),
                Number(offer.BaseAmount),
                Number(offer.ChargeAmount),
                Number(offer.TotalAmount)
            };

            parts.Add(offer.SalesChannel?.ToString() ?? string.Empty);
            parts.Add(Optional(offer.CustomerId));
            parts.Add(Optional(offer.SalesCountryId));

            foreach (var bound in offer.Bounds.OrderBy(bound => bound.Sequence))
            {
                parts.Add(bound.BoundId);
                parts.Add(bound.Sequence.ToString(CultureInfo.InvariantCulture));
                parts.Add(bound.Direction?.ToString() ?? string.Empty);
                parts.Add(bound.OriginAirportId.ToString(CultureInfo.InvariantCulture));
                parts.Add(bound.DestinationAirportId.ToString(CultureInfo.InvariantCulture));

                foreach (var flight in bound.Flights.OrderBy(flight => flight.Sequence))
                {
                    parts.Add(flight.Sequence.ToString(CultureInfo.InvariantCulture));
                    parts.Add(flight.FlightId.ToString(CultureInfo.InvariantCulture));
                    parts.Add(flight.FlightCapacityId.ToString(CultureInfo.InvariantCulture));
                    parts.Add(flight.FlightVersion.ToString(CultureInfo.InvariantCulture));
                    parts.Add(flight.FlightNumber ?? string.Empty);
                    parts.Add(flight.MarketingAirlineId.ToString(CultureInfo.InvariantCulture));
                    parts.Add(flight.OperatingAirlineId.ToString(CultureInfo.InvariantCulture));
                    parts.Add(Optional(flight.AircraftId));
                    parts.Add(Optional(flight.CabinClassId));
                    parts.Add(Optional(flight.RbdId));
                    parts.Add(flight.BookingClass ?? string.Empty);
                    parts.Add(flight.OriginAirportId.ToString(CultureInfo.InvariantCulture));
                    parts.Add(Optional(flight.OriginAirportTerminalId));
                    parts.Add(flight.DestinationAirportId.ToString(CultureInfo.InvariantCulture));
                    parts.Add(Optional(flight.DestinationAirportTerminalId));
                    parts.Add(Instant(flight.DepartureAt));
                    parts.Add(Instant(flight.ArrivalAt));

                    foreach (var leg in flight.Legs.OrderBy(leg => leg.Sequence))
                    {
                        parts.Add(leg.Sequence.ToString(CultureInfo.InvariantCulture));
                        parts.Add(leg.LegId.ToString(CultureInfo.InvariantCulture));
                        parts.Add(leg.OriginAirportId.ToString(CultureInfo.InvariantCulture));
                        parts.Add(leg.DestinationAirportId.ToString(CultureInfo.InvariantCulture));
                        parts.Add(Instant(leg.DepartureAt));
                        parts.Add(Instant(leg.ArrivalAt));
                    }
                }
            }

            foreach (var component in offer.FareComponents.OrderBy(component => component.BoundId, StringComparer.Ordinal).ThenBy(component => component.AirFareId))
            {
                parts.Add(component.BoundId);
                parts.Add(component.AirFareId.ToString(CultureInfo.InvariantCulture));
                parts.Add(Optional(component.CabinClassId));
                parts.Add(Optional(component.RbdId));
                parts.Add(component.BookingClass ?? string.Empty);
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
                    parts.Add(Optional(coupon.IsOpenForUse));
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

        private static string Optional(long? value) => value?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;

        private static string Optional(bool? value) => value is { } flag ? Flag(flag) : string.Empty;

        private static string Instant(DateTimeOffset value) => value.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);

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
