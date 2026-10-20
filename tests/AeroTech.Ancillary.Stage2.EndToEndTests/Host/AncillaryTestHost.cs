using AeroTech.Ancillary.Application.FlightOffers;
using AeroTech.Ancillary.Application.Orders;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Persistence;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers.Api;
using AeroTech.Ancillary.Query.Shopping.Common;
using AeroTech.Ancillary.Query._Shared.DbContexts;
using AeroTech.Ancillary.ReferenceData.Persistence;
using AeroTech.Ancillary.Shopping.Reading;
using AeroTech.Ancillary.Shopping.Tests.Fixtures;
using AeroTech.Ancillary.Stage2.EndToEndTests.Ordering;
using AeroTech.Framework.Core.ServiceContracts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;

namespace AeroTech.Ancillary.Stage2.EndToEndTests.Host;

public sealed class AncillaryTestHost(Stage2Ports ports) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Stage2Settings.Environment);

        foreach (var (key, value) in Stage2Settings.Values)
            builder.UseSetting(key, value);

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IHostedService>();

            services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = E2EAuthenticationHandler.SchemeName;
                    options.DefaultAuthenticateScheme = E2EAuthenticationHandler.SchemeName;
                    options.DefaultChallengeScheme = E2EAuthenticationHandler.SchemeName;
                })
                .AddScheme<AuthenticationSchemeOptions, E2EAuthenticationHandler>(E2EAuthenticationHandler.SchemeName, null);
            services.PostConfigure<JwtBearerOptions>(
                JwtBearerDefaults.AuthenticationScheme,
                options => options.BackchannelHttpHandler = new BlockedNetworkHandler(ports.Guard, "JwtBearer"));

            services.ConfigureAll<HttpClientFactoryOptions>(options =>
                options.HttpMessageHandlerBuilderActions.Add(handler => handler.PrimaryHandler = new BlockedNetworkHandler(ports.Guard, handler.Name ?? "unnamed")));
            services.Configure<HttpClientFactoryOptions>(nameof(IFlightOfferProvider), options =>
                options.HttpMessageHandlerBuilderActions.Add(handler => handler.PrimaryHandler = new AirAvailHandler(ports.AirAvail)));

            services.ConfigureDbContext<AncillaryDbContext>(options => options.AddInterceptors(new BlockedSqlInterceptor(ports.Guard, nameof(AncillaryDbContext))));
            services.ConfigureDbContext<AncillaryQueryDbContext>(options => options.AddInterceptors(new BlockedSqlInterceptor(ports.Guard, nameof(AncillaryQueryDbContext))));
            services.ConfigureDbContext<ReferenceDbContext>(options => options.AddInterceptors(new BlockedSqlInterceptor(ports.Guard, nameof(ReferenceDbContext))));

            services.Replace(ServiceDescriptor.Singleton<IClock>(ports.Clock));
            services.Replace(ServiceDescriptor.Singleton<IOrderCommercialContextReader>(new TestOrderCommercialContextReader(ports.Orders)));
            services.Replace(ServiceDescriptor.Singleton<IAircraftSeatMapProvider>(ports.SeatMaps));
            services.Replace(ServiceDescriptor.Singleton<IOccupiedSeatProvider>(ports.Occupancy));

            var catalog = ports.Lab.Catalog;

            services.Replace(ServiceDescriptor.Singleton<IActiveAncillaryDefinitionReader>(catalog));
            services.Replace(ServiceDescriptor.Singleton<IActiveAncillaryProvisionReader>(catalog));
            services.Replace(ServiceDescriptor.Singleton<IActiveAncillaryPricingReader>(catalog));
            services.Replace(ServiceDescriptor.Singleton<IInventoryConfigurationReader>(catalog));
            services.Replace(ServiceDescriptor.Singleton<ICurrencyReference>(catalog));
            services.Replace(ServiceDescriptor.Singleton<IRateOfExchangeReference>(catalog));
            services.Replace(ServiceDescriptor.Singleton<IAirlineReference>(catalog));
            services.Replace(ServiceDescriptor.Singleton<IAirportReference>(ports.Airports));
            services.Replace(ServiceDescriptor.Singleton<IAirFareReference>(ports.AirFares));
            services.Replace(ServiceDescriptor.Singleton<IPointOfSaleConditionReference>(new TestPointOfSaleConditions()));
            services.Replace(ServiceDescriptor.Singleton<IOperatorAirlineResolver>(new TestOperatorAirline()));
            services.Replace(ServiceDescriptor.Singleton<IApiOfferReferencesReader>(new TestApiOfferReferencesReader(catalog, ports.Airports)));

            if (ports.VerifiedExitRows)
            {
                services.AddScoped<ShoppingSource>();
                services.Replace(ServiceDescriptor.Scoped<IShoppingSource>(provider => new SeatMapDesk.VerifiedExitRows(provider.GetRequiredService<ShoppingSource>())));
            }
        });
    }
}
