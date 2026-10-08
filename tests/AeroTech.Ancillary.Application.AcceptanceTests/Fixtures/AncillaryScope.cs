using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.ActivateAncillaryPricing;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.ChangeAncillaryPricing;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.DefineAncillaryPricing;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.ReactivateAncillaryPricing;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.RetireAncillaryPricing;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.ReviseAncillaryPricing;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.SuspendAncillaryPricing;
using AeroTech.Ancillary.Application.AncillaryPricingAggregate.Commands.SwitchActiveAncillaryPricing;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ActivateAncillaryProvision;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionBlackoutPeriod;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionDayTimeRestriction;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionSeasonalPeriod;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.AddProvisionTravelDate;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeAncillaryProvision;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionBlackoutPeriod;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionDayTimeRestriction;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionSeasonalPeriod;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ChangeProvisionTravelDate;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.PublishAncillaryProvision;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.ReactivateAncillaryProvision;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RemoveProvisionBlackoutPeriod;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RemoveProvisionDayTimeRestriction;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RemoveProvisionSeasonalPeriod;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RemoveProvisionTravelDate;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.RetireAncillaryProvision;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.SuspendAncillaryProvision;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.ConfirmAncillaryHold;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.ActivateAncillaryServiceDefinition;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.AssignAncillaryServiceDefinitionPricingUnit;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.ChangeAncillaryServiceDefinition;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.ReactivateAncillaryServiceDefinition;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.RetireAncillaryServiceDefinition;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.ReviseAncillaryServiceDefinition;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.SuspendAncillaryServiceDefinition;
using AeroTech.Ancillary.Application.SupplierAggregate.Commands.RegisterSupplier;
using AeroTech.Ancillary.Application.SupplierAggregate.Commands.RetireSupplier;
using AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryReservationAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Contracts;
using AeroTech.Ancillary.Domain.SupplierAggregate.Contracts;
using AeroTech.Ancillary.Persistence;
using AeroTech.Ancillary.Persistence.AncillaryPricingAggregate;
using AeroTech.Ancillary.Persistence.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Persistence.AncillaryReservationAggregate;
using AeroTech.Ancillary.Persistence.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Persistence.SupplierAggregate;
using AeroTech.Ancillary.Query.AncillaryPricingAggregate.Queries.GetAncillaryPricingById;
using AeroTech.Ancillary.Query.AncillaryPricingAggregate.Queries.GetAncillaryPricingsPaginated;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetAncillaryProvisionById;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetAncillaryProvisionsPaginated;
using AeroTech.Ancillary.Query.AncillaryReservationAggregate.Queries.GetAncillaryHoldById;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionById;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionsPaginated;
using AeroTech.Ancillary.Query.SupplierAggregate.Queries.GetSupplierById;
using AeroTech.Ancillary.Query.SupplierAggregate.Queries.GetSuppliersPaginated;
using AeroTech.Ancillary.Query._Shared.DbContexts;
using AeroTech.Ancillary.Synchronizer.AncillaryPricingAggregate;
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
        Pricings = new AncillaryPricingRepository(Command);
        Reservations = new AncillaryReservationRepository(Command);

        var supplierSynchronizer = new SupplierQueryDbSynchronizer(Query, clock);
        var definitionSynchronizer = new AncillaryServiceDefinitionQueryDbSynchronizer(Query, clock);
        var provisionSynchronizer = new AncillaryProvisionQueryDbSynchronizer(Query, clock);
        var pricingSynchronizer = new AncillaryPricingQueryDbSynchronizer(Query, clock);

        RegisterSupplier = new RegisterSupplierService(Suppliers, supplierSynchronizer, UnitOfWork, ids, clock);
        RetireSupplier = new RetireSupplierService(Suppliers, supplierSynchronizer, UnitOfWork, clock);

        DefineServiceDefinition = new DefineAncillaryServiceDefinitionService(Definitions, Suppliers, definitionSynchronizer, UnitOfWork, ids, clock);
        ChangeServiceDefinition = new ChangeAncillaryServiceDefinitionService(Definitions, Suppliers, definitionSynchronizer, UnitOfWork);
        ActivateServiceDefinition = new ActivateAncillaryServiceDefinitionService(Definitions, Suppliers, definitionSynchronizer, UnitOfWork, clock);
        SuspendServiceDefinition = new SuspendAncillaryServiceDefinitionService(Definitions, Suppliers, definitionSynchronizer, UnitOfWork, clock);
        ReactivateServiceDefinition = new ReactivateAncillaryServiceDefinitionService(Definitions, Suppliers, definitionSynchronizer, UnitOfWork);
        RetireServiceDefinition = new RetireAncillaryServiceDefinitionService(Definitions, Suppliers, definitionSynchronizer, UnitOfWork, clock);
        ReviseServiceDefinition = new ReviseAncillaryServiceDefinitionService(Definitions, Suppliers, definitionSynchronizer, UnitOfWork, ids, clock);
        AssignPricingUnit = new AssignAncillaryServiceDefinitionPricingUnitService(
            Definitions,
            Suppliers,
            Pricings,
            definitionSynchronizer,
            pricingSynchronizer,
            UnitOfWork);

        DefineProvision = new DefineAncillaryProvisionService(Provisions, Definitions, provisionSynchronizer, UnitOfWork, ids, clock);
        ChangeProvision = new ChangeAncillaryProvisionService(Provisions, provisionSynchronizer, UnitOfWork, ids);
        ActivateProvision = new ActivateAncillaryProvisionService(Provisions, Definitions, Pricings, provisionSynchronizer, UnitOfWork, clock);
        PublishProvision = new PublishAncillaryProvisionService(
            Provisions,
            Definitions,
            Pricings,
            provisionSynchronizer,
            pricingSynchronizer,
            UnitOfWork,
            clock);
        SuspendProvision = new SuspendAncillaryProvisionService(Provisions, provisionSynchronizer, UnitOfWork, clock);
        ReactivateProvision = new ReactivateAncillaryProvisionService(Provisions, Pricings, provisionSynchronizer, UnitOfWork);
        RetireProvision = new RetireAncillaryProvisionService(Provisions, provisionSynchronizer, UnitOfWork, clock);

        AddTravelDate = new AddProvisionTravelDateService(Provisions, provisionSynchronizer, UnitOfWork, ids);
        ChangeTravelDate = new ChangeProvisionTravelDateService(Provisions, provisionSynchronizer, UnitOfWork);
        RemoveTravelDate = new RemoveProvisionTravelDateService(Provisions, provisionSynchronizer, UnitOfWork);
        AddSeasonalPeriod = new AddProvisionSeasonalPeriodService(Provisions, provisionSynchronizer, UnitOfWork, ids);
        ChangeSeasonalPeriod = new ChangeProvisionSeasonalPeriodService(Provisions, provisionSynchronizer, UnitOfWork);
        RemoveSeasonalPeriod = new RemoveProvisionSeasonalPeriodService(Provisions, provisionSynchronizer, UnitOfWork);
        AddBlackoutPeriod = new AddProvisionBlackoutPeriodService(Provisions, provisionSynchronizer, UnitOfWork, ids);
        ChangeBlackoutPeriod = new ChangeProvisionBlackoutPeriodService(Provisions, provisionSynchronizer, UnitOfWork);
        RemoveBlackoutPeriod = new RemoveProvisionBlackoutPeriodService(Provisions, provisionSynchronizer, UnitOfWork);
        AddDayTimeRestriction = new AddProvisionDayTimeRestrictionService(Provisions, provisionSynchronizer, UnitOfWork, ids);
        ChangeDayTimeRestriction = new ChangeProvisionDayTimeRestrictionService(Provisions, provisionSynchronizer, UnitOfWork);
        RemoveDayTimeRestriction = new RemoveProvisionDayTimeRestrictionService(Provisions, provisionSynchronizer, UnitOfWork);

        DefinePricing = new DefineAncillaryPricingService(Pricings, Provisions, Definitions, pricingSynchronizer, UnitOfWork, ids, clock);
        ChangePricing = new ChangeAncillaryPricingService(Pricings, pricingSynchronizer, UnitOfWork, ids);
        ActivatePricing = new ActivateAncillaryPricingService(Pricings, Provisions, Definitions, pricingSynchronizer, UnitOfWork, clock);
        SuspendPricing = new SuspendAncillaryPricingService(Pricings, Provisions, pricingSynchronizer, UnitOfWork, clock);
        ReactivatePricing = new ReactivateAncillaryPricingService(Pricings, Provisions, Definitions, pricingSynchronizer, UnitOfWork);
        RetirePricing = new RetireAncillaryPricingService(Pricings, Provisions, pricingSynchronizer, UnitOfWork, clock);
        RevisePricing = new ReviseAncillaryPricingService(Pricings, pricingSynchronizer, UnitOfWork, ids, clock);
        SwitchActivePricing = new SwitchActiveAncillaryPricingService(Pricings, Provisions, Definitions, pricingSynchronizer, UnitOfWork, clock);

        HoldAncillaryServices = new HoldAncillaryServicesService(Reservations, Definitions, Provisions, Suppliers, UnitOfWork, ids, clock);
        ConfirmAncillaryHold = new ConfirmAncillaryHoldService(Reservations, UnitOfWork, clock);

        GetSupplierById = new GetSupplierByIdService(Query);
        GetSuppliersPaginated = new GetSuppliersPaginatedService(Query);
        GetServiceDefinitionById = new GetAncillaryServiceDefinitionByIdService(Query);
        GetServiceDefinitionsPaginated = new GetAncillaryServiceDefinitionsPaginatedService(Query);
        GetProvisionById = new GetAncillaryProvisionByIdService(Query);
        GetProvisionsPaginated = new GetAncillaryProvisionsPaginatedService(Query);
        GetPricingById = new GetAncillaryPricingByIdService(Query);
        GetPricingsPaginated = new GetAncillaryPricingsPaginatedService(Query);
        GetHoldById = new GetAncillaryHoldByIdService(Reservations, clock);
    }

    public AncillaryDbContext Command { get; }

    public AncillaryQueryDbContext Query { get; }

    public IUnitOfWork UnitOfWork { get; }

    public ISupplierRepository Suppliers { get; }

    public IAncillaryServiceDefinitionRepository Definitions { get; }

    public IAncillaryProvisionRepository Provisions { get; }

    public IAncillaryPricingRepository Pricings { get; }

    public IAncillaryReservationRepository Reservations { get; }

    public IRegisterSupplierService RegisterSupplier { get; }

    public IRetireSupplierService RetireSupplier { get; }

    public IDefineAncillaryServiceDefinitionService DefineServiceDefinition { get; }

    public IChangeAncillaryServiceDefinitionService ChangeServiceDefinition { get; }

    public IActivateAncillaryServiceDefinitionService ActivateServiceDefinition { get; }

    public ISuspendAncillaryServiceDefinitionService SuspendServiceDefinition { get; }

    public IReactivateAncillaryServiceDefinitionService ReactivateServiceDefinition { get; }

    public IRetireAncillaryServiceDefinitionService RetireServiceDefinition { get; }

    public IReviseAncillaryServiceDefinitionService ReviseServiceDefinition { get; }

    public IAssignAncillaryServiceDefinitionPricingUnitService AssignPricingUnit { get; }

    public IDefineAncillaryProvisionService DefineProvision { get; }

    public IChangeAncillaryProvisionService ChangeProvision { get; }

    public IActivateAncillaryProvisionService ActivateProvision { get; }

    public IPublishAncillaryProvisionService PublishProvision { get; }

    public ISuspendAncillaryProvisionService SuspendProvision { get; }

    public IReactivateAncillaryProvisionService ReactivateProvision { get; }

    public IRetireAncillaryProvisionService RetireProvision { get; }

    public IAddProvisionTravelDateService AddTravelDate { get; }

    public IChangeProvisionTravelDateService ChangeTravelDate { get; }

    public IRemoveProvisionTravelDateService RemoveTravelDate { get; }

    public IAddProvisionSeasonalPeriodService AddSeasonalPeriod { get; }

    public IChangeProvisionSeasonalPeriodService ChangeSeasonalPeriod { get; }

    public IRemoveProvisionSeasonalPeriodService RemoveSeasonalPeriod { get; }

    public IAddProvisionBlackoutPeriodService AddBlackoutPeriod { get; }

    public IChangeProvisionBlackoutPeriodService ChangeBlackoutPeriod { get; }

    public IRemoveProvisionBlackoutPeriodService RemoveBlackoutPeriod { get; }

    public IAddProvisionDayTimeRestrictionService AddDayTimeRestriction { get; }

    public IChangeProvisionDayTimeRestrictionService ChangeDayTimeRestriction { get; }

    public IRemoveProvisionDayTimeRestrictionService RemoveDayTimeRestriction { get; }

    public IDefineAncillaryPricingService DefinePricing { get; }

    public IChangeAncillaryPricingService ChangePricing { get; }

    public IActivateAncillaryPricingService ActivatePricing { get; }

    public ISuspendAncillaryPricingService SuspendPricing { get; }

    public IReactivateAncillaryPricingService ReactivatePricing { get; }

    public IRetireAncillaryPricingService RetirePricing { get; }

    public IReviseAncillaryPricingService RevisePricing { get; }

    public ISwitchActiveAncillaryPricingService SwitchActivePricing { get; }

    public IHoldAncillaryServicesService HoldAncillaryServices { get; }

    public IConfirmAncillaryHoldService ConfirmAncillaryHold { get; }

    public IGetSupplierByIdService GetSupplierById { get; }

    public IGetSuppliersPaginatedService GetSuppliersPaginated { get; }

    public IGetAncillaryServiceDefinitionByIdService GetServiceDefinitionById { get; }

    public IGetAncillaryServiceDefinitionsPaginatedService GetServiceDefinitionsPaginated { get; }

    public IGetAncillaryProvisionByIdService GetProvisionById { get; }

    public IGetAncillaryProvisionsPaginatedService GetProvisionsPaginated { get; }

    public IGetAncillaryPricingByIdService GetPricingById { get; }

    public IGetAncillaryPricingsPaginatedService GetPricingsPaginated { get; }

    public IGetAncillaryHoldByIdService GetHoldById { get; }

    public async ValueTask DisposeAsync()
    {
        await Command.DisposeAsync();
        await Query.DisposeAsync();
    }
}
