using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.DefineAncillaryPriceRule;
using AeroTech.Messages.AirPrice.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Xunit;

namespace AeroTech.Ancillary.Application.AcceptanceTests.PublishedReadModel;

[Collection(DatabaseCollection.Name)]
public sealed class Phase2BPublishedReadModelTests(TestDatabase database) : IAsyncLifetime
{
    private const string MigrationBeforePhase2B = "20261002213503_AddLoungeDetail";

    private static readonly string[] ProductColumns =
    [
        "Id", "OwnerAirlineId", "ProductRef", "Version", "Type", "Name", "Description", "SalesScope", "QuantityUnit", "QuantityMin",
        "QuantityMax", "DocumentType", "Rfic", "Rfisc", "ServiceTypeCode", "GroupCode", "SubGroupCode", "Description1Code",
        "Description2Code", "Refundable", "Commissionable", "Reusable", "FormOfRefundCode", "InterlineSettlementAllowed",
        "InventoryControl", "BaggagePieces", "BaggageWeight", "BaggageWeightUnit", "LoungeAirportIds", "Status", "LastUpdateTime"
    ];

    private static readonly string[] PriceRuleColumns =
    [
        "Id", "OwnerAirlineId", "ProductRef", "Priority", "CurrencyId", "SalesFrom", "SalesTo", "TravelFrom", "TravelTo",
        "PassengerTypes", "OriginAirportIds", "DestinationAirportIds", "Status", "LastUpdateTime"
    ];

    private static readonly string[] PriceLineColumns = ["Id", "AncillaryPriceRuleId", "Category", "Code", "Name", "Amount"];

    private readonly AncillaryHarness _harness = new(database);

    private int Airline => _harness.AirlineId;

