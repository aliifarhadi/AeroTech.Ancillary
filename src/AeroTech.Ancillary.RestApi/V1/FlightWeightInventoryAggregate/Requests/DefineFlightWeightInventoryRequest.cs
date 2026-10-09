namespace AeroTech.Ancillary.RestApi.V1.FlightWeightInventoryAggregate.Requests
{
    public sealed record DefineFlightWeightInventoryRequest(
        int OwnerAirlineId,
        long FlightId,
        long WeightResourceId,
        decimal CapacityKg);
}
