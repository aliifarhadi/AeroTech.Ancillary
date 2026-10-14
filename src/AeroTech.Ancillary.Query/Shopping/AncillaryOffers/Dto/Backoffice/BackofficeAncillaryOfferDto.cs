using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;
using AeroTech.Ancillary.Query.Shopping.Common;
using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice
{
    public sealed record BackofficeAncillaryOfferDto(
        string OfferItemRef,
        long PointOfSaleId,
        OfferCandidateIdentityDto CandidateIdentity,
        OfferServiceDto Service,
        OfferCoverageDto Coverage,
        OfferEligibilityDto Eligibility,
        CustomerSelectionContractDto Selection,
        OfferQuantityDto Quantity,
        OfferPricingDto Pricing,
        OfferAvailabilityDto Availability,
        OfferConfirmationDto Confirmation,
        OfferDocumentDto Document,
        EnumValueDto OfferReadiness,
        EnumValueDto DisplayState,
        bool CanPurchase,
        IReadOnlyList<string> ReasonCodes);

    public sealed record OfferCandidateIdentityDto(
        long DefinitionVersionId,
        long ProvisionId,
        EnumValueDto CoverageScope,
        string TravellerRef,
        string CoverageRef,
        long? PricingRevisionId);

    public sealed record OfferServiceDto(
        string ServiceDefinitionRef,
        int DefinitionVersion,
        EnumValueDto Profile,
        string VariantCode,
        string CommercialName,
        string? Description,
        string ServiceTypeCode,
        string ServiceSubCode,
        string? Rfic,
        string? Rfisc);

    public sealed record OfferCoverageDto(
        EnumValueDto Scope,
        IReadOnlyList<string> TravellerRefs,
        IReadOnlyList<string> FlightRefs,
        IReadOnlyList<string> PortionRefs);

    public sealed record OfferEligibilityDto(EnumValueDto Status, IReadOnlyList<string> ReasonCodes, IReadOnlyList<string> MissingContextFields);

    public sealed record OfferQuantityDto(
        EnumValueDto Unit,
        int MinPerSelection,
        int MaxPerSelection,
        IReadOnlyList<OfferCumulativeLimitDto> CumulativeLimits);

    public sealed record OfferCumulativeLimitDto(
        string CountingFamilyCode,
        EnumValueDto LimitScope,
        int MaxUnits,
        EnumValueDto ConsumptionUnit,
        decimal? UnitsPerPurchase,
        bool FamilyVerified,
        decimal? VerifiedConsumed,
        decimal? VerifiedRemaining);

    public sealed record OfferPricingDto(
        EnumValueDto Origin,
        EnumValueDto Status,
        int? CurrencyId,
        int? FiledCurrencyId,
        OfferRateOfExchangeDto? RateOfExchange,
        EnumValueDto? PricingUnit,
        decimal? BaseAmount,
        decimal? CompleteUnitTotal,
        IReadOnlyList<OfferPriceLineDto> AddedTaxes,
        IReadOnlyList<OfferPriceLineDto> IncludedTaxes,
        IReadOnlyList<OfferPriceLineDto> AppliedUnitFees,
        IReadOnlyList<OfferPriceLineDto> UnappliedFees,
        bool IsOrderLevelTotalComplete,
        string? QuoteProviderKey,
        IReadOnlyList<string> ReasonCodes);

    public sealed record OfferRateOfExchangeDto(string RateOfExchangeTableId, int FromCurrencyId, int ToCurrencyId, decimal Rate);

    public sealed record OfferPriceLineDto(
        string? Code,
        decimal Amount,
        int CurrencyId,
        int? CountryId,
        int? StationAirportId,
        EnumValueDto? FeeApplicationUnit);

    public sealed record OfferAvailabilityDto(
        EnumValueDto? Authority,
        EnumValueDto ConfigState,
        EnumValueDto CheckedState,
        bool IsGuaranteed,
        bool RequiresCheckAtReserve,
        OfferConsumptionPreviewDto? ConsumptionPreview,
        IReadOnlyList<string> ReasonCodes);

    public sealed record OfferConsumptionPreviewDto(
        int? CountUnits,
        EnumValueDto? CountUnit,
        decimal? WeightKg,
        int? OccupancyMinutes,
        int? OccupiedPersons);

    public sealed record OfferConfirmationDto(
        EnumValueDto BookingMethod,
        EnumValueDto ConfirmationRequirement,
        EnumValueDto ProviderConfirmationStatus);

    public sealed record OfferDocumentDto(
        EnumValueDto Routing,
        EnumValueDto DocumentType,
        string? Rfic,
        string? Rfisc,
        bool DocumentRequired);
}
