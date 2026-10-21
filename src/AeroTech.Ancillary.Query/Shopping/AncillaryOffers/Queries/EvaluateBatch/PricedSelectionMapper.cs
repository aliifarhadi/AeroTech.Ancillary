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
            var ancillary = line.Ancillary!;
            var candidate = ancillary.Evaluation.Candidate;

            return new PricedSelection
            {
                SourceKind = ShoppingSourceKind.Order,
                OrderId = binding.OrderId,
                OrderCommercialVersion = binding.OrderCommercialVersion,
                SourceVersion = binding.SourceVersion,
                Scope = scope,
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
    }
}
