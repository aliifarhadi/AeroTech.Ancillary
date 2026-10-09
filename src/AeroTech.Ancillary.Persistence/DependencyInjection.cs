using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Framework.Infrastructure.HealthChecks;
using AeroTech.Ancillary.Domain.AirportSlotInventoryAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Contracts;
using AeroTech.Ancillary.Domain.FlightCountInventoryAggregate.Contracts;
using AeroTech.Ancillary.Domain.FlightWeightInventoryAggregate.Contracts;
using AeroTech.Ancillary.Domain.SupplierAggregate.Contracts;
using AeroTech.Ancillary.Persistence.AirportSlotInventoryAggregate;
using AeroTech.Ancillary.Persistence.AncillaryInventoryPolicyAggregate;
using AeroTech.Ancillary.Persistence.AncillaryPricingAggregate;
using AeroTech.Ancillary.Persistence.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Persistence.AncillaryReservationAggregate;
using AeroTech.Ancillary.Persistence.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Persistence.FlightCountInventoryAggregate;
using AeroTech.Ancillary.Persistence.FlightWeightInventoryAggregate;
using AeroTech.Ancillary.Persistence.Inbox;
using AeroTech.Ancillary.Persistence.Outbox;
using AeroTech.Ancillary.Persistence.SupplierAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AeroTech.Ancillary.Persistence
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("CommandDbContext")
                                   ?? configuration.GetConnectionString("AncillaryDbContext");

            services.AddDbContext<AncillaryDbContext>(options => options.UseSqlServer(
                connectionString,
                sql => sql.MigrationsHistoryTable(AncillaryDbContext.MigrationsHistoryTable, AncillaryDbContext.MigrationsHistorySchema)));
            services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<AncillaryDbContext>());
            services.AddScoped<ISupplierRepository, SupplierRepository>();
            services.AddScoped<IInventoryPolicyRepository, InventoryPolicyRepository>();
            services.AddScoped<IInventoryCommercialFactsReader, InventoryCommercialFactsReader>();
            services.AddScoped<IFlightCountInventoryRepository, FlightCountInventoryRepository>();
            services.AddScoped<IFlightWeightInventoryRepository, FlightWeightInventoryRepository>();
            services.AddScoped<IAirportSlotInventoryRepository, AirportSlotInventoryRepository>();
            services.AddScoped<IAncillaryServiceDefinitionRepository, AncillaryServiceDefinitionRepository>();
            services.AddScoped<IAncillaryProvisionRepository, AncillaryProvisionRepository>();
            services.AddScoped<IAncillaryPricingRepository, AncillaryPricingRepository>();
            services.AddScoped<IAncillaryReservationRepository, AncillaryReservationRepository>();
            services.Configure<IntegrationEventOptions>(configuration.GetSection("IntegrationEvents"));
            services.AddScoped<IOutboxWriter, OutboxWriter>();
            services.AddScoped<IInboxStore, InboxStore>();

            services.AddHealthChecks().AddDbContextReadinessCheck<AncillaryDbContext>("sql-server-command");

            return services;
        }
    }
}
