using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.Contracts
{
    public sealed record InventoryPolicyReadModelSnapshot(
        long PolicyId,
        int OwnerAirlineId,
        string ServiceDefinitionRef,
        long ServiceDefinitionId,
        InventoryAuthority Authority,
        LocalInventoryPattern? LocalPattern,
        string? ProviderKey,
        long? CountResourceId,
        int? CountPerAcceptedUnit,
        InventoryCountUnit? CountUnit,
        long? WeightResourceId,
        FlightWeightConsumptionMode? WeightConsumptionMode,
        decimal? WeightFixedKgPerUnit,
        long? SlotFacilityId,
        int? SlotOccupancyMinutes,
        int? SlotPeoplePerAcceptedUnit,
        InventoryRecordStatus Status,
        long Version,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt,
        DateTimeOffset? ActivatedAt,
        DateTimeOffset? SuspendedAt,
        DateTimeOffset? RetiredAt,
        IReadOnlyList<PassengerUsageLimitReadModelSnapshot> PassengerUsageLimits);

    public sealed record PassengerUsageLimitReadModelSnapshot(
        long LimitId,
        PassengerUsageLimitScope LimitScope,
        int MaxUnits,
        string CountingFamilyCode);
}
