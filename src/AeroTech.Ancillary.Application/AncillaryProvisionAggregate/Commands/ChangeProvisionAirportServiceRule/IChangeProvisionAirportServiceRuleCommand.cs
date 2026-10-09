using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionAirportServiceRule
{
    public interface IChangeProvisionAirportServiceRuleCommand
    {
        long ProvisionId { get; }

        ProvisionAirportServiceRuleInput? AirportServiceRule { get; }
    }
}
