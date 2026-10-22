using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateSelection;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOfferDetail;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetGroupedAncillaryOffers;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.SelectionIds;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Ancillary.Query.Shopping.Common;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Ancillary.Shopping.Selection;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetGroupedOfferDetail
{
    public sealed class GetGroupedOfferDetailService : IGetGroupedOfferDetailService
    {
        private readonly IGetAncillaryOfferDetailService _details;
        private readonly IEvaluateSelectionService _selections;
        private readonly IGroupedSelectionIdProvider _selectionIds;

        public GetGroupedOfferDetailService(IGetAncillaryOfferDetailService details, IEvaluateSelectionService selections, IGroupedSelectionIdProvider selectionIds)
        {
            _details = details;
            _selections = selections;
            _selectionIds = selectionIds;
        }

        public async Task<GroupedOfferDetailSnapshot> ExecuteAsync(
            IAncillaryOfferItemQuery query,
            AncillaryProfile family,
            AncillarySelection? selection,
            AncillarySalesScope scope,
            CancellationToken cancellationToken = default)
        {
            var detail = await _details.ExecuteAsync(query, scope, OfferFamilyFilters.Services, cancellationToken);
            var candidate = detail.Item.Candidate;

            if (candidate.Profile != family || !GetGroupedAncillaryOffersService.IsShown(candidate.OfferReadiness))
                throw ExceptionFactory.OfferItemWasNotFound();

            if (candidate.VariantCode == AncillaryVariant.ExtraSeat)
                return new GroupedOfferDetailSnapshot(detail, null, null, [ShoppingReasonCodes.SeatSelectionNotOffered]);

            if (selection is null)
            {
                var (evaluation, selectionId) = await _selectionIds.IssueAsync(detail.Shopping, detail.Item, query.OrderId, cancellationToken);

                return new GroupedOfferDetailSnapshot(detail, evaluation, selectionId, evaluation is null ? [] : Reasons(evaluation));
            }

            var evaluated = await _selections.EvaluateAsync(detail, selection, cancellationToken);
            var issued = _selectionIds.Issue(evaluated, query.OrderId);

            return new GroupedOfferDetailSnapshot(detail, evaluated, issued.SelectionId, Reasons(evaluated));
        }

        private static IReadOnlyList<string> Reasons(AncillarySelectionSnapshot evaluation)
            => evaluation.Evaluation.FieldIssues.Select(issue => issue.ReasonCode)
                .Union(evaluation.Evaluation.Candidate.ReasonCodes, StringComparer.Ordinal)
                .Union(DirectSelection.HasConsent(evaluation.Evaluation.Candidate, evaluation.Selection) ? [] : [ShoppingReasonCodes.SelectionFieldRequired], StringComparer.Ordinal)
                .ToList();
    }
}
