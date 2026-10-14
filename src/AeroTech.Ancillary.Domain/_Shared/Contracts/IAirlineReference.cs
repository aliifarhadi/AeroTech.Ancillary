namespace AeroTech.Ancillary.Domain._Shared.Contracts
{
    public interface IAirlineReference
    {
        Task<IReadOnlyDictionary<int, string>> FindIataCodesAsync(IReadOnlyCollection<int> airlineIds, CancellationToken cancellationToken = default);
    }
}
