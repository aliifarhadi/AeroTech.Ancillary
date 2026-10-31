using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers;
using MediatR;

namespace AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceSeats.Api
{
    public sealed class ApiPriceOrderSeatsCommandHandler : IRequestHandler<ApiPriceOrderSeatsCommand, SelectionSnapshotResult>
    {
        private readonly IPriceAncillaryOffersService _service;

        public ApiPriceOrderSeatsCommandHandler(IPriceAncillaryOffersService service) => _service = service;

        public Task<SelectionSnapshotResult> Handle(ApiPriceOrderSeatsCommand command, CancellationToken cancellationToken)
            => _service.SubmitAsync(command, cancellationToken);
    }
}
