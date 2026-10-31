using AeroTech.Ancillary.Query.Shopping.SeatMaps.Snapshots;

namespace AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.ResolveSeatMapSelections
{
    public interface IResolveSeatMapSelectionsService
    {
        Task<SeatMapResolutionSnapshot> ExecuteAsync(string? selectionId, CancellationToken cancellationToken = default);
    }
}
