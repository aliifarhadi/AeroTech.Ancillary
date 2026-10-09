using AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Queries.GetInventoryPolicyByServiceIdentity.Backoffice
{
    public sealed class BackofficeGetInventoryPolicyByServiceIdentityQueryHandler : IRequestHandler<BackofficeGetInventoryPolicyByServiceIdentityQuery, BackofficeInventoryPolicyDto>
    {
        private readonly IGetInventoryPolicyByServiceIdentityService _service;

        public BackofficeGetInventoryPolicyByServiceIdentityQueryHandler(IGetInventoryPolicyByServiceIdentityService service) => _service = service;

        public Task<BackofficeInventoryPolicyDto> Handle(BackofficeGetInventoryPolicyByServiceIdentityQuery query, CancellationToken cancellationToken)
            => _service.ExecuteAsync(query.ServiceDefinitionRef, cancellationToken);
    }
}
