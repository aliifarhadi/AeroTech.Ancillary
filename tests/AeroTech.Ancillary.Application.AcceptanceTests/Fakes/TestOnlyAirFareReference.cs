using AeroTech.Ancillary.Domain._Shared.Contracts;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Fakes;

public sealed class TestOnlyAirFareReference : IAirFareReference
{
    public Dictionary<long, long> FareFamilies { get; } = [];

    public Task<IReadOnlyDictionary<long, long>> FindFareFamilyIdsAsync(IReadOnlyCollection<long> airFareIds, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyDictionary<long, long>>(FareFamilies.Where(row => airFareIds.Contains(row.Key)).ToDictionary(row => row.Key, row => row.Value));
}
