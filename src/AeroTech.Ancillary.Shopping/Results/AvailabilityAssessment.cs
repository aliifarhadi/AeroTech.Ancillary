using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Shopping.Results
{
    public sealed record AvailabilityAssessment(
        InventoryAuthority? InventoryAuthority,
        InventoryCapacityReadState ConfigState,
        AvailabilityCheckState CheckedState,
        bool IsGuaranteed,
        bool RequiresCheckAtReserve,
        DateTimeOffset? AvailabilityAsOfUtc,
        DateTimeOffset? AvailabilityValidUntilUtc,
        ConsumptionPreview? ConsumptionPreview,
        IReadOnlyList<string> ReasonCodes);

    public sealed record ConsumptionPreview(int? CountUnits, InventoryCountUnit? CountUnit, decimal? WeightKg, int? OccupancyMinutes, int? OccupiedPersons);

    public sealed record ConfirmationAssessment(
        BookingMethod BookingMethod,
        ConfirmationRequirement ConfirmationRequirement,
        string? FulfillmentProviderKey,
        ProviderConfirmationStatus ProviderConfirmationStatus);
}
