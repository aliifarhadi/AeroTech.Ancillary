using AeroTech.Framework.Infrastructure.HealthChecks;
using AeroTech.Ancillary.Query.AncillaryPricingAggregate.Queries.GetAncillaryPricingById;
using AeroTech.Ancillary.Query.AncillaryPricingAggregate.Queries.GetAncillaryPricingsPaginated;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetAncillaryProvisionById;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetAncillaryProvisionsPaginated;
using AeroTech.Ancillary.Query.AncillaryReservationAggregate.Queries.GetAncillaryHoldById;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionById;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionsPaginated;
using AeroTech.Ancillary.Query.SupplierAggregate.Queries.GetSupplierById;
using AeroTech.Ancillary.Query.SupplierAggregate.Queries.GetSuppliersPaginated;
using AeroTech.Ancillary.Query._Shared.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AeroTech.Ancillary.Query
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddQuery(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("QueryDbContext")
                                   ?? configuration.GetConnectionString("CommandDbContext")
                                   ?? configuration.GetConnectionString("AncillaryDbContext");

            services.AddDbContext<AncillaryQueryDbContext>(options => options.UseSqlServer(
                connectionString,
                sql => sql.MigrationsHistoryTable(AncillaryQueryDbContext.MigrationsHistoryTable, AncillaryQueryDbContext.MigrationsHistorySchema)));

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

            services.AddScoped<IGetSupplierByIdService, GetSupplierByIdService>();
            services.AddScoped<IGetSuppliersPaginatedService, GetSuppliersPaginatedService>();
            services.AddScoped<IGetAncillaryServiceDefinitionByIdService, GetAncillaryServiceDefinitionByIdService>();
            services.AddScoped<IGetAncillaryServiceDefinitionsPaginatedService, GetAncillaryServiceDefinitionsPaginatedService>();
            services.AddScoped<IGetAncillaryProvisionByIdService, GetAncillaryProvisionByIdService>();
            services.AddScoped<IGetAncillaryProvisionsPaginatedService, GetAncillaryProvisionsPaginatedService>();
            services.AddScoped<IGetAncillaryPricingByIdService, GetAncillaryPricingByIdService>();
            services.AddScoped<IGetAncillaryPricingsPaginatedService, GetAncillaryPricingsPaginatedService>();
            services.AddScoped<IGetAncillaryHoldByIdService, GetAncillaryHoldByIdService>();

            services.AddHealthChecks().AddDbContextReadinessCheck<AncillaryQueryDbContext>("sql-server-query");

            return services;
        }
    }
}
