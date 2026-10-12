using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetAncillaryProvisionForm.Backoffice
{
    public sealed record BackofficeGetAncillaryProvisionFormQuery(long ProvisionId) : IRequest<BackofficeProvisionFormDto>;
}
