using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;
using AeroTech.Messages.Ancillary.Enums;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionForm.Backoffice
{
    public sealed class BackofficeGetPriorityServiceDefinitionFormQueryHandler
        : IRequestHandler<BackofficeGetPriorityServiceDefinitionFormQuery, BackofficeServiceDefinitionFormDto<PriorityBoardingCheckinSpecificationDto>>
    {
        private readonly IGetAncillaryServiceDefinitionFormService _service;

        public BackofficeGetPriorityServiceDefinitionFormQueryHandler(IGetAncillaryServiceDefinitionFormService service) => _service = service;

        public Task<BackofficeServiceDefinitionFormDto<PriorityBoardingCheckinSpecificationDto>> Handle(
            BackofficeGetPriorityServiceDefinitionFormQuery query,
            CancellationToken cancellationToken)
            => _service.ExecuteAsync(query.ServiceDefinitionId, AncillaryProfile.Priority, SingleVariantSpecificationMapper.ToPriorityBoardingCheckin, cancellationToken);
    }
}
