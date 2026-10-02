using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Query.AncillaryQuote.Dto;
using AeroTech.Ancillary.Query.AncillaryQuote.Queries.GetAncillaryQuote;
using AeroTech.Ancillary.Query.AncillaryQuote.Queries.GetAncillaryQuote.Service;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AeroTech.Ancillary.Application.AcceptanceTests.AncillaryQuotes;

[Collection(DatabaseCollection.Name)]
public sealed class Phase1AncillaryQuoteAcceptanceTests(TestDatabase database) : IAsyncLifetime
{
    private readonly AncillaryHarness _harness = new(database);

    private int Airline => _harness.AirlineId;

    public async Task InitializeAsync() => await _harness.RegisterAsync(Phase1Commands.SubCodeS(Airline));

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task P1_Q12_OnlyActiveProductsAndRulesAreOffered()
    {
        var product = await _harness.DefineAsync(Phase1Commands.ProductX(Airline));
        var rule = await _harness.DefineAsync(Phase1Commands.RuleR(Airline));

        Assert.Empty(await CatalogueAsync());

        await _harness.ActivateProductAsync(product.Id);

        Assert.Empty(await CatalogueAsync());

        await _harness.ActivatePriceRuleAsync(rule.Id);

        Assert.Single(await CatalogueAsync());

        await _harness.SuspendPriceRuleAsync(rule.Id);

        Assert.Empty(await CatalogueAsync());

        await _harness.ActivatePriceRuleAsync(rule.Id);
        await _harness.SuspendProductAsync(product.Id);

        Assert.Empty(await CatalogueAsync());

        await _harness.ActivateProductAsync(product.Id);

        Assert.Single(await CatalogueAsync());

        await _harness.RetirePriceRuleAsync(rule.Id);

        Assert.Empty(await CatalogueAsync());

        await _harness.ArrangePriceRuleAsync(Phase1Commands.RuleR(Airline));

        Assert.Single(await CatalogueAsync());

        await _harness.RetireProductAsync(product.Id);

        Assert.Empty(await CatalogueAsync());
    }

    [Fact]
    public async Task P1_Q13_ActivatedRevisionReplacesTheOfferedVersion()
    {
        var first = await _harness.ArrangeProductAsync(Phase1Commands.ProductX(Airline));
        var rule = await _harness.ArrangePriceRuleAsync(Phase1Commands.RuleR(Airline));
        var second = await _harness.ReviseProductAsync(first.Id);

        await _harness.ChangeAsync(Phase1Commands.ChangeTo(second.Id, Phase1Commands.ProductX(Airline) with { Name = "First extra bag" }));
        await _harness.ActivateProductAsync(second.Id);

        var item = Assert.Single(await CatalogueAsync());

        Assert.Equal((2, "First extra bag", rule.Id), (item.ProductVersion, item.Name, item.PriceRuleId));
        Assert.Equal("First extra bag", item.PriceLines[0].Name);
        await BusinessAssert.ThrowsAsync(16309, 409, () => _harness.QuoteAsync(Phase1Commands.Golden(Airline, Phase1Commands.Selection("XBAG1", 1, rule.Id))));
        Assert.Single((await _harness.QuoteAsync(Phase1Commands.Golden(Airline, Phase1Commands.Selection("XBAG1", 2, rule.Id)))).Items);
    }

    [Fact]
    public async Task P1_Q14_ReplacedRuleMakesAnEarlierSelectionStale()
    {
        await _harness.ArrangeProductAsync(Phase1Commands.ProductX(Airline));

        var old = await _harness.ArrangePriceRuleAsync(Phase1Commands.RuleR(Airline));

        await _harness.RetirePriceRuleAsync(old.Id);

        var current = await _harness.ArrangePriceRuleAsync(Phase1Commands.RuleR(Airline) with { Lines = [Phase1Commands.Ancillary(40.00m)] });

        await BusinessAssert.ThrowsAsync(16309, 409, () => _harness.QuoteAsync(Phase1Commands.Golden(Airline, Phase1Commands.Selection("XBAG1", 1, old.Id))));

        var item = Assert.Single((await _harness.QuoteAsync(Phase1Commands.Golden(Airline, Phase1Commands.Selection("XBAG1", 1, current.Id)))).Items);

        Assert.Equal((current.Id, 40.00m), (item.PriceRuleId, item.Total));
        Assert.Equal(current.Id, Assert.Single(await CatalogueAsync()).PriceRuleId);
    }

