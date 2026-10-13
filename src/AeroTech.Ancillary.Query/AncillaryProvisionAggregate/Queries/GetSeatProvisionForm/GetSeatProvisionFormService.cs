using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetAncillaryProvisionForm;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetSeatProvisionForm
{
    public sealed class GetSeatProvisionFormService : IGetSeatProvisionFormService
    {
        private readonly IGetAncillaryProvisionFormService _forms;

        public GetSeatProvisionFormService(IGetAncillaryProvisionFormService forms) => _forms = forms;

        public async Task<BackofficeSeatProvisionFormDto> ExecuteAsync(long serviceDefinitionId, long provisionId, CancellationToken cancellationToken = default)
        {
            var provision = await _forms.ExecuteAsync(provisionId, cancellationToken);

            if (provision.ServiceDefinitionId != serviceDefinitionId)
                throw ExceptionFactory.ProvisionNotFound();

            if (provision.Profile is not { } profile || profile.Value != (int)AncillaryProfile.Seat || provision.VariantCode is not { } variantCode)
                throw ExceptionFactory.ServiceDefinitionBelongsToAnotherProfile(provision.Profile?.Name, AncillaryProfile.Seat);

            var foreignRule = provision.Form.BaggageApplication is not null ? nameof(ProvisionFormDto.BaggageApplication)
                : provision.Form.PetRule is not null ? nameof(ProvisionFormDto.PetRule)
                : provision.Form.AssistedTravelRule is not null ? nameof(ProvisionFormDto.AssistedTravelRule)
                : provision.Form.AirportServiceRule is not null ? nameof(ProvisionFormDto.AirportServiceRule)
                : null;

            if (foreignRule is not null)
                throw ExceptionFactory.ProvisionProfileRuleNotAllowed(foreignRule, variantCode);

            return SeatProvisionFormMapper.ToSeatForm(provision, profile, variantCode);
        }
    }
}
