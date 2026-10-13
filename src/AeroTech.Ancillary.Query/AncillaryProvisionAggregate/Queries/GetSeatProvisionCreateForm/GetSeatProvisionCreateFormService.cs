using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetAncillaryProvisionById;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetSeatProvisionForm;
using AeroTech.Ancillary.Query._Shared.Enums;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetSeatProvisionCreateForm
{
    public sealed class GetSeatProvisionCreateFormService : IGetSeatProvisionCreateFormService
    {
        private readonly IAncillaryServiceDefinitionRepository _definitions;

        public GetSeatProvisionCreateFormService(IAncillaryServiceDefinitionRepository definitions) => _definitions = definitions;

        public async Task<BackofficeSeatProvisionCreateFormDto> ExecuteAsync(long serviceDefinitionId, CancellationToken cancellationToken = default)
        {
            var definition = await _definitions.GetAsync(serviceDefinitionId, cancellationToken)
                             ?? throw ExceptionFactory.ServiceDefinitionNotFound();

            if (definition is not { Profile: { } profile, Variant: { } variant, PricingUnit: { } pricingUnit, ServiceDateBasis: { } serviceDateBasis })
                throw ExceptionFactory.ServiceDefinitionNotClassified();

            if (profile != AncillaryProfile.Seat)
                throw ExceptionFactory.ServiceDefinitionBelongsToAnotherProfile(profile, AncillaryProfile.Seat);

            return new BackofficeSeatProvisionCreateFormDto(
                definition.Id,
                variant.Code,
                EnumValueDto.Of(profile),
                EnumValueDto.Of(definition.Status),
                EnumValueDto.Of(pricingUnit),
                EnumValueDto.Of(serviceDateBasis),
                EnumValueDto.Of(variant.ApplicationType),
                AncillaryProvisionMapper.RuleSections(profile, serviceDateBasis),
                SeatProvisionFormMapper.Constraints(variant.Code));
        }
    }
}
