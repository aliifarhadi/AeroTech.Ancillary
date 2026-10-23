using AeroTech.Ancillary.Shopping.Resolution;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.ResolveGroupedSelections
{
    public interface IResolveGroupedSelectionsService
    {
        Task<IReadOnlyList<ResolvedSelection>> ExecuteAsync(IReadOnlyList<string>? ids, CancellationToken cancellationToken = default);
    }
}
