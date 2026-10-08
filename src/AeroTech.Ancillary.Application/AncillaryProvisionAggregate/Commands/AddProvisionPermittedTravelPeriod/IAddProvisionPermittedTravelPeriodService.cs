namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionPermittedTravelPeriod
{
    public interface IAddProvisionPermittedTravelPeriodService
    {
        Task<ProvisionRuleRowResult> AddAsync(IAddProvisionPermittedTravelPeriodCommand command, CancellationToken cancellationToken = default);
    }
}
