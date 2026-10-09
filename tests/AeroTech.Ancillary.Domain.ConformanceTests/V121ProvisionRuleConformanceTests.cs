using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities;
using AeroTech.Ancillary.Domain.ConformanceTests.Fakes;
using AeroTech.Ancillary.Domain.ConformanceTests.Fixtures;
using AeroTech.Framework.Core.Domain.Entities;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;
using AeroTech.Messages.Core.Enums;
using Xunit;
using static AeroTech.Ancillary.Domain.ConformanceTests.Fixtures.P1Fixtures;
using static AeroTech.Ancillary.Domain.ConformanceTests.Fixtures.V121Fixtures;

namespace AeroTech.Ancillary.Domain.ConformanceTests;

public class V121ProvisionRuleConformanceTests
{
    private static readonly string[] RuleGroups =
    [
        "AdvancePurchase", "BaggageApplication", "DayTimeApplication", "FareApplication", "FlightApplication", "Geography", "PassengerEligibility",
        "SalesRestrictions", "SeatApplication", "TravelDate"
    ];

    private static (DateOnly Start, DateOnly End)[] Permitted(AncillaryProvision provision)
        => provision.TravelDate!.PermittedPeriods.OrderBy(row => row.StartDate).Select(row => (row.StartDate, row.EndDate)).ToArray();

    private static (DateOnly Start, DateOnly End)[] Blackouts(AncillaryProvision provision)
        => provision.TravelDate!.BlackoutPeriods.OrderBy(row => row.StartDate).Select(row => (row.StartDate, row.EndDate)).ToArray();

    [Fact]
    public void V121_B_a_provision_owns_exactly_ten_optional_rule_groups_and_no_peer_collection()
    {
        var properties = typeof(AncillaryProvision).GetProperties().Where(property => property.DeclaringType == typeof(AncillaryProvision)).ToList();
        var groups = properties.Where(property => property.PropertyType.Namespace == typeof(ProvisionTravelDateRule).Namespace).ToList();

        Assert.Equal(RuleGroups, groups.Select(property => property.Name).OrderBy(name => name, StringComparer.Ordinal));
        Assert.All(groups, group => Assert.Equal($"Provision{group.Name}Rule", group.PropertyType.Name));
        Assert.DoesNotContain(properties, property => typeof(System.Collections.IEnumerable).IsAssignableFrom(property.PropertyType));
        Assert.DoesNotContain(properties, property => property.Name is "TravelDates" or "SeasonalPeriods" or "SalesEffectiveFrom" or "SalesDiscontinueAt" or "Fee" or "PriceLines");

        var unrestricted = Provision();

        Assert.All(groups, group => Assert.Null(group.GetValue(unrestricted)));
    }

    [Fact]
    public void V121_B_the_ten_rule_groups_own_exactly_twenty_seven_typed_row_entities()
    {
        var entities = typeof(ProvisionTravelDateRule).Assembly.GetTypes()
            .Where(type => type.Namespace == typeof(ProvisionTravelDateRule).Namespace && type.IsSubclassOf(typeof(Entity<long>)))
            .ToList();
        var rules = entities.Where(type => type.Name.EndsWith("Rule", StringComparison.Ordinal)).ToList();
        var rows = entities.Except(rules).ToList();

        Assert.Equal(10, rules.Count);
        Assert.Equal(27, rows.Count);
        Assert.All(rules, rule => Assert.Contains(rule.GetProperties(), property => property.Name == "AncillaryProvisionId"));
        Assert.All(rows, row =>
        {
            Assert.Single(row.GetProperties(), property => property.Name.StartsWith("Provision", StringComparison.Ordinal) && property.Name.EndsWith("RuleId", StringComparison.Ordinal));
            Assert.Contains(row.GetProperties(), property => property.Name == "AncillaryProvisionId");
        });
        Assert.DoesNotContain(entities, type => type.Name is "ProvisionTravelDate" or "ProvisionSeasonalPeriod" or "ProvisionDayTimeRestriction");
        Assert.DoesNotContain(
            entities.SelectMany(type => type.GetProperties()),
            property => property.Name is "ConditionType" or "Operator" or "Expression" or "Json" or "Negate");
    }

