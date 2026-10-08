namespace AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments
{
    public sealed record ProvisionAgeBandArgs(
        int AgeFromInclusive,
        int? AgeToExclusive);
}
