using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.FlightCountInventoryAggregate.Models
{
    public sealed class FlightCountInventoryReadModel
    {
        public long Id { get; set; }

        public int OwnerAirlineId { get; set; }

        public long FlightId { get; set; }

        public long ResourceId { get; set; }

        public InventoryCountUnit CountUnit { get; set; }

        public int TotalCapacity { get; set; }

        public bool ClosedForSale { get; set; }

        public InventoryRecordStatus Status { get; set; }

        public long Version { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }

        public int AdjustmentCount { get; set; }

        public DateTimeOffset LastUpdateTime { get; set; }
    }
}
