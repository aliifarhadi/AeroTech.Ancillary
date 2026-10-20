using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Api;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.Shopping.Common;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateSelection.Api
{
    public sealed class ApiEvaluateSelectionQuery : IRequest<ApiSelectionResultDto>, IAncillaryOfferItemQuery
    {
        public string? OfferId { get; set; }

        long? IAncillaryOffersQuery.OrderId => null;

        public string? OfferItemRef { get; set; }

        public ApiAncillarySelectionInput? Selection { get; set; }
    }
}
