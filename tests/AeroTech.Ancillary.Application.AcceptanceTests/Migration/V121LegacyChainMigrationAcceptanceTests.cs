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

public class V121LegacyChainMigrationAcceptanceTests : IAsyncLifetime
{
    private string[] _legacyBefore = [];

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

        _legacyBefore = await LegacyRowsAsync();

        await _database.InitializeAsync(LegacySeeds.StockCapacityCommand, LegacySeeds.StockCapacityQuery);
    }

    private async Task<string[]> LegacyRowsAsync()
        => (await RowsAsync(
                "SELECT CONCAT('provision ', [Id], ' ', [Status], ' ', [Disposition], ' ', [FeeCurrencyId], ' ', [FeeApplicationUnit], ' ', [PassengerTypeCodes], ' ', [PointOfSaleIds], ' ', " +
                "[FlightNumbers], ' ', [DaysOfWeek], ' ', [TravelFrom], ' ', [TravelTo], ' ', [TimeFrom], ' ', [TimeTo], ' ', [SeatNumbers], ' ', [SalesEffectiveFrom], ' ', [AdvancePurchasePeriod], ' ', [BaggageWeight]) AS Value FROM [Ancillary].[AncillaryProvisions] " +
                "UNION ALL SELECT CONCAT('line ', [Id], ' ', [AncillaryProvisionId], ' ', [Category], ' ', [Code], ' ', [Name], ' ', [UnitAmount], ' ', [CountryId], ' ', [StationAirportId]) FROM [Ancillary].[ProvisionPriceLines] " +
                "UNION ALL SELECT CONCAT('read-line ', [Id], ' ', [AncillaryProvisionId], ' ', [Category], ' ', [Code], ' ', [UnitAmount]) FROM [ReadModel].[AncillaryProvisionPriceLines] " +
                "UNION ALL SELECT CONCAT('pair ', [Id], ' ', [AncillaryProvisionId], ' ', [OriginAirportId], ' ', [DestinationAirportId], ' ', [Direction]) FROM [Ancillary].[ProvisionRoutePairs] " +
                "UNION ALL SELECT CONCAT('definition ', [Id], ' ', [ServiceDefinitionRef], ' ', [ServiceSubCode], ' ', [Status], ' ', [BookingSsrCode], ' ', [DocumentRfisc]) FROM [Ancillary].[AncillaryServiceDefinitions] " +
                "UNION ALL SELECT CONCAT('supplier ', [Id], ' ', [Name], ' ', [Status]) FROM [Ancillary].[Suppliers]"))
            .OrderBy(row => row, StringComparer.Ordinal)
            .ToArray();

    [Fact]
    public async Task V121_M04_M06_a_v11_database_migrated_through_v12_to_v121_keeps_every_selector_price_line_status_and_row_identity()
    {
        Assert.Equal(30, _legacyBefore.Length);
        Assert.Equal(_legacyBefore, await LegacyRowsAsync());
        Assert.Contains(V121Command, await RowsAsync("SELECT [MigrationId] AS Value FROM [dbo].[__CommandsMigrationHistory]"));
        Assert.Contains(V121Query, await RowsAsync("SELECT [MigrationId] AS Value FROM [dbo].[__QueriesMigrationHistory]"));
        Assert.Empty(await ParityDifferencesAsync());

        Assert.Equal(
            $"PTC=ADT,CHD; SALEFROM=2026-10-31; SALEUNTIL=2026-11-30; POS={AirlineOffice},{AgencyOffice}; CUST=9001; CUSTTYPE=TravelAgency,Organization; ORG=1; DST=6; VIA=3; " +
            "ROUTE=1>6,3<>6; MKT=1,2; OPR=3; FLTNO=W5112,W5116; FLT=81234,81240; ACFT=1,2; FARE=7001; FARETYPE=Public,Private; FAMILY=5,6; BASIS=Y26LT; CABIN=2; RBD=41,42; " +
            "PERMIT=2026-12-20..2026-12-31; TIME=1:8-12:Allow,16:8-12:Allow; ADVANCE=3Days; BAG=1-2:23.5Kg:Prepaid",
            await TextAsync(FullRule));
        Assert.Equal("TIME=64:6-10:Allow,1:6-10:Allow,2:6-10:Allow,4:6-10:Allow,8:6-10:Allow,16:6-10:Allow,32:6-10:Allow; BAG=-:23Kg:Prepaid", await TextAsync(DraftRule));
        Assert.Equal("PTC=INF; TIME=4:-:Allow,8:-:Allow,16:-:Allow; BAG=-:23Kg:Prepaid", await TextAsync(FreeRule));
        Assert.Equal("BAG=-:23Kg:Prepaid", await TextAsync(RetiredRule));
        Assert.Equal("BAG=-:23Kg:Prepaid", await TextAsync(SuspendedRule));
        Assert.Equal("ACFT=1; SEAT=1A,1C; SEATCHAR=E", await TextAsync(SeatRule));
        Assert.Equal(string.Empty, await TextAsync(SimRule));

        var full = await RequestAsync(scope => scope.GetProvisionById.ExecuteAsync(FullRule));
        var stored = (await RequestAsync(scope => scope.Provisions.GetAsync(FullRule)))!;

        Assert.Equal(("Active", BagDefinition, 10, "Journey", "Piece", 1, 3, "Paid", "Baggage"), (full.Status.Name, full.ServiceDefinitionId, full.Sequence, full.CoverageScope.Name, full.QuantityUnit.Name, full.MinQuantity, full.MaxQuantity, full.Disposition.Name, full.ApplicationType.Name));
        Assert.Null(full.ServiceDateBasis);
        Assert.Equal(new DateTimeOffset(2026, 11, 1, 0, 0, 0, TimeSpan.FromMinutes(210)), full.SalesRestrictions!.SalesEffectiveFrom);
        Assert.Equal((0, 1, 2, 23.5m, "Kg", "AllSectors"), (full.BaggageApplication!.FreePieces, full.BaggageApplication.FirstExcessPiece, full.BaggageApplication.LastExcessPiece, full.BaggageApplication.Weight, full.BaggageApplication.WeightUnit.Name, full.BaggageApplication.TravelApplication!.Name));
        Assert.Equal(
            Enumerable.Repeat((long?)FullRule, 9),
            new long?[]
            {
                full.PassengerEligibility!.Id, full.SalesRestrictions.Id, full.Geography!.Id, full.FlightApplication!.Id, full.FareApplication!.Id, full.TravelDate!.Id,
                full.DayTimeApplication!.Id, full.AdvancePurchase!.Id, full.BaggageApplication.Id
            });
        Assert.Null(full.SeatApplication);
        Assert.Equal(new[] { 9401L, 9402L }, full.Geography.AllowedRoutePairs.Select(pair => pair.Id));
        Assert.Equal(
            (2, 2, 1, 2, 2, 2, 1, 2, 2, 2),
            (stored.PassengerEligibility!.PassengerTypes.Count, stored.SalesRestrictions!.PointsOfSale.Count, stored.SalesRestrictions.Customers.Count, stored.SalesRestrictions.CustomerTypes.Count,
                stored.Geography!.RoutePairs.Count, stored.FlightApplication!.FlightNumbers.Count, stored.TravelDate!.PermittedPeriods.Count, stored.DayTimeApplication!.Windows.Count,
                stored.FareApplication!.Rbds.Count, stored.FareApplication.AirFareTypes.Count));
        Assert.Equal(
            full.PassengerEligibility.AllowedPassengerTypes.Select(row => row.Id).Concat(full.DayTimeApplication.Windows.Select(row => row.Id)),
            stored.PassengerEligibility.PassengerTypes.OrderBy(row => row.Id).Select(row => row.Id).Concat(stored.DayTimeApplication.Windows.OrderBy(row => row.Id).Select(row => row.Id)));
        Assert.Equal(
            new[] { "season=window:2", "season=period:1" },
            await RowsAsync(
                "SELECT CONCAT('season=window:', COUNT(*)) AS Value FROM [Ancillary].[ProvisionDayTimeRestrictions] AS legacy JOIN [Ancillary].[ProvisionDayTimeWindows] AS migrated ON migrated.[Id] = legacy.[Id] WHERE legacy.[AncillaryProvisionId] = 9201 " +
                "UNION ALL SELECT CONCAT('season=period:', COUNT(*)) FROM [Ancillary].[ProvisionSeasonalPeriods] AS legacy JOIN [Ancillary].[ProvisionPermittedTravelPeriods] AS migrated ON migrated.[Id] = legacy.[Id] " +
                "AND migrated.[StartDate] = legacy.[StartDate] AND migrated.[EndDate] = legacy.[EndDate] WHERE legacy.[AncillaryProvisionId] = 9201"));

        var statuses = new List<string>();

        foreach (var provisionId in new[] { DraftRule, FreeRule, RetiredRule, SuspendedRule, SeatRule, SimRule })
        {
            var detail = await RequestAsync(scope => scope.GetProvisionById.ExecuteAsync(provisionId));

            statuses.Add($"{detail.Status.Name}:{detail.Disposition.Name}:{detail.ApplicationType.Name}");
        }

        Assert.Equal(new[] { "Draft:Paid:Baggage", "Active:Free:Baggage", "Retired:Paid:Baggage", "Suspended:Paid:Baggage", "Active:Paid:Seat", "Draft:Paid:Standard" }, statuses);

        var pricing = await RequestAsync(scope => scope.GetPricingById.ExecuteAsync(FullRule));

        Assert.Equal((FullRule, FullRule, 1, Eur, "EUR", "Item", "Active"), (pricing.Id, pricing.AncillaryProvisionId, pricing.Version, pricing.CurrencyId, pricing.Currency, pricing.FeeApplicationUnit!.Name, pricing.Status.Name));
        Assert.Null(pricing.PricingUnit);
        Assert.Equal(new long[] { 9301, 9302, 9303 }, pricing.PriceLines.Select(line => line.Id));
        Assert.Equal((30m, 2.7m, 1.05m, 33.75m), pricing.Rates.Select(rate => (rate.BaseAmount, rate.TaxAmount, rate.FeeAmount, rate.TotalAmount)).Single());
        Assert.Equal(
            new[] { "155 22.00", "47 65.74", "70 15000000.00" },
            await RowsAsync(
                "SELECT CONCAT(pricing.[CurrencyId], ' ', SUM(line.[Amount])) AS Value FROM [Ancillary].[AncillaryPricingLines] AS line " +
                "JOIN [Ancillary].[AncillaryPricings] AS pricing ON pricing.[Id] = line.[AncillaryPricingId] GROUP BY pricing.[CurrencyId] ORDER BY 1"));
        Assert.Empty((await RequestAsync(scope => scope.GetPricingsPaginated.ExecuteAsync(
            new BackofficeGetAncillaryPricingsPaginatedQuery { AncillaryProvisionId = FreeRule }))).Results);

        var definition = await RequestAsync(scope => scope.GetServiceDefinitionById.ExecuteAsync(BagDefinition));

        Assert.Equal(("XBAG_PIECE_23KG", "0CC", "Industry", "Active"), (definition.ServiceDefinitionRef, definition.ServiceSubCode, definition.SubCodeSource.Name, definition.Status.Name));
        Assert.Null(definition.PricingUnit);
        Assert.Null(definition.ServiceDateBasis);
        Assert.Equal(
            new[] { "Ancillary.9101::", "Ancillary.9102::", "ReadModel.9101::", "ReadModel.9102::" },
            await RowsAsync(
                "SELECT CONCAT('Ancillary.', [Id], ':', [PricingUnit], ':', [ServiceDateBasis]) AS Value FROM [Ancillary].[AncillaryServiceDefinitions] " +
                "UNION ALL SELECT CONCAT('ReadModel.', [Id], ':', [PricingUnit], ':', [ServiceDateBasis]) FROM [ReadModel].[AncillaryServiceDefinitions] ORDER BY 1"));
        Assert.Equal(
            new[] { "ConvertedToDayTimeWindow 12", "MergedIntoPermittedPeriod 1" },
            await RowsAsync("SELECT CONCAT([Outcome], ' ', COUNT(*)) AS Value FROM [Ancillary].[ProvisionRuleMigrationAudit] GROUP BY [Outcome] ORDER BY 1"));
    }

    [Fact]
    public async Task V121_M04_legacy_rows_stay_unmapped_until_the_pricing_unit_and_the_service_date_basis_are_assigned_and_then_work_as_current_rows()
    {
        await RefusedAsync(16210, 409, scope => scope.ReviseServiceDefinition.ReviseAsync(new TestServiceDefinitionLifecycleCommand(BagDefinition)));
        await RefusedAsync(16210, 409, scope => scope.DefinePricing.DefineAsync(Pricing(SuspendedRule, Eur, Base(11m))));
        await RefusedAsync(16210, 409, scope => scope.PublishProvision.PublishAsync(new TestPublishProvisionCommand(DraftRule, DraftRule)));
        await RefusedAsync(16210, 409, scope => scope.ActivateServiceDefinition.ActivateAsync(new TestActivateServiceDefinitionCommand(SimDefinition)));

        var assigned = await RequestAsync(scope => scope.AssignPricingUnit.AssignAsync(new TestAssignPricingUnitCommand(BagDefinition, PricingUnit.PerPiece)));

        Assert.Equal((BagDefinition, PricingUnit.PerPiece, ServiceDefinitionStatus.Active), (assigned.Id, assigned.PricingUnit!.Value, assigned.Status));
        Assert.Null(assigned.ServiceDateBasis);
        await RefusedAsync(16213, 409, scope => scope.PublishProvision.PublishAsync(new TestPublishProvisionCommand(DraftRule, DraftRule)));
        await RefusedAsync(16213, 409, scope => scope.ReactivateProvision.ReactivateAsync(new TestProvisionLifecycleCommand(SuspendedRule)));
        await RefusedAsync(16213, 409, scope => scope.ReviseServiceDefinition.ReviseAsync(new TestServiceDefinitionLifecycleCommand(BagDefinition)));
        await RefusedAsync(16213, 409, scope => scope.DefineServiceDefinition.DefineAsync(FirstExcessBagDefinition(7, 9001, "XBAG_PIECE_23KG")));

        Assert.Equal("Active", (await RequestAsync(scope => scope.GetPricingById.ExecuteAsync(FullRule))).Status.Name);
        Assert.Equal("Active", (await RequestAsync(scope => scope.GetProvisionById.ExecuteAsync(FullRule))).Status.Name);

        var dated = await RequestAsync(scope => scope.AssignServiceDateBasis.AssignAsync(new TestAssignServiceDateBasisCommand(BagDefinition, ServiceDateBasis.FlightDeparture)));

        Assert.Equal((ServiceDateBasis.FlightDeparture, ServiceDefinitionStatus.Active), (dated.ServiceDateBasis!.Value, dated.Status));
        Assert.Equal(
            new[] { "Ancillary.9101:6:1", "Ancillary.9102::", "ReadModel.9101:6:1", "ReadModel.9102::" },
            await RowsAsync(
                "SELECT CONCAT('Ancillary.', [Id], ':', [PricingUnit], ':', [ServiceDateBasis]) AS Value FROM [Ancillary].[AncillaryServiceDefinitions] " +
                "UNION ALL SELECT CONCAT('ReadModel.', [Id], ':', [PricingUnit], ':', [ServiceDateBasis]) FROM [ReadModel].[AncillaryServiceDefinitions] ORDER BY 1"));
        Assert.Equal("FlightDeparture", (await RequestAsync(scope => scope.GetProvisionById.ExecuteAsync(FullRule))).ServiceDateBasis!.Name);
        await RefusedAsync(16214, 409, scope => scope.AssignServiceDateBasis.AssignAsync(new TestAssignServiceDateBasisCommand(BagDefinition, ServiceDateBasis.FlightDeparture)));
        await RefusedAsync(16212, 409, scope => scope.AssignServiceDateBasis.AssignAsync(new TestAssignServiceDateBasisCommand(BagDefinition, ServiceDateBasis.ServiceStart)));

        var published = await RequestAsync(scope => scope.PublishProvision.PublishAsync(new TestPublishProvisionCommand(DraftRule, DraftRule)));

        Assert.Equal(ProvisionStatus.Active, published.Status);
        Assert.Equal(("Active", "IRR", 15000000m), await RequestAsync(async scope =>
        {
            var pricing = await scope.GetPricingById.ExecuteAsync(DraftRule);

            return (pricing.Status.Name, pricing.Currency!, pricing.Rates.Single().TotalAmount);
        }));

        var revision = await RequestAsync(scope => scope.RevisePricing.ReviseAsync(new TestPricingLifecycleCommand(FullRule)));

        await RequestAsync(scope => scope.ChangePricing.ChangeAsync(
            Change(revision.Id, Pricing(FullRule, Eur, Base(32m, name: "Extra bag"), Tax("VAT", 2.88m, countryId: 98, stationAirportId: Thr), Fee("HDL", 1.05m)))));

        var switched = await RequestAsync(scope => scope.SwitchActivePricing.SwitchAsync(new TestSwitchActivePricingCommand(FullRule, revision.Id, FullRule)));
        var original = await RequestAsync(scope => scope.GetPricingById.ExecuteAsync(FullRule));

        Assert.Equal((revision.Id, PricingStatus.Active), (switched.Id, switched.Status));
        Assert.Equal(("Retired", "PerPiece", 33.75m), (original.Status.Name, original.PricingUnit!.Name, original.Rates.Single().TotalAmount));

        var corrected = await RequestAsync(scope => scope.DefinePricing.DefineAsync(Pricing(SuspendedRule, Eur, Base(9.99m, name: "Extra bag"), Fee("HDL", 1m))));

        await RequestAsync(scope => scope.SwitchActivePricing.SwitchAsync(new TestSwitchActivePricingCommand(SuspendedRule, corrected.Id, SuspendedRule)));

        Assert.Equal(
            ProvisionStatus.Active,
            (await RequestAsync(scope => scope.ReactivateProvision.ReactivateAsync(new TestProvisionLifecycleCommand(SuspendedRule)))).Status);

        var sim = await RequestAsync(scope => scope.AssignPricingUnit.AssignAsync(new TestAssignPricingUnitCommand(SimDefinition, PricingUnit.PerItem)));

        Assert.Equal((PricingUnit.PerItem, ServiceDefinitionStatus.Draft), (sim.PricingUnit!.Value, sim.Status));
        await RefusedAsync(16213, 409, scope => scope.ActivateServiceDefinition.ActivateAsync(new TestActivateServiceDefinitionCommand(SimDefinition)));
        await RequestAsync(scope => scope.AssignServiceDateBasis.AssignAsync(new TestAssignServiceDateBasisCommand(SimDefinition, ServiceDateBasis.Activation)));
        await RequestAsync(scope => scope.ActivateServiceDefinition.ActivateAsync(new TestActivateServiceDefinitionCommand(SimDefinition)));

        Assert.Equal(
            ProvisionStatus.Active,
            (await RequestAsync(scope => scope.PublishProvision.PublishAsync(new TestPublishProvisionCommand(SimRule, SimRule)))).Status);
        Assert.Equal(
            ("Active", "PerItem", 9m, "Activation"),
            await RequestAsync(async scope =>
            {
                var pricing = await scope.GetPricingById.ExecuteAsync(SimRule);
                var provision = await scope.GetProvisionById.ExecuteAsync(SimRule);

                return (pricing.Status.Name, pricing.PricingUnit!.Name, pricing.Rates.Single().TotalAmount, provision.ServiceDateBasis!.Name);
            }));

        await RefusedAsync(16303, 409, scope => scope.AddPermittedTravelPeriod.AddAsync(new TestPermittedTravelPeriodRowCommand(FreeRule, 0, new DateOnly(2027, 1, 1), new DateOnly(2027, 1, 2))));
        Assert.Empty(await ParityDifferencesAsync());
    }
}
