using AeroTech.Ancillary.Query.AncillaryPricingAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryPricingAggregate.Models;
using AeroTech.Ancillary.Query._Shared.Enums;
using AeroTech.Messages.Ancillary.Enums;
using System.Globalization;

namespace AeroTech.Ancillary.Query.AncillaryPricingAggregate.Queries.GetAncillaryPricingById
{
    public static class AncillaryPricingMapper
    {
        public static BackofficePricingDto ToBackofficePricing(
            AncillaryPricingReadModel pricing,
            IReadOnlyList<AncillaryPricingLineReadModel> priceLines,
            string? currency)
            => new(
                pricing.Id,
                pricing.AncillaryProvisionId,
                pricing.Version,
                EnumValueDto.OfNullable(pricing.PricingUnit),
                pricing.CurrencyId,
                currency,
                EnumValueDto.OfNullable(pricing.FeeApplicationUnit),
                EnumValueDto.Of(pricing.Status),
                pricing.CreatedAt,
                pricing.ActivatedAt,
                pricing.SuspendedAt,
                pricing.RetiredAt,
                priceLines
                    .OrderBy(line => line.Id)
                    .Select(line => new BackofficePricingLineDto(
                        line.Id,
                        EnumValueDto.OfNullable(line.PassengerTypeCode),
                        line.AgeFromInclusive,
                        line.AgeToExclusive,
                        EnumValueDto.Of(line.Category),
                        line.Code,
                        line.Name,
                        line.CountryId,
                        line.StationAirportId,
                        line.Amount))
                    .ToList(),
                priceLines
                    .GroupBy(line => (line.PassengerTypeCode, line.AgeFromInclusive, line.AgeToExclusive))
                    .OrderBy(rate => rate.Key.PassengerTypeCode)
                    .ThenBy(rate => rate.Key.AgeFromInclusive)
                    .Select(rate => new BackofficePricingRateDto(
                        EnumValueDto.OfNullable(rate.Key.PassengerTypeCode),
                        rate.Key.AgeFromInclusive,
                        rate.Key.AgeToExclusive,
                        rate.Where(line => line.Category == AncillaryPriceLineCategory.Ancillary).Sum(line => line.Amount),
                        rate.Where(line => line.Category == AncillaryPriceLineCategory.Tax).Sum(line => line.Amount),
                        rate.Where(line => line.Category == AncillaryPriceLineCategory.Fee).Sum(line => line.Amount),
                        rate.Sum(line => line.Amount)))
                    .ToList());

        public static PricingPaginatedRowDto ToPaginatedRow(AncillaryPricingReadModel pricing, int rateCount, string? currency)
            => new()
            {
                Id = pricing.Id.ToString(CultureInfo.InvariantCulture),
                AncillaryProvisionId = pricing.AncillaryProvisionId.ToString(CultureInfo.InvariantCulture),
                Version = pricing.Version,
                PricingUnit = EnumValueDto.OfNullable(pricing.PricingUnit),
                Currency = currency,
                FeeApplicationUnit = EnumValueDto.OfNullable(pricing.FeeApplicationUnit),
                RateCount = rateCount,
                Status = EnumValueDto.Of(pricing.Status),
                CreatedAt = pricing.CreatedAt,
                ActivatedAt = pricing.ActivatedAt
            };
    }
}
