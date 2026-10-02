using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.DefineAncillaryPriceRule;
using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.DefineAncillaryProduct;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AeroTech.Ancillary.Application.AcceptanceTests.AncillaryQuotes;

[Collection(DatabaseCollection.Name)]
public sealed class Phase2AncillaryQuoteAcceptanceTests(TestDatabase database) : IAsyncLifetime
{
    private readonly AncillaryHarness _harness = new(database);

    private AncillaryProductResult _lounge = null!;
    private AncillaryPriceRuleResult _loungeRule = null!;

    private int Airline => _harness.AirlineId;

    public async Task InitializeAsync()
    {
        await _harness.RegisterAsync(Phase1Commands.SubCodeS(Airline));
        await _harness.RegisterAsync(Phase2Commands.SubCodeL(Airline));
        await _harness.ArrangeProductAsync(Phase1Commands.ProductX(Airline));
        await _harness.ArrangePriceRuleAsync(Phase1Commands.RuleR(Airline));

        _lounge = await _harness.ArrangeProductAsync(Phase2Commands.ProductL(Airline));
        _loungeRule = await _harness.ArrangePriceRuleAsync(Phase2Commands.RuleRL(Airline));
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task P2_Q04_StoredProductsAreQuotedInTheDocumentedOrder()
    {
        await _harness.RegisterAsync(Phase2Commands.SubCodeGL(Airline));
        await _harness.ArrangeProductAsync(Phase2Commands.ProductLG(Airline));
        await _harness.ArrangePriceRuleAsync(Phase2Commands.RuleRLG(Airline));

        var items = (await _harness.QuoteAsync(Phase2Commands.Golden(Airline))).Items;

        Assert.Equal(
            [("XBAG1", "B1", null), ("LNGTHR", "B1", "F1"), ("LNGXLG", "B1", "F1"), ("XBAG1", "B2", null)],
            items.Select(item => (item.ProductRef, item.BoundRef, item.FlightRef)));
        Assert.Equal([null, [1], [1, 5], null], items.Select(item => item.Lounge?.AirportIds));
    }

    [Fact]
    public async Task P2_Q15_LoungeSelectionMustStillBeCurrent()
    {
        var current = Phase2Commands.LoungeSelection("LNGTHR", 1, _loungeRule.Id);

        Assert.Equal(20.00m, Assert.Single((await _harness.QuoteAsync(Phase2Commands.Golden(Airline, current))).Items).Total);

        var revised = await _harness.ReviseProductAsync(_lounge.Id);

        await _harness.ActivateProductAsync(revised.Id);

        await BusinessAssert.ThrowsAsync(16309, 409, () => _harness.QuoteAsync(Phase2Commands.Golden(Airline, current)));
        Assert.Single((await _harness.QuoteAsync(Phase2Commands.Golden(Airline, current with { ProductVersion = 2 }))).Items);

        await _harness.RetirePriceRuleAsync(_loungeRule.Id);

        var replaced = await _harness.ArrangePriceRuleAsync(Phase2Commands.RuleRL(Airline) with { Lines = [Phase1Commands.Ancillary(25.00m)] });

        await BusinessAssert.ThrowsAsync(16309, 409, () => _harness.QuoteAsync(Phase2Commands.Golden(Airline, current with { ProductVersion = 2 })));
        Assert.Equal(
            25.00m,
            Assert.Single((await _harness.QuoteAsync(Phase2Commands.Golden(Airline, current with { ProductVersion = 2, PriceRuleId = replaced.Id }))).Items).Total);
    }

    [Fact]
    public async Task P2_Q16_QuoteChangesNoDataAndPublishesNothing()
    {
        var before = await SnapshotAsync();

        Assert.Equal(3, (await _harness.QuoteAsync(Phase2Commands.Golden(Airline))).Items.Count);
        Assert.Single((await _harness.QuoteAsync(Phase2Commands.Golden(Airline, Phase2Commands.LoungeSelection("LNGTHR", 1, _loungeRule.Id)))).Items);
        await BusinessAssert.ThrowsAsync(16305, 422, () => _harness.QuoteAsync(Phase2Commands.Golden(Airline, Phase2Commands.LoungeSelection("LNGTHR", 1, _loungeRule.Id, "F2"))));

        Assert.Equal(before, await SnapshotAsync());
        Assert.Equal(0, await _harness.RunAsync(scope => scope.Command.OutboxMessages.CountAsync()));
    }

    private Task<string> SnapshotAsync()
        => _harness.RunAsync(async scope =>
        {
            var products = await scope.Command.AncillaryProducts.AsNoTracking().OrderBy(row => row.Id).ToListAsync();
            var rules = await scope.Command.AncillaryPriceRules.AsNoTracking().Include(row => row.Lines).OrderBy(row => row.Id).ToListAsync();
            var subCodes = await scope.Command.ServiceSubCodes.AsNoTracking().OrderBy(row => row.Id).ToListAsync();
            var readProducts = await scope.Query.AncillaryProducts.AsNoTracking().OrderBy(row => row.Id).ToListAsync();
            var readRules = await scope.Query.AncillaryPriceRules.AsNoTracking().OrderBy(row => row.Id).Select(row => $"{row.Id}:{row.Status}:{row.Priority}").ToListAsync();
            var outbox = await scope.Command.OutboxMessages.CountAsync();

            return string.Join(
                '|',
                products.Select(row => $"{row.Id}:{row.Status}:{row.LastUpdateTime:O}:{Convert.ToHexString(row.RowVersion)}")
                    .Concat(rules.Select(row => $"{row.Id}:{row.Status}:{row.Lines.Count}:{Convert.ToHexString(row.RowVersion)}"))
                    .Concat(subCodes.Select(row => $"{row.Id}:{row.Status}:{Convert.ToHexString(row.RowVersion)}"))
                    .Concat(readProducts.Select(row => $"{row.Id}:{row.Status}:{string.Join(',', row.LoungeAirportIds ?? [])}"))
                    .Concat(readRules)
                    .Append($"outbox:{outbox}"));
        });
}
