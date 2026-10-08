using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Query.AncillaryPricingAggregate.Queries.GetAncillaryPricingsPaginated.Backoffice;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Xunit;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.P1Commands;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.V12Commands;
using static AeroTech.Ancillary.Application.AcceptanceTests.Migration.LegacySeeds;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Migration;

public class V121RuleGroupMigrationAcceptanceTests : IAsyncLifetime
{
    private const string Superseded =
        "SELECT CONCAT('date ', [Id], ' ', [AncillaryProvisionId], ' ', [TravelDate]) AS Value FROM [Ancillary].[ProvisionTravelDates] " +
        "UNION ALL SELECT CONCAT('season ', [Id], ' ', [AncillaryProvisionId], ' ', [StartDate], ' ', [EndDate]) FROM [Ancillary].[ProvisionSeasonalPeriods] " +
        "UNION ALL SELECT CONCAT('blackout ', [Id], ' ', [AncillaryProvisionId], ' ', [StartDate], ' ', [EndDate]) FROM [Ancillary].[ProvisionBlackoutPeriods] WHERE [Id] <> [AncillaryProvisionId] " +
        "UNION ALL SELECT CONCAT('time ', [Id], ' ', [AncillaryProvisionId], ' ', [DayOfWeek], ' ', [StartTime], ' ', [EndTime], ' ', [Effect]) FROM [Ancillary].[ProvisionDayTimeRestrictions] " +
        "UNION ALL SELECT CONCAT('read-date ', [Id], ' ', [AncillaryProvisionId], ' ', [TravelDate]) FROM [ReadModel].[AncillaryProvisionTravelDates] " +
        "UNION ALL SELECT CONCAT('read-season ', [Id], ' ', [AncillaryProvisionId], ' ', [StartDate], ' ', [EndDate]) FROM [ReadModel].[AncillaryProvisionSeasonalPeriods] " +
        "UNION ALL SELECT CONCAT('read-time ', [Id], ' ', [AncillaryProvisionId], ' ', [DayOfWeek], ' ', [StartTime], ' ', [EndTime], ' ', [Effect]) FROM [ReadModel].[AncillaryProvisionDayTimeRestrictions] " +
        "UNION ALL SELECT CONCAT('root ', [Id], ' ', [Status], ' ', [SalesEffectiveFrom], ' ', [SalesDiscontinueAt], ' ', [AdvancePurchasePeriod], ' ', [AdvancePurchaseUnit], ' ', [BaggageWeight], ' ', [BaggageFirstExcessPiece]) FROM [Ancillary].[AncillaryProvisions] " +
        "UNION ALL SELECT CONCAT('child ', 'pt ', [Id], ' ', [AncillaryProvisionId], ' ', [PassengerTypeCode]) FROM [Ancillary].[ProvisionPassengerTypes] " +
        "UNION ALL SELECT CONCAT('child ', 'pair ', [Id], ' ', [AncillaryProvisionId], ' ', [OriginAirportId], ' ', [DestinationAirportId]) FROM [Ancillary].[ProvisionRoutePairs]";

    private const string Periods =
        "SELECT CONCAT([AncillaryProvisionId], ' ', [Id], ' ', [ProvisionTravelDateRuleId], ' ', CONVERT(varchar(10), [StartDate], 23), '..', CONVERT(varchar(10), [EndDate], 23)) AS Value " +
        "FROM [Ancillary].[ProvisionPermittedTravelPeriods] ORDER BY [AncillaryProvisionId], [StartDate]";

    private string[] _supersededBefore = [];
    private long _seasonOfFullRule;

    private readonly TestDatabase _database = new();
    private readonly FixedClock _clock = new();

    public Task DisposeAsync() => _database.DisposeAsync();

    private async Task<TResult> RequestAsync<TResult>(Func<AncillaryScope, Task<TResult>> request)
    {
        await using var scope = new AncillaryScope(_database, _clock);

        return await request(scope);
    }

    private Task RefusedAsync(int code, int httpStatus, Func<AncillaryScope, Task> request)
        => BusinessAssert.ThrowsAsync(code, httpStatus, async () =>
        {
            await using var scope = new AncillaryScope(_database, _clock);

            await request(scope);
        });

