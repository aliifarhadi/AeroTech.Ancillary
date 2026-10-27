using AeroTech.Messages.Shared.Enums;
using MediatR;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers.OtaPanel
{
    public sealed record OtaPanelPriceOrderAncillaryOffersCommand(
        long? OrderId,
        IReadOnlyList<AncillarySelectionLine> Selections) : IRequest<SelectionSnapshotResult>, IPriceOrderAncillaryOffersCommand
    {
        public AuthorizationSurface Surface => AuthorizationSurface.OtaPanel;
    }
}
