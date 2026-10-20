using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;

namespace AeroTech.Ancillary.Stage2.EndToEndTests.Ordering;

public static class TestOrderMutations
{
    public const string CommercialChange = "CommercialChange";
    public const string CloseForChanges = "CloseForChanges";
    public const string Ticket = "Ticket";
    public const string Coupon = "Coupon";
    public const string UnlinkTraveller = "UnlinkTraveller";

    public static string? Apply(TestOrder order, OrderMutationRequest request, FixedClock clock)
    {
        lock (order)
        {
            switch (request.Kind)
            {
                case CommercialChange:
                    order.CommercialVersion++;
                    return null;
                case CloseForChanges:
                    order.AcceptsChanges = false;
                    return null;
                case Ticket:
                    order.TicketedAtUtc = clock.Now;
                    order.Air = order.Air with
                    {
                        Tickets = order.Air.Tickets
                            .Select(ticket => ticket with { Coupons = ticket.Coupons.Select(coupon => coupon with { IsOpenForUse = true }).ToList() })
                            .ToList()
                    };
                    order.CommercialVersion++;
                    return null;
                case Coupon when request.TravellerRef is { } travellerRef && request.BoundId is { } boundId:
                    order.Air = order.Air with
                    {
                        Tickets = order.Air.Tickets
                            .Select(ticket => ticket.TravellerRef != travellerRef
                                ? ticket
                                : ticket with
                                {
                                    Coupons = ticket.Coupons.Select(coupon => coupon.BoundId == boundId ? coupon with { IsOpenForUse = request.OpenForUse } : coupon).ToList()
                                })
                            .ToList()
                    };
                    return null;
                case UnlinkTraveller when request.TravellerRef is { } travellerRef:
                    var index = order.Travellers.FindIndex(traveller => traveller.TravellerRef == travellerRef);

                    if (index < 0)
                        return "TravellerNotInOrder";

                    order.Travellers[index] = order.Travellers[index] with { OrderTravellerId = 0 };
                    return null;
                default:
                    return $"UnknownMutation:{request.Kind}";
            }
        }
    }
}
