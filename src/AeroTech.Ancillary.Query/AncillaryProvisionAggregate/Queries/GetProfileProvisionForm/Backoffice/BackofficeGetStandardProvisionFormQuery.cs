using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;
using AeroTech.Messages.Ancillary.Enums;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetProfileProvisionForm.Backoffice
{
    public sealed record BackofficeGetStandardProvisionFormQuery(AncillaryProfile Profile, long ServiceDefinitionId, long ProvisionId)
        : IRequest<BackofficeProfileProvisionFormDto<StandardProvisionFormDto>>;
}
