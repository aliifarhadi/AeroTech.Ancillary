using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.RetireAncillaryServiceDefinition.Backoffice
{
    public sealed class BackofficeRetireAncillaryServiceDefinitionCommandHandler
        : IRequestHandler<BackofficeRetireAncillaryServiceDefinitionCommand, ServiceDefinitionResult>
    {
        private readonly IRetireAncillaryServiceDefinitionService _service;

        public BackofficeRetireAncillaryServiceDefinitionCommandHandler(IRetireAncillaryServiceDefinitionService service) => _service = service;

        public Task<ServiceDefinitionResult> Handle(BackofficeRetireAncillaryServiceDefinitionCommand command, CancellationToken cancellationToken)
            => _service.RetireAsync(command, cancellationToken);
    }
}
