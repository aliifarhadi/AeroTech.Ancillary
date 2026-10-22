using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateBatch;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.ResolveSeatMapSelections;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Snapshots;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.ResolveGroupedSelections
{
    public sealed class ResolveGroupedSelectionsService : IResolveGroupedSelectionsService
    {
        private readonly IResolveSeatMapSelectionsService _seats;
        private readonly GroupedSelectionRevalidation _revalidation;

        public ResolveGroupedSelectionsService(IResolveSeatMapSelectionsService seats, GroupedSelectionRevalidation revalidation)
        {
            _seats = seats;
            _revalidation = revalidation;
        }

        public async Task<GroupedSelectionResolutionSnapshot> ExecuteAsync(GroupedSelectionRequest request, CancellationToken cancellationToken = default)
        {
            var hasOffer = !string.IsNullOrWhiteSpace(request.OfferId);
            var hasOrder = request.OrderId is > 0;

            if (hasOffer == hasOrder)
                throw ExceptionFactory.ShoppingContextIsInvalid("send either an offerId or an orderId, not both");

            if (hasOrder ? request.ExpectedOrderCommercialVersion is not > 0 : request.ExpectedOrderCommercialVersion is not null)
                throw ExceptionFactory.ShoppingContextIsInvalid("expectedOrderCommercialVersion is required with an orderId and must be omitted with an offerId");

            var total = request.Seats.Count + request.Selections.Count;

            if (total is < 1 or > EvaluateBatchService.MaximumLines)
                throw ExceptionFactory.ShoppingContextIsInvalid($"the request must hold between 1 and {EvaluateBatchService.MaximumLines} selections");

            if (request.Seats.Concat(request.Selections.Select(selection => selection.SelectionId)).Distinct(StringComparer.Ordinal).Count() != total)
                throw ExceptionFactory.ShoppingContextIsInvalid("every selection id must appear once");

            var seats = request.Seats.Count == 0
                ? new SeatMapResolutionSnapshot([])
                : await _seats.ExecuteAsync(request.Seats, cancellationToken);
            var selections = request.Selections.Count == 0
                ? []
                : await _revalidation.RevalidateAsync(request.OfferId, request.OrderId, request.ExpectedOrderCommercialVersion, request.Selections, cancellationToken);

            return new GroupedSelectionResolutionSnapshot(
                hasOffer ? request.OfferId : null,
                request.OrderId,
                selections.Select(selection => selection.Context?.SourceIdentity.OrderCommercialVersion).FirstOrDefault(version => version is not null) ?? request.ExpectedOrderCommercialVersion,
                seats,
                selections);
        }
    }
}
