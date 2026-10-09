namespace AeroTech.Ancillary.RestApi.V1.FlightWeightInventoryAggregate.Requests
{
    public sealed record AdjustFlightWeightInventoryRequest(
        decimal NewKg,
        string ReasonCode,
        string CorrelationId,
        long ExpectedVersion);
}
