using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.ReviseAncillaryServiceDefinition.Backoffice
{
    public sealed class BackofficeReviseAncillaryServiceDefinitionCommandHandler
        : IRequestHandler<BackofficeReviseAncillaryServiceDefinitionCommand, ServiceDefinitionResult>
    {
        private readonly IReviseAncillaryServiceDefinitionService _service;

        public BackofficeReviseAncillaryServiceDefinitionCommandHandler(IReviseAncillaryServiceDefinitionService service) => _service = service;

        public Task<ServiceDefinitionResult> Handle(BackofficeReviseAncillaryServiceDefinitionCommand command, CancellationToken cancellationToken)
            => _service.ReviseAsync(command, cancellationToken);
    }
}
