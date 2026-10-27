using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.GetSeatMap.Api
{
    public sealed class ApiGetSeatMapQueryHandler : IRequestHandler<ApiGetSeatMapQuery, SeatMapsDto>
    {
        private readonly IApiSalesScopeResolver _salesScope;
        private readonly IGetSeatMapService _service;

        public ApiGetSeatMapQueryHandler(IApiSalesScopeResolver salesScope, IGetSeatMapService service)
        {
            _salesScope = salesScope;
            _service = service;
        }

        public async Task<SeatMapsDto> Handle(ApiGetSeatMapQuery query, CancellationToken cancellationToken)
            => ApiSeatMapMapper.Map(await _service.ExecuteAsync(query, await _salesScope.ResolveAsync(cancellationToken), cancellationToken));
    }
}
