namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RemoveProvisionBlackoutPeriod
{
    public interface IRemoveProvisionBlackoutPeriodCommand
    {
        long ProvisionId { get; }

        long RowId { get; }
    }
}
