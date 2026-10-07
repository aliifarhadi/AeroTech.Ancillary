using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionById.Backoffice
{
    public sealed record BackofficeGetAncillaryServiceDefinitionByIdQuery(long ServiceDefinitionId) : IRequest<BackofficeServiceDefinitionDto>;
}
