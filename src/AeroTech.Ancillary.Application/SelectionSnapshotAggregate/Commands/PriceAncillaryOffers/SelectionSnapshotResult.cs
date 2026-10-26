namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers
{
    public sealed record SelectionSnapshotResult(string SelectionId, DateTimeOffset ExpiresAtUtc);
}
