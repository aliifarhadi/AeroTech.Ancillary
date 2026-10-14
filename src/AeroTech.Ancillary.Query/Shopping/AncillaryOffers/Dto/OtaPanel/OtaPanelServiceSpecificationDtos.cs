namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.OtaPanel
{
    public sealed record OtaPanelServiceSpecificationDto(
        OtaPanelBaggageSpecificationDto? Baggage,
        OtaPanelSeatSpecificationDto? Seat,
        OtaPanelUpgradeSpecificationDto? Upgrade,
        OtaPanelMealSpecificationDto? Meal,
        OtaPanelPetSpecificationDto? Pet,
        OtaPanelAssistedTravelSpecificationDto? AssistedTravel,
        OtaPanelAirportServiceSpecificationDto? AirportService,
        OtaPanelPrioritySpecificationDto? Priority,
        OtaPanelConnectivitySpecificationDto? Connectivity);

    public sealed record OtaPanelDimensionsCmDto(
        decimal LengthCm,
        decimal WidthCm,
        decimal HeightCm);

    public sealed record OtaPanelBaggageSpecificationDto(
        string ChargeKind,
        string? AllowanceConcept,
        decimal? PackageWeightKg,
        decimal? MaxKgPerPiece,
        decimal? WeightFromExclusiveKg,
        decimal? WeightToInclusiveKg,
        OtaPanelDimensionsCmDto? MaxSize,
        decimal? MaxLinearSumCm,
        string? EquipmentKind,
        string? ChargeCombination);

    public sealed record OtaPanelSeatSpecificationDto(
        string SeatPurpose,
        IReadOnlyList<string> SeatCharacteristicCodes,
        IReadOnlyList<int> ApplicableCabinIds,
        bool RequiresExitRowEligibility,
        bool RequiresAdjacentSeat,
        string? ExtraSeatPurpose,
        int? ExtraOccupiedSeatCount,
        bool RequiresExternalTicketAction);

    public sealed record OtaPanelUpgradeSpecificationDto(
        int FromCabinId,
        int ToCabinId,
        string AllowedUpgradeKind,
        bool RequiresTicketExchange,
        IReadOnlyList<long> EligibleFareFamilyIds);

    public sealed record OtaPanelMealSpecificationDto(
        string MealKind,
        string? MealCode,
        string? MenuItemRef,
        string? DietaryCode,
        int CateringLeadTimeMinutes,
        string? ExclusiveMealFamilyCode);

    public sealed record OtaPanelPetAnimalDto(
        string AnimalType,
        string? OtherCode);

    public sealed record OtaPanelPetSizeBracketDto(
        string Code,
        decimal WeightFromExclusiveKg,
        decimal WeightToInclusiveKg);

    public sealed record OtaPanelPetSpecificationDto(
        string TransportMode,
        IReadOnlyList<OtaPanelPetAnimalDto> AllowedAnimalTypes,
        decimal MaxCombinedWeightKg,
        OtaPanelDimensionsCmDto CarrierDimensionsMaxCm,
        int? MinAnimalAgeWeeks,
        IReadOnlyList<string> RequiredDocumentCodes,
        IReadOnlyList<OtaPanelPetSizeBracketDto> AllowedHoldAnimalSizeBrackets);

    public sealed record OtaPanelWheelchairDetailsDto(
        IReadOnlyList<string> AllowedSsrCodes,
        string? AssistanceLevelCode,
        int LeadTimeMinutes);

    public sealed record OtaPanelDisabilityAssistanceDetailsDto(
        IReadOnlyList<string> AllowedSsrCodes,
        string? RequiredCommunicationMethod);

    public sealed record OtaPanelMedicalEquipmentDetailsDto(
        string MedicalServiceCode,
        bool RequiresMedicalApproval,
        string EquipmentKind,
        decimal? OxygenUnits,
        IReadOnlyList<string> EvidenceTypeCodes);

    public sealed record OtaPanelBassinetDetailsDto(
        decimal? MaxInfantWeightKg,
        int? MaxInfantAgeMonths,
        IReadOnlyList<string> CompatibleSeatGroups,
        bool RequiresInfantAndGuardian);

    public sealed record OtaPanelUnaccompaniedMinorDetailsDto(
        int MinAgeYears,
        int MaxAgeYearsExclusive,
        bool GuardianContactRequired,
        string ConnectionPolicy,
        IReadOnlyList<int> AllowedTransitAirportIds);

    public sealed record OtaPanelAssistedTravelSpecificationDto(
        string AssistanceKind,
        OtaPanelWheelchairDetailsDto? Wheelchair,
        OtaPanelDisabilityAssistanceDetailsDto? DisabilityAssistance,
        OtaPanelMedicalEquipmentDetailsDto? MedicalEquipment,
        OtaPanelBassinetDetailsDto? Bassinet,
        OtaPanelUnaccompaniedMinorDetailsDto? UnaccompaniedMinor);

    public sealed record OtaPanelAirportServiceSpecificationDto(
        string Kind,
        int AirportId,
        string? TerminalRef,
        long? FacilityId,
        string Direction,
        TimeOnly? ServiceWindowStart,
        TimeOnly? ServiceWindowEnd,
        string? IanaTimeZone,
        int? VisitDurationMinutes,
        int? MaxGuestsPerPrimary,
        IReadOnlyList<string> IncludedComponentCodes,
        bool RequiresSpecificAppointment);

    public sealed record OtaPanelPrioritySpecificationDto(
        string Kind,
        string? PriorityZoneCode,
        string? PriorityGroupCode,
        string? FareBenefitRef,
        IReadOnlyList<int> AirportIds);

    public sealed record OtaPanelConnectivitySpecificationDto(
        string PlanKind,
        int? DurationMinutes,
        int? IncludedDataMb,
        int? MaxDevices,
        IReadOnlyList<int> EligibleAircraftIds,
        string DeliveryStage,
        string? FulfillmentProviderRef);
}
