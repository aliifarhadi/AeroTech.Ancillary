using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Models
{
    public sealed class InventoryPolicyReadModel
    {
        public long Id { get; set; }

        public int OwnerAirlineId { get; set; }

        public string ServiceDefinitionRef { get; set; } = default!;

        public long ServiceDefinitionId { get; set; }

        public InventoryAuthority Authority { get; set; }

        public LocalInventoryPattern? LocalPattern { get; set; }

        public string? ProviderKey { get; set; }

        public long? CountResourceId { get; set; }

        public int? CountPerAcceptedUnit { get; set; }

        public InventoryCountUnit? CountUnit { get; set; }

        public long? WeightResourceId { get; set; }

        public FlightWeightConsumptionMode? WeightConsumptionMode { get; set; }

        public decimal? WeightFixedKgPerUnit { get; set; }

        public long? SlotFacilityId { get; set; }

        public int? SlotOccupancyMinutes { get; set; }

        public int? SlotPeoplePerAcceptedUnit { get; set; }

        public InventoryRecordStatus Status { get; set; }

        public long Version { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }

        public DateTimeOffset? ActivatedAt { get; set; }

        public DateTimeOffset? SuspendedAt { get; set; }

        public DateTimeOffset? RetiredAt { get; set; }

        public DateTimeOffset LastUpdateTime { get; set; }
    }
}
