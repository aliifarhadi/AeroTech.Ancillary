using MediatR;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers.Backoffice
{
    public sealed class BackofficePriceAncillaryOffersCommandHandler : IRequestHandler<BackofficePriceAncillaryOffersCommand, SelectionSnapshotResult>
    {
        private readonly IPriceAncillaryOffersService _service;

        public BackofficePriceAncillaryOffersCommandHandler(IPriceAncillaryOffersService service) => _service = service;

        public Task<SelectionSnapshotResult> Handle(BackofficePriceAncillaryOffersCommand command, CancellationToken cancellationToken)
            => _service.SubmitAsync(command, cancellationToken);
    }
}
