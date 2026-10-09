using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Xunit;
using static AeroTech.Ancillary.Application.AcceptanceTests.Migration.LegacySeeds;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Migration;

public class V121LegacySchemaCleanupAcceptanceTests : IAsyncLifetime
{
    private static readonly string[] Retained = ["Ancillary.ProvisionRuleMigrationAudit."];

    private static bool IsRetained(string column) => Retained.Any(retained => column.StartsWith(retained, StringComparison.Ordinal));
    private const string ProvisionColumn = ".AncillaryProvisions.";

    private const string StoredColumns =
        "SELECT s.name + '.' + t.name + '.' + c.name AS Value FROM sys.columns AS c JOIN sys.tables AS t ON t.object_id = c.object_id " +
        "JOIN sys.schemas AS s ON s.schema_id = t.schema_id WHERE t.name NOT LIKE '[_][_]%MigrationHistory'";

    private const string StoredShape =
        "SELECT CONCAT('column ', s.name, '.', t.name, '.', c.name, ' ', TYPE_NAME(c.user_type_id), ' ', c.max_length, ' ', c.precision, ' ', c.scale, ' ', c.is_nullable, ' ', d.definition COLLATE DATABASE_DEFAULT) AS Value " +
        "FROM sys.columns AS c JOIN sys.tables AS t ON t.object_id = c.object_id JOIN sys.schemas AS s ON s.schema_id = t.schema_id " +
        "LEFT JOIN sys.default_constraints AS d ON d.object_id = c.default_object_id WHERE s.name IN ('Ancillary', 'ReadModel') " +
        "UNION ALL SELECT CONCAT('index ', s.name, '.', t.name, '.', i.name, ' ', i.is_unique, ' ', i.is_primary_key, ' ', i.filter_definition COLLATE DATABASE_DEFAULT) " +
        "FROM sys.indexes AS i JOIN sys.tables AS t ON t.object_id = i.object_id JOIN sys.schemas AS s ON s.schema_id = t.schema_id WHERE s.name IN ('Ancillary', 'ReadModel') AND i.name IS NOT NULL " +
        "UNION ALL SELECT CONCAT('key ', s.name, '.', t.name, '.', k.name, ' ', k.delete_referential_action_desc COLLATE DATABASE_DEFAULT) " +
        "FROM sys.foreign_keys AS k JOIN sys.tables AS t ON t.object_id = k.parent_object_id JOIN sys.schemas AS s ON s.schema_id = t.schema_id WHERE s.name IN ('Ancillary', 'ReadModel')";

    private const string SupersededValues =
        "SELECT CONCAT(" +
        "(SELECT COUNT(*) FROM [Ancillary].[AncillaryProvisions] WHERE [PassengerTypeCodes] <> N'[]' OR [FlightNumbers] <> N'[]' OR [FeeCurrencyId] IS NOT NULL OR [TravelFrom] IS NOT NULL), ' ', " +
        "(SELECT COUNT(*) FROM [ReadModel].[AncillaryProvisions] WHERE [PassengerTypeCodes] <> N'[]' OR [FlightNumbers] <> N'[]' OR [FeeCurrencyId] IS NOT NULL OR [TravelFrom] IS NOT NULL), ' ', " +
        "(SELECT COUNT(*) FROM [Ancillary].[ProvisionPriceLines]) + (SELECT COUNT(*) FROM [Ancillary].[ProvisionTravelDates]) + (SELECT COUNT(*) FROM [Ancillary].[ProvisionSeasonalPeriods]) + " +
        "(SELECT COUNT(*) FROM [Ancillary].[ProvisionDayTimeRestrictions]), ' ', " +
        "(SELECT COUNT(*) FROM [ReadModel].[AncillaryProvisionPriceLines]) + (SELECT COUNT(*) FROM [ReadModel].[AncillaryProvisionTravelDates]) + " +
        "(SELECT COUNT(*) FROM [ReadModel].[AncillaryProvisionSeasonalPeriods]) + (SELECT COUNT(*) FROM [ReadModel].[AncillaryProvisionDayTimeRestrictions])) AS Value";

    private readonly TestDatabase _database = new();
    private readonly FixedClock _clock = new();
    private string[] _currentColumns = [];
    private string[] _currentRowsBefore = [];

    public async Task InitializeAsync()
    {
        await MigrateAsync(V11Command, V11Query);

        await using (var seed = _database.NewContext(_clock))
            await seed.Database.ExecuteSqlRawAsync(V11);

        await MigrateAsync(V12Command, V12Query);

        await using (var seed = _database.NewContext(_clock))
            await seed.Database.ExecuteSqlRawAsync(V12DatesAndTimes);

        await _database.InitializeAsync(StockCapacityCommand, StockCapacityQuery);

        _currentColumns = (await ModelColumnsAsync()).Intersect(await RowsAsync(StoredColumns)).ToArray();
        _currentRowsBefore = await CurrentRowsAsync();
    }

    public Task DisposeAsync() => _database.DisposeAsync();

    private async Task MigrateAsync(string? command, string? query)
    {
        await using (var context = _database.NewContext(_clock))
            await context.GetService<IMigrator>().MigrateAsync(command);

        await using (var context = _database.NewQueryContext())
            await context.GetService<IMigrator>().MigrateAsync(query);
    }

    private async Task<string[]> RowsAsync(string sql)
    {
        await using var context = _database.NewContext(_clock);

        return (await context.Database.SqlQueryRaw<string>(sql).ToListAsync()).OrderBy(row => row, StringComparer.Ordinal).ToArray();
    }

