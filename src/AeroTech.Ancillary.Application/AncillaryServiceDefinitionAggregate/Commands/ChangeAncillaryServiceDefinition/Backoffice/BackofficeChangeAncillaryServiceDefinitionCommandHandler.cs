using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.ChangeAncillaryServiceDefinition.Backoffice
{
    public sealed class BackofficeChangeAncillaryServiceDefinitionCommandHandler
        : IRequestHandler<BackofficeChangeAncillaryServiceDefinitionCommand, ServiceDefinitionResult>
    {
        private readonly IChangeAncillaryServiceDefinitionService _service;

        public BackofficeChangeAncillaryServiceDefinitionCommandHandler(IChangeAncillaryServiceDefinitionService service)
            => _service = service;

        public Task<ServiceDefinitionResult> Handle(BackofficeChangeAncillaryServiceDefinitionCommand command, CancellationToken cancellationToken)
            => _service.ChangeAsync(command, cancellationToken);
    }
}
