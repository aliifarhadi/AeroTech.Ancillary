using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.DefineAncillaryPriceRule;
using AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.DefineAncillaryPriceRule.Backoffice;
using AeroTech.Ancillary.Domain.AncillaryPriceRuleAggregate;
using AeroTech.Ancillary.Domain.AncillaryPriceRuleAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryPriceRuleAggregate.Entities;
using AeroTech.Ancillary.Domain.AncillaryPriceRuleAggregate.ValueObjects;
using AeroTech.Ancillary.Query.AncillaryPriceRuleAggregate.Dto;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AeroTech.Ancillary.Application.AcceptanceTests.AncillaryPriceRules;

[Collection(DatabaseCollection.Name)]
public sealed class Phase1AncillaryPriceRuleAcceptanceTests(TestDatabase database) : IAsyncLifetime
{
    private readonly AncillaryHarness _harness = new(database);

    private int Airline => _harness.AirlineId;

    public async Task InitializeAsync()
    {
        await _harness.RegisterAsync(Phase1Commands.SubCodeS(Airline));
        await _harness.DefineAsync(Phase1Commands.ProductX(Airline));
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task P1_R01_RuleIsDefinedAsADraftForAProductInAnyStatus()
    {
        var command = Phase1Commands.RuleR(Airline) with
        {
            SalesFrom = Phase1Commands.Instant("2026-10-01T00:00:00+00:00"),
            SalesTo = Phase1Commands.Instant("2026-11-01T00:00:00+03:30"),
            TravelFrom = new DateOnly(2026, 10, 10),
            TravelTo = new DateOnly(2026, 12, 31),
            Conditions = new PriceRuleConditionsInput([PassengerTypeCode.ADT, PassengerTypeCode.CHD], [1], [2, 3])
        };

        var result = await _harness.DefineAsync(command);

        Assert.Equal(
            (Airline, "XBAG1", 1, 978, AncillaryPriceRuleStatus.Draft),
            (result.OwnerAirlineId, result.ProductRef, result.Priority, result.CurrencyId, result.Status));

        var rule = await _harness.GetPriceRuleAsync(result.Id);

        Assert.Equal((result.Id, Airline, "XBAG1", 1, 978, "Draft"), (rule.Id, rule.OwnerAirlineId, rule.ProductRef, rule.Priority, rule.CurrencyId, rule.Status.Name));
        Assert.Equal((command.SalesFrom, command.SalesTo, command.TravelFrom, command.TravelTo), (rule.SalesFrom, rule.SalesTo, rule.TravelFrom, rule.TravelTo));
        Assert.Equal(["ADT", "CHD"], rule.Conditions.PassengerTypes!.Select(passengerType => passengerType.Name));
        Assert.Equal([1], rule.Conditions.OriginAirportIds);
        Assert.Equal([2, 3], rule.Conditions.DestinationAirportIds);
        Assert.Equal(_harness.Clock.Now, rule.CreatedAt);
        Assert.Equal([("Ancillary", null, null, 35.00m), ("Tax", "VAT", "Value added tax", 3.50m)], Lines(rule));

        var reversed = await _harness.DefineAsync(Phase1Commands.RuleR(Airline) with
        {
            Priority = 2,
            Lines = [Phase1Commands.Tax("VAT", 3.50m), Phase1Commands.Ancillary(35.00m, "Bag fee")]
        });
        var unrestricted = await _harness.GetPriceRuleAsync(reversed.Id);

        Assert.Equal([("Tax", "VAT", null, 3.50m), ("Ancillary", null, "Bag fee", 35.00m)], Lines(unrestricted));
        Assert.Equal((null, null, null), (unrestricted.Conditions.PassengerTypes, unrestricted.Conditions.OriginAirportIds, unrestricted.Conditions.DestinationAirportIds));

        var product = await _harness.RunAsync(scope => scope.Command.AncillaryProducts.SingleAsync(row => row.OwnerAirlineId == Airline));

        await _harness.RetireProductAsync(product.Id);

        Assert.Equal(AncillaryPriceRuleStatus.Draft, (await _harness.DefineAsync(Phase1Commands.RuleR(Airline) with { Priority = 3 })).Status);
    }

    [Fact]
    public async Task P1_R02_RuleForAProductThatDoesNotExistIsRefused()
    {
        await BusinessAssert.ThrowsAsync(16205, 422, () => _harness.DefineAsync(Phase1Commands.RuleR(Airline) with { ProductRef = "NOSUCH" }));
        await BusinessAssert.ThrowsAsync(16205, 422, () => _harness.DefineAsync(Phase1Commands.RuleR(database.NextAirlineId())));
    }

    [Fact]
    public async Task P1_R03_ExactlyOneAncillaryLineAndDistinctTaxCodes()
    {
        await InvalidAsync(Phase1Commands.RuleR(Airline) with { Lines = [Phase1Commands.Tax("VAT", 3.50m)] });
        await InvalidAsync(Phase1Commands.RuleR(Airline) with { Lines = [Phase1Commands.Ancillary(35m), Phase1Commands.Ancillary(5m)] });
        await InvalidAsync(Phase1Commands.RuleR(Airline) with { Lines = [Phase1Commands.Ancillary(35m), Phase1Commands.Tax(null, 3.50m)] });
        await InvalidAsync(Phase1Commands.RuleR(Airline) with { Lines = [Phase1Commands.Ancillary(35m), Phase1Commands.Tax("VAT", 3.50m), Phase1Commands.Tax("VAT", 1m)] });

        var result = await _harness.DefineAsync(Phase1Commands.RuleR(Airline) with { Lines = [Phase1Commands.Ancillary(35m)] });

        Assert.Equal([("Ancillary", null, null, 35m)], Lines(await _harness.GetPriceRuleAsync(result.Id)));
    }

    [Fact]
    public async Task P1_R04_AmountsPriorityWindowsAndConditionsAreChecked()
    {
        var instant = Phase1Commands.AsOf;
        var validator = new BackofficeDefineAncillaryPriceRuleCommandValidator();
        var invalid = new[]
        {
            Phase1Commands.RuleR(Airline) with { Lines = [Phase1Commands.Ancillary(0m)] },
            Phase1Commands.RuleR(Airline) with { Lines = [Phase1Commands.Ancillary(-35m)] },
            Phase1Commands.RuleR(Airline) with { Lines = [Phase1Commands.Ancillary(35m), Phase1Commands.Tax("VAT", 0m)] },
            Phase1Commands.RuleR(Airline) with { Lines = [Phase1Commands.Ancillary(35.005m)] },
            Phase1Commands.RuleR(Airline) with { Priority = 0 },
            Phase1Commands.RuleR(Airline) with { SalesFrom = instant, SalesTo = instant },
            Phase1Commands.RuleR(Airline) with { SalesFrom = instant.AddDays(1), SalesTo = instant },
            Phase1Commands.RuleR(Airline) with { TravelFrom = new DateOnly(2026, 10, 11), TravelTo = new DateOnly(2026, 10, 10) },
            Phase1Commands.RuleR(Airline) with { Conditions = new PriceRuleConditionsInput([], null, null) },
            Phase1Commands.RuleR(Airline) with { Conditions = new PriceRuleConditionsInput(null, [], null) },
            Phase1Commands.RuleR(Airline) with { Conditions = new PriceRuleConditionsInput(null, null, []) },
            Phase1Commands.RuleR(Airline) with { Conditions = new PriceRuleConditionsInput([PassengerTypeCode.ADT, PassengerTypeCode.ADT], null, null) },
            Phase1Commands.RuleR(Airline) with { Conditions = new PriceRuleConditionsInput(null, [1, 1], null) },
            Phase1Commands.RuleR(Airline) with { Conditions = new PriceRuleConditionsInput(null, null, [2, 2]) }
        };

        foreach (var command in invalid)
        {
            ValidationAssert.Accepts(validator, command);
            await InvalidAsync(command);
        }

        Assert.Empty(await _harness.RunAsync(scope => scope.Command.AncillaryPriceRules.Where(rule => rule.OwnerAirlineId == Airline).ToListAsync()));
    }

    [Fact]
    public async Task P1_R05_OnlyADraftRuleCanBeChanged()
    {
        var defined = await _harness.DefineAsync(Phase1Commands.RuleR(Airline));
        var change = Phase1Commands.RuleR(Airline) with
        {
            Priority = 4,
            CurrencyId = 840,
            Lines = [Phase1Commands.Ancillary(40m, "Bag fee")],
            TravelFrom = new DateOnly(2026, 11, 1),
            Conditions = new PriceRuleConditionsInput([PassengerTypeCode.CHD], null, null)
        };

        var changed = await _harness.ChangeAsync(Phase1Commands.ChangeTo(defined.Id, change));
        var rule = await _harness.GetPriceRuleAsync(defined.Id);

        Assert.Equal((defined.Id, Airline, "XBAG1", 4, 840, AncillaryPriceRuleStatus.Draft), (changed.Id, changed.OwnerAirlineId, changed.ProductRef, changed.Priority, changed.CurrencyId, changed.Status));
        Assert.Equal((4, 840, new DateOnly(2026, 11, 1), "Draft"), (rule.Priority, rule.CurrencyId, rule.TravelFrom, rule.Status.Name));
        Assert.Equal(["CHD"], rule.Conditions.PassengerTypes!.Select(passengerType => passengerType.Name));
        Assert.Equal([("Ancillary", null, "Bag fee", 40m)], Lines(rule));
        Assert.Equal(1, await _harness.RunAsync(scope => scope.Command.Set<PriceLine>().CountAsync(line => line.AncillaryPriceRuleId == defined.Id)));

        await _harness.ActivatePriceRuleAsync(defined.Id);
        await BusinessAssert.ThrowsAsync(16202, 409, () => _harness.ChangeAsync(Phase1Commands.ChangeTo(defined.Id, Phase1Commands.RuleR(Airline))));

        await _harness.SuspendPriceRuleAsync(defined.Id);
        await BusinessAssert.ThrowsAsync(16202, 409, () => _harness.ChangeAsync(Phase1Commands.ChangeTo(defined.Id, Phase1Commands.RuleR(Airline))));

        await _harness.RetirePriceRuleAsync(defined.Id);
        await BusinessAssert.ThrowsAsync(16202, 409, () => _harness.ChangeAsync(Phase1Commands.ChangeTo(defined.Id, Phase1Commands.RuleR(Airline))));

        Assert.Equal(4, (await _harness.GetPriceRuleAsync(defined.Id)).Priority);
    }

    [Fact]
    public async Task P1_R06_PriorityIsUniqueAmongActiveRulesOfAProductAndCurrency()
    {
        await _harness.ArrangePriceRuleAsync(Phase1Commands.RuleR(Airline));

        var samePriority = await _harness.DefineAsync(Phase1Commands.RuleR(Airline) with { Lines = [Phase1Commands.Ancillary(20m)] });

        await BusinessAssert.ThrowsAsync(16204, 409, () => _harness.ActivatePriceRuleAsync(samePriority.Id));
        Assert.Equal("Draft", (await _harness.GetPriceRuleAsync(samePriority.Id)).Status.Name);

        var otherCurrency = await _harness.DefineAsync(Phase1Commands.RuleR(Airline) with { CurrencyId = 840 });
        var otherPriority = await _harness.DefineAsync(Phase1Commands.RuleR(Airline) with { Priority = 2 });

        Assert.Equal(AncillaryPriceRuleStatus.Active, (await _harness.ActivatePriceRuleAsync(otherCurrency.Id)).Status);
        Assert.Equal(AncillaryPriceRuleStatus.Active, (await _harness.ActivatePriceRuleAsync(otherPriority.Id)).Status);
    }

    [Fact]
    public async Task P1_R06_UniqueIndexRejectsASecondActiveRuleWithTheSamePriority()
    {
        await _harness.ArrangePriceRuleAsync(Phase1Commands.RuleR(Airline));

        await using var scope = _harness.NewScope();

        var rule = AncillaryPriceRule.Define(
            database.Ids.NewId(),
            Airline,
            "XBAG1",
            1,
            Phase1Commands.CurrencyId,
            [new PriceLineArgs(AncillaryPriceLineCategory.Ancillary, null, null, 20m)],
            null,
            null,
            null,
            null,
            new PriceRuleConditions(null, null, null),
            database.Ids,
            _harness.Clock.Now);

        rule.Activate();
        await scope.PriceRules.AddAsync(rule);

        await Assert.ThrowsAsync<DbUpdateException>(() => scope.Command.SaveChangesAsync());
    }

    [Fact]
    public async Task P1_R07_SuspendedRuleFreesItsPriority()
    {
        var first = await _harness.ArrangePriceRuleAsync(Phase1Commands.RuleR(Airline));
        var second = await _harness.DefineAsync(Phase1Commands.RuleR(Airline) with { Lines = [Phase1Commands.Ancillary(20m)] });

        Assert.Equal(AncillaryPriceRuleStatus.Suspended, (await _harness.SuspendPriceRuleAsync(first.Id)).Status);
        Assert.Equal(AncillaryPriceRuleStatus.Active, (await _harness.ActivatePriceRuleAsync(second.Id)).Status);

        await BusinessAssert.ThrowsAsync(16204, 409, () => _harness.ActivatePriceRuleAsync(first.Id));
        Assert.Equal("Suspended", (await _harness.GetPriceRuleAsync(first.Id)).Status.Name);
    }

    [Fact]
    public async Task P1_R08_StatusChangesOutsideTheLifecycleAreRefused()
    {
        var draft = await _harness.DefineAsync(Phase1Commands.RuleR(Airline));

        await BusinessAssert.ThrowsAsync(16203, 409, () => _harness.SuspendPriceRuleAsync(draft.Id));

        await _harness.ActivatePriceRuleAsync(draft.Id);
        await BusinessAssert.ThrowsAsync(16203, 409, () => _harness.ActivatePriceRuleAsync(draft.Id));

        Assert.Equal(AncillaryPriceRuleStatus.Retired, (await _harness.RetirePriceRuleAsync(draft.Id)).Status);
        await BusinessAssert.ThrowsAsync(16203, 409, () => _harness.ActivatePriceRuleAsync(draft.Id));
        await BusinessAssert.ThrowsAsync(16203, 409, () => _harness.SuspendPriceRuleAsync(draft.Id));
        await BusinessAssert.ThrowsAsync(16203, 409, () => _harness.RetirePriceRuleAsync(draft.Id));

        var unknown = database.Ids.NewId();

        await BusinessAssert.ThrowsAsync(16201, 404, () => _harness.ChangeAsync(Phase1Commands.ChangeTo(unknown, Phase1Commands.RuleR(Airline))));
        await BusinessAssert.ThrowsAsync(16201, 404, () => _harness.ActivatePriceRuleAsync(unknown));
        await BusinessAssert.ThrowsAsync(16201, 404, () => _harness.SuspendPriceRuleAsync(unknown));
        await BusinessAssert.ThrowsAsync(16201, 404, () => _harness.RetirePriceRuleAsync(unknown));
        await BusinessAssert.ThrowsAsync(16201, 404, () => _harness.GetPriceRuleAsync(unknown));
    }

    private Task InvalidAsync(BackofficeDefineAncillaryPriceRuleCommand command)
        => BusinessAssert.ThrowsAsync(16206, 422, () => _harness.DefineAsync(command));

    private static IEnumerable<(string Category, string? Code, string? Name, decimal Amount)> Lines(BackofficeAncillaryPriceRuleDto rule)
        => rule.Lines.Select(line => (line.Category.Name, line.Code, line.Name, line.Amount));
}
