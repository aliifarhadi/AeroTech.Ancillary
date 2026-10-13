using AeroTech.Ancillary.Query.AncillaryOffers.Queries.EvaluateSelection;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.EvaluateSelection.Backoffice;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers.Backoffice;
using AeroTech.Ancillary.Query.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.SeatOffers.Dto.Backoffice;
using AeroTech.Ancillary.Query.SeatOffers.Queries.GetSeatOffers.Backoffice;
using MediatR;

namespace AeroTech.Ancillary.Query.SeatOffers.Queries.EvaluateSeatSelection.Backoffice
{
    public sealed class BackofficeEvaluateSeatSelectionQueryHandler : IRequestHandler<BackofficeEvaluateSeatSelectionQuery, BackofficeSeatSelectionResultDto>
    {
        private readonly IBackofficeSalesScopeResolver _salesScope;
        private readonly IEvaluateSelectionService _service;

        public BackofficeEvaluateSeatSelectionQueryHandler(IBackofficeSalesScopeResolver salesScope, IEvaluateSelectionService service)
        {
            _salesScope = salesScope;
            _service = service;
        }

        public async Task<BackofficeSeatSelectionResultDto> Handle(BackofficeEvaluateSeatSelectionQuery query, CancellationToken cancellationToken)
        {
            var selection = await _service.ExecuteAsync(
                query,
                BackofficeSelectionInputMapper.ToSelection(query.Selection),
                await _salesScope.ResolveAsync(query.Options, cancellationToken),
                OfferFamilyFilters.Seat,
                cancellationToken);

            return BackofficeSeatOffersMapper.Selection(selection);
        }
    }
}
