using System.Globalization;
using AeroTech.Ancillary.Query.AncillaryPricingAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryPricingAggregate.Models;
using AeroTech.Ancillary.Query._Shared.Enums;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.AncillaryPricingAggregate.Queries.GetAncillaryPricingById
{
    public static class AncillaryPricingMapper
    {
        public static BackofficePricingDto ToBackofficePricing(
            AncillaryPricingReadModel pricing,
            IReadOnlyList<AncillaryPricingRateReadModel> rates,
            IReadOnlyList<AncillaryPriceComponentReadModel> components,
            IReadOnlyDictionary<int, string> currencies)
            => new(
                pricing.Id,
                pricing.AncillaryProvisionId,
                pricing.Version,
                EnumValueDto.OfNullable(pricing.PricingUnit),
                EnumValueDto.Of(pricing.Status),
                pricing.CreatedAt,
                pricing.ActivatedAt,
                pricing.SuspendedAt,
                pricing.RetiredAt,
                CurrencyCodes(rates, currencies),
                rates
                    .OrderBy(rate => rate.CurrencyId)
                    .ThenBy(rate => rate.PassengerTypeCode)
                    .ThenBy(rate => rate.AgeFromInclusive)
                    .Select(rate => ToRate(rate, components.Where(component => component.AncillaryPricingRateId == rate.Id).OrderBy(component => component.Id).ToList(), currencies))
                    .ToList());

        public static PricingPaginatedRowDto ToPaginatedRow(
            AncillaryPricingReadModel pricing,
            IReadOnlyList<AncillaryPricingRateReadModel> rates,
            IReadOnlyDictionary<int, string> currencies)
            => new()
            {
                Id = pricing.Id.ToString(CultureInfo.InvariantCulture),
                AncillaryProvisionId = pricing.AncillaryProvisionId.ToString(CultureInfo.InvariantCulture),
                Version = pricing.Version,
                PricingUnit = EnumValueDto.OfNullable(pricing.PricingUnit),
                Currencies = string.Join(", ", CurrencyCodes(rates, currencies)),
                RateCount = rates.Count,
                Status = EnumValueDto.Of(pricing.Status),
                CreatedAt = pricing.CreatedAt,
                ActivatedAt = pricing.ActivatedAt
            };

        private static BackofficePricingRateDto ToRate(
            AncillaryPricingRateReadModel rate,
            IReadOnlyList<AncillaryPriceComponentReadModel> components,
            IReadOnlyDictionary<int, string> currencies)
        {
            var unapplied = components.Where(IsUnapplied).ToList();

            return new BackofficePricingRateDto(
                rate.Id,
                EnumValueDto.OfNullable(rate.PassengerTypeCode),
                rate.AgeFromInclusive,
                rate.AgeToExclusive,
                ToMoney(rate.BaseAmount, rate.CurrencyId, currencies),
                components.Select(component => ToComponent(component, currencies)).ToList(),
                ToMoney(rate.BaseAmount + components.Where(component => !IsUnapplied(component) && !IsIncluded(component)).Sum(component => component.Amount), rate.CurrencyId, currencies),
                ToMoney(components.Where(IsIncluded).Sum(component => component.Amount), rate.CurrencyId, currencies),
                unapplied.Count == 0 && !components.Any(HasUnknownTreatment),
                unapplied.Select(component => ToComponent(component, currencies)).ToList());
        }

        private static BackofficePriceComponentDto ToComponent(AncillaryPriceComponentReadModel component, IReadOnlyDictionary<int, string> currencies)
            => new(
                component.Id,
                EnumValueDto.Of(component.Category),
                component.Code,
                component.Name,
                component.CountryId,
                component.StationAirportId,
                ToMoney(component.Amount, component.CurrencyId, currencies),
                EnumValueDto.OfNullable(component.FeeApplicationUnit),
                component.TaxIncludedInSource,
                EnumValueDto.OfNullable(component.TaxTreatment));

        private static bool IsUnapplied(AncillaryPriceComponentReadModel component)
            => component.Category == AncillaryPriceLineCategory.Fee && component.FeeApplicationUnit != FeeApplicationUnit.Item;

        private static bool IsIncluded(AncillaryPriceComponentReadModel component)
            => component.TaxTreatment == TaxTreatment.IncludedInBase;

        private static bool HasUnknownTreatment(AncillaryPriceComponentReadModel component)
            => component.Category == AncillaryPriceLineCategory.Tax && !IsIncluded(component) && component.TaxTreatment != TaxTreatment.AddedToBase;

        private static MoneyDto ToMoney(decimal amount, int currencyId, IReadOnlyDictionary<int, string> currencies)
            => new(amount, currencyId, currencies.GetValueOrDefault(currencyId));

        private static IReadOnlyList<string> CurrencyCodes(IReadOnlyList<AncillaryPricingRateReadModel> rates, IReadOnlyDictionary<int, string> currencies)
            => rates
                .Select(rate => rate.CurrencyId)
                .Distinct()
                .OrderBy(currencyId => currencyId)
                .Select(currencyId => currencies.GetValueOrDefault(currencyId) ?? currencyId.ToString(CultureInfo.InvariantCulture))
                .ToList();
    }
}
