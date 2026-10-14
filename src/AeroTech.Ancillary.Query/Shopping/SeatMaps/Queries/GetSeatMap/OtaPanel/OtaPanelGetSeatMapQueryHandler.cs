using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.GetSeatMap.OtaPanel
{
    public sealed class OtaPanelGetSeatMapQueryHandler : IRequestHandler<OtaPanelGetSeatMapQuery, SeatMapsDto>
    {
        private readonly IOtaPanelSalesScopeResolver _salesScope;
        private readonly IGetSeatMapService _service;

        public OtaPanelGetSeatMapQueryHandler(IOtaPanelSalesScopeResolver salesScope, IGetSeatMapService service)
        {
            _salesScope = salesScope;
            _service = service;
        }

        public async Task<SeatMapsDto> Handle(OtaPanelGetSeatMapQuery query, CancellationToken cancellationToken)
        {
            var snapshot = await _service.ExecuteAsync(query, await _salesScope.ResolveAsync(cancellationToken), cancellationToken);

            return SeatMapMapper.Map(snapshot, travellerRef => travellerRef, flight => flight.FlightRef);
        }
    }
}
