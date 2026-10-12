using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;
using AeroTech.Messages.Ancillary.Enums;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionForm.Backoffice
{
    public sealed class BackofficeGetAssistedTravelServiceDefinitionFormQueryHandler
        : IRequestHandler<BackofficeGetAssistedTravelServiceDefinitionFormQuery, BackofficeServiceDefinitionFormDto<AssistedTravelVariantSpecificationDto>>
    {
        private readonly IGetAncillaryServiceDefinitionFormService _service;

        public BackofficeGetAssistedTravelServiceDefinitionFormQueryHandler(IGetAncillaryServiceDefinitionFormService service) => _service = service;

        public Task<BackofficeServiceDefinitionFormDto<AssistedTravelVariantSpecificationDto>> Handle(
            BackofficeGetAssistedTravelServiceDefinitionFormQuery query,
            CancellationToken cancellationToken)
            => _service.ExecuteAsync(query.ServiceDefinitionId, AncillaryProfile.AssistedTravel, AssistedTravelVariantSpecificationMapper.ToDto, cancellationToken);
    }
}
