using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Providers.InventoryReferences;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AeroTech.Ancillary.Providers
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddProviders(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IFlightOccurrenceReference, NotConnectedFlightOccurrenceReference>();
            services.AddScoped<IInventoryResourceReference, NotConnectedInventoryResourceReference>();
            services.AddScoped<IAirportFacilityReference, NotConnectedAirportFacilityReference>();
            services.AddScoped<IFlightFlowDelegationReference, NotConnectedFlightFlowDelegationReference>();
            services.AddScoped<ICountingFamilyReference, NotConnectedCountingFamilyReference>();

            return services;
        }
    }
}
