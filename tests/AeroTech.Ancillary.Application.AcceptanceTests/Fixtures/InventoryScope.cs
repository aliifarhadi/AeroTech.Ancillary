using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.ActivateAirportSlotInventory;
using AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.AdjustAirportSlotInventory;
using AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.CloseAirportSlotInventoryForSale;
using AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.DefineAirportSlotInventory;
using AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.OpenAirportSlotInventoryForSale;
using AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.RetireAirportSlotInventory;
using AeroTech.Ancillary.Application.AirportSlotInventoryAggregate.Commands.SuspendAirportSlotInventory;
using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.ActivateInventoryPolicy;
using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.ChangeInventoryPolicy;
using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.DefineInventoryPolicy;
using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.RetireInventoryPolicy;
using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Commands.SuspendInventoryPolicy;
using AeroTech.Ancillary.Application.AncillaryInventoryPolicyAggregate.Services;
using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.ActivateFlightCountInventory;
using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.AdjustFlightCountInventory;
using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.CloseFlightCountInventoryForSale;
using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.DefineFlightCountInventory;
using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.OpenFlightCountInventoryForSale;
using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.RetireFlightCountInventory;
using AeroTech.Ancillary.Application.FlightCountInventoryAggregate.Commands.SuspendFlightCountInventory;
using AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.ActivateFlightWeightInventory;
using AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.AdjustFlightWeightInventory;
using AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.CloseFlightWeightInventoryForSale;
using AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.DefineFlightWeightInventory;
using AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.OpenFlightWeightInventoryForSale;
using AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.RetireFlightWeightInventory;
using AeroTech.Ancillary.Application.FlightWeightInventoryAggregate.Commands.SuspendFlightWeightInventory;
using AeroTech.Ancillary.Application._Shared.Authorization;
using AeroTech.Ancillary.Domain.AirportSlotInventoryAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.Contracts;
using AeroTech.Ancillary.Domain.FlightCountInventoryAggregate.Contracts;
using AeroTech.Ancillary.Domain.FlightWeightInventoryAggregate.Contracts;
using AeroTech.Ancillary.Persistence;
using AeroTech.Ancillary.Persistence.AirportSlotInventoryAggregate;
using AeroTech.Ancillary.Persistence.AncillaryInventoryPolicyAggregate;
using AeroTech.Ancillary.Persistence.FlightCountInventoryAggregate;
using AeroTech.Ancillary.Persistence.FlightWeightInventoryAggregate;
using AeroTech.Ancillary.Query.AirportSlotInventoryAggregate.Queries.GetAirportSlotInventoriesPaginated;
using AeroTech.Ancillary.Query.AirportSlotInventoryAggregate.Queries.GetAirportSlotInventoryById;
using AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Queries.GetInventoryConfigurationSnapshot;
using AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Queries.GetInventoryPoliciesPaginated;
using AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Queries.GetInventoryPolicyById;
using AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Queries.GetInventoryPolicyByServiceIdentity;
using AeroTech.Ancillary.Query.FlightCountInventoryAggregate.Queries.GetFlightCountInventoriesPaginated;
using AeroTech.Ancillary.Query.FlightCountInventoryAggregate.Queries.GetFlightCountInventoryById;
using AeroTech.Ancillary.Query.FlightWeightInventoryAggregate.Queries.GetFlightWeightInventoriesPaginated;
using AeroTech.Ancillary.Query.FlightWeightInventoryAggregate.Queries.GetFlightWeightInventoryById;
using AeroTech.Ancillary.Query._Shared.DbContexts;
using AeroTech.Ancillary.Synchronizer.AirportSlotInventoryAggregate;
using AeroTech.Ancillary.Synchronizer.AncillaryInventoryPolicyAggregate;
using AeroTech.Ancillary.Synchronizer.FlightCountInventoryAggregate;
using AeroTech.Ancillary.Synchronizer.FlightWeightInventoryAggregate;
using AeroTech.Ancillary.Synchronizer._Shared;
using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;

