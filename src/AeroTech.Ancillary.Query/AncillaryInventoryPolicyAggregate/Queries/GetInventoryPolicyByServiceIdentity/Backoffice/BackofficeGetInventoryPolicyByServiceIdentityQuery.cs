using AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Queries.GetInventoryPolicyByServiceIdentity.Backoffice
{
    public sealed record BackofficeGetInventoryPolicyByServiceIdentityQuery(string ServiceDefinitionRef) : IRequest<BackofficeInventoryPolicyDto>;
}
