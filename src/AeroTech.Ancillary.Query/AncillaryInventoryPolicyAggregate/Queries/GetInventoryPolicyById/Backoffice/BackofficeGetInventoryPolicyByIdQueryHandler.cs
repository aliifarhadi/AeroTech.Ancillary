using AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Queries.GetInventoryPolicyById.Backoffice
{
    public sealed class BackofficeGetInventoryPolicyByIdQueryHandler : IRequestHandler<BackofficeGetInventoryPolicyByIdQuery, BackofficeInventoryPolicyDto>
    {
        private readonly IGetInventoryPolicyByIdService _service;

        public BackofficeGetInventoryPolicyByIdQueryHandler(IGetInventoryPolicyByIdService service) => _service = service;

        public Task<BackofficeInventoryPolicyDto> Handle(BackofficeGetInventoryPolicyByIdQuery query, CancellationToken cancellationToken)
            => _service.ExecuteAsync(query.PolicyId, cancellationToken);
    }
}
