using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers.Backoffice;
using AeroTech.Ancillary.Query.SeatOffers.Dto.Backoffice;
using MediatR;

namespace AeroTech.Ancillary.Query.SeatOffers.Queries.GetSeatOffers.Backoffice
{
    public sealed class BackofficeGetSeatOffersQuery : IRequest<BackofficeSeatOffersDto>, IAncillaryOffersQuery
    {
        public string? OfferId { get; set; }

        public BackofficeAncillaryOfferOptions? Options { get; set; }
    }
}
