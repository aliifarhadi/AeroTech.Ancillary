using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Services
{
    internal static class ServiceIdentityPricingUnitRule
    {
        public static async Task EnsurePricingUnitAllowedAsync(
            this IAncillaryServiceDefinitionRepository definitions,
            int ownerAirlineId,
            string serviceDefinitionRef,
            PricingUnit pricingUnit,
            CancellationToken cancellationToken)
        {
            var published = (await definitions.ListVersionsAsync(ownerAirlineId, serviceDefinitionRef, cancellationToken))
                .Where(version => version.ActivatedAt is not null)
                .ToList();

            if (published.Any(version => version.PricingUnit is null))
                throw ExceptionFactory.ServiceDefinitionPricingUnitNotAssigned();

            if (published.Any(version => version.PricingUnit != pricingUnit))
                throw ExceptionFactory.ServiceDefinitionPricingUnitConflict(pricingUnit);
        }
    }
}