    [Fact]
    public void V121_B_every_row_carries_the_key_of_its_rule_group_and_an_empty_group_is_not_materialized()
    {
        var provision = Provision(
            Groups(
                Passengers([PassengerTypeCode.ADT, PassengerTypeCode.CHD], [new ProvisionAgeBandArgs(0, 65), new ProvisionAgeBandArgs(65, null)]),
                Sales(Now, Now.AddDays(30), [501], [9001], [CustomerType.TravelAgency]),
                Geography([Thr], [Ist], [Mhd], [Pair(Thr, Ist)], [Location(ServiceLocationType.Airport, Thr)], [98]),
                Flights([1], [2], [" w5112"], [81234], [1]),
                Fares([7001], [AirFareType.Public], [5], ["y26lt"], [2], [41]),
                Dates([Period(Day(2027, 4, 1), Day(2027, 4, 30))], [Period(Day(2027, 4, 10), Day(2027, 4, 12))]),
                Windows(Window(Weekdays, 9, 17)),
                new ProvisionAdvancePurchaseArgs(24, TimeUnit.Hours, false),
                seatApplication: Seats(["12a"], ["w"])),
            ProvisionApplicationType.Seat);

        Assert.All(provision.PassengerEligibility!.PassengerTypes, row => Assert.Equal((provision.Id, provision.PassengerEligibility.Id), (row.AncillaryProvisionId, row.ProvisionPassengerEligibilityRuleId)));
        Assert.All(provision.PassengerEligibility.AgeBands, row => Assert.Equal(provision.PassengerEligibility.Id, row.ProvisionPassengerEligibilityRuleId));
        Assert.Equal((Now, Now.AddDays(30)), (provision.SalesRestrictions!.SalesEffectiveFrom!.Value, provision.SalesRestrictions.SalesDiscontinueAt!.Value));
        Assert.Equal(provision.SalesRestrictions.Id, provision.SalesRestrictions.PointsOfSale.Single().ProvisionSalesRestrictionsRuleId);
        Assert.Equal((ServiceLocationType.Airport, Thr, 98), (provision.Geography!.ServiceLocations.Single().LocationType, provision.Geography.ServiceLocations.Single().LocationId, provision.Geography.CoverageCountries.Single().CountryId));
        Assert.Equal(provision.Geography.Id, provision.Geography.RoutePairs.Single().ProvisionGeographyRuleId);
        Assert.Equal(("W5112", provision.FlightApplication!.Id), (provision.FlightApplication.FlightNumbers.Single().FlightNumber, provision.FlightApplication.Aircraft.Single().ProvisionFlightApplicationRuleId));
        Assert.Equal(("Y26LT", provision.FareApplication!.Id), (provision.FareApplication.FareBases.Single().FareBasisCode, provision.FareApplication.Rbds.Single().ProvisionFareApplicationRuleId));
        Assert.Equal(provision.TravelDate!.Id, provision.TravelDate.BlackoutPeriods.Single().ProvisionTravelDateRuleId);
        Assert.Equal((Weekdays, new TimeOnly(9, 0), new TimeOnly(17, 0), DayTimeRestrictionEffect.Allow), provision.DayTimeApplication!.Windows.Select(row => (row.DaysOfWeekMask, row.StartLocalTime!.Value, row.EndLocalTime!.Value, row.Effect)).Single());
        Assert.Equal((24, TimeUnit.Hours, false), (provision.AdvancePurchase!.MinimumPeriod, provision.AdvancePurchase.Unit, provision.AdvancePurchase.SameTimeAsTicketed));
        Assert.Equal(("12A", "W"), (provision.SeatApplication!.SeatNumbers.Single().SeatNumber, provision.SeatApplication.SeatCharacteristics.Single().CharacteristicCode));
        Assert.Null(provision.BaggageApplication);

        var ids = new[]
        {
            provision.PassengerEligibility.Id, provision.SalesRestrictions.Id, provision.Geography.Id, provision.FlightApplication.Id, provision.FareApplication.Id,
            provision.TravelDate.Id, provision.DayTimeApplication.Id, provision.AdvancePurchase.Id, provision.SeatApplication.Id
        };

        Assert.Equal(9, ids.Distinct().Count());

        var emptied = Provision(Groups(Passengers(), Sales(), Geography(), Flights(), Fares(), Dates(), Windows(), seatApplication: Seats()));

        Assert.Null(emptied.PassengerEligibility);
        Assert.Null(emptied.SalesRestrictions);
        Assert.Null(emptied.Geography);
        Assert.Null(emptied.FlightApplication);
        Assert.Null(emptied.FareApplication);
        Assert.Null(emptied.TravelDate);
        Assert.Null(emptied.DayTimeApplication);
        Assert.Null(emptied.SeatApplication);
    }

    [Fact]
    public void V121_D01_one_thousand_consecutive_days_are_one_permitted_period()
    {
        var days = Enumerable.Range(0, 1000).Select(offset => Day(2027, 1, 1).AddDays(offset)).Select(day => Period(day, day)).ToArray();
        var provision = Provision(Groups(travelDate: Dates(days)));

        Assert.Equal(Day(2029, 9, 26), Day(2027, 1, 1).AddDays(999));
        Assert.Equal(new[] { (Day(2027, 1, 1), Day(2029, 9, 26)) }, Permitted(provision));

        var range = Provision(Groups(travelDate: Dates([Period(Day(2027, 1, 1), Day(2029, 9, 26))])));

        Assert.Equal(Permitted(provision), Permitted(range));
    }

    [Fact]
    public void V121_D02_one_thousand_sparse_dates_are_one_thousand_single_day_periods()
    {
        var days = Enumerable.Range(0, 1000).Select(offset => Day(2027, 1, 1).AddDays(offset * 2)).ToArray();
        var provision = Provision(Groups(travelDate: Dates(days.Select(day => Period(day, day)).ToArray())));

        Assert.Equal(days.Select(day => (day, day)), Permitted(provision));
        Assert.Equal(1000, provision.TravelDate!.PermittedPeriods.Select(row => row.Id).Distinct().Count());
    }

