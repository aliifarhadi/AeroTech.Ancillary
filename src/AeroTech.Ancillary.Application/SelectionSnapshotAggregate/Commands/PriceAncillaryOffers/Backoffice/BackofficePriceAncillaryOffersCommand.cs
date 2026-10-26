using AeroTech.Messages.Shared.Enums;
using MediatR;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers.Backoffice
{
    public sealed record BackofficePriceAncillaryOffersCommand(
        string? OfferId,
        IReadOnlyList<AncillarySelectionLine> Selections) : IRequest<SelectionSnapshotResult>, IPriceAncillaryOffersCommand
    {
        public AuthorizationSurface Surface => AuthorizationSurface.Backoffice;
    }
}
