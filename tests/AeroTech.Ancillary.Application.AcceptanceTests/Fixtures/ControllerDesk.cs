using AeroTech.Ancillary.Application._Shared.Behaviors;
using AeroTech.Ancillary.Query._Shared.DbContexts;
using AeroTech.Framework.Core.ServiceContracts;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using DefinitionController = AeroTech.Ancillary.RestApi.V1.AncillaryServiceDefinitionAggregate.Controllers.BackofficeController;
using ProvisionController = AeroTech.Ancillary.RestApi.V1.AncillaryProvisionAggregate.Controllers.BackofficeController;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;

public sealed class ControllerDesk : IAsyncDisposable
{
    private readonly ServiceProvider _provider;

    public ControllerDesk(TestDatabase database, IClock clock)
    {
        Scope = new AncillaryScope(database, clock);

        var services = new ServiceCollection();
        var application = typeof(ValidationBehavior<,>).Assembly;

        services.AddMediatR(configuration => configuration.RegisterServicesFromAssemblies(application, typeof(AncillaryQueryDbContext).Assembly));
        services.AddValidatorsFromAssembly(application);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddSingleton(Scope.DefineServiceDefinition);
        services.AddSingleton(Scope.ChangeServiceDefinition);
        services.AddSingleton(Scope.GetServiceDefinitionById);
        services.AddSingleton(Scope.GetServiceDefinitionForm);
        services.AddSingleton(Scope.ChangeProvision);
        services.AddSingleton(Scope.GetProvisionById);
        services.AddSingleton(Scope.GetProvisionForm);

        _provider = services.BuildServiceProvider();
    }

    public AncillaryScope Scope { get; }

    public DefinitionController ServiceDefinitions => new(_provider.GetRequiredService<IMediator>());

    public ProvisionController Provisions => new(_provider.GetRequiredService<IMediator>());

    public async ValueTask DisposeAsync()
    {
        await _provider.DisposeAsync();
        await Scope.DisposeAsync();
    }
}
