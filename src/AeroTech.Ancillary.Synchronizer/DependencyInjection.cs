using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Ancillary.Synchronizer._Shared;
using Microsoft.Extensions.DependencyInjection;

namespace AeroTech.Ancillary.Synchronizer
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddSynchronizer(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, AncillaryUnitOfWork>();

            return services;
        }
    }
}
