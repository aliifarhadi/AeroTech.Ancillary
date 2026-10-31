using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers;
using AeroTech.Messages.Shared.Enums;
using MediatR;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceSeats.Api
{
    public sealed record ApiPriceOrderSeatsCommand(
        long? OrderId,
        IReadOnlyList<string> Ids) : IRequest<SelectionSnapshotResult>, IPriceOrderSeatsCommand
    {
        public AuthorizationSurface Surface => AuthorizationSurface.Api;
    }
}
