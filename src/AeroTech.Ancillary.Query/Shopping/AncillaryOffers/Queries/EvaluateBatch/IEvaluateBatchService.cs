using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateBatch
{
    public interface IEvaluateBatchService
    {
        Task<SelectionBatchSnapshot> ExecuteAsync(
            IAncillaryOffersQuery query,
            IReadOnlyList<SelectionLineRequest>? lines,
            AncillarySalesScope scope,
            CancellationToken cancellationToken = default);
    }
}
