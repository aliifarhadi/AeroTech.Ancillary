namespace AeroTech.Ancillary.Domain.AncillaryProductAggregate.Contracts
{
    public interface IAncillaryProductRepository
    {
        Task AddAsync(AncillaryProduct product, CancellationToken cancellationToken = default);

        Task<AncillaryProduct?> GetAsync(long id, CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(int ownerAirlineId, string productRef, CancellationToken cancellationToken = default);

        Task<bool> DraftExistsAsync(int ownerAirlineId, string productRef, CancellationToken cancellationToken = default);

        Task<int> HighestVersionAsync(int ownerAirlineId, string productRef, CancellationToken cancellationToken = default);

        Task<AncillaryProduct?> FindOfferedVersionAsync(int ownerAirlineId, string productRef, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<AncillaryProduct>> ListActiveAsync(CancellationToken cancellationToken = default);
    }
}
