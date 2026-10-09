namespace AeroTech.Ancillary.RestApi.V1.AirportSlotInventoryAggregate.Requests
{
    public sealed record DefineAirportSlotInventoryRequest(
        int OwnerAirlineId,
        int AirportId,
        long FacilityId,
        string StartUtc,
        string EndUtc,
        int CapacityPersons);
}
