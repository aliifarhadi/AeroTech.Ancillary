using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.SeatMaps.Snapshots
{
    public sealed record SeatMapResolutionSnapshot(
        long? OrderId,
        int? OrderCommercialVersion,
        ShoppingSourceKind SourceKind,
        string SourceReference,
        DateTimeOffset ExpiresAtUtc,
        int SelectedLineCount,
        IReadOnlyList<ResolvedSeatSnapshot> Seats);
}
