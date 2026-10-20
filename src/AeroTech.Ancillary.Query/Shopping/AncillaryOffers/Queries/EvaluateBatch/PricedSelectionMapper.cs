using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.Shopping.Common.PricedSelections;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateBatch
{
    public static class PricedSelectionMapper
    {
        public static PricedSelection Of(SelectionLineSnapshot line, AncillarySalesScope scope, OrderBinding binding)
        {
            if (line.Ancillary is { } ancillary)
            {
                var candidate = ancillary.Evaluation.Candidate;

                return new PricedSelection
                {
                    SourceKind = ShoppingSourceKind.Order,
                    OrderId = binding.OrderId,
                    OrderCommercialVersion = binding.OrderCommercialVersion,
                    SourceVersion = binding.SourceVersion,
                    Scope = scope,
                    Kind = SelectionLineKind.Ancillary,
                    OfferItemRef = ancillary.CurrentOfferItemRef,
                    Selection = ancillary.Selection,
                    CoverageScope = candidate.Scope.CoverageScope,
                    TravellerRefs = candidate.Travellers,
                    FlightRefs = candidate.FlightRefs,
                    PortionRefs = candidate.PortionRefs,
                    ServiceDefinitionRef = candidate.ServiceDefinitionRef,
                    DefinitionVersionId = candidate.DefinitionVersionId,
                    ProvisionId = candidate.ProvisionId,
                    PricingRevisionId = candidate.Price.PricingRevisionId,
                    CurrencyId = candidate.Price.CurrencyId!.Value,
                    Total = candidate.Price.RequestedQuantityTotal!.Value,
                    Units = candidate.Price.Units
                };
            }

            var seat = line.Seat!;
            var paid = seat.Offer.Candidate;

            return new PricedSelection
            {
                SourceKind = ShoppingSourceKind.Order,
                OrderId = binding.OrderId,
                OrderCommercialVersion = binding.OrderCommercialVersion,
                SourceVersion = binding.SourceVersion,
                Scope = scope,
                Kind = SelectionLineKind.SeatMap,
                SeatMapSelectionId = seat.SeatMapSelectionId,
                Seat = new PricedSeat(seat.Offer.TravellerRef, seat.Flight.FlightRef, seat.Flight.Flight.FlightId, seat.Flight.Flight.FlightCapacityId, seat.Offer.SeatNumber),
                CoverageScope = ServiceCoverageScope.Sector,
                TravellerRefs = [seat.Offer.TravellerRef],
                FlightRefs = [seat.Flight.FlightRef],
                PortionRefs = paid?.PortionRefs ?? [],
                ServiceDefinitionRef = paid?.ServiceDefinitionRef,
                DefinitionVersionId = paid?.DefinitionVersionId,
                ProvisionId = paid?.ProvisionId,
                PricingRevisionId = paid?.Price.PricingRevisionId,
                CurrencyId = paid?.Price.CurrencyId ?? seat.CurrencyId,
                Total = seat.Total!.Value,
                Units = paid?.Price.Units ?? 1
            };
        }
    }
}
