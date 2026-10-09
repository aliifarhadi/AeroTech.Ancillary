namespace AeroTech.Ancillary.RestApi.V1.AncillaryInventoryPolicyAggregate.Requests
{
    public sealed record InventoryConfigurationSnapshotRequest(
        string ServiceDefinitionRef,
        long? FlightId,
        string? AtUtc);
}
