using AeroTech.Messages.Shared.Enums;
using MediatR;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers.Api
{
    public sealed record ApiPriceOrderAncillaryOffersCommand(
        long? OrderId,
        IReadOnlyList<AncillarySelectionLine> Selections) : IRequest<SelectionSnapshotResult>, IPriceOrderAncillaryOffersCommand
    {
        public AuthorizationSurface Surface => AuthorizationSurface.Api;
    }
}
