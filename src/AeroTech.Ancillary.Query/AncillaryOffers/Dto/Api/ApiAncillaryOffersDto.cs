namespace AeroTech.Ancillary.Query.AncillaryOffers.Dto.Api
{
    public sealed record ApiAncillaryOffersDto(
        string SourceOfferId,
        ApiCurrencyDto Currency,
        ApiDataListsDto DataLists,
        IReadOnlyList<ApiALaCarteOfferDto> ALaCarteOffers,
        int WithheldItemCount,
        string ResponseStatus);

    public sealed record ApiCurrencyDto(int Id, string? Code);

    public sealed record ApiDataListsDto(
        IReadOnlyList<ApiPassengerDto> Passengers,
        IReadOnlyList<ApiPaxSegmentDto> PaxSegments,
        IReadOnlyList<ApiServiceDefinitionDto> ServiceDefinitions);

    public sealed record ApiPassengerDto(string PassengerRef, string Ptc);

    public sealed record ApiPaxSegmentDto(
        string SegmentRef,
        string JourneyRef,
        long FlightId,
        string? FlightNumber,
        int MarketingAirlineId,
        int OperatingAirlineId,
        int OriginAirportId,
        int DestinationAirportId,
        DateTimeOffset DepartureAt,
        DateTimeOffset ArrivalAt,
        int? CabinClassId);

    public sealed record ApiServiceDefinitionDto(
        string ServiceDefinitionRef,
        string Name,
        string? Description,
        string Category,
        string VariantCode,
        string ServiceTypeCode,
        string ServiceSubCode,
        string? Rfic,
        string? Rfisc,
        string DocumentType,
        string BookingMethod,
        string? SsrCode);

    public sealed record ApiALaCarteOfferDto(IReadOnlyList<ApiOfferItemDto> OfferItems);

    public sealed record ApiOfferItemDto(
        string OfferItemRef,
        string ServiceDefinitionRef,
        ApiEligibilityDto Eligibility,
        ApiPricingDto Pricing,
        ApiQuantityDto Quantity,
        IReadOnlyList<ApiSelectionRequirementDto> SelectionRequirements,
        ApiAvailabilityDto Availability,
        string ConfirmationRequirement,
        string Readiness,
        bool CanPurchase,
        IReadOnlyList<string> Reasons);

    public sealed record ApiEligibilityDto(
        string CoverageScope,
        IReadOnlyList<string> PassengerRefs,
        IReadOnlyList<string> SegmentRefs,
        IReadOnlyList<string> JourneyRefs);

    public sealed record ApiPricingDto(
        string Origin,
        string Status,
        string? Unit,
        ApiMoneyDto? UnitPrice,
        ApiMoneyDto? Base,
        IReadOnlyList<ApiPriceLineDto> Taxes,
        IReadOnlyList<ApiPriceLineDto> IncludedTaxes,
        IReadOnlyList<ApiPriceLineDto> AdditionalFees,
        IReadOnlyList<ApiPriceLineDto> OrderLevelFees,
        ApiMoneyDto? TotalPrice,
        bool TotalComplete);

    public sealed record ApiMoneyDto(decimal Amount, int CurrencyId);

    public sealed record ApiPriceLineDto(string? Code, decimal Amount, int CurrencyId, string? AppliesPer);

    public sealed record ApiQuantityDto(string Unit, int Min, int Max);

    public sealed record ApiSelectionRequirementDto(string Name, string Type, bool Required);

    public sealed record ApiAvailabilityDto(string State, bool Guaranteed, bool RequiresCheckAtReserve);

    public sealed record ApiAncillaryOfferDetailDto(
        string SourceOfferId,
        ApiCurrencyDto Currency,
        ApiOfferItemDto OfferItem,
        ApiServiceDefinitionDto ServiceDefinition,
        ApiServiceSpecificationDto Specification,
        IReadOnlyList<ApiIncludedAllowanceDto> IncludedAllowance,
        ApiBaggageRuleDto? BaggageRule,
        ApiSeatRuleDto? SeatRule,
        ApiRestrictionsDto Restrictions,
        ApiTermsDto Terms,
        IReadOnlyList<ApiRateDto> Rates);

    public sealed record ApiSelectionResultDto(
        string SourceOfferId,
        string Status,
        string OfferItemRef,
        string CurrentOfferItemRef,
        bool OfferItemChanged,
        ApiOfferItemDto OfferItem,
        ApiMoneyDto? SelectedPrice,
        IReadOnlyList<ApiSelectionIssueDto> Issues,
        bool CanPurchase);

    public sealed record ApiSelectionIssueDto(string Field, string Reason);
}
