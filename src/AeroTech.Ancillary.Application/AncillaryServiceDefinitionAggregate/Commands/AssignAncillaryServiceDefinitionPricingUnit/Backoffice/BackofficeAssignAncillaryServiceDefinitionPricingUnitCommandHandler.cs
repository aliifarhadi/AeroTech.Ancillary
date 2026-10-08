using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.AssignAncillaryServiceDefinitionPricingUnit.Backoffice
{
    public sealed class BackofficeAssignAncillaryServiceDefinitionPricingUnitCommandHandler
        : IRequestHandler<BackofficeAssignAncillaryServiceDefinitionPricingUnitCommand, ServiceDefinitionResult>
    {
        private readonly IAssignAncillaryServiceDefinitionPricingUnitService _service;

        public BackofficeAssignAncillaryServiceDefinitionPricingUnitCommandHandler(IAssignAncillaryServiceDefinitionPricingUnitService service) => _service = service;

        public Task<ServiceDefinitionResult> Handle(BackofficeAssignAncillaryServiceDefinitionPricingUnitCommand command, CancellationToken cancellationToken)
            => _service.AssignAsync(command, cancellationToken);
    }
}
