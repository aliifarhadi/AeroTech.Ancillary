using MediatR;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers.OtaPanel
{
    public sealed class OtaPanelPriceOrderAncillaryOffersCommandHandler : IRequestHandler<OtaPanelPriceOrderAncillaryOffersCommand, SelectionSnapshotResult>
    {
        private readonly IPriceAncillaryOffersService _service;

        public OtaPanelPriceOrderAncillaryOffersCommandHandler(IPriceAncillaryOffersService service) => _service = service;

        public Task<SelectionSnapshotResult> Handle(OtaPanelPriceOrderAncillaryOffersCommand command, CancellationToken cancellationToken)
            => _service.SubmitAsync(command, cancellationToken);
    }
}
