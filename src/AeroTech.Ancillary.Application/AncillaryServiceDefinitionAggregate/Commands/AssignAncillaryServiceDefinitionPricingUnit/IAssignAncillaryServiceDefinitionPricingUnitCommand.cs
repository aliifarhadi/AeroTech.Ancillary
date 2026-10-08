using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.AssignAncillaryServiceDefinitionPricingUnit
{
    public interface IAssignAncillaryServiceDefinitionPricingUnitCommand
    {
        long ServiceDefinitionId { get; }

        PricingUnit PricingUnit { get; }
    }
}
