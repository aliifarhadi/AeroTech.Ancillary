using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetSeatProvisionForm.Backoffice
{
    public sealed record BackofficeGetSeatProvisionFormQuery(long ServiceDefinitionId, long ProvisionId) : IRequest<BackofficeSeatProvisionFormDto>;
}
