using AeroTech.Ancillary.Domain.AncillaryPricingAggregate;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryPricingAggregate.Services
{
    internal static class PricingPublicationRule
    {
        public static void EnsurePriceable(this AncillaryProvision provision)
        {
            if (provision.Outcome.Disposition != CommercialDisposition.Paid || provision.Status == ProvisionStatus.Retired)
                throw ExceptionFactory.PricingProvisionNotPriceable();
        }

        public static void EnsureSameUnit(this AncillaryPricing pricing, AncillaryServiceDefinition definition)
        {
            if (pricing.PricingUnit != definition.PricingUnit)
                throw ExceptionFactory.PricingUnitMismatch();
        }

        public static void EnsureAllowedBy(this AncillaryProvision provision, AncillaryPricing? activePricing)
        {
            if (provision.Outcome.Disposition == CommercialDisposition.Paid)
            {
                if (activePricing is null)
                    throw ExceptionFactory.ProvisionActivePricingRequired();
            }
            else if (activePricing is not null)
            {
                throw ExceptionFactory.ProvisionActivePricingNotAllowed();
            }
        }
    }
}
