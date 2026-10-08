namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models
{
    public sealed class AncillaryProvisionEligibleAgeBandReadModel
    {
        public long Id { get; set; }

        public long AncillaryProvisionId { get; set; }

        public int AgeFromInclusive { get; set; }

        public int? AgeToExclusive { get; set; }
    }
}
