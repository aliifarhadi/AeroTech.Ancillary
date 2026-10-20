using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using MediatR;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateBatch.Backoffice
{
    public sealed class BackofficeEvaluateBatchQuery : IRequest<BackofficeSelectionBatchDto>, IAncillaryOffersQuery
    {
        public string? OfferId { get; set; }

        public long? OrderId { get; set; }

        public IReadOnlyList<BackofficeSelectionLineInput>? Selections { get; set; }

        public BackofficeAncillaryOfferOptions? Options { get; set; }
    }
}
