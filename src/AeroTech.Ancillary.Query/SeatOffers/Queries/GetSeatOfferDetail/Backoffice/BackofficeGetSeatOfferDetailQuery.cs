using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers.Backoffice;
using AeroTech.Ancillary.Query.SeatOffers.Dto.Backoffice;
using MediatR;

namespace AeroTech.Ancillary.Query.SeatOffers.Queries.GetSeatOfferDetail.Backoffice
{
    public sealed class BackofficeGetSeatOfferDetailQuery : IRequest<BackofficeSeatOfferDetailDto>, IAncillaryOfferItemQuery
    {
        public string? OfferId { get; set; }

        public string? OfferItemRef { get; set; }

        public BackofficeAncillaryOfferOptions? Options { get; set; }
    }
}
