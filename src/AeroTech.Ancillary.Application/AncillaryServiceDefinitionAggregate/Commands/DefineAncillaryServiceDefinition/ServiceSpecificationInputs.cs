using System.Text.Json.Serialization;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition
{
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed record ServiceSpecificationInput(
        BaggageSpecificationInput? Baggage = null,
        SeatSpecificationInput? Seat = null,
        UpgradeSpecificationInput? Upgrade = null,
        MealSpecificationInput? Meal = null,
        PetSpecificationInput? Pet = null,
        AssistedTravelSpecificationInput? AssistedTravel = null,
        AirportServiceSpecificationInput? AirportService = null,
        PrioritySpecificationInput? Priority = null,
        ConnectivitySpecificationInput? Connectivity = null);

    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed record DimensionsCmInput(
        decimal LengthCm,
        decimal WidthCm,
        decimal HeightCm);

    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed record BaggageSpecificationInput(
        BaggageChargeKind ChargeKind,
        BaggageAllowanceConcept? AllowanceConcept,
        decimal? PackageWeightKg,
        decimal? MaxKgPerPiece,
        decimal? WeightFromExclusiveKg,
        decimal? WeightToInclusiveKg,
        DimensionsCmInput? MaxSize,
        decimal? MaxLinearSumCm,
        string? EquipmentKind,
        BaggageChargeCombination? ChargeCombination);

    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed record SeatSpecificationInput(
        SeatPurpose SeatPurpose,
        IReadOnlyList<string>? SeatCharacteristicCodes,
        IReadOnlyList<int>? ApplicableCabinIds,
        bool RequiresExitRowEligibility,
        bool RequiresAdjacentSeat,
        ExtraSeatPurpose? ExtraSeatPurpose,
        int? ExtraOccupiedSeatCount,
        bool RequiresExternalTicketAction);

    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed record UpgradeSpecificationInput(
        int FromCabinId,
        int ToCabinId,
        UpgradeKind AllowedUpgradeKind,
        bool RequiresTicketExchange,
        IReadOnlyList<long>? EligibleFareFamilyIds);

    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed record MealSpecificationInput(
        MealKind MealKind,
        string? MealCode,
        string? MenuItemRef,
        string? DietaryCode,
        int CateringLeadTimeMinutes,
        string? ExclusiveMealFamilyCode);

    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed record PetAnimalInput(
        PetAnimalType AnimalType,
        string? OtherCode);

    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed record PetSizeBracketInput(
        string Code,
        decimal WeightFromExclusiveKg,
        decimal WeightToInclusiveKg);

    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed record PetSpecificationInput(
        PetTransportMode TransportMode,
        IReadOnlyList<PetAnimalInput>? AllowedAnimalTypes,
        decimal MaxCombinedWeightKg,
        DimensionsCmInput CarrierDimensionsMaxCm,
        int? MinAnimalAgeWeeks,
        IReadOnlyList<string>? RequiredDocumentCodes,
        IReadOnlyList<PetSizeBracketInput>? AllowedHoldAnimalSizeBrackets);

    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed record WheelchairDetailsInput(
        IReadOnlyList<string>? AllowedSsrCodes,
        string? AssistanceLevelCode,
        int LeadTimeMinutes);

    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed record DisabilityAssistanceDetailsInput(
        IReadOnlyList<string>? AllowedSsrCodes,
        AssistanceCommunicationMethod? RequiredCommunicationMethod);

    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed record MedicalEquipmentDetailsInput(
        string MedicalServiceCode,
        bool RequiresMedicalApproval,
        MedicalEquipmentKind EquipmentKind,
        decimal? OxygenUnits,
        IReadOnlyList<string>? EvidenceTypeCodes);

    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed record BassinetDetailsInput(
        decimal? MaxInfantWeightKg,
        int? MaxInfantAgeMonths,
        IReadOnlyList<string>? CompatibleSeatGroups,
        bool RequiresInfantAndGuardian);

    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed record UnaccompaniedMinorDetailsInput(
        int MinAgeYears,
        int MaxAgeYearsExclusive,
        bool GuardianContactRequired,
        MinorConnectionPolicy ConnectionPolicy,
        IReadOnlyList<int>? AllowedTransitAirportIds);

    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed record AssistedTravelSpecificationInput(
        AssistanceKind AssistanceKind,
        WheelchairDetailsInput? Wheelchair = null,
        DisabilityAssistanceDetailsInput? DisabilityAssistance = null,
        MedicalEquipmentDetailsInput? MedicalEquipment = null,
        BassinetDetailsInput? Bassinet = null,
        UnaccompaniedMinorDetailsInput? UnaccompaniedMinor = null);

    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed record AirportServiceSpecificationInput(
        AirportServiceKind Kind,
        int AirportId,
        string? TerminalRef,
        long? FacilityId,
        AirportServiceDirection Direction,
        TimeOnly? ServiceWindowStart,
        TimeOnly? ServiceWindowEnd,
        string? IanaTimeZone,
        int? VisitDurationMinutes,
        int? MaxGuestsPerPrimary,
        IReadOnlyList<string>? IncludedComponentCodes,
        bool RequiresSpecificAppointment);

    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed record PrioritySpecificationInput(
        PriorityKind Kind,
        string? PriorityZoneCode,
        string? PriorityGroupCode,
        string? FareBenefitRef,
        IReadOnlyList<int>? AirportIds);

    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    public sealed record ConnectivitySpecificationInput(
        ConnectivityPlanKind PlanKind,
        int? DurationMinutes,
        int? IncludedDataMb,
        int? MaxDevices,
        IReadOnlyList<int>? EligibleAircraftIds,
        PurchaseStage DeliveryStage,
        string? FulfillmentProviderRef);
}
