using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Arguments
{
    public sealed record ServiceSpecificationArgs(
        BaggageSpecificationArgs? Baggage = null,
        SeatSpecificationArgs? Seat = null,
        UpgradeSpecificationArgs? Upgrade = null,
        MealSpecificationArgs? Meal = null,
        PetSpecificationArgs? Pet = null,
        AssistedTravelSpecificationArgs? AssistedTravel = null,
        AirportServiceSpecificationArgs? AirportService = null,
        PrioritySpecificationArgs? Priority = null,
        ConnectivitySpecificationArgs? Connectivity = null);

    public sealed record DimensionsCmArgs(
        decimal LengthCm,
        decimal WidthCm,
        decimal HeightCm);

    public sealed record BaggageSpecificationArgs(
        BaggageChargeKind ChargeKind,
        BaggageAllowanceConcept? AllowanceConcept,
        decimal? PackageWeightKg,
        decimal? MaxKgPerPiece,
        decimal? WeightFromExclusiveKg,
        decimal? WeightToInclusiveKg,
        DimensionsCmArgs? MaxSize,
        decimal? MaxLinearSumCm,
        string? EquipmentKind,
        BaggageChargeCombination? ChargeCombination);

    public sealed record SeatSpecificationArgs(
        SeatPurpose SeatPurpose,
        IReadOnlyList<string> SeatCharacteristicCodes,
        IReadOnlyList<int> ApplicableCabinIds,
        bool RequiresExitRowEligibility,
        bool RequiresAdjacentSeat,
        ExtraSeatPurpose? ExtraSeatPurpose,
        int? ExtraOccupiedSeatCount,
        bool RequiresExternalTicketAction);

    public sealed record UpgradeSpecificationArgs(
        int FromCabinId,
        int ToCabinId,
        UpgradeKind AllowedUpgradeKind,
        bool RequiresTicketExchange,
        IReadOnlyList<long> EligibleFareFamilyIds);

    public sealed record MealSpecificationArgs(
        MealKind MealKind,
        string? MealCode,
        string? MenuItemRef,
        string? DietaryCode,
        int CateringLeadTimeMinutes,
        string? ExclusiveMealFamilyCode);

    public sealed record PetAnimalArgs(
        PetAnimalType AnimalType,
        string? OtherCode);

    public sealed record PetSizeBracketArgs(
        string Code,
        decimal WeightFromExclusiveKg,
        decimal WeightToInclusiveKg);

    public sealed record PetSpecificationArgs(
        PetTransportMode TransportMode,
        IReadOnlyList<PetAnimalArgs> AllowedAnimalTypes,
        decimal MaxCombinedWeightKg,
        DimensionsCmArgs CarrierDimensionsMaxCm,
        int? MinAnimalAgeWeeks,
        IReadOnlyList<string> RequiredDocumentCodes,
        IReadOnlyList<PetSizeBracketArgs> AllowedHoldAnimalSizeBrackets);

    public sealed record WheelchairDetailsArgs(
        IReadOnlyList<string> AllowedSsrCodes,
        string? AssistanceLevelCode,
        int LeadTimeMinutes);

    public sealed record DisabilityAssistanceDetailsArgs(
        IReadOnlyList<string> AllowedSsrCodes,
        AssistanceCommunicationMethod? RequiredCommunicationMethod);

    public sealed record MedicalEquipmentDetailsArgs(
        string MedicalServiceCode,
        bool RequiresMedicalApproval,
        MedicalEquipmentKind EquipmentKind,
        decimal? OxygenUnits,
        IReadOnlyList<string> EvidenceTypeCodes);

    public sealed record BassinetDetailsArgs(
        decimal? MaxInfantWeightKg,
        int? MaxInfantAgeMonths,
        IReadOnlyList<string> CompatibleSeatGroups,
        bool RequiresInfantAndGuardian);

    public sealed record UnaccompaniedMinorDetailsArgs(
        int MinAgeYears,
        int MaxAgeYearsExclusive,
        bool GuardianContactRequired,
        MinorConnectionPolicy ConnectionPolicy,
        IReadOnlyList<int> AllowedTransitAirportIds);

    public sealed record AssistedTravelSpecificationArgs(
        AssistanceKind AssistanceKind,
        WheelchairDetailsArgs? Wheelchair = null,
        DisabilityAssistanceDetailsArgs? DisabilityAssistance = null,
        MedicalEquipmentDetailsArgs? MedicalEquipment = null,
        BassinetDetailsArgs? Bassinet = null,
        UnaccompaniedMinorDetailsArgs? UnaccompaniedMinor = null);

    public sealed record AirportServiceSpecificationArgs(
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
        IReadOnlyList<string> IncludedComponentCodes,
        bool RequiresSpecificAppointment);

    public sealed record PrioritySpecificationArgs(
        PriorityKind Kind,
        string? PriorityZoneCode,
        string? PriorityGroupCode,
        string? FareBenefitRef,
        IReadOnlyList<int> AirportIds);

    public sealed record ConnectivitySpecificationArgs(
        ConnectivityPlanKind PlanKind,
        int? DurationMinutes,
        int? IncludedDataMb,
        int? MaxDevices,
        IReadOnlyList<int> EligibleAircraftIds,
        PurchaseStage DeliveryStage,
        string? FulfillmentProviderRef);
}
