using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Providers.FlightFlow.Services;
using AeroTech.Ancillary.Providers.InventoryReferences;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AeroTech.Ancillary.Providers
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddProviders(this IServiceCollection services, IConfiguration configuration)
        {
            var flightFlowBaseUrl = configuration["FlightFlow:BaseUrl"];

            services.AddHttpClient<IFlightOccurrenceReference, FlightFlowFlightOccurrenceReference>(client =>
            {
                if (!string.IsNullOrWhiteSpace(flightFlowBaseUrl))
                    client.BaseAddress = new Uri(flightFlowBaseUrl);
            });

            services.AddScoped<IInventoryResourceReference, NotConnectedInventoryResourceReference>();
            services.AddScoped<IAirportFacilityReference, NotConnectedAirportFacilityReference>();
            services.AddScoped<IFlightFlowDelegationReference, NotConnectedFlightFlowDelegationReference>();
            services.AddScoped<ICountingFamilyReference, NotConnectedCountingFamilyReference>();

            return services;
        }
    }
}
