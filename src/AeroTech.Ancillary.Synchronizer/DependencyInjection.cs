using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Ancillary.Domain.AncillaryPriceRuleAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryProductAggregate.Contracts;
using AeroTech.Ancillary.Domain.ServiceSubCodeAggregate.Contracts;
using AeroTech.Ancillary.Synchronizer.AncillaryPriceRuleAggregate;
using AeroTech.Ancillary.Synchronizer.AncillaryProductAggregate;
using AeroTech.Ancillary.Synchronizer.ServiceSubCodeAggregate;
using AeroTech.Ancillary.Synchronizer._Shared;
using Microsoft.Extensions.DependencyInjection;

namespace AeroTech.Ancillary.Synchronizer
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddSynchronizer(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, AncillaryUnitOfWork>();
            services.AddScoped<IServiceSubCodeQueryDbSynchronizer, ServiceSubCodeQueryDbSynchronizer>();
            services.AddScoped<IAncillaryProductQueryDbSynchronizer, AncillaryProductQueryDbSynchronizer>();
            services.AddScoped<IAncillaryPriceRuleQueryDbSynchronizer, AncillaryPriceRuleQueryDbSynchronizer>();

            return services;
        }
    }
}
