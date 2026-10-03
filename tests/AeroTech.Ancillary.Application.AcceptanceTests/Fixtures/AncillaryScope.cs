using AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.ActivateAncillaryPriceRule;
using AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.ChangeAncillaryPriceRule;
using AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.DefineAncillaryPriceRule;
using AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.RetireAncillaryPriceRule;
using AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.SuspendAncillaryPriceRule;
using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.ActivateAncillaryProduct;
using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.ChangeAncillaryProduct;
using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.DefineAncillaryProduct;
using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.RetireAncillaryProduct;
using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.ReviseAncillaryProduct;
using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.SuspendAncillaryProduct;
using AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.CancelServiceReservationUnits;
using AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.ConfirmServiceReservation;
using AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.ReleaseServiceReservation;
using AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.ReserveServiceReservation;
using AeroTech.Ancillary.Application.ServiceSubCodeAggregate.Commands.ReactivateServiceSubCode;
using AeroTech.Ancillary.Application.ServiceSubCodeAggregate.Commands.RegisterServiceSubCode;
using AeroTech.Ancillary.Application.ServiceSubCodeAggregate.Commands.RetireServiceSubCode;
using AeroTech.Ancillary.Domain.AncillaryPriceRuleAggregate.Contracts;
using AeroTech.Ancillary.Domain.AncillaryProductAggregate.Contracts;
using AeroTech.Ancillary.Domain.ServiceReservationAggregate.Contracts;
using AeroTech.Ancillary.Domain.ServiceSubCodeAggregate.Contracts;
using AeroTech.Ancillary.Persistence;
using AeroTech.Ancillary.Persistence.AncillaryPriceRuleAggregate;
using AeroTech.Ancillary.Persistence.AncillaryProductAggregate;
using AeroTech.Ancillary.Persistence.ServiceReservationAggregate;
using AeroTech.Ancillary.Persistence.ServiceSubCodeAggregate;
using AeroTech.Ancillary.Query.AncillaryPriceRuleAggregate.Queries.GetAncillaryPriceRuleById;
using AeroTech.Ancillary.Query.AncillaryProductAggregate.Queries.GetAncillaryProductById;
using AeroTech.Ancillary.Query.AncillaryProductAggregate.Queries.GetAncillaryProductsPaginated;
using AeroTech.Ancillary.Query.AncillaryQuote.Queries.GetAncillaryQuote;
using AeroTech.Ancillary.Query.ServiceReservationAggregate.Queries.GetServiceReservationById;
using AeroTech.Ancillary.Query.ServiceSubCodeAggregate.Queries.GetServiceSubCodeById;
using AeroTech.Ancillary.Query._Shared.DbContexts;
using AeroTech.Ancillary.Synchronizer.AncillaryPriceRuleAggregate;
using AeroTech.Ancillary.Synchronizer.AncillaryProductAggregate;
using AeroTech.Ancillary.Synchronizer.ServiceSubCodeAggregate;
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
        SubCodes = new ServiceSubCodeRepository(Command);
        Products = new AncillaryProductRepository(Command);
        PriceRules = new AncillaryPriceRuleRepository(Command);
        Reservations = new ServiceReservationRepository(Command);
        ProductSynchronizer = new AncillaryProductQueryDbSynchronizer(Query, clock);

        var subCodeSynchronizer = new ServiceSubCodeQueryDbSynchronizer(Query);
        var priceRuleSynchronizer = new AncillaryPriceRuleQueryDbSynchronizer(Query, clock);

        RegisterServiceSubCode = new RegisterServiceSubCodeService(SubCodes, subCodeSynchronizer, UnitOfWork, ids, clock);
        RetireServiceSubCode = new RetireServiceSubCodeService(SubCodes, subCodeSynchronizer, UnitOfWork);
        ReactivateServiceSubCode = new ReactivateServiceSubCodeService(SubCodes, subCodeSynchronizer, UnitOfWork);
        DefineProduct = new DefineAncillaryProductService(Products, SubCodes, ProductSynchronizer, UnitOfWork, ids, clock);
        ChangeProduct = new ChangeAncillaryProductService(Products, SubCodes, ProductSynchronizer, UnitOfWork);
        ActivateProduct = new ActivateAncillaryProductService(Products, SubCodes, ProductSynchronizer, UnitOfWork, clock);
        SuspendProduct = new SuspendAncillaryProductService(Products, ProductSynchronizer, UnitOfWork);
        RetireProduct = new RetireAncillaryProductService(Products, ProductSynchronizer, UnitOfWork, clock);
        ReviseProduct = new ReviseAncillaryProductService(Products, ProductSynchronizer, UnitOfWork, ids, clock);
        DefinePriceRule = new DefineAncillaryPriceRuleService(PriceRules, Products, priceRuleSynchronizer, UnitOfWork, ids, clock);
        ChangePriceRule = new ChangeAncillaryPriceRuleService(PriceRules, priceRuleSynchronizer, UnitOfWork, ids);
        ActivatePriceRule = new ActivateAncillaryPriceRuleService(PriceRules, priceRuleSynchronizer, UnitOfWork);
        SuspendPriceRule = new SuspendAncillaryPriceRuleService(PriceRules, priceRuleSynchronizer, UnitOfWork);
        RetirePriceRule = new RetireAncillaryPriceRuleService(PriceRules, priceRuleSynchronizer, UnitOfWork);
        GetServiceSubCodeById = new GetServiceSubCodeByIdService(Query);
        GetProductById = new GetAncillaryProductByIdService(Query);
        GetProductsPaginated = new GetAncillaryProductsPaginatedService(Query);
        GetPriceRuleById = new GetAncillaryPriceRuleByIdService(Query);
        GetQuote = new GetAncillaryQuoteService(Products, PriceRules);
        ReserveReservation = new ReserveServiceReservationService(Reservations, Products, PriceRules, UnitOfWork, ids, clock);
        ConfirmReservation = new ConfirmServiceReservationService(Reservations, UnitOfWork, clock);
        ReleaseReservation = new ReleaseServiceReservationService(Reservations, UnitOfWork, clock);
        CancelReservationUnits = new CancelServiceReservationUnitsService(Reservations, UnitOfWork, clock);
        GetReservationById = new GetServiceReservationByIdService(Reservations, clock);
    }

    public AncillaryDbContext Command { get; }

    public AncillaryQueryDbContext Query { get; }

    public IUnitOfWork UnitOfWork { get; }

    public IServiceSubCodeRepository SubCodes { get; }

    public IAncillaryProductRepository Products { get; }

    public IAncillaryPriceRuleRepository PriceRules { get; }

    public IServiceReservationRepository Reservations { get; }

    public IAncillaryProductQueryDbSynchronizer ProductSynchronizer { get; }

    public IRegisterServiceSubCodeService RegisterServiceSubCode { get; }

    public IRetireServiceSubCodeService RetireServiceSubCode { get; }

    public IReactivateServiceSubCodeService ReactivateServiceSubCode { get; }

    public IDefineAncillaryProductService DefineProduct { get; }

    public IChangeAncillaryProductService ChangeProduct { get; }

    public IActivateAncillaryProductService ActivateProduct { get; }

    public ISuspendAncillaryProductService SuspendProduct { get; }

    public IRetireAncillaryProductService RetireProduct { get; }

    public IReviseAncillaryProductService ReviseProduct { get; }

    public IDefineAncillaryPriceRuleService DefinePriceRule { get; }

    public IChangeAncillaryPriceRuleService ChangePriceRule { get; }

    public IActivateAncillaryPriceRuleService ActivatePriceRule { get; }

    public ISuspendAncillaryPriceRuleService SuspendPriceRule { get; }

    public IRetireAncillaryPriceRuleService RetirePriceRule { get; }

    public IGetServiceSubCodeByIdService GetServiceSubCodeById { get; }

    public IGetAncillaryProductByIdService GetProductById { get; }

    public IGetAncillaryProductsPaginatedService GetProductsPaginated { get; }

    public IGetAncillaryPriceRuleByIdService GetPriceRuleById { get; }

    public IGetAncillaryQuoteService GetQuote { get; }

    public IReserveServiceReservationService ReserveReservation { get; }

    public IConfirmServiceReservationService ConfirmReservation { get; }

    public IReleaseServiceReservationService ReleaseReservation { get; }

    public ICancelServiceReservationUnitsService CancelReservationUnits { get; }

    public IGetServiceReservationByIdService GetReservationById { get; }

    public async ValueTask DisposeAsync()
    {
        await Command.DisposeAsync();
        await Query.DisposeAsync();
    }
}
