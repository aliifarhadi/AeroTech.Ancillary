using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;

namespace AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.AssignAncillaryServiceDefinitionPricingUnit
{
    public interface IAssignAncillaryServiceDefinitionPricingUnitService
    {
        Task<ServiceDefinitionResult> AssignAsync(IAssignAncillaryServiceDefinitionPricingUnitCommand command, CancellationToken cancellationToken = default);
    }
}
