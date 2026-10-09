using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetAncillaryProvisionsPaginated.Backoffice;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;
using AeroTech.Messages.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.P1Commands;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.V121Commands;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.V12Commands;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Provisions;

[Collection(DatabaseCollection.Name)]
public class V121ProvisionRuleAcceptanceTests
{
    private readonly TestDatabase _database;
    private readonly FixedClock _clock = new();
    private readonly FamilyProof _proof;

    public V121ProvisionRuleAcceptanceTests(TestDatabase database)
    {
        _database = database;
        _proof = new FamilyProof(database, _clock);
    }

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

    private async Task<long> ActiveDefinitionAsync(string reference = "PRIORITY_BOARDING")
    {
        var airlineId = _database.NextAirlineId();
        var supplierId = await _proof.SupplierAsync(M1Commands.LocalSupplier(airlineId));

        return (await _proof.DefinitionAsync(CarrierDefinition(airlineId, supplierId, reference, "PRB", "F", "TS", "Priority boarding"))).Id;
    }

    private static TestDefineProvisionCommand Free(long definitionId, int sequence = 10) => Provision(definitionId, sequence, CommercialDisposition.Free);

    private static (DateOnly Start, DateOnly End)[] Periods(IReadOnlyList<BackofficeProvisionDatePeriodDto> rows)
        => rows.Select(row => (row.StartDate, row.EndDate)).ToArray();

    private Task<List<string>> RowsAsync(FormattableString sql) => RequestAsync(scope => scope.Command.Database.SqlQuery<string>(sql).ToListAsync());

    private Task<BackofficeProvisionDto> ReadAsync(long provisionId) => RequestAsync(scope => scope.GetProvisionById.ExecuteAsync(provisionId));

    [Fact]
    public async Task V121_D01_one_thousand_consecutive_days_are_one_permitted_period_row_in_both_stores()
    {
        var definitionId = await ActiveDefinitionAsync();
        var days = Enumerable.Range(0, 1000).Select(offset => Day(2027, 1, 1).AddDays(offset)).Select(day => Period(day, day)).ToArray();
        var draft = await RequestAsync(scope => scope.DefineProvision.DefineAsync(Free(definitionId) with { TravelDate = Dates(days) }));
        var detail = await ReadAsync(draft.Id);

        Assert.Equal(new[] { (Day(2027, 1, 1), Day(2029, 9, 26)) }, Periods(detail.TravelDate!.PermittedPeriods));
        Assert.Empty(detail.TravelDate.BlackoutPeriods);
        Assert.Equal(
            new[] { $"{detail.TravelDate.PermittedPeriods[0].Id}|{detail.TravelDate.Id}|2027-01-01|2029-09-26" },
            await RowsAsync($"SELECT CONCAT(Id, '|', ProvisionTravelDateRuleId, '|', CONVERT(varchar(10), StartDate, 23), '|', CONVERT(varchar(10), EndDate, 23)) AS Value FROM Ancillary.ProvisionPermittedTravelPeriods WHERE AncillaryProvisionId = {draft.Id}"));
        Assert.Equal(
            new[] { $"{detail.TravelDate.PermittedPeriods[0].Id}|2027-01-01|2029-09-26" },
            await RowsAsync($"SELECT CONCAT(Id, '|', CONVERT(varchar(10), StartDate, 23), '|', CONVERT(varchar(10), EndDate, 23)) AS Value FROM ReadModel.AncillaryProvisionPermittedTravelPeriods WHERE AncillaryProvisionId = {draft.Id}"));
        Assert.Equal(
            new[] { detail.TravelDate.Id.ToString() },
            await RowsAsync($"SELECT CAST(Id AS varchar(30)) AS Value FROM Ancillary.ProvisionTravelDateRules WHERE AncillaryProvisionId = {draft.Id}"));
        Assert.Equal((1, 0, 0), (await _proof.ListedProvisionsAsync(definitionId)).Select(row => (row.PermittedPeriodCount, row.BlackoutPeriodCount, row.DayTimeWindowCount)).Single());

        await RequestAsync(scope => scope.ActivateProvision.ActivateAsync(new TestActivateProvisionCommand(draft.Id)));

        Assert.Equal(("Active", 1), ((await ReadAsync(draft.Id)).Status.Name, (await ReadAsync(draft.Id)).TravelDate!.PermittedPeriods.Count));
    }

    [Fact]
    public async Task V121_D02_one_thousand_sparse_dates_are_one_thousand_single_day_rows_written_in_batches()
    {
        var definitionId = await ActiveDefinitionAsync();
        var days = Enumerable.Range(0, 1000).Select(offset => Day(2027, 1, 1).AddDays(offset * 2)).ToArray();
        var before = _database.Sql.Count;
        var draft = await RequestAsync(scope => scope.DefineProvision.DefineAsync(Free(definitionId) with { TravelDate = Dates(days.Select(day => Period(day, day)).ToArray()) }));
        var written = _database.Sql.Count - before;

        Assert.InRange(written, 2, 120);

        before = _database.Sql.Count;

        var detail = await ReadAsync(draft.Id);

        Assert.InRange(_database.Sql.Count - before, 1, 60);
        Assert.Equal(days.Select(day => (day, day)), Periods(detail.TravelDate!.PermittedPeriods));
        Assert.Equal(1000, detail.TravelDate.PermittedPeriods.Select(row => row.Id).Distinct().Count());
        Assert.Equal(
            new[] { "1000|1000|1" },
            await RowsAsync($"SELECT CONCAT((SELECT COUNT(*) FROM Ancillary.ProvisionPermittedTravelPeriods WHERE AncillaryProvisionId = {draft.Id}), '|', (SELECT COUNT(*) FROM ReadModel.AncillaryProvisionPermittedTravelPeriods WHERE AncillaryProvisionId = {draft.Id}), '|', (SELECT COUNT(*) FROM Ancillary.ProvisionTravelDateRules WHERE AncillaryProvisionId = {draft.Id})) AS Value"));

        before = _database.Sql.Count;

        var listed = (await _proof.ListedProvisionsAsync(definitionId)).Single();

        Assert.InRange(_database.Sql.Count - before, 1, 10);
        Assert.Equal(1000, listed.PermittedPeriodCount);
    }