public sealed class InventoryScope : IAsyncDisposable
{
    public InventoryScope(TestDatabase database, IClock clock, InventoryFixture fixture)
    {
        var ids = database.Ids;

        Command = database.NewContext(clock);
        Query = database.NewQueryContext();
        UnitOfWork = new AncillaryUnitOfWork(Command, Query);
        Policies = new InventoryPolicyRepository(Command);
        FlightCountInventories = new FlightCountInventoryRepository(Command);
        FlightWeightInventories = new FlightWeightInventoryRepository(Command);
        AirportSlotInventories = new AirportSlotInventoryRepository(Command);

        var policySynchronizer = new InventoryPolicyQueryDbSynchronizer(Query, clock);
        var flightCountSynchronizer = new FlightCountInventoryQueryDbSynchronizer(Query, clock);
        var flightWeightSynchronizer = new FlightWeightInventoryQueryDbSynchronizer(Query, clock);
        var airportSlotSynchronizer = new AirportSlotInventoryQueryDbSynchronizer(Query, clock);
        var caller = new InventoryCallerScope(fixture, fixture);
        var facts = new InventoryCommercialFactsReader(Command);
        var evidence = new InventoryPolicyEvidenceBuilder(facts, fixture, fixture, fixture, fixture, FlightCountInventories, Policies);

        DefineInventoryPolicy = new DefineInventoryPolicyService(Policies, policySynchronizer, UnitOfWork, caller, facts, ids, clock);
        ChangeInventoryPolicy = new ChangeInventoryPolicyService(Policies, policySynchronizer, UnitOfWork, caller, facts, ids, clock);
        ActivateInventoryPolicy = new ActivateInventoryPolicyService(Policies, policySynchronizer, UnitOfWork, caller, evidence, clock);
        SuspendInventoryPolicy = new SuspendInventoryPolicyService(Policies, policySynchronizer, UnitOfWork, caller, clock);
        RetireInventoryPolicy = new RetireInventoryPolicyService(Policies, policySynchronizer, UnitOfWork, caller, clock);
        DefineFlightCountInventory = new DefineFlightCountInventoryService(FlightCountInventories, flightCountSynchronizer, UnitOfWork, caller, ids, clock);
        ActivateFlightCountInventory = new ActivateFlightCountInventoryService(FlightCountInventories, flightCountSynchronizer, UnitOfWork, caller, fixture, fixture, clock);
        AdjustFlightCountInventory = new AdjustFlightCountInventoryService(FlightCountInventories, flightCountSynchronizer, UnitOfWork, caller, ids, clock);
        CloseFlightCountInventoryForSale = new CloseFlightCountInventoryForSaleService(FlightCountInventories, flightCountSynchronizer, UnitOfWork, caller, clock);
        OpenFlightCountInventoryForSale = new OpenFlightCountInventoryForSaleService(FlightCountInventories, flightCountSynchronizer, UnitOfWork, caller, clock);
        SuspendFlightCountInventory = new SuspendFlightCountInventoryService(FlightCountInventories, flightCountSynchronizer, UnitOfWork, caller, clock);
        RetireFlightCountInventory = new RetireFlightCountInventoryService(FlightCountInventories, flightCountSynchronizer, UnitOfWork, caller, clock);
        DefineFlightWeightInventory = new DefineFlightWeightInventoryService(FlightWeightInventories, flightWeightSynchronizer, UnitOfWork, caller, ids, clock);
        ActivateFlightWeightInventory = new ActivateFlightWeightInventoryService(FlightWeightInventories, flightWeightSynchronizer, UnitOfWork, caller, fixture, fixture, clock);
        AdjustFlightWeightInventory = new AdjustFlightWeightInventoryService(FlightWeightInventories, flightWeightSynchronizer, UnitOfWork, caller, ids, clock);
        CloseFlightWeightInventoryForSale = new CloseFlightWeightInventoryForSaleService(FlightWeightInventories, flightWeightSynchronizer, UnitOfWork, caller, clock);
        OpenFlightWeightInventoryForSale = new OpenFlightWeightInventoryForSaleService(FlightWeightInventories, flightWeightSynchronizer, UnitOfWork, caller, clock);
        SuspendFlightWeightInventory = new SuspendFlightWeightInventoryService(FlightWeightInventories, flightWeightSynchronizer, UnitOfWork, caller, clock);
        RetireFlightWeightInventory = new RetireFlightWeightInventoryService(FlightWeightInventories, flightWeightSynchronizer, UnitOfWork, caller, clock);
        DefineAirportSlotInventory = new DefineAirportSlotInventoryService(AirportSlotInventories, airportSlotSynchronizer, UnitOfWork, caller, fixture, ids, clock);
        ActivateAirportSlotInventory = new ActivateAirportSlotInventoryService(AirportSlotInventories, airportSlotSynchronizer, UnitOfWork, caller, fixture, fixture, clock);
        AdjustAirportSlotInventory = new AdjustAirportSlotInventoryService(AirportSlotInventories, airportSlotSynchronizer, UnitOfWork, caller, ids, clock);
        CloseAirportSlotInventoryForSale = new CloseAirportSlotInventoryForSaleService(AirportSlotInventories, airportSlotSynchronizer, UnitOfWork, caller, clock);
        OpenAirportSlotInventoryForSale = new OpenAirportSlotInventoryForSaleService(AirportSlotInventories, airportSlotSynchronizer, UnitOfWork, caller, clock);
        SuspendAirportSlotInventory = new SuspendAirportSlotInventoryService(AirportSlotInventories, airportSlotSynchronizer, UnitOfWork, caller, clock);
        RetireAirportSlotInventory = new RetireAirportSlotInventoryService(AirportSlotInventories, airportSlotSynchronizer, UnitOfWork, caller, clock);
        GetInventoryPolicyById = new GetInventoryPolicyByIdService(Query, caller, facts);
        GetInventoryPolicyByServiceIdentity = new GetInventoryPolicyByServiceIdentityService(Query, caller, facts);
        GetInventoryPoliciesPaginated = new GetInventoryPoliciesPaginatedService(Query, caller);
        GetInventoryConfigurationSnapshot = new GetInventoryConfigurationSnapshotService(Query, caller, facts, clock);
        GetFlightCountInventoryById = new GetFlightCountInventoryByIdService(Query, caller);
        GetFlightCountInventoriesPaginated = new GetFlightCountInventoriesPaginatedService(Query, caller);
        GetFlightWeightInventoryById = new GetFlightWeightInventoryByIdService(Query, caller);
        GetFlightWeightInventoriesPaginated = new GetFlightWeightInventoriesPaginatedService(Query, caller);
        GetAirportSlotInventoryById = new GetAirportSlotInventoryByIdService(Query, caller);
        GetAirportSlotInventoriesPaginated = new GetAirportSlotInventoriesPaginatedService(Query, caller);
    }

