using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Snapshots;
using AeroTech.Ancillary.Shopping.Reading;
using AeroTech.Ancillary.Shopping.Resolution;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.ResolveSeatMapSelections
{
    public sealed class ResolveSeatMapSelectionsService : IResolveSeatMapSelectionsService
    {
        private readonly ISelectionResolver _selections;
        private readonly IActiveAncillaryPricingReader _pricings;
        private readonly IActiveAncillaryProvisionReader _provisions;
        private readonly IActiveAncillaryDefinitionReader _definitions;

        public ResolveSeatMapSelectionsService(
            ISelectionResolver selections,
            IActiveAncillaryPricingReader pricings,
            IActiveAncillaryProvisionReader provisions,
            IActiveAncillaryDefinitionReader definitions)
        {
            _selections = selections;
            _pricings = pricings;
            _provisions = provisions;
            _definitions = definitions;
        }

        public async Task<SeatMapResolutionSnapshot> ExecuteAsync(string? selectionId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(selectionId))
                throw ExceptionFactory.ShoppingContextIsInvalid("selectionId is required");

            var snapshot = await _selections.OpenAsync(selectionId, cancellationToken);
            var lines = snapshot.Lines.OrderBy(line => line.Sequence).ToList();
            var rates = (await _pricings.FindActiveByRateIdsAsync(lines.Select(line => line.PricingRateId).Distinct().ToList(), cancellationToken))
                .SelectMany(pricing => pricing.Rates.Select(rate => (Pricing: pricing, Rate: rate)))
                .ToDictionary(owner => owner.Rate.Id);
            var provisions = (await _provisions.FindActiveProvisionsAsync(rates.Values.Select(owner => owner.Pricing.AncillaryProvisionId).Distinct().ToList(), cancellationToken))
                .ToDictionary(provision => provision.Id);
            var definitions = (await _definitions.FindActiveDefinitionsAsync(provisions.Values.Select(provision => provision.ServiceDefinitionId).Distinct().ToList(), cancellationToken))
                .ToDictionary(definition => definition.Id);

            return new SeatMapResolutionSnapshot(
                snapshot.OrderId,
                snapshot.OrderCommercialVersion,
                snapshot.ExpiresAtUtc,
                lines
                    .Select(line => SeatMapSelectionId.Decode(line.OfferItemId) is { } selection
                                    && rates.TryGetValue(line.PricingRateId, out var owner)
                                    && provisions.GetValueOrDefault(owner.Pricing.AncillaryProvisionId) is { } provision
                                    && definitions.GetValueOrDefault(provision.ServiceDefinitionId) is { Profile: AncillaryProfile.Seat } definition
                        ? new ResolvedSeatSnapshot(line.OfferItemId, selection, definition, provision, owner.Pricing, owner.Rate)
                        : throw ExceptionFactory.OfferItemWasNotFound($"selection {selectionId} line {line.Sequence} names no active seat pricing"))
                    .ToList());
        }
    }
}
