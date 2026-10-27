using AeroTech.Ancillary.Shopping.Resolution;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.ResolveAncillarySelection
{
    public interface IResolveAncillarySelectionService
    {
        Task<ResolvedSelections> ExecuteAsync(string? selectionId, CancellationToken cancellationToken = default);
    }
}
