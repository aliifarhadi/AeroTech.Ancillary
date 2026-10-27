using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Dto;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.GetSeatMap.Api;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.GetOrderSeatMap.Api
{
    public sealed class ApiGetOrderSeatMapQueryHandler : IRequestHandler<ApiGetOrderSeatMapQuery, OrderSeatMapsDto>
    {
        private readonly IApiSalesScopeResolver _salesScope;
        private readonly IGetOrderSeatMapService _service;

        public ApiGetOrderSeatMapQueryHandler(IApiSalesScopeResolver salesScope, IGetOrderSeatMapService service)
        {
            _salesScope = salesScope;
            _service = service;
        }

        public async Task<OrderSeatMapsDto> Handle(ApiGetOrderSeatMapQuery query, CancellationToken cancellationToken)
            => ApiSeatMapMapper.Map(await _service.ExecuteAsync(query, await _salesScope.ResolveAsync(cancellationToken), cancellationToken));
    }
}
