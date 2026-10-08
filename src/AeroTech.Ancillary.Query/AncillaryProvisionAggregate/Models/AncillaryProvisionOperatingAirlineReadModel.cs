namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models
{
    public sealed class AncillaryProvisionOperatingAirlineReadModel
    {
        public long Id { get; set; }

        public long AncillaryProvisionId { get; set; }

        public int AirlineId { get; set; }
    }
}
