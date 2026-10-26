using AeroTech.Messages.Shared.Enums;
using MediatR;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers.OtaPanel
{
    public sealed record OtaPanelPriceAncillaryOffersCommand(
        string? OfferId,
        IReadOnlyList<AncillarySelectionLine> Selections) : IRequest<SelectionSnapshotResult>, IPriceAncillaryOffersCommand
    {
        public AuthorizationSurface Surface => AuthorizationSurface.OtaPanel;
    }
}
