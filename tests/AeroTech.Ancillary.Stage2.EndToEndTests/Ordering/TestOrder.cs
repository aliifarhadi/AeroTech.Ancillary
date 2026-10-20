using AeroTech.Ancillary.Application.FlightOffers;

namespace AeroTech.Ancillary.Stage2.EndToEndTests.Ordering;

public sealed class TestOrder
{
    public required long OrderId { get; init; }

    public required string SourceOfferId { get; init; }

    public required FlightOffer Air { get; set; }

    public int CommercialVersion { get; set; } = 1;

    public bool AcceptsChanges { get; set; } = true;

    public DateTimeOffset? TicketedAtUtc { get; set; }

    public List<TestOrderTraveller> Travellers { get; } = [];

    public List<TestOrderAirService> AirServices { get; } = [];

    public List<TestOrderAncillaryService> Services { get; } = [];

    public List<TestOrderSeat> Seats { get; } = [];

    public Dictionary<string, TestAddRecord> AddsByIdempotencyKey { get; } = new(StringComparer.Ordinal);

    public SemaphoreSlim AddGate { get; } = new(1, 1);
}
