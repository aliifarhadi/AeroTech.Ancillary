using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Ancillary.Domain.ConformanceTests.Fakes;
using AeroTech.Ancillary.Domain.ConformanceTests.Fixtures;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;
using AeroTech.Messages.Core.Enums;
using Xunit;
using static AeroTech.Ancillary.Domain.ConformanceTests.Fixtures.P1Fixtures;

namespace AeroTech.Ancillary.Domain.ConformanceTests;

public class V12ProvisionConformanceTests
{
    private static readonly string[] Dimensions =
    [
        "PassengerType", "PointOfSale", "Customer", "CustomerType", "OriginAirport", "DestinationAirport", "ViaAirport", "RoutePair",
        "MarketingAirline", "OperatingAirline", "FlightNumber", "Flight", "Aircraft", "AirFare", "AirFareType", "FareFamily",
        "FareBasis", "CabinClass", "Rbd", "TravelDate", "SeasonalPeriod", "BlackoutPeriod", "DayTimeRestriction", "SeatNumber",
        "SeatCharacteristic"
    ];

    private static readonly ProvisionConditionsArgs None = ProvisionConditionsArgs.Unrestricted;

    private static ProvisionDatePeriodArgs Period(int fromMonth, int fromDay, int toMonth, int toDay)
        => new(new DateOnly(2027, fromMonth, fromDay), new DateOnly(2027, toMonth, toDay));

    private static ProvisionDayTimeRestrictionArgs DayTime(DayOfWeek day, int? fromHour, int? toHour, DayTimeRestrictionEffect effect)
        => new(day, fromHour is null ? null : new TimeOnly(fromHour.Value, 0), toHour is null ? null : new TimeOnly(toHour.Value, 0), effect);

    [Fact]
    public void V12_N05_a_provision_without_conditions_is_unconstrained_in_every_dimension()
    {
        var provision = Provision();

        Assert.Empty(provision.PassengerTypes);
        Assert.Empty(provision.PointsOfSale);
        Assert.Empty(provision.Customers);
        Assert.Empty(provision.CustomerTypes);
        Assert.Empty(provision.OriginAirports);
        Assert.Empty(provision.DestinationAirports);
        Assert.Empty(provision.ViaAirports);
        Assert.Empty(provision.RoutePairs);
        Assert.Empty(provision.MarketingAirlines);
        Assert.Empty(provision.OperatingAirlines);
        Assert.Empty(provision.FlightNumbers);
        Assert.Empty(provision.Flights);
        Assert.Empty(provision.Aircraft);
        Assert.Empty(provision.AirFares);
        Assert.Empty(provision.AirFareTypes);
        Assert.Empty(provision.FareFamilies);
        Assert.Empty(provision.FareBases);
        Assert.Empty(provision.CabinClasses);
        Assert.Empty(provision.Rbds);
        Assert.Empty(provision.TravelDates);
        Assert.Empty(provision.SeasonalPeriods);
        Assert.Empty(provision.BlackoutPeriods);
        Assert.Empty(provision.DayTimeRestrictions);
        Assert.Empty(provision.SeatNumbers);
        Assert.Empty(provision.SeatCharacteristics);
    }

    [Fact]
    public void V12_N05_every_dimension_has_its_own_typed_methods_and_no_generic_condition_api_exists()
    {
        var methods = typeof(AncillaryProvision).GetMethods().Select(method => method.Name).ToHashSet();

        foreach (var dimension in Dimensions)
        {
            Assert.Contains($"Add{dimension}", methods);
            Assert.Contains($"Change{dimension}", methods);
            Assert.Contains($"Remove{dimension}", methods);
        }

        Assert.DoesNotContain(methods, name => name is "AddCondition" or "ChangeCondition" or "RemoveCondition" or "Evaluate" or "Matches");
        Assert.DoesNotContain(
            typeof(AncillaryProvision).Assembly.GetTypes(),
            type => type.Name.Contains("ConditionType", StringComparison.Ordinal)
                    || type.Name.Contains("Operator", StringComparison.Ordinal)
                    || type.Name.Contains("Expression", StringComparison.Ordinal));
    }

