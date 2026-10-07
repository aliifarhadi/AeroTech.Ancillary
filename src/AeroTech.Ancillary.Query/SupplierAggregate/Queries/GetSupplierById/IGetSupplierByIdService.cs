using AeroTech.Ancillary.Query.SupplierAggregate.Dto;

namespace AeroTech.Ancillary.Query.SupplierAggregate.Queries.GetSupplierById
{
    public interface IGetSupplierByIdService
    {
        Task<BackofficeSupplierDto> ExecuteAsync(long supplierId, CancellationToken cancellationToken = default);
    }
}
