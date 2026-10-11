using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers.OtaPanel;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetSeatMap.OtaPanel
{
    public sealed class OtaPanelGetSeatMapQueryHandler : IRequestHandler<OtaPanelGetSeatMapQuery>
    {
        private readonly IOtaPanelSalesScopeResolver _salesScope;
        private readonly IGetSeatMapService _service;

        public OtaPanelGetSeatMapQueryHandler(IOtaPanelSalesScopeResolver salesScope, IGetSeatMapService service)
        {
            _salesScope = salesScope;
            _service = service;
        }

        public async Task Handle(OtaPanelGetSeatMapQuery query, CancellationToken cancellationToken)
            => await _service.ExecuteAsync(query, await _salesScope.ResolveAsync(cancellationToken), cancellationToken);
    }
}
