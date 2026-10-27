using AeroTech.Messages.Shared.Enums;
using MediatR;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers.Backoffice
{
    public sealed record BackofficePriceOrderAncillaryOffersCommand(
        long? OrderId,
        IReadOnlyList<AncillarySelectionLine> Selections) : IRequest<SelectionSnapshotResult>, IPriceOrderAncillaryOffersCommand
    {
        public AuthorizationSurface Surface => AuthorizationSurface.Backoffice;
    }
}
