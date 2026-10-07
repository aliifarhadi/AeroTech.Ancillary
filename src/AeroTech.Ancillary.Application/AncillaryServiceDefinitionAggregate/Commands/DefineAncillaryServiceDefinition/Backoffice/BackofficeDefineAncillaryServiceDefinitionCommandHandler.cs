using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition.Backoffice
{
    public sealed class BackofficeDefineAncillaryServiceDefinitionCommandHandler
        : IRequestHandler<BackofficeDefineAncillaryServiceDefinitionCommand, ServiceDefinitionResult>
    {
        private readonly IDefineAncillaryServiceDefinitionService _service;

        public BackofficeDefineAncillaryServiceDefinitionCommandHandler(IDefineAncillaryServiceDefinitionService service)
            => _service = service;

        public Task<ServiceDefinitionResult> Handle(BackofficeDefineAncillaryServiceDefinitionCommand command, CancellationToken cancellationToken)
            => _service.DefineAsync(command, cancellationToken);
    }
}
