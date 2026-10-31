using AeroTech.Messages.Shared.Enums;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Services
{
    public sealed record OrderSeatPriceVerification(
        AuthorizationSurface Surface,
        long OrderId,
        IReadOnlyList<string> Ids);
}
