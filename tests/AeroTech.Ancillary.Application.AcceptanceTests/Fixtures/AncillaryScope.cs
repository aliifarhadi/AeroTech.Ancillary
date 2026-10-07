using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ActivateAncillaryProvision;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeAncillaryProvision;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ReactivateAncillaryProvision;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RetireAncillaryProvision;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.SuspendAncillaryProvision;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.ConfirmAncillaryHold;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.ActivateAncillaryServiceDefinition;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.ChangeAncillaryServiceDefinition;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.ReactivateAncillaryServiceDefinition;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.RetireAncillaryServiceDefinition;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.ReviseAncillaryServiceDefinition;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.SuspendAncillaryServiceDefinition;
using AeroTech.Ancillary.Application.SupplierAggregate.Commands.RegisterSupplier;
using AeroTech.Ancillary.Application.SupplierAggregate.Commands.RetireSupplier;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Contracts;
using AeroTech.Ancillary.Domain.SupplierAggregate.Contracts;
using AeroTech.Ancillary.Persistence;
using AeroTech.Ancillary.Persistence.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Persistence.AncillaryReservationAggregate;
using AeroTech.Ancillary.Persistence.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Persistence.SupplierAggregate;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetAncillaryProvisionById;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetAncillaryProvisionsPaginated;
using AeroTech.Ancillary.Query.AncillaryReservationAggregate.Queries.GetAncillaryHoldById;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionById;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionsPaginated;
using AeroTech.Ancillary.Query.SupplierAggregate.Queries.GetSupplierById;
using AeroTech.Ancillary.Query.SupplierAggregate.Queries.GetSuppliersPaginated;
using AeroTech.Ancillary.Query._Shared.DbContexts;
using AeroTech.Ancillary.Synchronizer.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Synchronizer.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Synchronizer.SupplierAggregate;
using AeroTech.Ancillary.Synchronizer._Shared;
using AeroTech.Framework.Core.Domain.Repository;
using AeroTech.Framework.Core.ServiceContracts;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;

public sealed class AncillaryScope : IAsyncDisposable
{
    public AncillaryScope(TestDatabase database, IClock clock)
    {
        var ids = database.Ids;

        Command = database.NewContext(clock);
        Query = database.NewQueryContext();
        UnitOfWork = new AncillaryUnitOfWork(Command, Query);
        Suppliers = new SupplierRepository(Command);
        Definitions = new AncillaryServiceDefinitionRepository(Command);
        Provisions = new AncillaryProvisionRepository(Command);
        Reservations = new AncillaryReservationRepository(Command);

        var supplierSynchronizer = new SupplierQueryDbSynchronizer(Query, clock);
        var definitionSynchronizer = new AncillaryServiceDefinitionQueryDbSynchronizer(Query, clock);
        var provisionSynchronizer = new AncillaryProvisionQueryDbSynchronizer(Query, clock);

        RegisterSupplier = new RegisterSupplierService(Suppliers, supplierSynchronizer, UnitOfWork, ids, clock);
        DefineServiceDefinition = new DefineAncillaryServiceDefinitionService(Definitions, Suppliers, definitionSynchronizer, UnitOfWork, ids, clock);
        ActivateServiceDefinition = new ActivateAncillaryServiceDefinitionService(Definitions, Suppliers, definitionSynchronizer, UnitOfWork, clock);
        DefineProvision = new DefineAncillaryProvisionService(Provisions, Definitions, provisionSynchronizer, UnitOfWork, ids, clock);
        ActivateProvision = new ActivateAncillaryProvisionService(Provisions, provisionSynchronizer, UnitOfWork, clock);
        RetireSupplier = new RetireSupplierService(Suppliers, supplierSynchronizer, UnitOfWork, clock);
        ChangeServiceDefinition = new ChangeAncillaryServiceDefinitionService(Definitions, Suppliers, definitionSynchronizer, UnitOfWork);
        SuspendServiceDefinition = new SuspendAncillaryServiceDefinitionService(Definitions, Suppliers, definitionSynchronizer, UnitOfWork, clock);
        ReactivateServiceDefinition = new ReactivateAncillaryServiceDefinitionService(Definitions, Suppliers, definitionSynchronizer, UnitOfWork);
        RetireServiceDefinition = new RetireAncillaryServiceDefinitionService(Definitions, Suppliers, definitionSynchronizer, UnitOfWork, clock);
        ReviseServiceDefinition = new ReviseAncillaryServiceDefinitionService(Definitions, Suppliers, definitionSynchronizer, UnitOfWork, ids, clock);
        ChangeProvision = new ChangeAncillaryProvisionService(Provisions, provisionSynchronizer, UnitOfWork, ids);
        SuspendProvision = new SuspendAncillaryProvisionService(Provisions, provisionSynchronizer, UnitOfWork, clock);
        ReactivateProvision = new ReactivateAncillaryProvisionService(Provisions, provisionSynchronizer, UnitOfWork);
        RetireProvision = new RetireAncillaryProvisionService(Provisions, provisionSynchronizer, UnitOfWork, clock);
        HoldAncillaryServices = new HoldAncillaryServicesService(Reservations, Definitions, Provisions, Suppliers, UnitOfWork, ids, clock);
        ConfirmAncillaryHold = new ConfirmAncillaryHoldService(Reservations, UnitOfWork, clock);
        GetSupplierById = new GetSupplierByIdService(Query);
        GetSuppliersPaginated = new GetSuppliersPaginatedService(Query);
        GetServiceDefinitionById = new GetAncillaryServiceDefinitionByIdService(Query);
        GetServiceDefinitionsPaginated = new GetAncillaryServiceDefinitionsPaginatedService(Query);
        GetProvisionById = new GetAncillaryProvisionByIdService(Query);
        GetProvisionsPaginated = new GetAncillaryProvisionsPaginatedService(Query);
        GetHoldById = new GetAncillaryHoldByIdService(Reservations, clock);
    }

