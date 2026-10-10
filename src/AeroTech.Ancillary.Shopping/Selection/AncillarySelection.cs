using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Shopping.Selection
{
    public sealed record AncillarySelection
    {
        public int? Quantity { get; init; }

        public string? PackageProductRef { get; init; }

        public string? BagRef { get; init; }

        public decimal? MeasuredWeightKg { get; init; }

        public SelectedDimensions? Dimensions { get; init; }

        public string? EquipmentKind { get; init; }

        public decimal? WeightKg { get; init; }

        public string? SeatNumber { get; init; }

        public IReadOnlyList<string>? VerifiedSeatCharacteristicCodes { get; init; }

        public bool? ExitRowTermsAccepted { get; init; }

        public ExtraSeatPurpose? Purpose { get; init; }

        public int? TargetCabinId { get; init; }

        public string? QuoteRef { get; init; }

        public string? MealCode { get; init; }

        public string? MenuItemRef { get; init; }

        public PetAnimalType? AnimalType { get; init; }

        public string? OtherAnimalCode { get; init; }

        public int? AnimalAgeWeeks { get; init; }

        public decimal? CombinedWeightKg { get; init; }

        public SelectedDimensions? CarrierDimensions { get; init; }

        public IReadOnlyList<string>? DocumentAcknowledgements { get; init; }

        public string? SizeBracket { get; init; }

        public string? AssistanceSsrCode { get; init; }

        public string? EquipmentCode { get; init; }

        public IReadOnlyList<string>? EvidenceDocumentRefs { get; init; }

        public decimal? OxygenUnits { get; init; }

        public string? InfantRef { get; init; }

        public string? GuardianRef { get; init; }

        public string? ChildRef { get; init; }

        public bool GuardianHandoffContactProvided { get; init; }

        public bool GuardianPickupContactProvided { get; init; }

        public int? AirportId { get; init; }

        public long? FacilityRef { get; init; }

        public DateTimeOffset? TimeWithOffset { get; init; }

        public int? GuestCount { get; init; }

        public bool? OptIn { get; init; }

        public string? PlanCode { get; init; }

        public int? DeviceCount { get; init; }
    }

    public sealed record SelectedDimensions(decimal LengthCm, decimal WidthCm, decimal HeightCm);
}
