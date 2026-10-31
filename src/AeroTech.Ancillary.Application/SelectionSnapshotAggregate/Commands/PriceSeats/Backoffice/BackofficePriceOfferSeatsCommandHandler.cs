using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers;
using MediatR;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceSeats.Backoffice
{
    public sealed class BackofficePriceOfferSeatsCommandHandler : IRequestHandler<BackofficePriceOfferSeatsCommand, SelectionSnapshotResult>
    {
        private readonly IPriceAncillaryOffersService _service;

        public BackofficePriceOfferSeatsCommandHandler(IPriceAncillaryOffersService service) => _service = service;

        public Task<SelectionSnapshotResult> Handle(BackofficePriceOfferSeatsCommand command, CancellationToken cancellationToken)
            => _service.SubmitAsync(command, cancellationToken);
    }
}
