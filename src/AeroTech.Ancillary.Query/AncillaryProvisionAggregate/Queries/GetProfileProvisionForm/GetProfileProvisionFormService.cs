using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetAncillaryProvisionForm;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetProfileProvisionForm
{
    public sealed class GetProfileProvisionFormService : IGetProfileProvisionFormService
    {
        private readonly IGetAncillaryProvisionFormService _forms;
        private readonly IAncillaryServiceDefinitionRepository _definitions;

        public GetProfileProvisionFormService(IGetAncillaryProvisionFormService forms, IAncillaryServiceDefinitionRepository definitions)
        {
            _forms = forms;
            _definitions = definitions;
        }

        public async Task<BackofficeProfileProvisionFormDto<ProvisionFormDto>> ExecuteAsync(
            AncillaryProfile profile,
            string? profileRuleSection,
            long serviceDefinitionId,
            long provisionId,
            CancellationToken cancellationToken = default)
        {
            var provision = await _forms.ExecuteAsync(provisionId, cancellationToken);

            if (provision.ServiceDefinitionId != serviceDefinitionId)
                throw ExceptionFactory.ProvisionNotFound();

            if (provision.Profile is not { } storedProfile || storedProfile.Value != (int)profile || provision.VariantCode is not { } variantCode)
                throw ExceptionFactory.ServiceDefinitionBelongsToAnotherProfile(provision.Profile?.Name, profile);

            if (ProfileProvisionFormMapper.ForeignRuleSection(provision.Form, profileRuleSection) is { } foreignRule)
                throw ExceptionFactory.ProvisionProfileRuleNotAllowed(foreignRule, variantCode);

            var definition = await _definitions.GetAsync(serviceDefinitionId, cancellationToken)
                             ?? throw ExceptionFactory.ServiceDefinitionNotFound();

            return new BackofficeProfileProvisionFormDto<ProvisionFormDto>(
                provision.Id,
                provision.ServiceDefinitionId,
                provision.Status,
                storedProfile,
                variantCode,
                provision.Status.Value == (int)ProvisionStatus.Draft,
                provision.ApplicableRuleSections,
                ProfileProvisionFormMapper.Constraints(definition),
                provision.CreatedAt,
                provision.ActivatedAt,
                provision.SuspendedAt,
                provision.RetiredAt,
                provision.Form);
        }
    }
}
