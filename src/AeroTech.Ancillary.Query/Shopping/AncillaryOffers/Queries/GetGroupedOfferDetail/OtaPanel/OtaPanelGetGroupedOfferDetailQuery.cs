using System.Text.Json.Serialization;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.OtaPanel;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateSelection.OtaPanel;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Messages.Ancillary.Enums;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetGroupedOfferDetail.OtaPanel
{
    public sealed class OtaPanelGetGroupedOfferDetailQuery : IRequest<OtaPanelGroupedOfferDetailDto>, IAncillaryOfferItemQuery
    {
        public string? OfferId { get; set; }

        public long? OrderId { get; set; }

        public string? OfferItemRef { get; set; }

        public OtaPanelAncillarySelectionInput? Selection { get; set; }

        [JsonIgnore]
        public AncillaryProfile Family { get; set; }
    }
}