    [Fact]
    public async Task V121_D03_D04_D10_permitted_and_blackout_periods_are_edited_row_by_row_and_stay_canonical()
    {
        var definitionId = await ActiveDefinitionAsync();
        var draft = await RequestAsync(scope => scope.DefineProvision.DefineAsync(Free(definitionId)));

        Assert.Null((await ReadAsync(draft.Id)).TravelDate);

        var april = await RequestAsync(scope => scope.AddPermittedTravelPeriod.AddAsync(new TestPermittedTravelPeriodRowCommand(draft.Id, 0, Day(2027, 4, 1), Day(2027, 4, 30))));
        var june = await RequestAsync(scope => scope.AddPermittedTravelPeriod.AddAsync(new TestPermittedTravelPeriodRowCommand(draft.Id, 0, Day(2027, 6, 1), Day(2027, 6, 30))));
        var ruleId = (await ReadAsync(draft.Id)).TravelDate!.Id;

        Assert.NotEqual(april.RowId, june.RowId);
        await RefusedAsync(16309, 409, scope => scope.AddPermittedTravelPeriod.AddAsync(new TestPermittedTravelPeriodRowCommand(draft.Id, 0, Day(2027, 4, 1), Day(2027, 4, 30))));
        await RefusedAsync(16302, 422, scope => scope.AddPermittedTravelPeriod.AddAsync(new TestPermittedTravelPeriodRowCommand(draft.Id, 0, Day(2027, 5, 2), Day(2027, 5, 1))));
        await RefusedAsync(16301, 404, scope => scope.AddPermittedTravelPeriod.AddAsync(new TestPermittedTravelPeriodRowCommand(999_999_999, 0, Day(2027, 5, 1), Day(2027, 5, 2))));

        var overlapping = await RequestAsync(scope => scope.AddPermittedTravelPeriod.AddAsync(new TestPermittedTravelPeriodRowCommand(draft.Id, 0, Day(2027, 4, 20), Day(2027, 5, 10))));

        Assert.Equal(april.RowId, overlapping.RowId);
        Assert.Equal(
            new[] { (april.RowId, Day(2027, 4, 1), Day(2027, 5, 10)), (june.RowId, Day(2027, 6, 1), Day(2027, 6, 30)) },
            (await ReadAsync(draft.Id)).TravelDate!.PermittedPeriods.Select(row => (row.Id, row.StartDate, row.EndDate)));

        var adjacent = await RequestAsync(scope => scope.AddPermittedTravelPeriod.AddAsync(new TestPermittedTravelPeriodRowCommand(draft.Id, 0, Day(2027, 5, 11), Day(2027, 5, 31))));

        Assert.Equal(april.RowId, adjacent.RowId);
        Assert.Equal(new[] { (Day(2027, 4, 1), Day(2027, 6, 30)) }, Periods((await ReadAsync(draft.Id)).TravelDate!.PermittedPeriods));
        Assert.Equal(
            new[] { $"{april.RowId}|{ruleId}" },
            await RowsAsync($"SELECT CONCAT(Id, '|', ProvisionTravelDateRuleId) AS Value FROM Ancillary.ProvisionPermittedTravelPeriods WHERE AncillaryProvisionId = {draft.Id}"));

        var blackout = await RequestAsync(scope => scope.AddBlackoutPeriod.AddAsync(new TestBlackoutPeriodRowCommand(draft.Id, 0, Day(2027, 4, 10), Day(2027, 4, 12))));

        await RefusedAsync(16309, 409, scope => scope.AddBlackoutPeriod.AddAsync(new TestBlackoutPeriodRowCommand(draft.Id, 0, Day(2027, 4, 10), Day(2027, 4, 12))));

        var changedBlackout = await RequestAsync(scope => scope.ChangeBlackoutPeriod.ChangeAsync(new TestBlackoutPeriodRowCommand(draft.Id, blackout.RowId, Day(2027, 4, 11), Day(2027, 4, 11))));
        var changedPeriod = await RequestAsync(scope => scope.ChangePermittedTravelPeriod.ChangeAsync(new TestPermittedTravelPeriodRowCommand(draft.Id, april.RowId, Day(2027, 4, 1), Day(2027, 4, 30))));

        await RefusedAsync(16308, 404, scope => scope.ChangePermittedTravelPeriod.ChangeAsync(new TestPermittedTravelPeriodRowCommand(draft.Id, blackout.RowId, Day(2027, 9, 1), Day(2027, 9, 2))));
        await RefusedAsync(16308, 404, scope => scope.RemoveBlackoutPeriod.RemoveAsync(new TestBlackoutPeriodRowCommand(draft.Id, april.RowId, default, default)));

        var edited = await ReadAsync(draft.Id);

        Assert.Equal((blackout.RowId, april.RowId), (changedBlackout.RowId, changedPeriod.RowId));
        Assert.Equal((ruleId, 1, 1), (edited.TravelDate!.Id, edited.TravelDate.PermittedPeriods.Count, edited.TravelDate.BlackoutPeriods.Count));
        Assert.Equal(new[] { (Day(2027, 4, 1), Day(2027, 4, 30)) }, Periods(edited.TravelDate.PermittedPeriods));
        Assert.Equal(new[] { (Day(2027, 4, 11), Day(2027, 4, 11)) }, Periods(edited.TravelDate.BlackoutPeriods));
        Assert.Equal(
            new[] { "1|1|1|1" },
            await RowsAsync($"SELECT CONCAT((SELECT COUNT(*) FROM Ancillary.ProvisionPermittedTravelPeriods WHERE AncillaryProvisionId = {draft.Id}), '|', (SELECT COUNT(*) FROM Ancillary.ProvisionBlackoutPeriods WHERE AncillaryProvisionId = {draft.Id}), '|', (SELECT COUNT(*) FROM ReadModel.AncillaryProvisionPermittedTravelPeriods WHERE AncillaryProvisionId = {draft.Id}), '|', (SELECT COUNT(*) FROM ReadModel.AncillaryProvisionBlackoutPeriods WHERE AncillaryProvisionId = {draft.Id})) AS Value"));

        await RequestAsync(scope => scope.RemovePermittedTravelPeriod.RemoveAsync(new TestPermittedTravelPeriodRowCommand(draft.Id, april.RowId, default, default)));

        Assert.Equal((ruleId, 0, 1), ((await ReadAsync(draft.Id)).TravelDate!.Id, (await ReadAsync(draft.Id)).TravelDate!.PermittedPeriods.Count, (await ReadAsync(draft.Id)).TravelDate!.BlackoutPeriods.Count));

        await RequestAsync(scope => scope.RemoveBlackoutPeriod.RemoveAsync(new TestBlackoutPeriodRowCommand(draft.Id, blackout.RowId, default, default)));

        Assert.Null((await ReadAsync(draft.Id)).TravelDate);
        Assert.Equal(
            new[] { "0|0" },
            await RowsAsync($"SELECT CONCAT((SELECT COUNT(*) FROM Ancillary.ProvisionTravelDateRules WHERE AncillaryProvisionId = {draft.Id}), '|', (SELECT COUNT(*) FROM ReadModel.AncillaryProvisions WHERE Id = {draft.Id} AND TravelDateRuleId IS NOT NULL)) AS Value"));
    }

