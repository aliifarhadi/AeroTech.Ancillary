namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionTravelDate
{
    public interface IAddProvisionTravelDateService
    {
        Task<ProvisionConditionRowResult> AddAsync(IAddProvisionTravelDateCommand command, CancellationToken cancellationToken = default);
    }
}
