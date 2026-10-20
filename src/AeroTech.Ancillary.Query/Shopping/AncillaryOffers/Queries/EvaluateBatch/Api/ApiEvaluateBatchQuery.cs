using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Api;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateBatch.Api
{
    public sealed class ApiEvaluateBatchQuery : IRequest<ApiSelectionBatchDto>, IAncillaryOffersQuery
    {
        public string? OfferId { get; set; }

        public long? OrderId { get; set; }

        public IReadOnlyList<ApiSelectionLineInput>? Selections { get; set; }
    }
}
