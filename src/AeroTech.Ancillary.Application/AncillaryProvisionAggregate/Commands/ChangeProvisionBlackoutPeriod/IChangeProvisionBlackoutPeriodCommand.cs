namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionBlackoutPeriod
{
    public interface IChangeProvisionBlackoutPeriodCommand
    {
        long ProvisionId { get; }

        long RowId { get; }

        DateOnly StartDate { get; }

        DateOnly EndDate { get; }
    }
}