    private async Task<List<string>> RowsAsync(string sql)
    {
        await using var context = _database.NewContext(_clock);

        return await context.Database.SqlQueryRaw<string>(sql).ToListAsync();
    }

    private async Task<string> TextAsync(long provisionId) => RuleText.Of(await RequestAsync(scope => scope.GetProvisionById.ExecuteAsync(provisionId)));

    private async Task MigrateAsync(string? command, string? query)
    {
        await using (var context = _database.NewContext(_clock))
            await context.GetService<IMigrator>().MigrateAsync(command);

        await using (var context = _database.NewQueryContext())
            await context.GetService<IMigrator>().MigrateAsync(query);
    }

    private async Task<string[]> ParityDifferencesAsync()
    {
        var differences = new List<string>();

        foreach (var (command, readModel) in RuleRowTables)
        {
            differences.AddRange(await RowsAsync(
                $"SELECT CONCAT('{command} ', [Id], ':', [AncillaryProvisionId]) AS Value FROM (" +
                $"(SELECT [Id], [AncillaryProvisionId] FROM [Ancillary].[{command}] EXCEPT SELECT [Id], [AncillaryProvisionId] FROM [ReadModel].[{readModel}]) " +
                $"UNION ALL (SELECT [Id], [AncillaryProvisionId] FROM [ReadModel].[{readModel}] EXCEPT SELECT [Id], [AncillaryProvisionId] FROM [Ancillary].[{command}])) AS difference"));
        }

        foreach (var (ruleTable, readColumn) in RuleGroups)
        {
            differences.AddRange(await RowsAsync(
                $"SELECT CONCAT('{ruleTable} ', provision.[Id]) AS Value FROM [ReadModel].[AncillaryProvisions] AS provision " +
                $"FULL JOIN [Ancillary].[{ruleTable}] AS ruleRow ON ruleRow.[AncillaryProvisionId] = provision.[Id] " +
                $"WHERE ISNULL(provision.[{readColumn}], 0) <> ISNULL(ruleRow.[Id], 0)"));
        }

        differences.AddRange(await RowsAsync(
            "SELECT CONCAT('period ', command.[Id]) AS Value FROM [Ancillary].[ProvisionPermittedTravelPeriods] AS command JOIN [ReadModel].[AncillaryProvisionPermittedTravelPeriods] AS model ON model.[Id] = command.[Id] " +
            "WHERE command.[StartDate] <> model.[StartDate] OR command.[EndDate] <> model.[EndDate] " +
            "UNION ALL SELECT CONCAT('blackout ', command.[Id]) FROM [Ancillary].[ProvisionBlackoutPeriods] AS command JOIN [ReadModel].[AncillaryProvisionBlackoutPeriods] AS model ON model.[Id] = command.[Id] " +
            "WHERE command.[StartDate] <> model.[StartDate] OR command.[EndDate] <> model.[EndDate] " +
            "UNION ALL SELECT CONCAT('window ', command.[Id]) FROM [Ancillary].[ProvisionDayTimeWindows] AS command JOIN [ReadModel].[AncillaryProvisionDayTimeWindows] AS model ON model.[Id] = command.[Id] " +
            "WHERE command.[DaysOfWeekMask] <> model.[DaysOfWeekMask] OR command.[Effect] <> model.[Effect] OR ISNULL(command.[StartLocalTime], '00:00') <> ISNULL(model.[StartLocalTime], '00:00') " +
            "OR ISNULL(command.[EndLocalTime], '00:00') <> ISNULL(model.[EndLocalTime], '00:00') " +
            "UNION ALL SELECT CONCAT('provision ', command.[Id]) FROM [Ancillary].[AncillaryProvisions] AS command FULL JOIN [ReadModel].[AncillaryProvisions] AS model ON model.[Id] = command.[Id] " +
            "WHERE command.[Id] IS NULL OR model.[Id] IS NULL OR command.[Status] <> model.[Status] OR command.[Disposition] <> model.[Disposition] OR command.[Sequence] <> model.[Sequence]"));

        return differences.ToArray();
    }