    [Fact]
    public void V121_D03_D04_D05_adjacent_and_overlapping_permitted_periods_are_one_canonical_period_and_a_single_date_has_equal_bounds()
    {
        var adjacent = Provision(Groups(travelDate: Dates([Period(Day(2027, 2, 1), Day(2027, 2, 28)), Period(Day(2027, 1, 1), Day(2027, 1, 31))])));
        var overlapping = Provision(Groups(travelDate: Dates([Period(Day(2027, 1, 1), Day(2027, 1, 31)), Period(Day(2027, 1, 15), Day(2027, 2, 28))])));
        var contained = Provision(Groups(travelDate: Dates([Period(Day(2027, 1, 1), Day(2027, 2, 28)), Period(Day(2027, 1, 10), Day(2027, 1, 12))])));
        var disjoint = Provision(Groups(travelDate: Dates([Period(Day(2027, 3, 2), Day(2027, 3, 5)), Period(Day(2027, 1, 1), Day(2027, 2, 28))])));
        var single = Provision(Groups(travelDate: Dates([Period(Day(2027, 5, 9), Day(2027, 5, 9))])));

        Assert.Equal(new[] { (Day(2027, 1, 1), Day(2027, 2, 28)) }, Permitted(adjacent));
        Assert.Equal(new[] { (Day(2027, 1, 1), Day(2027, 2, 28)) }, Permitted(overlapping));
        Assert.Equal(new[] { (Day(2027, 1, 1), Day(2027, 2, 28)) }, Permitted(contained));
        Assert.Equal(new[] { (Day(2027, 1, 1), Day(2027, 2, 28)), (Day(2027, 3, 2), Day(2027, 3, 5)) }, Permitted(disjoint));
        Assert.Equal(new[] { (Day(2027, 5, 9), Day(2027, 5, 9)) }, Permitted(single));

        var blackout = Provision(Groups(travelDate: Dates(blackout: [Period(Day(2027, 12, 24), Day(2027, 12, 25)), Period(Day(2027, 12, 26), Day(2027, 12, 26))])));

        Assert.Empty(blackout.TravelDate!.PermittedPeriods);
        Assert.Equal(new[] { (Day(2027, 12, 24), Day(2027, 12, 26)) }, Blackouts(blackout));
    }

    [Fact]
    public void V121_D09_D10_a_reversed_missing_or_repeated_period_is_refused_before_it_is_stored()
    {
        var period = Period(Day(2027, 4, 1), Day(2027, 4, 30));

        BusinessAssert.Throws(16302, 422, () => Provision(Groups(travelDate: Dates([Period(Day(2027, 4, 30), Day(2027, 4, 1))]))));
        BusinessAssert.Throws(16302, 422, () => Provision(Groups(travelDate: Dates([Period(Day(2027, 4, 1), default)]))));
        BusinessAssert.Throws(16302, 422, () => Provision(Groups(travelDate: Dates([Period(default, Day(2027, 4, 1))]))));
        BusinessAssert.Throws(16302, 422, () => Provision(Groups(travelDate: Dates([period, period]))));
        BusinessAssert.Throws(16302, 422, () => Provision(Groups(travelDate: Dates(blackout: [period, period]))));
        BusinessAssert.Throws(16302, 422, () => Provision(Groups(travelDate: Dates(blackout: [Period(Day(2027, 4, 2), Day(2027, 4, 1))]))));

        var provision = Provision(Groups(travelDate: Dates([period], [Period(Day(2027, 4, 10), Day(2027, 4, 12))])));
        var ids = new SequentialIdGenerator();

        BusinessAssert.Throws(16309, 409, () => provision.AddPermittedTravelPeriod(period, ids));
        BusinessAssert.Throws(16309, 409, () => provision.AddBlackoutPeriod(Period(Day(2027, 4, 10), Day(2027, 4, 12)), ids));
        BusinessAssert.Throws(16302, 422, () => provision.AddPermittedTravelPeriod(Period(Day(2027, 6, 2), Day(2027, 6, 1)), ids));
        Assert.Equal((1, 1), (provision.TravelDate!.PermittedPeriods.Count, provision.TravelDate.BlackoutPeriods.Count));
    }

    [Fact]
    public void V121_D10_M07_row_level_periods_merge_into_the_oldest_row_and_keep_untouched_identities()
    {
        var ids = new SequentialIdGenerator();
        var provision = Provision(ids: ids);

        var april = provision.AddPermittedTravelPeriod(Period(Day(2027, 4, 1), Day(2027, 4, 30)), ids);
        var june = provision.AddPermittedTravelPeriod(Period(Day(2027, 6, 1), Day(2027, 6, 30)), ids);
        var ruleId = provision.TravelDate!.Id;

        Assert.Equal(new[] { april.Id, june.Id }, provision.TravelDate.PermittedPeriods.Select(row => row.Id));
        Assert.All(provision.TravelDate.PermittedPeriods, row => Assert.Equal(ruleId, row.ProvisionTravelDateRuleId));

        var merged = provision.AddPermittedTravelPeriod(Period(Day(2027, 5, 1), Day(2027, 5, 31)), ids);

        Assert.Equal(april.Id, merged.Id);
        Assert.Equal(new[] { (Day(2027, 4, 1), Day(2027, 6, 30)) }, Permitted(provision));

        var august = provision.AddPermittedTravelPeriod(Period(Day(2027, 8, 1), Day(2027, 8, 31)), ids);
        var changed = provision.ChangePermittedTravelPeriod(august.Id, Period(Day(2027, 8, 5), Day(2027, 8, 20)));

        Assert.Equal((august.Id, Day(2027, 8, 5), Day(2027, 8, 20)), (changed.Id, changed.StartDate, changed.EndDate));
        BusinessAssert.Throws(16309, 409, () => provision.ChangePermittedTravelPeriod(august.Id, Period(Day(2027, 4, 1), Day(2027, 6, 30))));
        BusinessAssert.Throws(16308, 404, () => provision.ChangePermittedTravelPeriod(999_999, Period(Day(2027, 9, 1), Day(2027, 9, 2))));
        BusinessAssert.Throws(16308, 404, () => provision.RemoveBlackoutPeriod(august.Id));

        var blackout = provision.AddBlackoutPeriod(Period(Day(2027, 4, 10), Day(2027, 4, 12)), ids);

        provision.RemovePermittedTravelPeriod(august.Id);
        provision.RemovePermittedTravelPeriod(april.Id);

        Assert.Equal((ruleId, 0, 1), (provision.TravelDate!.Id, provision.TravelDate.PermittedPeriods.Count, provision.TravelDate.BlackoutPeriods.Count));

        provision.RemoveBlackoutPeriod(blackout.Id);

        Assert.Null(provision.TravelDate);
    }

