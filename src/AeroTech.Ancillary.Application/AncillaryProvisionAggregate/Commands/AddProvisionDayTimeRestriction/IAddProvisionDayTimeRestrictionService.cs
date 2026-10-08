using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionTravelDate;

namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionDayTimeRestriction
{
    public interface IAddProvisionDayTimeRestrictionService
    {
        Task<ProvisionConditionRowResult> AddAsync(IAddProvisionDayTimeRestrictionCommand command, CancellationToken cancellationToken = default);
    }
}
