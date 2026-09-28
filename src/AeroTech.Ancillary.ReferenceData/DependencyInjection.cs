using AeroTech.Ancillary.ReferenceData.AirInfo;
using AeroTech.Ancillary.ReferenceData.Configuration;
using AeroTech.Ancillary.ReferenceData.Core;
using AeroTech.Ancillary.ReferenceData.Persistence;
using AeroTech.Ancillary.ReferenceData.Syncing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AeroTech.Ancillary.ReferenceData
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddReferenceData(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ReferenceDataOptions>(configuration.GetSection(ReferenceDataOptions.SectionName));

            var connectionString = configuration.GetConnectionString("QueryDbContext")
                                   ?? configuration.GetConnectionString("CommandDbContext")
                                   ?? configuration.GetConnectionString("AncillaryDbContext");

            services.AddDbContext<ReferenceDbContext>(options => options.UseSqlServer(
                connectionString,
                sql => sql.MigrationsHistoryTable(ReferenceDbContext.MigrationsHistoryTable, ReferenceDbContext.MigrationsHistorySchema)));

            var airInfoBaseUrl = configuration["AirInfo:BaseUrl"];
            services.AddHttpClient<IAirInfoClient, AirInfoClient>(client =>
            {
                if (!string.IsNullOrWhiteSpace(airInfoBaseUrl))
                    client.BaseAddress = new Uri(airInfoBaseUrl);
            });

            var coreBaseUrl = configuration["AeroCore:BaseUrl"];
            services.AddHttpClient<ICoreClient, CoreClient>(client =>
            {
                if (!string.IsNullOrWhiteSpace(coreBaseUrl))
                    client.BaseAddress = new Uri(coreBaseUrl);
            });

            services.TryAddSingleton(TimeProvider.System);

            services.AddScoped<CurrencySyncer>();
            services.AddScoped<AirlineSyncer>();
            services.AddScoped<CitySyncer>();
            services.AddScoped<AirportSyncer>();
            services.AddScoped<CountrySyncer>();
            services.AddScoped<CustomerSyncer>();
            services.AddScoped<OperatorSettingsSyncer>();
            services.AddScoped<AirlineOfficeSyncer>();
            services.AddScoped<TravelAgencySyncer>();
            services.AddScoped<TravelAgencyOfficeSyncer>();

            return services;
        }
    }
}
