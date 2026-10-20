using AeroTech.Ancillary.Application.FlightOffers;
using AeroTech.Ancillary.Application.Orders;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Providers.AirInfo.Services;
using AeroTech.Ancillary.Providers.FlightFlow.Services;
using AeroTech.Ancillary.Providers.InventoryReferences;
using AeroTech.Ancillary.Providers.Offer.Services;
using AeroTech.Ancillary.Providers.Orders;
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

            services.AddHttpClient<IOccupiedSeatProvider, FlightFlowOccupiedSeatProvider>(client =>
            {
                if (!string.IsNullOrWhiteSpace(flightFlowBaseUrl))
                    client.BaseAddress = new Uri(flightFlowBaseUrl);
            });

            var airInfoBaseUrl = configuration["AirInfo:BaseUrl"];

            services.AddHttpClient<IAircraftSeatMapProvider, AirInfoAircraftSeatMapProvider>(client =>
            {
                if (!string.IsNullOrWhiteSpace(airInfoBaseUrl))
                    client.BaseAddress = new Uri(airInfoBaseUrl);
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

            services.AddScoped<IInventoryResourceReference, NotConnectedInventoryResourceReference>();
            services.AddScoped<IAirportFacilityReference, NotConnectedAirportFacilityReference>();
            services.AddScoped<ICountingFamilyReference, NotConnectedCountingFamilyReference>();
            services.AddScoped<IOrderCommercialContextReader, NotConnectedOrderCommercialContextReader>();

            return services;
        }
    }
}