    private static IEnumerable<string> ColumnsOf(DbContext context)
        => context.Model.GetEntityTypes()
            .Where(type => type.GetTableName() is not null)
            .SelectMany(type => type.GetProperties()
                .Select(property => property.GetColumnName(StoreObjectIdentifier.Table(type.GetTableName()!, type.GetSchema())))
                .Where(column => column is not null)
                .Select(column => $"{type.GetSchema() ?? "dbo"}.{type.GetTableName()}.{column}"));

    private async Task<string[]> ModelColumnsAsync()
    {
        await using var command = _database.NewContext(_clock);
        await using var query = _database.NewQueryContext();
        await using var reference = _database.NewReferenceContext();

        return ColumnsOf(command).Concat(ColumnsOf(query)).Concat(ColumnsOf(reference)).Distinct().OrderBy(column => column, StringComparer.Ordinal).ToArray();
    }

    private async Task<string[]> CurrentRowsAsync()
    {
        var tables = _currentColumns
            .Where(column => column.StartsWith("Ancillary.", StringComparison.Ordinal) || column.StartsWith("ReadModel.", StringComparison.Ordinal))
            .GroupBy(column => column[..column.LastIndexOf('.')], column => column[(column.LastIndexOf('.') + 1)..]);
        var rows = new List<string>();

        foreach (var table in tables)
        {
            var name = string.Join('.', table.Key.Split('.').Select(part => $"[{part}]"));
            var columns = string.Join(", ", table.Select(column => $"[{column}]"));

            rows.AddRange(await RowsAsync($"SELECT CONCAT('{table.Key} ', COUNT_BIG(*), ' ', ISNULL(CHECKSUM_AGG(BINARY_CHECKSUM({columns})), 0)) AS Value FROM {name}"));
        }

        return rows.ToArray();
    }

    private async Task<bool> PendingAsync()
    {
        await using var command = _database.NewContext(_clock);
        await using var query = _database.NewQueryContext();

        return command.Database.HasPendingModelChanges() || query.Database.HasPendingModelChanges();
    }

    [Fact]
    public async Task V121_M05_the_approved_cleanup_drops_only_the_superseded_columns_and_tables_and_leaves_the_database_exactly_as_the_model()
    {
        var before = await RowsAsync(StoredColumns);

        Assert.NotEqual("0 0 0 0", (await RowsAsync(SupersededValues)).Single());
        Assert.Contains(_currentRowsBefore, row => row.StartsWith("Ancillary.AncillaryProvisions ", StringComparison.Ordinal) && !row.StartsWith("Ancillary.AncillaryProvisions 0 ", StringComparison.Ordinal));

        await MigrateAsync(LegacyCleanupCommand, LegacyCleanupQuery);

        var after = await RowsAsync(StoredColumns);
        var dropped = before.Except(after).ToList();

        Assert.Empty(after.Except(before));
        Assert.Equal(39, dropped.Count(column => column.StartsWith("Ancillary" + ProvisionColumn, StringComparison.Ordinal)));
        Assert.Equal(27, dropped.Count(column => column.StartsWith("ReadModel" + ProvisionColumn, StringComparison.Ordinal)));
        Assert.Equal(
            new[]
            {
                "Ancillary.ProvisionDayTimeRestrictions", "Ancillary.ProvisionPriceLines", "Ancillary.ProvisionSeasonalPeriods", "Ancillary.ProvisionTravelDates",
                "ReadModel.AncillaryProvisionDayTimeRestrictions", "ReadModel.AncillaryProvisionPriceLines", "ReadModel.AncillaryProvisionSeasonalPeriods",
                "ReadModel.AncillaryProvisionTravelDates"
            },
            dropped.Where(column => !column.Contains(ProvisionColumn, StringComparison.Ordinal)).Select(column => column[..column.LastIndexOf('.')]).Distinct().OrderBy(table => table, StringComparer.Ordinal));
        Assert.Equal(7, after.Count(column => column.StartsWith(Retained[0], StringComparison.Ordinal)));
        Assert.Equal(_currentRowsBefore, await CurrentRowsAsync());

        await MigrateAsync(null, null);

        Assert.Equal(await ModelColumnsAsync(), (await RowsAsync(StoredColumns)).Where(column => !IsRetained(column)));
        Assert.Equal(_currentRowsBefore, await CurrentRowsAsync());
        Assert.Contains(LegacyCleanupCommand, await RowsAsync("SELECT [MigrationId] AS Value FROM [dbo].[__CommandsMigrationHistory]"));
        Assert.Contains(LegacyCleanupQuery, await RowsAsync("SELECT [MigrationId] AS Value FROM [dbo].[__QueriesMigrationHistory]"));
        Assert.False(await PendingAsync());
    }

    [Fact]
    public async Task V121_M12_the_cleanup_is_reversible_in_shape_while_the_dropped_values_are_gone_for_good()
    {
        var shape = await RowsAsync(StoredShape);

        await MigrateAsync(LegacyCleanupCommand, LegacyCleanupQuery);
        await MigrateAsync(StockCapacityCommand, StockCapacityQuery);

        Assert.Equal(shape, await RowsAsync(StoredShape));
        Assert.Equal("0 0 0 0", (await RowsAsync(SupersededValues)).Single());
        Assert.Equal(_currentRowsBefore, await CurrentRowsAsync());
        Assert.DoesNotContain(LegacyCleanupCommand, await RowsAsync("SELECT [MigrationId] AS Value FROM [dbo].[__CommandsMigrationHistory]"));
        Assert.DoesNotContain(LegacyCleanupQuery, await RowsAsync("SELECT [MigrationId] AS Value FROM [dbo].[__QueriesMigrationHistory]"));

        await MigrateAsync(null, null);

        Assert.Equal(await ModelColumnsAsync(), (await RowsAsync(StoredColumns)).Where(column => !IsRetained(column)));
        Assert.Equal(_currentRowsBefore, await CurrentRowsAsync());
        Assert.False(await PendingAsync());
    }
}
