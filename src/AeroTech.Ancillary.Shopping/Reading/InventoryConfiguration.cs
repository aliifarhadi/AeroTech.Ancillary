using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Shopping.Reading
{
    public sealed record InventoryConfiguration(
        AncillaryInventoryPolicy Policy,
        IReadOnlyList<FlightInventorySource> FlightSources,
        IReadOnlyList<SlotInventorySource> SlotSources);

    public sealed record FlightInventorySource(InventoryResourceKind Kind, long FlightId, long ResourceId, InventoryRecordStatus Status, bool ClosedForSale);

    public sealed record SlotInventorySource(
        long FacilityId,
        int AirportId,
        DateTimeOffset StartUtc,
        DateTimeOffset EndUtc,
        InventoryRecordStatus Status,
        bool ClosedForSale);
}
