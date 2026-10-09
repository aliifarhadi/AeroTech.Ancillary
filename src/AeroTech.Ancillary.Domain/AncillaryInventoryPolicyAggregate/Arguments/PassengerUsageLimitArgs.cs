using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.Arguments
{
    public sealed record PassengerUsageLimitArgs(
        PassengerUsageLimitScope LimitScope,
        int MaxUnits,
        string CountingFamilyCode);
}
