using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Domain._Shared.Contracts;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Fakes;

public sealed class TestOnlyAirportReference : IAirportReference
{
    private static readonly int[] Known = [P1Commands.Thr, P1Commands.Ika, P1Commands.Mhd, P1Commands.Ist];

    public Task<bool> ExistsAsync(int airportId, CancellationToken cancellationToken = default) => Task.FromResult(Known.Contains(airportId));
}
