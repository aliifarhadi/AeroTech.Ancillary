namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RemoveProvisionTravelDate
{
    public interface IRemoveProvisionTravelDateCommand
    {
        long ProvisionId { get; }

        long RowId { get; }
    }
}
