using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Contracts;
using AeroTech.Ancillary.Domain._Shared.Resources;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Services
{
    internal static class ServiceIdentityServiceDateBasisRule
    {
        public static async Task EnsureServiceDateBasisAllowedAsync(
            this IAncillaryServiceDefinitionRepository definitions,
            int ownerAirlineId,
            string serviceDefinitionRef,
            ServiceDateBasis serviceDateBasis,
            CancellationToken cancellationToken)
        {
            var published = (await definitions.ListVersionsAsync(ownerAirlineId, serviceDefinitionRef, cancellationToken))
                .Where(version => version.ActivatedAt is not null)
                .ToList();

            if (published.Any(version => version.ServiceDateBasis is null))
                throw ExceptionFactory.ServiceDefinitionServiceDateBasisNotAssigned();

            if (published.Any(version => version.ServiceDateBasis != serviceDateBasis))
                throw ExceptionFactory.ServiceDefinitionServiceDateBasisConflict(serviceDateBasis);
        }
    }
}