    public async Task InitializeAsync()
    {
        await MigrateAsync(LegacySeeds.V11Command, LegacySeeds.V11Query);

        await using (var seed = _database.NewContext(_clock))
            await seed.Database.ExecuteSqlRawAsync(V11);

        await MigrateAsync(LegacySeeds.V12Command, LegacySeeds.V12Query);

        await using (var seed = _database.NewContext(_clock))
            await seed.Database.ExecuteSqlRawAsync(V12DatesAndTimes);

        _supersededBefore = await SupersededRowsAsync();
        _seasonOfFullRule = long.Parse((await RowsAsync("SELECT CAST([Id] AS varchar(30)) AS Value FROM [Ancillary].[ProvisionSeasonalPeriods] WHERE [AncillaryProvisionId] = 9201")).Single());

        await _database.InitializeAsync();
    }

    private async Task<string[]> SupersededRowsAsync() => (await RowsAsync(Superseded)).OrderBy(row => row, StringComparer.Ordinal).ToArray();

    [Fact]
    public async Task V121_M01_one_thousand_adjacent_travel_dates_become_one_permitted_period_with_the_same_dates_and_an_audit_trail()
    {
        Assert.Equal(1007, _supersededBefore.Count(row => row.StartsWith("date ", StringComparison.Ordinal)));
        Assert.Equal(
            new[] { "9205 700001 9205 2027-01-01..2029-09-26" },
            (await RowsAsync(Periods)).Where(row => row.StartsWith("9205 ", StringComparison.Ordinal)));
        Assert.Equal(
            new[] { "1000 1000 0 0" },
            await RowsAsync(
                "SELECT CONCAT(" +
                "(SELECT COUNT(*) FROM [Ancillary].[ProvisionTravelDates] WHERE [AncillaryProvisionId] = 9205), ' ', " +
                "(SELECT SUM(DATEDIFF(DAY, [StartDate], [EndDate]) + 1) FROM [Ancillary].[ProvisionPermittedTravelPeriods] WHERE [AncillaryProvisionId] = 9205), ' ', " +
                "(SELECT COUNT(*) FROM [Ancillary].[ProvisionTravelDates] AS legacy WHERE legacy.[AncillaryProvisionId] = 9205 AND NOT EXISTS (SELECT 1 FROM [Ancillary].[ProvisionPermittedTravelPeriods] AS period " +
                "WHERE period.[AncillaryProvisionId] = 9205 AND legacy.[TravelDate] BETWEEN period.[StartDate] AND period.[EndDate])), ' ', " +
                "(SELECT COUNT(*) FROM [Ancillary].[ProvisionBlackoutPeriods] WHERE [AncillaryProvisionId] = 9205)) AS Value"));
        Assert.Equal(
            new[] { "ProvisionTravelDates ProvisionPermittedTravelPeriods 700001 MergedIntoPermittedPeriod 1000 700001 701000" },
            await RowsAsync(
                "SELECT CONCAT([SourceTable], ' ', [TargetTable], ' ', [TargetRowId], ' ', [Outcome], ' ', COUNT(*), ' ', MIN([SourceRowId]), ' ', MAX([SourceRowId])) AS Value " +
                "FROM [Ancillary].[ProvisionRuleMigrationAudit] WHERE [AncillaryProvisionId] = 9205 GROUP BY [SourceTable], [TargetTable], [TargetRowId], [Outcome]"));
        Assert.Equal("PERMIT=2027-01-01..2029-09-26; BAG=-:23Kg:Prepaid", await TextAsync(SuspendedRule));

        var listed = (await RequestAsync(scope => scope.Query.AncillaryProvisionPermittedTravelPeriods.AsNoTracking().Where(row => row.AncillaryProvisionId == SuspendedRule).ToListAsync())).Single();

        Assert.Equal((700001L, new DateOnly(2027, 1, 1), new DateOnly(2029, 9, 26)), (listed.Id, listed.StartDate, listed.EndDate));
    }

