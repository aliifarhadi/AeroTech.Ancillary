using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Dto
{
    public sealed record InventoryConfigurationSnapshotDto(
        long? PolicyId,
        int OwnerAirlineId,
        string ServiceDefinitionRef,
        EnumValueDto? Authority,
        EnumValueDto? Pattern,
        EnumValueDto? ResourceKind,
        InventoryResourceLocatorDto? Resource,
        int? ConfiguredCount,
        decimal? ConfiguredKg,
        bool? ClosedForSale,
        EnumValueDto State,
        DateTimeOffset ObservedAt,
        DateTimeOffset? StaleAfter,
        bool IsGuaranteed,
        string? ReasonCode);

    public sealed record InventoryResourceLocatorDto(
        long? FlightId,
        long? ResourceId,
        long? WeightResourceId,
        int? AirportId,
        long? FacilityId,
        DateTimeOffset? StartUtc,
        DateTimeOffset? EndUtc);
}
