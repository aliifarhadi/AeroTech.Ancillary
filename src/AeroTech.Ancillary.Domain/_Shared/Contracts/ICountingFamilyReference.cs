namespace AeroTech.Ancillary.Domain._Shared.Contracts
{
    public interface ICountingFamilyReference
    {
        Task<InventoryReferenceCheck> CheckAsync(int ownerAirlineId, string countingFamilyCode, CancellationToken cancellationToken = default);
    }
}