    [Fact]
    public async Task V121_T01_T04_T05_T10_day_time_windows_are_authored_row_by_row_with_a_weekday_mask()
    {
        var definitionId = await ActiveDefinitionAsync();
        var draft = await RequestAsync(scope => scope.DefineProvision.DefineAsync(Free(definitionId)));
        var office = await RequestAsync(scope => scope.AddDayTimeWindow.AddAsync(WindowRow(draft.Id, 0, Window(Weekdays, 9, 17))));
        var saturdayNight = await RequestAsync(scope => scope.AddDayTimeWindow.AddAsync(WindowRow(draft.Id, 0, Window(Saturday, 22))));
        var sundayMorning = await RequestAsync(scope => scope.AddDayTimeWindow.AddAsync(WindowRow(draft.Id, 0, Window(Sunday, toHour: 2))));
        var lunch = await RequestAsync(scope => scope.AddDayTimeWindow.AddAsync(WindowRow(draft.Id, 0, Window(Monday, 12, 13, DayTimeRestrictionEffect.Deny))));

        await RefusedAsync(16309, 409, scope => scope.AddDayTimeWindow.AddAsync(WindowRow(draft.Id, 0, Window(Weekdays, 9, 17))));
        await RefusedAsync(16302, 422, scope => scope.AddDayTimeWindow.AddAsync(WindowRow(draft.Id, 0, Window(0, 9, 17))));
        await RefusedAsync(16302, 422, scope => scope.AddDayTimeWindow.AddAsync(WindowRow(draft.Id, 0, Window(128, 9, 17))));
        await RefusedAsync(16302, 422, scope => scope.AddDayTimeWindow.AddAsync(WindowRow(draft.Id, 0, Window(Monday, 17, 9))));
        await RefusedAsync(16302, 422, scope => scope.AddDayTimeWindow.AddAsync(WindowRow(draft.Id, 0, Window(Monday, 9, 17, (DayTimeRestrictionEffect)9))));
        await RefusedAsync(16308, 404, scope => scope.ChangeDayTimeWindow.ChangeAsync(WindowRow(draft.Id, 424242, Window(Monday, 9, 17))));

        var detail = await ReadAsync(draft.Id);

        Assert.Equal(
            new[]
            {
                (office.RowId, (byte)31, "Monday,Tuesday,Wednesday,Thursday,Friday", (int?)9, (int?)17, "Allow"),
                (saturdayNight.RowId, (byte)32, "Saturday", 22, null, "Allow"),
                (sundayMorning.RowId, (byte)64, "Sunday", null, 2, "Allow"),
                (lunch.RowId, (byte)1, "Monday", 12, 13, "Deny")
            },
            detail.DayTimeApplication!.Windows.Select(row => (row.Id, row.DaysOfWeekMask, string.Join(",", row.DaysOfWeek), row.StartLocalTime?.Hour, row.EndLocalTime?.Hour, row.Effect.Name)));

        var changed = await RequestAsync(scope => scope.ChangeDayTimeWindow.ChangeAsync(WindowRow(draft.Id, lunch.RowId, Window((byte)(Monday | Friday), 12, 14, DayTimeRestrictionEffect.Deny))));

        await RefusedAsync(16309, 409, scope => scope.ChangeDayTimeWindow.ChangeAsync(WindowRow(draft.Id, lunch.RowId, Window(Weekdays, 9, 17))));
        await RequestAsync(scope => scope.RemoveDayTimeWindow.RemoveAsync(WindowRow(draft.Id, saturdayNight.RowId, Window(Saturday))));
        await RequestAsync(scope => scope.RemoveDayTimeWindow.RemoveAsync(WindowRow(draft.Id, sundayMorning.RowId, Window(Sunday))));

        var edited = await ReadAsync(draft.Id);

        Assert.Equal(lunch.RowId, changed.RowId);
        Assert.Equal(detail.DayTimeApplication.Id, edited.DayTimeApplication!.Id);
        Assert.Equal(
            new[] { (office.RowId, (byte)31, "Allow"), (lunch.RowId, (byte)17, "Deny") },
            edited.DayTimeApplication.Windows.Select(row => (row.Id, row.DaysOfWeekMask, row.Effect.Name)));
        Assert.Equal(
            new[] { $"{office.RowId}|31|09:00:00|17:00:00|1", $"{lunch.RowId}|17|12:00:00|14:00:00|2" },
            (await RowsAsync($"SELECT CONCAT(Id, '|', DaysOfWeekMask, '|', CONVERT(varchar(8), StartLocalTime, 108), '|', CONVERT(varchar(8), EndLocalTime, 108), '|', Effect) AS Value FROM ReadModel.AncillaryProvisionDayTimeWindows WHERE AncillaryProvisionId = {draft.Id}")).OrderBy(row => row, StringComparer.Ordinal));
        Assert.Equal(2, (await _proof.ListedProvisionsAsync(definitionId)).Single().DayTimeWindowCount);

        await RequestAsync(scope => scope.ActivateProvision.ActivateAsync(new TestActivateProvisionCommand(draft.Id)));
        await RefusedAsync(16303, 409, scope => scope.AddDayTimeWindow.AddAsync(WindowRow(draft.Id, 0, Window(Sunday))));
        await RefusedAsync(16303, 409, scope => scope.RemoveDayTimeWindow.RemoveAsync(WindowRow(draft.Id, office.RowId, Window(Sunday))));
        await RefusedAsync(16303, 409, scope => scope.ChangeDayTimeWindow.ChangeAsync(WindowRow(draft.Id, office.RowId, Window(Sunday))));

        Assert.Equal(2, (await ReadAsync(draft.Id)).DayTimeApplication!.Windows.Count);
    }

    private static TestDefineProvisionCommand FullyRestricted(long definitionId)
        => Provision(definitionId, 10, CommercialDisposition.Free, applicationType: ProvisionApplicationType.Seat) with
        {
            PassengerEligibility = new([PassengerTypeCode.ADT, PassengerTypeCode.CHD], [new(2, 12), new(12, null)]),
            SalesRestrictions = new(
                new DateTimeOffset(2026, 11, 1, 0, 0, 0, TimeSpan.FromMinutes(210)),
                new DateTimeOffset(2027, 12, 1, 0, 0, 0, TimeSpan.FromMinutes(210)),
                [501, 502],
                [9001],
                [CustomerType.TravelAgency, CustomerType.Organization]),
            Geography = new(
                [Thr],
                [Ist],
                [Mhd],
                [Pair(Thr, Ist), Pair(Mhd, Ist, RoutePairDirection.BothDirections)],
                [Location(ServiceLocationType.Airport, Thr), Location(ServiceLocationType.City, 7)],
                [98, 90]),
            FlightApplication = new([1, 2], [3], [" w5112", "W5116"], [81234, 81240], [1, 2]),
            FareApplication = new([7001], [AirFareType.Public, AirFareType.Private], [5, 6], ["y26lt"], [2], [41, 42]),
            TravelDate = Dates([Period(Day(2027, 4, 1), Day(2027, 4, 30))], [Period(Day(2027, 4, 10), Day(2027, 4, 12))]),
            DayTimeApplication = DayTime(Window(Monday, 8, 12), Window(Monday, 9, 10, DayTimeRestrictionEffect.Deny)),
            AdvancePurchase = new(3, TimeUnit.Days),
            SeatApplication = Seat(["12a", "12F"], ["w", "LS"])
        };

