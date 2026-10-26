namespace AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices
{
    public interface IHoldAncillaryServicesCommand
    {
        string IdempotencyKey { get; }

        long OrderId { get; }

        string Reference { get; }

        DateTimeOffset RequestedExpiresAt { get; }

        IReadOnlyList<AncillaryVariantHoldInput<SelectedUnitInput<BaggageHoldOptionsInput>>>? Baggages { get; }

        IReadOnlyList<AncillaryVariantHoldInput<SelectedUnitInput>>? Meals { get; }

        IReadOnlyList<AncillaryVariantHoldInput<SelectedUnitInput<AssistedTravelHoldOptionsInput>>>? AssistedTravel { get; }

        IReadOnlyList<AncillaryVariantHoldInput<SelectedUnitInput<AirportServiceHoldOptionsInput>>>? AirportServices { get; }

        IReadOnlyList<AncillaryVariantHoldInput<SelectedUnitInput<PriorityHoldOptionsInput>>>? PriorityServices { get; }

        IReadOnlyList<AncillaryVariantHoldInput<SelectedUnitInput<ConnectivityHoldOptionsInput>>>? Connectivity { get; }

        IReadOnlyList<AncillaryVariantHoldInput<SelectedUnitInput>>? Upgrades { get; }
    }
}
