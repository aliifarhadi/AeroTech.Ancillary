using AeroTech.Ancillary.Query.AncillaryOffers.Queries.EvaluateSelection.Api;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.SeatOffers.Dto.Api;
using MediatR;

namespace AeroTech.Ancillary.Query.SeatOffers.Queries.EvaluateSeatSelection.Api
{
    public sealed class ApiEvaluateSeatSelectionQuery : IRequest<ApiSeatSelectionResultDto>, IAncillaryOfferItemQuery
    {
        public string? OfferId { get; set; }

        public string? OfferItemRef { get; set; }

        public ApiAncillarySelectionInput? Selection { get; set; }
    }
}