    public AncillaryDbContext Command { get; }

    public AncillaryQueryDbContext Query { get; }

    public IUnitOfWork UnitOfWork { get; }

    public ISupplierRepository Suppliers { get; }

    public IAncillaryServiceDefinitionRepository Definitions { get; }

    public IAncillaryProvisionRepository Provisions { get; }

    public IAncillaryReservationRepository Reservations { get; }

    public IRegisterSupplierService RegisterSupplier { get; }

    public IDefineAncillaryServiceDefinitionService DefineServiceDefinition { get; }

    public IActivateAncillaryServiceDefinitionService ActivateServiceDefinition { get; }

    public IDefineAncillaryProvisionService DefineProvision { get; }

    public IActivateAncillaryProvisionService ActivateProvision { get; }

    public IRetireSupplierService RetireSupplier { get; }

    public IChangeAncillaryServiceDefinitionService ChangeServiceDefinition { get; }

    public ISuspendAncillaryServiceDefinitionService SuspendServiceDefinition { get; }

    public IReactivateAncillaryServiceDefinitionService ReactivateServiceDefinition { get; }

    public IRetireAncillaryServiceDefinitionService RetireServiceDefinition { get; }

    public IReviseAncillaryServiceDefinitionService ReviseServiceDefinition { get; }

    public IChangeAncillaryProvisionService ChangeProvision { get; }

    public ISuspendAncillaryProvisionService SuspendProvision { get; }

    public IReactivateAncillaryProvisionService ReactivateProvision { get; }

    public IRetireAncillaryProvisionService RetireProvision { get; }

    public IHoldAncillaryServicesService HoldAncillaryServices { get; }

    public IConfirmAncillaryHoldService ConfirmAncillaryHold { get; }

    public IGetSupplierByIdService GetSupplierById { get; }

    public IGetSuppliersPaginatedService GetSuppliersPaginated { get; }

    public IGetAncillaryServiceDefinitionByIdService GetServiceDefinitionById { get; }

    public IGetAncillaryServiceDefinitionsPaginatedService GetServiceDefinitionsPaginated { get; }

    public IGetAncillaryProvisionByIdService GetProvisionById { get; }

    public IGetAncillaryProvisionsPaginatedService GetProvisionsPaginated { get; }

    public IGetAncillaryHoldByIdService GetHoldById { get; }

    public async ValueTask DisposeAsync()
    {
        await Command.DisposeAsync();
        await Query.DisposeAsync();
    }
}
