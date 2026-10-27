using AeroTech.Ancillary.Domain.AncillaryPricingAggregate;
using AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Entities;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Domain.SelectionSnapshotAggregate;
using AeroTech.Ancillary.Domain.SelectionSnapshotAggregate.Entities;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Shopping.Engine;
using AeroTech.Ancillary.Shopping.Offers;
using AeroTech.Ancillary.Shopping.Reading;
using AeroTech.Ancillary.Shopping.Selections;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Shopping.Resolution
{
    public sealed class SelectionResolver : ISelectionResolver
    {
        private readonly ISelectionSnapshotReader _snapshots;
        private readonly IActiveAncillaryPricingReader _pricings;
        private readonly IActiveAncillaryProvisionReader _provisions;
        private readonly IActiveAncillaryDefinitionReader _definitions;
        private readonly IClock _clock;

        public SelectionResolver(
            ISelectionSnapshotReader snapshots,
            IActiveAncillaryPricingReader pricings,
            IActiveAncillaryProvisionReader provisions,
            IActiveAncillaryDefinitionReader definitions,
            IClock clock)
        {
            _snapshots = snapshots;
            _pricings = pricings;
            _provisions = provisions;
            _definitions = definitions;
            _clock = clock;
        }

        public Task<SelectionSnapshot> OpenAsync(string selectionId, ShoppingSourceKind source, CancellationToken cancellationToken = default)
            => FindOpenAsync(selectionId, source, cancellationToken);

        public async Task<ResolvedSelections> ResolveAsync(string selectionId, CancellationToken cancellationToken = default)
        {
            var snapshot = await FindOpenAsync(selectionId, null, cancellationToken);
            var lines = snapshot.Lines.OrderBy(line => line.Sequence).ToList();
            var rates = (await _pricings.FindActiveByRateIdsAsync(lines.Select(line => line.PricingRateId).Distinct().ToList(), cancellationToken))
                .SelectMany(pricing => pricing.Rates.Select(rate => (Pricing: pricing, Rate: rate)))
                .ToDictionary(owner => owner.Rate.Id);
            var provisions = (await _provisions.FindActiveProvisionsAsync(rates.Values.Select(owner => owner.Pricing.AncillaryProvisionId).Distinct().ToList(), cancellationToken))
                .ToDictionary(provision => provision.Id);
            var definitions = (await _definitions.FindActiveDefinitionsAsync(provisions.Values.Select(provision => provision.ServiceDefinitionId).Distinct().ToList(), cancellationToken))
                .ToDictionary(definition => definition.Id);
            var selections = new List<ResolvedSelection>(lines.Count);

            foreach (var line in lines)
            {
                var selection = Resolve(line, rates, provisions, definitions);

                if (SelectionBasketRules.Conflict(selections.Select(BasketLine).ToList(), BasketLine(selection)) is { } conflict)
                    throw ExceptionFactory.SelectionIsNotSellable($"selections[{line.Sequence - 1}]: {conflict}");

                selections.Add(selection);
            }

            return new ResolvedSelections(snapshot, selections);
        }

        private async Task<SelectionSnapshot> FindOpenAsync(string selectionId, ShoppingSourceKind? source, CancellationToken cancellationToken)
        {
            var snapshot = await _snapshots.FindAsync(selectionId, cancellationToken);

            if (snapshot is null || (source is { } kind && snapshot.SourceKind != kind))
                throw ExceptionFactory.SelectionWasNotFound($"selection {selectionId} does not exist");

            if (snapshot.HasExpired(_clock.GetDateTime()))
                throw ExceptionFactory.SelectionHasExpired();

            return snapshot;
        }

        private static SelectionBasketLine BasketLine(ResolvedSelection selection)
            => new(selection.Rate.Id, selection.CoverageScope, selection.TravellerRef, selection.CoverageRef, selection.Definition);

        private static ResolvedSelection Resolve(
            SelectionSnapshotLine line,
            IReadOnlyDictionary<long, (AncillaryPricing Pricing, AncillaryPricingRate Rate)> rates,
            IReadOnlyDictionary<long, AncillaryProvision> provisions,
            IReadOnlyDictionary<long, AncillaryServiceDefinition> definitions)
        {
            if (!rates.TryGetValue(line.PricingRateId, out var owner))
                throw ExceptionFactory.SelectionIsNotSellable($"selections[{line.Sequence - 1}]: the price is no longer active");

            var provision = provisions.GetValueOrDefault(owner.Pricing.AncillaryProvisionId)
                            ?? throw ExceptionFactory.SelectionIsNotSellable($"selections[{line.Sequence - 1}]: the product rule is no longer active");
            var definition = definitions.GetValueOrDefault(provision.ServiceDefinitionId)
                             ?? throw ExceptionFactory.SelectionIsNotSellable($"selections[{line.Sequence - 1}]: the product is no longer active");

            if (provision.CoverageScope != line.CoverageScope)
                throw ExceptionFactory.SelectionIsNotSellable($"selections[{line.Sequence - 1}]: the unit does not match the product rule");

            if (definition.Profile == AncillaryProfile.Seat || CustomerInputs.AreRequested(definition, provision) != (line.SelectedJson is not null))
                throw ExceptionFactory.SelectionIsNotSellable($"selections[{line.Sequence - 1}]: the selection kind does not match the product");

            if (owner.Pricing.PricingUnit is not { } pricingUnit)
                throw ExceptionFactory.SelectionIsNotSellable($"selections[{line.Sequence - 1}]: the price has no pricing unit");

            var unitAmount = owner.Rate.UnitTotal.Amount;

            return new ResolvedSelection(
                line.OfferItemId,
                SelectionScopes.Of(line.CoverageScope),
                line.CoverageScope,
                line.TravellerRef ?? string.Empty,
                line.CoverageRef,
                line.SourceBinding,
                line.SalesBinding,
                definition,
                provision,
                owner.Pricing,
                owner.Rate,
                pricingUnit,
                line.Quantity,
                unitAmount,
                unitAmount * line.Quantity,
                provision.PetRule?.AcceptanceMode ?? definition.Booking.ConfirmationRequirement,
                AvailabilityAssessor.MustCheckAtReserve(definition, provision),
                line.SelectedJson);
        }
    }
}
