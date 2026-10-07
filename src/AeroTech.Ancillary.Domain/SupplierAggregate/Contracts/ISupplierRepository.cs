namespace AeroTech.Ancillary.Domain.SupplierAggregate.Contracts
{
    public interface ISupplierRepository
    {
        Task AddAsync(Supplier supplier, CancellationToken cancellationToken = default);

        Task<Supplier?> GetAsync(long id, CancellationToken cancellationToken = default);
    }
}
