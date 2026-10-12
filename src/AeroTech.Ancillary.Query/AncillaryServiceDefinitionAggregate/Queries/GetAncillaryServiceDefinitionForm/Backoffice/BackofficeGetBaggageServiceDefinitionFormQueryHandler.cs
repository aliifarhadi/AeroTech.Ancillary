using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;
using AeroTech.Messages.Ancillary.Enums;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionForm.Backoffice
{
    public sealed class BackofficeGetBaggageServiceDefinitionFormQueryHandler
        : IRequestHandler<BackofficeGetBaggageServiceDefinitionFormQuery, BackofficeServiceDefinitionFormDto<BaggageVariantSpecificationDto>>
    {
        private readonly IGetAncillaryServiceDefinitionFormService _service;

        public BackofficeGetBaggageServiceDefinitionFormQueryHandler(IGetAncillaryServiceDefinitionFormService service) => _service = service;

        public Task<BackofficeServiceDefinitionFormDto<BaggageVariantSpecificationDto>> Handle(
            BackofficeGetBaggageServiceDefinitionFormQuery query,
            CancellationToken cancellationToken)
            => _service.ExecuteAsync(query.ServiceDefinitionId, AncillaryProfile.Baggage, BaggageVariantSpecificationMapper.ToDto, cancellationToken);
    }
}
