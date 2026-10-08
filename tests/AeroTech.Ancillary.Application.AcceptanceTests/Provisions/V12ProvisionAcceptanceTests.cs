using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.AncillaryProvisionAggregate.Commands.DefineAncillaryProvision;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryProvisionAggregate.Queries.GetAncillaryProvisionsPaginated.Backoffice;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;
using AeroTech.Messages.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.P1Commands;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.V12Commands;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Provisions;

[Collection(DatabaseCollection.Name)]
public class V12ProvisionAcceptanceTests
{
    private readonly TestDatabase _database;
    private readonly FixedClock _clock = new();
    private readonly FamilyProof _proof;

    public V12ProvisionAcceptanceTests(TestDatabase database)
    {
        _database = database;
        _proof = new FamilyProof(database, _clock);
    }

    private async Task<long> ActiveDefinitionAsync(string reference = "LOUNGE_ACCESS")
    {
        var airlineId = _database.NextAirlineId();
        var supplierId = await _proof.SupplierAsync(M1Commands.LocalSupplier(airlineId));

        return (await _proof.DefinitionAsync(CarrierDefinition(airlineId, supplierId, reference, "LNG", "F", "LG", "Lounge access"))).Id;
    }

    private static TestDefineProvisionCommand Free(long definitionId, int sequence = 10, ProvisionTravelCriteriaInput? travel = null)
        => Provision(definitionId, sequence, CommercialDisposition.Free, travel: travel);

    private static DateOnly Day(int offset) => new DateOnly(2027, 1, 1).AddDays(offset);

    [Fact]
    public async Task V12_N01_one_thousand_travel_dates_are_separate_rows_and_one_date_is_edited_or_deleted_alone()
    {
        var definitionId = await ActiveDefinitionAsync();
        var dates = Enumerable.Range(0, 1000).Select(Day).ToList();
        long provisionId;

        await using (var author = new AncillaryScope(_database, _clock))
            provisionId = (await author.DefineProvision.DefineAsync(Free(definitionId, travel: new ProvisionTravelCriteriaInput(TravelDates: dates)))).Id;

        Dictionary<DateOnly, long> before;

        await using (var reader = new AncillaryScope(_database, _clock))
        {
            var detail = await reader.GetProvisionById.ExecuteAsync(provisionId);

            Assert.Equal(1000, detail.Travel.TravelDates.Count);
            Assert.Equal(1000, detail.Travel.TravelDates.Select(row => row.Id).Distinct().Count());
            Assert.Equal(dates, detail.Travel.TravelDates.Select(row => row.Value).OrderBy(date => date));

            before = detail.Travel.TravelDates.ToDictionary(row => row.Value, row => row.Id);

            var commandRows = await reader.Command.Set<ProvisionTravelDate>().AsNoTracking()
                .Where(row => row.AncillaryProvisionId == provisionId)
                .ToDictionaryAsync(row => row.TravelDate, row => row.Id);
            var readRows = await reader.Query.AncillaryProvisionTravelDates.AsNoTracking()
                .Where(row => row.AncillaryProvisionId == provisionId)
                .ToDictionaryAsync(row => row.TravelDate, row => row.Id);

            Assert.Equal(before.OrderBy(pair => pair.Key), commandRows.OrderBy(pair => pair.Key));
            Assert.Equal(before.OrderBy(pair => pair.Key), readRows.OrderBy(pair => pair.Key));

            var columns = await reader.Command.Database
                .SqlQueryRaw<string>(
                    "SELECT c.name + ':' + t.name AS Value FROM sys.columns c JOIN sys.types t ON t.user_type_id = c.user_type_id " +
                    "WHERE c.object_id = OBJECT_ID('Ancillary.ProvisionTravelDates') ORDER BY c.column_id")
                .ToListAsync();

            Assert.Equal(
                new[] { "AncillaryProvisionId:bigint", "Id:bigint", "LastUpdateTime:datetimeoffset", "LastUpdatedBy:bigint", "TravelDate:date" },
                columns.OrderBy(column => column, StringComparer.Ordinal));

            var listed = (await reader.GetProvisionsPaginated.ExecuteAsync(
                new BackofficeGetAncillaryProvisionsPaginatedQuery { ServiceDefinitionId = definitionId })).Results.Single();

            Assert.Equal((provisionId.ToString(), 1000, 0, 0, 0), (listed.Id, listed.TravelDateCount, listed.SeasonalPeriodCount, listed.BlackoutPeriodCount, listed.DayTimeRestrictionCount));
            Assert.DoesNotContain(
                typeof(ProvisionPaginatedRowDto).GetProperties(),
                property => property.PropertyType != typeof(string) && typeof(System.Collections.IEnumerable).IsAssignableFrom(property.PropertyType));
        }

        long addedId;

        await using (var editor = new AncillaryScope(_database, _clock))
        {
            await editor.ChangeTravelDate.ChangeAsync(new TestTravelDateRowCommand(provisionId, before[Day(10)], new DateOnly(2030, 5, 5)));
            await editor.RemoveTravelDate.RemoveAsync(new TestTravelDateRowCommand(provisionId, before[Day(20)], default));

            addedId = (await editor.AddTravelDate.AddAsync(new TestTravelDateRowCommand(provisionId, 0, new DateOnly(2031, 1, 1)))).RowId;
        }

        await using (var editor = new AncillaryScope(_database, _clock))
        {
            await BusinessAssert.ThrowsAsync(16309, 409, () => editor.AddTravelDate.AddAsync(new TestTravelDateRowCommand(provisionId, 0, Day(30))));
            await BusinessAssert.ThrowsAsync(16309, 409, () => editor.ChangeTravelDate.ChangeAsync(new TestTravelDateRowCommand(provisionId, before[Day(31)], Day(30))));
            await BusinessAssert.ThrowsAsync(16308, 404, () => editor.RemoveTravelDate.RemoveAsync(new TestTravelDateRowCommand(provisionId, 987_654_321, default)));
            await BusinessAssert.ThrowsAsync(16301, 404, () => editor.AddTravelDate.AddAsync(new TestTravelDateRowCommand(987_654_321, 0, Day(1))));
        }

        await using (var reader = new AncillaryScope(_database, _clock))
        {
            var after = (await reader.GetProvisionById.ExecuteAsync(provisionId)).Travel.TravelDates;

            Assert.Equal(1000, after.Count);
            Assert.Equal(new DateOnly(2030, 5, 5), after.Single(row => row.Id == before[Day(10)]).Value);
            Assert.DoesNotContain(after, row => row.Id == before[Day(20)] || row.Value == Day(20));
            Assert.Equal(new DateOnly(2031, 1, 1), after.Single(row => row.Id == addedId).Value);
            Assert.DoesNotContain(before.Values, id => id == addedId);

            var untouched = after.Where(row => row.Id != before[Day(10)] && row.Id != addedId).ToList();

            Assert.Equal(998, untouched.Count);
            Assert.All(untouched, row => Assert.Equal(before[row.Value], row.Id));
            Assert.Equal(1000, await reader.Query.AncillaryProvisionTravelDates.CountAsync(row => row.AncillaryProvisionId == provisionId));
        }

        await using (var publisher = new AncillaryScope(_database, _clock))
        {
            await publisher.ActivateProvision.ActivateAsync(new TestActivateProvisionCommand(provisionId));
            await BusinessAssert.ThrowsAsync(16303, 409, () => publisher.AddTravelDate.AddAsync(new TestTravelDateRowCommand(provisionId, 0, Day(20))));
            await BusinessAssert.ThrowsAsync(16303, 409, () => publisher.RemoveTravelDate.RemoveAsync(new TestTravelDateRowCommand(provisionId, addedId, default)));
        }

        await using (var reader = new AncillaryScope(_database, _clock))
        {
            var active = await reader.GetProvisionById.ExecuteAsync(provisionId);

            Assert.Equal(("Active", 1000), (active.Status.Name, active.Travel.TravelDates.Count));
        }
    }

