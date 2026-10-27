using AeroTech.Ancillary.Query.Shopping.SeatMaps.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.GetSeatMap.OtaPanel
{
    public sealed class OtaPanelGetSeatMapQuery : IRequest<SeatMapsDto>, ISeatMapQuery
    {
        public string? OfferId { get; set; }
    }
}