    [Fact]
    public void V12_N01_one_thousand_travel_dates_are_one_row_each_with_a_stable_identity()
    {
        var ids = new SequentialIdGenerator();
        var first = new DateOnly(2027, 1, 1);
        var dates = Enumerable.Range(0, 1000).Select(first.AddDays).ToList();
        var provision = Provision(None with { TravelDates = dates }, ids: ids);

        Assert.Equal(1000, provision.TravelDates.Count);
        Assert.Equal(1000, provision.TravelDates.Select(row => row.Id).Distinct().Count());
        Assert.Equal(dates, provision.TravelDates.Select(row => row.TravelDate));
        Assert.All(provision.TravelDates, row => Assert.Equal(provision.Id, row.AncillaryProvisionId));

        var before = provision.TravelDates.ToDictionary(row => row.TravelDate, row => row.Id);
        var changed = provision.TravelDates.Single(row => row.TravelDate == first.AddDays(10));
        var removed = provision.TravelDates.Single(row => row.TravelDate == first.AddDays(20));

        provision.ChangeTravelDate(changed.Id, new DateOnly(2030, 5, 5));
        provision.RemoveTravelDate(removed.Id);

        Assert.Equal(999, provision.TravelDates.Count);
        Assert.Equal(changed.Id, provision.TravelDates.Single(row => row.TravelDate == new DateOnly(2030, 5, 5)).Id);
        Assert.DoesNotContain(provision.TravelDates, row => row.Id == removed.Id);
        Assert.All(
            provision.TravelDates.Where(row => row.Id != changed.Id),
            row => Assert.Equal(before[row.TravelDate], row.Id));

        var added = provision.AddTravelDate(new DateOnly(2031, 1, 1), ids);

        Assert.DoesNotContain(before.Values, id => id == added.Id);
        Assert.Equal(1000, provision.TravelDates.Count);
    }

    [Fact]
    public void V12_N01_a_duplicate_travel_date_is_refused_in_a_list_and_as_a_row()
    {
        var ids = new SequentialIdGenerator();
        var date = new DateOnly(2027, 3, 21);

        BusinessAssert.Throws(16302, 422, () => Provision(None with { TravelDates = [date, date] }));

        var provision = Provision(None with { TravelDates = [date, date.AddDays(1)] }, ids: ids);

        BusinessAssert.Throws(16309, 409, () => provision.AddTravelDate(date, ids));
        BusinessAssert.Throws(16309, 409, () => provision.ChangeTravelDate(provision.TravelDates.Last().Id, date));
        BusinessAssert.Throws(16308, 404, () => provision.RemoveTravelDate(999_999));
        Assert.Equal(2, provision.TravelDates.Count);
    }

    [Fact]
    public void V12_N02_seasons_and_blackouts_are_inclusive_ranges_filed_as_separate_rows()
    {
        var ids = new SequentialIdGenerator();
        var blackouts = Enumerable.Range(1, 10).Select(day => Period(8, day * 2, 8, day * 2)).ToList();
        var provision = Provision(
            None with
            {
                SeasonalPeriods = [Period(3, 15, 4, 5), Period(6, 1, 8, 31), Period(12, 15, 12, 31)],
                BlackoutPeriods = blackouts
            },
            ids: ids);

        Assert.Equal(3, provision.SeasonalPeriods.Count);
        Assert.Equal(10, provision.BlackoutPeriods.Count);
        Assert.All(provision.BlackoutPeriods, row => Assert.Equal(row.StartDate, row.EndDate));
        Assert.Equal(13, provision.SeasonalPeriods.Select(row => row.Id).Concat(provision.BlackoutPeriods.Select(row => row.Id)).Distinct().Count());

        BusinessAssert.Throws(16302, 422, () => Provision(None with { SeasonalPeriods = [Period(4, 5, 3, 15)] }));
        BusinessAssert.Throws(16302, 422, () => Provision(None with { BlackoutPeriods = [Period(4, 5, 3, 15)] }));
        BusinessAssert.Throws(16302, 422, () => Provision(None with { SeasonalPeriods = [Period(3, 15, 4, 5), Period(3, 15, 4, 5)] }));
        BusinessAssert.Throws(16309, 409, () => provision.AddSeasonalPeriod(Period(6, 1, 8, 31), ids));
        BusinessAssert.Throws(16309, 409, () => provision.AddBlackoutPeriod(Period(8, 2, 8, 2), ids));

        var overlapping = provision.AddSeasonalPeriod(Period(3, 20, 4, 20), ids);

        Assert.Equal(4, provision.SeasonalPeriods.Count);

        provision.ChangeSeasonalPeriod(overlapping.Id, Period(9, 1, 9, 30));
        provision.RemoveBlackoutPeriod(provision.BlackoutPeriods.First().Id);

        Assert.Equal(new DateOnly(2027, 9, 1), provision.SeasonalPeriods.Single(row => row.Id == overlapping.Id).StartDate);
        Assert.Equal(9, provision.BlackoutPeriods.Count);

        provision.Activate(Now);

        Assert.Equal(4, provision.SeasonalPeriods.Count);
        Assert.Equal(9, provision.BlackoutPeriods.Count);
    }

