using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Services
{
    internal static class ServiceIdentityProfileRule
    {
        public static async Task EnsureProfileAllowedAsync(
            this IAncillaryServiceDefinitionRepository definitions,
            int ownerAirlineId,
            string serviceDefinitionRef,
            AncillaryProfile profile,
            string variantCode,
            CancellationToken cancellationToken)
        {
            var published = (await definitions.ListVersionsAsync(ownerAirlineId, serviceDefinitionRef, cancellationToken))
                .Where(version => version.ActivatedAt is not null && version.IsClassified);

            if (published.Any(version => version.Profile != profile || version.VariantCode != variantCode))
                throw ExceptionFactory.ServiceDefinitionProfileIsImmutable();
        }
    }
}
