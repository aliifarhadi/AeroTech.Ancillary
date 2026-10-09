using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.AncillaryPricingAggregate.Dto
{
    public sealed record BackofficePricingDto(
        long Id,
        long AncillaryProvisionId,
        int Version,
        EnumValueDto? PricingUnit,
        EnumValueDto Status,
        DateTimeOffset CreatedAt,
        DateTimeOffset? ActivatedAt,
        DateTimeOffset? SuspendedAt,
        DateTimeOffset? RetiredAt,
        IReadOnlyList<string> Currencies,
        IReadOnlyList<BackofficePricingRateDto> Rates);

    public sealed record MoneyDto(decimal Amount, int CurrencyId, string? Currency);

    public sealed record BackofficePricingRateDto(
        long Id,
        EnumValueDto? PassengerTypeCode,
        int? AgeFromInclusive,
        int? AgeToExclusive,
        MoneyDto BasePrice,
        IReadOnlyList<BackofficePriceComponentDto> Components,
        MoneyDto UnitTotal,
        bool IsUnitTotalComplete,
        IReadOnlyList<BackofficePriceComponentDto> UnappliedFees);

    public sealed record BackofficePriceComponentDto(
        long Id,
        EnumValueDto Category,
        string? Code,
        string? Name,
        int? CountryId,
        int? StationAirportId,
        MoneyDto Amount,
        EnumValueDto? FeeApplicationUnit,
        bool? TaxIncludedInSource);
}
