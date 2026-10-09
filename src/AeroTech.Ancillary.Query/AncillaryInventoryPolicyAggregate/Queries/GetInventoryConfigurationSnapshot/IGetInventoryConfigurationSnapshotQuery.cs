namespace AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Queries.GetInventoryConfigurationSnapshot
{
    public interface IGetInventoryConfigurationSnapshotQuery
    {
        string ServiceDefinitionRef { get; }

        long? FlightId { get; }

        DateTimeOffset? AtUtc { get; }
    }
}
