using AeroTech.Messages.Shared.Enums;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceSeats
{
    public interface IPriceSeatsCommand
    {
        AuthorizationSurface Surface { get; }

        IReadOnlyList<string> Ids { get; }
    }
}
