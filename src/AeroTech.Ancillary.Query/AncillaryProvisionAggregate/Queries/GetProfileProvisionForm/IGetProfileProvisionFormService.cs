using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetProfileProvisionForm
{
    public interface IGetProfileProvisionFormService
    {
        Task<BackofficeProfileProvisionFormDto<ProvisionFormDto>> ExecuteAsync(
            AncillaryProfile profile,
            string? profileRuleSection,
            long serviceDefinitionId,
            long provisionId,
            CancellationToken cancellationToken = default);
    }
}
