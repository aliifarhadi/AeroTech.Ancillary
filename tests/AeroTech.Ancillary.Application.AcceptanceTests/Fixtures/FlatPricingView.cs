using AeroTech.Ancillary.Query.AncillaryPricingAggregate.Dto;
using AeroTech.Ancillary.Query._Shared.Enums;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;

public sealed record FlatPriceLine(
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

public static class FlatPricingView
{
    extension(BackofficePricingRateDto rate)
    {
        public decimal BaseAmount => rate.BasePrice.Amount;

        public decimal TaxAmount => rate.Components.Where(component => component.Category.Name == nameof(AncillaryPriceLineCategory.Tax)).Sum(component => component.Amount.Amount);

        public decimal FeeAmount => rate.Components.Where(component => component.Category.Name == nameof(AncillaryPriceLineCategory.Fee)).Sum(component => component.Amount.Amount);

        public decimal TotalAmount => rate.UnitTotal.Amount;
    }

    extension(BackofficePricingDto pricing)
    {
        public int CurrencyId => pricing.Rates.Select(rate => rate.BasePrice.CurrencyId).Distinct().Single();

        public string? Currency => pricing.Currencies.Single();

        public IReadOnlyList<FlatPriceLine> PriceLines => pricing.Rates
            .SelectMany(rate => rate.Components
                .Select(component => new FlatPriceLine(
                    component.Id,
                    rate.PassengerTypeCode,
                    rate.AgeFromInclusive,
                    rate.AgeToExclusive,
                    component.Category,
                    component.Code,
                    component.Name,
                    component.CountryId,
                    component.StationAirportId,
                    component.Amount.Amount))
                .Prepend(new FlatPriceLine(
                    rate.Id,
                    rate.PassengerTypeCode,
                    rate.AgeFromInclusive,
                    rate.AgeToExclusive,
                    EnumValueDto.Of(AncillaryPriceLineCategory.Ancillary),
                    null,
                    null,
                    null,
                    null,
                    rate.BasePrice.Amount)))
            .OrderBy(line => line.Id)
            .ToList();
    }

    extension(PricingPaginatedRowDto row)
    {
        public string Currency => row.Currencies;
    }
}
