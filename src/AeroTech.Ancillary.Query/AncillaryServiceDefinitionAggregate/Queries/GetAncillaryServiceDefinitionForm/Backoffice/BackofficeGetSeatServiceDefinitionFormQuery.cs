using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionForm.Backoffice
{
    public sealed record BackofficeGetSeatServiceDefinitionFormQuery(long ServiceDefinitionId)
        : IRequest<BackofficeServiceDefinitionFormDto<SeatVariantSpecificationDto>>;
}