    [Fact]
    public void P1_Q20_QuantityZeroIsMalformed()
    {
        var validator = new ServiceGetAncillaryQuoteQueryValidator();

        ValidationAssert.Accepts(validator, Phase1Commands.Golden(Airline, Phase1Commands.Selection("XBAG1", 1, 1)));
        ValidationAssert.Rejects(validator, Phase1Commands.Golden(Airline, Phase1Commands.Selection("XBAG1", 1, 1, quantity: 0)));
        ValidationAssert.Rejects(
            validator,
            Phase1Commands.Golden(Airline) with { Existing = [new QuoteExistingOccurrence("XBAG1", "T1", "B1", null, 0)] });
    }

    [Fact]
    public async Task P1_Q24_QuoteChangesNoDataAndPublishesNothing()
    {
        await _harness.ArrangeProductAsync(Phase1Commands.ProductX(Airline));

        var rule = await _harness.ArrangePriceRuleAsync(Phase1Commands.RuleR(Airline));
        var before = await SnapshotAsync();

        Assert.Single(await CatalogueAsync());
        Assert.Single((await _harness.QuoteAsync(Phase1Commands.Golden(Airline, Phase1Commands.Selection("XBAG1", 1, rule.Id)))).Items);
        await BusinessAssert.ThrowsAsync(16306, 422, () => _harness.QuoteAsync(Phase1Commands.Golden(Airline, Phase1Commands.Selection("XBAG1", 1, rule.Id, quantity: 2))));

        Assert.Equal(before, await SnapshotAsync());
        Assert.Equal(0, await _harness.RunAsync(scope => scope.Command.OutboxMessages.CountAsync()));
    }

    private async Task<IReadOnlyList<AncillaryQuoteItemDto>> CatalogueAsync()
        => (await _harness.QuoteAsync(Phase1Commands.Golden(Airline))).Items;

    private Task<string> SnapshotAsync()
        => _harness.RunAsync(async scope =>
        {
            var subCodes = await scope.Command.ServiceSubCodes.AsNoTracking().OrderBy(row => row.Id).ToListAsync();
            var products = await scope.Command.AncillaryProducts.AsNoTracking().OrderBy(row => row.Id).ToListAsync();
            var rules = await scope.Command.AncillaryPriceRules.AsNoTracking().Include(row => row.Lines).OrderBy(row => row.Id).ToListAsync();
            var readSubCodes = await scope.Query.ServiceSubCodes.AsNoTracking().OrderBy(row => row.Id).Select(row => $"{row.Id}:{row.Status}").ToListAsync();
            var readProducts = await scope.Query.AncillaryProducts.AsNoTracking().OrderBy(row => row.Id).Select(row => $"{row.Id}:{row.Status}:{row.Name}").ToListAsync();
            var readRules = await scope.Query.AncillaryPriceRules.AsNoTracking().OrderBy(row => row.Id).Select(row => $"{row.Id}:{row.Status}:{row.Priority}").ToListAsync();
            var readLines = await scope.Query.PriceLines.AsNoTracking().OrderBy(row => row.Id).Select(row => $"{row.Id}:{row.Amount}").ToListAsync();
            var outbox = await scope.Command.OutboxMessages.CountAsync();
            var inbox = await scope.Command.InboxMessages.CountAsync();

            return string.Join(
                '|',
                subCodes.Select(row => $"{row.Id}:{row.Status}:{Convert.ToHexString(row.RowVersion)}")
                    .Concat(products.Select(row => $"{row.Id}:{row.Status}:{row.LastUpdateTime:O}:{Convert.ToHexString(row.RowVersion)}"))
                    .Concat(rules.Select(row => $"{row.Id}:{row.Status}:{row.Lines.Count}:{Convert.ToHexString(row.RowVersion)}"))
                    .Concat(readSubCodes)
                    .Concat(readProducts)
                    .Concat(readRules)
                    .Concat(readLines)
                    .Append($"outbox:{outbox}")
                    .Append($"inbox:{inbox}"));
        });
}
