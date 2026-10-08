namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models
{
    public sealed class AncillaryProvisionOriginAirportReadModel
    {
        public long Id { get; set; }

        public long AncillaryProvisionId { get; set; }

        public int AirportId { get; set; }
    }
}
