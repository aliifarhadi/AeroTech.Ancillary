using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers;
using AeroTech.Messages.Shared.Enums;
using MediatR;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceSeats.Api
{
    public sealed record ApiPriceOfferSeatsCommand(
        string? OfferId,
        IReadOnlyList<string> Ids) : IRequest<SelectionSnapshotResult>, IPriceOfferSeatsCommand
    {
        public AuthorizationSurface Surface => AuthorizationSurface.Api;
    }
}
