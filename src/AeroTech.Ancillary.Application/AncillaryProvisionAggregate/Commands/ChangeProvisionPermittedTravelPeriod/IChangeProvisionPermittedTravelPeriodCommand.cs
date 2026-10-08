namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionPermittedTravelPeriod
{
    public interface IChangeProvisionPermittedTravelPeriodCommand
    {
        long ProvisionId { get; }

        long RowId { get; }

        DateOnly StartDate { get; }

        DateOnly EndDate { get; }
    }
}
