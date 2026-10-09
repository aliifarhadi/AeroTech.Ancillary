using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto
{
    public sealed record BackofficeProvisionDto(
        long Id,
        long ServiceDefinitionId,
        EnumValueDto? ServiceDateBasis,
        EnumValueDto? Profile,
        string? VariantCode,
        IReadOnlyList<string> ApplicableRuleSections,
        int Sequence,
        EnumValueDto Status,
        EnumValueDto CoverageScope,
        EnumValueDto PurchaseStage,
        EnumValueDto PriceOrigin,
        string? QuoteProviderKey,
        EnumValueDto QuantityUnit,
        int MinQuantity,
        int MaxQuantity,
        EnumValueDto ApplicationType,
        EnumValueDto Disposition,
        bool DocumentRequired,
        bool BookingRequired,
        EnumValueDto ReissueRefund,
        EnumValueDto? FormOfRefund,
        bool Commissionable,
        bool InterlineSettlement,
        bool MustCheckAvailability,
        string FulfillmentProviderKey,
        DateTimeOffset CreatedAt,
        DateTimeOffset? ActivatedAt,
        DateTimeOffset? SuspendedAt,
        DateTimeOffset? RetiredAt,
        BackofficeProvisionPassengerEligibilityDto? PassengerEligibility,
        BackofficeProvisionSalesRestrictionsDto? SalesRestrictions,
        BackofficeProvisionGeographyDto? Geography,
        BackofficeProvisionFlightApplicationDto? FlightApplication,
        BackofficeProvisionFareApplicationDto? FareApplication,
        BackofficeProvisionTravelDateDto? TravelDate,
        BackofficeProvisionDayTimeApplicationDto? DayTimeApplication,
        BackofficeProvisionAdvancePurchaseDto? AdvancePurchase,
        BackofficeProvisionBaggageApplicationDto? BaggageApplication,
        BackofficeProvisionSeatApplicationDto? SeatApplication,
        BackofficeProvisionPetRuleDto? PetRule,
        BackofficeProvisionAssistedTravelRuleDto? AssistedTravelRule,
        BackofficeProvisionAirportServiceRuleDto? AirportServiceRule);

    public sealed record BackofficeProvisionRuleRowDto<TValue>(
        long Id,
        TValue Value);

    public sealed record BackofficeProvisionPassengerEligibilityDto(
        long Id,
        IReadOnlyList<BackofficeProvisionRuleRowDto<EnumValueDto>> AllowedPassengerTypes,
        IReadOnlyList<BackofficeProvisionAgeBandDto> AllowedAgeBands);

    public sealed record BackofficeProvisionSalesRestrictionsDto(
        long Id,
        DateTimeOffset? SalesEffectiveFrom,
        DateTimeOffset? SalesDiscontinueAt,
        IReadOnlyList<BackofficeProvisionRuleRowDto<long>> AllowedPointsOfSale,
        IReadOnlyList<BackofficeProvisionRuleRowDto<long>> AllowedCustomers,
        IReadOnlyList<BackofficeProvisionRuleRowDto<EnumValueDto>> AllowedCustomerTypes);

    public sealed record BackofficeProvisionGeographyDto(
        long Id,
        IReadOnlyList<BackofficeProvisionRuleRowDto<int>> AllowedOriginAirports,
        IReadOnlyList<BackofficeProvisionRuleRowDto<int>> AllowedDestinationAirports,
        IReadOnlyList<BackofficeProvisionRuleRowDto<int>> AllowedViaAirports,
        IReadOnlyList<BackofficeProvisionRuleRowDto<int>> CoverageCountries,
        IReadOnlyList<BackofficeProvisionRoutePairDto> AllowedRoutePairs,
        IReadOnlyList<BackofficeProvisionServiceLocationDto> ServiceLocations);

    public sealed record BackofficeProvisionFlightApplicationDto(
        long Id,
        IReadOnlyList<BackofficeProvisionRuleRowDto<int>> AllowedMarketingAirlines,
        IReadOnlyList<BackofficeProvisionRuleRowDto<int>> AllowedOperatingAirlines,
        IReadOnlyList<BackofficeProvisionRuleRowDto<string>> AllowedFlightNumbers,
        IReadOnlyList<BackofficeProvisionRuleRowDto<long>> AllowedFlights,
        IReadOnlyList<BackofficeProvisionRuleRowDto<int>> AllowedAircraft);

    public sealed record BackofficeProvisionFareApplicationDto(
        long Id,
        IReadOnlyList<BackofficeProvisionRuleRowDto<long>> AllowedAirFares,
        IReadOnlyList<BackofficeProvisionRuleRowDto<EnumValueDto>> AllowedAirFareTypes,
        IReadOnlyList<BackofficeProvisionRuleRowDto<long>> AllowedFareFamilies,
        IReadOnlyList<BackofficeProvisionRuleRowDto<string>> AllowedFareBases,
        IReadOnlyList<BackofficeProvisionRuleRowDto<int>> AllowedCabinClasses,
        IReadOnlyList<BackofficeProvisionRuleRowDto<long>> AllowedRbds);

    public sealed record BackofficeProvisionTravelDateDto(
        long Id,
        IReadOnlyList<BackofficeProvisionDatePeriodDto> PermittedPeriods,
        IReadOnlyList<BackofficeProvisionDatePeriodDto> BlackoutPeriods);

    public sealed record BackofficeProvisionDayTimeApplicationDto(
        long Id,
        IReadOnlyList<BackofficeProvisionDayTimeWindowDto> Windows);

    public sealed record BackofficeProvisionSeatApplicationDto(
        long Id,
        IReadOnlyList<BackofficeProvisionRuleRowDto<string>> SeatNumbers,
        IReadOnlyList<BackofficeProvisionRuleRowDto<string>> SeatCharacteristics);

    public sealed record BackofficeProvisionPetRuleDto(
        long Id,
        string? CountryExceptionCode,
        int? MinAnimalAgeWeeksOverride,
        decimal? MaxCombinedKgOverride,
        EnumValueDto AcceptanceMode);

    public sealed record BackofficeProvisionAssistedTravelRuleDto(
        long Id,
        int? MinimumLeadTimeMinutes,
        EnumValueDto? ConnectionPolicy,
        bool? MedicalApprovalRequired);

    public sealed record BackofficeProvisionAirportServiceRuleDto(
        long Id,
        string? TerminalRef,
        EnumValueDto? Direction,
        TimeOnly? ServiceWindowStart,
        TimeOnly? ServiceWindowEnd,
        long? FacilityId,
        int? MaxGuestsPerPrimary);

    public sealed record BackofficeProvisionAgeBandDto(
        long Id,
        int AgeFromInclusive,
        int? AgeToExclusive);

    public sealed record BackofficeProvisionRoutePairDto(
        long Id,
        int OriginAirportId,
        int DestinationAirportId,
        EnumValueDto Direction);

    public sealed record BackofficeProvisionServiceLocationDto(
        long Id,
        EnumValueDto LocationType,
        int LocationId);

    public sealed record BackofficeProvisionDatePeriodDto(
        long Id,
        DateOnly StartDate,
        DateOnly EndDate);

    public sealed record BackofficeProvisionDayTimeWindowDto(
        long Id,
        byte DaysOfWeekMask,
        IReadOnlyList<string> DaysOfWeek,
        TimeOnly? StartLocalTime,
        TimeOnly? EndLocalTime,
        EnumValueDto Effect);

    public sealed record BackofficeProvisionAdvancePurchaseDto(
        long Id,
        int MinimumPeriod,
        EnumValueDto Unit,
        bool SameTimeAsTicketed,
        int? MaximumPeriod);

    public sealed record BackofficeProvisionBaggageApplicationDto(
        long Id,
        int? FreePieces,
        int? FirstExcessPiece,
        int? LastExcessPiece,
        decimal? Weight,
        EnumValueDto WeightUnit,
        EnumValueDto? TravelApplication,
        EnumValueDto PurchaseApplication,
        EnumValueDto? RuleDeference,
        EnumValueDto? ChargeKind,
        EnumValueDto? AllowanceConcept);
}
