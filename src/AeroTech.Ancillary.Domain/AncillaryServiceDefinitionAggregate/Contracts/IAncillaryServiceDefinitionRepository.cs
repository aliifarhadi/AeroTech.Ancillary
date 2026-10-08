namespace AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Contracts
{
    public interface IAncillaryServiceDefinitionRepository
    {
        Task AddAsync(AncillaryServiceDefinition definition, CancellationToken cancellationToken = default);

        Task<AncillaryServiceDefinition?> GetAsync(long id, CancellationToken cancellationToken = default);

        Task<bool> HasActiveAsync(int ownerAirlineId, string serviceDefinitionRef, CancellationToken cancellationToken = default);

        Task<int> MaxVersionAsync(int ownerAirlineId, string serviceDefinitionRef, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<AncillaryServiceDefinition>> ListVersionsAsync(int ownerAirlineId, string serviceDefinitionRef, CancellationToken cancellationToken = default);
    }
}
