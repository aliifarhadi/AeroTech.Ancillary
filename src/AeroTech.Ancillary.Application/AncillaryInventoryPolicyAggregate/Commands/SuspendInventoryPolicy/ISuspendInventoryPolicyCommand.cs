namespace AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.SuspendInventoryPolicy
{
    public interface ISuspendInventoryPolicyCommand
    {
        long PolicyId { get; }

        long ExpectedVersion { get; }
    }
}
