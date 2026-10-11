using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Shopping.FlightOffers
{
    public static class FlightOfferShoppingContextMapper
    {
        public const string Provenance = "FlightOffers/Details";

        public static string FlightRef(FlightOfferBound bound, FlightOfferFlight flight) => $"{bound.BoundId}:F{flight.Sequence}";

        public static AncillaryShoppingContext Map(FlightOffer offer, FlightOfferSalesScope scope, IReadOnlyDictionary<int, int> airportCountryIds)
        {
            var coupons = offer.Tickets
                .SelectMany(ticket => ticket.Coupons.Select(coupon => (Ticket: ticket, Coupon: coupon, Flight: FlightOf(offer, coupon))))
                .ToList();
            var fareFacts = coupons.Select(row => FareFacts(offer, row.Ticket, row.Coupon, row.Flight)).ToList();
            var baggageFacts = coupons.Select(row => BaggageFacts(row.Ticket, row.Coupon, row.Flight)).ToList();

            return new AncillaryShoppingContext
            {
                OwnerAirlineId = scope.OwnerAirlineId,
                PointOfSaleId = scope.PointOfSaleId,
                CustomerId = scope.CustomerId,
                ShoppingStage = ShoppingStage.PreOrder,
                EvaluatedAtUtc = scope.EvaluatedAtUtc,
                CurrencyId = scope.PreferredCurrencyId ?? offer.CurrencyId,
                SourceIdentity = new SourceIdentityContext { SourceKind = ShoppingSourceKind.Offer, TrustedSourceReference = offer.OfferId },
                Travellers = offer.Tickets
                    .Select(ticket => new ShoppingTraveller { TravellerRef = ticket.TravellerRef, PassengerTypeCode = ticket.PassengerTypeCode })
                    .ToList(),
                Portions = offer.Bounds.Select(Portion).ToList(),
                Flights = offer.Bounds.SelectMany(bound => bound.Flights.Select(flight => Flight(bound, flight, airportCountryIds))).ToList(),
                TravellerFareFacts = fareFacts.OfType<TravellerFareFacts>().ToList(),
                TravellerBaggageFacts = baggageFacts.OfType<TravellerFlightBaggageFacts>().ToList(),
                CoverageCompleteness = new FactCompleteness
                {
                    TravellersComplete = true,
                    ItineraryComplete = true,
                    FareFactsComplete = fareFacts.All(fact => fact is not null),
                    BaggageFactsComplete = coupons.All(row => row.Coupon is { CheckedBaggage: not null, CabinBaggage: not null }),
                    FareBenefitsComplete = false,
                    ExistingServicesComplete = false,
                    Provenance = Provenance
                }
            };
        }

        private static (FlightOfferBound Bound, FlightOfferFlight Flight) FlightOf(FlightOffer offer, FlightOfferCoupon coupon)
        {
            var bound = offer.Bounds.Single(row => row.BoundId == coupon.BoundId);

            return (bound, bound.Flights.Single(flight => flight.FlightId == coupon.FlightId));
        }

        private static ShoppingPortion Portion(FlightOfferBound bound)
            => new()
            {
                PortionRef = bound.BoundId,
                Sequence = bound.Sequence,
                FlightRefs = bound.Flights.Select(flight => FlightRef(bound, flight)).ToList(),
                OriginAirportId = bound.OriginAirportId,
                DestinationAirportId = bound.DestinationAirportId,
                ViaAirportIds = bound.Flights.SkipLast(1).Select(flight => flight.DestinationAirportId).ToList()
            };

        private static ShoppingFlight Flight(FlightOfferBound bound, FlightOfferFlight flight, IReadOnlyDictionary<int, int> airportCountryIds)
            => new()
            {
                FlightRef = FlightRef(bound, flight),
                FlightId = flight.FlightId,
                FlightVersion = flight.FlightVersion,
                FlightNumber = flight.FlightNumber,
                MarketingAirlineId = flight.MarketingAirlineId,
                OperatingAirlineId = flight.OperatingAirlineId,
                OriginAirportId = flight.OriginAirportId,
                DestinationAirportId = flight.DestinationAirportId,
                ViaAirportIds = flight.Legs.SkipLast(1).Select(leg => leg.DestinationAirportId).ToList(),
                OriginCountryId = airportCountryIds.TryGetValue(flight.OriginAirportId, out var origin) ? origin : null,
                DestinationCountryId = airportCountryIds.TryGetValue(flight.DestinationAirportId, out var destination) ? destination : null,
                OriginTerminalId = flight.OriginAirportTerminalId,
                DestinationTerminalId = flight.DestinationAirportTerminalId,
                DepartureAt = flight.DepartureAt,
                ArrivalAt = flight.ArrivalAt,
                AircraftId = flight.AircraftId,
                CabinClassId = flight.CabinClassId,
                RbdId = flight.RbdId
            };

        private static TravellerFareFacts? FareFacts(
            FlightOffer offer,
            FlightOfferTicket ticket,
            FlightOfferCoupon coupon,
            (FlightOfferBound Bound, FlightOfferFlight Flight) sold)
        {
            var fares = offer.FareComponents
                .Where(component => component.BoundId == coupon.BoundId && coupon.AirFareIds.Contains(component.AirFareId))
                .ToList();

            if (coupon.AirFareIds.Count != 1 || fares.Count != 1)
                return null;

            return new TravellerFareFacts
            {
                TravellerRef = ticket.TravellerRef,
                FlightRef = FlightRef(sold.Bound, sold.Flight),
                PortionRef = coupon.BoundId,
                AirFareId = fares[0].AirFareId,
                AirFareType = fares[0].FareType,
                FareFamilyCodeOrName = fares[0].FareFamily,
                FareBasisCode = fares[0].FareBasis,
                CabinClassId = fares[0].CabinClassId,
                RbdId = fares[0].RbdId,
                BookingClass = fares[0].BookingClass
            };
        }

        private static TravellerFlightBaggageFacts? BaggageFacts(
            FlightOfferTicket ticket,
            FlightOfferCoupon coupon,
            (FlightOfferBound Bound, FlightOfferFlight Flight) sold)
            => coupon.CheckedBaggage is null && coupon.CabinBaggage is null
                ? null
                : new TravellerFlightBaggageFacts
                {
                    TravellerRef = ticket.TravellerRef,
                    FlightRef = FlightRef(sold.Bound, sold.Flight),
                    PortionRef = coupon.BoundId,
                    CheckedPieces = coupon.CheckedBaggage?.Pieces,
                    CheckedWeight = coupon.CheckedBaggage?.Weight,
                    CheckedWeightUnit = coupon.CheckedBaggage?.Unit,
                    CabinPieces = coupon.CabinBaggage?.Pieces,
                    CabinWeight = coupon.CabinBaggage?.Weight,
                    CabinWeightUnit = coupon.CabinBaggage?.Unit,
                    SourceCompleteness = FactEvidence.Verified
                };
    }
}
