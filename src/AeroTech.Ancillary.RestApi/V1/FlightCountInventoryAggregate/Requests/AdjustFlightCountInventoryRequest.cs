namespace AeroTech.Ancillary.RestApi.V1.FlightCountInventoryAggregate.Requests
{
    public sealed record AdjustFlightCountInventoryRequest(
        int NewTotal,
        string ReasonCode,
        string CorrelationId,
        long ExpectedVersion);
}
