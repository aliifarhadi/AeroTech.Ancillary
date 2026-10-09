using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Models
{
    public sealed class InventoryPassengerUsageLimitReadModel
    {
        public long Id { get; set; }

        public long InventoryPolicyId { get; set; }

        public PassengerUsageLimitScope LimitScope { get; set; }

        public int MaxUnits { get; set; }

        public string CountingFamilyCode { get; set; } = default!;

        public UsageConsumptionUnit ConsumptionUnit { get; set; }

        public decimal? UnitsPerPurchase { get; set; }
    }
}
