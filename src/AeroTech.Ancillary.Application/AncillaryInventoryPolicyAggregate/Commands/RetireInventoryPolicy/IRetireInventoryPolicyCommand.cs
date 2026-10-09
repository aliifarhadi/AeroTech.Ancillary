namespace AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.RetireInventoryPolicy
{
    public interface IRetireInventoryPolicyCommand
    {
        long PolicyId { get; }

        long ExpectedVersion { get; }
    }
}
