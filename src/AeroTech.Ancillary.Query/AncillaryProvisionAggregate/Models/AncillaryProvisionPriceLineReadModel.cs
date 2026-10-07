using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models
{
    public sealed class AncillaryProvisionPriceLineReadModel
    {
        public long Id { get; set; }

        public long AncillaryProvisionId { get; set; }

        public AncillaryPriceLineCategory Category { get; set; }

        public string? Code { get; set; }

        public string? Name { get; set; }

        public decimal UnitAmount { get; set; }
    }
}
