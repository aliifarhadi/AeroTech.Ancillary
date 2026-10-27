using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.GetOrderSeatMap.Backoffice
{
    public sealed class BackofficeGetOrderSeatMapQuery : IRequest<OrderSeatMapsDto>, IOrderSeatMapQuery
    {
        public long? OrderId { get; set; }

        public BackofficeAncillaryOfferOptions? Options { get; set; }
    }
}
