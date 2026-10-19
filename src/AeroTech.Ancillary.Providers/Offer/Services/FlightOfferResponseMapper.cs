using System.Globalization;
using AeroTech.Ancillary.Application.FlightOffers;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Providers.Offer.Wire;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Providers.Offer.Services
{
    public static class FlightOfferResponseMapper
    {
        public static FlightOffer ToDomain(FlightOfferDetailResponse source, string requestedOfferId)
        {
            Require(string.Equals(source.OfferId, requestedOfferId, StringComparison.Ordinal), nameof(source.OfferId));

            if (source.AirTransports.Count == 0 || source.Tickets.Count == 0)
                throw ExceptionFactory.FlightOfferNotFound();

            Require(source.CurrencyId > 0, nameof(source.CurrencyId));
            Require(PricedOfferId.TryReadCustomerId(source.OfferId, out var customerId), nameof(source.OfferId));
            Unique(source.AirTransports.Select(transport => transport.BoundId), nameof(OfferAirTransport.BoundId));
            Unique(source.AirTransports.Select(transport => transport.BoundOfferId), nameof(OfferAirTransport.BoundOfferId));
            Unique(source.AirTransports.Select(transport => transport.Sequence.ToString(CultureInfo.InvariantCulture)), nameof(OfferAirTransport.Sequence));
            Unique(source.Tickets.Select(ticket => ticket.TravellerRef), nameof(OfferTicket.TravellerRef));

            var bounds = source.AirTransports.OrderBy(transport => transport.Sequence).Select(MapBound).ToList();
            var boundOfferIds = source.AirTransports.Select(transport => transport.BoundOfferId).ToHashSet(StringComparer.Ordinal);

            Require(
                source.PricingUnits.SelectMany(unit => unit.CoveredBoundOfferIds).All(boundOfferIds.Contains),
                nameof(OfferPricingUnit.CoveredBoundOfferIds));

            return new FlightOffer(
                source.OfferId,
                source.PricedAt,
                source.LastTicketingDate,
                customerId,
                source.CurrencyId,
                string.IsNullOrWhiteSpace(source.CurrencyCode) ? null : source.CurrencyCode,
                source.BaseAmount,
                source.ChargeAmount,
                source.TotalAmount,
                bounds,
                source.PricingUnits.SelectMany(unit => unit.FareComponents).Select(component => MapFareComponent(component, bounds)).ToList(),
                source.Tickets.OrderBy(ticket => ticket.TravellerIndex).Select(ticket => MapTicket(ticket, bounds)).ToList(),
                source.OrderCharges.Select(MapLine).ToList(),
                source.RatesOfExchange.Select(MapRateOfExchange).ToList());
        }

        private static FlightOfferBound MapBound(OfferAirTransport transport)
        {
            Require(transport.Sequence > 0, nameof(OfferAirTransport.Sequence));
            Require(transport.OriginAirportId > 0 && transport.DestinationAirportId > 0, nameof(OfferAirTransport.OriginAirportId));
            Require(transport.Flights.Count > 0, nameof(OfferAirTransport.Flights));
            Unique(transport.Flights.Select(flight => flight.Sequence.ToString(CultureInfo.InvariantCulture)), nameof(OfferFlight.Sequence));
            Unique(transport.Flights.Select(flight => flight.FlightId.ToString(CultureInfo.InvariantCulture)), nameof(OfferFlight.FlightId));

            return new FlightOfferBound(
                transport.BoundId,
                transport.Sequence,
                Direction(transport.Direction),
                transport.OriginAirportId,
                transport.DestinationAirportId,
                transport.Flights.OrderBy(flight => flight.Sequence).Select(MapFlight).ToList());
        }

        private static BoundDirection? Direction(OfferBoundDirection direction)
            => direction switch
            {
                OfferBoundDirection.Outbound => BoundDirection.Outbound,
                OfferBoundDirection.Inbound => BoundDirection.Inbound,
                _ => null
            };

        private static FlightOfferFlight MapFlight(OfferFlight flight)
        {
            Require(flight.FlightId > 0, nameof(OfferFlight.FlightId));
            Require(flight.OriginAirportId > 0 && flight.DestinationAirportId > 0, nameof(OfferFlight.OriginAirportId));
            Require(flight.OperatingAirlineId > 0 && flight.MarketingAirlineId > 0, nameof(OfferFlight.OperatingAirlineId));
            Require(flight.DepartureDateTime < flight.ArrivalDateTime, nameof(OfferFlight.ArrivalDateTime));

            Require(flight.FlightCapacityId > 0, nameof(OfferFlight.FlightCapacityId));

            return new FlightOfferFlight(
                flight.Sequence,
                flight.FlightCapacityId,
                flight.FlightId,
                flight.FlightVersion,
                string.IsNullOrWhiteSpace(flight.FlightNumber) ? null : flight.FlightNumber,
                flight.OriginAirportId,
                flight.OriginAirportTerminalId,
                flight.DestinationAirportId,
                flight.DestinationAirportTerminalId,
                flight.OperatingAirlineId,
                flight.MarketingAirlineId,
                flight.DepartureDateTime,
                flight.ArrivalDateTime,
                flight.AircraftId is > 0 ? flight.AircraftId : null,
                flight.CabinClassId is > 0 ? flight.CabinClassId : null,
                flight.RbdId is > 0 ? flight.RbdId : null,
                string.IsNullOrWhiteSpace(flight.BookingClass) ? null : flight.BookingClass,
                flight.Legs
                    .OrderBy(leg => leg.Sequence)
                    .Select(leg => new FlightOfferLeg(leg.Sequence, leg.LegId, leg.OriginAirportId, leg.DestinationAirportId, leg.DepartureDateTime, leg.ArrivalDateTime))
                    .ToList());
        }

        private static FlightOfferFareComponent MapFareComponent(OfferFareComponent component, IReadOnlyList<FlightOfferBound> bounds)
        {
            Require(component.AirFareId > 0, nameof(OfferFareComponent.AirFareId));
            Require(bounds.Any(bound => bound.BoundId == component.BoundId), nameof(OfferFareComponent.BoundId));

            return new FlightOfferFareComponent(
                component.AirFareId,
                component.BoundId,
                component.CabinClassId is > 0 ? component.CabinClassId : null,
                component.RbdId is > 0 ? component.RbdId : null,
                string.IsNullOrWhiteSpace(component.BookingClass) ? null : component.BookingClass,
                string.IsNullOrWhiteSpace(component.FareBasis) ? null : component.FareBasis,
                string.IsNullOrWhiteSpace(component.FareFamily) ? null : component.FareFamily,
                Named<AirFareType>(component.FareType));
        }

        private static FlightOfferTicket MapTicket(OfferTicket ticket, IReadOnlyList<FlightOfferBound> bounds)
        {
            var passengerType = Named<PassengerTypeCode>(ticket.PassengerTypeCode);
            var sold = bounds.SelectMany(bound => bound.Flights.Select(flight => (bound.BoundId, flight.FlightId))).ToList();
            var covered = ticket.Coupons.Select(coupon => (coupon.BoundId, coupon.FlightId)).ToList();

            Require(passengerType is not null, nameof(OfferTicket.PassengerTypeCode));
            Require(covered.Count == sold.Count && covered.Distinct().Count() == covered.Count && covered.All(sold.Contains), nameof(OfferTicket.Coupons));

            return new FlightOfferTicket(
                ticket.TravellerRef,
                ticket.TravellerIndex,
                passengerType!.Value,
                sold.Select(flight => MapCoupon(ticket.Coupons.Single(coupon => (coupon.BoundId, coupon.FlightId) == flight))).ToList());
        }

        private static FlightOfferCoupon MapCoupon(OfferCoupon coupon)
            => new(
                coupon.BoundId,
                coupon.FlightId,
                Baggage(coupon.BaggagePieces, coupon.BaggageWeight, coupon.BaggageUnit),
                Baggage(coupon.CabinBaggagePieces, coupon.CabinBaggageWeight, coupon.CabinBaggageUnit),
                coupon.IsRefundable,
                coupon.IsChangeable,
                coupon.IsUpgradable,
                coupon.Pricings
                    .Where(line => line.Category == OfferPricingCategory.Fare)
                    .Select(line => long.TryParse(line.Reference, NumberStyles.None, CultureInfo.InvariantCulture, out var airFareId) ? airFareId : 0)
                    .Where(airFareId => airFareId > 0)
                    .Distinct()
                    .ToList(),
                coupon.Pricings.Select(MapLine).ToList());

        private static FlightOfferPriceLine MapLine(OfferPricingLine line)
        {
            Require(Enum.IsDefined(line.Category), nameof(OfferPricingLine.Category));

            return new FlightOfferPriceLine(
                line.Category switch
                {
                    OfferPricingCategory.Fare => FlightOfferPriceCategory.Fare,
                    OfferPricingCategory.Tax => FlightOfferPriceCategory.Tax,
                    OfferPricingCategory.Fee => FlightOfferPriceCategory.Fee,
                    _ => FlightOfferPriceCategory.Surcharge
                },
                string.IsNullOrWhiteSpace(line.Name) ? null : line.Name,
                string.IsNullOrWhiteSpace(line.Code) ? null : line.Code,
                string.IsNullOrWhiteSpace(line.Reference) ? null : line.Reference,
                line.Amount,
                line.CurrencyId,
                line.IsPercentage,
                line.EquivalentAmount,
                line.EquivalentCurrencyId,
                string.IsNullOrWhiteSpace(line.RateOfExchangePeriodId) ? null : line.RateOfExchangePeriodId);
        }

        private static FlightOfferRateOfExchange MapRateOfExchange(OfferRateOfExchange rate)
        {
            Require(!string.IsNullOrWhiteSpace(rate.RateOfExchangePeriodId), nameof(OfferRateOfExchange.RateOfExchangePeriodId));
            Require(rate.FromCurrencyId > 0 && rate.ToCurrencyId > 0, nameof(OfferRateOfExchange.FromCurrencyId));
            Require(rate.Rate > 0, nameof(OfferRateOfExchange.Rate));

            return new FlightOfferRateOfExchange(
                rate.RateOfExchangePeriodId,
                rate.FromCurrencyId,
                rate.ToCurrencyId,
                rate.Rate,
                rate.DecimalPlaces,
                rate.RoundingFactor);
        }

        private static FlightOfferBaggage? Baggage(int pieces, decimal weight, string? unit)
            => Named<WeightUnit>(unit) is { } known && pieces >= 0 && weight >= 0 ? new FlightOfferBaggage(pieces, weight, known) : null;

        private static TEnum? Named<TEnum>(string? name)
            where TEnum : struct, Enum
            => name is not null && Enum.GetNames<TEnum>().Contains(name, StringComparer.Ordinal) ? Enum.Parse<TEnum>(name) : null;

        private static void Unique(IEnumerable<string> values, string field)
        {
            var list = values.ToList();

            Require(list.All(value => !string.IsNullOrWhiteSpace(value)) && list.Distinct(StringComparer.Ordinal).Count() == list.Count, field);
        }

        private static void Require(bool condition, string field)
        {
            if (!condition)
                throw ExceptionFactory.FlightOfferIsInconsistent(field);
        }
    }
}
