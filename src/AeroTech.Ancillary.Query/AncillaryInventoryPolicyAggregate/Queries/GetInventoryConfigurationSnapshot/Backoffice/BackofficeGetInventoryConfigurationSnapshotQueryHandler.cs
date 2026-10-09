using AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Queries.GetInventoryConfigurationSnapshot.Backoffice
{
    public sealed class BackofficeGetInventoryConfigurationSnapshotQueryHandler : IRequestHandler<BackofficeGetInventoryConfigurationSnapshotQuery, InventoryConfigurationSnapshotDto>
    {
        private readonly IGetInventoryConfigurationSnapshotService _service;

        public BackofficeGetInventoryConfigurationSnapshotQueryHandler(IGetInventoryConfigurationSnapshotService service) => _service = service;

        public Task<InventoryConfigurationSnapshotDto> Handle(BackofficeGetInventoryConfigurationSnapshotQuery query, CancellationToken cancellationToken)
            => _service.ExecuteAsync(query, cancellationToken);
    }
}
