using AeroTech.Ancillary.Query.AncillaryOffers.Queries.EvaluateSelection.OtaPanel;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.SeatOffers.Dto.OtaPanel;
using MediatR;

namespace AeroTech.Ancillary.Query.SeatOffers.Queries.EvaluateSeatSelection.OtaPanel
{
    public sealed class OtaPanelEvaluateSeatSelectionQuery : IRequest<OtaPanelSeatSelectionResultDto>, IAncillaryOfferItemQuery
    {
        public string? OfferId { get; set; }

        public string? OfferItemRef { get; set; }

        public OtaPanelAncillarySelectionInput? Selection { get; set; }
    }
}
