using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Framework.Infrastructure.HealthChecks;
using AeroTech.Ancillary.Domain.AncillaryPriceRuleAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryProductAggregate.Contracts;
using AeroTech.Ancillary.Domain.ServiceSubCodeAggregate.Contracts;
using AeroTech.Ancillary.Persistence.AncillaryPriceRuleAggregate;
using AeroTech.Ancillary.Persistence.AncillaryProductAggregate;
using AeroTech.Ancillary.Persistence.Inbox;
using AeroTech.Ancillary.Persistence.Outbox;
using AeroTech.Ancillary.Persistence.ServiceSubCodeAggregate;
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
            services.AddScoped<IServiceSubCodeRepository, ServiceSubCodeRepository>();
            services.AddScoped<IAncillaryProductRepository, AncillaryProductRepository>();
            services.AddScoped<IAncillaryPriceRuleRepository, AncillaryPriceRuleRepository>();
            services.Configure<IntegrationEventOptions>(configuration.GetSection("IntegrationEvents"));
            services.AddScoped<IOutboxWriter, OutboxWriter>();
            services.AddScoped<IInboxStore, InboxStore>();

            services.AddHealthChecks().AddDbContextReadinessCheck<AncillaryDbContext>("sql-server-command");

            return services;
        }
    }
}
