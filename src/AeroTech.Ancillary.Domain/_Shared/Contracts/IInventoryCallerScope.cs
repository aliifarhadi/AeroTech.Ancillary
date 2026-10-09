namespace AeroTech.Ancillary.Domain._Shared.Contracts
{
    public interface IInventoryCallerScope
    {
        Task<int> RequireOwnerAirlineIdAsync(CancellationToken cancellationToken = default);

        Task EnsureOwnerAsync(int ownerAirlineId, CancellationToken cancellationToken = default);

        long RequireActorId();
    }
}
