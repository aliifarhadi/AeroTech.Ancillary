using AeroTech.Ancillary.Shopping.Engine;
using AeroTech.Ancillary.Shopping.Offers;
using AeroTech.Ancillary.Shopping.Options;
using AeroTech.Ancillary.Shopping.Reading;
using AeroTech.Ancillary.Shopping.Reading.Sql;
using AeroTech.Ancillary.Shopping.Resolution;
using AeroTech.Ancillary.Shopping.SeatMaps;
using AeroTech.Ancillary.Shopping.Selections;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AeroTech.Ancillary.Shopping
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddAncillaryShopping(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IActiveAncillaryDefinitionReader, SqlActiveAncillaryDefinitionReader>();
            services.AddScoped<IActiveAncillaryProvisionReader, SqlActiveAncillaryProvisionReader>();
            services.AddScoped<IActiveAncillaryPricingReader, SqlActiveAncillaryPricingReader>();
            services.AddScoped<IInventoryConfigurationReader, SqlInventoryConfigurationReader>();
            services.AddScoped<IAncillaryShoppingEngine, AncillaryShoppingEngine>();
            services.AddScoped<ISeatMapPricingEngine, AncillaryShoppingEngine>();
            services.AddSingleton(SelectionIdOptions.From(configuration));
            services.AddScoped<ISelectionIdCodec, SelectionIdCodec>();
            services.AddScoped<ISelectionSigner, SelectionSigner>();
            services.AddScoped<IAncillaryOfferListing, AncillaryOfferListing>();
            services.AddScoped<ISelectionProofService, SelectionProofService>();
            services.AddScoped<ISelectionSnapshotReader, SqlSelectionSnapshotReader>();
            services.AddScoped<ISelectionResolver, SelectionResolver>();

            return services;
        }
    }
}
