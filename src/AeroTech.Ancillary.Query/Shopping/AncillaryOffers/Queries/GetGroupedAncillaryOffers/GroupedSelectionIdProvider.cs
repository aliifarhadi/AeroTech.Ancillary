using System.Globalization;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateSelection;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.SelectionIds;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.Shopping.Common;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetGroupedAncillaryOffers
{
    public sealed class GroupedSelectionIdProvider : IGroupedSelectionIdProvider
    {
        private readonly IEvaluateSelectionService _selections;

        public GroupedSelectionIdProvider(IEvaluateSelectionService selections) => _selections = selections;

        public async Task<(AncillarySelectionSnapshot? Evaluation, string? SelectionId)> IssueAsync(
            AncillaryShoppingSnapshot shopping,
            AncillaryOfferItem item,
            long? orderId,
            CancellationToken cancellationToken = default)
        {
            if (DirectSelection.TryBuild(item.Candidate, item.OptionGroups) is not { } selection)
                return (null, null);

            return Issue(await _selections.EvaluateAsync(shopping, item, selection, cancellationToken), orderId);
        }

        public (AncillarySelectionSnapshot Evaluation, string? SelectionId) Issue(AncillarySelectionSnapshot evaluation, long? orderId)
            => (evaluation, OfferPurchase.CanPurchase(evaluation.Evaluation) && DirectSelection.HasConsent(evaluation.Evaluation.Candidate, evaluation.Selection) ? IdOf(evaluation, orderId) : null);

        public static string IdOf(AncillarySelectionSnapshot evaluation, long? orderId)
        {
            var context = evaluation.Item.Context;
            var candidate = evaluation.Evaluation.Candidate;

            return NonSeatSelectionIdCodec.Encode(new NonSeatSelectionId(
                orderId is null ? ShoppingSourceKind.Offer : ShoppingSourceKind.Order,
                orderId is { } order ? order.ToString(CultureInfo.InvariantCulture) : evaluation.Shopping.Offer.OfferId,
                orderId is null ? null : context.SourceIdentity.OrderCommercialVersion,
                NonSeatSelectionScope.Of(evaluation.Shopping.Scope, evaluation.Item.PointOfSaleId),
                evaluation.Item.OfferItemRef,
                candidate.Profile,
                candidate.VariantCode,
                evaluation.Selection));
        }
    }
}
