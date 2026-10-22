using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldGroupedSelections.Service
{
    public sealed class ServiceHoldGroupedSelectionsCommandHandler : IRequestHandler<ServiceHoldGroupedSelectionsCommand, GroupedHoldResult>
    {
        private readonly IHoldGroupedSelectionsService _service;

        public ServiceHoldGroupedSelectionsCommandHandler(IHoldGroupedSelectionsService service) => _service = service;

        public Task<GroupedHoldResult> Handle(ServiceHoldGroupedSelectionsCommand command, CancellationToken cancellationToken)
            => _service.HoldAsync(command, cancellationToken);
    }
}
