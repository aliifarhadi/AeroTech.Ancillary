using MediatR;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers.OtaPanel
{
    public sealed class OtaPanelPriceAncillaryOffersCommandHandler : IRequestHandler<OtaPanelPriceAncillaryOffersCommand, SelectionSnapshotResult>
    {
        private readonly IPriceAncillaryOffersService _service;

        public OtaPanelPriceAncillaryOffersCommandHandler(IPriceAncillaryOffersService service) => _service = service;

        public Task<SelectionSnapshotResult> Handle(OtaPanelPriceAncillaryOffersCommand command, CancellationToken cancellationToken)
            => _service.SubmitAsync(command, cancellationToken);
    }
}
