using AeroTech.Ancillary.Query.AncillaryOffers.Queries.EvaluateSelection;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.EvaluateSelection.OtaPanel;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers.OtaPanel;
using AeroTech.Ancillary.Query.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.SeatOffers.Dto.OtaPanel;
using AeroTech.Ancillary.Query.SeatOffers.Queries.GetSeatOffers.OtaPanel;
using MediatR;

namespace AeroTech.Ancillary.Query.SeatOffers.Queries.EvaluateSeatSelection.OtaPanel
{
    public sealed class OtaPanelEvaluateSeatSelectionQueryHandler : IRequestHandler<OtaPanelEvaluateSeatSelectionQuery, OtaPanelSeatSelectionResultDto>
    {
        private readonly IOtaPanelSalesScopeResolver _salesScope;
        private readonly IEvaluateSelectionService _service;

        public OtaPanelEvaluateSeatSelectionQueryHandler(IOtaPanelSalesScopeResolver salesScope, IEvaluateSelectionService service)
        {
            _salesScope = salesScope;
            _service = service;
        }

        public async Task<OtaPanelSeatSelectionResultDto> Handle(OtaPanelEvaluateSeatSelectionQuery query, CancellationToken cancellationToken)
        {
            var selection = await _service.ExecuteAsync(
                query,
                OtaPanelSelectionInputMapper.ToSelection(query.Selection),
                await _salesScope.ResolveAsync(cancellationToken),
                OfferFamilyFilters.Seat,
                cancellationToken);

            return OtaPanelSeatOffersMapper.Selection(selection);
        }
    }
}
