using AeroTech.Ancillary.Application._Shared.Behaviors;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.ReleaseAncillaryHold.Service;
using AeroTech.Ancillary.Query.AncillaryReservationAggregate.Queries.GetAncillaryHoldById.Service;
using AeroTech.Ancillary.RestApi;
using AeroTech.Ancillary.RestApi.V1._Shared;
using AeroTech.Framework.Core.ServiceContracts;
using AeroTech.Framework.Presentation.Extensions;
using AeroTech.Framework.Presentation.Options;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;

public sealed class ReservationHttpHost : IAsyncDisposable
{
    private readonly WebApplication _application;

    private ReservationHttpHost(WebApplication application, HttpClient client)
    {
        _application = application;
        Client = client;
    }

    public HttpClient Client { get; }

    public static async Task<ReservationHttpHost> StartAsync(TestDatabase database, IClock clock)
    {
        var builder = WebApplication.CreateBuilder();
        var application = typeof(ServiceReleaseAncillaryHoldCommandHandler).Assembly;

        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            [$"{JwtOptions.SectionName}:{nameof(JwtOptions.Authority)}"] = "https://identity.invalid/",
            [$"{JwtOptions.SectionName}:{nameof(JwtOptions.Audience)}"] = "pss-api"
        });
        builder.Services
            .AddPresentation(builder.Configuration, typeof(RestApiAssembly).Assembly)
            .AddSurfaceAuthorization();
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(application, typeof(ServiceGetAncillaryHoldByIdQueryHandler).Assembly));
        builder.Services.AddValidatorsFromAssembly(application);
        builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        builder.Services.AddScoped(_ => new AncillaryScope(database, clock));
        builder.Services.AddScoped(services => services.GetRequiredService<AncillaryScope>().HoldAncillaryServices);
        builder.Services.AddScoped(services => services.GetRequiredService<AncillaryScope>().ReleaseAncillaryHold);
        builder.Services.AddScoped(services => services.GetRequiredService<AncillaryScope>().ConfirmAncillaryHold);
        builder.Services.AddScoped(services => services.GetRequiredService<AncillaryScope>().CancelAncillaryHold);
        builder.Services.AddScoped(services => services.GetRequiredService<AncillaryScope>().GetHoldById);

        var host = builder.Build();

        host.UsePresentation();
        await host.StartAsync();

        return new ReservationHttpHost(host, new HttpClient { BaseAddress = new Uri(host.Urls.First()) });
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await _application.StopAsync();
        await _application.DisposeAsync();
    }
}
