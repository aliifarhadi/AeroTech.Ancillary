namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments
{
    public sealed record ProvisionTravelDateArgs(
        IReadOnlyList<ProvisionDatePeriodArgs> PermittedPeriods,
        IReadOnlyList<ProvisionDatePeriodArgs> BlackoutPeriods)
    {
        public bool IsEmpty
            => PermittedPeriods.Count == 0 && BlackoutPeriods.Count == 0;
    }
}
