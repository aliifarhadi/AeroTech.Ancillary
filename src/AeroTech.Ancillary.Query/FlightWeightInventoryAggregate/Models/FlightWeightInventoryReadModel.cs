using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.FlightWeightInventoryAggregate.Models
{
    public sealed class FlightWeightInventoryReadModel
    {
        public long Id { get; set; }

        public int OwnerAirlineId { get; set; }

        public long FlightId { get; set; }

        public long WeightResourceId { get; set; }

        public decimal CapacityKg { get; set; }

        public bool ClosedForSale { get; set; }

        public InventoryRecordStatus Status { get; set; }

        public long Version { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }

        public int AdjustmentCount { get; set; }

        public DateTimeOffset LastUpdateTime { get; set; }
    }
}
