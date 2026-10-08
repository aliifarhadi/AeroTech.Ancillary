using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionPassengerEligibility
{
    public interface IChangeProvisionPassengerEligibilityCommand
    {
        long ProvisionId { get; }

        ProvisionPassengerEligibilityInput? PassengerEligibility { get; }
    }
}
