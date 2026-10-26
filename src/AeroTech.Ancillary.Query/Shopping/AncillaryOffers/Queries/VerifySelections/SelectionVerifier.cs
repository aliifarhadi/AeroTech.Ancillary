using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers;
using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Services;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.Shopping.Common;
using AeroTech.Ancillary.Query.Shopping.Common.FlightOffers;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Options;
using AeroTech.Ancillary.Shopping.Selection;
using AeroTech.Ancillary.Shopping.Selections;
using AeroTech.Messages.Shared.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.VerifySelections
{
    public sealed class SelectionVerifier : ISelectionVerifier
    {
        private const string SelectionsMember = "selections";

        private readonly IBackofficeSalesScopeResolver _backoffice;
        private readonly IOtaPanelSalesScopeResolver _otaPanel;
        private readonly IApiSalesScopeResolver _api;
        private readonly IFlightOfferShoppingSource _source;
        private readonly ISelectionProofService _proofs;

        public SelectionVerifier(
            IBackofficeSalesScopeResolver backoffice,
            IOtaPanelSalesScopeResolver otaPanel,
            IApiSalesScopeResolver api,
            IFlightOfferShoppingSource source,
            ISelectionProofService proofs)
        {
            _backoffice = backoffice;
            _otaPanel = otaPanel;
            _api = api;
            _source = source;
            _proofs = proofs;
        }

        public async Task<IReadOnlyList<VerifiedSelection>> VerifyAsync(SelectionVerification verification, CancellationToken cancellationToken = default)
        {
            var selections = verification.Selections;
            var signed = new List<SignedSelection>(selections.Count);

            for (var index = 0; index < selections.Count; index++)
                signed.Add(await SelectionBatch.ItemAsync(SelectionsMember, index, () => _proofs.ReadAsync(selections[index].Id, selections[index].Kind)));

            var scope = await ScopeAsync(verification.Surface, cancellationToken);
            var shopping = await _source.LoadAsync(verification.OfferId, scope, cancellationToken);

            PricedOfferScopeGuard.EnsureSellable(shopping.Offer, scope);

            var contexts = shopping.PointsOfSale.Select(pointOfSale => pointOfSale.Context).ToList();
            var proofs = new List<SelectionProof>(selections.Count);

            for (var index = 0; index < selections.Count; index++)
                proofs.Add(await SelectionBatch.ItemAsync(SelectionsMember, index, () => ProveAsync(selections[index], signed[index], proofs, contexts, cancellationToken)));

            return proofs.Select(Verified).ToList();
        }

        private async Task<SelectionProof> ProveAsync(
            SubmittedSelection selection,
            SignedSelection signed,
            IReadOnlyList<SelectionProof> earlier,
            IReadOnlyList<AncillaryShoppingContext> contexts,
            CancellationToken cancellationToken)
        {
            var proof = selection switch
            {
                TypedSelection typed => await _proofs.ProveTypedAsync(signed, typed.Family, Selection(typed.Options), contexts, cancellationToken),
                DirectSelection direct => await _proofs.ProveDirectAsync(signed, direct.Quantity, contexts, cancellationToken),
                _ => throw new ArgumentOutOfRangeException(nameof(selection), selection, null)
            };

            if (SelectionBasketRules.Conflict(earlier.Select(BasketLine).ToList(), BasketLine(proof)) is { } conflict)
                throw ExceptionFactory.SelectionIsNotSellable(conflict);

            return proof;
        }

        private static SelectionBasketLine BasketLine(SelectionProof proof)
            => new(proof.PricingRateId, proof.CoverageScope, proof.TravellerRef, proof.CoverageRef, proof.Definition);

        private Task<AncillarySalesScope> ScopeAsync(AuthorizationSurface surface, CancellationToken cancellationToken)
            => surface switch
            {
                AuthorizationSurface.Backoffice => _backoffice.ResolveAsync(null, cancellationToken),
                AuthorizationSurface.OtaPanel => _otaPanel.ResolveAsync(cancellationToken),
                AuthorizationSurface.Api => _api.ResolveAsync(cancellationToken),
                _ => throw ExceptionFactory.ShoppingCallerNotAuthorized()
            };

        private static VerifiedSelection Verified(SelectionProof proof)
            => new(
                proof.PricingRateId,
                proof.CoverageScope,
                proof.TravellerRef,
                proof.CoverageRef,
                proof.OfferBinding,
                proof.SalesBinding,
                proof.Quantity,
                proof.ExpiresAtUtc);

        private static AncillarySelection Selection(ISelectionOptions options)
            => new()
            {
                BagRef = options.BagRef,
                MeasuredWeightKg = options.MeasuredWeightKg,
                Dimensions = Dimensions(options.Dimensions),
                WeightKg = options.WeightKg,
                AnimalType = options.AnimalType,
                OtherAnimalCode = options.OtherAnimalCode,
                AnimalAgeWeeks = options.AnimalAgeWeeks,
                CombinedWeightKg = options.CombinedWeightKg,
                CarrierDimensions = Dimensions(options.CarrierDimensions),
                DocumentAcknowledgements = options.DocumentAcknowledgements,
                SizeBracket = options.SizeBracket,
                AssistanceSsrCode = options.AssistanceSsrCode,
                InfantRef = options.InfantRef,
                GuardianRef = options.GuardianRef,
                TimeWithOffset = options.TimeWithOffset,
                GuestCount = options.GuestCount,
                OptIn = options.OptIn,
                DeviceCount = options.DeviceCount
            };

        private static SelectedDimensions? Dimensions(SelectedDimensionsInput? dimensions)
            => dimensions is null ? null : new SelectedDimensions(dimensions.LengthCm, dimensions.WidthCm, dimensions.HeightCm);
    }
}
