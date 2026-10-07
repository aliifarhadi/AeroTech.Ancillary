namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ReactivateAncillaryProvision
{
    public interface IReactivateAncillaryProvisionCommand
    {
        long ProvisionId { get; }
    }
}
