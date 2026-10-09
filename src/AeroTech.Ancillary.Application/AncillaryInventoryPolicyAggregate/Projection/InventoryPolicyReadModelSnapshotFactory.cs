using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate;
using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.Contracts;

namespace AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Projection
{
    internal static class InventoryPolicyReadModelSnapshotFactory
    {
        public static InventoryPolicyReadModelSnapshot ToReadModelSnapshot(this AncillaryInventoryPolicy policy)
            => new(
                policy.Id,
                policy.OwnerAirlineId,
                policy.ServiceDefinitionRef,
                policy.ServiceDefinitionId,
                policy.Authority,
                policy.LocalPattern,
                policy.ProviderKey,
                policy.CountConsumption?.ResourceId,
                policy.CountConsumption?.CountPerAcceptedUnit,
                policy.CountConsumption?.CountUnit,
                policy.WeightConsumption?.WeightResourceId,
                policy.WeightConsumption?.ConsumptionMode,
                policy.WeightConsumption?.FixedKgPerUnit,
                policy.SlotConsumption?.FacilityId,
                policy.SlotConsumption?.OccupancyMinutes,
                policy.SlotConsumption?.PeoplePerAcceptedUnit,
                policy.Status,
                policy.Version,
                policy.CreatedAt,
                policy.UpdatedAt,
                policy.ActivatedAt,
                policy.SuspendedAt,
                policy.RetiredAt,
                policy.PassengerUsageLimits
                    .Select(limit => new PassengerUsageLimitReadModelSnapshot(limit.Id, limit.LimitScope, limit.MaxUnits, limit.CountingFamilyCode))
                    .ToList());
    }
}
