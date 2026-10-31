using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers;
using MediatR;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceSeats.OtaPanel
{
    public sealed class OtaPanelPriceOrderSeatsCommandHandler : IRequestHandler<OtaPanelPriceOrderSeatsCommand, SelectionSnapshotResult>
    {
        private readonly IPriceAncillaryOffersService _service;

        public OtaPanelPriceOrderSeatsCommandHandler(IPriceAncillaryOffersService service) => _service = service;

        public Task<SelectionSnapshotResult> Handle(OtaPanelPriceOrderSeatsCommand command, CancellationToken cancellationToken)
            => _service.SubmitAsync(command, cancellationToken);
    }
}
