using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.Contracts
{
    public sealed record PassengerUsageKey(
        string CountingFamilyCode,
        PassengerUsageLimitScope LimitScope,
        string? StableTravellerIdentity,
        long? OrderId,
        long? OrderTravellerId,
        long? FlightId,
        DateOnly? ServiceDate);
}
