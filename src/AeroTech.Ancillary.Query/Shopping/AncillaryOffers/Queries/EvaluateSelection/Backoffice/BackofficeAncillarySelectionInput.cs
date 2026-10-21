using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateSelection.Backoffice
{
    public sealed class BackofficeAncillarySelectionInput
    {
        public int? Quantity { get; set; }

        public string? PackageProductRef { get; set; }

        public string? BagRef { get; set; }

        public decimal? MeasuredWeightKg { get; set; }

        public BackofficeSelectedDimensionsInput? Dimensions { get; set; }

        public string? EquipmentKind { get; set; }

        public decimal? WeightKg { get; set; }

        public string? SeatNumber { get; set; }

        public ExtraSeatPurpose? Purpose { get; set; }

        public int? TargetCabinId { get; set; }

        public string? QuoteRef { get; set; }

        public string? MealCode { get; set; }

        public string? MenuItemRef { get; set; }

        public PetAnimalType? AnimalType { get; set; }

        public string? OtherAnimalCode { get; set; }

        public int? AnimalAgeWeeks { get; set; }

        public decimal? CombinedWeightKg { get; set; }

        public BackofficeSelectedDimensionsInput? CarrierDimensions { get; set; }

        public IReadOnlyList<string>? DocumentAcknowledgements { get; set; }

        public string? SizeBracket { get; set; }

        public string? AssistanceSsrCode { get; set; }

        public string? EquipmentCode { get; set; }

        public IReadOnlyList<string>? EvidenceDocumentRefs { get; set; }

        public decimal? OxygenUnits { get; set; }

        public string? InfantRef { get; set; }

        public string? GuardianRef { get; set; }

        public string? ChildRef { get; set; }

        public bool GuardianHandoffContactProvided { get; set; }

        public bool GuardianPickupContactProvided { get; set; }

        public int? AirportId { get; set; }

        public long? FacilityRef { get; set; }

        public DateTimeOffset? TimeWithOffset { get; set; }

        public int? GuestCount { get; set; }

        public bool? OptIn { get; set; }

        public string? PlanCode { get; set; }

        public int? DeviceCount { get; set; }
    }
}
