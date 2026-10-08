using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.AssignAncillaryServiceDefinitionServiceDateBasis.Backoffice
{
    public sealed class BackofficeAssignAncillaryServiceDefinitionServiceDateBasisCommandHandler
        : IRequestHandler<BackofficeAssignAncillaryServiceDefinitionServiceDateBasisCommand, ServiceDefinitionResult>
    {
        private readonly IAssignAncillaryServiceDefinitionServiceDateBasisService _service;

        public BackofficeAssignAncillaryServiceDefinitionServiceDateBasisCommandHandler(IAssignAncillaryServiceDefinitionServiceDateBasisService service) => _service = service;

        public Task<ServiceDefinitionResult> Handle(BackofficeAssignAncillaryServiceDefinitionServiceDateBasisCommand command, CancellationToken cancellationToken)
            => _service.AssignAsync(command, cancellationToken);
    }
}
