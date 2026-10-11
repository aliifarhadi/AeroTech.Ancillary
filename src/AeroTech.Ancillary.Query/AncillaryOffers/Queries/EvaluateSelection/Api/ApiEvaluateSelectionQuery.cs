using AeroTech.Ancillary.Query.AncillaryOffers.Dto.Api;
using AeroTech.Ancillary.Query.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Messages.Ancillary.Enums;
using MediatR;

namespace AeroTech.Ancillary.Query.AncillaryOffers.Queries.EvaluateSelection.Api
{
    public sealed class ApiEvaluateSelectionQuery : IRequest<ApiSelectionResultDto>, IAncillaryOfferItemQuery
    {
        public string? OfferId { get; set; }

        public string? OfferItemRef { get; set; }

        public ApiAncillarySelectionInput? Selection { get; set; }
    }

    public sealed class ApiAncillarySelectionInput
    {
        public int? Quantity { get; set; }

        public string? PackageProductRef { get; set; }

        public string? BagRef { get; set; }

        public decimal? MeasuredWeightKg { get; set; }

        public ApiSelectedDimensionsInput? Dimensions { get; set; }

        public string? EquipmentKind { get; set; }

        public decimal? WeightKg { get; set; }

        public string? SeatNumber { get; set; }

        public bool? ExitRowTermsAccepted { get; set; }

        public ExtraSeatPurpose? Purpose { get; set; }

        public int? TargetCabinId { get; set; }

        public string? QuoteRef { get; set; }

        public string? MealCode { get; set; }

        public string? MenuItemRef { get; set; }

        public PetAnimalType? AnimalType { get; set; }

        public string? OtherAnimalCode { get; set; }

        public int? AnimalAgeWeeks { get; set; }

        public decimal? CombinedWeightKg { get; set; }

        public ApiSelectedDimensionsInput? CarrierDimensions { get; set; }

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

    public sealed class ApiSelectedDimensionsInput
    {
        public decimal LengthCm { get; set; }

        public decimal WidthCm { get; set; }

        public decimal HeightCm { get; set; }
    }
}
