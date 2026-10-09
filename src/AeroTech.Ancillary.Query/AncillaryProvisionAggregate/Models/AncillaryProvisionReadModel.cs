using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Models
{
    public sealed class AncillaryProvisionReadModel
    {
        public long Id { get; set; }

        public long ServiceDefinitionId { get; set; }

        public int Sequence { get; set; }

        public ProvisionStatus Status { get; set; }

        public DateTimeOffset? SalesEffectiveFrom { get; set; }

        public DateTimeOffset? SalesDiscontinueAt { get; set; }

        public ServiceCoverageScope CoverageScope { get; set; }

        public PurchaseStage PurchaseStage { get; set; }

        public PriceOrigin PriceOrigin { get; set; }

        public string? QuoteProviderKey { get; set; }

        public int? AdvancePurchasePeriod { get; set; }

        public TimeUnit? AdvancePurchaseUnit { get; set; }

        public bool AdvancePurchaseSameTimeAsTicketed { get; set; }

        public int? AdvancePurchaseMaximumPeriod { get; set; }

        public AncillaryQuantityUnit QuantityUnit { get; set; }

        public int MinQuantity { get; set; }

        public int MaxQuantity { get; set; }

        public ProvisionApplicationType ApplicationType { get; set; }

        public int? BaggageFreePieces { get; set; }

        public int? BaggageFirstExcessPiece { get; set; }

        public int? BaggageLastExcessPiece { get; set; }

        public decimal? BaggageWeight { get; set; }

        public WeightUnit? BaggageWeightUnit { get; set; }

        public BaggageTravelApplication? BaggageTravelApplication { get; set; }

        public BaggagePurchaseApplication? BaggagePurchaseApplication { get; set; }

        public BaggageRuleDeference? BaggageRuleDeference { get; set; }

        public BaggageChargeKind? BaggageChargeKind { get; set; }

        public BaggageAllowanceConcept? BaggageAllowanceConcept { get; set; }

        public CommercialDisposition Disposition { get; set; }

        public bool DocumentRequired { get; set; }

        public bool BookingRequired { get; set; }

        public ReissueRefundPolicy ReissueRefund { get; set; }

        public FormOfRefund? FormOfRefund { get; set; }

        public bool Commissionable { get; set; }

        public bool InterlineSettlement { get; set; }

        public bool MustCheckAvailability { get; set; }

        public string FulfillmentProviderKey { get; set; } = default!;

        public long? PassengerEligibilityRuleId { get; set; }

        public long? SalesRestrictionsRuleId { get; set; }

        public long? GeographyRuleId { get; set; }

        public long? FlightApplicationRuleId { get; set; }

        public long? FareApplicationRuleId { get; set; }

        public long? TravelDateRuleId { get; set; }

        public long? DayTimeApplicationRuleId { get; set; }

        public long? AdvancePurchaseRuleId { get; set; }

        public long? BaggageApplicationRuleId { get; set; }

        public long? SeatApplicationRuleId { get; set; }

        public long? PetRuleId { get; set; }

        public string? PetCountryExceptionCode { get; set; }

        public int? PetMinAnimalAgeWeeksOverride { get; set; }

        public decimal? PetMaxCombinedKgOverride { get; set; }

        public ConfirmationRequirement? PetAcceptanceMode { get; set; }

        public long? AssistedTravelRuleId { get; set; }

        public int? AssistedTravelMinimumLeadTimeMinutes { get; set; }

        public MinorConnectionPolicy? AssistedTravelConnectionPolicy { get; set; }

        public bool? AssistedTravelMedicalApprovalRequired { get; set; }

        public long? AirportServiceRuleId { get; set; }

        public string? AirportServiceTerminalRef { get; set; }

        public AirportServiceDirection? AirportServiceDirection { get; set; }

        public TimeOnly? AirportServiceWindowStart { get; set; }

        public TimeOnly? AirportServiceWindowEnd { get; set; }

        public long? AirportServiceFacilityId { get; set; }

        public int? AirportServiceMaxGuestsPerPrimary { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset? ActivatedAt { get; set; }

        public DateTimeOffset? SuspendedAt { get; set; }

        public DateTimeOffset? RetiredAt { get; set; }

        public DateTimeOffset LastUpdateTime { get; set; }
    }
}
