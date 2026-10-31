using AeroTech.Ancillary.Domain.AncillaryPricingAggregate;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Engine;

namespace AeroTech.Ancillary.Shopping.Options
{
    public sealed class SelectionProofBasket
    {
        private readonly IReadOnlyList<AncillaryPricing> _pricings;
        private readonly IReadOnlyDictionary<long, AncillaryProvision> _provisions;
        private readonly IReadOnlyDictionary<long, AncillaryServiceDefinition> _definitions;

        internal SelectionProofBasket(
            IReadOnlyList<AncillaryShoppingContext> contexts,
            IReadOnlyList<AncillaryPricing> pricings,
            IReadOnlyList<AncillaryProvision> provisions,
            IReadOnlyList<AncillaryServiceDefinition> definitions)
        {
            Contexts = contexts;
            Definitions = definitions;
            _pricings = pricings;
            _provisions = provisions.ToDictionary(provision => provision.Id);
            _definitions = definitions.ToDictionary(definition => definition.Id);
        }

        internal IReadOnlyList<AncillaryShoppingContext> Contexts { get; }

        internal IReadOnlyList<AncillaryServiceDefinition> Definitions { get; }

        internal Dictionary<string, ISelectionEvaluator> Evaluators { get; } = new(StringComparer.Ordinal);

        internal AncillaryPricing? PricingOf(long pricingRateId)
            => _pricings.FirstOrDefault(pricing => pricing.Rates.Any(rate => rate.Id == pricingRateId));

        internal AncillaryProvision? ProvisionOf(long provisionId) => _provisions.GetValueOrDefault(provisionId);

        internal AncillaryServiceDefinition? DefinitionOf(long serviceDefinitionId) => _definitions.GetValueOrDefault(serviceDefinitionId);
    }
}
