using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.OtaPanel;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateBatch.OtaPanel
{
    public sealed class OtaPanelEvaluateBatchQuery : IRequest<OtaPanelSelectionBatchDto>, IAncillaryOffersQuery
    {
        public string? OfferId { get; set; }

        public long? OrderId { get; set; }

        public IReadOnlyList<OtaPanelSelectionLineInput>? Selections { get; set; }
    }
}
