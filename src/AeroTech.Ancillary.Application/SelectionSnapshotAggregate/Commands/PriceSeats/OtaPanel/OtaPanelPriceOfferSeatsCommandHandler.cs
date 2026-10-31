using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers;
using MediatR;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceSeats.OtaPanel
{
    public sealed class OtaPanelPriceOfferSeatsCommandHandler : IRequestHandler<OtaPanelPriceOfferSeatsCommand, SelectionSnapshotResult>
    {
        private readonly IPriceAncillaryOffersService _service;

        public OtaPanelPriceOfferSeatsCommandHandler(IPriceAncillaryOffersService service) => _service = service;

        public Task<SelectionSnapshotResult> Handle(OtaPanelPriceOfferSeatsCommand command, CancellationToken cancellationToken)
            => _service.SubmitAsync(command, cancellationToken);
    }
}
