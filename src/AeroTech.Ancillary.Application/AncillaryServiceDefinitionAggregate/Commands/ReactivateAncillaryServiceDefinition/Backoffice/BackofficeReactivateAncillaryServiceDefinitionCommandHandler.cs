using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.ReactivateAncillaryServiceDefinition.Backoffice
{
    public sealed class BackofficeReactivateAncillaryServiceDefinitionCommandHandler
        : IRequestHandler<BackofficeReactivateAncillaryServiceDefinitionCommand, ServiceDefinitionResult>
    {
        private readonly IReactivateAncillaryServiceDefinitionService _service;

        public BackofficeReactivateAncillaryServiceDefinitionCommandHandler(IReactivateAncillaryServiceDefinitionService service) => _service = service;

        public Task<ServiceDefinitionResult> Handle(BackofficeReactivateAncillaryServiceDefinitionCommand command, CancellationToken cancellationToken)
            => _service.ReactivateAsync(command, cancellationToken);
    }
}
