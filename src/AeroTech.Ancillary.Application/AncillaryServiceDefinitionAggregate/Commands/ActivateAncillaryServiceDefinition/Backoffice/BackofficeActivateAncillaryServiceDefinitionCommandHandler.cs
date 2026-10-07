using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.ActivateAncillaryServiceDefinition.Backoffice
{
    public sealed class BackofficeActivateAncillaryServiceDefinitionCommandHandler
        : IRequestHandler<BackofficeActivateAncillaryServiceDefinitionCommand, ServiceDefinitionResult>
    {
        private readonly IActivateAncillaryServiceDefinitionService _service;

        public BackofficeActivateAncillaryServiceDefinitionCommandHandler(IActivateAncillaryServiceDefinitionService service)
            => _service = service;

        public Task<ServiceDefinitionResult> Handle(BackofficeActivateAncillaryServiceDefinitionCommand command, CancellationToken cancellationToken)
            => _service.ActivateAsync(command, cancellationToken);
    }
}
