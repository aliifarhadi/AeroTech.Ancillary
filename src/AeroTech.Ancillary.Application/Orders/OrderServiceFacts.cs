using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.Orders
{
    public sealed record OrderServiceFacts(
        long OrderServiceId,
        string TravellerRef,
        string ServiceRef,
        string? PortionRef,
        IReadOnlyList<string> FlightRefs,
        int Quantity,
        ExistingServiceCommercialState CommercialState);
}
