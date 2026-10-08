namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models
{
    public sealed class AncillaryProvisionFareBasisReadModel
    {
        public long Id { get; set; }

        public long AncillaryProvisionId { get; set; }

        public string FareBasisCode { get; set; } = default!;
    }
}