    [Fact]
    public async Task V12_N02_seasons_and_blackouts_are_authored_row_by_row_and_survive_activation()
    {
        var definitionId = await ActiveDefinitionAsync();
        var seasons = new[]
        {
            Period(new DateOnly(2027, 3, 15), new DateOnly(2027, 4, 5)),
            Period(new DateOnly(2027, 6, 1), new DateOnly(2027, 8, 31)),
            Period(new DateOnly(2027, 12, 15), new DateOnly(2027, 12, 31))
        };
        var blackouts = Enumerable.Range(1, 10).Select(day => Period(new DateOnly(2027, 7, day * 2), new DateOnly(2027, 7, day * 2))).ToArray();
        long provisionId;

        await using (var author = new AncillaryScope(_database, _clock))
        {
            provisionId = (await author.DefineProvision.DefineAsync(
                Free(definitionId, travel: new ProvisionTravelCriteriaInput(SeasonalPeriods: seasons, BlackoutPeriods: blackouts)))).Id;

            await BusinessAssert.ThrowsAsync(16302, 422, () => author.DefineProvision.DefineAsync(Free(
                definitionId,
                20,
                new ProvisionTravelCriteriaInput(SeasonalPeriods: [Period(new DateOnly(2027, 4, 5), new DateOnly(2027, 3, 15))]))));
            await BusinessAssert.ThrowsAsync(16302, 422, () => author.DefineProvision.DefineAsync(Free(
                definitionId,
                20,
                new ProvisionTravelCriteriaInput(BlackoutPeriods: [blackouts[0], blackouts[0]]))));
        }

        long addedSeason;
        long singleton;

        await using (var editor = new AncillaryScope(_database, _clock))
        {
            var detail = await editor.GetProvisionById.ExecuteAsync(provisionId);

            Assert.Equal(seasons.Select(season => (season.StartDate, season.EndDate)), detail.Travel.SeasonalPeriods.Select(row => (row.StartDate, row.EndDate)));
            Assert.Equal(10, detail.Travel.BlackoutPeriods.Count);
            Assert.All(detail.Travel.BlackoutPeriods, row => Assert.Equal(row.StartDate, row.EndDate));

            addedSeason = (await editor.AddSeasonalPeriod.AddAsync(
                new TestSeasonalPeriodRowCommand(provisionId, 0, new DateOnly(2027, 3, 20), new DateOnly(2027, 4, 20)))).RowId;
            singleton = (await editor.AddBlackoutPeriod.AddAsync(
                new TestBlackoutPeriodRowCommand(provisionId, 0, new DateOnly(2027, 12, 25), new DateOnly(2027, 12, 25)))).RowId;

            await editor.ChangeSeasonalPeriod.ChangeAsync(
                new TestSeasonalPeriodRowCommand(provisionId, addedSeason, new DateOnly(2027, 9, 1), new DateOnly(2027, 9, 30)));
            await editor.ChangeBlackoutPeriod.ChangeAsync(
                new TestBlackoutPeriodRowCommand(provisionId, singleton, new DateOnly(2027, 12, 24), new DateOnly(2027, 12, 25)));
            await editor.RemoveBlackoutPeriod.RemoveAsync(
                new TestBlackoutPeriodRowCommand(provisionId, detail.Travel.BlackoutPeriods[0].Id, default, default));
            await editor.RemoveSeasonalPeriod.RemoveAsync(
                new TestSeasonalPeriodRowCommand(provisionId, detail.Travel.SeasonalPeriods[2].Id, default, default));
        }

        await using (var editor = new AncillaryScope(_database, _clock))
        {
            await BusinessAssert.ThrowsAsync(16302, 422, () => editor.AddSeasonalPeriod.AddAsync(
                new TestSeasonalPeriodRowCommand(provisionId, 0, new DateOnly(2027, 5, 2), new DateOnly(2027, 5, 1))));
            await BusinessAssert.ThrowsAsync(16302, 422, () => editor.ChangeBlackoutPeriod.ChangeAsync(
                new TestBlackoutPeriodRowCommand(provisionId, singleton, new DateOnly(2027, 12, 26), new DateOnly(2027, 12, 25))));
            await BusinessAssert.ThrowsAsync(16309, 409, () => editor.AddSeasonalPeriod.AddAsync(
                new TestSeasonalPeriodRowCommand(provisionId, 0, seasons[0].StartDate, seasons[0].EndDate)));
            await BusinessAssert.ThrowsAsync(16309, 409, () => editor.AddBlackoutPeriod.AddAsync(
                new TestBlackoutPeriodRowCommand(provisionId, 0, new DateOnly(2027, 12, 24), new DateOnly(2027, 12, 25))));
            await BusinessAssert.ThrowsAsync(16308, 404, () => editor.RemoveSeasonalPeriod.RemoveAsync(
                new TestSeasonalPeriodRowCommand(provisionId, singleton, default, default)));
        }

        await using (var publisher = new AncillaryScope(_database, _clock))
            await publisher.ActivateProvision.ActivateAsync(new TestActivateProvisionCommand(provisionId));

        await using (var reader = new AncillaryScope(_database, _clock))
        {
            var active = await reader.GetProvisionById.ExecuteAsync(provisionId);
            var listed = (await reader.GetProvisionsPaginated.ExecuteAsync(
                new BackofficeGetAncillaryProvisionsPaginatedQuery { ServiceDefinitionId = definitionId })).Results.Single();

            Assert.Equal("Active", active.Status.Name);
            Assert.Equal(
                new[]
                {
                    (new DateOnly(2027, 3, 15), new DateOnly(2027, 4, 5)),
                    (new DateOnly(2027, 6, 1), new DateOnly(2027, 8, 31)),
                    (new DateOnly(2027, 9, 1), new DateOnly(2027, 9, 30))
                },
                active.Travel.SeasonalPeriods.Select(row => (row.StartDate, row.EndDate)));
            Assert.Equal(addedSeason, active.Travel.SeasonalPeriods[2].Id);
            Assert.Equal(10, active.Travel.BlackoutPeriods.Count);
            Assert.Equal((new DateOnly(2027, 12, 24), new DateOnly(2027, 12, 25)), active.Travel.BlackoutPeriods.Where(row => row.Id == singleton).Select(row => (row.StartDate, row.EndDate)).Single());
            Assert.Equal((3, 10), (listed.SeasonalPeriodCount, listed.BlackoutPeriodCount));
            Assert.Equal(3, await reader.Command.Set<ProvisionSeasonalPeriod>().CountAsync(row => row.AncillaryProvisionId == provisionId));
            Assert.Equal(10, await reader.Query.AncillaryProvisionBlackoutPeriods.CountAsync(row => row.AncillaryProvisionId == provisionId));
            await BusinessAssert.ThrowsAsync(16303, 409, () => reader.RemoveBlackoutPeriod.RemoveAsync(
                new TestBlackoutPeriodRowCommand(provisionId, singleton, default, default)));
        }
    }

