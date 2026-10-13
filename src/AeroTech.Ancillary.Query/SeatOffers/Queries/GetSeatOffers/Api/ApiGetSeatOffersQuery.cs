using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.SeatOffers.Dto.Api;
using MediatR;

namespace AeroTech.Ancillary.Query.SeatOffers.Queries.GetSeatOffers.Api
{
    public sealed class ApiGetSeatOffersQuery : IRequest<ApiSeatOffersDto>, IAncillaryOffersQuery
    {
        public string? OfferId { get; set; }
    }
}
