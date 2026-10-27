using AeroTech.Ancillary.Query.Shopping.SeatMaps.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.GetOrderSeatMap.Api
{
    public sealed class ApiGetOrderSeatMapQuery : IRequest<OrderSeatMapsDto>, IOrderSeatMapQuery
    {
        public long? OrderId { get; set; }
    }
}
