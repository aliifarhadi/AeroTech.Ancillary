namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionSeasonalPeriod
{
    public interface IAddProvisionSeasonalPeriodCommand
    {
        long ProvisionId { get; }

        DateOnly StartDate { get; }

        DateOnly EndDate { get; }
    }
}
