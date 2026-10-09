using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Dto
{
    public sealed record BackofficeInventoryPolicyDto(
        long Id,
        int OwnerAirlineId,
        string ServiceDefinitionRef,
        long ServiceDefinitionId,
        long? CurrentServiceDefinitionId,
        EnumValueDto Authority,
        EnumValueDto? LocalPattern,
        string? ProviderKey,
        BackofficeFlightCountConsumptionDto? CountConsumption,
        BackofficeFlightWeightConsumptionDto? WeightConsumption,
        BackofficeAirportSlotConsumptionDto? SlotConsumption,
        IReadOnlyList<BackofficePassengerUsageLimitDto> PassengerUsageLimits,
        EnumValueDto Status,
        long Version,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt,
        DateTimeOffset? ActivatedAt,
        DateTimeOffset? SuspendedAt,
        DateTimeOffset? RetiredAt);

    public sealed record BackofficeFlightCountConsumptionDto(
        long ResourceId,
        int CountPerAcceptedUnit,
        EnumValueDto CountUnit);

    public sealed record BackofficeFlightWeightConsumptionDto(
        long WeightResourceId,
        EnumValueDto ConsumptionMode,
        decimal? FixedKgPerUnit);

    public sealed record BackofficeAirportSlotConsumptionDto(
        long FacilityId,
        int OccupancyMinutes,
        int PeoplePerAcceptedUnit);

    public sealed record BackofficePassengerUsageLimitDto(
        long Id,
        EnumValueDto LimitScope,
        int MaxUnits,
        string CountingFamilyCode,
        EnumValueDto ConsumptionUnit,
        decimal? UnitsPerPurchase);
}
