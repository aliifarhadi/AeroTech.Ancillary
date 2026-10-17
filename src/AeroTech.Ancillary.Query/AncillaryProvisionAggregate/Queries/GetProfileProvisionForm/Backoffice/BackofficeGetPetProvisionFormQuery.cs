using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetProfileProvisionForm.Backoffice
{
    public sealed record BackofficeGetPetProvisionFormQuery(long ServiceDefinitionId, long ProvisionId)
        : IRequest<BackofficeProfileProvisionFormDto<PetProvisionFormDto>>;
}
