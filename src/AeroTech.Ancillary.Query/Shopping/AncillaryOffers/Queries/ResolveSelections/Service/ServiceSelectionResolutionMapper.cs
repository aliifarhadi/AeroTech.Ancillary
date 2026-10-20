using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Service;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateSelection;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Results;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.ResolveSelections.Service
{
    public static class ServiceSelectionResolutionMapper
    {
        public static ServiceSelectionResolutionDto Map(SelectionResolutionSnapshot resolution, string? correlationId)
            => new(resolution.OrderId, resolution.OrderCommercialVersion, correlationId, resolution.AllValid, resolution.Results.Select(Result).ToList());

        private static ServiceResolvedSelectionDto Result(ResolvedSelectionSnapshot result)
            => new(
                result.PricedSelectionRef,
                result.Status.ToString(),
                result.ReasonCodes,
                result is { Line: { Candidate: { } candidate } line, Context: { } context } ? Service(line, candidate, context) : null,
                result is { Line.Seat: { IsIncludedSeat: true } seat, Context: { } seatContext } ? IncludedSeat(seat, seatContext) : null);

        private static ServiceResolvedServiceDto Service(SelectionLineSnapshot line, CanonicalAncillaryOfferCandidate candidate, AncillaryShoppingContext context)
            => new(
                line.Kind.ToString(),
                candidate.ServiceDefinitionRef,
                candidate.DefinitionVersionId,
                candidate.ProvisionId,
                candidate.Price.PricingRevisionId,
                candidate.Document.Rfic,
                candidate.Document.Rfisc,
                candidate.Booking.SsrCode,
                candidate.Scope.CoverageScope.ToString(),
                context.Travellers.Where(traveller => candidate.Travellers.Contains(traveller.TravellerRef)).Select(traveller => traveller.OrderTravellerId).OfType<long>().ToList(),
                candidate.Travellers,
                context.Flights.Where(flight => candidate.FlightRefs.Contains(flight.FlightRef)).Select(flight => flight.FlightId).ToList(),
                candidate.PortionRefs,
                line.Seat?.Offer.SeatNumber,
                line.Seat?.Flight.Flight.FlightCapacityId,
                line.Ancillary is { } ancillary ? OfferSelectionResultsMapper.SelectedOptions(ancillary) : [],
                OfferSelectionResultsMapper.ExactPrice(candidate.Price, line.CanPurchase),
                candidate.Availability.RequiresCheckAtReserve);

        private static ServiceIncludedSeatDto IncludedSeat(SeatSelectionSnapshot seat, AncillaryShoppingContext context)
            => new(
                context.Travellers.FirstOrDefault(traveller => traveller.TravellerRef == seat.Offer.TravellerRef)?.OrderTravellerId,
                seat.Offer.TravellerRef,
                seat.Flight.Flight.FlightId,
                seat.Flight.Flight.FlightCapacityId,
                seat.Offer.SeatNumber,
                seat.CurrencyId,
                decimal.Zero);
    }
}
