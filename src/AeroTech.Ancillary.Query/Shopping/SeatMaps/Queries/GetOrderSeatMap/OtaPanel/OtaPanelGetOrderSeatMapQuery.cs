using AeroTech.Ancillary.Query.Shopping.SeatMaps.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.GetOrderSeatMap.OtaPanel
{
    public sealed class OtaPanelGetOrderSeatMapQuery : IRequest<OrderSeatMapsDto>, IOrderSeatMapQuery
    {
        public long? OrderId { get; set; }
    }
}
