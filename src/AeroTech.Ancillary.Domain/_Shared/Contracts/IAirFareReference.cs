namespace AeroTech.Ancillary.Domain._Shared.Contracts
{
    public interface IAirFareReference
    {
        Task<IReadOnlyDictionary<long, long>> FindFareFamilyIdsAsync(IReadOnlyCollection<long> airFareIds, CancellationToken cancellationToken = default);
    }
}
