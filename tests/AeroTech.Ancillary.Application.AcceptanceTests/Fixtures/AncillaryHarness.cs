using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.ActivateAncillaryPriceRule.Backoffice;
using AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.ChangeAncillaryPriceRule.Backoffice;
using AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.DefineAncillaryPriceRule;
using AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.DefineAncillaryPriceRule.Backoffice;
using AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.RetireAncillaryPriceRule.Backoffice;
using AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.SuspendAncillaryPriceRule.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.ActivateAncillaryProduct;
using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.ActivateAncillaryProduct.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.ChangeAncillaryProduct.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.DefineAncillaryProduct;
using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.DefineAncillaryProduct.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.RetireAncillaryProduct.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.ReviseAncillaryProduct.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.SuspendAncillaryProduct.Backoffice;
using AeroTech.Ancillary.Application.ServiceSubCodeAggregate.Commands.ReactivateServiceSubCode.Backoffice;
using AeroTech.Ancillary.Application.ServiceSubCodeAggregate.Commands.RegisterServiceSubCode;
using AeroTech.Ancillary.Application.ServiceSubCodeAggregate.Commands.RegisterServiceSubCode.Backoffice;
using AeroTech.Ancillary.Application.ServiceSubCodeAggregate.Commands.RetireServiceSubCode.Backoffice;
using AeroTech.Ancillary.Query.AncillaryPriceRuleAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryProductAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryQuote.Dto;
using AeroTech.Ancillary.Query.AncillaryQuote.Queries.GetAncillaryQuote.Service;
using AeroTech.Ancillary.Query.ServiceSubCodeAggregate.Dto;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;

public sealed class AncillaryHarness(TestDatabase database)
{
    public FixedClock Clock { get; } = new();

    public int AirlineId { get; } = database.NextAirlineId();

    public AncillaryScope NewScope() => new(database, Clock);

    public async Task<TResult> RunAsync<TResult>(Func<AncillaryScope, Task<TResult>> request)
    {
        await using var scope = NewScope();

        return await request(scope);
    }

    public Task<ServiceSubCodeResult> RegisterAsync(BackofficeRegisterServiceSubCodeCommand command)
        => RunAsync(scope => scope.RegisterServiceSubCode.RegisterAsync(command));

    public Task<ServiceSubCodeResult> RetireSubCodeAsync(long id)
        => RunAsync(scope => scope.RetireServiceSubCode.RetireAsync(new BackofficeRetireServiceSubCodeCommand(id)));

    public Task<ServiceSubCodeResult> ReactivateSubCodeAsync(long id)
        => RunAsync(scope => scope.ReactivateServiceSubCode.ReactivateAsync(new BackofficeReactivateServiceSubCodeCommand(id)));

    public Task<BackofficeServiceSubCodeDto> GetSubCodeAsync(long id)
        => RunAsync(scope => scope.GetServiceSubCodeById.ExecuteAsync(id));

    public Task<AncillaryProductResult> DefineAsync(BackofficeDefineAncillaryProductCommand command)
        => RunAsync(scope => scope.DefineProduct.DefineAsync(command));

    public Task<AncillaryProductResult> ChangeAsync(BackofficeChangeAncillaryProductCommand command)
        => RunAsync(scope => scope.ChangeProduct.ChangeAsync(command));

    public Task<ActivateAncillaryProductResult> ActivateProductAsync(long id)
        => RunAsync(scope => scope.ActivateProduct.ActivateAsync(new BackofficeActivateAncillaryProductCommand(id)));

    public Task<AncillaryProductResult> SuspendProductAsync(long id)
        => RunAsync(scope => scope.SuspendProduct.SuspendAsync(new BackofficeSuspendAncillaryProductCommand(id)));

    public Task<AncillaryProductResult> RetireProductAsync(long id)
        => RunAsync(scope => scope.RetireProduct.RetireAsync(new BackofficeRetireAncillaryProductCommand(id)));

    public Task<AncillaryProductResult> ReviseProductAsync(long id)
        => RunAsync(scope => scope.ReviseProduct.ReviseAsync(new BackofficeReviseAncillaryProductCommand(id)));

    public Task<BackofficeAncillaryProductDto> GetProductAsync(long id)
        => RunAsync(scope => scope.GetProductById.ExecuteAsync(id));

    public Task<AncillaryPriceRuleResult> DefineAsync(BackofficeDefineAncillaryPriceRuleCommand command)
        => RunAsync(scope => scope.DefinePriceRule.DefineAsync(command));

    public Task<AncillaryPriceRuleResult> ChangeAsync(BackofficeChangeAncillaryPriceRuleCommand command)
        => RunAsync(scope => scope.ChangePriceRule.ChangeAsync(command));

    public Task<AncillaryPriceRuleResult> ActivatePriceRuleAsync(long id)
        => RunAsync(scope => scope.ActivatePriceRule.ActivateAsync(new BackofficeActivateAncillaryPriceRuleCommand(id)));

    public Task<AncillaryPriceRuleResult> SuspendPriceRuleAsync(long id)
        => RunAsync(scope => scope.SuspendPriceRule.SuspendAsync(new BackofficeSuspendAncillaryPriceRuleCommand(id)));

    public Task<AncillaryPriceRuleResult> RetirePriceRuleAsync(long id)
        => RunAsync(scope => scope.RetirePriceRule.RetireAsync(new BackofficeRetireAncillaryPriceRuleCommand(id)));

    public Task<BackofficeAncillaryPriceRuleDto> GetPriceRuleAsync(long id)
        => RunAsync(scope => scope.GetPriceRuleById.ExecuteAsync(id));

    public Task<AncillaryQuoteDto> QuoteAsync(ServiceGetAncillaryQuoteQuery query)
        => RunAsync(scope => scope.GetQuote.ExecuteAsync(query));

    public async Task<AncillaryProductResult> ArrangeProductAsync(BackofficeDefineAncillaryProductCommand command, bool activate = true)
    {
        var product = await DefineAsync(command);

        return activate ? Published(await ActivateProductAsync(product.Id)) : product;
    }

    public async Task<AncillaryPriceRuleResult> ArrangePriceRuleAsync(BackofficeDefineAncillaryPriceRuleCommand command, bool activate = true)
    {
        var rule = await DefineAsync(command);

        return activate ? await ActivatePriceRuleAsync(rule.Id) : rule;
    }

    private static AncillaryProductResult Published(ActivateAncillaryProductResult result)
        => new(result.Id, result.OwnerAirlineId, result.ProductRef, result.Version, result.Status);
}
