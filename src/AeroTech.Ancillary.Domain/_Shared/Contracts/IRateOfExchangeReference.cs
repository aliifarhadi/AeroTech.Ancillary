namespace AeroTech.Ancillary.Domain._Shared.Contracts
{
    public sealed record PublishedRateOfExchange(
        string RateOfExchangeTableId,
        int FromCurrencyId,
        int ToCurrencyId,
        decimal Rate,
        int ToCurrencyDecimalPlaces,
        double ToCurrencyRoundingFactor);

    public interface IRateOfExchangeReference
    {
        Task<IReadOnlyList<PublishedRateOfExchange>> FindRatesAsync(int toCurrencyId, DateTimeOffset at, CancellationToken cancellationToken = default);
    }
}