    [Fact]
    public void V12_N03_day_time_rows_allow_or_deny_a_weekday_window_in_local_time()
    {
        var ids = new SequentialIdGenerator();
        var provision = Provision(
            None with
            {
                DayTimeRestrictions =
                [
                    DayTime(DayOfWeek.Monday, 8, 12, DayTimeRestrictionEffect.Allow),
                    DayTime(DayOfWeek.Monday, 9, 10, DayTimeRestrictionEffect.Deny),
                    DayTime(DayOfWeek.Friday, null, null, DayTimeRestrictionEffect.Deny),
                    DayTime(DayOfWeek.Tuesday, null, null, DayTimeRestrictionEffect.Allow),
                    DayTime(DayOfWeek.Wednesday, 6, 6, DayTimeRestrictionEffect.Allow)
                ]
            },
            ids: ids);

        Assert.Equal(5, provision.DayTimeRestrictions.Count);
        Assert.Equal(
            new[] { (DayOfWeek.Monday, DayTimeRestrictionEffect.Allow), (DayOfWeek.Monday, DayTimeRestrictionEffect.Deny) },
            provision.DayTimeRestrictions.Where(row => row.DayOfWeek == DayOfWeek.Monday).Select(row => (row.DayOfWeek, row.Effect)));

        var wholeDay = provision.DayTimeRestrictions.Single(row => row.DayOfWeek == DayOfWeek.Friday);

        Assert.Equal(((TimeOnly?)null, (TimeOnly?)null, DayTimeRestrictionEffect.Deny), (wholeDay.StartTime, wholeDay.EndTime, wholeDay.Effect));

        BusinessAssert.Throws(16302, 422, () => Provision(None with
        {
            DayTimeRestrictions = [new ProvisionDayTimeRestrictionArgs(DayOfWeek.Monday, new TimeOnly(8, 0), null, DayTimeRestrictionEffect.Allow)]
        }));
        BusinessAssert.Throws(16302, 422, () => Provision(None with
        {
            DayTimeRestrictions = [new ProvisionDayTimeRestrictionArgs(DayOfWeek.Monday, null, new TimeOnly(8, 0), DayTimeRestrictionEffect.Allow)]
        }));
        BusinessAssert.Throws(16302, 422, () => Provision(None with { DayTimeRestrictions = [DayTime(DayOfWeek.Monday, 22, 2, DayTimeRestrictionEffect.Allow)] }));
        BusinessAssert.Throws(16302, 422, () => Provision(None with { DayTimeRestrictions = [DayTime((DayOfWeek)9, 8, 12, DayTimeRestrictionEffect.Allow)] }));
        BusinessAssert.Throws(16302, 422, () => Provision(None with { DayTimeRestrictions = [DayTime(DayOfWeek.Monday, 8, 12, (DayTimeRestrictionEffect)7)] }));
        BusinessAssert.Throws(16309, 409, () => provision.AddDayTimeRestriction(DayTime(DayOfWeek.Monday, 8, 12, DayTimeRestrictionEffect.Allow), ids));

        var night = provision.AddDayTimeRestriction(DayTime(DayOfWeek.Saturday, 22, 23, DayTimeRestrictionEffect.Allow), ids);

        provision.ChangeDayTimeRestriction(night.Id, DayTime(DayOfWeek.Sunday, 0, 2, DayTimeRestrictionEffect.Allow));

        Assert.Equal(DayOfWeek.Sunday, provision.DayTimeRestrictions.Single(row => row.Id == night.Id).DayOfWeek);
        Assert.DoesNotContain(
            typeof(AncillaryProvision).Assembly.GetTypes(),
            type => type.Name.Contains("Checker", StringComparison.Ordinal) || type.Name.Contains("TimeZone", StringComparison.Ordinal) && !type.IsInterface);
    }

