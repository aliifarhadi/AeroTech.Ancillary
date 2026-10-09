using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionPetRule
{
    public interface IChangeProvisionPetRuleService
    {
        Task<ProvisionResult> ChangeAsync(IChangeProvisionPetRuleCommand command, CancellationToken cancellationToken = default);
    }
}
