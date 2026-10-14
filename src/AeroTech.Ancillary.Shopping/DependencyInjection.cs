using AeroTech.Ancillary.Shopping.Engine;
using AeroTech.Ancillary.Shopping.Reading;
using AeroTech.Ancillary.Shopping.Reading.Sql;
using AeroTech.Ancillary.Shopping.SeatMaps;
using Microsoft.Extensions.DependencyInjection;

namespace AeroTech.Ancillary.Shopping
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddAncillaryShopping(this IServiceCollection services)
        {
            services.AddScoped<IActiveAncillaryDefinitionReader, SqlActiveAncillaryDefinitionReader>();
            services.AddScoped<IActiveAncillaryProvisionReader, SqlActiveAncillaryProvisionReader>();
            services.AddScoped<IActiveAncillaryPricingReader, SqlActiveAncillaryPricingReader>();
            services.AddScoped<IInventoryConfigurationReader, SqlInventoryConfigurationReader>();
            services.AddScoped<IAncillaryShoppingEngine, AncillaryShoppingEngine>();
            services.AddScoped<ISeatMapPricingEngine, AncillaryShoppingEngine>();

            return services;
        }
    }
}
