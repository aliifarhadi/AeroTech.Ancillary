using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.AirportSlotInventoryAggregate.Models
{
    public sealed class AirportSlotInventoryReadModel
    {
        public long Id { get; set; }

        public int OwnerAirlineId { get; set; }

        public int AirportId { get; set; }

        public long FacilityId { get; set; }

        public DateTimeOffset StartUtc { get; set; }

        public DateTimeOffset EndUtc { get; set; }

        public int CapacityPersons { get; set; }

        public bool ClosedForSale { get; set; }

        public InventoryRecordStatus Status { get; set; }

        public long Version { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }

        public int AdjustmentCount { get; set; }

        public DateTimeOffset LastUpdateTime { get; set; }
    }
}
