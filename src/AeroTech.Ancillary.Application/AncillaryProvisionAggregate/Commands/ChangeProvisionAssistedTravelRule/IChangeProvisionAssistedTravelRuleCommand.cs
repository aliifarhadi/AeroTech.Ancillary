using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionAssistedTravelRule
{
    public interface IChangeProvisionAssistedTravelRuleCommand
    {
        long ProvisionId { get; }

        ProvisionAssistedTravelRuleInput? AssistedTravelRule { get; }
    }
}
