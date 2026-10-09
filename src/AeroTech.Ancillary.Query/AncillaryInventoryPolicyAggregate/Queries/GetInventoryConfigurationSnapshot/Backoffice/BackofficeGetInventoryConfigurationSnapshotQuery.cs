using AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Queries.GetInventoryConfigurationSnapshot.Backoffice
{
    public sealed class BackofficeGetInventoryConfigurationSnapshotQuery
        : IRequest<InventoryConfigurationSnapshotDto>, IGetInventoryConfigurationSnapshotQuery
    {
        public string ServiceDefinitionRef { get; set; } = default!;

        public long? FlightId { get; set; }

        public DateTimeOffset? AtUtc { get; set; }
    }
}
