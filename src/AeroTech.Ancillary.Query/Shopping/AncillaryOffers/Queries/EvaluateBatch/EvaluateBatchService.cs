using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateSelection;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOfferDetail;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.Shopping.Common;
using AeroTech.Ancillary.Query.Shopping.Common.FlightOffers;
using AeroTech.Ancillary.Query.Shopping.Common.PricedSelections;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.GetSeatMap;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Snapshots;
using AeroTech.Ancillary.Shopping.Engine;
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
        private readonly IGetSeatMapService _seatMaps;
        private readonly IPricedSelectionTokens _tokens;
        private readonly IAncillaryShoppingEngine _engine;

        public EvaluateBatchService(
            IShoppingSource source,
            IGetAncillaryOffersService offers,
            IGetAncillaryOfferDetailService details,
            IEvaluateSelectionService selections,
            IGetSeatMapService seatMaps,
            IPricedSelectionTokens tokens,
            IAncillaryShoppingEngine engine)
        {
            _source = source;
            _offers = offers;
            _details = details;
            _selections = selections;
            _seatMaps = seatMaps;
            _tokens = tokens;
            _engine = engine;
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
            var seatMap = requested.Any(line => line.Kind == SelectionLineKind.SeatMap) ? await _seatMaps.BuildAsync(source, scope, cancellationToken) : null;
            var evaluated = new List<SelectionLineSnapshot>(requested.Count);

            foreach (var line in requested)
                evaluated.Add(line.Kind == SelectionLineKind.SeatMap ? await SeatAsync(line, seatMap!, cancellationToken) : await AncillaryAsync(line, shopping, cancellationToken));

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

        private async Task<SelectionLineSnapshot> AncillaryAsync(SelectionLineRequest line, AncillaryShoppingSnapshot shopping, CancellationToken cancellationToken)
        {
            var item = shopping.Items.SingleOrDefault(candidate => string.Equals(candidate.OfferItemRef, line.OfferItemRef, StringComparison.Ordinal));

            if (item is null)
                return Rejected(line, ShoppingReasonCodes.OfferItemNotOffered);

            var detail = await _details.DescribeAsync(shopping, item, cancellationToken);
            var selection = await _selections.EvaluateAsync(detail, line.Selection ?? new AncillarySelection(), cancellationToken);

            return OptionRefsMatch(line.SelectedOptionRefs, detail, selection.Selection)
                ? new SelectionLineSnapshot(line.ClientSelectionId!, SelectionLineKind.Ancillary, selection.Evaluation.Status, [], OfferPurchase.CanPurchase(selection.Evaluation), selection, null, null)
                : new SelectionLineSnapshot(line.ClientSelectionId!, SelectionLineKind.Ancillary, SelectionEvaluationStatus.Rejected, [ShoppingReasonCodes.SelectedOptionNotOffered], false, selection, null, null);
        }

        private static bool OptionRefsMatch(IReadOnlyDictionary<string, string>? optionRefs, AncillaryOfferDetailSnapshot detail, AncillarySelection selection)
            => optionRefs is null
               || optionRefs.All(pair => detail.OptionGroups.SingleOrDefault(group => string.Equals(group.FieldName, pair.Key, StringComparison.Ordinal)) is { SourceState: OptionSourceState.Known } group
                                        && SelectionFieldValues.Of(selection, pair.Key) is { } value
                                        && group.Choices.Any(choice => string.Equals(choice.Value, value, StringComparison.Ordinal))
                                        && string.Equals(pair.Value, OfferItemRef.OptionOf(detail.Item.OfferItemRef, pair.Key, value), StringComparison.Ordinal));

        private async Task<SelectionLineSnapshot> SeatAsync(SelectionLineRequest line, SeatMapSnapshot seatMap, CancellationToken cancellationToken)
        {
            var found = seatMap.Flights
                .SelectMany(flight => flight.Offers.Select(offer => (Flight: flight, Offer: offer)))
                .FirstOrDefault(row => string.Equals(row.Offer.Id, line.SeatMapSelectionId, StringComparison.Ordinal));

            if (found.Offer is null)
                return Rejected(line, ShoppingReasonCodes.SeatSelectionNotOffered);

            if (found.Offer.Candidate is not { } candidate)
                return new SelectionLineSnapshot(
                    line.ClientSelectionId!,
                    SelectionLineKind.SeatMap,
                    SelectionEvaluationStatus.Accepted,
                    [],
                    true,
                    null,
                    new SeatSelectionSnapshot(line.SeatMapSelectionId!, found.Flight, found.Offer, seatMap.CurrencyId, new AncillarySelection { SeatNumber = found.Offer.SeatNumber }, null),
                    null);

            var selection = new AncillarySelection { SeatNumber = found.Offer.SeatNumber, ExitRowTermsAccepted = line.ExitRowTermsAccepted };
            var context = seatMap.PointsOfSale.Single(pointOfSale => pointOfSale.PointOfSaleId == found.Offer.PointOfSaleId).Context;
            var evaluation = await _engine.EvaluateSelectionAsync(context, candidate.CandidateIdentity, selection, cancellationToken);

            return new SelectionLineSnapshot(
                line.ClientSelectionId!,
                SelectionLineKind.SeatMap,
                evaluation.Status,
                [],
                OfferPurchase.CanPurchase(evaluation),
                null,
                new SeatSelectionSnapshot(line.SeatMapSelectionId!, found.Flight, found.Offer with { Candidate = evaluation.Candidate }, seatMap.CurrencyId, selection, evaluation),
                null);
        }

        private static SelectionLineSnapshot Rejected(SelectionLineRequest line, string reasonCode)
            => new(line.ClientSelectionId!, line.Kind!.Value, SelectionEvaluationStatus.Rejected, [reasonCode], false, null, null, null);

        private static IReadOnlyList<SelectionLineRequest> WellFormed(IReadOnlyList<SelectionLineRequest>? lines)
        {
            if (lines is not { Count: > 0 and <= MaximumLines })
                throw ExceptionFactory.ShoppingContextIsInvalid($"selections must hold between 1 and {MaximumLines} lines");

            if (lines.Any(line => string.IsNullOrWhiteSpace(line.ClientSelectionId)))
                throw ExceptionFactory.ShoppingContextIsInvalid("every selection needs a clientSelectionId");

            if (lines.GroupBy(line => line.ClientSelectionId, StringComparer.Ordinal).Any(group => group.Skip(1).Any()))
                throw ExceptionFactory.ShoppingContextIsInvalid("clientSelectionId must be unique within the batch");

            var malformed = lines.FirstOrDefault(line => line.Kind switch
            {
                SelectionLineKind.Ancillary => string.IsNullOrWhiteSpace(line.OfferItemRef) || line.SeatMapSelectionId is not null || line.ExitRowTermsAccepted is not null,
                SelectionLineKind.SeatMap => string.IsNullOrWhiteSpace(line.SeatMapSelectionId) || line.OfferItemRef is not null || line.Selection is not null || line.SelectedOptionRefs is not null,
                _ => true
            });

            return malformed is null
                ? lines
                : throw ExceptionFactory.ShoppingContextIsInvalid(
                    $"selection '{malformed.ClientSelectionId}' must be an ancillary line with an offerItemRef or a seatMap line with a seatMapSelectionId and at most exitRowTermsAccepted");
        }
    }
}
