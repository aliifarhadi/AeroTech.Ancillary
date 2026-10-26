namespace AeroTech.Ancillary.Domain._Shared.Contracts
{
    public interface IInventoryLedgerSession : IAsyncDisposable
    {
        Task<IReadOnlyDictionary<InventorySourceKey, InventorySourceBalance>> BalancesAsync(DateTimeOffset now, CancellationToken cancellationToken = default);

        Task CommitAsync(CancellationToken cancellationToken = default);
    }
}