    [Fact]
    public void V12_N04_every_selector_is_a_typed_child_row_with_its_own_identity()
    {
        var provision = Provision(
            new ProvisionConditionsArgs(
                [PassengerTypeCode.ADT, PassengerTypeCode.CHD],
                [501, 502],
                [9001],
                [CustomerType.TravelAgency],
                [Thr],
                [Mhd, Ist],
                [7],
                [Pair(Thr, Mhd), Pair(Thr, Ist, RoutePairDirection.BothDirections)],
                [1],
                [1, 2],
                [" da2021 ", "2022"],
                [81234],
                [320, 321],
                [4001],
                [AirFareType.Private],
                [5, 6],
                [" ylow ", "YFLEX"],
                [1, 2],
                [15, 21],
                [new DateOnly(2027, 3, 21)],
                [Period(3, 15, 4, 5)],
                [Period(4, 1, 4, 2)],
                [DayTime(DayOfWeek.Thursday, 5, 11, DayTimeRestrictionEffect.Allow)],
                ["12a"],
                ["w", "LS"]),
            ProvisionApplicationType.Seat);

        Assert.Equal(new[] { PassengerTypeCode.ADT, PassengerTypeCode.CHD }, provision.PassengerTypes.Select(row => row.PassengerTypeCode));
        Assert.Equal(new long[] { 501, 502 }, provision.PointsOfSale.Select(row => row.PointOfSaleId));
        Assert.Equal(new long[] { 9001 }, provision.Customers.Select(row => row.CustomerId));
        Assert.Equal(new[] { CustomerType.TravelAgency }, provision.CustomerTypes.Select(row => row.CustomerType));
        Assert.Equal(new[] { Thr }, provision.OriginAirports.Select(row => row.AirportId));
        Assert.Equal(new[] { Mhd, Ist }, provision.DestinationAirports.Select(row => row.AirportId));
        Assert.Equal(new[] { 7 }, provision.ViaAirports.Select(row => row.AirportId));
        Assert.Equal(
            new[] { (Thr, Mhd, RoutePairDirection.Directional), (Thr, Ist, RoutePairDirection.BothDirections) },
            provision.RoutePairs.Select(row => (row.OriginAirportId, row.DestinationAirportId, row.Direction)));
        Assert.Equal(new[] { 1 }, provision.MarketingAirlines.Select(row => row.AirlineId));
        Assert.Equal(new[] { 1, 2 }, provision.OperatingAirlines.Select(row => row.AirlineId));
        Assert.Equal(new[] { "DA2021", "2022" }, provision.FlightNumbers.Select(row => row.FlightNumber));
        Assert.Equal(new long[] { 81234 }, provision.Flights.Select(row => row.FlightId));
        Assert.Equal(new[] { 320, 321 }, provision.Aircraft.Select(row => row.AircraftId));
        Assert.Equal(new long[] { 4001 }, provision.AirFares.Select(row => row.AirFareId));
        Assert.Equal(new[] { AirFareType.Private }, provision.AirFareTypes.Select(row => row.AirFareType));
        Assert.Equal(new long[] { 5, 6 }, provision.FareFamilies.Select(row => row.FareFamilyId));
        Assert.Equal(new[] { "YLOW", "YFLEX" }, provision.FareBases.Select(row => row.FareBasisCode));
        Assert.Equal(new[] { 1, 2 }, provision.CabinClasses.Select(row => row.CabinClassId));
        Assert.Equal(new long[] { 15, 21 }, provision.Rbds.Select(row => row.RbdId));
        Assert.Equal(new[] { new DateOnly(2027, 3, 21) }, provision.TravelDates.Select(row => row.TravelDate));
        Assert.Equal(new[] { "12A" }, provision.SeatNumbers.Select(row => row.SeatNumber));
        Assert.Equal(new[] { "W", "LS" }, provision.SeatCharacteristics.Select(row => row.CharacteristicCode));

        var rowIds = provision.PassengerTypes.Select(row => row.Id)
            .Concat(provision.PointsOfSale.Select(row => row.Id))
            .Concat(provision.RoutePairs.Select(row => row.Id))
            .Concat(provision.FlightNumbers.Select(row => row.Id))
            .Concat(provision.SeatCharacteristics.Select(row => row.Id))
            .ToList();

        Assert.Equal(rowIds.Count, rowIds.Distinct().Count());
        Assert.All(rowIds, id => Assert.True(id > 0));
    }

