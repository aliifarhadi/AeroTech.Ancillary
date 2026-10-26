namespace AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Arguments
{
    public sealed record AncillaryReservationSelectionArgs(
        BaggageSelectionArgs? Baggage = null,
        MealSelectionArgs? Meal = null,
        AssistedTravelSelectionArgs? AssistedTravel = null,
        AirportServiceSelectionArgs? AirportService = null,
        ConnectivitySelectionArgs? Connectivity = null,
        UpgradeSelectionArgs? Upgrade = null);

    public sealed record SelectedDimensionsArgs(decimal LengthCm, decimal WidthCm, decimal HeightCm);

    public sealed record BaggageSelectionArgs(
        string? BagRef = null,
        decimal? MeasuredWeightKg = null,
        SelectedDimensionsArgs? Dimensions = null,
        decimal? WeightKg = null,
        string? EquipmentKind = null);

    public sealed record MealSelectionArgs(string? MealCode = null, string? MenuItemRef = null);

    public sealed record AssistedTravelSelectionArgs(string? AssistanceSsrCode);

    public sealed record AirportServiceSelectionArgs(
        DateTimeOffset? TimeWithOffset = null,
        int? GuestCount = null,
        int? AirportId = null,
        long? FacilityId = null);

    public sealed record ConnectivitySelectionArgs(int? DeviceCount);

    public sealed record UpgradeSelectionArgs(int ToCabinId);
}
