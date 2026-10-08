using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;
using AeroTech.Messages.Ancillary.Enums;
using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.AssignAncillaryServiceDefinitionPricingUnit.Backoffice
{
    public sealed record BackofficeAssignAncillaryServiceDefinitionPricingUnitCommand(
        long ServiceDefinitionId,
        PricingUnit PricingUnit) : IRequest<ServiceDefinitionResult>, IAssignAncillaryServiceDefinitionPricingUnitCommand;
}
