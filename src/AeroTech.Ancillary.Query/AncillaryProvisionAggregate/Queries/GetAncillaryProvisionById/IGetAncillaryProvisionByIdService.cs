using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetAncillaryProvisionById
{
    public interface IGetAncillaryProvisionByIdService
    {
        Task<BackofficeProvisionDto> ExecuteAsync(long provisionId, CancellationToken cancellationToken = default);
    }
}
