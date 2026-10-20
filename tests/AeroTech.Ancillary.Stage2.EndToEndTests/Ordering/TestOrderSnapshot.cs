namespace AeroTech.Ancillary.Stage2.EndToEndTests.Ordering;

public sealed record TestOrderSnapshot(
    long OrderId,
    string SourceOfferId,
    int CurrencyId,
    int CommercialVersion,
    bool AcceptsChanges,
    DateTimeOffset? TicketedAtUtc,
    IReadOnlyList<TestOrderTraveller> Travellers,
    IReadOnlyList<TestOrderAirService> AirServices,
    IReadOnlyList<TestOrderAncillaryService> Services,
    IReadOnlyList<TestOrderSeat> Seats)
{
    public static TestOrderSnapshot Of(TestOrder order)
    {
        lock (order)
            return new TestOrderSnapshot(
                order.OrderId,
                order.SourceOfferId,
                order.Air.CurrencyId,
                order.CommercialVersion,
                order.AcceptsChanges,
                order.TicketedAtUtc,
                order.Travellers.ToList(),
                order.AirServices.ToList(),
                order.Services.ToList(),
                order.Seats.ToList());
    }
}
