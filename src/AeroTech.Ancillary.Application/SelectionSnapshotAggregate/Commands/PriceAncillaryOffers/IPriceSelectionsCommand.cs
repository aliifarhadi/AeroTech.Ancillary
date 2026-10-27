using AeroTech.Messages.Shared.Enums;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers
{
    public interface IPriceSelectionsCommand
    {
        AuthorizationSurface Surface { get; }

        IReadOnlyList<AncillarySelectionLine> Selections { get; }
    }
}
