namespace AeroTech.Ancillary.Domain._Shared.Contracts
{
    public interface IFlightFlowDelegationReference
    {
        Task<InventoryReferenceCheck> CheckAsync(int ownerAirlineId, string providerKey, CancellationToken cancellationToken = default);
    }
}