    [Fact]
    public void V12_N04_each_selector_value_is_validated_and_unique_within_its_provision()
    {
        BusinessAssert.Throws(16302, 422, () => Provision(None with { PassengerTypeCodes = [(PassengerTypeCode)9999] }));
        BusinessAssert.Throws(16302, 422, () => Provision(None with { PassengerTypeCodes = [PassengerTypeCode.ADT, PassengerTypeCode.ADT] }));
        BusinessAssert.Throws(16302, 422, () => Provision(None with { PointOfSaleIds = [0] }));
        BusinessAssert.Throws(16302, 422, () => Provision(None with { CustomerIds = [7, 7] }));
        BusinessAssert.Throws(16302, 422, () => Provision(None with { CustomerTypes = [(CustomerType)99] }));
        BusinessAssert.Throws(16302, 422, () => Provision(None with { OriginAirportIds = [-1] }));
        BusinessAssert.Throws(16302, 422, () => Provision(None with { DestinationAirportIds = [3, 3] }));
        BusinessAssert.Throws(16302, 422, () => Provision(None with { ViaAirportIds = [0] }));
        BusinessAssert.Throws(16302, 422, () => Provision(None with { MarketingAirlineIds = [1, 1] }));
        BusinessAssert.Throws(16302, 422, () => Provision(None with { OperatingAirlineIds = [0] }));
        BusinessAssert.Throws(16302, 422, () => Provision(None with { FlightNumbers = ["DA 21"] }));
        BusinessAssert.Throws(16302, 422, () => Provision(None with { FlightNumbers = ["da21", "DA21"] }));
        BusinessAssert.Throws(16302, 422, () => Provision(None with { FlightNumbers = [new string('1', 17)] }));
        BusinessAssert.Throws(16302, 422, () => Provision(None with { FlightIds = [0] }));
        BusinessAssert.Throws(16302, 422, () => Provision(None with { AircraftIds = [320, 320] }));
        BusinessAssert.Throws(16302, 422, () => Provision(None with { AirFareIds = [0] }));
        BusinessAssert.Throws(16302, 422, () => Provision(None with { FareFamilyIds = [5, 5] }));
        BusinessAssert.Throws(16302, 422, () => Provision(None with { FareBasisCodes = [""] }));
        BusinessAssert.Throws(16302, 422, () => Provision(None with { CabinClassIds = [0] }));
        BusinessAssert.Throws(16302, 422, () => Provision(None with { RbdIds = [15, 15] }));
    }

    [Fact]
    public void V12_N04_route_pairs_reject_the_same_airport_and_inverse_duplicates()
    {
        var ids = new SequentialIdGenerator();

        BusinessAssert.Throws(16302, 422, () => Provision(None with { RoutePairs = [Pair(Thr, Thr)] }));
        BusinessAssert.Throws(16302, 422, () => Provision(None with { RoutePairs = [Pair(Thr, Mhd), Pair(Thr, Mhd, RoutePairDirection.BothDirections)] }));
        BusinessAssert.Throws(16302, 422, () => Provision(None with
        {
            RoutePairs = [Pair(Thr, Mhd, RoutePairDirection.BothDirections), Pair(Mhd, Thr, RoutePairDirection.BothDirections)]
        }));
        BusinessAssert.Throws(16302, 422, () => Provision(None with { RoutePairs = [Pair(Thr, Mhd, RoutePairDirection.BothDirections), Pair(Mhd, Thr)] }));

        var provision = Provision(None with { RoutePairs = [Pair(Thr, Mhd), Pair(Mhd, Thr)] }, ids: ids);

        Assert.Equal(2, provision.RoutePairs.Count);
        BusinessAssert.Throws(16309, 409, () => provision.AddRoutePair(Pair(Mhd, Thr, RoutePairDirection.BothDirections), ids));

        var added = provision.AddRoutePair(Pair(Thr, Ist, RoutePairDirection.BothDirections), ids);

        provision.ChangeRoutePair(added.Id, Pair(Ist, Mhd));
        provision.RemoveRoutePair(provision.RoutePairs.First().Id);

        Assert.Equal(
            new[] { (Mhd, Thr), (Ist, Mhd) },
            provision.RoutePairs.Select(row => (row.OriginAirportId, row.DestinationAirportId)));
    }

