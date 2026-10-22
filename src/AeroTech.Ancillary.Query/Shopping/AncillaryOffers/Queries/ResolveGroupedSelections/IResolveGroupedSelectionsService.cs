using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.ResolveGroupedSelections
{
    public interface IResolveGroupedSelectionsService
    {
        Task<GroupedSelectionResolutionSnapshot> ExecuteAsync(GroupedSelectionRequest request, CancellationToken cancellationToken = default);
    }
}
