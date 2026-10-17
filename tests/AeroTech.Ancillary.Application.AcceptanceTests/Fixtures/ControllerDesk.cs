using AeroTech.Ancillary.Application._Shared.Behaviors;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetProfileProvisionCreateForm;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetProfileProvisionForm;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetSeatProvisionCreateForm;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetSeatProvisionForm;
using AeroTech.Ancillary.Query._Shared.DbContexts;
using AeroTech.Framework.Core.ServiceContracts;
using FluentValidation;
using AeroTech.Ancillary.RestApi.V1.AncillaryProvisionAggregate.Controllers;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using DefinitionController = AeroTech.Ancillary.RestApi.V1.AncillaryServiceDefinitionAggregate.Controllers.BackofficeController;
using ProvisionController = AeroTech.Ancillary.RestApi.V1.AncillaryProvisionAggregate.Controllers.BackofficeController;
using SeatProvisionController = AeroTech.Ancillary.RestApi.V1.AncillaryProvisionAggregate.Controllers.SeatBackofficeController;

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
        services.AddSingleton(Scope.DefineProvision);
        services.AddSingleton(Scope.ChangeProvision);
        services.AddSingleton(Scope.GetProvisionById);
        services.AddSingleton(Scope.GetProvisionForm);
        services.AddSingleton<IGetSeatProvisionCreateFormService>(new GetSeatProvisionCreateFormService(Scope.Definitions));
        services.AddSingleton<IGetSeatProvisionFormService>(new GetSeatProvisionFormService(Scope.GetProvisionForm));
        services.AddSingleton<IGetProfileProvisionCreateFormService>(new GetProfileProvisionCreateFormService(Scope.Definitions));
        services.AddSingleton<IGetProfileProvisionFormService>(new GetProfileProvisionFormService(Scope.GetProvisionForm, Scope.Definitions));

        _provider = services.BuildServiceProvider();
    }

    public AncillaryScope Scope { get; }

    public DefinitionController ServiceDefinitions => new(_provider.GetRequiredService<IMediator>());

    public ProvisionController Provisions => new(_provider.GetRequiredService<IMediator>());

    public SeatProvisionController SeatProvisions => new(_provider.GetRequiredService<IMediator>());

    public BaggageBackofficeController BaggageProvisions => new(_provider.GetRequiredService<IMediator>());

    public UpgradeBackofficeController UpgradeProvisions => new(_provider.GetRequiredService<IMediator>());

    public MealBackofficeController MealProvisions => new(_provider.GetRequiredService<IMediator>());

    public PetBackofficeController PetProvisions => new(_provider.GetRequiredService<IMediator>());

    public AssistedTravelBackofficeController AssistedTravelProvisions => new(_provider.GetRequiredService<IMediator>());

    public AirportServiceBackofficeController AirportServiceProvisions => new(_provider.GetRequiredService<IMediator>());

    public PriorityBackofficeController PriorityProvisions => new(_provider.GetRequiredService<IMediator>());

    public ConnectivityBackofficeController ConnectivityProvisions => new(_provider.GetRequiredService<IMediator>());

    public async ValueTask DisposeAsync()
    {
        await _provider.DisposeAsync();
        await Scope.DisposeAsync();
    }
}
