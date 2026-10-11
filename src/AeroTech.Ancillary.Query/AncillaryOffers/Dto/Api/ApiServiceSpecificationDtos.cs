namespace AeroTech.Ancillary.Query.AncillaryOffers.Dto.Api
{
    public sealed record ApiServiceSpecificationDto(
        ApiBaggageSpecificationDto? Baggage,
        ApiSeatSpecificationDto? Seat,
        ApiUpgradeSpecificationDto? Upgrade,
        ApiMealSpecificationDto? Meal,
        ApiPetSpecificationDto? Pet,
        ApiAssistedTravelSpecificationDto? AssistedTravel,
        ApiAirportServiceSpecificationDto? AirportService,
        ApiPrioritySpecificationDto? Priority,
        ApiConnectivitySpecificationDto? Connectivity);

    public sealed record ApiDimensionsCmDto(
        decimal LengthCm,
        decimal WidthCm,
        decimal HeightCm);

    public sealed record ApiBaggageSpecificationDto(
        string ChargeKind,
        string? AllowanceConcept,
        decimal? PackageWeightKg,
        decimal? MaxKgPerPiece,
        decimal? WeightFromExclusiveKg,
        decimal? WeightToInclusiveKg,
        ApiDimensionsCmDto? MaxSize,
        decimal? MaxLinearSumCm,
        string? EquipmentKind,
        string? ChargeCombination);

    public sealed record ApiSeatSpecificationDto(
        string SeatPurpose,
        IReadOnlyList<string> SeatCharacteristicCodes,
        IReadOnlyList<int> ApplicableCabinIds,
        bool RequiresExitRowEligibility,
        bool RequiresAdjacentSeat,
        string? ExtraSeatPurpose,
        int? ExtraOccupiedSeatCount,
        bool RequiresExternalTicketAction);

    public sealed record ApiUpgradeSpecificationDto(
        int FromCabinId,
        int ToCabinId,
        string AllowedUpgradeKind,
        bool RequiresTicketExchange,
        IReadOnlyList<long> EligibleFareFamilyIds);

    public sealed record ApiMealSpecificationDto(
        string MealKind,
        string? MealCode,
        string? MenuItemRef,
        string? DietaryCode,
        int CateringLeadTimeMinutes,
        string? ExclusiveMealFamilyCode);

    public sealed record ApiPetAnimalDto(
        string AnimalType,
        string? OtherCode);

    public sealed record ApiPetSizeBracketDto(
        string Code,
        decimal WeightFromExclusiveKg,
        decimal WeightToInclusiveKg);

    public sealed record ApiPetSpecificationDto(
        string TransportMode,
        IReadOnlyList<ApiPetAnimalDto> AllowedAnimalTypes,
        decimal MaxCombinedWeightKg,
        ApiDimensionsCmDto CarrierDimensionsMaxCm,
        int? MinAnimalAgeWeeks,
        IReadOnlyList<string> RequiredDocumentCodes,
        IReadOnlyList<ApiPetSizeBracketDto> AllowedHoldAnimalSizeBrackets);

    public sealed record ApiWheelchairDetailsDto(
        IReadOnlyList<string> AllowedSsrCodes,
        string? AssistanceLevelCode,
        int LeadTimeMinutes);

    public sealed record ApiDisabilityAssistanceDetailsDto(
        IReadOnlyList<string> AllowedSsrCodes,
        string? RequiredCommunicationMethod);

    public sealed record ApiMedicalEquipmentDetailsDto(
        string MedicalServiceCode,
        bool RequiresMedicalApproval,
        string EquipmentKind,
        decimal? OxygenUnits,
        IReadOnlyList<string> EvidenceTypeCodes);

    public sealed record ApiBassinetDetailsDto(
        decimal? MaxInfantWeightKg,
        int? MaxInfantAgeMonths,
        IReadOnlyList<string> CompatibleSeatGroups,
        bool RequiresInfantAndGuardian);

    public sealed record ApiUnaccompaniedMinorDetailsDto(
        int MinAgeYears,
        int MaxAgeYearsExclusive,
        bool GuardianContactRequired,
        string ConnectionPolicy,
        IReadOnlyList<int> AllowedTransitAirportIds);

    public sealed record ApiAssistedTravelSpecificationDto(
        string AssistanceKind,
        ApiWheelchairDetailsDto? Wheelchair,
        ApiDisabilityAssistanceDetailsDto? DisabilityAssistance,
        ApiMedicalEquipmentDetailsDto? MedicalEquipment,
        ApiBassinetDetailsDto? Bassinet,
        ApiUnaccompaniedMinorDetailsDto? UnaccompaniedMinor);

    public sealed record ApiAirportServiceSpecificationDto(
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

    public sealed record ApiPrioritySpecificationDto(
        string Kind,
        string? PriorityZoneCode,
        string? PriorityGroupCode,
        string? FareBenefitRef,
        IReadOnlyList<int> AirportIds);

    public sealed record ApiConnectivitySpecificationDto(
        string PlanKind,
        int? DurationMinutes,
        int? IncludedDataMb,
        int? MaxDevices,
        IReadOnlyList<int> EligibleAircraftIds,
        string DeliveryStage,
        string? FulfillmentProviderRef);
}
