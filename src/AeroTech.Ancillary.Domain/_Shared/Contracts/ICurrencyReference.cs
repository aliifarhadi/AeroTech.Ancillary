namespace AeroTech.Ancillary.Domain._Shared.Contracts
{
    public interface ICurrencyReference
    {
        Task<IReadOnlyDictionary<int, int>> FindDecimalPlacesAsync(IReadOnlyCollection<int> currencyIds, CancellationToken cancellationToken = default);
    }
}
