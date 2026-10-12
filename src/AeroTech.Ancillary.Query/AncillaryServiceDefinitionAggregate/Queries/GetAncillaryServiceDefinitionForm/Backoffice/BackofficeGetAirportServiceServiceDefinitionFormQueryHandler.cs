using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;
using AeroTech.Messages.Ancillary.Enums;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionForm.Backoffice
{
    public sealed class BackofficeGetAirportServiceServiceDefinitionFormQueryHandler
        : IRequestHandler<BackofficeGetAirportServiceServiceDefinitionFormQuery, BackofficeServiceDefinitionFormDto<AirportServiceVariantSpecificationDto>>
    {
        private readonly IGetAncillaryServiceDefinitionFormService _service;

        public BackofficeGetAirportServiceServiceDefinitionFormQueryHandler(IGetAncillaryServiceDefinitionFormService service) => _service = service;

        public Task<BackofficeServiceDefinitionFormDto<AirportServiceVariantSpecificationDto>> Handle(
            BackofficeGetAirportServiceServiceDefinitionFormQuery query,
            CancellationToken cancellationToken)
            => _service.ExecuteAsync(query.ServiceDefinitionId, AncillaryProfile.AirportService, AirportServiceVariantSpecificationMapper.ToDto, cancellationToken);
    }
}
