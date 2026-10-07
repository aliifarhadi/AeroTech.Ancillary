using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.SupplierAggregate.Models
{
    public sealed class SupplierReadModel
    {
        public long Id { get; set; }

        public int OwnerAirlineId { get; set; }

        public string Name { get; set; } = default!;

        public SupplierFulfillmentKind FulfillmentKind { get; set; }

        public string? FulfillmentProviderKey { get; set; }

        public SupplierStatus Status { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset? RetiredAt { get; set; }

        public DateTimeOffset LastUpdateTime { get; set; }
    }
}
