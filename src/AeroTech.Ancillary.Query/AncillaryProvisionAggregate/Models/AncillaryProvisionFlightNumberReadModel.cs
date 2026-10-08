namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models
{
    public sealed class AncillaryProvisionFlightNumberReadModel
    {
        public long Id { get; set; }

        public long AncillaryProvisionId { get; set; }

        public string FlightNumber { get; set; } = default!;
    }
}
