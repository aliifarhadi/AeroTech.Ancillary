using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;
using AeroTech.Messages.Ancillary.Enums;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionForm.Backoffice
{
    public sealed class BackofficeGetUpgradeServiceDefinitionFormQueryHandler
        : IRequestHandler<BackofficeGetUpgradeServiceDefinitionFormQuery, BackofficeServiceDefinitionFormDto<CabinUpgradeSpecificationDto>>
    {
        private readonly IGetAncillaryServiceDefinitionFormService _service;

        public BackofficeGetUpgradeServiceDefinitionFormQueryHandler(IGetAncillaryServiceDefinitionFormService service) => _service = service;

        public Task<BackofficeServiceDefinitionFormDto<CabinUpgradeSpecificationDto>> Handle(
            BackofficeGetUpgradeServiceDefinitionFormQuery query,
            CancellationToken cancellationToken)
            => _service.ExecuteAsync(query.ServiceDefinitionId, AncillaryProfile.Upgrade, SingleVariantSpecificationMapper.ToCabinUpgrade, cancellationToken);
    }
}
