using AeroTech.Ancillary.Domain.AncillaryPricingAggregate;

namespace AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing
{
    internal static class PricingResultFactory
    {
        public static PricingResult ToResult(this AncillaryPricing pricing)
            => new(
                pricing.Id,
                pricing.AncillaryProvisionId,
                pricing.Version,
                pricing.PricingUnit,
                pricing.CurrencyId,
                pricing.Status);
    }
}