    [Fact]
    public async Task V12_N03_day_time_rows_allow_or_deny_a_local_weekday_window()
    {
        var definitionId = await ActiveDefinitionAsync();
        long provisionId;

        await using (var author = new AncillaryScope(_database, _clock))
        {
            provisionId = (await author.DefineProvision.DefineAsync(Free(
                definitionId,
                travel: new ProvisionTravelCriteriaInput(DayTimeRestrictions:
                [
                    DayTime(DayOfWeek.Monday, 8, 12),
                    DayTime(DayOfWeek.Monday, 9, 10, DayTimeRestrictionEffect.Deny)
                ])))).Id;

            await BusinessAssert.ThrowsAsync(16302, 422, () => author.DefineProvision.DefineAsync(Free(
                definitionId,
                20,
                new ProvisionTravelCriteriaInput(DayTimeRestrictions:
                [
                    new ProvisionDayTimeRestrictionInput(DayOfWeek.Monday, new TimeOnly(8, 0), null, DayTimeRestrictionEffect.Allow)
                ]))));
            await BusinessAssert.ThrowsAsync(16302, 422, () => author.DefineProvision.DefineAsync(Free(
                definitionId,
                20,
                new ProvisionTravelCriteriaInput(DayTimeRestrictions: [DayTime(DayOfWeek.Monday, 22, 2)]))));
        }

        long wholeDay;
        long moved;

        await using (var editor = new AncillaryScope(_database, _clock))
        {
            wholeDay = (await editor.AddDayTimeRestriction.AddAsync(
                new TestDayTimeRestrictionRowCommand(provisionId, 0, DayOfWeek.Friday, null, null, DayTimeRestrictionEffect.Deny))).RowId;
            moved = (await editor.AddDayTimeRestriction.AddAsync(
                new TestDayTimeRestrictionRowCommand(provisionId, 0, DayOfWeek.Saturday, new TimeOnly(22, 0), new TimeOnly(23, 59), DayTimeRestrictionEffect.Allow))).RowId;

            await editor.ChangeDayTimeRestriction.ChangeAsync(
                new TestDayTimeRestrictionRowCommand(provisionId, moved, DayOfWeek.Sunday, new TimeOnly(0, 0), new TimeOnly(2, 0), DayTimeRestrictionEffect.Allow));
            await BusinessAssert.ThrowsAsync(16309, 409, () => editor.AddDayTimeRestriction.AddAsync(
                new TestDayTimeRestrictionRowCommand(provisionId, 0, DayOfWeek.Monday, new TimeOnly(8, 0), new TimeOnly(12, 0), DayTimeRestrictionEffect.Allow)));
        }

        await using (var editor = new AncillaryScope(_database, _clock))
        {
            await BusinessAssert.ThrowsAsync(16302, 422, () => editor.AddDayTimeRestriction.AddAsync(
                new TestDayTimeRestrictionRowCommand(provisionId, 0, DayOfWeek.Tuesday, null, new TimeOnly(9, 0), DayTimeRestrictionEffect.Allow)));
            await BusinessAssert.ThrowsAsync(16302, 422, () => editor.ChangeDayTimeRestriction.ChangeAsync(
                new TestDayTimeRestrictionRowCommand(provisionId, moved, DayOfWeek.Sunday, new TimeOnly(3, 0), new TimeOnly(2, 0), DayTimeRestrictionEffect.Allow)));
        }

        await using (var reader = new AncillaryScope(_database, _clock))
        {
            var rows = (await reader.GetProvisionById.ExecuteAsync(provisionId)).Travel.DayTimeRestrictions;

            Assert.Equal(
                new[]
                {
                    ("Monday", (TimeOnly?)new TimeOnly(8, 0), (TimeOnly?)new TimeOnly(12, 0), "Allow"),
                    ("Monday", new TimeOnly(9, 0), new TimeOnly(10, 0), "Deny"),
                    ("Friday", null, null, "Deny"),
                    ("Sunday", new TimeOnly(0, 0), new TimeOnly(2, 0), "Allow")
                },
                rows.Select(row => (row.DayOfWeek.Name, row.StartTime, row.EndTime, row.Effect.Name)));
            Assert.Equal((wholeDay, moved), (rows[2].Id, rows[3].Id));
            Assert.Equal(4, await reader.Query.AncillaryProvisionDayTimeRestrictions.CountAsync(row => row.AncillaryProvisionId == provisionId));

            var columns = await reader.Command.Database
                .SqlQueryRaw<string>(
                    "SELECT c.name + ':' + t.name AS Value FROM sys.columns c JOIN sys.types t ON t.user_type_id = c.user_type_id " +
                    "WHERE c.object_id = OBJECT_ID('Ancillary.ProvisionDayTimeRestrictions') AND c.name IN ('DayOfWeek', 'StartTime', 'EndTime', 'Effect') ORDER BY c.column_id")
                .ToListAsync();

            Assert.Equal(
                new[] { "DayOfWeek:int", "Effect:int", "EndTime:time", "StartTime:time" },
                columns.OrderBy(column => column, StringComparer.Ordinal));
        }

        await using (var editor = new AncillaryScope(_database, _clock))
        {
            await editor.RemoveDayTimeRestriction.RemoveAsync(
                new TestDayTimeRestrictionRowCommand(provisionId, wholeDay, default, null, null, default));

            Assert.Equal(3, (await editor.GetProvisionById.ExecuteAsync(provisionId)).Travel.DayTimeRestrictions.Count);
        }
    }

