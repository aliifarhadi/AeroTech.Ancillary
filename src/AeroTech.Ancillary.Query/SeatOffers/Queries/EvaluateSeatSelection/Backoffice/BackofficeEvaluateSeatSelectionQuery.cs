using AeroTech.Ancillary.Query.AncillaryOffers.Queries.EvaluateSelection.Backoffice;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers.Backoffice;
using AeroTech.Ancillary.Query.SeatOffers.Dto.Backoffice;
using MediatR;

namespace AeroTech.Ancillary.Query.SeatOffers.Queries.EvaluateSeatSelection.Backoffice
{
    public sealed class BackofficeEvaluateSeatSelectionQuery : IRequest<BackofficeSeatSelectionResultDto>, IAncillaryOfferItemQuery
    {
        public string? OfferId { get; set; }

        public string? OfferItemRef { get; set; }

        public BackofficeAncillarySelectionInput? Selection { get; set; }

        public BackofficeAncillaryOfferOptions? Options { get; set; }
    }
}