    [Fact]
    public void V121_D11_a_period_ending_on_the_last_calendar_day_is_canonicalized_without_overflow()
    {
        var provision = Provision(Groups(travelDate: Dates(
            [Period(DateOnly.MaxValue, DateOnly.MaxValue), Period(DateOnly.MaxValue.AddDays(-1), DateOnly.MaxValue.AddDays(-1)), Period(Day(2027, 1, 1), DateOnly.MaxValue.AddDays(-10))])));

        Assert.Equal(new[] { (Day(2027, 1, 1), DateOnly.MaxValue.AddDays(-10)), (DateOnly.MaxValue.AddDays(-1), DateOnly.MaxValue) }, Permitted(provision));

        var open = Provision(Groups(travelDate: Dates([Period(Day(2027, 1, 1), DateOnly.MaxValue), Period(DateOnly.MaxValue, DateOnly.MaxValue)])));

        Assert.Equal(new[] { (Day(2027, 1, 1), DateOnly.MaxValue) }, Permitted(open));
    }

    [Fact]
    public void V121_T04_T05_T10_a_window_needs_a_mask_in_range_ordered_times_a_known_effect_and_no_duplicate()
    {
        var window = Window(Monday, 8, 12);

        foreach (var mask in new byte[] { 0, 128, 255 })
            BusinessAssert.Throws(16302, 422, () => Provision(Groups(dayTimeApplication: Windows(Window(mask, 8, 12)))));

        BusinessAssert.Throws(16302, 422, () => Provision(Groups(dayTimeApplication: Windows(Window(Monday, 12, 8)))));
        BusinessAssert.Throws(16302, 422, () => Provision(Groups(dayTimeApplication: Windows(Window(Monday, 9, 9)))));
        BusinessAssert.Throws(16302, 422, () => Provision(Groups(dayTimeApplication: Windows(new ProvisionDayTimeWindowArgs(Monday, null, TimeOnly.MinValue, DayTimeRestrictionEffect.Allow)))));
        BusinessAssert.Throws(16302, 422, () => Provision(Groups(dayTimeApplication: Windows(Window(Monday, 8, 12, (DayTimeRestrictionEffect)9)))));
        BusinessAssert.Throws(16302, 422, () => Provision(Groups(dayTimeApplication: Windows(window, window))));

        var night = Provision(Groups(dayTimeApplication: Windows(Window(Saturday, 22), Window(Sunday, toHour: 2), Window(EveryDay), Window(Monday, 8, 12, DayTimeRestrictionEffect.Deny), window)));

        Assert.Equal(
            new[] { (Saturday, (int?)22, (int?)null), (Sunday, null, 2), (EveryDay, null, null), (Monday, 8, 12), (Monday, 8, 12) },
            night.DayTimeApplication!.Windows.Select(row => (row.DaysOfWeekMask, row.StartLocalTime?.Hour, row.EndLocalTime?.Hour)));

        var ids = new SequentialIdGenerator();
        var provision = Provision(ids: ids);
        var added = provision.AddDayTimeWindow(window, ids);
        var ruleId = provision.DayTimeApplication!.Id;

        BusinessAssert.Throws(16309, 409, () => provision.AddDayTimeWindow(window, ids));
        BusinessAssert.Throws(16302, 422, () => provision.AddDayTimeWindow(Window(0, 8, 12), ids));

        var deny = provision.AddDayTimeWindow(Window(Monday, 9, 10, DayTimeRestrictionEffect.Deny), ids);
        var changed = provision.ChangeDayTimeWindow(deny.Id, Window((byte)(Monday | Tuesday), 9, 11, DayTimeRestrictionEffect.Deny));

        Assert.Equal((deny.Id, (byte)3, 11), (changed.Id, changed.DaysOfWeekMask, changed.EndLocalTime!.Value.Hour));
        BusinessAssert.Throws(16309, 409, () => provision.ChangeDayTimeWindow(deny.Id, window));
        BusinessAssert.Throws(16308, 404, () => provision.RemoveDayTimeWindow(424242));

        provision.RemoveDayTimeWindow(deny.Id);

        Assert.Equal((ruleId, added.Id), (provision.DayTimeApplication!.Id, provision.DayTimeApplication.Windows.Single().Id));

        provision.RemoveDayTimeWindow(added.Id);

        Assert.Null(provision.DayTimeApplication);
    }

