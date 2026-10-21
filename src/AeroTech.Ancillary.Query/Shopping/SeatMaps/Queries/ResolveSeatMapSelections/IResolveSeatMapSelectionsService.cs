using AeroTech.Ancillary.Query.Shopping.SeatMaps.Snapshots;

namespace AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.ResolveSeatMapSelections
{
    public interface IResolveSeatMapSelectionsService
    {
        Task<SeatMapResolutionSnapshot> ExecuteAsync(IReadOnlyList<string>? ids, CancellationToken cancellationToken = default);
    }
}
