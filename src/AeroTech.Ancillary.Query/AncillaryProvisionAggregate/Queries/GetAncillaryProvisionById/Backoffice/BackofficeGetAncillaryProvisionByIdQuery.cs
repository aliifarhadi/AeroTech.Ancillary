using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetAncillaryProvisionById.Backoffice
{
    public sealed record BackofficeGetAncillaryProvisionByIdQuery(long ProvisionId) : IRequest<BackofficeProvisionDto>;
}