    public async Task InitializeAsync()
    {
        await _harness.RegisterAsync(Phase1Commands.SubCodeS(Airline));
        await _harness.RegisterAsync(Phase2Commands.SubCodeGL(Airline));
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task P2B_M01_ReadModelTablesHaveEveryPublishedColumn()
    {
        var columns = await _harness.RunAsync(scope => scope.Query.Database
            .SqlQuery<string>($"SELECT TABLE_NAME + '.' + COLUMN_NAME AS [Value] FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'ReadModel'")
            .ToListAsync());

        var published = ProductColumns.Select(column => $"AncillaryProducts.{column}")
            .Concat(PriceRuleColumns.Select(column => $"AncillaryPriceRules.{column}"))
            .Concat(PriceLineColumns.Select(column => $"PriceLines.{column}"));

        Assert.All(published, column => Assert.Contains(column, columns));
    }

    [Fact]
    public async Task P2B_M02_EveryProductOperationStampsTheRowsItWrites()
    {
        var at = Tick();
        var first = await _harness.DefineAsync(Phase1Commands.ProductX(Airline));

        Assert.Equal(at, await ProductStampAsync(first.Id));

        at = Tick();
        await _harness.ChangeAsync(Phase1Commands.ChangeTo(first.Id, Phase1Commands.ProductX(Airline) with { Name = "First extra bag" }));

        Assert.Equal(at, await ProductStampAsync(first.Id));

        at = Tick();
        await _harness.ActivateProductAsync(first.Id);

        Assert.Equal(at, await ProductStampAsync(first.Id));

        at = Tick();
        await _harness.SuspendProductAsync(first.Id);

        Assert.Equal(at, await ProductStampAsync(first.Id));

        var suspendedAt = at;

        at = Tick();
        var second = await _harness.ReviseProductAsync(first.Id);

        Assert.Equal((suspendedAt, at), (await ProductStampAsync(first.Id), await ProductStampAsync(second.Id)));

        at = Tick();
        await _harness.ActivateProductAsync(second.Id);

        Assert.Equal((at, at), (await ProductStampAsync(first.Id), await ProductStampAsync(second.Id)));

        at = Tick();
        await _harness.RetireProductAsync(second.Id);

        Assert.Equal(at, await ProductStampAsync(second.Id));
    }

    [Fact]
    public async Task P2B_M03_EveryPriceRuleOperationStampsItsRow()
    {
        await _harness.ArrangeProductAsync(Phase1Commands.ProductX(Airline));

        var at = Tick();
        var rule = await _harness.DefineAsync(Phase1Commands.RuleR(Airline));

        Assert.Equal(at, await PriceRuleStampAsync(rule.Id));

        at = Tick();
        await _harness.ChangeAsync(Phase1Commands.ChangeTo(rule.Id, Phase1Commands.RuleR(Airline) with { Lines = [Phase1Commands.Ancillary(36.00m)] }));

        Assert.Equal(at, await PriceRuleStampAsync(rule.Id));

        at = Tick();
        await _harness.ActivatePriceRuleAsync(rule.Id);

        Assert.Equal(at, await PriceRuleStampAsync(rule.Id));

        at = Tick();
        await _harness.SuspendPriceRuleAsync(rule.Id);

        Assert.Equal(at, await PriceRuleStampAsync(rule.Id));

        at = Tick();
        await _harness.RetirePriceRuleAsync(rule.Id);

        Assert.Equal(at, await PriceRuleStampAsync(rule.Id));
    }

    [Fact]
    public async Task P2B_M04_StampChangesAfterEveryOperation()
    {
        _harness.Clock.Now = new DateTimeOffset(2100, 1, 1, 0, 0, 0, TimeSpan.Zero);

        var stamps = new List<string> { await StampAsync() };

        async Task StepAsync(Func<Task> operation)
        {
            Tick();
            await operation();
            stamps.Add(await StampAsync());
        }

        long productId = 0;
        long revisionId = 0;
        long ruleId = 0;

        await StepAsync(async () => productId = (await _harness.DefineAsync(Phase1Commands.ProductX(Airline))).Id);
        await StepAsync(() => _harness.ChangeAsync(Phase1Commands.ChangeTo(productId, Phase1Commands.ProductX(Airline) with { Name = "First extra bag" })));
        await StepAsync(() => _harness.ActivateProductAsync(productId));
        await StepAsync(() => _harness.SuspendProductAsync(productId));
        await StepAsync(async () => revisionId = (await _harness.ReviseProductAsync(productId)).Id);
        await StepAsync(() => _harness.ActivateProductAsync(revisionId));
        await StepAsync(async () => ruleId = (await _harness.DefineAsync(Phase1Commands.RuleR(Airline))).Id);
        await StepAsync(() => _harness.ChangeAsync(Phase1Commands.ChangeTo(ruleId, Phase1Commands.RuleR(Airline) with { Lines = [Phase1Commands.Ancillary(36.00m)] })));
        await StepAsync(() => _harness.ActivatePriceRuleAsync(ruleId));
        await StepAsync(() => _harness.SuspendPriceRuleAsync(ruleId));
        await StepAsync(() => _harness.RetirePriceRuleAsync(ruleId));
        await StepAsync(() => _harness.RetireProductAsync(revisionId));

        Assert.Equal(13, stamps.Count);
        Assert.Equal(stamps.Count, stamps.Distinct().Count());
    }

    [Fact]
    public async Task P2B_M05_ListsAreJsonArraysOfNumbersAndEnumeratedColumnsAreNumeric()
    {
        var bag = await _harness.ArrangeProductAsync(Phase1Commands.ProductX(Airline));
        var lounge = await _harness.ArrangeProductAsync(Phase2Commands.ProductLG(Airline));
        var restricted = await _harness.ArrangePriceRuleAsync(Phase1Commands.RuleR(Airline) with
        {
            Conditions = new PriceRuleConditionsInput([PassengerTypeCode.ADT, PassengerTypeCode.CHD], [1, 5], null)
        });

        Assert.Equal("[1,5]", await ValueAsync($"SELECT LoungeAirportIds AS [Value] FROM ReadModel.AncillaryProducts WHERE Id = {lounge.Id}"));
        Assert.Null(await ValueAsync($"SELECT LoungeAirportIds AS [Value] FROM ReadModel.AncillaryProducts WHERE Id = {bag.Id}"));
        Assert.Equal(
            $"[{(int)PassengerTypeCode.ADT},{(int)PassengerTypeCode.CHD}]",
            await ValueAsync($"SELECT PassengerTypes AS [Value] FROM ReadModel.AncillaryPriceRules WHERE Id = {restricted.Id}"));
        Assert.Equal("[1,5]", await ValueAsync($"SELECT OriginAirportIds AS [Value] FROM ReadModel.AncillaryPriceRules WHERE Id = {restricted.Id}"));
        Assert.Null(await ValueAsync($"SELECT DestinationAirportIds AS [Value] FROM ReadModel.AncillaryPriceRules WHERE Id = {restricted.Id}"));

        Assert.Equal(
            "1,1,1,2,1,1,2",
            await ValueAsync($"SELECT CONCAT(Type, ',', SalesScope, ',', QuantityUnit, ',', DocumentType, ',', InventoryControl, ',', BaggageWeightUnit, ',', Status) AS [Value] FROM ReadModel.AncillaryProducts WHERE Id = {bag.Id}"));
        Assert.Equal(
            "2,2,3,3,1,,2",
            await ValueAsync($"SELECT CONCAT(Type, ',', SalesScope, ',', QuantityUnit, ',', DocumentType, ',', InventoryControl, ',', BaggageWeightUnit, ',', Status) AS [Value] FROM ReadModel.AncillaryProducts WHERE Id = {lounge.Id}"));
        Assert.Equal("2", await ValueAsync($"SELECT CONCAT(Status, '') AS [Value] FROM ReadModel.AncillaryPriceRules WHERE Id = {restricted.Id}"));
        Assert.Equal("1,2", await ValueAsync($"SELECT STRING_AGG(CONCAT(Category, ''), ',') WITHIN GROUP (ORDER BY Id) AS [Value] FROM ReadModel.PriceLines WHERE AncillaryPriceRuleId = {restricted.Id}"));

        var enumeratedTypes = await _harness.RunAsync(scope => scope.Query.Database
            .SqlQuery<string>($"""
                SELECT DISTINCT DATA_TYPE AS [Value] FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_SCHEMA = 'ReadModel'
                  AND ((TABLE_NAME = 'AncillaryProducts' AND COLUMN_NAME IN ('Type', 'SalesScope', 'QuantityUnit', 'DocumentType', 'InventoryControl', 'BaggageWeightUnit', 'Status'))
                    OR (TABLE_NAME = 'AncillaryPriceRules' AND COLUMN_NAME = 'Status')
                    OR (TABLE_NAME = 'PriceLines' AND COLUMN_NAME = 'Category'))
                """)
            .ToListAsync());

        Assert.Equal(["int"], enumeratedTypes);
    }

    [Fact]
    public async Task P2B_M06_MigrationStampsExistingRowsWithTheirCreationTime()
    {
        await using var context = database.NewScratchQueryContext("m06");

        try
        {
            await context.GetService<IMigrator>().MigrateAsync(MigrationBeforePhase2B);
            await context.Database.ExecuteSqlRawAsync("""
                INSERT INTO ReadModel.AncillaryProducts
                    (Id, OwnerAirlineId, ProductRef, Version, Type, Name, SalesScope, QuantityUnit, QuantityMin, QuantityMax, DocumentType,
                     ServiceTypeCode, GroupCode, Refundable, InventoryControl, Status, CreatedAt)
                VALUES (1, 10, 'XBAG1', 1, 1, 'First extra bag 23kg', 1, 1, 1, 1, 2, 'C', 'BG', 0, 1, 2, '2026-10-02T21:39:06.6455194+00:00');
                INSERT INTO ReadModel.AncillaryPriceRules (Id, OwnerAirlineId, ProductRef, Priority, CurrencyId, Status, CreatedAt)
                VALUES (2, 10, 'XBAG1', 1, 978, 2, '2026-10-02T23:39:07.3559340+03:30');
                """);

            await context.Database.MigrateAsync();

            var stamps = await context.Database
                .SqlQuery<string>($"""
                    SELECT CONCAT('product ', Id, ': ', IIF(LastUpdateTime = CreatedAt AND DATEPART(TZOFFSET, LastUpdateTime) = DATEPART(TZOFFSET, CreatedAt), 'CreatedAt', 'other')) AS [Value]
                    FROM ReadModel.AncillaryProducts
                    UNION ALL
                    SELECT CONCAT('rule ', Id, ': ', IIF(LastUpdateTime = CreatedAt AND DATEPART(TZOFFSET, LastUpdateTime) = DATEPART(TZOFFSET, CreatedAt), 'CreatedAt', 'other'))
                    FROM ReadModel.AncillaryPriceRules
                    """)
                .ToListAsync();

            Assert.Equal(["product 1: CreatedAt", "rule 2: CreatedAt"], stamps.Order(StringComparer.Ordinal));
        }
        finally
        {
            await context.Database.EnsureDeletedAsync();
        }
    }

    private DateTimeOffset Tick() => _harness.Clock.Now = _harness.Clock.Now.AddMinutes(1);

    private Task<DateTimeOffset> ProductStampAsync(long id)
        => _harness.RunAsync(scope => scope.Query.AncillaryProducts.Where(row => row.Id == id).Select(row => row.LastUpdateTime).SingleAsync());

    private Task<DateTimeOffset> PriceRuleStampAsync(long id)
        => _harness.RunAsync(scope => scope.Query.AncillaryPriceRules.Where(row => row.Id == id).Select(row => row.LastUpdateTime).SingleAsync());

    private Task<string> StampAsync()
        => _harness.RunAsync(scope => scope.Query.Database
            .SqlQuery<string>($"""
                SELECT CONCAT(CONVERT(nvarchar(40), MAX(LastUpdateTime), 127), '|', COUNT(*)) AS [Value]
                FROM (SELECT LastUpdateTime FROM ReadModel.AncillaryProducts UNION ALL SELECT LastUpdateTime FROM ReadModel.AncillaryPriceRules) AS rows
                """)
            .SingleAsync());

    private Task<string?> ValueAsync(FormattableString sql)
        => _harness.RunAsync(scope => scope.Query.Database.SqlQuery<string?>(sql).SingleAsync());
}
