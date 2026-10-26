using AeroTech.Ancillary.Shopping.Resolution;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.ResolveAncillarySelection
{
    public interface IResolveAncillarySelectionService
    {
        Task<IReadOnlyList<ResolvedSelection>> ExecuteAsync(string? selectionId, CancellationToken cancellationToken = default);
    }
}
