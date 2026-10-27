using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Domain.SelectionSnapshotAggregate;
using AeroTech.Ancillary.Domain.SelectionSnapshotAggregate.Entities;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Engine;
using AeroTech.Ancillary.Shopping.Options;
using AeroTech.Ancillary.Shopping.Reading;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Ancillary.Shopping.Selections;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Shopping.Offers
{
    public sealed class AncillaryOfferListing : IAncillaryOfferListing
    {
        private const int SingleUnit = 1;

        private readonly IAncillaryShoppingEngine _engine;
        private readonly IActiveAncillaryDefinitionReader _definitions;
        private readonly IActiveAncillaryProvisionReader _provisions;
        private readonly ISelectionSigner _signer;

        public AncillaryOfferListing(
            IAncillaryShoppingEngine engine,
            IActiveAncillaryDefinitionReader definitions,
            IActiveAncillaryProvisionReader provisions,
            ISelectionSigner signer)
        {
            _engine = engine;
            _definitions = definitions;
            _provisions = provisions;
            _signer = signer;
        }

        public async Task<IReadOnlyList<AncillaryOfferRow>> ListAsync(
            IReadOnlyList<AncillaryShoppingContext> contexts,
            ShoppingFilter filter,
            SelectionSnapshot? selection,
            CancellationToken cancellationToken = default)
        {
            var lines = selection?.Lines.ToList() ?? [];

            EnsureBound(lines, contexts);

            var rows = new List<AncillaryOfferRow>();
            var selected = new HashSet<string>(StringComparer.Ordinal);

            foreach (var context in contexts)
            {
                var candidates = (await _engine.ShopAsync(context, filter, cancellationToken)).Candidates;

                if (candidates.Count == 0)
                    continue;

                var definitions = (await _definitions.FindActiveDefinitionsAsync(candidates.Select(candidate => candidate.DefinitionVersionId).Distinct().ToList(), cancellationToken))
                    .ToDictionary(definition => definition.Id);
                var provisions = (await _provisions.FindActiveProvisionsAsync(candidates.Select(candidate => candidate.ProvisionId).Distinct().ToList(), cancellationToken))
                    .ToDictionary(provision => provision.Id);
                var salesBinding = SelectionBindings.Sales(context);

                foreach (var candidate in candidates)
                {
                    if (!definitions.TryGetValue(candidate.DefinitionVersionId, out var definition)
                        || !provisions.TryGetValue(candidate.ProvisionId, out var provision)
                        || Row(context, candidate, definition, provision) is not { } row)
                        continue;

                    if (lines.FirstOrDefault(line => line.SalesBinding == salesBinding && Offers(row, line)) is { } line && selected.Add(line.OfferItemId))
                        row = row with { Selected = line };

                    rows.Add(row);
                }
            }

            if (selected.Count != lines.Count)
                throw ExceptionFactory.SelectionIsNotSellable("a selection is no longer offered; ask for the ancillary offers again");

            return rows;
        }

        private static void EnsureBound(IReadOnlyList<SelectionSnapshotLine> lines, IReadOnlyList<AncillaryShoppingContext> contexts)
        {
            foreach (var line in lines)
            {
                var context = contexts.FirstOrDefault(candidate => SelectionBindings.Sales(candidate) == line.SalesBinding)
                              ?? throw ExceptionFactory.SelectionBindingMismatch();

                if (SelectionBindings.Source(context.SourceIdentity) is not { } binding || binding != line.SourceBinding)
                    throw ExceptionFactory.SelectionIsNotSellable("the offer or order changed; ask for the ancillary offers again");
            }
        }

        private static bool Offers(AncillaryOfferRow row, SelectionSnapshotLine line)
        {
            var identity = row.Candidate.CandidateIdentity;

            return row.Candidate.Price.PricingRateId == line.PricingRateId
                   && identity.CoverageScope == line.CoverageScope
                   && identity.TravellerRef == (line.TravellerRef ?? string.Empty)
                   && identity.CoverageRef == line.CoverageRef
                   && (line.SelectedJson is null
                       ? row.Options is null && line.Quantity >= row.QuantityPolicy.Min && line.Quantity <= row.QuantityPolicy.Max
                       : row.Options is not null);
        }

        private AncillaryOfferRow? Row(
            AncillaryShoppingContext context,
            CanonicalAncillaryOfferCandidate candidate,
            AncillaryServiceDefinition definition,
            AncillaryProvision provision)
        {
            if (!CustomerInputs.FixedChoicesAreDefined(definition) || candidate.Price.CompleteUnitTotal is not { } unitAmount)
                return null;

            var limits = QuantityPolicyOf(candidate);
            var quantities = Enumerable.Range(limits.Min, limits.Max - limits.Min + 1).TakeWhile(quantity => Allows(candidate.Quantity, quantity)).ToList();

            if (quantities.Count == 0)
                return null;

            var policy = limits with { Selectable = quantities.Count > 1, Min = quantities[0], Max = quantities[^1] };
            var row = new AncillaryOfferRow(candidate, definition, provision, SelectionScopes.Of(candidate.CandidateIdentity.CoverageScope), unitAmount, policy);

            if (CustomerInputs.AreRequested(definition, provision))
                return _signer.SignReference(context, candidate) is { } reference
                    ? row with { OfferItemId = reference, Options = TypedSelectionForm.Fields(definition, provision, context.Travellers, candidate.CandidateIdentity.TravellerRef) }
                    : null;

            return _signer.SignDirect(context, candidate, provision, policy.Min, policy.Max) is { } direct ? row with { OfferItemId = direct } : null;
        }

        private static AncillaryQuantityPolicy QuantityPolicyOf(CanonicalAncillaryOfferCandidate candidate)
        {
            var quantity = candidate.Quantity;

            return candidate.Selection.Kind == SelectionKind.QuantityChoice
                ? new AncillaryQuantityPolicy(quantity.MaxPerSelection > quantity.MinPerSelection, quantity.MinPerSelection, quantity.MaxPerSelection, quantity.Unit)
                : new AncillaryQuantityPolicy(false, SingleUnit, SingleUnit, quantity.Unit);
        }

        private static bool Allows(QuantityAssessment assessment, int quantity)
            => quantity >= assessment.MinPerSelection
               && quantity <= assessment.MaxPerSelection
               && assessment.CumulativeLimits.All(limit => limit.VerifiedRemaining is { } remaining && Requested(limit, quantity) <= remaining);

        private static decimal? Requested(CumulativeLimitAssessment limit, int quantity)
            => limit.ConsumptionUnit == UsageConsumptionUnit.Kilogram ? quantity * limit.UnitsPerPurchase : quantity;
    }
}
