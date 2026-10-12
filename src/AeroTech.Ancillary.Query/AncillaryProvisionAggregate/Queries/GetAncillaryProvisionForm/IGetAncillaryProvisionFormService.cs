using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetAncillaryProvisionForm
{
    public interface IGetAncillaryProvisionFormService
    {
        Task<BackofficeProvisionFormDto> ExecuteAsync(long provisionId, CancellationToken cancellationToken = default);
    }
}
