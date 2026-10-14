using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Domain._Shared.Contracts;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Fakes;

public sealed class TestOnlyAirportReference : IAirportReference
{
    private static readonly int[] Known = [P1Commands.Thr, P1Commands.Ika, P1Commands.Mhd, P1Commands.Ist];

    public Dictionary<int, int> Countries { get; } = [];

    public Dictionary<int, string> TimeZones { get; } = [];

    public Dictionary<int, string> IataCodes { get; } = [];

    public Dictionary<int, string> TerminalNumbers { get; } = [];

    public Task<bool> ExistsAsync(int airportId, CancellationToken cancellationToken = default) => Task.FromResult(Known.Contains(airportId));

    public Task<IReadOnlyDictionary<int, int>> FindCountryIdsAsync(IReadOnlyCollection<int> airportIds, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyDictionary<int, int>>(Countries.Where(row => airportIds.Contains(row.Key)).ToDictionary(row => row.Key, row => row.Value));

    public Task<IReadOnlyDictionary<int, string>> FindTimeZoneIdsAsync(IReadOnlyCollection<int> airportIds, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyDictionary<int, string>>(TimeZones.Where(row => airportIds.Contains(row.Key)).ToDictionary(row => row.Key, row => row.Value));

    public Task<IReadOnlyDictionary<int, string>> FindIataCodesAsync(IReadOnlyCollection<int> airportIds, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyDictionary<int, string>>(IataCodes.Where(row => airportIds.Contains(row.Key)).ToDictionary(row => row.Key, row => row.Value));

    public Task<IReadOnlyDictionary<int, string>> FindTerminalNumbersAsync(IReadOnlyCollection<int> terminalIds, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyDictionary<int, string>>(TerminalNumbers.Where(row => terminalIds.Contains(row.Key)).ToDictionary(row => row.Key, row => row.Value));
}
