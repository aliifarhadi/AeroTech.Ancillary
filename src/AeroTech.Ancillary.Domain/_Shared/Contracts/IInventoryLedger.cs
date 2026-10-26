namespace AeroTech.Ancillary.Domain._Shared.Contracts
{
    public interface IInventoryLedger
    {
        Task<IInventoryLedgerSession> OpenAsync(IReadOnlyCollection<InventorySourceKey> sources, CancellationToken cancellationToken = default);
    }
}
