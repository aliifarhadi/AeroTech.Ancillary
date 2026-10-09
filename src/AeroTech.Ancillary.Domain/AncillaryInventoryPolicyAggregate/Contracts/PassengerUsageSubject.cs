namespace AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.Contracts
{
    public sealed record PassengerUsageSubject(
        string? StableTravellerIdentity,
        long? OrderId,
        long? OrderTravellerId,
        long? FlightId,
        DateOnly? ServiceDate);
}
