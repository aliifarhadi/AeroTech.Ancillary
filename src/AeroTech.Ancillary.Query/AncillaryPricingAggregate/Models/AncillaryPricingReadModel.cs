using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.AncillaryPricingAggregate.Models
{
    public sealed class AncillaryPricingReadModel
    {
        public long Id { get; set; }

        public long AncillaryProvisionId { get; set; }

        public PricingUnit? PricingUnit { get; set; }

        public int Version { get; set; }

        public PricingStatus Status { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset? ActivatedAt { get; set; }

        public DateTimeOffset? SuspendedAt { get; set; }

        public DateTimeOffset? RetiredAt { get; set; }

        public DateTimeOffset LastUpdateTime { get; set; }
    }
}