    [Fact]
    public async Task V121_M02_dates_and_seasons_keep_their_intersection_dates_alone_or_seasons_alone_keep_their_union_and_nothing_is_widened()
    {
        Assert.Equal(
            new[]
            {
                "9201 710001 9201 2026-12-24..2026-12-25",
                "9204 730001 9204 2027-02-01..2027-02-02",
                "9204 730003 9204 2027-02-10..2027-02-10",
                "9205 700001 9205 2027-01-01..2029-09-26",
                "9206 740001 9206 2027-03-01..2027-04-30",
                "9206 740004 9206 2027-06-01..2027-06-30"
            },
            await RowsAsync(Periods));
        Assert.Empty(await RowsAsync(
            "SELECT CONCAT(period.[AncillaryProvisionId], ' ', day.[Value]) AS Value FROM [Ancillary].[ProvisionPermittedTravelPeriods] AS period " +
            "CROSS APPLY (SELECT TOP (DATEDIFF(DAY, period.[StartDate], period.[EndDate]) + 1) DATEADD(DAY, ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) - 1, period.[StartDate]) AS [Value] " +
            "FROM sys.all_objects AS a CROSS JOIN sys.all_objects AS b) AS day " +
            "WHERE (EXISTS (SELECT 1 FROM [Ancillary].[ProvisionTravelDates] AS legacy WHERE legacy.[AncillaryProvisionId] = period.[AncillaryProvisionId]) " +
            "AND NOT EXISTS (SELECT 1 FROM [Ancillary].[ProvisionTravelDates] AS legacy WHERE legacy.[AncillaryProvisionId] = period.[AncillaryProvisionId] AND legacy.[TravelDate] = day.[Value])) " +
            "OR (EXISTS (SELECT 1 FROM [Ancillary].[ProvisionSeasonalPeriods] AS legacy WHERE legacy.[AncillaryProvisionId] = period.[AncillaryProvisionId]) " +
            "AND NOT EXISTS (SELECT 1 FROM [Ancillary].[ProvisionSeasonalPeriods] AS legacy WHERE legacy.[AncillaryProvisionId] = period.[AncillaryProvisionId] AND day.[Value] BETWEEN legacy.[StartDate] AND legacy.[EndDate]))"));
        Assert.Equal(
            new[]
            {
                $"9201 ProvisionSeasonalPeriods {_seasonOfFullRule}   IntersectedWithTravelDates",
                "9201 ProvisionTravelDates 710001 ProvisionPermittedTravelPeriods 710001 MergedIntoPermittedPeriod",
                "9201 ProvisionTravelDates 710002 ProvisionPermittedTravelPeriods 710001 MergedIntoPermittedPeriod",
                "9201 ProvisionTravelDates 710003   ExcludedByIntersection",
                "9204 ProvisionTravelDates 730001 ProvisionPermittedTravelPeriods 730001 MergedIntoPermittedPeriod",
                "9204 ProvisionTravelDates 730002 ProvisionPermittedTravelPeriods 730001 MergedIntoPermittedPeriod",
                "9204 ProvisionTravelDates 730003 ProvisionPermittedTravelPeriods 730003 MergedIntoPermittedPeriod",
                "9206 ProvisionSeasonalPeriods 740001 ProvisionPermittedTravelPeriods 740001 MergedIntoPermittedPeriod",
                "9206 ProvisionSeasonalPeriods 740002 ProvisionPermittedTravelPeriods 740001 MergedIntoPermittedPeriod",
                "9206 ProvisionSeasonalPeriods 740003 ProvisionPermittedTravelPeriods 740001 MergedIntoPermittedPeriod",
                "9206 ProvisionSeasonalPeriods 740004 ProvisionPermittedTravelPeriods 740004 MergedIntoPermittedPeriod"
            },
            await RowsAsync(
                "SELECT CONCAT([AncillaryProvisionId], ' ', [SourceTable], ' ', [SourceRowId], ' ', [TargetTable], ' ', [TargetRowId], ' ', [Outcome]) AS Value FROM [Ancillary].[ProvisionRuleMigrationAudit] " +
                "WHERE [AncillaryProvisionId] IN (9201, 9204, 9206) AND [SourceTable] IN (N'ProvisionTravelDates', N'ProvisionSeasonalPeriods') ORDER BY [AncillaryProvisionId], [SourceTable], [SourceRowId]"));
        Assert.Contains("PERMIT=2026-12-24..2026-12-25;", await TextAsync(FullRule), StringComparison.Ordinal);
        Assert.Equal("PERMIT=2027-02-01..2027-02-02,2027-02-10..2027-02-10; BAG=-:23Kg:Prepaid", await TextAsync(RetiredRule));
        Assert.Equal("ACFT=1; PERMIT=2027-03-01..2027-04-30,2027-06-01..2027-06-30; SEAT=1A,1C; SEATCHAR=E", await TextAsync(SeatRule));
    }

