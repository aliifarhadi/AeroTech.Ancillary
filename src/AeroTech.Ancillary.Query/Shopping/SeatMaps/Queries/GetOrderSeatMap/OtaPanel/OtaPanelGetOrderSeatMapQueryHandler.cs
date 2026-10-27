using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Dto;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.GetSeatMap.OtaPanel;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.GetOrderSeatMap.OtaPanel
{
    public sealed class OtaPanelGetOrderSeatMapQueryHandler : IRequestHandler<OtaPanelGetOrderSeatMapQuery, OrderSeatMapsDto>
    {
        private readonly IOtaPanelSalesScopeResolver _salesScope;
        private readonly IGetOrderSeatMapService _service;

        public OtaPanelGetOrderSeatMapQueryHandler(IOtaPanelSalesScopeResolver salesScope, IGetOrderSeatMapService service)
        {
            _salesScope = salesScope;
            _service = service;
        }

        public async Task<OrderSeatMapsDto> Handle(OtaPanelGetOrderSeatMapQuery query, CancellationToken cancellationToken)
            => OtaPanelSeatMapMapper.Map(await _service.ExecuteAsync(query, await _salesScope.ResolveAsync(cancellationToken), cancellationToken));
    }
}
