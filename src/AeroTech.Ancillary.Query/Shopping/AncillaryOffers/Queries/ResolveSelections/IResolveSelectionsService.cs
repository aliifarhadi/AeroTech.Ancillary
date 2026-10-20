using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.ResolveSelections
{
    public interface IResolveSelectionsService
    {
        Task<SelectionResolutionSnapshot> ExecuteAsync(
            long? orderId,
            int? expectedOrderCommercialVersion,
            IReadOnlyList<string>? pricedSelectionRefs,
            CancellationToken cancellationToken = default);
    }
}
