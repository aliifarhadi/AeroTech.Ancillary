using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.OtaPanel;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateSelection;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Ancillary.Query.Shopping.Common;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateSelection.OtaPanel
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
