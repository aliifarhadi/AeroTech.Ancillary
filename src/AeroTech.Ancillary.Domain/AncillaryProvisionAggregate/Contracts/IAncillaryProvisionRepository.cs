namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Contracts
{
    public interface IAncillaryProvisionRepository
    {
        Task AddAsync(AncillaryProvision provision, CancellationToken cancellationToken = default);

        Task<AncillaryProvision?> GetAsync(long id, CancellationToken cancellationToken = default);

        Task<AncillaryProvision?> FindActiveAtSequenceAsync(long serviceDefinitionId, int sequence, CancellationToken cancellationToken = default);
    }
}
