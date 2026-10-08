namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionPermittedTravelPeriod
{
    public interface IAddProvisionPermittedTravelPeriodCommand
    {
        long ProvisionId { get; }

        DateOnly StartDate { get; }

        DateOnly EndDate { get; }
    }
}