    [Fact]
    public async Task V121_M02_M03_an_empty_intersection_or_an_unconvertible_window_is_quarantined_as_unsaleable_and_denials_keep_their_precedence()
    {
        Assert.Equal(
            new[]
            {
                "9202 9202 9202 0001-01-01..9999-12-31",
                "9203 9203 9203 0001-01-01..9999-12-31",
                "9207 750001 9207 2027-12-24..2027-12-26"
            },
            await RowsAsync(
                "SELECT CONCAT([AncillaryProvisionId], ' ', [Id], ' ', [ProvisionTravelDateRuleId], ' ', CONVERT(varchar(10), [StartDate], 23), '..', CONVERT(varchar(10), [EndDate], 23)) AS Value " +
                "FROM [Ancillary].[ProvisionBlackoutPeriods] ORDER BY [AncillaryProvisionId], [StartDate]"));
        Assert.Equal(
            new[]
            {
                "9202 AncillaryProvisions 9202 ProvisionBlackoutPeriods 9202 QuarantinedAsUnsaleable",
                "9202 ProvisionDayTimeRestrictions 760001   ManualMappingRequired",
                "9203 AncillaryProvisions 9203 ProvisionBlackoutPeriods 9203 QuarantinedAsUnsaleable",
                "9203 ProvisionSeasonalPeriods 720001   IntersectedWithTravelDates",
                "9203 ProvisionTravelDates 720002   ExcludedByIntersection"
            },
            await RowsAsync(
                "SELECT CONCAT([AncillaryProvisionId], ' ', [SourceTable], ' ', [SourceRowId], ' ', [TargetTable], ' ', [TargetRowId], ' ', [Outcome]) AS Value FROM [Ancillary].[ProvisionRuleMigrationAudit] " +
                "WHERE [AncillaryProvisionId] IN (9202, 9203) AND [Outcome] <> N'ConvertedToDayTimeWindow' ORDER BY [AncillaryProvisionId], [SourceTable], [SourceRowId]"));
        Assert.Equal(
            new[] { "ConvertedToDayTimeWindow 14", "ExcludedByIntersection 2", "IntersectedWithTravelDates 2", "ManualMappingRequired 1", "MergedIntoPermittedPeriod 1009", "QuarantinedAsUnsaleable 2" },
            await RowsAsync("SELECT CONCAT([Outcome], ' ', COUNT(*)) AS Value FROM [Ancillary].[ProvisionRuleMigrationAudit] GROUP BY [Outcome] ORDER BY 1"));
        Assert.Equal("PTC=INF; BLACKOUT=0001-01-01..9999-12-31; TIME=4:-:Allow,8:-:Allow,16:-:Allow; BAG=-:23Kg:Prepaid", await TextAsync(FreeRule));
        Assert.Equal(
            "BLACKOUT=0001-01-01..9999-12-31; TIME=64:6-10:Allow,1:6-10:Allow,2:6-10:Allow,4:6-10:Allow,8:6-10:Allow,16:6-10:Allow,32:6-10:Allow; BAG=-:23Kg:Prepaid",
            await TextAsync(DraftRule));
        Assert.Equal("BLACKOUT=2027-12-24..2027-12-26; TIME=16:-:Deny,1:9-10:Deny", await TextAsync(SimRule));
        Assert.Equal(
            new[] { "750002 16   2", "750003 1 09:00:00 10:00:00 2" },
            await RowsAsync(
                "SELECT CONCAT([Id], ' ', [DaysOfWeekMask], ' ', CONVERT(varchar(8), [StartLocalTime], 108), ' ', CONVERT(varchar(8), [EndLocalTime], 108), ' ', [Effect]) AS Value " +
                "FROM [Ancillary].[ProvisionDayTimeWindows] WHERE [AncillaryProvisionId] = 9207 ORDER BY [Id]"));

        var free = (await RequestAsync(scope => scope.Provisions.GetAsync(FreeRule)))!;

        Assert.Equal((ProvisionStatus.Active, 0, 1, FreeRule), (free.Status, free.TravelDate!.PermittedPeriods.Count, free.TravelDate.BlackoutPeriods.Count, free.TravelDate.BlackoutPeriods.Single().Id));

        await RequestAsync(scope => scope.AssignPricingUnit.AssignAsync(new TestAssignPricingUnitCommand(BagDefinition, PricingUnit.PerPiece)));
        await RequestAsync(scope => scope.AssignServiceDateBasis.AssignAsync(new TestAssignServiceDateBasisCommand(BagDefinition, ServiceDateBasis.FlightDeparture)));
        await RefusedAsync(16315, 409, scope => scope.PublishProvision.PublishAsync(new TestPublishProvisionCommand(DraftRule, DraftRule)));

        Assert.Equal("Draft", (await RequestAsync(scope => scope.GetProvisionById.ExecuteAsync(DraftRule))).Status.Name);

        await RequestAsync(scope => scope.RemoveBlackoutPeriod.RemoveAsync(new TestBlackoutPeriodRowCommand(DraftRule, DraftRule, default, default)));
        await RequestAsync(scope => scope.AddDayTimeWindow.AddAsync(new TestDayTimeWindowRowCommand(DraftRule, 0, 32, new TimeOnly(22, 0), null, DayTimeRestrictionEffect.Allow)));
        await RequestAsync(scope => scope.AddDayTimeWindow.AddAsync(new TestDayTimeWindowRowCommand(DraftRule, 0, 64, null, new TimeOnly(2, 0), DayTimeRestrictionEffect.Allow)));

        Assert.Equal(
            ProvisionStatus.Active,
            (await RequestAsync(scope => scope.PublishProvision.PublishAsync(new TestPublishProvisionCommand(DraftRule, DraftRule)))).Status);
        Assert.Equal(
            "TIME=64:6-10:Allow,1:6-10:Allow,2:6-10:Allow,4:6-10:Allow,8:6-10:Allow,16:6-10:Allow,32:6-10:Allow,32:22-:Allow,64:-2:Allow; BAG=-:23Kg:Prepaid",
            await TextAsync(DraftRule));
    }

