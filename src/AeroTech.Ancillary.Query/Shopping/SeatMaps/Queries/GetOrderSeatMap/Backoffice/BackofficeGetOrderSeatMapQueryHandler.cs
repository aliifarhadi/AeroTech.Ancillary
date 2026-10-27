using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Dto;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.GetSeatMap.Backoffice;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.GetOrderSeatMap.Backoffice
{
    public sealed class BackofficeGetOrderSeatMapQueryHandler : IRequestHandler<BackofficeGetOrderSeatMapQuery, OrderSeatMapsDto>
    {
        private readonly IBackofficeSalesScopeResolver _salesScope;
        private readonly IGetOrderSeatMapService _service;

        public BackofficeGetOrderSeatMapQueryHandler(IBackofficeSalesScopeResolver salesScope, IGetOrderSeatMapService service)
        {
            _salesScope = salesScope;
            _service = service;
        }

        public async Task<OrderSeatMapsDto> Handle(BackofficeGetOrderSeatMapQuery query, CancellationToken cancellationToken)
            => BackofficeSeatMapMapper.Map(await _service.ExecuteAsync(query, await _salesScope.ResolveAsync(query.Options, cancellationToken), cancellationToken));
    }
}
