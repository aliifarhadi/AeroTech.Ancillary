using AeroTech.Ancillary.Application.FlightOffers;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Providers.FlightFlow.Services;
using AeroTech.Ancillary.Providers.InventoryReferences;
using AeroTech.Ancillary.Providers.Offer.Services;
using AeroTech.Ancillary.Providers.SeatMap;
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

            var offerBaseUrl = configuration["Offer:BaseUrl"];
            var offerRequestTimeout = configuration["Offer:RequestTimeout"];

            services.AddHttpClient<IFlightOfferProvider, FlightOfferProvider>(client =>
            {
                if (!string.IsNullOrWhiteSpace(offerBaseUrl))
                    client.BaseAddress = new Uri(offerBaseUrl);

                if (int.TryParse(offerRequestTimeout, out var seconds) && seconds > 0)
                    client.Timeout = TimeSpan.FromSeconds(seconds);
            });

            services.AddScoped<IFlightSeatMapReference, NotConnectedFlightSeatMapReference>();
            services.AddScoped<IInventoryResourceReference, NotConnectedInventoryResourceReference>();
            services.AddScoped<IAirportFacilityReference, NotConnectedAirportFacilityReference>();
            services.AddScoped<IFlightFlowDelegationReference, NotConnectedFlightFlowDelegationReference>();
            services.AddScoped<ICountingFamilyReference, NotConnectedCountingFamilyReference>();

            return services;
        }
    }
}