    [Fact]
    public async Task V12_N04_every_selector_survives_the_command_store_the_read_model_the_backoffice_and_a_draft_edit()
    {
        var definitionId = await ActiveDefinitionAsync("SEAT_SELECT");
        var draft = Provision(
            definitionId,
            10,
            CommercialDisposition.Free,
            passenger: Passengers(PassengerTypeCode.ADT, PassengerTypeCode.CHD),
            sales: new ProvisionSalesCriteriaInput([501, 502], [9001], [CustomerType.TravelAgency]),
            travel: new ProvisionTravelCriteriaInput(
                OriginAirportIds: [Thr],
                DestinationAirportIds: [Mhd, Ist],
                ViaAirportIds: [Ika],
                RoutePairs: [Pair(Thr, Mhd), Pair(Thr, Ist, RoutePairDirection.BothDirections)],
                TravelDates: [new DateOnly(2027, 3, 21)],
                SeasonalPeriods: [Period(new DateOnly(2027, 3, 15), new DateOnly(2027, 4, 5))],
                BlackoutPeriods: [Period(new DateOnly(2027, 4, 1), new DateOnly(2027, 4, 2))],
                DayTimeRestrictions: [DayTime(DayOfWeek.Thursday, 5, 11)],
                MarketingAirlineIds: [1],
                OperatingAirlineIds: [1, 2],
                FlightNumbers: [" da2021 ", "2022"],
                FlightIds: [81234],
                AircraftIds: [320, 321]),
            fare: new ProvisionFareCriteriaInput([4001], [AirFareType.Private], [5, 6], [" ylow "], [1, 2], [15, 21]),
            advancePurchase: new ProvisionAdvancePurchaseInput(24, TimeUnit.Hours),
            application: Seat(["12a"], ["w", "LS"]));
        long provisionId;

        await using (var author = new AncillaryScope(_database, _clock))
            provisionId = (await author.DefineProvision.DefineAsync(draft)).Id;

        BackofficeProvisionDto first;

        await using (var reader = new AncillaryScope(_database, _clock))
        {
            first = await reader.GetProvisionById.ExecuteAsync(provisionId);

            Assert.Equal(new[] { "ADT", "CHD" }, first.Passenger.PassengerTypes.Select(row => row.Value.Name));
            Assert.Equal(new long[] { 501, 502 }, first.Sales.PointsOfSale.Select(row => row.Value));
            Assert.Equal(new long[] { 9001 }, first.Sales.Customers.Select(row => row.Value));
            Assert.Equal(new[] { "TravelAgency" }, first.Sales.CustomerTypes.Select(row => row.Value.Name));
            Assert.Equal(new[] { Thr }, first.Travel.OriginAirports.Select(row => row.Value));
            Assert.Equal(new[] { Mhd, Ist }, first.Travel.DestinationAirports.Select(row => row.Value));
            Assert.Equal(new[] { Ika }, first.Travel.ViaAirports.Select(row => row.Value));
            Assert.Equal(
                new[] { (Thr, Mhd, "Directional"), (Thr, Ist, "BothDirections") },
                first.Travel.RoutePairs.Select(row => (row.OriginAirportId, row.DestinationAirportId, row.Direction.Name)));
            Assert.Equal(new[] { new DateOnly(2027, 3, 21) }, first.Travel.TravelDates.Select(row => row.Value));
            Assert.Equal((new DateOnly(2027, 3, 15), new DateOnly(2027, 4, 5)), first.Travel.SeasonalPeriods.Select(row => (row.StartDate, row.EndDate)).Single());
            Assert.Equal((new DateOnly(2027, 4, 1), new DateOnly(2027, 4, 2)), first.Travel.BlackoutPeriods.Select(row => (row.StartDate, row.EndDate)).Single());
            Assert.Equal(("Thursday", "Allow"), first.Travel.DayTimeRestrictions.Select(row => (row.DayOfWeek.Name, row.Effect.Name)).Single());
            Assert.Equal(new[] { 1 }, first.Travel.MarketingAirlines.Select(row => row.Value));
            Assert.Equal(new[] { 1, 2 }, first.Travel.OperatingAirlines.Select(row => row.Value));
            Assert.Equal(new[] { "DA2021", "2022" }, first.Travel.FlightNumbers.Select(row => row.Value));
            Assert.Equal(new long[] { 81234 }, first.Travel.Flights.Select(row => row.Value));
            Assert.Equal(new[] { 320, 321 }, first.Travel.Aircraft.Select(row => row.Value));
            Assert.Equal(new long[] { 4001 }, first.Fare.AirFares.Select(row => row.Value));
            Assert.Equal(new[] { "Private" }, first.Fare.AirFareTypes.Select(row => row.Value.Name));
            Assert.Equal(new long[] { 5, 6 }, first.Fare.FareFamilies.Select(row => row.Value));
            Assert.Equal(new[] { "YLOW" }, first.Fare.FareBases.Select(row => row.Value));
            Assert.Equal(new[] { 1, 2 }, first.Fare.CabinClasses.Select(row => row.Value));
            Assert.Equal(new long[] { 15, 21 }, first.Fare.Rbds.Select(row => row.Value));
            Assert.Equal(new[] { "12A" }, first.Seat!.SeatNumbers.Select(row => row.Value));
            Assert.Equal(new[] { "W", "LS" }, first.Seat.SeatCharacteristics.Select(row => row.Value));
            Assert.Equal((24, "Hours"), (first.AdvancePurchase!.Period, first.AdvancePurchase.Unit.Name));

            Assert.Equal(
                first.Passenger.PassengerTypes.Select(row => (row.Id, (PassengerTypeCode)row.Value.Value)),
                (await reader.Command.Set<ProvisionPassengerType>().AsNoTracking().Where(row => row.AncillaryProvisionId == provisionId).OrderBy(row => row.Id).ToListAsync())
                    .Select(row => (row.Id, row.PassengerTypeCode)));
            Assert.Equal(
                first.Fare.FareFamilies.Select(row => (row.Id, row.Value)),
                (await reader.Query.AncillaryProvisionFareFamilies.AsNoTracking().Where(row => row.AncillaryProvisionId == provisionId).OrderBy(row => row.Id).ToListAsync())
                    .Select(row => (row.Id, row.FareFamilyId)));
            Assert.Equal(
                first.Seat.SeatCharacteristics.Select(row => (row.Id, row.Value)),
                (await reader.Query.AncillaryProvisionSeatCharacteristics.AsNoTracking().Where(row => row.AncillaryProvisionId == provisionId).OrderBy(row => row.Id).ToListAsync())
                    .Select(row => (row.Id, row.CharacteristicCode)));
        }

        var edited = draft with
        {
            Passenger = Passengers(PassengerTypeCode.ADT, PassengerTypeCode.INF),
            Sales = new ProvisionSalesCriteriaInput([502], null, [CustomerType.TravelAgency, CustomerType.Individual]),
            Travel = draft.Travel! with { RoutePairs = [Pair(Thr, Mhd)], AircraftIds = [320, 321, 330], FlightNumbers = ["2022", "2023"] },
            Fare = new ProvisionFareCriteriaInput(FareFamilyIds: [6, 7], RbdIds: [15])
        };

        await using (var editor = new AncillaryScope(_database, _clock))
            await editor.ChangeProvision.ChangeAsync(Change(provisionId, edited));

        await using (var reader = new AncillaryScope(_database, _clock))
        {
            var second = await reader.GetProvisionById.ExecuteAsync(provisionId);

            Assert.Equal(new[] { "ADT", "INF" }, second.Passenger.PassengerTypes.Select(row => row.Value.Name));
            Assert.Equal(first.Passenger.PassengerTypes[0].Id, second.Passenger.PassengerTypes[0].Id);
            Assert.Equal(new long[] { 502 }, second.Sales.PointsOfSale.Select(row => row.Value));
            Assert.Equal(first.Sales.PointsOfSale[1].Id, second.Sales.PointsOfSale.Single().Id);
            Assert.Empty(second.Sales.Customers);
            Assert.Equal(new[] { "TravelAgency", "Individual" }, second.Sales.CustomerTypes.Select(row => row.Value.Name));
            Assert.Equal((first.Travel.RoutePairs[0].Id, Mhd), second.Travel.RoutePairs.Select(row => (row.Id, row.DestinationAirportId)).Single());
            Assert.Equal(new[] { 320, 321, 330 }, second.Travel.Aircraft.Select(row => row.Value));
            Assert.Equal(first.Travel.Aircraft.Select(row => row.Id), second.Travel.Aircraft.Take(2).Select(row => row.Id));
            Assert.Equal(new[] { "2022", "2023" }, second.Travel.FlightNumbers.Select(row => row.Value));
            Assert.Equal(first.Travel.FlightNumbers[1].Id, second.Travel.FlightNumbers[0].Id);
            Assert.Equal(new long[] { 6, 7 }, second.Fare.FareFamilies.Select(row => row.Value));
            Assert.Equal(first.Fare.FareFamilies[1].Id, second.Fare.FareFamilies[0].Id);
            Assert.Equal(first.Fare.Rbds[0].Id, second.Fare.Rbds.Single().Id);
            Assert.Empty(second.Fare.AirFares);
            Assert.Empty(second.Fare.CabinClasses);
            Assert.Equal(first.Travel.TravelDates.Single().Id, second.Travel.TravelDates.Single().Id);
            Assert.Equal(first.Seat!.SeatNumbers.Single().Id, second.Seat!.SeatNumbers.Single().Id);
            Assert.Equal(1, await reader.Command.Set<ProvisionPointOfSale>().CountAsync(row => row.AncillaryProvisionId == provisionId));
            Assert.Equal(0, await reader.Query.AncillaryProvisionCustomers.CountAsync(row => row.AncillaryProvisionId == provisionId));
            Assert.Equal(1, await reader.Query.AncillaryProvisionRoutePairs.CountAsync(row => row.AncillaryProvisionId == provisionId));
        }
    }

