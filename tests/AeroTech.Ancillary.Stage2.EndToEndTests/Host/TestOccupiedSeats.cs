using System.Collections.Concurrent;
using AeroTech.Ancillary.Application.FlightOffers;
using AeroTech.Ancillary.Domain._Shared.Resources;

namespace AeroTech.Ancillary.Stage2.EndToEndTests.Host;

public sealed class TestOccupiedSeats : IOccupiedSeatProvider
{
    private readonly ConcurrentDictionary<long, HashSet<string>> _occupied = new();

    public bool Unavailable { get; set; }

    public HashSet<long> MissingFlights { get; } = [];

    public void Occupy(long flightId, params string[] seats)
        => _occupied.GetOrAdd(flightId, _ => new HashSet<string>(StringComparer.OrdinalIgnoreCase)).UnionWith(seats);

    public Task<IReadOnlyDictionary<long, IReadOnlySet<string>>> GetAsync(IReadOnlyCollection<long> flightIds, CancellationToken cancellationToken = default)
    {
        if (Unavailable)
            throw ExceptionFactory.OccupiedSeatsCouldNotBeRetrieved("the provider is unreachable");

        return Task.FromResult<IReadOnlyDictionary<long, IReadOnlySet<string>>>(
            flightIds
                .Where(flightId => !MissingFlights.Contains(flightId))
                .ToDictionary(
                    flightId => flightId,
                    flightId => (IReadOnlySet<string>)new HashSet<string>(_occupied.GetValueOrDefault(flightId) ?? [], StringComparer.OrdinalIgnoreCase)));
    }
}
