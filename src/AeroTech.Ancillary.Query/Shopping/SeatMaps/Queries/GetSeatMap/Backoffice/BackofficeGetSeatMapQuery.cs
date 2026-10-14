using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.GetSeatMap.Backoffice
{
    public sealed class BackofficeGetSeatMapQuery : IRequest<SeatMapsDto>, ISeatMapQuery
    {
        public string? OfferId { get; set; }

        public BackofficeAncillaryOfferOptions? Options { get; set; }
    }
}