    private static object?[] Shape(BackofficeProvisionDto detail)
        =>
        [
            string.Join(",", detail.PassengerEligibility?.AllowedPassengerTypes.Select(row => row.Value.Name) ?? []),
            string.Join(",", detail.PassengerEligibility?.AllowedAgeBands.Select(row => $"{row.AgeFromInclusive}-{row.AgeToExclusive}") ?? []),
            detail.SalesRestrictions?.SalesEffectiveFrom,
            detail.SalesRestrictions?.SalesDiscontinueAt,
            string.Join(",", detail.SalesRestrictions?.AllowedPointsOfSale.Select(row => row.Value) ?? []),
            string.Join(",", detail.SalesRestrictions?.AllowedCustomers.Select(row => row.Value) ?? []),
            string.Join(",", detail.SalesRestrictions?.AllowedCustomerTypes.Select(row => row.Value.Name) ?? []),
            string.Join(",", detail.Geography?.AllowedOriginAirports.Select(row => row.Value) ?? []),
            string.Join(",", detail.Geography?.AllowedDestinationAirports.Select(row => row.Value) ?? []),
            string.Join(",", detail.Geography?.AllowedViaAirports.Select(row => row.Value) ?? []),
            string.Join(",", detail.Geography?.AllowedRoutePairs.Select(row => $"{row.OriginAirportId}>{row.DestinationAirportId}:{row.Direction.Name}") ?? []),
            string.Join(",", detail.Geography?.ServiceLocations.Select(row => $"{row.LocationType.Name}:{row.LocationId}") ?? []),
            string.Join(",", detail.Geography?.CoverageCountries.Select(row => row.Value) ?? []),
            string.Join(",", detail.FlightApplication?.AllowedMarketingAirlines.Select(row => row.Value) ?? []),
            string.Join(",", detail.FlightApplication?.AllowedOperatingAirlines.Select(row => row.Value) ?? []),
            string.Join(",", detail.FlightApplication?.AllowedFlightNumbers.Select(row => row.Value) ?? []),
            string.Join(",", detail.FlightApplication?.AllowedFlights.Select(row => row.Value) ?? []),
            string.Join(",", detail.FlightApplication?.AllowedAircraft.Select(row => row.Value) ?? []),
            string.Join(",", detail.FareApplication?.AllowedAirFares.Select(row => row.Value) ?? []),
            string.Join(",", detail.FareApplication?.AllowedAirFareTypes.Select(row => row.Value.Name) ?? []),
            string.Join(",", detail.FareApplication?.AllowedFareFamilies.Select(row => row.Value) ?? []),
            string.Join(",", detail.FareApplication?.AllowedFareBases.Select(row => row.Value) ?? []),
            string.Join(",", detail.FareApplication?.AllowedCabinClasses.Select(row => row.Value) ?? []),
            string.Join(",", detail.FareApplication?.AllowedRbds.Select(row => row.Value) ?? []),
            string.Join(",", detail.TravelDate?.PermittedPeriods.Select(row => $"{row.StartDate:yyyy-MM-dd}..{row.EndDate:yyyy-MM-dd}") ?? []),
            string.Join(",", detail.TravelDate?.BlackoutPeriods.Select(row => $"{row.StartDate:yyyy-MM-dd}..{row.EndDate:yyyy-MM-dd}") ?? []),
            string.Join(",", detail.DayTimeApplication?.Windows.Select(row => $"{row.DaysOfWeekMask}:{row.StartLocalTime?.Hour}-{row.EndLocalTime?.Hour}:{row.Effect.Name}") ?? []),
            detail.AdvancePurchase is null ? null : $"{detail.AdvancePurchase.MinimumPeriod}:{detail.AdvancePurchase.Unit.Name}:{detail.AdvancePurchase.SameTimeAsTicketed}",
            detail.BaggageApplication is null ? null : $"{detail.BaggageApplication.Weight}:{detail.BaggageApplication.WeightUnit.Name}:{detail.BaggageApplication.PurchaseApplication.Name}",
            string.Join(",", detail.SeatApplication?.SeatNumbers.Select(row => row.Value) ?? []),
            string.Join(",", detail.SeatApplication?.SeatCharacteristics.Select(row => row.Value) ?? [])
        ];

    private static long[] RowIds(BackofficeProvisionDto detail)
        => new[]
            {
                detail.PassengerEligibility?.AllowedPassengerTypes.Select(row => row.Id), detail.PassengerEligibility?.AllowedAgeBands.Select(row => row.Id),
                detail.SalesRestrictions?.AllowedPointsOfSale.Select(row => row.Id), detail.SalesRestrictions?.AllowedCustomers.Select(row => row.Id),
                detail.SalesRestrictions?.AllowedCustomerTypes.Select(row => row.Id), detail.Geography?.AllowedOriginAirports.Select(row => row.Id),
                detail.Geography?.AllowedDestinationAirports.Select(row => row.Id), detail.Geography?.AllowedViaAirports.Select(row => row.Id),
                detail.Geography?.AllowedRoutePairs.Select(row => row.Id), detail.Geography?.ServiceLocations.Select(row => row.Id),
                detail.Geography?.CoverageCountries.Select(row => row.Id), detail.FlightApplication?.AllowedMarketingAirlines.Select(row => row.Id),
                detail.FlightApplication?.AllowedOperatingAirlines.Select(row => row.Id), detail.FlightApplication?.AllowedFlightNumbers.Select(row => row.Id),
                detail.FlightApplication?.AllowedFlights.Select(row => row.Id), detail.FlightApplication?.AllowedAircraft.Select(row => row.Id),
                detail.FareApplication?.AllowedAirFares.Select(row => row.Id), detail.FareApplication?.AllowedAirFareTypes.Select(row => row.Id),
                detail.FareApplication?.AllowedFareFamilies.Select(row => row.Id), detail.FareApplication?.AllowedFareBases.Select(row => row.Id),
                detail.FareApplication?.AllowedCabinClasses.Select(row => row.Id), detail.FareApplication?.AllowedRbds.Select(row => row.Id),
                detail.TravelDate?.PermittedPeriods.Select(row => row.Id), detail.TravelDate?.BlackoutPeriods.Select(row => row.Id),
                detail.DayTimeApplication?.Windows.Select(row => row.Id), detail.SeatApplication?.SeatNumbers.Select(row => row.Id),
                detail.SeatApplication?.SeatCharacteristics.Select(row => row.Id)
            }
            .SelectMany(ids => ids ?? [])
            .ToArray();

    private static long?[] RuleIds(BackofficeProvisionDto detail)
        =>
        [
            detail.PassengerEligibility?.Id, detail.SalesRestrictions?.Id, detail.Geography?.Id, detail.FlightApplication?.Id, detail.FareApplication?.Id,
            detail.TravelDate?.Id, detail.DayTimeApplication?.Id, detail.AdvancePurchase?.Id, detail.BaggageApplication?.Id, detail.SeatApplication?.Id
        ];

