using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetSeatProvisionCreateForm
{
    public interface IGetSeatProvisionCreateFormService
    {
        Task<BackofficeSeatProvisionCreateFormDto> ExecuteAsync(long serviceDefinitionId, CancellationToken cancellationToken = default);
    }
}
