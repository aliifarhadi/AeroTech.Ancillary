namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models
{
    public sealed class AncillaryProvisionTravelDateReadModel
    {
        public long Id { get; set; }

        public long AncillaryProvisionId { get; set; }

        public DateOnly TravelDate { get; set; }
    }
}
