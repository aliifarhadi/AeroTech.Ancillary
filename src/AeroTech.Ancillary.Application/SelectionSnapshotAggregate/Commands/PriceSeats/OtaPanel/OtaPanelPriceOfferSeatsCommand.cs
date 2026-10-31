using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers;
using AeroTech.Messages.Shared.Enums;
using MediatR;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceSeats.OtaPanel
{
    public sealed record OtaPanelPriceOfferSeatsCommand(
        string? OfferId,
        IReadOnlyList<string> Ids) : IRequest<SelectionSnapshotResult>, IPriceOfferSeatsCommand
    {
        public AuthorizationSurface Surface => AuthorizationSurface.OtaPanel;
    }
}
