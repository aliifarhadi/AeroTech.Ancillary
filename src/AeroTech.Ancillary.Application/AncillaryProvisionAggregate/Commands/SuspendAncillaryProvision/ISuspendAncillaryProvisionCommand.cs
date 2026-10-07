namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.SuspendAncillaryProvision
{
    public interface ISuspendAncillaryProvisionCommand
    {
        long ProvisionId { get; }
    }
}
