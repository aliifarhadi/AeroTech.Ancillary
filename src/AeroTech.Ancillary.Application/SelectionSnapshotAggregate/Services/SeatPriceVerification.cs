using AeroTech.Messages.Shared.Enums;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Services
{
    public sealed record SeatPriceVerification(
        AuthorizationSurface Surface,
        string? OfferId,
        IReadOnlyList<string> Ids);
}
