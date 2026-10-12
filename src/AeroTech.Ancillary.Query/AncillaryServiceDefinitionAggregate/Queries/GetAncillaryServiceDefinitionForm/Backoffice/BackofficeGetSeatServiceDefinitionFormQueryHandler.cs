using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;
using AeroTech.Messages.Ancillary.Enums;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionForm.Backoffice
{
    public sealed class BackofficeGetSeatServiceDefinitionFormQueryHandler
        : IRequestHandler<BackofficeGetSeatServiceDefinitionFormQuery, BackofficeServiceDefinitionFormDto<SeatVariantSpecificationDto>>
    {
        private readonly IGetAncillaryServiceDefinitionFormService _service;

        public BackofficeGetSeatServiceDefinitionFormQueryHandler(IGetAncillaryServiceDefinitionFormService service) => _service = service;

        public Task<BackofficeServiceDefinitionFormDto<SeatVariantSpecificationDto>> Handle(
            BackofficeGetSeatServiceDefinitionFormQuery query,
            CancellationToken cancellationToken)
            => _service.ExecuteAsync(query.ServiceDefinitionId, AncillaryProfile.Seat, SeatVariantSpecificationMapper.ToDto, cancellationToken);
    }
}
