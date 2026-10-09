using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionAssistedTravelRule
{
    public interface IChangeProvisionAssistedTravelRuleService
    {
        Task<ProvisionResult> ChangeAsync(IChangeProvisionAssistedTravelRuleCommand command, CancellationToken cancellationToken = default);
    }
}
