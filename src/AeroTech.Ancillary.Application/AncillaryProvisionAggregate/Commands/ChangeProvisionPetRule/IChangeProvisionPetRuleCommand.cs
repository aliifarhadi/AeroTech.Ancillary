using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionPetRule
{
    public interface IChangeProvisionPetRuleCommand
    {
        long ProvisionId { get; }

        ProvisionPetRuleInput? PetRule { get; }
    }
}
