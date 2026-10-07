using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;

namespace AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionById
{
    public interface IGetAncillaryServiceDefinitionByIdService
    {
        Task<BackofficeServiceDefinitionDto> ExecuteAsync(long serviceDefinitionId, CancellationToken cancellationToken = default);
    }
}
