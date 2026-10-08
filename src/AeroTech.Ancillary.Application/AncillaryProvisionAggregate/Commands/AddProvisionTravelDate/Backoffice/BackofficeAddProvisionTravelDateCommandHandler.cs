using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionTravelDate.Backoffice
{
    public sealed class BackofficeAddProvisionTravelDateCommandHandler
        : IRequestHandler<BackofficeAddProvisionTravelDateCommand, ProvisionConditionRowResult>
    {
        private readonly IAddProvisionTravelDateService _service;

        public BackofficeAddProvisionTravelDateCommandHandler(IAddProvisionTravelDateService service) => _service = service;

        public Task<ProvisionConditionRowResult> Handle(BackofficeAddProvisionTravelDateCommand command, CancellationToken cancellationToken)
            => _service.AddAsync(command, cancellationToken);
    }
}
