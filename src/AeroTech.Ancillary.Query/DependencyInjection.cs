using AeroTech.Framework.Infrastructure.HealthChecks;
using AeroTech.Ancillary.Query.AirportSlotInventoryAggregate.Queries.GetAirportSlotInventoriesPaginated;
using AeroTech.Ancillary.Query.AirportSlotInventoryAggregate.Queries.GetAirportSlotInventoryById;
using AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Queries.GetInventoryConfigurationSnapshot;
using AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Queries.GetInventoryPoliciesPaginated;
using AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Queries.GetInventoryPolicyById;
using AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Queries.GetInventoryPolicyByServiceIdentity;
using AeroTech.Ancillary.Query.AncillaryPricingAggregate.Queries.GetAncillaryPricingById;
using AeroTech.Ancillary.Query.AncillaryPricingAggregate.Queries.GetAncillaryPricingsPaginated;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetAncillaryProvisionById;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetAncillaryProvisionsPaginated;
using AeroTech.Ancillary.Query.AncillaryReservationAggregate.Queries.GetAncillaryHoldById;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionById;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionsPaginated;
using AeroTech.Ancillary.Query.FlightCountInventoryAggregate.Queries.GetFlightCountInventoriesPaginated;
using AeroTech.Ancillary.Query.FlightCountInventoryAggregate.Queries.GetFlightCountInventoryById;
using AeroTech.Ancillary.Query.FlightWeightInventoryAggregate.Queries.GetFlightWeightInventoriesPaginated;
using AeroTech.Ancillary.Query.FlightWeightInventoryAggregate.Queries.GetFlightWeightInventoryById;
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
            services.AddScoped<IGetInventoryPolicyByIdService, GetInventoryPolicyByIdService>();
            services.AddScoped<IGetInventoryPolicyByServiceIdentityService, GetInventoryPolicyByServiceIdentityService>();
            services.AddScoped<IGetInventoryPoliciesPaginatedService, GetInventoryPoliciesPaginatedService>();
            services.AddScoped<IGetInventoryConfigurationSnapshotService, GetInventoryConfigurationSnapshotService>();
            services.AddScoped<IGetFlightCountInventoryByIdService, GetFlightCountInventoryByIdService>();
            services.AddScoped<IGetFlightCountInventoriesPaginatedService, GetFlightCountInventoriesPaginatedService>();
            services.AddScoped<IGetFlightWeightInventoryByIdService, GetFlightWeightInventoryByIdService>();
            services.AddScoped<IGetFlightWeightInventoriesPaginatedService, GetFlightWeightInventoriesPaginatedService>();
            services.AddScoped<IGetAirportSlotInventoryByIdService, GetAirportSlotInventoryByIdService>();
            services.AddScoped<IGetAirportSlotInventoriesPaginatedService, GetAirportSlotInventoriesPaginatedService>();

            services.AddHealthChecks().AddDbContextReadinessCheck<AncillaryQueryDbContext>("sql-server-query");

            return services;
        }
    }
}
