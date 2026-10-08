using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.AncillaryPricingAggregate.Dto
{
    public sealed record BackofficePricingDto(
        long Id,
        long AncillaryProvisionId,
        int Version,
        EnumValueDto? PricingUnit,
        int CurrencyId,
        string? Currency,
        EnumValueDto? FeeApplicationUnit,
        EnumValueDto Status,
        DateTimeOffset CreatedAt,
        DateTimeOffset? ActivatedAt,
        DateTimeOffset? SuspendedAt,
        DateTimeOffset? RetiredAt,
        IReadOnlyList<BackofficePricingLineDto> PriceLines,
        IReadOnlyList<BackofficePricingRateDto> Rates);

    public sealed record BackofficePricingLineDto(
        long Id,
        EnumValueDto? PassengerTypeCode,
        int? AgeFromInclusive,
        int? AgeToExclusive,
        EnumValueDto Category,
        string? Code,
        string? Name,
        int? CountryId,
        int? StationAirportId,
        decimal Amount);

    public sealed record BackofficePricingRateDto(
        EnumValueDto? PassengerTypeCode,
        int? AgeFromInclusive,
        int? AgeToExclusive,
        decimal BaseAmount,
        decimal TaxAmount,
        decimal FeeAmount,
        decimal TotalAmount);
}
