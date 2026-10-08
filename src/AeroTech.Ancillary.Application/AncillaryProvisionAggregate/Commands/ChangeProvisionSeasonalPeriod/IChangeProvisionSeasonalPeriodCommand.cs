namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionSeasonalPeriod
{
    public interface IChangeProvisionSeasonalPeriodCommand
    {
        long ProvisionId { get; }

        long RowId { get; }

        DateOnly StartDate { get; }

        DateOnly EndDate { get; }
    }
}