    [Fact]
    public void V121_E02_E12_passenger_types_and_age_bands_are_typed_disjoint_and_unique()
    {
        var provision = Provision(Groups(Passengers([PassengerTypeCode.ADT], [new ProvisionAgeBandArgs(65, null), new ProvisionAgeBandArgs(0, 65)])));

        Assert.Equal(new[] { (65, (int?)null), (0, 65) }, provision.PassengerEligibility!.AgeBands.Select(band => (band.AgeFromInclusive, band.AgeToExclusive)));
        BusinessAssert.Throws(16302, 422, () => Provision(Groups(Passengers(bands: [new ProvisionAgeBandArgs(0, 65), new ProvisionAgeBandArgs(64, null)]))));
        BusinessAssert.Throws(16302, 422, () => Provision(Groups(Passengers(bands: [new ProvisionAgeBandArgs(0, null), new ProvisionAgeBandArgs(65, null)]))));
        BusinessAssert.Throws(16302, 422, () => Provision(Groups(Passengers(bands: [new ProvisionAgeBandArgs(0, 65), new ProvisionAgeBandArgs(0, 65)]))));
        BusinessAssert.Throws(16302, 422, () => Provision(Groups(Passengers(bands: [new ProvisionAgeBandArgs(-1, 5)]))));
        BusinessAssert.Throws(16302, 422, () => Provision(Groups(Passengers(bands: [new ProvisionAgeBandArgs(12, 12)]))));
        BusinessAssert.Throws(16302, 422, () => Provision(Groups(Passengers([PassengerTypeCode.ADT, PassengerTypeCode.ADT]))));
        BusinessAssert.Throws(16302, 422, () => Provision(Groups(Passengers([(PassengerTypeCode)(-1)]))));
    }

    [Fact]
    public void V121_E12_selector_rows_are_validated_unique_and_route_pairs_reject_inverse_duplicates()
    {
        BusinessAssert.Throws(16302, 422, () => Provision(Groups(salesRestrictions: Sales(pointsOfSale: [501, 501]))));
        BusinessAssert.Throws(16302, 422, () => Provision(Groups(salesRestrictions: Sales(customers: [0]))));
        BusinessAssert.Throws(16302, 422, () => Provision(Groups(salesRestrictions: Sales(customerTypes: [(CustomerType)99]))));
        BusinessAssert.Throws(16302, 422, () => Provision(Groups(salesRestrictions: Sales(Now, Now))));
        BusinessAssert.Throws(16302, 422, () => Provision(Groups(geography: Geography(origins: [Thr, Thr]))));
        BusinessAssert.Throws(16302, 422, () => Provision(Groups(geography: Geography(pairs: [Pair(Thr, Thr)]))));
        BusinessAssert.Throws(16302, 422, () => Provision(Groups(geography: Geography(pairs: [Pair(Thr, Ist), Pair(Thr, Ist, RoutePairDirection.BothDirections)]))));
        BusinessAssert.Throws(16302, 422, () => Provision(Groups(geography: Geography(pairs: [Pair(Thr, Ist), Pair(Ist, Thr, RoutePairDirection.BothDirections)]))));
        BusinessAssert.Throws(16302, 422, () => Provision(Groups(geography: Geography(locations: [Location(ServiceLocationType.Airport, Thr), Location(ServiceLocationType.Airport, Thr)]))));
        BusinessAssert.Throws(16302, 422, () => Provision(Groups(geography: Geography(locations: [Location((ServiceLocationType)9, Thr)]))));
        BusinessAssert.Throws(16302, 422, () => Provision(Groups(geography: Geography(countries: [98, 98]))));
        BusinessAssert.Throws(16302, 422, () => Provision(Groups(flightApplication: Flights(numbers: ["W5112", " w5112 "]))));
        BusinessAssert.Throws(16302, 422, () => Provision(Groups(flightApplication: Flights(numbers: [new string('9', 17)]))));
        BusinessAssert.Throws(16302, 422, () => Provision(Groups(fareApplication: Fares(bases: [""]))));
        BusinessAssert.Throws(16302, 422, () => Provision(Groups(fareApplication: Fares(types: [(AirFareType)99]))));
        BusinessAssert.Throws(16302, 422, () => Provision(Groups(fareApplication: Fares(rbds: [41, 41]))));

        var geography = Provision(Groups(geography: Geography(
            pairs: [Pair(Thr, Ist), Pair(Ist, Thr)],
            locations: [Location(ServiceLocationType.Airport, Thr), Location(ServiceLocationType.City, Thr), Location(ServiceLocationType.Country, 98)])));

        Assert.Equal((2, 3), (geography.Geography!.RoutePairs.Count, geography.Geography.ServiceLocations.Count));
    }

