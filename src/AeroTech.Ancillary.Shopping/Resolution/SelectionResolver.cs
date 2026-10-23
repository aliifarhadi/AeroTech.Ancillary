using AeroTech.Ancillary.Domain.AncillaryPricingAggregate;
using AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Entities;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Domain.SelectionSnapshotAggregate;
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
        private const string SnapshotPrefix = "s_";

        private readonly ISelectionIdCodec _codec;
        private readonly ISelectionSnapshotReader _snapshots;
        private readonly IActiveAncillaryPricingReader _pricings;
        private readonly IActiveAncillaryProvisionReader _provisions;
        private readonly IActiveAncillaryDefinitionReader _definitions;
        private readonly IClock _clock;

        public SelectionResolver(
            ISelectionIdCodec codec,
            ISelectionSnapshotReader snapshots,
            IActiveAncillaryPricingReader pricings,
            IActiveAncillaryProvisionReader provisions,
            IActiveAncillaryDefinitionReader definitions,
            IClock clock)
        {
            _codec = codec;
            _snapshots = snapshots;
            _pricings = pricings;
            _provisions = provisions;
            _definitions = definitions;
            _clock = clock;
        }

        public async Task<IReadOnlyList<ResolvedSelection>> ResolveAsync(IReadOnlyList<string> ids, CancellationToken cancellationToken = default)
        {
            var now = _clock.GetDateTime();
            var snapshots = (await _snapshots.FindAsync(ids.Where(IsSnapshot).ToList(), cancellationToken)).ToDictionary(snapshot => snapshot.Id, StringComparer.Ordinal);
            var units = new List<SelectedUnit>();

            foreach (var id in ids)
                units.Add(IsSnapshot(id) ? FromSnapshot(id, snapshots, now) : await FromSignedAsync(id));

            var rates = (await _pricings.FindActiveByRateIdsAsync(units.Select(unit => unit.PricingRateId).Distinct().ToList(), cancellationToken))
                .SelectMany(pricing => pricing.Rates.Select(rate => (Pricing: pricing, Rate: rate)))
                .ToDictionary(owner => owner.Rate.Id);
            var provisions = (await _provisions.FindActiveProvisionsAsync(rates.Values.Select(owner => owner.Pricing.AncillaryProvisionId).Distinct().ToList(), cancellationToken))
                .ToDictionary(provision => provision.Id);
            var definitions = (await _definitions.FindActiveDefinitionsAsync(provisions.Values.Select(provision => provision.ServiceDefinitionId).Distinct().ToList(), cancellationToken))
                .ToDictionary(definition => definition.Id);

            return units.Select(unit => Resolve(unit, rates, provisions, definitions)).ToList();
        }

        private static bool IsSnapshot(string id) => id.StartsWith(SnapshotPrefix, StringComparison.Ordinal);

        private static SelectedUnit FromSnapshot(string id, IReadOnlyDictionary<string, SelectionSnapshot> snapshots, DateTimeOffset now)
        {
            var snapshot = snapshots.GetValueOrDefault(id) ?? throw ExceptionFactory.SelectionWasNotFound($"selection {id} does not exist");

            if (now >= snapshot.ExpiresAtUtc)
                throw ExceptionFactory.SelectionHasExpired();

            return new SelectedUnit(
                id,
                snapshot.PricingRateId,
                snapshot.CoverageScope,
                snapshot.TravellerRef ?? string.Empty,
                snapshot.CoverageRef,
                snapshot.OfferBinding,
                snapshot.SalesBinding,
                snapshot.Quantity,
                snapshot.SelectedJson);
        }

        private async Task<SelectedUnit> FromSignedAsync(string id)
            => await _codec.ReadAsync(id, SelectionIdKind.Direct) switch
            {
                { Status: SignedSelectionStatus.Valid, Selection: { Quantity: { } quantity } selection } => new SelectedUnit(
                    id,
                    selection.PricingRateId,
                    selection.CoverageScope,
                    selection.TravellerRef,
                    selection.CoverageRef,
                    selection.OfferBinding,
                    selection.SalesBinding,
                    quantity,
                    null),
                { Status: SignedSelectionStatus.Expired } => throw ExceptionFactory.SelectionHasExpired(),
                _ => throw ExceptionFactory.SelectionSignatureIsInvalid()
            };

        private static ResolvedSelection Resolve(
            SelectedUnit unit,
            IReadOnlyDictionary<long, (AncillaryPricing Pricing, AncillaryPricingRate Rate)> rates,
            IReadOnlyDictionary<long, AncillaryProvision> provisions,
            IReadOnlyDictionary<long, AncillaryServiceDefinition> definitions)
        {
            if (!rates.TryGetValue(unit.PricingRateId, out var owner))
                throw ExceptionFactory.SelectionIsNotSellable($"selection {unit.SelectionId}: the price is no longer active");

            var provision = provisions.GetValueOrDefault(owner.Pricing.AncillaryProvisionId)
                            ?? throw ExceptionFactory.SelectionIsNotSellable($"selection {unit.SelectionId}: the product rule is no longer active");
            var definition = definitions.GetValueOrDefault(provision.ServiceDefinitionId)
                             ?? throw ExceptionFactory.SelectionIsNotSellable($"selection {unit.SelectionId}: the product is no longer active");

            if (provision.CoverageScope != unit.CoverageScope)
                throw ExceptionFactory.SelectionIsNotSellable($"selection {unit.SelectionId}: the unit does not match the product rule");

            if (definition.Profile == AncillaryProfile.Seat || CustomerInputs.AreRequested(definition, provision) != (unit.SelectedJson is not null))
                throw ExceptionFactory.SelectionIsNotSellable($"selection {unit.SelectionId}: the selection kind does not match the product");

            if (owner.Pricing.PricingUnit is not { } pricingUnit)
                throw ExceptionFactory.SelectionIsNotSellable($"selection {unit.SelectionId}: the price has no pricing unit");

            var unitAmount = owner.Rate.UnitTotal.Amount;

            return new ResolvedSelection(
                unit.SelectionId,
                SelectionScopes.Of(unit.CoverageScope),
                unit.CoverageScope,
                unit.TravellerRef,
                unit.CoverageRef,
                unit.OfferBinding,
                unit.SalesBinding,
                definition,
                provision,
                owner.Pricing,
                owner.Rate,
                pricingUnit,
                unit.Quantity,
                unitAmount,
                unitAmount * unit.Quantity,
                provision.PetRule?.AcceptanceMode ?? definition.Booking.ConfirmationRequirement,
                AvailabilityAssessor.MustCheckAtReserve(definition, provision),
                unit.SelectedJson);
        }

        private sealed record SelectedUnit(
            string SelectionId,
            long PricingRateId,
            ServiceCoverageScope CoverageScope,
            string TravellerRef,
            string CoverageRef,
            string OfferBinding,
            string SalesBinding,
            int Quantity,
            string? SelectedJson);
    }
}