    [Fact]
    public async Task V121_M10_every_rule_group_round_trips_through_the_command_store_the_read_model_and_the_backoffice()
    {
        var definitionId = await ActiveDefinitionAsync();
        var command = FullyRestricted(definitionId);
        var draft = await RequestAsync(scope => scope.DefineProvision.DefineAsync(command));
        var detail = await ReadAsync(draft.Id);

        Assert.Equal(
            new object?[]
            {
                "ADT,CHD", "2-12,12-", command.SalesRestrictions!.SalesEffectiveFrom, command.SalesRestrictions.SalesDiscontinueAt, "501,502", "9001", "TravelAgency,Organization",
                "1", "6", "3", "1>6:Directional,3>6:BothDirections", "Airport:1,City:7", "98,90", "1,2", "3", "W5112,W5116", "81234,81240", "1,2",
                "7001", "Public,Private", "5,6", "Y26LT", "2", "41,42", "2027-04-01..2027-04-30", "2027-04-10..2027-04-12", "1:8-12:Allow,1:9-10:Deny",
                "3:Days:False", null, "12A,12F", "W,LS"
            },
            Shape(detail));
        Assert.Equal(("Seat", "FlightDeparture", "Draft"), (detail.ApplicationType.Name, detail.ServiceDateBasis!.Name, detail.Status.Name));
        Assert.Null(detail.BaggageApplication);
        Assert.Equal(9, RuleIds(detail).Count(id => id is not null));
        Assert.Equal(9, RuleIds(detail).Where(id => id is not null).Distinct().Count());
        Assert.Equal(RowIds(detail).Length, RowIds(detail).Distinct().Count());

        await using (var reader = new AncillaryScope(_database, _clock))
        {
            var aggregate = (await reader.Provisions.GetAsync(draft.Id))!;
            var row = await reader.Query.AncillaryProvisions.AsNoTracking().SingleAsync(provision => provision.Id == draft.Id);

            Assert.Equal(
                new long?[]
                {
                    aggregate.PassengerEligibility!.Id, aggregate.SalesRestrictions!.Id, aggregate.Geography!.Id, aggregate.FlightApplication!.Id, aggregate.FareApplication!.Id,
                    aggregate.TravelDate!.Id, aggregate.DayTimeApplication!.Id, aggregate.AdvancePurchase!.Id, null, aggregate.SeatApplication!.Id
                },
                RuleIds(detail));
            Assert.Equal(
                RuleIds(detail),
                new long?[]
                {
                    row.PassengerEligibilityRuleId, row.SalesRestrictionsRuleId, row.GeographyRuleId, row.FlightApplicationRuleId, row.FareApplicationRuleId,
                    row.TravelDateRuleId, row.DayTimeApplicationRuleId, row.AdvancePurchaseRuleId, row.BaggageApplicationRuleId, row.SeatApplicationRuleId
                });
            Assert.Equal(
                (command.SalesRestrictions.SalesEffectiveFrom, command.SalesRestrictions.SalesDiscontinueAt, 3, TimeUnit.Days),
                (row.SalesEffectiveFrom, row.SalesDiscontinueAt, row.AdvancePurchasePeriod!.Value, row.AdvancePurchaseUnit!.Value));
            Assert.Equal(
                aggregate.Geography.RoutePairs.Select(pair => pair.Id).Concat(aggregate.SeatApplication.SeatNumbers.Select(seat => seat.Id)),
                detail.Geography!.AllowedRoutePairs.Select(pair => pair.Id).Concat(detail.SeatApplication!.SeatNumbers.Select(seat => seat.Id)));
        }

        var orphans = await RowsAsync($"""
            SELECT CONCAT(t.name, ':', COUNT(*)) AS Value FROM (
                SELECT 'PassengerTypes' AS name, r.Id FROM Ancillary.ProvisionPassengerTypes r LEFT JOIN Ancillary.ProvisionPassengerEligibilityRules g ON g.Id = r.ProvisionPassengerEligibilityRuleId AND g.AncillaryProvisionId = r.AncillaryProvisionId WHERE g.Id IS NULL
                UNION ALL SELECT 'RoutePairs', r.Id FROM Ancillary.ProvisionRoutePairs r LEFT JOIN Ancillary.ProvisionGeographyRules g ON g.Id = r.ProvisionGeographyRuleId AND g.AncillaryProvisionId = r.AncillaryProvisionId WHERE g.Id IS NULL
                UNION ALL SELECT 'Flights', r.Id FROM Ancillary.ProvisionFlights r LEFT JOIN Ancillary.ProvisionFlightApplicationRules g ON g.Id = r.ProvisionFlightApplicationRuleId AND g.AncillaryProvisionId = r.AncillaryProvisionId WHERE g.Id IS NULL
                UNION ALL SELECT 'Rbds', r.Id FROM Ancillary.ProvisionRbds r LEFT JOIN Ancillary.ProvisionFareApplicationRules g ON g.Id = r.ProvisionFareApplicationRuleId AND g.AncillaryProvisionId = r.AncillaryProvisionId WHERE g.Id IS NULL
                UNION ALL SELECT 'Windows', r.Id FROM Ancillary.ProvisionDayTimeWindows r LEFT JOIN Ancillary.ProvisionDayTimeApplicationRules g ON g.Id = r.ProvisionDayTimeApplicationRuleId AND g.AncillaryProvisionId = r.AncillaryProvisionId WHERE g.Id IS NULL
                UNION ALL SELECT 'SeatNumbers', r.Id FROM Ancillary.ProvisionSeatNumbers r LEFT JOIN Ancillary.ProvisionSeatApplicationRules g ON g.Id = r.ProvisionSeatApplicationRuleId AND g.AncillaryProvisionId = r.AncillaryProvisionId WHERE g.Id IS NULL
            ) t GROUP BY t.name
            """);

        Assert.Empty(orphans);
    }

