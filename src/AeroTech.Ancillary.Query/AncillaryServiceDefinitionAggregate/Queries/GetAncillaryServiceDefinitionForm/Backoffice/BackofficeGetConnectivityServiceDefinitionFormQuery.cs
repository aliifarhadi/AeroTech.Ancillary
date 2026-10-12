using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionForm.Backoffice
{
    public sealed record BackofficeGetConnectivityServiceDefinitionFormQuery(long ServiceDefinitionId)
        : IRequest<BackofficeServiceDefinitionFormDto<OnboardWifiSpecificationDto>>;
}
