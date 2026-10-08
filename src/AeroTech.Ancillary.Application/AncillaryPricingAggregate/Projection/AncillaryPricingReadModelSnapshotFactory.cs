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
                pricing.CurrencyId,
                pricing.FeeApplicationUnit,
                pricing.Status,
                pricing.CreatedAt,
                pricing.ActivatedAt,
                pricing.SuspendedAt,
                pricing.RetiredAt,
                pricing.PriceLines
                    .OrderBy(line => line.Id)
                    .Select(line => new AncillaryPricingLineReadModelSnapshot(
                        line.Id,
                        line.PassengerTypeCode,
                        line.AgeFromInclusive,
                        line.AgeToExclusive,
                        line.Category,
                        line.Code,
                        line.Name,
                        line.CountryId,
                        line.StationAirportId,
                        line.Amount))
                    .ToList());
    }
}
