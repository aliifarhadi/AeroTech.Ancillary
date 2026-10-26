using MediatR;

namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices.Service
{
    public sealed record ServiceHoldAncillaryServicesCommand(
        string IdempotencyKey,
        long OrderId,
        string Reference,
        DateTimeOffset RequestedExpiresAt,
        IReadOnlyList<AncillaryVariantHoldInput<SelectedUnitInput<BaggageHoldOptionsInput>>>? Baggages,
        IReadOnlyList<AncillaryVariantHoldInput<SelectedUnitInput>>? Meals,
        IReadOnlyList<AncillaryVariantHoldInput<SelectedUnitInput<AssistedTravelHoldOptionsInput>>>? AssistedTravel,
        IReadOnlyList<AncillaryVariantHoldInput<SelectedUnitInput<AirportServiceHoldOptionsInput>>>? AirportServices,
        IReadOnlyList<AncillaryVariantHoldInput<SelectedUnitInput<PriorityHoldOptionsInput>>>? PriorityServices,
        IReadOnlyList<AncillaryVariantHoldInput<SelectedUnitInput<ConnectivityHoldOptionsInput>>>? Connectivity,
        IReadOnlyList<AncillaryVariantHoldInput<SelectedUnitInput>>? Upgrades) : IRequest<AncillaryHoldReceipt>, IHoldAncillaryServicesCommand;
}