    [Fact]
    public void V121_M07_M08_replacing_a_group_keeps_the_identity_of_unchanged_rows_and_only_a_draft_is_edited()
    {
        var ids = new SequentialIdGenerator();
        var provision = Provision(
            Groups(
                Passengers([PassengerTypeCode.ADT, PassengerTypeCode.CHD]),
                flightApplication: Flights(flights: [81234, 81240]),
                travelDate: Dates([Period(Day(2027, 4, 1), Day(2027, 4, 30))], [Period(Day(2027, 4, 10), Day(2027, 4, 12))])),
            ids: ids);
        var passengerRule = provision.PassengerEligibility!.Id;
        var adult = provision.PassengerEligibility.PassengerTypes.Single(row => row.PassengerTypeCode == PassengerTypeCode.ADT).Id;
        var flight = provision.FlightApplication!.Flights.Single(row => row.FlightId == 81240).Id;
        var permitted = provision.TravelDate!.PermittedPeriods.Single().Id;
        var dateRule = provision.TravelDate.Id;

        provision.ChangePassengerEligibility(Passengers([PassengerTypeCode.INF, PassengerTypeCode.ADT]), ids);
        provision.ChangeFlightApplication(Flights(flights: [81240], aircraft: [2]), ids);
        provision.ChangeTravelDate(Dates([Period(Day(2027, 4, 1), Day(2027, 4, 30)), Period(Day(2027, 6, 1), Day(2027, 6, 30))]), ids);

        Assert.Equal(passengerRule, provision.PassengerEligibility!.Id);
        Assert.Equal(new[] { PassengerTypeCode.INF, PassengerTypeCode.ADT }, provision.PassengerEligibility.PassengerTypes.Select(row => row.PassengerTypeCode));
        Assert.Equal(adult, provision.PassengerEligibility.PassengerTypes.Single(row => row.PassengerTypeCode == PassengerTypeCode.ADT).Id);
        Assert.Equal((flight, 2), (provision.FlightApplication!.Flights.Single().Id, provision.FlightApplication.Aircraft.Single().AircraftId));
        Assert.Equal((dateRule, permitted), (provision.TravelDate!.Id, provision.TravelDate.PermittedPeriods.Single(row => row.StartDate == Day(2027, 4, 1)).Id));
        Assert.Empty(provision.TravelDate.BlackoutPeriods);

        provision.ChangePassengerEligibility(null, ids);
        provision.ChangeFlightApplication(Flights(), ids);

        Assert.Null(provision.PassengerEligibility);
        Assert.Null(provision.FlightApplication);

        provision.Activate(CarrierDefinition(), Now);

        BusinessAssert.Throws(16303, 409, () => provision.ChangeTravelDate(null, ids));
        BusinessAssert.Throws(16303, 409, () => provision.ChangeGeography(Geography(origins: [Thr]), ids));
        BusinessAssert.Throws(16303, 409, () => provision.AddPermittedTravelPeriod(Period(Day(2027, 9, 1), Day(2027, 9, 2)), ids));
        BusinessAssert.Throws(16303, 409, () => provision.RemovePermittedTravelPeriod(permitted));
        BusinessAssert.Throws(16303, 409, () => provision.AddDayTimeWindow(Window(Monday), ids));
        BusinessAssert.Throws(16303, 409, () => Replace(provision, Groups(), ids));
        Assert.Equal(2, provision.TravelDate.PermittedPeriods.Count);
    }

    [Fact]
    public void V121_M07_a_refused_change_leaves_the_draft_untouched()
    {
        var ids = new SequentialIdGenerator();
        var provision = Provision(Groups(Passengers([PassengerTypeCode.ADT]), travelDate: Dates([Period(Day(2027, 4, 1), Day(2027, 4, 30))])), ids: ids);
        var adult = provision.PassengerEligibility!.PassengerTypes.Single().Id;

        BusinessAssert.Throws(16302, 422, () => Replace(
            provision,
            Groups(Passengers([PassengerTypeCode.CHD]), geography: Geography(origins: [Thr]), travelDate: Dates([Period(Day(2027, 5, 2), Day(2027, 5, 1))])),
            ids));

        Assert.Equal((adult, PassengerTypeCode.ADT), provision.PassengerEligibility.PassengerTypes.Select(row => (row.Id, row.PassengerTypeCode)).Single());
        Assert.Null(provision.Geography);
        Assert.Equal(new[] { (Day(2027, 4, 1), Day(2027, 4, 30)) }, Permitted(provision));
    }

    [Fact]
    public void V121_P06_the_baggage_and_seat_groups_follow_the_application_type_and_seat_numbers_need_an_aircraft()
    {
        BusinessAssert.Throws(16302, 422, () => Provision(Groups(baggageApplication: Baggage())));
        BusinessAssert.Throws(16302, 422, () => Provision(applicationType: ProvisionApplicationType.Seat));
        BusinessAssert.Throws(16302, 422, () => Provision(Groups(seatApplication: Seats(characteristics: ["W"]))));
        BusinessAssert.Throws(16302, 422, () => Provision(Groups(seatApplication: Seats(["12A"])), ProvisionApplicationType.Seat));
        BusinessAssert.Throws(16302, 422, () => Provision(Groups(baggageApplication: Baggage() with { Weight = 0m }), ProvisionApplicationType.Baggage));
        BusinessAssert.Throws(16302, 422, () => Provision(Groups(baggageApplication: Baggage() with { FirstExcessPiece = 3, LastExcessPiece = 2 }), ProvisionApplicationType.Baggage));

        var baggage = Provision(applicationType: ProvisionApplicationType.Baggage);
        var seat = Provision(Groups(flightApplication: Flights(aircraft: [1]), seatApplication: Seats(["12A", "12F"], ["LS"])), ProvisionApplicationType.Seat);
        var ids = new SequentialIdGenerator();

        Assert.Equal((23m, WeightUnit.Kg, BaggagePurchaseApplication.Prepaid), (baggage.BaggageApplication!.Weight, baggage.BaggageApplication.WeightUnit, baggage.BaggageApplication.PurchaseApplication));
        Assert.Equal((2, 1), (seat.SeatApplication!.SeatNumbers.Count, seat.SeatApplication.SeatCharacteristics.Count));
        BusinessAssert.Throws(16302, 422, () => baggage.ChangeBaggageApplication(null, ids));
        BusinessAssert.Throws(16302, 422, () => seat.ChangeSeatApplication(Seats(), ids));
        BusinessAssert.Throws(16302, 422, () => seat.ChangeFlightApplication(null, ids));
        BusinessAssert.Throws(16302, 422, () => baggage.ChangeSeatApplication(Seats(characteristics: ["W"]), ids));

        seat.ChangeSeatApplication(Seats(characteristics: ["W"]), ids);
        seat.ChangeFlightApplication(null, ids);
        baggage.ChangeBaggageApplication(Baggage() with { Weight = 32m }, ids);

        Assert.Equal(("W", 32m), (seat.SeatApplication!.SeatCharacteristics.Single().CharacteristicCode, baggage.BaggageApplication!.Weight));
    }