    [Fact]
    public async Task V12_N05_an_unconditioned_provision_is_unrestricted_and_every_condition_table_is_typed()
    {
        var definitionId = await ActiveDefinitionAsync();
        var rule = await _proof.RuleAsync(Free(definitionId));
        var provision = rule.Provision;

        Assert.Empty(provision.Passenger.PassengerTypes);
        Assert.Empty(provision.Sales.PointsOfSale);
        Assert.Empty(provision.Sales.Customers);
        Assert.Empty(provision.Sales.CustomerTypes);
        Assert.Empty(provision.Travel.OriginAirports);
        Assert.Empty(provision.Travel.DestinationAirports);
        Assert.Empty(provision.Travel.ViaAirports);
        Assert.Empty(provision.Travel.RoutePairs);
        Assert.Empty(provision.Travel.TravelDates);
        Assert.Empty(provision.Travel.SeasonalPeriods);
        Assert.Empty(provision.Travel.BlackoutPeriods);
        Assert.Empty(provision.Travel.DayTimeRestrictions);
        Assert.Empty(provision.Travel.MarketingAirlines);
        Assert.Empty(provision.Travel.OperatingAirlines);
        Assert.Empty(provision.Travel.FlightNumbers);
        Assert.Empty(provision.Travel.Flights);
        Assert.Empty(provision.Travel.Aircraft);
        Assert.Empty(provision.Fare.AirFares);
        Assert.Empty(provision.Fare.AirFareTypes);
        Assert.Empty(provision.Fare.FareFamilies);
        Assert.Empty(provision.Fare.FareBases);
        Assert.Empty(provision.Fare.CabinClasses);
        Assert.Empty(provision.Fare.Rbds);
        Assert.Null(provision.Seat);
        Assert.Null(provision.AdvancePurchase);
        Assert.Equal("Active", provision.Status.Name);

        await using var reader = new AncillaryScope(_database, _clock);

        var tables = await reader.Command.Database
            .SqlQueryRaw<string>(
                "SELECT s.name + '.' + t.name AS Value FROM sys.tables t JOIN sys.schemas s ON s.schema_id = t.schema_id " +
                "WHERE t.name LIKE '%Provision%' AND t.name NOT LIKE '%PriceLines' AND t.name NOT IN ('AncillaryProvisions') ORDER BY 1")
            .ToListAsync();

        Assert.Equal(50, tables.Count);
        Assert.Equal(25, tables.Count(table => table.StartsWith("Ancillary.", StringComparison.Ordinal)));
        Assert.Equal(25, tables.Count(table => table.StartsWith("ReadModel.", StringComparison.Ordinal)));

        var suspicious = await reader.Command.Database
            .SqlQueryRaw<string>(
                "SELECT s.name + '.' + t.name + '.' + c.name AS Value FROM sys.columns c JOIN sys.tables t ON t.object_id = c.object_id " +
                "JOIN sys.schemas s ON s.schema_id = t.schema_id " +
                "WHERE ((t.name LIKE 'Provision%' AND s.name = 'Ancillary') OR (t.name LIKE 'AncillaryProvision_%' AND s.name = 'ReadModel') OR t.name LIKE 'AncillaryPricing%') " +
                "AND t.name NOT LIKE '%PriceLines' AND t.name <> 'AncillaryProvisions' " +
                "AND (c.max_length = -1 OR c.name IN ('ConditionType', 'Operator', 'Value', 'Values', 'Expression', 'Json', 'Rule', 'Rules'))")
            .ToListAsync();

        Assert.Empty(suspicious);
    }

