using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Services;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.Shopping.Common;
using AeroTech.Ancillary.Query.Shopping.Common.FlightOffers;
using AeroTech.Ancillary.Query.Shopping.Common.Orders;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Options;
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
        private readonly IOrderShoppingSource _orders;
        private readonly ISelectionProofService _proofs;

        public SelectionVerifier(
            IBackofficeSalesScopeResolver backoffice,
            IOtaPanelSalesScopeResolver otaPanel,
            IApiSalesScopeResolver api,
            IFlightOfferShoppingSource source,
            IOrderShoppingSource orders,
            ISelectionProofService proofs)
        {
            _backoffice = backoffice;
            _otaPanel = otaPanel;
            _api = api;
            _source = source;
            _orders = orders;
            _proofs = proofs;
        }

        public async Task<IReadOnlyList<VerifiedSelection>> VerifyAsync(SelectionVerification verification, CancellationToken cancellationToken = default)
        {
            var signed = await ReadSignedAsync(verification.Selections);
            var scope = await ScopeAsync(verification.Surface, cancellationToken);
            var shopping = await _source.LoadAsync(verification.OfferId, scope, cancellationToken);

            PricedOfferScopeGuard.EnsureSellable(shopping.Offer, scope);

            return await ProveAllAsync(verification.Selections, signed, shopping, cancellationToken);
        }

        public async Task<VerifiedOrderSelections> VerifyAsync(OrderSelectionVerification verification, CancellationToken cancellationToken = default)
        {
            var signed = await ReadSignedAsync(verification.Selections);
            var scope = await ScopeAsync(verification.Surface, cancellationToken);
            var order = await _orders.LoadAsync(verification.OrderId, scope, cancellationToken);

            return new VerifiedOrderSelections(
                order.Order.CommercialVersion,
                await ProveAllAsync(verification.Selections, signed, order.Shopping, cancellationToken));
        }

        private async Task<IReadOnlyList<SignedSelection>> ReadSignedAsync(IReadOnlyList<SubmittedSelection> selections)
        {
            var signed = new List<SignedSelection>(selections.Count);

            for (var index = 0; index < selections.Count; index++)
                signed.Add(await SelectionBatch.ItemAsync(SelectionsMember, index, () => _proofs.ReadAsync(selections[index].Id, selections[index].Kind)));

            return signed;
        }

        private async Task<IReadOnlyList<VerifiedSelection>> ProveAllAsync(
            IReadOnlyList<SubmittedSelection> selections,
            IReadOnlyList<SignedSelection> signed,
            FlightOfferShopping shopping,
            CancellationToken cancellationToken)
        {
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
                TypedSelection typed => await _proofs.ProveTypedAsync(signed, typed.Family, TypedSelectionMapper.ToSelection(typed.Options), contexts, cancellationToken),
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
                proof.SourceBinding,
                proof.SalesBinding,
                proof.Quantity,
                proof.ExpiresAtUtc);
    }
}