    [Fact]
    public async Task V121_M10_M07_each_typed_group_endpoint_replaces_only_its_group_and_keeps_the_identity_of_unchanged_rows()
    {
        var definitionId = await ActiveDefinitionAsync();
        var command = FullyRestricted(definitionId);
        var draft = await RequestAsync(scope => scope.DefineProvision.DefineAsync(command));
        var before = await ReadAsync(draft.Id);

        await RequestAsync(scope => scope.ChangePassengerEligibility.ChangeAsync(new TestChangeProvisionPassengerEligibilityCommand(draft.Id, new([PassengerTypeCode.INF, PassengerTypeCode.ADT]))));
        await RequestAsync(scope => scope.ChangeSalesRestrictions.ChangeAsync(new TestChangeProvisionSalesRestrictionsCommand(draft.Id, new(AllowedPointOfSaleIds: [502, 503]))));
        await RequestAsync(scope => scope.ChangeGeography.ChangeAsync(new TestChangeProvisionGeographyCommand(draft.Id, new(AllowedRoutePairs: [Pair(Thr, Ist)], CoverageCountryIds: [90]))));
        await RequestAsync(scope => scope.ChangeFlightApplication.ChangeAsync(new TestChangeProvisionFlightApplicationCommand(draft.Id, new(AllowedFlightIds: [81240], AllowedAircraftIds: [2, 3]))));
        await RequestAsync(scope => scope.ChangeFareApplication.ChangeAsync(new TestChangeProvisionFareApplicationCommand(draft.Id, null)));
        await RequestAsync(scope => scope.ChangeTravelDate.ChangeAsync(new TestChangeProvisionTravelDateCommand(draft.Id, Dates([Period(Day(2027, 4, 1), Day(2027, 4, 30)), Period(Day(2027, 5, 10), Day(2027, 5, 15))]))));
        await RequestAsync(scope => scope.ChangeDayTimeApplication.ChangeAsync(new TestChangeProvisionDayTimeApplicationCommand(draft.Id, DayTime(Window(Monday, 8, 12), Window(Friday, effect: DayTimeRestrictionEffect.Deny)))));
        await RequestAsync(scope => scope.ChangeAdvancePurchase.ChangeAsync(new TestChangeProvisionAdvancePurchaseCommand(draft.Id, new(48, TimeUnit.Hours, true))));
        await RequestAsync(scope => scope.ChangeSeatApplication.ChangeAsync(new TestChangeProvisionSeatApplicationCommand(draft.Id, Seat(["12F", "14C"], null))));

        var after = await ReadAsync(draft.Id);

        Assert.Equal(
            new object?[]
            {
                "ADT,INF", "", null, null, "502,503", "", "", "", "", "", "1>6:Directional", "", "90", "", "", "", "81240", "2,3",
                "", "", "", "", "", "", "2027-04-01..2027-04-30,2027-05-10..2027-05-15", "", "1:8-12:Allow,16:-:Deny", "48:Hours:True", null, "12F,14C", ""
            },
            Shape(after));
        Assert.Null(after.FareApplication);
        Assert.Equal(
            new[] { before.PassengerEligibility!.Id, before.SalesRestrictions!.Id, before.Geography!.Id, before.FlightApplication!.Id, before.TravelDate!.Id, before.DayTimeApplication!.Id, before.AdvancePurchase!.Id, before.SeatApplication!.Id },
            new[] { after.PassengerEligibility!.Id, after.SalesRestrictions!.Id, after.Geography!.Id, after.FlightApplication!.Id, after.TravelDate!.Id, after.DayTimeApplication!.Id, after.AdvancePurchase!.Id, after.SeatApplication!.Id });
        Assert.Equal(
            new[]
            {
                before.PassengerEligibility.AllowedPassengerTypes.Single(row => row.Value.Name == "ADT").Id,
                before.SalesRestrictions.AllowedPointsOfSale.Single(row => row.Value == 502).Id,
                before.Geography.AllowedRoutePairs.Single(row => row.DestinationAirportId == Ist && row.OriginAirportId == Thr).Id,
                before.Geography.CoverageCountries.Single(row => row.Value == 90).Id,
                before.FlightApplication.AllowedFlights.Single(row => row.Value == 81240).Id,
                before.FlightApplication.AllowedAircraft.Single(row => row.Value == 2).Id,
                before.TravelDate.PermittedPeriods.Single().Id,
                before.DayTimeApplication.Windows.Single(row => row.Effect.Name == "Allow").Id,
                before.SeatApplication.SeatNumbers.Single(row => row.Value == "12F").Id
            },
            new[]
            {
                after.PassengerEligibility.AllowedPassengerTypes.Single(row => row.Value.Name == "ADT").Id,
                after.SalesRestrictions.AllowedPointsOfSale.Single(row => row.Value == 502).Id,
                after.Geography.AllowedRoutePairs.Single().Id,
                after.Geography.CoverageCountries.Single().Id,
                after.FlightApplication.AllowedFlights.Single().Id,
                after.FlightApplication.AllowedAircraft.Single(row => row.Value == 2).Id,
                after.TravelDate.PermittedPeriods.Single(row => row.StartDate == Day(2027, 4, 1)).Id,
                after.DayTimeApplication.Windows.Single(row => row.Effect.Name == "Allow").Id,
                after.SeatApplication.SeatNumbers.Single(row => row.Value == "12F").Id
            });

        await RefusedAsync(16302, 422, scope => scope.ChangeSeatApplication.ChangeAsync(new TestChangeProvisionSeatApplicationCommand(draft.Id, null)));
        await RefusedAsync(16302, 422, scope => scope.ChangeFlightApplication.ChangeAsync(new TestChangeProvisionFlightApplicationCommand(draft.Id, null)));
        await RefusedAsync(16302, 422, scope => scope.ChangeBaggageApplication.ChangeAsync(new TestChangeProvisionBaggageApplicationCommand(draft.Id, Baggage(23m))));
        await RefusedAsync(16302, 422, scope => scope.ChangeGeography.ChangeAsync(new TestChangeProvisionGeographyCommand(draft.Id, new(AllowedRoutePairs: [Pair(Thr, Thr)]))));
        await RefusedAsync(16301, 404, scope => scope.ChangeTravelDate.ChangeAsync(new TestChangeProvisionTravelDateCommand(999_999_999, null)));

        Assert.Equal(Shape(after), Shape(await ReadAsync(draft.Id)));

        await RequestAsync(scope => scope.ChangePassengerEligibility.ChangeAsync(new TestChangeProvisionPassengerEligibilityCommand(draft.Id, null)));
        await RequestAsync(scope => scope.ChangeSalesRestrictions.ChangeAsync(new TestChangeProvisionSalesRestrictionsCommand(draft.Id, new())));
        await RequestAsync(scope => scope.ChangeGeography.ChangeAsync(new TestChangeProvisionGeographyCommand(draft.Id, null)));
        await RequestAsync(scope => scope.ChangeTravelDate.ChangeAsync(new TestChangeProvisionTravelDateCommand(draft.Id, null)));
        await RequestAsync(scope => scope.ChangeDayTimeApplication.ChangeAsync(new TestChangeProvisionDayTimeApplicationCommand(draft.Id, DayTime())));
        await RequestAsync(scope => scope.ChangeAdvancePurchase.ChangeAsync(new TestChangeProvisionAdvancePurchaseCommand(draft.Id, null)));

        var cleared = await ReadAsync(draft.Id);

        Assert.Equal(new long?[] { null, null, null, after.FlightApplication.Id, null, null, null, null, null, after.SeatApplication.Id }, RuleIds(cleared));
        Assert.Equal(
            new[] { "0" },
            await RowsAsync($"SELECT CAST((SELECT COUNT(*) FROM Ancillary.ProvisionPassengerEligibilityRules WHERE AncillaryProvisionId = {draft.Id}) + (SELECT COUNT(*) FROM Ancillary.ProvisionPassengerTypes WHERE AncillaryProvisionId = {draft.Id}) + (SELECT COUNT(*) FROM Ancillary.ProvisionTravelDateRules WHERE AncillaryProvisionId = {draft.Id}) + (SELECT COUNT(*) FROM Ancillary.ProvisionPermittedTravelPeriods WHERE AncillaryProvisionId = {draft.Id}) + (SELECT COUNT(*) FROM Ancillary.ProvisionAdvancePurchaseRules WHERE AncillaryProvisionId = {draft.Id}) + (SELECT COUNT(*) FROM ReadModel.AncillaryProvisionPermittedTravelPeriods WHERE AncillaryProvisionId = {draft.Id}) + (SELECT COUNT(*) FROM ReadModel.AncillaryProvisionDayTimeWindows WHERE AncillaryProvisionId = {draft.Id}) AS varchar(10)) AS Value"));
    }

