using AeroTech.Ancillary.Domain.AncillaryPricingAggregate;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Engine;
using AeroTech.Ancillary.Shopping.Offers;
using AeroTech.Ancillary.Shopping.Reading;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Ancillary.Shopping.Selection;
using AeroTech.Ancillary.Shopping.Selections;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Shopping.Options
{
    public sealed class SelectionProofService : ISelectionProofService
    {
        private const int SingleUnit = 1;

        private readonly ISelectionIdCodec _codec;
        private readonly IActiveAncillaryPricingReader _pricings;
        private readonly IActiveAncillaryProvisionReader _provisions;
        private readonly IActiveAncillaryDefinitionReader _definitions;
        private readonly IAncillaryShoppingEngine _engine;
        private readonly SelectionIdOptions _options;
        private readonly IClock _clock;

        public SelectionProofService(
            ISelectionIdCodec codec,
            IActiveAncillaryPricingReader pricings,
            IActiveAncillaryProvisionReader provisions,
            IActiveAncillaryDefinitionReader definitions,
            IAncillaryShoppingEngine engine,
            SelectionIdOptions options,
            IClock clock)
        {
            _codec = codec;
            _pricings = pricings;
            _provisions = provisions;
            _definitions = definitions;
            _engine = engine;
            _options = options;
            _clock = clock;
        }

        public async Task<SignedSelection> ReadAsync(string? id, SelectionIdKind kind)
            => await _codec.ReadAsync(id, kind) switch
            {
                { Status: SignedSelectionStatus.Valid, Selection: { } selection } => selection,
                { Status: SignedSelectionStatus.Expired } => throw ExceptionFactory.SelectionHasExpired(),
                _ => throw ExceptionFactory.SelectionSignatureIsInvalid()
            };

        public async Task<SelectionProof> ProveDirectAsync(
            SignedSelection selection,
            int? quantity,
            IReadOnlyList<AncillaryShoppingContext> contexts,
            CancellationToken cancellationToken = default)
        {
            if (selection is not { MinQuantity: { } minimum, MaxQuantity: { } maximum })
                throw ExceptionFactory.SelectionSignatureIsInvalid();

            var target = await TargetAsync(selection, null, contexts, cancellationToken);
            var requested = quantity ?? minimum;

            if (requested < minimum || requested > maximum)
                throw ExceptionFactory.SelectionOptionsAreInvalid($"quantity must be between {minimum} and {maximum}");

            await EvaluateAsync(
                target,
                selection,
                new AncillarySelection { Quantity = target.Definition.SelectionContract!.Kind == SelectionKind.QuantityChoice ? requested : null },
                cancellationToken);

            return Proof(selection, requested, target.Definition);
        }

        public async Task<SelectionProof> ProveTypedAsync(
            SignedSelection reference,
            AncillaryProfile family,
            AncillarySelection selection,
            IReadOnlyList<AncillaryShoppingContext> contexts,
            CancellationToken cancellationToken = default)
        {
            var target = await TargetAsync(reference, family, contexts, cancellationToken);

            if (target.Definition.VariantCode == AncillaryVariant.PriorityBoardingCheckin && selection.OptIn != true)
                throw ExceptionFactory.SelectionOptionsAreInvalid("optIn must be true");

            await EvaluateAsync(target, reference, selection, cancellationToken);

            return Proof(reference, SingleUnit, target.Definition);
        }

        private async Task EvaluateAsync(Target target, SignedSelection signed, AncillarySelection selection, CancellationToken cancellationToken)
        {
            var evaluation = await _engine.EvaluateSelectionAsync(
                target.Context,
                new CandidateIdentity(target.Definition.Id, target.Provision.Id, signed.CoverageScope, signed.TravellerRef, signed.CoverageRef, target.Pricing.Id),
                TypedSelectionForm.WithFixedChoices(selection, target.Definition, target.Provision),
                cancellationToken);

            if (evaluation.Status != SelectionEvaluationStatus.Accepted)
                throw evaluation.FieldIssues.Count > 0
                    ? ExceptionFactory.SelectionOptionsAreInvalid(string.Join(", ", evaluation.FieldIssues.Select(issue => $"{issue.Field} {issue.ReasonCode}")))
                    : ExceptionFactory.SelectionIsNotSellable(string.Join(", ", evaluation.Candidate.ReasonCodes));

            if (evaluation.Candidate.Price.PricingRateId != signed.PricingRateId || !SellableCandidate.Is(evaluation.Candidate, target.Context.CurrencyId))
                throw ExceptionFactory.SelectionIsNotSellable("the price or the sale conditions changed; ask for the ancillary offers again");
        }

        private SelectionProof Proof(SignedSelection selection, int quantity, AncillaryServiceDefinition definition)
        {
            var now = _clock.GetDateTime();
            var expiresAtUtc = selection.ExpiresAtUtc < now + _options.MaxAge ? selection.ExpiresAtUtc : now + _options.MaxAge;

            if (expiresAtUtc <= now)
                throw ExceptionFactory.SelectionHasExpired();

            return new SelectionProof(
                selection.PricingRateId,
                selection.CoverageScope,
                selection.TravellerRef,
                selection.CoverageRef,
                selection.OfferBinding,
                selection.SalesBinding,
                quantity,
                expiresAtUtc,
                definition);
        }

        private async Task<Target> TargetAsync(
            SignedSelection selection,
            AncillaryProfile? family,
            IReadOnlyList<AncillaryShoppingContext> contexts,
            CancellationToken cancellationToken)
        {
            var context = contexts.FirstOrDefault(candidate => SelectionBindings.Sales(candidate) == selection.SalesBinding)
                          ?? throw ExceptionFactory.SelectionBindingMismatch();

            if (context.SourceIdentity is not { TrustedSourceReference: { } offerId, TrustedSourceVersion: { } version }
                || SelectionBindings.Offer(offerId, version) != selection.OfferBinding)
                throw ExceptionFactory.SelectionIsNotSellable("the flight offer does not match the selection; ask for the ancillary offers again");

            var pricing = (await _pricings.FindActiveByRateIdsAsync([selection.PricingRateId], cancellationToken)).SingleOrDefault()
                          ?? throw ExceptionFactory.SelectionIsNotSellable("the price is no longer active");
            var provision = (await _provisions.FindActiveProvisionsAsync([pricing.AncillaryProvisionId], cancellationToken)).SingleOrDefault()
                            ?? throw ExceptionFactory.SelectionIsNotSellable("the product rule is no longer active");
            var definition = (await _definitions.FindActiveDefinitionsAsync([provision.ServiceDefinitionId], cancellationToken)).SingleOrDefault()
                             ?? throw ExceptionFactory.SelectionIsNotSellable("the product is no longer active");

            if (family is not null && definition.Profile != family)
                throw ExceptionFactory.SelectionWasNotFound("the reference does not belong to this family");

            if (provision.CoverageScope != selection.CoverageScope || !CustomerInputs.FixedChoicesAreDefined(definition))
                throw ExceptionFactory.SelectionIsNotSellable("the unit does not match the product rule");

            if (CustomerInputs.AreRequested(definition, provision) != (selection.Kind == SelectionIdKind.Reference))
                throw ExceptionFactory.SelectionIsNotSellable(selection.Kind == SelectionIdKind.Reference ? "the product takes no options" : "the product needs options");

            return new Target(context, definition, provision, pricing);
        }

        private sealed record Target(
            AncillaryShoppingContext Context,
            AncillaryServiceDefinition Definition,
            AncillaryProvision Provision,
            AncillaryPricing Pricing);
    }
}
