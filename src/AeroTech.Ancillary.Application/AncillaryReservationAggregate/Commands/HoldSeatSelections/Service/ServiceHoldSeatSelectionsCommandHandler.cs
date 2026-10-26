using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldSeatSelections.Service
{
    public sealed class ServiceHoldSeatSelectionsCommandHandler : IRequestHandler<ServiceHoldSeatSelectionsCommand, AncillaryHoldReceipt>
    {
        private readonly IHoldSeatSelectionsService _service;

        public ServiceHoldSeatSelectionsCommandHandler(IHoldSeatSelectionsService service) => _service = service;

        public Task<AncillaryHoldReceipt> Handle(ServiceHoldSeatSelectionsCommand command, CancellationToken cancellationToken)
            => _service.HoldAsync(command, cancellationToken);
    }
}