    [Theory]
    [InlineData(PricingUnit.PerPassenger, AncillaryQuantityUnit.Each, true)]
    [InlineData(PricingUnit.PerRoom, AncillaryQuantityUnit.Each, true)]
    [InlineData(PricingUnit.PerItem, AncillaryQuantityUnit.Each, true)]
    [InlineData(PricingUnit.PerVehicle, AncillaryQuantityUnit.Each, true)]
    [InlineData(PricingUnit.PerSeat, AncillaryQuantityUnit.Each, true)]
    [InlineData(PricingUnit.PerPiece, AncillaryQuantityUnit.Piece, true)]
    [InlineData(PricingUnit.PerKilogram, AncillaryQuantityUnit.Kilogram, true)]
    [InlineData(PricingUnit.PerPiece, AncillaryQuantityUnit.Kilogram, false)]
    [InlineData(PricingUnit.PerKilogram, AncillaryQuantityUnit.Piece, false)]
    [InlineData(PricingUnit.PerPassenger, AncillaryQuantityUnit.Kilogram, false)]
    [InlineData(PricingUnit.PerItem, AncillaryQuantityUnit.Piece, false)]
    [InlineData(PricingUnit.PerSeat, AncillaryQuantityUnit.Kilogram, false)]
    [InlineData(PricingUnit.PerPiece, AncillaryQuantityUnit.Each, false)]
    public void V121_P03_P04_P05_P07_P08_P09_the_quantity_unit_must_fit_the_pricing_unit_at_publication(PricingUnit pricingUnit, AncillaryQuantityUnit quantityUnit, bool compatible)
    {
        var definition = CarrierDefinition(pricingUnit: pricingUnit);
        var provision = Provision(quantityUnit: quantityUnit);

        if (compatible)
        {
            provision.Activate(definition, Now);
            Assert.Equal(ProvisionStatus.Active, provision.Status);

            return;
        }

        BusinessAssert.Throws(16312, 409, () => provision.Activate(definition, Now));
        Assert.Equal(ProvisionStatus.Draft, provision.Status);
    }

    [Fact]
    public void V121_E10_E11_A03_P18_flight_fare_route_baggage_seat_and_ticket_rules_need_a_flight_dated_service()
    {
        var flightDated = CarrierDefinition();
        (string Name, Func<AncillaryProvision> Rule)[] flightOnly =
        [
            ("fare", () => Provision(Groups(fareApplication: Fares(families: [5])))),
            ("flight", () => Provision(Groups(flightApplication: Flights(flights: [81234])))),
            ("origin", () => Provision(Groups(geography: Geography(origins: [Thr])))),
            ("route", () => Provision(Groups(geography: Geography(pairs: [Pair(Thr, Ist)])))),
            ("ticket", () => Provision(Groups(advancePurchase: new ProvisionAdvancePurchaseArgs(0, TimeUnit.Hours, true)))),
            ("seat", () => Provision(Groups(seatApplication: Seats(characteristics: ["W"])), ProvisionApplicationType.Seat))
        ];

        foreach (var basis in new[] { ServiceDateBasis.ServiceStart, ServiceDateBasis.CheckIn, ServiceDateBasis.CoverageStart, ServiceDateBasis.Activation })
        {
            var standalone = CarrierDefinition(serviceDateBasis: basis);

            foreach (var (_, rule) in flightOnly)
                BusinessAssert.Throws(16313, 409, () => rule().Activate(standalone, Now));

            var neutral = Provision(Groups(
                Passengers([PassengerTypeCode.ADT], [new ProvisionAgeBandArgs(0, 65)]),
                Sales(pointsOfSale: [501]),
                Geography(locations: [Location(ServiceLocationType.City, 7)], countries: [98]),
                travelDate: Dates([Period(Day(2027, 4, 1), Day(2027, 4, 30))]),
                dayTimeApplication: Windows(Window(Weekdays, 9, 17)),
                advancePurchase: new ProvisionAdvancePurchaseArgs(24, TimeUnit.Hours, false)));

            neutral.Activate(standalone, Now);

            Assert.Equal(ProvisionStatus.Active, neutral.Status);
        }

        foreach (var (_, rule) in flightOnly)
        {
            var provision = rule();

            provision.Activate(flightDated, Now);

            Assert.Equal(ProvisionStatus.Active, provision.Status);
        }

        var baggage = Provision(applicationType: ProvisionApplicationType.Baggage, quantityUnit: AncillaryQuantityUnit.Piece);

        BusinessAssert.Throws(16313, 409, () => baggage.Activate(CarrierDefinition(pricingUnit: PricingUnit.PerPiece, serviceDateBasis: ServiceDateBasis.ServiceStart), Now));
        baggage.Activate(CarrierDefinition(pricingUnit: PricingUnit.PerPiece), Now);
        BusinessAssert.Throws(16302, 422, () => Provision().Activate(CarrierDefinition(id: 1002), Now));
    }

