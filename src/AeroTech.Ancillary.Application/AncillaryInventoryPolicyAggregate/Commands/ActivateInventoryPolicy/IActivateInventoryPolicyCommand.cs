namespace AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.ActivateInventoryPolicy
{
    public interface IActivateInventoryPolicyCommand
    {
        long PolicyId { get; }

        long ExpectedVersion { get; }
    }
}
