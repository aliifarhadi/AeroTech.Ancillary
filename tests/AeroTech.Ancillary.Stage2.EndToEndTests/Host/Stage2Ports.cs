using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Shopping.Tests.Fixtures;
using AeroTech.Ancillary.Stage2.EndToEndTests.Ordering;

namespace AeroTech.Ancillary.Stage2.EndToEndTests.Host;

public sealed class Stage2Ports
{
    public required ShoppingLab Lab { get; init; }

    public FixedClock Clock { get; } = new() { Now = new DateTimeOffset(2026, 10, 11, 9, 0, 0, TimeSpan.Zero) };

    public TestAirAvail AirAvail { get; } = new();

    public TestOccupiedSeats Occupancy { get; } = new();

    public SeatMapDesk.FixedSeatMaps SeatMaps { get; } = new(SeatMapDesk.Aircraft());

    public TestOnlyAirportReference Airports { get; } = new();

    public TestOnlyAirFareReference AirFares { get; } = new();

    public TestOrderStore Orders { get; } = new();

    public IsolationGuard Guard { get; } = new();

    public bool VerifiedExitRows { get; init; }
}
