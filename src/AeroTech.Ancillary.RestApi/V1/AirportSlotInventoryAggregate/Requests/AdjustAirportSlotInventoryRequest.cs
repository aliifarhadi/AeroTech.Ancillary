namespace AeroTech.Ancillary.RestApi.V1.AirportSlotInventoryAggregate.Requests
{
    public sealed record AdjustAirportSlotInventoryRequest(
        int NewTotal,
        string ReasonCode,
        string CorrelationId,
        long ExpectedVersion);
}
