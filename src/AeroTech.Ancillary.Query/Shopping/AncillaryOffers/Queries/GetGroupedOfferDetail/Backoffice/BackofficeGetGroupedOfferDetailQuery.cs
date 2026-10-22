using System.Text.Json.Serialization;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateSelection.Backoffice;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Messages.Ancillary.Enums;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetGroupedOfferDetail.Backoffice
{
    public sealed class BackofficeGetGroupedOfferDetailQuery : IRequest<BackofficeGroupedOfferDetailDto>, IAncillaryOfferItemQuery
    {
        public string? OfferId { get; set; }

        public long? OrderId { get; set; }

        public string? OfferItemRef { get; set; }

        public BackofficeAncillarySelectionInput? Selection { get; set; }

        public BackofficeAncillaryOfferOptions? Options { get; set; }

        [JsonIgnore]
        public AncillaryProfile Family { get; set; }
    }
}
