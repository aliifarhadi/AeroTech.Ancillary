namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models
{
    public sealed class AncillaryProvisionFareFamilyReadModel
    {
        public long Id { get; set; }

        public long AncillaryProvisionId { get; set; }

        public long FareFamilyId { get; set; }
    }
}
