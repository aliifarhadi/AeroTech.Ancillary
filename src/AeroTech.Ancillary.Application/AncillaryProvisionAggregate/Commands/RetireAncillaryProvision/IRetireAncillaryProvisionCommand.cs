namespace AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RetireAncillaryProvision
{
    public interface IRetireAncillaryProvisionCommand
    {
        long ProvisionId { get; }
    }
}