    [Fact]
    public void V12_N04_replacing_the_conditions_keeps_the_identity_of_unchanged_rows()
    {
        var ids = new SequentialIdGenerator();
        var provision = Provision(
            None with
            {
                PassengerTypeCodes = [PassengerTypeCode.ADT, PassengerTypeCode.CHD],
                FareFamilyIds = [5, 6],
                TravelDates = [new DateOnly(2027, 3, 21), new DateOnly(2027, 3, 22)],
                RoutePairs = [Pair(Thr, Mhd)]
            },
            ids: ids);
        var adult = provision.PassengerTypes.Single(row => row.PassengerTypeCode == PassengerTypeCode.ADT).Id;
        var family = provision.FareFamilies.Single(row => row.FareFamilyId == 6).Id;
        var date = provision.TravelDates.Single(row => row.TravelDate == new DateOnly(2027, 3, 22)).Id;
        var pair = provision.RoutePairs.Single().Id;

        provision.ReplaceConditions(
            None with
            {
                PassengerTypeCodes = [PassengerTypeCode.ADT, PassengerTypeCode.INF],
                FareFamilyIds = [6, 7],
                TravelDates = [new DateOnly(2027, 3, 22), new DateOnly(2027, 3, 23)],
                RoutePairs = [Pair(Thr, Mhd), Pair(Thr, Ist)]
            },
            ids);

        Assert.Equal(adult, provision.PassengerTypes.Single(row => row.PassengerTypeCode == PassengerTypeCode.ADT).Id);
        Assert.Equal(new[] { PassengerTypeCode.ADT, PassengerTypeCode.INF }, provision.PassengerTypes.Select(row => row.PassengerTypeCode));
        Assert.Equal(family, provision.FareFamilies.Single(row => row.FareFamilyId == 6).Id);
        Assert.Equal(date, provision.TravelDates.Single(row => row.TravelDate == new DateOnly(2027, 3, 22)).Id);
        Assert.Equal(pair, provision.RoutePairs.Single(row => row.DestinationAirportId == Mhd).Id);
        Assert.Equal(2, provision.RoutePairs.Count);
    }

    [Fact]
    public void V12_N06_a_refused_replacement_leaves_the_draft_untouched()
    {
        var ids = new SequentialIdGenerator();
        var provision = Provision(None with { PassengerTypeCodes = [PassengerTypeCode.ADT], TravelDates = [new DateOnly(2027, 3, 21)] }, ids: ids);
        var rowId = provision.PassengerTypes.Single().Id;

        BusinessAssert.Throws(16302, 422, () => provision.ReplaceConditions(
            None with { PassengerTypeCodes = [PassengerTypeCode.CHD], SeasonalPeriods = [Period(4, 5, 3, 15)] },
            ids));
        BusinessAssert.Throws(16302, 422, () => Replace(provision, None with { TravelDates = [DateOnly.MinValue] }, ids));

        Assert.Equal(rowId, provision.PassengerTypes.Single().Id);
        Assert.Equal(PassengerTypeCode.ADT, provision.PassengerTypes.Single().PassengerTypeCode);
        Assert.Single(provision.TravelDates);
        Assert.Empty(provision.SeasonalPeriods);
    }

    [Fact]
    public void V12_N06_the_children_of_a_provision_that_left_draft_cannot_be_mutated()
    {
        var ids = new SequentialIdGenerator();
        var provision = Provision(
            None with
            {
                TravelDates = [new DateOnly(2027, 3, 21)],
                SeasonalPeriods = [Period(3, 15, 4, 5)],
                BlackoutPeriods = [Period(4, 1, 4, 1)],
                DayTimeRestrictions = [DayTime(DayOfWeek.Monday, 8, 12, DayTimeRestrictionEffect.Allow)],
                PassengerTypeCodes = [PassengerTypeCode.ADT]
            },
            ids: ids);

        provision.Activate(Now);

        BusinessAssert.Throws(16303, 409, () => provision.AddTravelDate(new DateOnly(2027, 3, 22), ids));
        BusinessAssert.Throws(16303, 409, () => provision.ChangeTravelDate(provision.TravelDates.Single().Id, new DateOnly(2027, 3, 22)));
        BusinessAssert.Throws(16303, 409, () => provision.RemoveTravelDate(provision.TravelDates.Single().Id));
        BusinessAssert.Throws(16303, 409, () => provision.AddSeasonalPeriod(Period(6, 1, 6, 30), ids));
        BusinessAssert.Throws(16303, 409, () => provision.RemoveBlackoutPeriod(provision.BlackoutPeriods.Single().Id));
        BusinessAssert.Throws(16303, 409, () => provision.RemoveDayTimeRestriction(provision.DayTimeRestrictions.Single().Id));
        BusinessAssert.Throws(16303, 409, () => provision.AddPassengerType(PassengerTypeCode.CHD, ids));
        BusinessAssert.Throws(16303, 409, () => provision.ReplaceConditions(None, ids));
        BusinessAssert.Throws(16303, 409, () => Replace(provision, None, ids));

        provision.Suspend(Now.AddHours(1));

        BusinessAssert.Throws(16303, 409, () => provision.RemoveTravelDate(provision.TravelDates.Single().Id));

        provision.Reactivate();
        provision.Retire(Now.AddHours(2));

        BusinessAssert.Throws(16303, 409, () => provision.ReplaceConditions(None, ids));
        Assert.Single(provision.TravelDates);
        Assert.Single(provision.PassengerTypes);
        Assert.Equal(ProvisionStatus.Retired, provision.Status);
    }

