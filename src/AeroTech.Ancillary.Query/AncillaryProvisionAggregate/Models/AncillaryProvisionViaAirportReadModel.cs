namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models
{
    public sealed class AncillaryProvisionViaAirportReadModel
    {
        public long Id { get; set; }

        public long AncillaryProvisionId { get; set; }

        public int AirportId { get; set; }
    }
}
