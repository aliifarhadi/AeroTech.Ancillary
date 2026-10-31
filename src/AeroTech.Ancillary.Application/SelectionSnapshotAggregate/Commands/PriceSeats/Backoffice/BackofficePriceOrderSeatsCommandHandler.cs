using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers;
using MediatR;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceSeats.Backoffice
{
    public sealed class BackofficePriceOrderSeatsCommandHandler : IRequestHandler<BackofficePriceOrderSeatsCommand, SelectionSnapshotResult>
    {
        private readonly IPriceAncillaryOffersService _service;

        public BackofficePriceOrderSeatsCommandHandler(IPriceAncillaryOffersService service) => _service = service;

        public Task<SelectionSnapshotResult> Handle(BackofficePriceOrderSeatsCommand command, CancellationToken cancellationToken)
            => _service.SubmitAsync(command, cancellationToken);
    }
}
