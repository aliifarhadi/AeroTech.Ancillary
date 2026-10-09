using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Framework.Infrastructure;
using AeroTech.Framework.Presentation.Extensions;
using AeroTech.Ancillary.Application;
using AeroTech.Ancillary.Consumers;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Persistence;
using AeroTech.Ancillary.Providers;
using AeroTech.Ancillary.Query;
using AeroTech.Ancillary.ReferenceData;
using AeroTech.Ancillary.RestApi;
using AeroTech.Ancillary.RestApi.V1._Shared;
using AeroTech.Ancillary.ServiceHost.CallerContext;
using AeroTech.Ancillary.ServiceHost.ReferenceData;
using AeroTech.Ancillary.Synchronizer;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration).Enrich.FromLogContext().WriteTo.Console());

builder.Services
    .AddFrameworkInfrastructure(builder.Configuration)
    .AddPersistence(builder.Configuration)
    .AddProviders(builder.Configuration)
    .AddQuery(builder.Configuration)
    .AddSynchronizer()
    .AddConsumers(builder.Configuration)
    .AddApplication(builder.Configuration)
    .AddReferenceData(builder.Configuration)
    .AddPresentation(builder.Configuration, typeof(RestApiAssembly).Assembly, typeof(ReferenceDataAssembly).Assembly)
    .AddSurfaceAuthorization();

builder.Services.AddScoped<ICallerContext, ClaimsCallerContext>();
builder.Services.AddScoped<ICountryCodeResolver, ReferenceDataCountryCodeResolver>();
builder.Services.AddScoped<IAirlineOfficeTimeZoneResolver, ReferenceDataAirlineOfficeTimeZoneResolver>();
builder.Services.AddScoped<IOperatorAirlineResolver, ReferenceDataOperatorAirlineResolver>();
builder.Services.AddScoped<IAirportReference, ReferenceDataAirportReference>();
builder.Services.Replace(ServiceDescriptor.Scoped<IActorResolver, CallerContextActorResolver>());

var app = builder.Build();

app.UsePresentation();

app.Run();

public partial class Program
{
}
