using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Queries.GetInventoryConfigurationSnapshot.Backoffice;
using AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Queries.GetInventoryPoliciesPaginated.Backoffice;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Xunit;
using static AeroTech.Ancillary.Application.AcceptanceTests.Migration.LegacySeeds;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Migration;

public class P2StockCapacityMigrationAcceptanceTests : IAsyncLifetime
{
    private const int LegacyAirline = 7;

    private const string InventoryTables =
        "SELECT s.name + '.' + t.name AS Value FROM sys.tables AS t JOIN sys.schemas AS s ON s.schema_id = t.schema_id " +
        "WHERE (s.name = 'Ancillary' AND t.name IN ('AncillaryInventoryPolicies', 'InventoryPassengerUsageLimits', 'FlightCountInventories', 'FlightCountAdjustments', " +
        "'FlightWeightInventories', 'FlightWeightAdjustments', 'AirportSlotInventories', 'AirportSlotAdjustments')) " +
        "OR (s.name = 'ReadModel' AND t.name IN ('AncillaryInventoryPolicies', 'AncillaryInventoryPassengerUsageLimits', 'FlightCountInventories', 'FlightCountAdjustments', " +
        "'FlightWeightInventories', 'FlightWeightAdjustments', 'AirportSlotInventories', 'AirportSlotAdjustments')) ORDER BY 1";

    private readonly TestDatabase _database = new();
    private readonly FixedClock _clock = new();
    private string[] _tablesBefore = [];
    private string[] _rowsBefore = [];

    public async Task InitializeAsync()
    {
        await MigrateAsync(LegacySeeds.V11Command, LegacySeeds.V11Query);

        await using (var seed = _database.NewContext(_clock))
            await seed.Database.ExecuteSqlRawAsync(V11);

        await MigrateAsync(LegacySeeds.V121Command, LegacySeeds.V121Query);

        _tablesBefore = (await RowsAsync(
                "SELECT '[' + s.name + '].[' + t.name + ']' AS Value FROM sys.tables AS t JOIN sys.schemas AS s ON s.schema_id = t.schema_id WHERE s.name IN ('Ancillary', 'ReadModel') ORDER BY 1"))
            .ToArray();
        _rowsBefore = await ContentAsync();

        await _database.InitializeAsync(StockCapacityCommand, StockCapacityQuery);
    }

    public Task DisposeAsync() => _database.DisposeAsync();

    private async Task MigrateAsync(string? command, string? query)
    {
        await using (var context = _database.NewContext(_clock))
            await context.GetService<IMigrator>().MigrateAsync(command);

        await using (var context = _database.NewQueryContext())
            await context.GetService<IMigrator>().MigrateAsync(query);
    }

    private async Task<List<string>> RowsAsync(string sql)
    {
        await using var context = _database.NewContext(_clock);

        return await context.Database.SqlQueryRaw<string>(sql).ToListAsync();
    }

    private async Task<string[]> ContentAsync()
    {
        var content = new List<string>();

        foreach (var table in _tablesBefore)
            content.AddRange(await RowsAsync($"SELECT CONCAT('{table} ', COUNT_BIG(*), ' ', ISNULL(CHECKSUM_AGG(BINARY_CHECKSUM(*)), 0)) AS Value FROM {table}"));

        return content.ToArray();
    }

    [Fact]
    public async Task P2_X09_a_v121_database_gains_only_empty_inventory_tables_and_no_policy_for_any_existing_definition()
    {
        Assert.Equal(85, _tablesBefore.Length);
        Assert.Equal(_rowsBefore, await ContentAsync());
        Assert.Contains(_rowsBefore, row => row.StartsWith("[Ancillary].[AncillaryProvisions] 7 ", StringComparison.Ordinal));
        Assert.Contains(StockCapacityCommand, await RowsAsync("SELECT [MigrationId] AS Value FROM [dbo].[__CommandsMigrationHistory]"));
        Assert.Contains(StockCapacityQuery, await RowsAsync("SELECT [MigrationId] AS Value FROM [dbo].[__QueriesMigrationHistory]"));

        var tables = await RowsAsync(InventoryTables);

        Assert.Equal(16, tables.Count);

        foreach (var table in tables)
            Assert.Equal("0", (await RowsAsync($"SELECT CAST(COUNT(*) AS varchar(10)) AS Value FROM {table}")).Single());

        Assert.Equal(
            new[] { "bigint|YES" },
            await RowsAsync("SELECT CONCAT(DATA_TYPE, '|', IS_NULLABLE) AS Value FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'Ancillary' AND TABLE_NAME = 'AncillaryReservationUnits' AND COLUMN_NAME = 'StockPoolId'"));

        var fixture = InventoryFixture.Connected(LegacyAirline);

        await using var scope = new InventoryScope(_database, _clock, fixture);

        foreach (var reference in new[] { "XBAG_PIECE_23KG", "SIM_CARD" })
        {
            var snapshot = await scope.GetInventoryConfigurationSnapshot.ExecuteAsync(
                new BackofficeGetInventoryConfigurationSnapshotQuery { ServiceDefinitionRef = reference, FlightId = 81234 });

            Assert.Equal(("NotConfigured", "PolicyNotConfigured", false), (snapshot.State.Name, snapshot.ReasonCode, snapshot.IsGuaranteed));
            Assert.Null(snapshot.Authority);
            Assert.Null(snapshot.ConfiguredCount);
        }

        Assert.Equal(0, (await scope.GetInventoryPoliciesPaginated.ExecuteAsync(new BackofficeGetInventoryPoliciesPaginatedQuery())).TotalCount);
        Assert.Equal(_rowsBefore, await ContentAsync());
    }

    [Fact]
    public async Task P2_X06_the_stock_capacity_migrations_are_reversible_and_leave_every_existing_row_untouched()
    {
        Assert.False(await PendingAsync());

        await MigrateAsync(LegacySeeds.V121Command, LegacySeeds.V121Query);

        Assert.Empty(await RowsAsync(InventoryTables));
        Assert.Equal(_rowsBefore, await ContentAsync());
        Assert.DoesNotContain(StockCapacityCommand, await RowsAsync("SELECT [MigrationId] AS Value FROM [dbo].[__CommandsMigrationHistory]"));
        Assert.DoesNotContain(StockCapacityQuery, await RowsAsync("SELECT [MigrationId] AS Value FROM [dbo].[__QueriesMigrationHistory]"));
        Assert.Equal(
            _tablesBefore,
            await RowsAsync("SELECT '[' + s.name + '].[' + t.name + ']' AS Value FROM sys.tables AS t JOIN sys.schemas AS s ON s.schema_id = t.schema_id WHERE s.name IN ('Ancillary', 'ReadModel') ORDER BY 1"));

        await MigrateAsync(StockCapacityCommand, StockCapacityQuery);

        Assert.Equal(16, (await RowsAsync(InventoryTables)).Count);
        Assert.Equal(_rowsBefore, await ContentAsync());
        Assert.False(await PendingAsync());
    }

    private async Task<bool> PendingAsync()
    {
        await using var command = _database.NewContext(_clock);
        await using var query = _database.NewQueryContext();

        return command.Database.HasPendingModelChanges() || query.Database.HasPendingModelChanges();
    }
}
