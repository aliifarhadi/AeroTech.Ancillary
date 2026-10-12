using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;
using AeroTech.Messages.Ancillary.Enums;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionForm.Backoffice
{
    public sealed class BackofficeGetConnectivityServiceDefinitionFormQueryHandler
        : IRequestHandler<BackofficeGetConnectivityServiceDefinitionFormQuery, BackofficeServiceDefinitionFormDto<OnboardWifiSpecificationDto>>
    {
        private readonly IGetAncillaryServiceDefinitionFormService _service;

        public BackofficeGetConnectivityServiceDefinitionFormQueryHandler(IGetAncillaryServiceDefinitionFormService service) => _service = service;

        public Task<BackofficeServiceDefinitionFormDto<OnboardWifiSpecificationDto>> Handle(
            BackofficeGetConnectivityServiceDefinitionFormQuery query,
            CancellationToken cancellationToken)
            => _service.ExecuteAsync(query.ServiceDefinitionId, AncillaryProfile.Connectivity, SingleVariantSpecificationMapper.ToOnboardWifi, cancellationToken);
    }
}
