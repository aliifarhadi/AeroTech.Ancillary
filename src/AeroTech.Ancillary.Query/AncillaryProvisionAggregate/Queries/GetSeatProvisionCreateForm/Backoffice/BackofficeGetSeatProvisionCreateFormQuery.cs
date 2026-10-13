using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetSeatProvisionCreateForm.Backoffice
{
    public sealed record BackofficeGetSeatProvisionCreateFormQuery(long ServiceDefinitionId) : IRequest<BackofficeSeatProvisionCreateFormDto>;
}
