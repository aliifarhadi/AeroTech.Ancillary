namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RemoveProvisionDayTimeWindow
{
    public interface IRemoveProvisionDayTimeWindowCommand
    {
        long ProvisionId { get; }

        long RowId { get; }
    }
}
