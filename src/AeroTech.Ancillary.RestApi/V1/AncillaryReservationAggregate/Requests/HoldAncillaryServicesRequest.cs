using System.Text.Json.Serialization;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices;

namespace AeroTech.Ancillary.RestApi.V1.AncillaryReservationAggregate.Requests
{
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed record HoldAncillaryServicesRequest(
        string IdempotencyKey,
        long OrderId,
        string Reference,
        DateTimeOffset RequestedExpiresAt,
        IReadOnlyList<AncillaryVariantHoldInput<SelectedUnitInput<BaggageHoldOptionsInput>>>? Baggages = null,
        IReadOnlyList<AncillaryVariantHoldInput<SelectedUnitInput>>? Meals = null,
        IReadOnlyList<AncillaryVariantHoldInput<SelectedUnitInput<AssistedTravelHoldOptionsInput>>>? AssistedTravel = null,
        IReadOnlyList<AncillaryVariantHoldInput<SelectedUnitInput<AirportServiceHoldOptionsInput>>>? AirportServices = null,
        IReadOnlyList<AncillaryVariantHoldInput<SelectedUnitInput<PriorityHoldOptionsInput>>>? PriorityServices = null,
        IReadOnlyList<AncillaryVariantHoldInput<SelectedUnitInput<ConnectivityHoldOptionsInput>>>? Connectivity = null,
        IReadOnlyList<AncillaryVariantHoldInput<SelectedUnitInput>>? Upgrades = null);
}
