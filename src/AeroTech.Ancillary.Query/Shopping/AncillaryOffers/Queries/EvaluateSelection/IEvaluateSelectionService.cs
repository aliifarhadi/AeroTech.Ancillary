using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Selection;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateSelection
{
    public interface IEvaluateSelectionService
    {
        Task<AncillarySelectionSnapshot> ExecuteAsync(
            IAncillaryOfferItemQuery query,
            AncillarySelection selection,
            AncillarySalesScope scope,
            CancellationToken cancellationToken = default);

        Task<AncillarySelectionSnapshot> ExecuteAsync(
            IAncillaryOfferItemQuery query,
            AncillarySelection selection,
            AncillarySalesScope scope,
            ShoppingFilter family,
            CancellationToken cancellationToken = default);

        Task<AncillarySelectionSnapshot> EvaluateAsync(AncillaryOfferDetailSnapshot detail, AncillarySelection selection, CancellationToken cancellationToken = default);
    }
}
