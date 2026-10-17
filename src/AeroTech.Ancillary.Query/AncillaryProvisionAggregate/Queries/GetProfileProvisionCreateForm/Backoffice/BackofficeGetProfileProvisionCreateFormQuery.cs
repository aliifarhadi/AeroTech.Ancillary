using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;
using AeroTech.Messages.Ancillary.Enums;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetProfileProvisionCreateForm.Backoffice
{
    public sealed record BackofficeGetProfileProvisionCreateFormQuery(AncillaryProfile Profile, long ServiceDefinitionId)
        : IRequest<BackofficeProfileProvisionCreateFormDto>;
}
