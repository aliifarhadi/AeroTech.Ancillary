using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateSelection;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOfferDetail;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.Shopping.Common;
using AeroTech.Ancillary.Query.Shopping.Common.PricedSelections;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Ancillary.Shopping.Selection;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateBatch
{
    public sealed class EvaluateBatchService : IEvaluateBatchService
    {
        public const int MaximumLines = 50;

        private readonly IShoppingSource _source;
        private readonly IGetAncillaryOffersService _offers;
        private readonly IGetAncillaryOfferDetailService _details;
        private readonly IEvaluateSelectionService _selections;
        private readonly IPricedSelectionTokens _tokens;

        public EvaluateBatchService(
            IShoppingSource source,
            IGetAncillaryOffersService offers,
            IGetAncillaryOfferDetailService details,
            IEvaluateSelectionService selections,
            IPricedSelectionTokens tokens)
        {
            _source = source;
            _offers = offers;
            _details = details;
            _selections = selections;
            _tokens = tokens;
        }

        public async Task<SelectionBatchSnapshot> ExecuteAsync(
            IAncillaryOffersQuery query,
            IReadOnlyList<SelectionLineRequest>? lines,
            AncillarySalesScope scope,
            CancellationToken cancellationToken = default)
        {
            var requested = WellFormed(lines);
            var source = await _source.LoadAsync(query.OfferId, query.OrderId, scope, cancellationToken);
            var shopping = await _offers.ShopAsync(source, scope, OfferFamilyFilters.Services, cancellationToken);
            var evaluated = new List<SelectionLineSnapshot>(requested.Count);

            foreach (var line in requested)
                evaluated.Add(await EvaluateAsync(line, shopping, cancellationToken));

            var context = shopping.PointsOfSale.Select(pointOfSale => pointOfSale.Context).FirstOrDefault();
            var binding = OrderBinding.Of(context?.SourceIdentity);
            var settled = SelectionBasketRules.Settle(evaluated);

            if (binding is not null && context is not null)
                settled = SelectionBasketRules.Link(settled, context);

            var basket = SelectionBasketRules.Basket(settled, shopping.CurrencyId, binding is not null);

            if (basket.AllReady && binding is not null)
                settled = settled.Select(line => line with { PricedSelection = _tokens.Issue(PricedSelectionMapper.Of(line, scope, binding), binding.ValidUntilUtc) }).ToList();

            return new SelectionBatchSnapshot(shopping, settled, basket);
        }

        private async Task<SelectionLineSnapshot> EvaluateAsync(SelectionLineRequest line, AncillaryShoppingSnapshot shopping, CancellationToken cancellationToken)
        {
            var item = shopping.Items.SingleOrDefault(candidate => string.Equals(candidate.OfferItemRef, line.OfferItemRef, StringComparison.Ordinal));

            if (item is null)
                return new SelectionLineSnapshot(line.ClientSelectionId!, SelectionEvaluationStatus.Rejected, [ShoppingReasonCodes.OfferItemNotOffered], false, null, null);

            var detail = await _details.DescribeAsync(shopping, item, cancellationToken);
            var selection = await _selections.EvaluateAsync(detail, line.Selection ?? new AncillarySelection(), cancellationToken);

            return OptionRefsMatch(line.SelectedOptionRefs, detail, selection.Selection)
                ? new SelectionLineSnapshot(line.ClientSelectionId!, selection.Evaluation.Status, [], OfferPurchase.CanPurchase(selection.Evaluation), selection, null)
                : new SelectionLineSnapshot(line.ClientSelectionId!, SelectionEvaluationStatus.Rejected, [ShoppingReasonCodes.SelectedOptionNotOffered], false, selection, null);
        }

        private static bool OptionRefsMatch(IReadOnlyDictionary<string, string>? optionRefs, AncillaryOfferDetailSnapshot detail, AncillarySelection selection)
            => optionRefs is null
               || optionRefs.All(pair => detail.OptionGroups.SingleOrDefault(group => string.Equals(group.FieldName, pair.Key, StringComparison.Ordinal)) is { SourceState: OptionSourceState.Known } group
                                        && SelectionFieldValues.Of(selection, pair.Key) is { } value
                                        && group.Choices.Any(choice => string.Equals(choice.Value, value, StringComparison.Ordinal))
                                        && string.Equals(pair.Value, OfferItemRef.OptionOf(detail.Item.OfferItemRef, pair.Key, value), StringComparison.Ordinal));

        private static IReadOnlyList<SelectionLineRequest> WellFormed(IReadOnlyList<SelectionLineRequest>? lines)
        {
            if (lines is not { Count: > 0 and <= MaximumLines })
                throw ExceptionFactory.ShoppingContextIsInvalid($"selections must hold between 1 and {MaximumLines} lines");

            if (lines.Any(line => string.IsNullOrWhiteSpace(line.ClientSelectionId)))
                throw ExceptionFactory.ShoppingContextIsInvalid("every selection needs a clientSelectionId");

            if (lines.GroupBy(line => line.ClientSelectionId, StringComparer.Ordinal).Any(group => group.Skip(1).Any()))
                throw ExceptionFactory.ShoppingContextIsInvalid("clientSelectionId must be unique within the batch");

            return lines.All(line => !string.IsNullOrWhiteSpace(line.OfferItemRef))
                ? lines
                : throw ExceptionFactory.ShoppingContextIsInvalid("every selection needs an offerItemRef");
        }
    }
}
