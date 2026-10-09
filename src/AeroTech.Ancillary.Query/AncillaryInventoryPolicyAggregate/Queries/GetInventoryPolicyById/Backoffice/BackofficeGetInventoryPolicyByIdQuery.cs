using AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Queries.GetInventoryPolicyById.Backoffice
{
    public sealed record BackofficeGetInventoryPolicyByIdQuery(long PolicyId) : IRequest<BackofficeInventoryPolicyDto>;
}
