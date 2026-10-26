using System.Text.Json.Serialization;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.AncillaryReservationAggregate.Dto
{
    public sealed record AncillaryHoldDto(
        long HoldId,
        long OrderId,
        string Reference,
        AncillaryReservationStatus Status,
        DateTimeOffset? ExpiresAt,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyList<AncillaryVariantHoldDto<HeldBaggageDetailsDto, AncillaryUnitHoldDto<HeldBaggageOptionsDto>>>? Baggages,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyList<AncillaryVariantHoldDto<HeldMealDetailsDto, AncillaryUnitHoldDto>>? Meals,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyList<AncillaryVariantHoldDto<AncillaryUnitHoldDto<HeldAssistedTravelOptionsDto>>>? AssistedTravel,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyList<AncillaryVariantHoldDto<HeldAirportServiceDetailsDto, AncillaryUnitHoldDto<HeldAirportServiceOptionsDto>>>? AirportServices,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyList<AncillaryVariantHoldDto<AncillaryUnitHoldDto<HeldPriorityOptionsDto>>>? PriorityServices,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyList<AncillaryVariantHoldDto<AncillaryUnitHoldDto<HeldConnectivityOptionsDto>>>? Connectivity,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyList<AncillaryVariantHoldDto<HeldUpgradeDetailsDto, AncillaryUnitHoldDto>>? Upgrades,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyList<AncillaryVariantHoldDto<SeatUnitHoldDto>>? Seats);
}
