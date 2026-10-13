using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetSeatProvisionForm
{
    public interface IGetSeatProvisionFormService
    {
        Task<BackofficeSeatProvisionFormDto> ExecuteAsync(long serviceDefinitionId, long provisionId, CancellationToken cancellationToken = default);
    }
}
