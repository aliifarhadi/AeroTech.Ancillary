using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Shopping.Selection;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Queries.EvaluateSelection
{
    public interface IEvaluateSelectionService
    {
        Task<AncillarySelectionSnapshot> ExecuteAsync(
            IAncillaryOfferItemQuery query,
            AncillarySelection selection,
            AncillarySalesScope scope,
            CancellationToken cancellationToken = default);
    }
}
