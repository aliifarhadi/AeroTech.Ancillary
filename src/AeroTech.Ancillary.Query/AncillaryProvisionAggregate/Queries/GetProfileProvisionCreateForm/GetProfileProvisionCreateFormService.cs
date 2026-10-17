using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetAncillaryProvisionById;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetProfileProvisionForm;
using AeroTech.Ancillary.Query._Shared.Enums;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetProfileProvisionCreateForm
{
    public sealed class GetProfileProvisionCreateFormService : IGetProfileProvisionCreateFormService
    {
        private readonly IAncillaryServiceDefinitionRepository _definitions;

        public GetProfileProvisionCreateFormService(IAncillaryServiceDefinitionRepository definitions) => _definitions = definitions;

        public async Task<BackofficeProfileProvisionCreateFormDto> ExecuteAsync(AncillaryProfile profile, long serviceDefinitionId, CancellationToken cancellationToken = default)
        {
            var definition = await _definitions.GetAsync(serviceDefinitionId, cancellationToken)
                             ?? throw ExceptionFactory.ServiceDefinitionNotFound();

            if (definition is not { Profile: { } definitionProfile, Variant: { } variant, PricingUnit: { } pricingUnit, ServiceDateBasis: { } serviceDateBasis })
                throw ExceptionFactory.ServiceDefinitionNotClassified();

            if (definitionProfile != profile)
                throw ExceptionFactory.ServiceDefinitionBelongsToAnotherProfile(definitionProfile, profile);

            return new BackofficeProfileProvisionCreateFormDto(
                definition.Id,
                variant.Code,
                EnumValueDto.Of(profile),
                EnumValueDto.Of(definition.Status),
                EnumValueDto.Of(pricingUnit),
                EnumValueDto.Of(serviceDateBasis),
                EnumValueDto.Of(variant.ApplicationType),
                AncillaryProvisionMapper.RuleSections(profile, serviceDateBasis),
                ProfileProvisionFormMapper.Constraints(definition));
        }
    }
}