    [Fact]
    public async Task V121_B_an_unrestricted_provision_has_no_rule_group_row_and_every_rule_table_is_typed()
    {
        var definitionId = await ActiveDefinitionAsync();
        var rule = await _proof.RuleAsync(Free(definitionId));

        Assert.All(RuleIds(rule.Provision), Assert.Null);
        Assert.Equal("Active", rule.Provision.Status.Name);

        var tables = await RowsAsync($"SELECT s.name + '.' + t.name AS Value FROM sys.tables t JOIN sys.schemas s ON s.schema_id = t.schema_id WHERE t.name LIKE '%Provision%' AND t.name NOT LIKE '%PriceLines' AND t.name NOT IN ('AncillaryProvisions') ORDER BY 1");
        string[] legacy = ["ProvisionTravelDates", "ProvisionSeasonalPeriods", "ProvisionDayTimeRestrictions"];
        var command = tables.Where(table => table.StartsWith("Ancillary.", StringComparison.Ordinal)).Select(table => table["Ancillary.".Length..]).ToList();
        var read = tables.Where(table => table.StartsWith("ReadModel.", StringComparison.Ordinal)).Select(table => table["ReadModel.AncillaryProvision".Length..]).ToList();

        Assert.Equal(10, command.Count(table => table.EndsWith("Rules", StringComparison.Ordinal)));
        Assert.Equal(27, command.Count(table => !table.EndsWith("Rules", StringComparison.Ordinal) && !legacy.Contains(table) && table != "ProvisionRuleMigrationAudit"));
        Assert.Equal(38, command.Count);
        Assert.Equal(27, read.Count);
        Assert.All(legacy, table => Assert.DoesNotContain(table, command));

        var withoutRuleKey = await RowsAsync($"""
            SELECT t.name AS Value FROM sys.tables t JOIN sys.schemas s ON s.schema_id = t.schema_id
            WHERE s.name = 'Ancillary' AND t.name LIKE 'Provision%' AND t.name NOT LIKE '%Rules' AND t.name NOT LIKE '%PriceLines'
              AND t.name NOT IN ('ProvisionTravelDates', 'ProvisionSeasonalPeriods', 'ProvisionDayTimeRestrictions', 'ProvisionRuleMigrationAudit')
              AND NOT EXISTS (
                  SELECT 1 FROM sys.foreign_key_columns k JOIN sys.columns c ON c.object_id = k.parent_object_id AND c.column_id = k.parent_column_id
                  JOIN sys.tables p ON p.object_id = k.referenced_object_id
                  WHERE k.parent_object_id = t.object_id AND c.name LIKE 'Provision%RuleId' AND c.is_nullable = 0 AND p.name LIKE 'Provision%Rules')
            """);
        var rulesWithoutOwner = await RowsAsync($"""
            SELECT t.name AS Value FROM sys.tables t JOIN sys.schemas s ON s.schema_id = t.schema_id
            WHERE s.name = 'Ancillary' AND t.name LIKE 'Provision%Rules'
              AND NOT EXISTS (
                  SELECT 1 FROM sys.indexes i JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
                  JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
                  WHERE i.object_id = t.object_id AND i.is_unique = 1 AND c.name = 'AncillaryProvisionId'
                    AND (SELECT COUNT(*) FROM sys.index_columns x WHERE x.object_id = i.object_id AND x.index_id = i.index_id) = 1)
            """);
        var suspicious = await RowsAsync($"""
            SELECT s.name + '.' + t.name + '.' + c.name AS Value FROM sys.columns c JOIN sys.tables t ON t.object_id = c.object_id
            JOIN sys.schemas s ON s.schema_id = t.schema_id
            WHERE ((t.name LIKE 'Provision%' AND s.name = 'Ancillary') OR (t.name LIKE 'AncillaryProvision_%' AND s.name = 'ReadModel') OR t.name LIKE 'AncillaryPricing%')
              AND t.name NOT LIKE '%PriceLines' AND t.name NOT IN ('AncillaryProvisions', 'ProvisionRuleMigrationAudit')
              AND (c.max_length = -1 OR c.name IN ('ConditionType', 'Operator', 'Value', 'Values', 'Expression', 'Json', 'Rule', 'Rules', 'Negate'))
            """);

        Assert.Empty(withoutRuleKey);
        Assert.Empty(rulesWithoutOwner);
        Assert.Empty(suspicious);
    }

    [Fact]
    public async Task V121_M08_an_active_provision_is_immutable_and_a_new_draft_supersedes_it_at_the_same_sequence()
    {
        var definitionId = await ActiveDefinitionAsync();
        var original = Free(definitionId) with { TravelDate = Dates([Period(Day(2027, 4, 1), Day(2027, 4, 30))]), PassengerEligibility = Passengers(PassengerTypeCode.ADT) };
        var published = (await _proof.RuleAsync(original)).Provision;
        var periodId = published.TravelDate!.PermittedPeriods.Single().Id;

        await RefusedAsync(16303, 409, scope => scope.ChangeProvision.ChangeAsync(Change(published.Id, original with { TravelDate = null })));
        await RefusedAsync(16303, 409, scope => scope.ChangeTravelDate.ChangeAsync(new TestChangeProvisionTravelDateCommand(published.Id, null)));
        await RefusedAsync(16303, 409, scope => scope.ChangePassengerEligibility.ChangeAsync(new TestChangeProvisionPassengerEligibilityCommand(published.Id, Passengers(PassengerTypeCode.CHD))));
        await RefusedAsync(16303, 409, scope => scope.ChangeGeography.ChangeAsync(new TestChangeProvisionGeographyCommand(published.Id, new(AllowedOriginAirportIds: [Thr]))));
        await RefusedAsync(16303, 409, scope => scope.ChangePermittedTravelPeriod.ChangeAsync(new TestPermittedTravelPeriodRowCommand(published.Id, periodId, Day(2027, 4, 1), Day(2027, 4, 29))));
        await RefusedAsync(16303, 409, scope => scope.RemovePermittedTravelPeriod.RemoveAsync(new TestPermittedTravelPeriodRowCommand(published.Id, periodId, default, default)));
        await RefusedAsync(16303, 409, scope => scope.AddBlackoutPeriod.AddAsync(new TestBlackoutPeriodRowCommand(published.Id, 0, Day(2027, 4, 10), Day(2027, 4, 12))));
        await RefusedAsync(16303, 409, scope => scope.AddDayTimeWindow.AddAsync(WindowRow(published.Id, 0, Window(Monday))));

        _clock.Now = _clock.Now.AddHours(1);

        var replacement = (await _proof.RuleAsync(original with { TravelDate = Dates([Period(Day(2027, 4, 1), Day(2027, 5, 31))], [Period(Day(2027, 4, 10), Day(2027, 4, 12))]) })).Provision;
        var retired = await ReadAsync(published.Id);

        Assert.NotEqual(published.Id, replacement.Id);
        Assert.Equal(("Retired", _clock.Now), (retired.Status.Name, retired.RetiredAt!.Value));
        Assert.Equal((periodId, Day(2027, 4, 1), Day(2027, 4, 30)), retired.TravelDate!.PermittedPeriods.Select(row => (row.Id, row.StartDate, row.EndDate)).Single());
        Assert.Equal(published.PassengerEligibility!.AllowedPassengerTypes.Single().Id, retired.PassengerEligibility!.AllowedPassengerTypes.Single().Id);
        Assert.Equal(("Active", 1, 1), (replacement.Status.Name, replacement.TravelDate!.PermittedPeriods.Count, replacement.TravelDate.BlackoutPeriods.Count));
        Assert.Equal(
            new[] { (replacement.Id.ToString(), "Active"), (published.Id.ToString(), "Retired") },
            (await _proof.ListedProvisionsAsync(definitionId)).Select(row => (row.Id, row.Status.Name)));
    }

