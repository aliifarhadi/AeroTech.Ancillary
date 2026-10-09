using AeroTech.Ancillary.Domain.AncillaryPricingAggregate;
using AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Contracts;

namespace AeroTech.Ancillary.Application.AncillaryPricingAggregate.Projection
{
    internal static class AncillaryPricingReadModelSnapshotFactory
    {
        public static AncillaryPricingReadModelSnapshot ToReadModelSnapshot(this AncillaryPricing pricing)
            => new(
                pricing.Id,
                pricing.AncillaryProvisionId,
                pricing.PricingUnit,
                pricing.Version,
                pricing.Status,
                pricing.CreatedAt,
                pricing.ActivatedAt,
                pricing.SuspendedAt,
                pricing.RetiredAt,
                pricing.Rates
                    .OrderBy(rate => rate.Id)
                    .Select(rate => new AncillaryPricingRateReadModelSnapshot(
                        rate.Id,
                        rate.PassengerTypeCode,
                        rate.AgeFromInclusive,
                        rate.AgeToExclusive,
                        rate.CurrencyId,
                        rate.BaseAmount,
                        rate.Components
                            .OrderBy(component => component.Id)
                            .Select(component => new AncillaryPriceComponentReadModelSnapshot(
                                component.Id,
                                component.Category,
                                component.Code,
                                component.Name,
                                component.CountryId,
                                component.StationAirportId,
                                component.Amount.Amount,
                                component.Amount.CurrencyId,
                                component.FeeApplicationUnit,
                                component.TaxIncludedInSource,
                                component.TaxTreatment))
                            .ToList()))
                    .ToList());
    }
}
