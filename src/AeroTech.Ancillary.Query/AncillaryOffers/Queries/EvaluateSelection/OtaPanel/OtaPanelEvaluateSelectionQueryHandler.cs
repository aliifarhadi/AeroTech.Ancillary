using AeroTech.Ancillary.Query.AncillaryOffers.Dto.OtaPanel;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers.OtaPanel;
using AeroTech.Ancillary.Query.AncillaryOffers.Snapshots;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Queries.EvaluateSelection.OtaPanel
{
    public sealed class OtaPanelEvaluateSelectionQueryHandler : IRequestHandler<OtaPanelEvaluateSelectionQuery, OtaPanelSelectionResultDto>
    {
        private readonly IOtaPanelSalesScopeResolver _salesScope;
        private readonly IEvaluateSelectionService _service;

        public OtaPanelEvaluateSelectionQueryHandler(IOtaPanelSalesScopeResolver salesScope, IEvaluateSelectionService service)
        {
            _salesScope = salesScope;
            _service = service;
        }

        public async Task<OtaPanelSelectionResultDto> Handle(OtaPanelEvaluateSelectionQuery query, CancellationToken cancellationToken)
            => OtaPanelSelectionResultMapper.Map(
                await _service.ExecuteAsync(
                    query,
                    OtaPanelSelectionInputMapper.ToSelection(query.Selection),
                    await _salesScope.ResolveAsync(cancellationToken),
                    OfferFamilyFilters.Services,
                    cancellationToken));
    }
}
