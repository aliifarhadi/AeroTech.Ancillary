using AeroTech.Ancillary.Application.FlightOffers;
using AeroTech.Ancillary.Query.AncillaryOffers.Dto.OtaPanel;
using AeroTech.Ancillary.Query.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers.OtaPanel
{
    public static class OtaPanelAncillaryOffersMapper
    {
        public static string? DisplayStatusOf(OfferReadiness readiness)
            => readiness switch
            {
                OfferReadiness.Selectable => "AvailableForSelection",
                OfferReadiness.NeedsSelection => "NeedsInput",
                OfferReadiness.NeedsQuote => "QuoteRequired",
                _ => null
            };

        public static OtaPanelAncillaryOffersDto Map(AncillaryShoppingSnapshot snapshot)
        {
            var offer = snapshot.Offer;
            var flightRefs = offer.Bounds.SelectMany(bound => bound.Flights.Select(flight => FlightOfferShoppingContextMapper.FlightRef(bound, flight))).ToList();
            var shown = snapshot.Items.Where(item => DisplayStatusOf(item.Candidate.OfferReadiness) is not null).ToList();

            return new OtaPanelAncillaryOffersDto(
                offer.OfferId,
                snapshot.CurrencyId,
                snapshot.CurrencyId == offer.CurrencyId ? offer.CurrencyCode : null,
                offer.Tickets
                    .Select(ticket => new OtaPanelTravellerGroupDto(
                        ticket.TravellerRef,
                        ticket.PassengerTypeCode.ToString(),
                        offer.Bounds.SelectMany(bound => bound.Flights.Select(flight => FlightGroup(snapshot, ticket, bound, flight, flightRefs, shown))).ToList()))
                    .ToList(),
                snapshot.Items.Count() - shown.Count,
                RatesOfExchange(snapshot));
        }

        public static IReadOnlyList<OtaPanelRateOfExchangeDto> RatesOfExchange(AncillaryShoppingSnapshot snapshot)
            => snapshot.PointsOfSale
                .SelectMany(pointOfSale => pointOfSale.Result.RatesOfExchange)
                .DistinctBy(rate => rate.RateOfExchangeTableId, StringComparer.Ordinal)
                .OrderBy(rate => rate.FromCurrencyId)
                .Select(rate => new OtaPanelRateOfExchangeDto(rate.RateOfExchangeTableId, rate.FromCurrencyId, rate.ToCurrencyId, rate.Rate))
                .ToList();

        public static OtaPanelServiceDto Service(string offerItemRef, CanonicalAncillaryOfferCandidate candidate)
            => new(
                offerItemRef,
                candidate.Profile.ToString(),
                candidate.VariantCode,
                candidate.CommercialName,
                candidate.Description,
                candidate.Scope.CoverageScope.ToString(),
                candidate.FlightRefs,
                candidate.Price.Status.ToString(),
                candidate.Price.PricingUnit?.ToString(),
                Money(candidate.Price.CompleteUnitTotal, candidate.Price.CurrencyId),
                candidate.Price.RateOfExchange?.RateOfExchangeTableId,
                candidate.Price.UnappliedFeeLines.Select(Fee).ToList(),
                candidate.Quantity.Unit.ToString(),
                candidate.Quantity.MinPerSelection,
                candidate.Quantity.MaxPerSelection,
                candidate.Selection.Fields.Where(field => field.Required).Select(field => field.Name).ToList(),
                DisplayStatusOf(candidate.OfferReadiness) ?? candidate.OfferReadiness.ToString(),
                !candidate.Availability.IsGuaranteed,
                false);

        public static OtaPanelMoneyDto? Money(decimal? amount, int? currencyId)
            => amount is { } value && currencyId is { } currency ? new OtaPanelMoneyDto(value, currency) : null;

        public static OtaPanelFeeDto Fee(PriceComponentLine line) => new(line.Code, line.Amount, line.CurrencyId, line.FeeApplicationUnit?.ToString());

        private static OtaPanelFlightGroupDto FlightGroup(
            AncillaryShoppingSnapshot snapshot,
            FlightOfferTicket ticket,
            FlightOfferBound bound,
            FlightOfferFlight flight,
            IReadOnlyList<string> flightRefs,
            IReadOnlyList<AncillaryOfferItem> shown)
        {
            var flightRef = FlightOfferShoppingContextMapper.FlightRef(bound, flight);
            var baggage = snapshot.Facts.Baggage.SingleOrDefault(fact => fact.TravellerRef == ticket.TravellerRef && fact.FlightRef == flightRef);

            return new OtaPanelFlightGroupDto(
                flightRef,
                bound.BoundId,
                flight.FlightNumber,
                flight.OriginAirportId,
                flight.DestinationAirportId,
                flight.DepartureAt,
                baggage is null ? null : new OtaPanelIncludedBaggageDto(baggage.CheckedPieces, baggage.CheckedWeight, baggage.CheckedWeightUnit?.ToString()),
                baggage is null ? null : new OtaPanelIncludedBaggageDto(baggage.CabinPieces, baggage.CabinWeight, baggage.CabinWeightUnit?.ToString()),
                shown
                    .Where(item => item.Candidate.CandidateIdentity.TravellerRef == ticket.TravellerRef
                                   && flightRefs.First(candidateFlight => item.Candidate.FlightRefs.Contains(candidateFlight)) == flightRef)
                    .OrderBy(item => item.Candidate.Profile)
                    .ThenBy(item => item.Candidate.ServiceDefinitionRef, StringComparer.Ordinal)
                    .ThenBy(item => item.PointOfSaleId)
                    .Select(item => Service(item.OfferItemRef, item.Candidate))
                    .ToList());
        }
    }
}
