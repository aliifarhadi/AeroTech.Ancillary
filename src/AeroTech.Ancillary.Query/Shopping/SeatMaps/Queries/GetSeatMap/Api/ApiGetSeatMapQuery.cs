using AeroTech.Ancillary.Query.Shopping.SeatMaps.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.GetSeatMap.Api
{
    public sealed class ApiGetSeatMapQuery : IRequest<SeatMapsDto>, ISeatMapQuery
    {
        public string? OfferId { get; set; }

        public long? OrderId { get; set; }
    }
}
