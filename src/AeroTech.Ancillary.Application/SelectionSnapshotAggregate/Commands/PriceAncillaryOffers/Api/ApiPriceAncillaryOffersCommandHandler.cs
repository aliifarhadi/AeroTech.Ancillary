using MediatR;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers.Api
{
    public sealed class ApiPriceAncillaryOffersCommandHandler : IRequestHandler<ApiPriceAncillaryOffersCommand, SelectionSnapshotResult>
    {
        private readonly IPriceAncillaryOffersService _service;

        public ApiPriceAncillaryOffersCommandHandler(IPriceAncillaryOffersService service) => _service = service;

        public Task<SelectionSnapshotResult> Handle(ApiPriceAncillaryOffersCommand command, CancellationToken cancellationToken)
            => _service.SubmitAsync(command, cancellationToken);
    }
}