    [Fact]
    public async Task V121_M05_M06_the_superseded_rows_stay_untouched_and_unread_and_both_stores_hold_the_same_rule_rows_without_orphans()
    {
        Assert.Equal(_supersededBefore, await SupersededRowsAsync());
        Assert.Empty(await ParityDifferencesAsync());
        Assert.Equal(
            new[] { "7 7 7 7" },
            await RowsAsync(
                "SELECT CONCAT((SELECT COUNT(*) FROM [Ancillary].[AncillaryProvisions]), ' ', (SELECT COUNT(*) FROM [ReadModel].[AncillaryProvisions]), ' ', " +
                "(SELECT COUNT(*) FROM [Ancillary].[ProvisionTravelDateRules]), ' ', (SELECT COUNT(*) FROM [ReadModel].[AncillaryProvisions] WHERE [TravelDateRuleId] = [Id])) AS Value"));

        var orphans = new List<string>();

        foreach (var (command, _) in RuleRowTables)
        {
            orphans.AddRange(await RowsAsync(
                $"SELECT CONCAT('{command} ', child.[Id]) AS Value FROM [Ancillary].[{command}] AS child " +
                $"WHERE NOT EXISTS (SELECT 1 FROM [Ancillary].[AncillaryProvisions] AS provision WHERE provision.[Id] = child.[AncillaryProvisionId])"));
        }

        Assert.Empty(orphans);

        await RequestAsync(scope => scope.AssignPricingUnit.AssignAsync(new TestAssignPricingUnitCommand(SimDefinition, PricingUnit.PerItem)));
        await RequestAsync(scope => scope.AssignServiceDateBasis.AssignAsync(new TestAssignServiceDateBasisCommand(SimDefinition, ServiceDateBasis.Activation)));
        await RequestAsync(scope => scope.ChangeTravelDate.ChangeAsync(new TestChangeProvisionTravelDateCommand(SimRule, new([new(new DateOnly(2028, 1, 1), new DateOnly(2028, 1, 31))]))));

        Assert.Equal(_supersededBefore.Where(row => !row.StartsWith("blackout ", StringComparison.Ordinal)), (await SupersededRowsAsync()).Where(row => !row.StartsWith("blackout ", StringComparison.Ordinal)));
        Assert.Equal("PERMIT=2028-01-01..2028-01-31; TIME=16:-:Deny,1:9-10:Deny", await TextAsync(SimRule));
        Assert.Empty(await ParityDifferencesAsync());
    }

