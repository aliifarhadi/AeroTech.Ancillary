using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto
{
    public sealed record ServiceSpecificationDto(
        BaggageSpecificationDto? Baggage,
        SeatSpecificationDto? Seat,
        UpgradeSpecificationDto? Upgrade,
        MealSpecificationDto? Meal,
        PetSpecificationDto? Pet,
        AssistedTravelSpecificationDto? AssistedTravel,
        AirportServiceSpecificationDto? AirportService,
        PrioritySpecificationDto? Priority,
        ConnectivitySpecificationDto? Connectivity);

    public sealed record DimensionsCmDto(
        decimal LengthCm,
        decimal WidthCm,
        decimal HeightCm);

    public sealed record BaggageSpecificationDto(
        EnumValueDto ChargeKind,
        EnumValueDto? AllowanceConcept,
        decimal? PackageWeightKg,
        decimal? MaxKgPerPiece,
        decimal? WeightFromExclusiveKg,
        decimal? WeightToInclusiveKg,
        DimensionsCmDto? MaxSize,
        decimal? MaxLinearSumCm,
        string? EquipmentKind,
        EnumValueDto? ChargeCombination);

    public sealed record SeatSpecificationDto(
        EnumValueDto SeatPurpose,
        IReadOnlyList<string> SeatCharacteristicCodes,
        IReadOnlyList<int> ApplicableCabinIds,
        bool RequiresExitRowEligibility,
        bool RequiresAdjacentSeat,
        EnumValueDto? ExtraSeatPurpose,
        int? ExtraOccupiedSeatCount,
        bool RequiresExternalTicketAction);

    public sealed record UpgradeSpecificationDto(
        int FromCabinId,
        int ToCabinId,
        EnumValueDto AllowedUpgradeKind,
        bool RequiresTicketExchange,
        IReadOnlyList<long> EligibleFareFamilyIds);

    public sealed record MealSpecificationDto(
        EnumValueDto MealKind,
        string? MealCode,
        string? MenuItemRef,
        string? DietaryCode,
        int CateringLeadTimeMinutes,
        string? ExclusiveMealFamilyCode);

    public sealed record PetAnimalDto(
        EnumValueDto AnimalType,
        string? OtherCode);

    public sealed record PetSizeBracketDto(
        string Code,
        decimal WeightFromExclusiveKg,
        decimal WeightToInclusiveKg);

    public sealed record PetSpecificationDto(
        EnumValueDto TransportMode,
        IReadOnlyList<PetAnimalDto> AllowedAnimalTypes,
        decimal MaxCombinedWeightKg,
        DimensionsCmDto CarrierDimensionsMaxCm,
        int? MinAnimalAgeWeeks,
        IReadOnlyList<string> RequiredDocumentCodes,
        IReadOnlyList<PetSizeBracketDto> AllowedHoldAnimalSizeBrackets);

    public sealed record WheelchairDetailsDto(
        IReadOnlyList<string> AllowedSsrCodes,
        string? AssistanceLevelCode,
        int LeadTimeMinutes);

    public sealed record DisabilityAssistanceDetailsDto(
        IReadOnlyList<string> AllowedSsrCodes,
        EnumValueDto? RequiredCommunicationMethod);

    public sealed record MedicalEquipmentDetailsDto(
        string MedicalServiceCode,
        bool RequiresMedicalApproval,
        EnumValueDto EquipmentKind,
        decimal? OxygenUnits,
        IReadOnlyList<string> EvidenceTypeCodes);

    public sealed record BassinetDetailsDto(
        decimal? MaxInfantWeightKg,
        int? MaxInfantAgeMonths,
        IReadOnlyList<string> CompatibleSeatGroups,
        bool RequiresInfantAndGuardian);

    public sealed record UnaccompaniedMinorDetailsDto(
        int MinAgeYears,
        int MaxAgeYearsExclusive,
        bool GuardianContactRequired,
        EnumValueDto ConnectionPolicy,
        IReadOnlyList<int> AllowedTransitAirportIds);

    public sealed record AssistedTravelSpecificationDto(
        EnumValueDto AssistanceKind,
        WheelchairDetailsDto? Wheelchair,
        DisabilityAssistanceDetailsDto? DisabilityAssistance,
        MedicalEquipmentDetailsDto? MedicalEquipment,
        BassinetDetailsDto? Bassinet,
        UnaccompaniedMinorDetailsDto? UnaccompaniedMinor);

    public sealed record AirportServiceSpecificationDto(
        EnumValueDto Kind,
        int AirportId,
        string? TerminalRef,
        long? FacilityId,
        EnumValueDto Direction,
        TimeOnly? ServiceWindowStart,
        TimeOnly? ServiceWindowEnd,
        string? IanaTimeZone,
        int? VisitDurationMinutes,
        int? MaxGuestsPerPrimary,
        IReadOnlyList<string> IncludedComponentCodes,
        bool RequiresSpecificAppointment);

    public sealed record PrioritySpecificationDto(
        EnumValueDto Kind,
        string? PriorityZoneCode,
        string? PriorityGroupCode,
        string? FareBenefitRef,
        IReadOnlyList<int> AirportIds);

    public sealed record ConnectivitySpecificationDto(
        EnumValueDto PlanKind,
        int? DurationMinutes,
        int? IncludedDataMb,
        int? MaxDevices,
        IReadOnlyList<int> EligibleAircraftIds,
        EnumValueDto DeliveryStage,
        string? FulfillmentProviderRef);
}
