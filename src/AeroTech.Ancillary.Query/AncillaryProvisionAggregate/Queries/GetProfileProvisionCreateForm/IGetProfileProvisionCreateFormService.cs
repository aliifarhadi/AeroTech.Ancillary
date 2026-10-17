using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetProfileProvisionCreateForm
{
    public interface IGetProfileProvisionCreateFormService
    {
        Task<BackofficeProfileProvisionCreateFormDto> ExecuteAsync(AncillaryProfile profile, long serviceDefinitionId, CancellationToken cancellationToken = default);
    }
}
