using MediatR;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers.Backoffice
{
    public sealed class BackofficePriceOrderAncillaryOffersCommandHandler : IRequestHandler<BackofficePriceOrderAncillaryOffersCommand, SelectionSnapshotResult>
    {
        private readonly IPriceAncillaryOffersService _service;

        public BackofficePriceOrderAncillaryOffersCommandHandler(IPriceAncillaryOffersService service) => _service = service;

        public Task<SelectionSnapshotResult> Handle(BackofficePriceOrderAncillaryOffersCommand command, CancellationToken cancellationToken)
            => _service.SubmitAsync(command, cancellationToken);
    }
}
