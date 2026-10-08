namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models
{
    public sealed class AncillaryProvisionAircraftReadModel
    {
        public long Id { get; set; }

        public long AncillaryProvisionId { get; set; }

        public int AircraftId { get; set; }
    }
}
