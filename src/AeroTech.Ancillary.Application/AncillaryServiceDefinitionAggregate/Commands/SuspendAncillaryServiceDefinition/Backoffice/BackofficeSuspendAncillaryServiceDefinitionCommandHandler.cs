using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.SuspendAncillaryServiceDefinition.Backoffice
{
    public sealed class BackofficeSuspendAncillaryServiceDefinitionCommandHandler
        : IRequestHandler<BackofficeSuspendAncillaryServiceDefinitionCommand, ServiceDefinitionResult>
    {
        private readonly ISuspendAncillaryServiceDefinitionService _service;

        public BackofficeSuspendAncillaryServiceDefinitionCommandHandler(ISuspendAncillaryServiceDefinitionService service) => _service = service;

        public Task<ServiceDefinitionResult> Handle(BackofficeSuspendAncillaryServiceDefinitionCommand command, CancellationToken cancellationToken)
            => _service.SuspendAsync(command, cancellationToken);
    }
}