    [Fact]
    public async Task V121_M12_the_rule_group_migrations_are_reversible_and_repeatable_on_the_seeded_database()
    {
        var periods = await RowsAsync(Periods);
        var audit = await RowsAsync("SELECT CONCAT([AncillaryProvisionId], ' ', [SourceTable], ' ', [SourceRowId], ' ', [TargetRowId], ' ', [Outcome]) AS Value FROM [Ancillary].[ProvisionRuleMigrationAudit] ORDER BY 1");

        Assert.Equal(1030, audit.Count);

        await MigrateAsync(LegacySeeds.V12Command, LegacySeeds.V12Query);

        Assert.Equal(_supersededBefore, await SupersededRowsAsync());
        Assert.Equal(
            new[] { "0" },
            await RowsAsync(
                "SELECT CAST(COUNT(*) AS varchar(10)) AS Value FROM sys.tables AS t JOIN sys.schemas AS s ON s.schema_id = t.schema_id " +
                "WHERE (s.name = 'Ancillary' AND (t.name LIKE 'Provision%Rules' OR t.name IN ('ProvisionPermittedTravelPeriods', 'ProvisionDayTimeWindows', 'ProvisionEligibleAgeBands', 'ProvisionServiceLocations', 'ProvisionCoverageCountries', 'ProvisionRuleMigrationAudit'))) " +
                "OR (s.name = 'ReadModel' AND t.name IN ('AncillaryProvisionPermittedTravelPeriods', 'AncillaryProvisionDayTimeWindows', 'AncillaryProvisionEligibleAgeBands', 'AncillaryProvisionServiceLocations', 'AncillaryProvisionCoverageCountries'))"));
        Assert.Equal(
            new[] { "0" },
            await RowsAsync(
                "SELECT CAST(COUNT(*) AS varchar(10)) AS Value FROM sys.columns AS c JOIN sys.tables AS t ON t.object_id = c.object_id " +
                "WHERE c.name LIKE '%RuleId' OR c.name = 'ServiceDateBasis' OR c.name = 'AdvancePurchaseSameTimeAsTicketed'"));
        Assert.DoesNotContain(V121Command, await RowsAsync("SELECT [MigrationId] AS Value FROM [dbo].[__CommandsMigrationHistory]"));
        Assert.DoesNotContain(V121Query, await RowsAsync("SELECT [MigrationId] AS Value FROM [dbo].[__QueriesMigrationHistory]"));

        await MigrateAsync(null, null);

        Assert.Equal(periods, await RowsAsync(Periods));
        Assert.Equal(
            audit,
            await RowsAsync("SELECT CONCAT([AncillaryProvisionId], ' ', [SourceTable], ' ', [SourceRowId], ' ', [TargetRowId], ' ', [Outcome]) AS Value FROM [Ancillary].[ProvisionRuleMigrationAudit] ORDER BY 1"));
        Assert.Equal(_supersededBefore, await SupersededRowsAsync());
        Assert.Empty(await ParityDifferencesAsync());
        Assert.False(await RequestAsync(scope => Task.FromResult(scope.Command.Database.HasPendingModelChanges())));
        Assert.False(await RequestAsync(scope => Task.FromResult(scope.Query.Database.HasPendingModelChanges())));
    }
}
