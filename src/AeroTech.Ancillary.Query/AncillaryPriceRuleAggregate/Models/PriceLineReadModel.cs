using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.AncillaryPriceRuleAggregate.Models
{
    public sealed class PriceLineReadModel
    {
        public long Id { get; set; }

        public long AncillaryPriceRuleId { get; set; }

        public AncillaryPriceLineCategory Category { get; set; }

        public string? Code { get; set; }

        public string? Name { get; set; }

        public decimal Amount { get; set; }
    }
}
