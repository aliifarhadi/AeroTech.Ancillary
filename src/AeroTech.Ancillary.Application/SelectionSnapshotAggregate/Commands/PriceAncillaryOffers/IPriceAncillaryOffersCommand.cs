using AeroTech.Messages.Shared.Enums;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers
{
    public interface IPriceAncillaryOffersCommand
    {
        AuthorizationSurface Surface { get; }

        string? OfferId { get; }

        IReadOnlyList<AncillarySelectionLine> Selections { get; }
    }
}
