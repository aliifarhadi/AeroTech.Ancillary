namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models
{
    public sealed class AncillaryProvisionCabinClassReadModel
    {
        public long Id { get; set; }

        public long AncillaryProvisionId { get; set; }

        public int CabinClassId { get; set; }
    }
}
