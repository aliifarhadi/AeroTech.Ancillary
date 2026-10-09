using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.RestApi.V1.FlightCountInventoryAggregate.Requests
{
    public sealed record DefineFlightCountInventoryRequest(
        int OwnerAirlineId,
        long FlightId,
        long ResourceId,
        InventoryCountUnit CountUnit,
        int TotalCapacity);
}