    [Fact]
    public void V12_J03_J04_seat_selectors_are_rows_and_exact_seat_numbers_need_an_aircraft()
    {
        var ids = new SequentialIdGenerator();

        BusinessAssert.Throws(16302, 422, () => Provision(None, ProvisionApplicationType.Seat));
        BusinessAssert.Throws(16302, 422, () => Provision(None with { SeatCharacteristicCodes = ["W"] }));
        BusinessAssert.Throws(16302, 422, () => Provision(None with { SeatNumbers = ["12A"] }, ProvisionApplicationType.Seat));
        BusinessAssert.Throws(16302, 422, () => Provision(None with { SeatNumbers = ["12A", "12a"], AircraftIds = [320] }, ProvisionApplicationType.Seat));

        var provision = Provision(None with { SeatCharacteristicCodes = ["W"] }, ProvisionApplicationType.Seat, ids: ids);

        BusinessAssert.Throws(16302, 422, () => provision.AddSeatNumber("12A", ids));
        BusinessAssert.Throws(16302, 422, () => provision.RemoveSeatCharacteristic(provision.SeatCharacteristics.Single().Id));

        var aircraft = provision.AddAircraft(320, ids);
        var seat = provision.AddSeatNumber(" 12a ", ids);

        Assert.Equal("12A", seat.SeatNumber);
        BusinessAssert.Throws(16302, 422, () => provision.RemoveAircraft(aircraft.Id));

        provision.RemoveSeatCharacteristic(provision.SeatCharacteristics.Single().Id);

        Assert.Empty(provision.SeatCharacteristics);
        Assert.Equal(new[] { "12A" }, provision.SeatNumbers.Select(row => row.SeatNumber));
        Assert.DoesNotContain(
            PropertiesOf<AncillaryProvision>(),
            name => name.Contains("Occupied", StringComparison.Ordinal) || name.Contains("Blocked", StringComparison.Ordinal));
    }

    [Fact]
    public void V12_REQ_the_provision_owns_eligibility_and_outcome_but_no_money()
    {
        Assert.Equal(
            new[]
            {
                "ActivatedAt", "AdvancePurchase", "AirFareTypes", "AirFares", "Aircraft", "Application", "Availability", "BlackoutPeriods",
                "CabinClasses", "CoverageScope", "CreatedAt", "CustomerTypes", "Customers", "DayTimeRestrictions", "DestinationAirports",
                "FareBases", "FareFamilies", "FlightNumbers", "Flights", "Fulfillment", "MarketingAirlines", "OperatingAirlines",
                "OriginAirports", "Outcome", "PassengerTypes", "PointsOfSale", "Quantity", "Rbds", "RetiredAt", "RoutePairs",
                "SalesDiscontinueAt", "SalesEffectiveFrom", "SeasonalPeriods", "SeatCharacteristics", "SeatNumbers", "Sequence",
                "ServiceDefinitionId", "Settlement", "Status", "SuspendedAt", "TravelDates", "ViaAirports"
            },
            PropertiesOf<AncillaryProvision>());
        Assert.DoesNotContain(
            PropertiesOf<AncillaryProvision>(),
            name => name.Contains("Fee", StringComparison.Ordinal)
                    || name.Contains("Price", StringComparison.Ordinal)
                    || name.Contains("Currency", StringComparison.Ordinal)
                    || name.Contains("Amount", StringComparison.Ordinal));
    }
}
