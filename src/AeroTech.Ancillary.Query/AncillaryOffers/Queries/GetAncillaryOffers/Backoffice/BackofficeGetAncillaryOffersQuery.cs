using AeroTech.Ancillary.Query.AncillaryOffers.Dto.Backoffice;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers.Backoffice
{
    public sealed class BackofficeGetAncillaryOffersQuery : IRequest<BackofficeAncillaryOffersDto>, IAncillaryOffersQuery
    {
        public string? OfferId { get; set; }

        public BackofficeAncillaryOfferOptions? Options { get; set; }
    }

    public sealed class BackofficeAncillaryOfferOptions
    {
        public string? CustomerId { get; set; }

        public long? CountryId { get; set; }

        public long? PointOfSaleId { get; set; }

        public DateTimeOffset? SalesDate { get; set; }

        public int? PreferredCurrencyId { get; set; }
    }
}
