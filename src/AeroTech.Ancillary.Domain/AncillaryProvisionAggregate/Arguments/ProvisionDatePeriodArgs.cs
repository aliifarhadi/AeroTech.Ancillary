namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments
{
    public sealed record ProvisionDatePeriodArgs(
        DateOnly StartDate,
        DateOnly EndDate);
}
