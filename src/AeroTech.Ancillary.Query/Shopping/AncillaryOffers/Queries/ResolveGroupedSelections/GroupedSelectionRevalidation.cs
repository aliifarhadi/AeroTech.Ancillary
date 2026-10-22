using System.Globalization;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateBatch;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetGroupedAncillaryOffers;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.SelectionIds;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.ResolveGroupedSelections
{
    public sealed class GroupedSelectionRevalidation
    {
        private readonly IEvaluateBatchService _batches;
        private readonly IOperatorAirlineResolver _operatorAirline;

        public GroupedSelectionRevalidation(IEvaluateBatchService batches, IOperatorAirlineResolver operatorAirline)
        {
            _batches = batches;
            _operatorAirline = operatorAirline;
        }

        public async Task<IReadOnlyList<GroupedResolvedSelectionSnapshot>> RevalidateAsync(
            string? offerId,
            long? orderId,
            int? expectedOrderCommercialVersion,
            IReadOnlyList<GroupedSelectionEntry> entries,
            CancellationToken cancellationToken = default)
        {
            var homeAirlineId = await _operatorAirline.FindHomeAirlineIdAsync(cancellationToken);

            if (homeAirlineId is not (> 0 and <= int.MaxValue))
                throw ExceptionFactory.ShoppingCallerNotAuthorized();

            var results = new GroupedResolvedSelectionSnapshot?[entries.Count];
            var decoded = new NonSeatSelectionId?[entries.Count];

            for (var index = 0; index < entries.Count; index++)
            {
                decoded[index] = NonSeatSelectionIdCodec.Decode(entries[index].SelectionId);
                results[index] = decoded[index] switch
                {
                    null => Unresolved(entries[index], SelectionResolutionStatus.Rejected, ShoppingReasonCodes.PricedSelectionNotAuthentic),
                    { } id when id.Profile != entries[index].Profile => Unresolved(entries[index], SelectionResolutionStatus.Rejected, ShoppingReasonCodes.OfferItemNotOffered),
                    { } id when !SameSource(id, offerId, orderId) => Unresolved(entries[index], SelectionResolutionStatus.Rejected, ShoppingReasonCodes.PricedSelectionForAnotherOrder),
                    _ => null
                };
            }

            foreach (var group in Enumerable.Range(0, entries.Count).Where(index => results[index] is null).GroupBy(index => decoded[index]!.Scope))
            {
                var scope = group.Key.ToSalesScope((int)homeAirlineId.Value);
                var batch = await _batches.ExecuteAsync(
                    new GroupedSelectionQuery(offerId, orderId),
                    group.Select(index => new SelectionLineRequest(index.ToString(CultureInfo.InvariantCulture), decoded[index]!.OfferItemRef, decoded[index]!.Selection)).ToList(),
                    scope,
                    cancellationToken);
                var context = batch.Shopping.PointsOfSale.First().Context;

                foreach (var line in batch.Lines)
                {
                    var index = int.Parse(line.ClientSelectionId, CultureInfo.InvariantCulture);

                    results[index] = Resolve(entries[index], decoded[index]!, line, batch.Shopping, context, orderId, expectedOrderCommercialVersion);
                }
            }

            return results.Select(result => result!).ToList();
        }

        private static bool SameSource(NonSeatSelectionId id, string? offerId, long? orderId)
            => orderId is { } order
                ? id.SourceKind == ShoppingSourceKind.Order && id.SourceRef == order.ToString(CultureInfo.InvariantCulture)
                : id.SourceKind == ShoppingSourceKind.Offer && id.SourceRef == offerId;

        private static GroupedResolvedSelectionSnapshot Resolve(
            GroupedSelectionEntry entry,
            NonSeatSelectionId id,
            SelectionLineSnapshot line,
            AncillaryShoppingSnapshot shopping,
            AncillaryShoppingContext context,
            long? orderId,
            int? expectedOrderCommercialVersion)
        {
            if (orderId is not null && (context.SourceIdentity.OrderCommercialVersion != id.OrderCommercialVersion || context.SourceIdentity.OrderCommercialVersion != expectedOrderCommercialVersion))
                return Unresolved(entry, SelectionResolutionStatus.Stale, ShoppingReasonCodes.OrderVersionChanged);

            if (line.Status == SelectionEvaluationStatus.Rejected)
                return new GroupedResolvedSelectionSnapshot(
                    entry.Profile,
                    entry.SelectionId,
                    line.ReasonCodes.Contains(ShoppingReasonCodes.OfferItemNotOffered, StringComparer.Ordinal) ? SelectionResolutionStatus.Stale : SelectionResolutionStatus.Rejected,
                    [.. line.ReasonCodes.Union(line.Ancillary?.Evaluation.FieldIssues.Select(issue => issue.ReasonCode) ?? [], StringComparer.Ordinal)],
                    null,
                    null);

            if (!line.CanPurchase)
                return new GroupedResolvedSelectionSnapshot(
                    entry.Profile,
                    entry.SelectionId,
                    line.ReasonCodes.Contains(ShoppingReasonCodes.OrderTravellerNotLinked, StringComparer.Ordinal)
                        ? SelectionResolutionStatus.NeedsVerification
                        : line.Candidate?.OfferReadiness switch
                        {
                            OfferReadiness.NeedsQuote => SelectionResolutionStatus.NeedsQuote,
                            OfferReadiness.NeedsVerification => SelectionResolutionStatus.NeedsVerification,
                            _ => SelectionResolutionStatus.Unavailable
                        },
                    [.. line.ReasonCodes.Union(line.Candidate?.ReasonCodes ?? [], StringComparer.Ordinal)],
                    null,
                    null);

            var ancillary = line.Ancillary!;

            if (!string.Equals(ancillary.CurrentOfferItemRef, id.OfferItemRef, StringComparison.Ordinal))
                return Unresolved(entry, SelectionResolutionStatus.Stale, ShoppingReasonCodes.SourceVersionChanged);

            if (line.CurrencyId != shopping.CurrencyId)
                return Unresolved(entry, SelectionResolutionStatus.Stale, ShoppingReasonCodes.PriceChanged);

            if (ancillary.Evaluation.Candidate.Price.UnappliedFeeLines.Count > 0)
                return Unresolved(entry, SelectionResolutionStatus.Unavailable, ShoppingReasonCodes.SharedFeeNotApplied);

            return string.Equals(GroupedSelectionIdProvider.IdOf(ancillary, orderId), entry.SelectionId, StringComparison.Ordinal)
                ? new GroupedResolvedSelectionSnapshot(entry.Profile, entry.SelectionId, SelectionResolutionStatus.Valid, [], line, context)
                : Unresolved(entry, SelectionResolutionStatus.Stale, ShoppingReasonCodes.SignedFactsChanged);
        }

        private static GroupedResolvedSelectionSnapshot Unresolved(GroupedSelectionEntry entry, SelectionResolutionStatus status, params string[] reasonCodes)
            => new(entry.Profile, entry.SelectionId, status, reasonCodes, null, null);
    }
}
