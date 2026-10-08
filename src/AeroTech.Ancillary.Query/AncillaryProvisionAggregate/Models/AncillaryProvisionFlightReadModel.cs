namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models
{
    public sealed class AncillaryProvisionFlightReadModel
    {
        public long Id { get; set; }

        public long AncillaryProvisionId { get; set; }

        public long FlightId { get; set; }
    }
}
