using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using Microsoft.EntityFrameworkCore;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Fakes;

public sealed class ReferenceCurrencies : ICurrencyReference
{
    private readonly TestDatabase _database;

    public ReferenceCurrencies(TestDatabase database) => _database = database;

    public async Task<IReadOnlyDictionary<int, int>> FindDecimalPlacesAsync(IReadOnlyCollection<int> currencyIds, CancellationToken cancellationToken = default)
    {
        await using var reference = _database.NewReferenceContext();

        return await reference.Currencies.AsNoTracking()
            .Where(currency => currencyIds.Contains(currency.Id))
            .ToDictionaryAsync(currency => currency.Id, currency => currency.DecimalPlaces, cancellationToken);
    }
}
