using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.OtaPanel;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateBatch.OtaPanel
{
    public sealed class OtaPanelEvaluateBatchQueryHandler : IRequestHandler<OtaPanelEvaluateBatchQuery, OtaPanelSelectionBatchDto>
    {
        private readonly IOtaPanelSalesScopeResolver _salesScope;
        private readonly IEvaluateBatchService _service;

        public OtaPanelEvaluateBatchQueryHandler(IOtaPanelSalesScopeResolver salesScope, IEvaluateBatchService service)
        {
            _salesScope = salesScope;
            _service = service;
        }

        public async Task<OtaPanelSelectionBatchDto> Handle(OtaPanelEvaluateBatchQuery query, CancellationToken cancellationToken)
            => OtaPanelSelectionBatchMapper.Map(
                await _service.ExecuteAsync(
                    query,
                    OtaPanelSelectionLineInputMapper.ToRequests(query.Selections),
                    await _salesScope.ResolveAsync(cancellationToken),
                    cancellationToken));
    }
}
