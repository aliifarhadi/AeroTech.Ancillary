using AeroTech.Messages.Shared.Enums;
using MediatR;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers.Api
{
    public sealed record ApiPriceAncillaryOffersCommand(
        string? OfferId,
        IReadOnlyList<AncillarySelectionLine> Selections) : IRequest<SelectionSnapshotResult>, IPriceAncillaryOffersCommand
    {
        public AuthorizationSurface Surface => AuthorizationSurface.Api;
    }
}
