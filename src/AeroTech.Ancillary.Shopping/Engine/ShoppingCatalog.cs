using AeroTech.Ancillary.Domain.AncillaryPricingAggregate;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Shopping.Reading;

namespace AeroTech.Ancillary.Shopping.Engine
{
    internal sealed class ShoppingCatalog
    {
        private readonly ILookup<long, AncillaryProvision> _provisions;
        private readonly ILookup<long, AncillaryPricing> _pricings;
        private readonly IReadOnlyDictionary<string, InventoryConfiguration> _inventory;

        public ShoppingCatalog(
            IReadOnlyList<AncillaryServiceDefinition> definitions,
            IEnumerable<AncillaryProvision> provisions,
            IEnumerable<AncillaryPricing> pricings,
            IEnumerable<InventoryConfiguration> inventory,
            IReadOnlyDictionary<int, int> currencyDecimalPlaces,
            InventoryReferenceProbe references)
        {
            Definitions = definitions;
            _provisions = provisions.ToLookup(provision => provision.ServiceDefinitionId);
            _pricings = pricings.ToLookup(pricing => pricing.AncillaryProvisionId);
            _inventory = inventory
                .GroupBy(configuration => configuration.Policy.ServiceDefinitionRef, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
            CurrencyDecimalPlaces = currencyDecimalPlaces;
            References = references;
        }

        public IReadOnlyList<AncillaryServiceDefinition> Definitions { get; }

        public IReadOnlyDictionary<int, int> CurrencyDecimalPlaces { get; }

        public InventoryReferenceProbe References { get; }

        public IReadOnlyList<AncillaryProvision> ProvisionsOf(long serviceDefinitionId)
            => _provisions[serviceDefinitionId].OrderBy(provision => provision.Sequence).ThenBy(provision => provision.Id).ToList();

        public AncillaryPricing? PricingOf(long ancillaryProvisionId)
        {
            var active = _pricings[ancillaryProvisionId].ToList();

            return active.Count == 1 ? active[0] : null;
        }

        public InventoryConfiguration? InventoryOf(string serviceDefinitionRef) => _inventory.GetValueOrDefault(serviceDefinitionRef);
    }
}