    public AncillaryDbContext Command { get; }

    public AncillaryQueryDbContext Query { get; }

    public IUnitOfWork UnitOfWork { get; }

    public IInventoryPolicyRepository Policies { get; }

    public IFlightCountInventoryRepository FlightCountInventories { get; }

    public IFlightWeightInventoryRepository FlightWeightInventories { get; }

    public IAirportSlotInventoryRepository AirportSlotInventories { get; }

    public IDefineInventoryPolicyService DefineInventoryPolicy { get; }

    public IChangeInventoryPolicyService ChangeInventoryPolicy { get; }

    public IActivateInventoryPolicyService ActivateInventoryPolicy { get; }

    public ISuspendInventoryPolicyService SuspendInventoryPolicy { get; }

    public IRetireInventoryPolicyService RetireInventoryPolicy { get; }

    public IDefineFlightCountInventoryService DefineFlightCountInventory { get; }

    public IActivateFlightCountInventoryService ActivateFlightCountInventory { get; }

    public IAdjustFlightCountInventoryService AdjustFlightCountInventory { get; }

    public ICloseFlightCountInventoryForSaleService CloseFlightCountInventoryForSale { get; }

    public IOpenFlightCountInventoryForSaleService OpenFlightCountInventoryForSale { get; }

    public ISuspendFlightCountInventoryService SuspendFlightCountInventory { get; }

    public IRetireFlightCountInventoryService RetireFlightCountInventory { get; }

    public IDefineFlightWeightInventoryService DefineFlightWeightInventory { get; }

    public IActivateFlightWeightInventoryService ActivateFlightWeightInventory { get; }

    public IAdjustFlightWeightInventoryService AdjustFlightWeightInventory { get; }

    public ICloseFlightWeightInventoryForSaleService CloseFlightWeightInventoryForSale { get; }

    public IOpenFlightWeightInventoryForSaleService OpenFlightWeightInventoryForSale { get; }

    public ISuspendFlightWeightInventoryService SuspendFlightWeightInventory { get; }

    public IRetireFlightWeightInventoryService RetireFlightWeightInventory { get; }

    public IDefineAirportSlotInventoryService DefineAirportSlotInventory { get; }

    public IActivateAirportSlotInventoryService ActivateAirportSlotInventory { get; }

    public IAdjustAirportSlotInventoryService AdjustAirportSlotInventory { get; }

    public ICloseAirportSlotInventoryForSaleService CloseAirportSlotInventoryForSale { get; }

    public IOpenAirportSlotInventoryForSaleService OpenAirportSlotInventoryForSale { get; }

    public ISuspendAirportSlotInventoryService SuspendAirportSlotInventory { get; }

    public IRetireAirportSlotInventoryService RetireAirportSlotInventory { get; }

    public IGetInventoryPolicyByIdService GetInventoryPolicyById { get; }

    public IGetInventoryPolicyByServiceIdentityService GetInventoryPolicyByServiceIdentity { get; }

    public IGetInventoryPoliciesPaginatedService GetInventoryPoliciesPaginated { get; }

    public IGetInventoryConfigurationSnapshotService GetInventoryConfigurationSnapshot { get; }

    public IGetFlightCountInventoryByIdService GetFlightCountInventoryById { get; }

    public IGetFlightCountInventoriesPaginatedService GetFlightCountInventoriesPaginated { get; }

    public IGetFlightWeightInventoryByIdService GetFlightWeightInventoryById { get; }

    public IGetFlightWeightInventoriesPaginatedService GetFlightWeightInventoriesPaginated { get; }

    public IGetAirportSlotInventoryByIdService GetAirportSlotInventoryById { get; }

    public IGetAirportSlotInventoriesPaginatedService GetAirportSlotInventoriesPaginated { get; }

    public async ValueTask DisposeAsync()
    {
        await Command.DisposeAsync();
        await Query.DisposeAsync();
    }
}
