namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionBlackoutPeriod
{
    public interface IAddProvisionBlackoutPeriodCommand
    {
        long ProvisionId { get; }

        DateOnly StartDate { get; }

        DateOnly EndDate { get; }
    }
}