    [Fact]
    public async Task V121_E17_two_drafts_racing_for_the_same_active_sequence_end_with_one_active_and_a_conflict()
    {
        var definitionId = await ActiveDefinitionAsync();
        var first = (await _proof.RuleAsync(Free(definitionId), publish: false)).Provision;
        var second = (await _proof.RuleAsync(Free(definitionId), publish: false)).Provision;

        await using var winner = new AncillaryScope(_database, _clock);
        await using var loser = new AncillaryScope(_database, _clock);

        var winning = (await winner.Provisions.GetAsync(first.Id))!;
        var losing = (await loser.Provisions.GetAsync(second.Id))!;
        var definition = (await winner.Definitions.GetAsync(definitionId))!;

        winning.Activate(definition, _clock.Now);
        losing.Activate(definition, _clock.Now);

        await winner.UnitOfWork.SaveChangesAsync();
        await BusinessAssert.ThrowsAsync(16306, 409, () => loser.UnitOfWork.SaveChangesAsync());

        await using var reader = new AncillaryScope(_database, _clock);

        Assert.Equal(
            new[] { first.Id },
            await reader.Command.AncillaryProvisions.AsNoTracking()
                .Where(row => row.ServiceDefinitionId == definitionId && row.Status == ProvisionStatus.Active)
                .Select(row => row.Id)
                .ToListAsync());
        Assert.Equal(ProvisionStatus.Draft, (await reader.Command.AncillaryProvisions.AsNoTracking().SingleAsync(row => row.Id == second.Id)).Status);
    }

    [Fact]
    public async Task V121_M07_two_concurrent_edits_of_one_draft_rule_group_conflict_instead_of_overwriting_or_overlapping()
    {
        var definitionId = await ActiveDefinitionAsync();
        var draftCommand = Free(definitionId) with { TravelDate = Dates([Period(Day(2027, 4, 1), Day(2027, 4, 10))]), PassengerEligibility = Passengers(PassengerTypeCode.ADT) };
        var draft = (await _proof.RuleAsync(draftCommand, publish: false)).Provision;

        await using (var firstEditor = new AncillaryScope(_database, _clock))
        await using (var secondEditor = new AncillaryScope(_database, _clock))
        {
            await firstEditor.Provisions.GetAsync(draft.Id);
            await secondEditor.Provisions.GetAsync(draft.Id);
            await firstEditor.ChangeProvision.ChangeAsync(Change(draft.Id, draftCommand with { Sequence = 20 }));
            await BusinessAssert.ThrowsAsync(16005, 409, () => secondEditor.ChangeProvision.ChangeAsync(Change(draft.Id, draftCommand with { Sequence = 30 })));
        }

        await using (var firstEditor = new AncillaryScope(_database, _clock))
        await using (var secondEditor = new AncillaryScope(_database, _clock))
        {
            await firstEditor.Provisions.GetAsync(draft.Id);
            await secondEditor.Provisions.GetAsync(draft.Id);
            await firstEditor.AddPermittedTravelPeriod.AddAsync(new TestPermittedTravelPeriodRowCommand(draft.Id, 0, Day(2027, 5, 1), Day(2027, 5, 10)));
            await BusinessAssert.ThrowsAsync(16005, 409, () => secondEditor.AddPermittedTravelPeriod.AddAsync(new TestPermittedTravelPeriodRowCommand(draft.Id, 0, Day(2027, 5, 5), Day(2027, 5, 20))));
        }

        await using (var firstEditor = new AncillaryScope(_database, _clock))
        await using (var secondEditor = new AncillaryScope(_database, _clock))
        await using (var thirdEditor = new AncillaryScope(_database, _clock))
        {
            await firstEditor.Provisions.GetAsync(draft.Id);
            await secondEditor.Provisions.GetAsync(draft.Id);
            await thirdEditor.Provisions.GetAsync(draft.Id);
            await firstEditor.ChangeDayTimeApplication.ChangeAsync(new TestChangeProvisionDayTimeApplicationCommand(draft.Id, DayTime(Window(Monday, 9, 10))));
            await BusinessAssert.ThrowsAsync(16005, 409, () => secondEditor.ChangePassengerEligibility.ChangeAsync(new TestChangeProvisionPassengerEligibilityCommand(draft.Id, Passengers(PassengerTypeCode.CHD))));
            await BusinessAssert.ThrowsAsync(16005, 409, () => thirdEditor.AddDayTimeWindow.AddAsync(WindowRow(draft.Id, 0, Window(Monday, 8, 12, DayTimeRestrictionEffect.Deny))));
        }

        var stored = await ReadAsync(draft.Id);

        Assert.Equal(20, stored.Sequence);
        Assert.Equal(
            new[] { (draft.TravelDate!.PermittedPeriods.Single().Id, Day(2027, 4, 1), Day(2027, 4, 10)), (stored.TravelDate!.PermittedPeriods[1].Id, Day(2027, 5, 1), Day(2027, 5, 10)) },
            stored.TravelDate.PermittedPeriods.Select(row => (row.Id, row.StartDate, row.EndDate)));
        Assert.Equal((draft.PassengerEligibility!.AllowedPassengerTypes.Single().Id, "ADT"), stored.PassengerEligibility!.AllowedPassengerTypes.Select(row => (row.Id, row.Value.Name)).Single());
        Assert.Equal((byte)1, stored.DayTimeApplication!.Windows.Single().DaysOfWeekMask);

        await RequestAsync(scope => scope.AddPermittedTravelPeriod.AddAsync(new TestPermittedTravelPeriodRowCommand(draft.Id, 0, Day(2027, 5, 5), Day(2027, 5, 20))));

        Assert.Equal(new[] { (Day(2027, 4, 1), Day(2027, 4, 10)), (Day(2027, 5, 1), Day(2027, 5, 20)) }, Periods((await ReadAsync(draft.Id)).TravelDate!.PermittedPeriods));
    }
}
