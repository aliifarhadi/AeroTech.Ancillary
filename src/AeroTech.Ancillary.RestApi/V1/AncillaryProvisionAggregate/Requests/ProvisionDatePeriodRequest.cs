namespace AeroTech.Ancillary.RestApi.V1.AncillaryProvisionAggregate.Requests
{
    public sealed record ProvisionDatePeriodRequest(
        DateOnly StartDate,
        DateOnly EndDate);
}
