namespace AeroTech.Ancillary.Query.Shopping.SeatMaps.Snapshots
{
    public sealed record SeatMapResolutionSnapshot(
        long? OrderId,
        int? OrderCommercialVersion,
        DateTimeOffset ExpiresAtUtc,
        IReadOnlyList<ResolvedSeatSnapshot> Seats);
}
