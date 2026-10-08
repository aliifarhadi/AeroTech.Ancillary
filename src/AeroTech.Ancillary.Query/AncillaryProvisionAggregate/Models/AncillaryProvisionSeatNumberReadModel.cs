namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models
{
    public sealed class AncillaryProvisionSeatNumberReadModel
    {
        public long Id { get; set; }

        public long AncillaryProvisionId { get; set; }

        public string SeatNumber { get; set; } = default!;
    }
}