    [Fact]
    public async Task V12_N06_an_active_provision_is_immutable_and_a_new_draft_supersedes_it_at_the_same_sequence()
    {
        var definitionId = await ActiveDefinitionAsync();
        var original = Free(definitionId, travel: new ProvisionTravelCriteriaInput(TravelDates: [Day(1), Day(2)]));
        var published = (await _proof.RuleAsync(original)).Provision;

        await using (var editor = new AncillaryScope(_database, _clock))
        {
            await BusinessAssert.ThrowsAsync(16303, 409, () => editor.ChangeProvision.ChangeAsync(
                Change(published.Id, original with { Travel = new ProvisionTravelCriteriaInput(TravelDates: [Day(3)]) })));
            await BusinessAssert.ThrowsAsync(16303, 409, () => editor.ChangeTravelDate.ChangeAsync(
                new TestTravelDateRowCommand(published.Id, published.Travel.TravelDates[0].Id, Day(9))));
            await BusinessAssert.ThrowsAsync(16303, 409, () => editor.AddSeasonalPeriod.AddAsync(
                new TestSeasonalPeriodRowCommand(published.Id, 0, Day(1), Day(5))));
            await BusinessAssert.ThrowsAsync(16303, 409, () => editor.AddDayTimeRestriction.AddAsync(
                new TestDayTimeRestrictionRowCommand(published.Id, 0, DayOfWeek.Monday, null, null, DayTimeRestrictionEffect.Deny)));
        }

        _clock.Now = _clock.Now.AddHours(1);

        var replacement = (await _proof.RuleAsync(
            Free(definitionId, travel: new ProvisionTravelCriteriaInput(TravelDates: [Day(3), Day(4), Day(5)])))).Provision;

        await using var reader = new AncillaryScope(_database, _clock);
        var retired = await reader.GetProvisionById.ExecuteAsync(published.Id);

        Assert.NotEqual(published.Id, replacement.Id);
        Assert.Equal(("Retired", _clock.Now), (retired.Status.Name, retired.RetiredAt!.Value));
        Assert.Equal(published.Travel.TravelDates.Select(row => (row.Id, row.Value)), retired.Travel.TravelDates.Select(row => (row.Id, row.Value)));
        Assert.Equal(("Active", 3), (replacement.Status.Name, replacement.Travel.TravelDates.Count));
        Assert.Equal(
            new[] { (replacement.Id.ToString(), "Active"), (published.Id.ToString(), "Retired") },
            (await _proof.ListedProvisionsAsync(definitionId)).Select(row => (row.Id, row.Status.Name)));
    }

