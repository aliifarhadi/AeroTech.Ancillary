namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionTravelDate
{
    public interface IChangeProvisionTravelDateCommand
    {
        long ProvisionId { get; }

        long RowId { get; }

        DateOnly TravelDate { get; }
    }
}
