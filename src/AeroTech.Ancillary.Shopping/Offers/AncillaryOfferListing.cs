using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Engine;
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
            CancellationToken cancellationToken = default)
        {
            var rows = new List<AncillaryOfferRow>();

            foreach (var context in contexts)
            {
                var candidates = (await _engine.ShopAsync(context, filter, cancellationToken)).Candidates;

                if (candidates.Count == 0)
                    continue;

                var definitions = (await _definitions.FindActiveDefinitionsAsync(candidates.Select(candidate => candidate.DefinitionVersionId).Distinct().ToList(), cancellationToken))
                    .ToDictionary(definition => definition.Id);
                var provisions = (await _provisions.FindActiveProvisionsAsync(candidates.Select(candidate => candidate.ProvisionId).Distinct().ToList(), cancellationToken))
                    .ToDictionary(provision => provision.Id);

                foreach (var candidate in candidates)
                {
                    if (definitions.TryGetValue(candidate.DefinitionVersionId, out var definition)
                        && provisions.TryGetValue(candidate.ProvisionId, out var provision)
                        && Row(context, candidate, definition, provision) is { } row)
                        rows.Add(row);
                }
            }

            return rows;
        }

        private AncillaryOfferRow? Row(
            AncillaryShoppingContext context,
            CanonicalAncillaryOfferCandidate candidate,
            AncillaryServiceDefinition definition,
            AncillaryProvision provision)
        {
            if (!CustomerInputs.FixedChoicesAreDefined(definition) || candidate.Price.CompleteUnitTotal is not { } unitAmount)
                return null;

            var policy = QuantityPolicyOf(candidate);
            var quantities = Enumerable.Range(policy.Min, policy.Max - policy.Min + 1).Where(quantity => Allows(candidate.Quantity, quantity)).ToList();

            if (quantities.Count == 0)
                return null;

            var row = new AncillaryOfferRow(candidate, definition, provision, SelectionScopes.Of(candidate.CandidateIdentity.CoverageScope), unitAmount, policy);

            if (CustomerInputs.AreRequested(definition, provision))
                return _signer.SignReference(context, candidate) is { } reference ? row with { Reference = reference } : null;

            if (!policy.Selectable)
                return _signer.SignDirect(context, candidate, provision, policy.Min) is { } selectionId ? row with { SelectionId = selectionId } : null;

            var options = quantities
                .Select(quantity => _signer.SignDirect(context, candidate, provision, quantity) is { } selectionId
                    ? new AncillaryQuantityOption(quantity, selectionId, unitAmount * quantity)
                    : null)
                .OfType<AncillaryQuantityOption>()
                .ToList();

            return options.Count == 0 ? null : row with { QuantityOptions = options };
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
