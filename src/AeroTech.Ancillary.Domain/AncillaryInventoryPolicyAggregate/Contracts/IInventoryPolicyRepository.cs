namespace AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.Contracts
{
    public interface IInventoryPolicyRepository
    {
        Task AddAsync(AncillaryInventoryPolicy policy, CancellationToken cancellationToken = default);

        Task<AncillaryInventoryPolicy?> GetAsync(long id, CancellationToken cancellationToken = default);

        Task<bool> HasCurrentAsync(int ownerAirlineId, string serviceDefinitionRef, CancellationToken cancellationToken = default);
    }
}
