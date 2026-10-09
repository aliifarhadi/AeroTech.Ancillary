using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionAirportServiceRule
{
    public interface IChangeProvisionAirportServiceRuleService
    {
        Task<ProvisionResult> ChangeAsync(IChangeProvisionAirportServiceRuleCommand command, CancellationToken cancellationToken = default);
    }
}