    [Theory]
    [InlineData(TimeUnit.Minutes, true)]
    [InlineData(TimeUnit.Hours, true)]
    [InlineData(TimeUnit.Days, true)]
    [InlineData(TimeUnit.Months, true)]
    [InlineData(TimeUnit.Weeks, false)]
    [InlineData(TimeUnit.Years, false)]
    public void V121_A01_A02_an_advance_purchase_unit_without_a_defined_meaning_is_not_published(TimeUnit unit, bool activatable)
    {
        var provision = Provision(Groups(advancePurchase: new ProvisionAdvancePurchaseArgs(1, unit, false)));

        BusinessAssert.Throws(16302, 422, () => Provision(Groups(advancePurchase: new ProvisionAdvancePurchaseArgs(-1, unit, false))));
        BusinessAssert.Throws(16302, 422, () => Provision(Groups(advancePurchase: new ProvisionAdvancePurchaseArgs(1, (TimeUnit)99, false))));

        if (activatable)
        {
            provision.Activate(CarrierDefinition(), Now);
            Assert.Equal((ProvisionStatus.Active, unit), (provision.Status, provision.AdvancePurchase!.Unit));

            return;
        }

        BusinessAssert.Throws(16314, 422, () => provision.Activate(CarrierDefinition(), Now));
    }

    [Fact]
    public void V121_T06_D06_a_date_or_time_rule_that_can_never_match_is_not_published()
    {
        var definition = CarrierDefinition();
        AncillaryProvision[] unreachable =
        [
            Provision(Groups(travelDate: Dates([Period(Day(2027, 4, 10), Day(2027, 4, 12))], [Period(Day(2027, 4, 1), Day(2027, 4, 30))]))),
            Provision(Groups(travelDate: Dates([Period(Day(2027, 4, 10), Day(2027, 4, 12))], [Period(Day(2027, 4, 10), Day(2027, 4, 11)), Period(Day(2027, 4, 12), Day(2027, 4, 12))]))),
            Provision(Groups(dayTimeApplication: Windows(Window(Monday, 9, 10), Window(Monday, 8, 12, DayTimeRestrictionEffect.Deny)))),
            Provision(Groups(dayTimeApplication: Windows(Window(Weekdays, 9, 17), Window(EveryDay, effect: DayTimeRestrictionEffect.Deny)))),
            Provision(Groups(dayTimeApplication: Windows(Window(EveryDay, effect: DayTimeRestrictionEffect.Deny)))),
            Provision(Groups(dayTimeApplication: Windows(Window(Monday, 9, 12), Window(Monday, 9, 10, DayTimeRestrictionEffect.Deny), Window(Monday, 10, 12, DayTimeRestrictionEffect.Deny))))
        ];
        AncillaryProvision[] reachable =
        [
            Provision(Groups(travelDate: Dates([Period(Day(2027, 4, 1), Day(2027, 4, 30))], [Period(Day(2027, 4, 10), Day(2027, 4, 12))]))),
            Provision(Groups(travelDate: Dates(blackout: [Period(Day(2027, 12, 25), Day(2027, 12, 25))]))),
            Provision(Groups(dayTimeApplication: Windows(Window(Monday, 8, 12), Window(Monday, 9, 10, DayTimeRestrictionEffect.Deny)))),
            Provision(Groups(dayTimeApplication: Windows(Window(Friday, effect: DayTimeRestrictionEffect.Deny)))),
            Provision(Groups(dayTimeApplication: Windows(Window(Monday, 9, 10), Window(Tuesday, 9, 10), Window(Monday, effect: DayTimeRestrictionEffect.Deny))))
        ];

        Assert.All(unreachable, provision => BusinessAssert.Throws(16315, 409, () => provision.Activate(definition, Now)));
        Assert.All(reachable, provision =>
        {
            provision.Activate(definition, Now);
            Assert.Equal(ProvisionStatus.Active, provision.Status);
        });
    }

    [Fact]
    public void V121_P15_REQ_the_provision_owns_eligibility_and_outcome_but_no_money()
    {
        var names = PropertiesOf<AncillaryProvision>();
        var wheelchair = Provision(disposition: CommercialDisposition.Free);

        Assert.Equal(
            new[]
            {
                "ActivatedAt", "AdvancePurchase", "ApplicationType", "Availability", "BaggageApplication", "CoverageScope", "CreatedAt", "DayTimeApplication",
                "FareApplication", "FlightApplication", "Fulfillment", "Geography", "Outcome", "PassengerEligibility", "PurchaseStage", "Quantity", "RetiredAt", "SalesRestrictions",
                "SeatApplication", "Sequence", "ServiceDefinitionId", "Settlement", "Status", "SuspendedAt", "TravelDate"
            },
            names);
        Assert.Equal((CommercialDisposition.Free, false), (wheelchair.Outcome.Disposition, wheelchair.Outcome.DocumentRequired));
        Assert.DoesNotContain(
            typeof(ProvisionTravelDateRule).Assembly.GetTypes().Where(type => type.Namespace?.Contains("AncillaryProvisionAggregate", StringComparison.Ordinal) == true).SelectMany(type => type.GetProperties()),
            property => property.Name is "Amount" or "CurrencyId" or "UnitAmount");
    }
}
