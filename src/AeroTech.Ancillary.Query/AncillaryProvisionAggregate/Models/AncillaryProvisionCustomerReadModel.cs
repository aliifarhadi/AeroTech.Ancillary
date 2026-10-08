namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models
{
    public sealed class AncillaryProvisionCustomerReadModel
    {
        public long Id { get; set; }

        public long AncillaryProvisionId { get; set; }

        public long CustomerId { get; set; }
    }
}
