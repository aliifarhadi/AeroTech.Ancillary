using AeroTech.Ancillary.Query.AncillaryProductAggregate.Dto;

namespace AeroTech.Ancillary.Query.AncillaryProductAggregate.Queries.GetAncillaryProductById
{
    public interface IGetAncillaryProductByIdService
    {
        Task<BackofficeAncillaryProductDto> ExecuteAsync(long ancillaryProductId, CancellationToken cancellationToken = default);
    }
}
