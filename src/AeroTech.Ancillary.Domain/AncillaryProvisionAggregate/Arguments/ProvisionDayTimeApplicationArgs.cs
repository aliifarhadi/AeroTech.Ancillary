namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments
{
    public sealed record ProvisionDayTimeApplicationArgs(
        IReadOnlyList<ProvisionDayTimeWindowArgs> Windows)
    {
        public bool IsEmpty
            => Windows.Count == 0;
    }
}
