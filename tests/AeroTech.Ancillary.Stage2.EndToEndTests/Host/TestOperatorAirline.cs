using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Shopping.Tests.Fixtures;

namespace AeroTech.Ancillary.Stage2.EndToEndTests.Host;

public sealed class TestOperatorAirline : IOperatorAirlineResolver
{
    public Task<long?> FindHomeAirlineIdAsync(CancellationToken cancellationToken = default) => Task.FromResult<long?>(ShoppingLab.Airline);
}