    [Fact]
    public async Task V12_C06_two_drafts_racing_for_the_same_active_sequence_end_with_one_active_and_a_conflict()
    {
        var definitionId = await ActiveDefinitionAsync();
        var first = (await _proof.RuleAsync(Free(definitionId), publish: false)).Provision;
        var second = (await _proof.RuleAsync(Free(definitionId), publish: false)).Provision;

        await using var winner = new AncillaryScope(_database, _clock);
        await using var loser = new AncillaryScope(_database, _clock);

        var winning = (await winner.Provisions.GetAsync(first.Id))!;
        var losing = (await loser.Provisions.GetAsync(second.Id))!;

        winning.Activate(_clock.Now);
        losing.Activate(_clock.Now);

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
    public async Task V12_N06_two_concurrent_edits_of_one_draft_do_not_overwrite_each_other_silently()
    {
        var definitionId = await ActiveDefinitionAsync();
        var draftCommand = Free(definitionId, travel: new ProvisionTravelCriteriaInput(TravelDates: [Day(1)]));
        var draft = (await _proof.RuleAsync(draftCommand, publish: false)).Provision;

        await using var firstEditor = new AncillaryScope(_database, _clock);
        await using var secondEditor = new AncillaryScope(_database, _clock);

        var firstCopy = (await firstEditor.Provisions.GetAsync(draft.Id))!;
        var secondCopy = (await secondEditor.Provisions.GetAsync(draft.Id))!;

        await firstEditor.ChangeProvision.ChangeAsync(Change(draft.Id, draftCommand with { Sequence = 20 }));

        Assert.Equal(20, firstCopy.Sequence);
        Assert.Equal(10, secondCopy.Sequence);
        await BusinessAssert.ThrowsAsync(16005, 409, () => secondEditor.ChangeProvision.ChangeAsync(Change(draft.Id, draftCommand with { Sequence = 30 })));

        await using var thirdEditor = new AncillaryScope(_database, _clock);
        await using var fourthEditor = new AncillaryScope(_database, _clock);

        await thirdEditor.Provisions.GetAsync(draft.Id);
        await fourthEditor.Provisions.GetAsync(draft.Id);
        await thirdEditor.AddTravelDate.AddAsync(new TestTravelDateRowCommand(draft.Id, 0, Day(2)));
        await BusinessAssert.ThrowsAsync(16309, 409, () => fourthEditor.AddTravelDate.AddAsync(new TestTravelDateRowCommand(draft.Id, 0, Day(2))));

        await using var reader = new AncillaryScope(_database, _clock);
        var stored = await reader.GetProvisionById.ExecuteAsync(draft.Id);

        Assert.Equal((20, 2), (stored.Sequence, stored.Travel.TravelDates.Count));
    }
}
