namespace AeroTech.Ancillary.Domain._Shared.Contracts
{
    public interface IAirportReference
    {
        Task<bool> ExistsAsync(int airportId, CancellationToken cancellationToken = default);

        Task<IReadOnlyDictionary<int, int>> FindCountryIdsAsync(IReadOnlyCollection<int> airportIds, CancellationToken cancellationToken = default);

        Task<IReadOnlyDictionary<int, string>> FindTimeZoneIdsAsync(IReadOnlyCollection<int> airportIds, CancellationToken cancellationToken = default);
    }
}
