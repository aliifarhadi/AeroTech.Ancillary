namespace AeroTech.Ancillary.Domain._Shared.Contracts
{
    public interface ICurrencyReference
    {
        Task<IReadOnlyDictionary<int, int>> FindDecimalPlacesAsync(IReadOnlyCollection<int> currencyIds, CancellationToken cancellationToken = default);

        Task<IReadOnlyDictionary<int, string>> FindCodesAsync(IReadOnlyCollection<int> currencyIds, CancellationToken cancellationToken = default);
    }
}
