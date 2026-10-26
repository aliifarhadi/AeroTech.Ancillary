using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Engine;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Shopping.Selections
{
    public sealed class SelectionSigner : ISelectionSigner
    {
        private readonly ISelectionIdCodec _codec;
        private readonly SelectionIdOptions _options;
        private readonly IClock _clock;

        public SelectionSigner(ISelectionIdCodec codec, SelectionIdOptions options, IClock clock)
        {
            _codec = codec;
            _options = options;
            _clock = clock;
        }

        public string? SignDirect(AncillaryShoppingContext context, CanonicalAncillaryOfferCandidate candidate, AncillaryProvision provision, int minQuantity, int maxQuantity)
            => minQuantity > 0 && maxQuantity >= minQuantity && IsDecided(candidate, provision) && ExpiryOf(context, candidate) is { } expiresAtUtc
                ? Sign(SelectionIdKind.Direct, context, candidate, minQuantity, maxQuantity, expiresAtUtc)
                : null;

        public string? SignReference(AncillaryShoppingContext context, CanonicalAncillaryOfferCandidate candidate)
            => candidate.OfferReadiness is OfferReadiness.Selectable or OfferReadiness.NeedsSelection && ExpiryOf(context, candidate) is { } expiresAtUtc
                ? Sign(SelectionIdKind.Reference, context, candidate, null, null, expiresAtUtc)
                : null;

        private static bool IsDecided(CanonicalAncillaryOfferCandidate candidate, AncillaryProvision provision)
            => candidate.ProvisionId == provision.Id
               && (candidate.OfferReadiness == OfferReadiness.Selectable
                   || (candidate is { OfferReadiness: OfferReadiness.NeedsSelection, Eligibility.Status: EligibilityStatus.Eligible }
                       && !AncillaryShoppingEngine.IsUnverifiable(provision, candidate.Availability, candidate.Quantity)));

        private string Sign(
            SelectionIdKind kind,
            AncillaryShoppingContext context,
            CanonicalAncillaryOfferCandidate candidate,
            int? minQuantity,
            int? maxQuantity,
            DateTimeOffset expiresAtUtc)
        {
            var source = context.SourceIdentity;
            var identity = candidate.CandidateIdentity;

            return _codec.Issue(new SignedSelection(
                kind,
                candidate.Price.PricingRateId!.Value,
                identity.CoverageScope,
                identity.TravellerRef,
                identity.CoverageRef,
                minQuantity,
                maxQuantity,
                SelectionBindings.Offer(source.TrustedSourceReference!, source.TrustedSourceVersion!),
                SelectionBindings.Sales(context),
                expiresAtUtc));
        }

        private DateTimeOffset? ExpiryOf(AncillaryShoppingContext context, CanonicalAncillaryOfferCandidate candidate)
        {
            if (context.SourceIdentity is not { SourceKind: ShoppingSourceKind.Offer, TrustedSourceReference.Length: > 0, TrustedSourceVersion.Length: > 0, ValidUntilUtc: { } validUntilUtc }
                || !SellableCandidate.Is(candidate, context.CurrencyId))
                return null;

            var now = _clock.GetDateTime();
            var expiresAtUtc = DateTimeOffset.FromUnixTimeSeconds(Math.Min(validUntilUtc.ToUnixTimeSeconds(), (now + _options.MaxAge).ToUnixTimeSeconds()));

            return expiresAtUtc > now ? expiresAtUtc : null;
        }
    }
}
