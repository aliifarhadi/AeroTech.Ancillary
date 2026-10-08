namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionTravelDate
{
    public interface IAddProvisionTravelDateCommand
    {
        long ProvisionId { get; }

        DateOnly TravelDate { get; }
    }
}
