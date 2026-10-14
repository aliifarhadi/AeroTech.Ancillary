using System.Globalization;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.GetSeatMap.Backoffice
{
    public sealed class BackofficeGetSeatMapQueryHandler : IRequestHandler<BackofficeGetSeatMapQuery, SeatMapsDto>
    {
        private readonly IBackofficeSalesScopeResolver _salesScope;
        private readonly IGetSeatMapService _service;

        public BackofficeGetSeatMapQueryHandler(IBackofficeSalesScopeResolver salesScope, IGetSeatMapService service)
        {
            _salesScope = salesScope;
            _service = service;
        }

        public async Task<SeatMapsDto> Handle(BackofficeGetSeatMapQuery query, CancellationToken cancellationToken)
        {
            var snapshot = await _service.ExecuteAsync(query, await _salesScope.ResolveAsync(query.Options, cancellationToken), cancellationToken);

            return SeatMapMapper.Map(snapshot, travellerRef => travellerRef, flight => flight.Flight.FlightId.ToString(CultureInfo.InvariantCulture));
        }
    }
}
