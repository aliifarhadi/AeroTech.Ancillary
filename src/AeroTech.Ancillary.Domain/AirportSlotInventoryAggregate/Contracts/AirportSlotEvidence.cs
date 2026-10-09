using AeroTech.Ancillary.Domain._Shared.Contracts;

namespace AeroTech.Ancillary.Domain.AirportSlotInventoryAggregate.Contracts
{
    public sealed record AirportSlotEvidence(
        bool AirportExists,
        InventoryReferenceCheck Facility,
        int? FacilityAirportId,
        string? FacilityTimeZoneId);
}
