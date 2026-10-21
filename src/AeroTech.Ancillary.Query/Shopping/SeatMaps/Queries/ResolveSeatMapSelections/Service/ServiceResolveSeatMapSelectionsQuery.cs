using AeroTech.Ancillary.Query.Shopping.SeatMaps.Dto.Service;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.ResolveSeatMapSelections.Service
{
    public sealed class ServiceResolveSeatMapSelectionsQuery : IRequest<ServiceSeatMapResolutionDto>
    {
        public IReadOnlyList<string>? Ids { get; set; }
    }
}
