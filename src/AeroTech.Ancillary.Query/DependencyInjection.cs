using AeroTech.Framework.Infrastructure.HealthChecks;
using AeroTech.Ancillary.Query.AncillaryPriceRuleAggregate.Queries.GetAncillaryPriceRuleById;
using AeroTech.Ancillary.Query.AncillaryPriceRuleAggregate.Queries.GetAncillaryPriceRulesPaginated;
using AeroTech.Ancillary.Query.AncillaryProductAggregate.Queries.GetAncillaryProductById;
using AeroTech.Ancillary.Query.AncillaryProductAggregate.Queries.GetAncillaryProductsPaginated;
using AeroTech.Ancillary.Query.AncillaryQuote.Queries.GetAncillaryQuote;
using AeroTech.Ancillary.Query.ServiceSubCodeAggregate.Queries.GetServiceSubCodeById;
using AeroTech.Ancillary.Query.ServiceSubCodeAggregate.Queries.GetServiceSubCodesPaginated;
using AeroTech.Ancillary.Query._Shared.DbContexts;
using FluentValidation;
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
            services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

            services.AddScoped<IGetServiceSubCodeByIdService, GetServiceSubCodeByIdService>();
            services.AddScoped<IGetServiceSubCodesPaginatedService, GetServiceSubCodesPaginatedService>();
            services.AddScoped<IGetAncillaryProductByIdService, GetAncillaryProductByIdService>();
            services.AddScoped<IGetAncillaryProductsPaginatedService, GetAncillaryProductsPaginatedService>();
            services.AddScoped<IGetAncillaryPriceRuleByIdService, GetAncillaryPriceRuleByIdService>();
            services.AddScoped<IGetAncillaryPriceRulesPaginatedService, GetAncillaryPriceRulesPaginatedService>();
            services.AddScoped<IGetAncillaryQuoteService, GetAncillaryQuoteService>();

            services.AddHealthChecks().AddDbContextReadinessCheck<AncillaryQueryDbContext>("sql-server-query");

            return services;
        }
    }
}
