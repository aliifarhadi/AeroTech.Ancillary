namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models
{
    public sealed class AncillaryProvisionCoverageCountryReadModel
    {
        public long Id { get; set; }

        public long AncillaryProvisionId { get; set; }

        public int CountryId { get; set; }
    }
}
