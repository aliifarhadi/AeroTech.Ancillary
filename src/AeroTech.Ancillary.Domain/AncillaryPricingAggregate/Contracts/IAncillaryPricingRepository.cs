namespace AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Contracts
{
    public interface IAncillaryPricingRepository
    {
        Task AddAsync(AncillaryPricing pricing, CancellationToken cancellationToken = default);

        Task<AncillaryPricing?> GetAsync(long id, CancellationToken cancellationToken = default);

        Task<AncillaryPricing?> FindActiveAsync(long ancillaryProvisionId, CancellationToken cancellationToken = default);

        Task<int> MaxVersionAsync(long ancillaryProvisionId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<AncillaryPricing>> ListUnassignedAsync(IReadOnlyCollection<long> serviceDefinitionIds, CancellationToken cancellationToken = default);
    }
}
