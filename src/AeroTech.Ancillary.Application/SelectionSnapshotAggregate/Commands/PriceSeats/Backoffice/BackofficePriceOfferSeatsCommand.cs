using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers;
using AeroTech.Messages.Shared.Enums;
using MediatR;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceSeats.Backoffice
{
    public sealed record BackofficePriceOfferSeatsCommand(
        string? OfferId,
        IReadOnlyList<string> Ids) : IRequest<SelectionSnapshotResult>, IPriceOfferSeatsCommand
    {
        public AuthorizationSurface Surface => AuthorizationSurface.Backoffice;
    }
}
