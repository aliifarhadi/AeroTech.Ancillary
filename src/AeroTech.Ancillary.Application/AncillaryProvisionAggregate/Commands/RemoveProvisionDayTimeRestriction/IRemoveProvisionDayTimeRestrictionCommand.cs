namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RemoveProvisionDayTimeRestriction
{
    public interface IRemoveProvisionDayTimeRestrictionCommand
    {
        long ProvisionId { get; }

        long RowId { get; }
    }
}
