using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Ancillary.Domain.ConformanceTests.Fakes;
using AeroTech.Ancillary.Domain.ConformanceTests.Fixtures;
using AeroTech.Ancillary.Domain.ConformanceTests.Oracle;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;
using AeroTech.Messages.Core.Enums;
using Xunit;
using static AeroTech.Ancillary.Domain.ConformanceTests.Fixtures.P1Fixtures;
using static AeroTech.Ancillary.Domain.ConformanceTests.Fixtures.V121Fixtures;

namespace AeroTech.Ancillary.Domain.ConformanceTests;

public class V121ReferenceOracleTruthTableTests
{
    private static readonly TimeZoneInfo Tehran = TimeZoneInfo.CreateCustomTimeZone("ORACLE_THR", new TimeSpan(3, 30, 0), "Tehran", "Tehran");

    private static readonly TimeZoneInfo Istanbul = TimeZoneInfo.CreateCustomTimeZone("ORACLE_IST", TimeSpan.FromHours(3), "Istanbul", "Istanbul");

    private static readonly TimeZoneInfo Paris = TimeZoneInfo.CreateCustomTimeZone(
        "ORACLE_PAR",
        TimeSpan.FromHours(1),
        "Paris",
        "Paris",
        "Paris Summer",
        [
            TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(
                DateTime.MinValue.Date,
                DateTime.MaxValue.Date,
                TimeSpan.FromHours(1),
                TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 2, 0, 0), 3, 5, DayOfWeek.Sunday),
                TimeZoneInfo.TransitionTime.CreateFloatingDateRule(new DateTime(1, 1, 1, 3, 0, 0), 10, 5, DayOfWeek.Sunday))
        ]);

    private static OracleContext At(int year, int month, int day, int hour = 12, int minute = 0, TimeZoneInfo? zone = null)
        => new() { Occurrence = OracleOccurrence.AtLocal(new DateTime(year, month, day, hour, minute, 0), zone ?? Tehran) };

    private static AncillaryProvision Active(
        ProvisionRulesArgs rules,
        CommercialDisposition disposition = CommercialDisposition.Paid,
        int sequence = 10,
        long id = 5001,
        ServiceDateBasis basis = ServiceDateBasis.FlightDeparture)
    {
        var provision = Provision(rules, disposition: disposition, id: id, sequence: sequence, ids: new SequentialIdGenerator());

        provision.Activate(CarrierDefinition(serviceDateBasis: basis), null, Now);

        return provision;
    }

    [Theory]
    [InlineData(2027, 3, 31, OracleVerdict.NoMatch)]
    [InlineData(2027, 4, 1, OracleVerdict.Match)]
    [InlineData(2027, 4, 9, OracleVerdict.Match)]
    [InlineData(2027, 4, 10, OracleVerdict.NoMatch)]
    [InlineData(2027, 4, 11, OracleVerdict.NoMatch)]
    [InlineData(2027, 4, 12, OracleVerdict.NoMatch)]
    [InlineData(2027, 4, 13, OracleVerdict.Match)]
    [InlineData(2027, 4, 30, OracleVerdict.Match)]
    [InlineData(2027, 5, 1, OracleVerdict.NoMatch)]
    public void V121_D06_a_blackout_inside_a_permitted_period_denies_only_its_own_days(int year, int month, int day, OracleVerdict expected)
    {
        var provision = Active(Groups(travelDate: Dates([Period(Day(2027, 4, 1), Day(2027, 4, 30))], [Period(Day(2027, 4, 10), Day(2027, 4, 12))])));

        Assert.Equal(expected, ProvisionRuleOracle.Evaluate(provision, At(year, month, day)));
    }

    [Fact]
    public void V121_D07_D08_a_blackout_without_a_permitted_period_is_not_deny_all_and_no_rule_is_no_restriction()
    {
        var christmas = Active(Groups(travelDate: Dates(blackout: [Period(Day(2027, 12, 25), Day(2027, 12, 25))])));
        var unrestricted = Active(Groups());

        Assert.Equal(OracleVerdict.Match, ProvisionRuleOracle.Evaluate(christmas, At(2027, 12, 24)));
        Assert.Equal(OracleVerdict.NoMatch, ProvisionRuleOracle.Evaluate(christmas, At(2027, 12, 25)));
        Assert.Equal(OracleVerdict.Match, ProvisionRuleOracle.Evaluate(christmas, At(2027, 12, 26)));
        Assert.Equal(OracleVerdict.UnsupportedContext, ProvisionRuleOracle.Evaluate(christmas, new OracleContext()));
        Assert.Null(unrestricted.TravelDate);
        Assert.Equal(OracleVerdict.Match, ProvisionRuleOracle.Evaluate(unrestricted, new OracleContext()));
        Assert.Equal(OracleVerdict.Match, ProvisionRuleOracle.Evaluate(unrestricted, At(2027, 12, 25)));
    }

    [Fact]
    public void V121_D01_D02_the_canonical_periods_match_exactly_the_authored_dates()
    {
        var consecutive = Enumerable.Range(0, 1000).Select(offset => Day(2027, 1, 1).AddDays(offset)).ToArray();
        var sparse = Enumerable.Range(0, 1000).Select(offset => Day(2027, 1, 1).AddDays(offset * 2)).ToArray();
        var range = Active(Groups(travelDate: Dates(consecutive.Select(day => Period(day, day)).ToArray())));
        var scattered = Active(Groups(travelDate: Dates(sparse.Select(day => Period(day, day)).ToArray())));
        OracleContext On(DateOnly date) => At(date.Year, date.Month, date.Day);

        Assert.All(consecutive, date => Assert.Equal(OracleVerdict.Match, ProvisionRuleOracle.Evaluate(range, On(date))));
        Assert.Equal(OracleVerdict.NoMatch, ProvisionRuleOracle.Evaluate(range, On(Day(2026, 12, 31))));
        Assert.Equal(OracleVerdict.NoMatch, ProvisionRuleOracle.Evaluate(range, On(Day(2029, 9, 27))));
        Assert.All(sparse, date =>
        {
            Assert.Equal(OracleVerdict.Match, ProvisionRuleOracle.Evaluate(scattered, On(date)));
            Assert.Equal(OracleVerdict.NoMatch, ProvisionRuleOracle.Evaluate(scattered, On(date.AddDays(1))));
        });
    }

    [Fact]
    public void V121_D12_D13_the_date_is_read_at_the_service_occurrence_of_the_definition_basis_and_never_from_a_flight()
    {
        var rules = Groups(travelDate: Dates([Period(Day(2027, 7, 10), Day(2027, 7, 12))]));
        var flightDeparture = new DateTime(2027, 7, 9, 23, 30, 0);
        var checkIn = new DateTime(2027, 7, 10, 14, 0, 0);

        foreach (var basis in new[] { ServiceDateBasis.ServiceStart })
        {
            var provision = Active(rules, basis: basis);

            Assert.Equal(OracleVerdict.Match, ProvisionRuleOracle.Evaluate(provision, new OracleContext { Occurrence = OracleOccurrence.AtLocal(checkIn, Istanbul) }));
            Assert.Equal(OracleVerdict.NoMatch, ProvisionRuleOracle.Evaluate(provision, new OracleContext { Occurrence = OracleOccurrence.AtLocal(flightDeparture, Tehran) }));
            Assert.Equal(OracleVerdict.UnsupportedContext, ProvisionRuleOracle.Evaluate(provision, new OracleContext { OriginAirportId = Thr, DestinationAirportId = Ist, FlightId = 81234 }));
        }
    }

    [Theory]
    [InlineData(2027, 4, 5, 9, 0, OracleVerdict.Match)]
    [InlineData(2027, 4, 5, 8, 59, OracleVerdict.NoMatch)]
    [InlineData(2027, 4, 9, 16, 59, OracleVerdict.Match)]
    [InlineData(2027, 4, 9, 17, 0, OracleVerdict.NoMatch)]
    [InlineData(2027, 4, 10, 10, 0, OracleVerdict.NoMatch)]
    [InlineData(2027, 4, 11, 10, 0, OracleVerdict.NoMatch)]
    public void V121_T01_one_weekday_mask_window_allows_monday_to_friday_office_hours(int year, int month, int day, int hour, int minute, OracleVerdict expected)
    {
        var provision = Active(Groups(dayTimeApplication: Windows(Window(Weekdays, 9, 17))));

        Assert.Single(provision.DayTimeApplication!.Windows);
        Assert.Equal(DayOfWeek.Monday, new DateOnly(2027, 4, 5).DayOfWeek);
        Assert.Equal(expected, ProvisionRuleOracle.Evaluate(provision, At(year, month, day, hour, minute)));
    }

    [Theory]
    [InlineData(7, 59, OracleVerdict.NoMatch)]
    [InlineData(8, 0, OracleVerdict.Match)]
    [InlineData(8, 30, OracleVerdict.Match)]
    [InlineData(9, 0, OracleVerdict.NoMatch)]
    [InlineData(9, 30, OracleVerdict.NoMatch)]
    [InlineData(10, 0, OracleVerdict.Match)]
    [InlineData(11, 59, OracleVerdict.Match)]
    [InlineData(12, 0, OracleVerdict.NoMatch)]
    public void V121_T02_T06_a_deny_window_overrides_an_overlapping_allow_window_in_any_authoring_order(int hour, int minute, OracleVerdict expected)
    {
        var allowFirst = Active(Groups(dayTimeApplication: Windows(Window(Monday, 8, 12), Window(Monday, 9, 10, DayTimeRestrictionEffect.Deny))));
        var denyFirst = Active(Groups(dayTimeApplication: Windows(Window(Monday, 9, 10, DayTimeRestrictionEffect.Deny), Window(Monday, 8, 12))));

        Assert.Equal(expected, ProvisionRuleOracle.Evaluate(allowFirst, At(2027, 4, 5, hour, minute)));
        Assert.Equal(expected, ProvisionRuleOracle.Evaluate(denyFirst, At(2027, 4, 5, hour, minute)));
        Assert.Equal(OracleVerdict.NoMatch, ProvisionRuleOracle.Evaluate(allowFirst, At(2027, 4, 6, hour, minute)));
    }

    [Fact]
    public void V121_T03_T09_deny_windows_without_an_allow_window_leave_every_other_weekly_time_allowed()
    {
        var friday = Active(Groups(dayTimeApplication: Windows(Window(Friday, effect: DayTimeRestrictionEffect.Deny))));
        var twoDenies = Active(Groups(dayTimeApplication: Windows(Window(Friday, effect: DayTimeRestrictionEffect.Deny), Window(Monday, 0, 6, DayTimeRestrictionEffect.Deny))));

        Assert.Equal(OracleVerdict.NoMatch, ProvisionRuleOracle.Evaluate(friday, At(2027, 4, 9, 0, 0)));
        Assert.Equal(OracleVerdict.NoMatch, ProvisionRuleOracle.Evaluate(friday, At(2027, 4, 9, 23, 59)));
        Assert.Equal(OracleVerdict.Match, ProvisionRuleOracle.Evaluate(friday, At(2027, 4, 10, 0, 0)));
        Assert.Equal(OracleVerdict.Match, ProvisionRuleOracle.Evaluate(friday, At(2027, 4, 8, 23, 59)));
        Assert.Equal(OracleVerdict.NoMatch, ProvisionRuleOracle.Evaluate(twoDenies, At(2027, 4, 9, 12, 0)));
        Assert.Equal(OracleVerdict.NoMatch, ProvisionRuleOracle.Evaluate(twoDenies, At(2027, 4, 5, 5, 59)));
        Assert.Equal(OracleVerdict.Match, ProvisionRuleOracle.Evaluate(twoDenies, At(2027, 4, 5, 6, 0)));
        Assert.Equal(OracleVerdict.Match, ProvisionRuleOracle.Evaluate(twoDenies, At(2027, 4, 7, 3, 0)));
    }

    [Theory]
    [InlineData(2027, 4, 10, 21, 59, OracleVerdict.NoMatch)]
    [InlineData(2027, 4, 10, 22, 0, OracleVerdict.Match)]
    [InlineData(2027, 4, 10, 23, 59, OracleVerdict.Match)]
    [InlineData(2027, 4, 11, 0, 0, OracleVerdict.Match)]
    [InlineData(2027, 4, 11, 1, 0, OracleVerdict.Match)]
    [InlineData(2027, 4, 11, 2, 0, OracleVerdict.NoMatch)]
    [InlineData(2027, 4, 11, 22, 30, OracleVerdict.NoMatch)]
    [InlineData(2027, 4, 10, 1, 0, OracleVerdict.NoMatch)]
    public void V121_T04_a_night_span_is_two_windows_with_an_open_end_and_an_open_start(int year, int month, int day, int hour, int minute, OracleVerdict expected)
    {
        var provision = Active(Groups(dayTimeApplication: Windows(Window(Saturday, 22), Window(Sunday, toHour: 2))));

        Assert.Equal(
            new[] { (Saturday, (TimeOnly?)new TimeOnly(22, 0), (TimeOnly?)null), (Sunday, null, new TimeOnly(2, 0)) },
            provision.DayTimeApplication!.Windows.Select(window => (window.DaysOfWeekMask, window.StartLocalTime, window.EndLocalTime)));
        Assert.Equal(expected, ProvisionRuleOracle.Evaluate(provision, At(year, month, day, hour, minute)));
    }

    [Fact]
    public void V121_T07_a_departure_is_matched_on_its_origin_local_date_and_time_not_on_utc()
    {
        var provision = Active(Groups(
            travelDate: Dates([Period(Day(2027, 4, 6), Day(2027, 4, 6))]),
            dayTimeApplication: Windows(Window(Tuesday, 0, 3))));
        var departure = new DateTimeOffset(2027, 4, 5, 21, 30, 0, TimeSpan.Zero);

        Assert.Equal((DayOfWeek.Monday, DayOfWeek.Tuesday), (departure.UtcDateTime.DayOfWeek, TimeZoneInfo.ConvertTime(departure, Tehran).DayOfWeek));
        Assert.Equal(OracleVerdict.Match, ProvisionRuleOracle.Evaluate(provision, new OracleContext { Occurrence = OracleOccurrence.AtInstant(departure, Tehran) }));
        Assert.Equal(OracleVerdict.NoMatch, ProvisionRuleOracle.Evaluate(provision, new OracleContext { Occurrence = OracleOccurrence.AtInstant(departure, TimeZoneInfo.Utc) }));
        Assert.Equal(OracleVerdict.NoMatch, ProvisionRuleOracle.Evaluate(provision, new OracleContext { Occurrence = OracleOccurrence.AtInstant(departure.AddHours(3), Tehran) }));
    }

    [Fact]
    public void V121_T08_a_missing_time_zone_or_an_ambiguous_or_skipped_local_time_is_unsupported_and_never_guessed()
    {
        var timed = Active(Groups(dayTimeApplication: Windows(Window(EveryDay, 1, 4))));
        var dated = Active(Groups(travelDate: Dates([Period(Day(2027, 1, 1), Day(2027, 12, 31))])));
        var ambiguous = new DateTime(2027, 10, 31, 2, 30, 0);
        var skipped = new DateTime(2027, 3, 28, 2, 30, 0);

        Assert.True(Paris.IsAmbiguousTime(ambiguous));
        Assert.True(Paris.IsInvalidTime(skipped));

        foreach (var provision in new[] { timed, dated })
        {
            Assert.Equal(OracleVerdict.UnsupportedContext, ProvisionRuleOracle.Evaluate(provision, new OracleContext { Occurrence = OracleOccurrence.AtLocal(ambiguous, null) }));
            Assert.Equal(OracleVerdict.UnsupportedContext, ProvisionRuleOracle.Evaluate(provision, new OracleContext { Occurrence = OracleOccurrence.AtLocal(ambiguous, Paris) }));
            Assert.Equal(OracleVerdict.UnsupportedContext, ProvisionRuleOracle.Evaluate(provision, new OracleContext { Occurrence = OracleOccurrence.AtLocal(skipped, Paris) }));
            Assert.Equal(OracleVerdict.UnsupportedContext, ProvisionRuleOracle.Evaluate(provision, new OracleContext()));
            Assert.Equal(OracleVerdict.Match, ProvisionRuleOracle.Evaluate(provision, new OracleContext { Occurrence = OracleOccurrence.AtLocal(ambiguous.AddDays(1), Paris) }));
            Assert.Equal(OracleVerdict.Match, ProvisionRuleOracle.Evaluate(provision, new OracleContext { Occurrence = OracleOccurrence.AtInstant(new DateTimeOffset(2027, 10, 31, 0, 30, 0, TimeSpan.Zero), Paris) }));
        }
    }

    [Theory]
    [InlineData(25 * 60, OracleVerdict.Match)]
    [InlineData(24 * 60, OracleVerdict.Match)]
    [InlineData((24 * 60) - 1, OracleVerdict.NoMatch)]
    [InlineData(23 * 60, OracleVerdict.NoMatch)]
    public void V121_A01_an_hour_lead_is_an_elapsed_duration_between_the_sale_and_the_occurrence(int minutesBefore, OracleVerdict expected)
    {
        var provision = Active(Groups(advancePurchase: new ProvisionAdvancePurchaseArgs(24, TimeUnit.Hours, false)));
        var occurrence = new DateTimeOffset(2027, 4, 6, 6, 0, 0, TimeSpan.Zero);
        var context = new OracleContext { Occurrence = OracleOccurrence.AtInstant(occurrence, Tehran), SaleInstant = occurrence.AddMinutes(-minutesBefore) };

        Assert.Equal(expected, ProvisionRuleOracle.Evaluate(provision, context));
        Assert.Equal(OracleVerdict.UnsupportedContext, ProvisionRuleOracle.Evaluate(provision, context with { SaleInstant = null }));
        Assert.Equal(OracleVerdict.UnsupportedContext, ProvisionRuleOracle.Evaluate(provision, context with { Occurrence = null }));
    }

    [Fact]
    public void V121_A02_a_one_day_lead_is_a_local_calendar_day_and_differs_from_twenty_four_elapsed_hours()
    {
        var oneDay = Active(Groups(advancePurchase: new ProvisionAdvancePurchaseArgs(1, TimeUnit.Days, false)));
        var twentyFourHours = Active(Groups(advancePurchase: new ProvisionAdvancePurchaseArgs(24, TimeUnit.Hours, false)));
        var oneMonth = Active(Groups(advancePurchase: new ProvisionAdvancePurchaseArgs(1, TimeUnit.Months, false)));
        var occurrence = OracleOccurrence.AtLocal(new DateTime(2027, 4, 6, 0, 30, 0), Tehran);
        var lateEvening = new OracleContext { Occurrence = occurrence, SaleInstant = new DateTimeOffset(2027, 4, 5, 23, 30, 0, new TimeSpan(3, 30, 0)) };
        var sameMorning = new OracleContext
        {
            Occurrence = OracleOccurrence.AtLocal(new DateTime(2027, 4, 6, 23, 30, 0), Tehran),
            SaleInstant = new DateTimeOffset(2027, 4, 6, 0, 15, 0, new TimeSpan(3, 30, 0))
        };

        Assert.Equal((OracleVerdict.Match, OracleVerdict.NoMatch), (ProvisionRuleOracle.Evaluate(oneDay, lateEvening), ProvisionRuleOracle.Evaluate(twentyFourHours, lateEvening)));
        Assert.Equal((OracleVerdict.NoMatch, OracleVerdict.NoMatch), (ProvisionRuleOracle.Evaluate(oneDay, sameMorning), ProvisionRuleOracle.Evaluate(twentyFourHours, sameMorning)));
        Assert.Equal(OracleVerdict.Match, ProvisionRuleOracle.Evaluate(oneMonth, new OracleContext { Occurrence = occurrence, SaleInstant = new DateTimeOffset(2027, 3, 6, 20, 0, 0, new TimeSpan(3, 30, 0)) }));
        Assert.Equal(OracleVerdict.NoMatch, ProvisionRuleOracle.Evaluate(oneMonth, new OracleContext { Occurrence = occurrence, SaleInstant = new DateTimeOffset(2027, 3, 7, 0, 5, 0, new TimeSpan(3, 30, 0)) }));
    }

    [Fact]
    public void V121_E01_a_passenger_type_allow_list_is_positive_and_needs_the_passenger_type()
    {
        var provision = Active(Groups(Passengers([PassengerTypeCode.ADT])));

        Assert.Equal(OracleVerdict.Match, ProvisionRuleOracle.Evaluate(provision, new OracleContext { PassengerType = PassengerTypeCode.ADT }));
        Assert.Equal(OracleVerdict.NoMatch, ProvisionRuleOracle.Evaluate(provision, new OracleContext { PassengerType = PassengerTypeCode.CHD }));
        Assert.Equal(OracleVerdict.NoMatch, ProvisionRuleOracle.Evaluate(provision, new OracleContext { PassengerType = PassengerTypeCode.INF }));
        Assert.Equal(OracleVerdict.UnsupportedContext, ProvisionRuleOracle.Evaluate(provision, new OracleContext()));
    }

    [Theory]
    [InlineData(1962, 4, 7, 64, OracleVerdict.Match, OracleVerdict.NoMatch)]
    [InlineData(1962, 4, 6, 65, OracleVerdict.NoMatch, OracleVerdict.Match)]
    [InlineData(1962, 4, 5, 65, OracleVerdict.NoMatch, OracleVerdict.Match)]
    [InlineData(2027, 4, 6, 0, OracleVerdict.Match, OracleVerdict.NoMatch)]
    [InlineData(1927, 1, 1, 100, OracleVerdict.NoMatch, OracleVerdict.Match)]
    public void V121_E02_age_is_completed_years_at_the_occurrence_and_selects_exactly_one_band(int year, int month, int day, int age, OracleVerdict younger, OracleVerdict senior)
    {
        var under = Active(Groups(Passengers(bands: [new ProvisionAgeBandArgs(0, 65)])));
        var over = Active(Groups(Passengers(bands: [new ProvisionAgeBandArgs(65, null)])), id: 5002, sequence: 20);
        var both = Active(Groups(Passengers(bands: [new ProvisionAgeBandArgs(0, 65), new ProvisionAgeBandArgs(65, null)])), id: 5003, sequence: 30);
        var context = At(2027, 4, 6) with { DateOfBirth = Day(year, month, day) };

        Assert.Equal(age, ProvisionRuleOracle.CompletedYears(Day(year, month, day), Day(2027, 4, 6)));
        Assert.Equal((younger, senior), (ProvisionRuleOracle.Evaluate(under, context), ProvisionRuleOracle.Evaluate(over, context)));
        Assert.Equal(OracleVerdict.Match, ProvisionRuleOracle.Evaluate(both, context));
    }

    [Fact]
    public void V121_E03_E18_a_populated_restriction_without_its_context_is_unsupported_and_an_empty_restriction_needs_none()
    {
        var birth = Day(1990, 1, 1);
        var aged = Active(Groups(Passengers(bands: [new ProvisionAgeBandArgs(18, null)])));
        (string Name, AncillaryProvision Provision, OracleContext Complete)[] vectors =
        [
            ("age", aged, At(2027, 4, 6) with { DateOfBirth = birth }),
            ("country", Active(Groups(geography: Geography(countries: [90]))), new OracleContext { CoverageCountryId = 90 }),
            ("flight", Active(Groups(flightApplication: Flights(flights: [81234]))), new OracleContext { FlightId = 81234 }),
            ("fare", Active(Groups(fareApplication: Fares(families: [5]))), new OracleContext { FareFamilyId = 5 }),
            ("pos", Active(Groups(salesRestrictions: Sales(pointsOfSale: [501]))), new OracleContext { PointOfSaleId = 501 }),
            ("sale", Active(Groups(salesRestrictions: Sales(Now, Now.AddDays(1), [V122Fixtures.PointOfSale]))), new OracleContext { SaleInstant = Now }),
            ("origin", Active(Groups(geography: Geography(origins: [Thr]))), new OracleContext { OriginAirportId = Thr }),
            ("via", Active(Groups(geography: Geography(vias: [Mhd]))), new OracleContext { ViaAirportIds = [Mhd] }),
            ("place", Active(Groups(geography: Geography(locations: [Location(ServiceLocationType.Airport, Thr)]))), new OracleContext { ServicePlaces = [new OracleServicePlace(ServiceLocationType.Airport, Thr)] })
        ];

        Assert.All(vectors, vector =>
        {
            Assert.Equal(OracleVerdict.Match, ProvisionRuleOracle.Evaluate(vector.Provision, vector.Complete));
            Assert.Equal(OracleVerdict.UnsupportedContext, ProvisionRuleOracle.Evaluate(vector.Provision, new OracleContext { PointOfSaleId = null }));
            Assert.Equal(OracleOutcome.UnsupportedContext, ProvisionRuleOracle.Select([vector.Provision], new OracleContext { PointOfSaleId = null }).Outcome);
        });
        Assert.Equal(OracleVerdict.UnsupportedContext, ProvisionRuleOracle.Evaluate(aged, At(2027, 4, 6)));
        Assert.Equal(OracleVerdict.UnsupportedContext, ProvisionRuleOracle.Evaluate(aged, new OracleContext { DateOfBirth = birth }));
        Assert.Equal(OracleVerdict.Match, ProvisionRuleOracle.Evaluate(Active(Groups()), new OracleContext()));
        Assert.Equal(OracleOutcome.Paid, ProvisionRuleOracle.Select([Active(Groups())], new OracleContext()).Outcome);
    }

    [Fact]
    public void V121_E04_origin_and_destination_are_directional_and_a_route_pair_reverses_only_when_both_directions()
    {
        var airports = Active(Groups(geography: Geography(origins: [Thr], destinations: [Ist])));
        var directional = Active(Groups(geography: Geography(pairs: [Pair(Thr, Ist)])));
        var both = Active(Groups(geography: Geography(pairs: [Pair(Thr, Ist, RoutePairDirection.BothDirections)])));
        var outbound = new OracleContext { OriginAirportId = Thr, DestinationAirportId = Ist };
        var inbound = new OracleContext { OriginAirportId = Ist, DestinationAirportId = Thr };
        var other = new OracleContext { OriginAirportId = Thr, DestinationAirportId = Mhd };

        Assert.Equal((OracleVerdict.Match, OracleVerdict.NoMatch, OracleVerdict.NoMatch), (ProvisionRuleOracle.Evaluate(airports, outbound), ProvisionRuleOracle.Evaluate(airports, inbound), ProvisionRuleOracle.Evaluate(airports, other)));
        Assert.Equal((OracleVerdict.Match, OracleVerdict.NoMatch, OracleVerdict.NoMatch), (ProvisionRuleOracle.Evaluate(directional, outbound), ProvisionRuleOracle.Evaluate(directional, inbound), ProvisionRuleOracle.Evaluate(directional, other)));
        Assert.Equal((OracleVerdict.Match, OracleVerdict.Match, OracleVerdict.NoMatch), (ProvisionRuleOracle.Evaluate(both, outbound), ProvisionRuleOracle.Evaluate(both, inbound), ProvisionRuleOracle.Evaluate(both, other)));
        Assert.Equal(OracleVerdict.UnsupportedContext, ProvisionRuleOracle.Evaluate(directional, new OracleContext { OriginAirportId = Thr }));
    }

    [Fact]
    public void V121_E05_E06_E10_a_service_location_or_coverage_country_needs_no_itinerary_fare_or_flight()
    {
        var lounge = Active(Groups(geography: Geography(locations: [Location(ServiceLocationType.Airport, Thr)])), basis: ServiceDateBasis.ServiceStart);
        var sim = Active(Groups(geography: Geography(countries: [90])), basis: ServiceDateBasis.ServiceStart);
        var hotel = Active(Groups(geography: Geography(locations: [Location(ServiceLocationType.City, 7)])), basis: ServiceDateBasis.ServiceStart);
        var atAirport = new OracleContext { ServicePlaces = [new OracleServicePlace(ServiceLocationType.Airport, Thr), new OracleServicePlace(ServiceLocationType.City, 1)] };

        Assert.Equal(OracleVerdict.Match, ProvisionRuleOracle.Evaluate(lounge, atAirport));
        Assert.Equal(OracleVerdict.NoMatch, ProvisionRuleOracle.Evaluate(lounge, new OracleContext { ServicePlaces = [new OracleServicePlace(ServiceLocationType.Airport, Ist)] }));
        Assert.Equal(OracleVerdict.NoMatch, ProvisionRuleOracle.Evaluate(lounge, new OracleContext { ServicePlaces = [new OracleServicePlace(ServiceLocationType.City, Thr)] }));
        Assert.Equal(OracleVerdict.UnsupportedContext, ProvisionRuleOracle.Evaluate(lounge, new OracleContext { OriginAirportId = Thr, DestinationAirportId = Ist }));
        Assert.Equal(OracleVerdict.Match, ProvisionRuleOracle.Evaluate(sim, new OracleContext { CoverageCountryId = 90 }));
        Assert.Equal(OracleVerdict.NoMatch, ProvisionRuleOracle.Evaluate(sim, new OracleContext { CoverageCountryId = 98 }));
        Assert.Equal(OracleVerdict.UnsupportedContext, ProvisionRuleOracle.Evaluate(sim, new OracleContext { PointOfSaleId = 90, ServicePlaces = [new OracleServicePlace(ServiceLocationType.Country, 90)] }));
        Assert.Null(hotel.FareApplication);
        Assert.Null(hotel.FlightApplication);
        Assert.Equal(OracleVerdict.Match, ProvisionRuleOracle.Evaluate(hotel, new OracleContext { ServicePlaces = [new OracleServicePlace(ServiceLocationType.City, 7)] }));
    }

    [Fact]
    public void V121_E07_E08_E09_every_populated_dimension_must_match_and_alternatives_inside_one_dimension_are_or()
    {
        var sales = Active(Groups(salesRestrictions: Sales(pointsOfSale: [1], customerTypes: [CustomerType.TravelAgency])));
        var fare = Active(Groups(fareApplication: Fares(families: [5], cabins: [1], rbds: [2])));
        var flight = Active(Groups(flightApplication: Flights(operating: [4], numbers: ["A123"])));
        var agency = new OracleContext { PointOfSaleId = 1, CustomerType = CustomerType.TravelAgency };
        var priced = new OracleContext { FareFamilyId = 5, CabinClassId = 1, RbdId = 2 };
        var flown = new OracleContext { OperatingAirlineId = 4, FlightNumber = "a123 " };
        var otherType = Enum.GetValues<CustomerType>().First(type => type != CustomerType.TravelAgency);

        Assert.Equal(OracleVerdict.Match, ProvisionRuleOracle.Evaluate(sales, agency));
        Assert.Equal(OracleVerdict.NoMatch, ProvisionRuleOracle.Evaluate(sales, agency with { PointOfSaleId = 2 }));
        Assert.Equal(OracleVerdict.NoMatch, ProvisionRuleOracle.Evaluate(sales, agency with { PointOfSaleId = 3 }));
        Assert.Equal(OracleVerdict.NoMatch, ProvisionRuleOracle.Evaluate(sales, agency with { CustomerType = otherType }));
        Assert.Equal(OracleVerdict.UnsupportedContext, ProvisionRuleOracle.Evaluate(sales, agency with { CustomerType = null }));
        Assert.Equal(OracleVerdict.Match, ProvisionRuleOracle.Evaluate(fare, priced));
        Assert.Equal(OracleVerdict.NoMatch, ProvisionRuleOracle.Evaluate(fare, priced with { FareFamilyId = 6 }));
        Assert.Equal(OracleVerdict.NoMatch, ProvisionRuleOracle.Evaluate(fare, priced with { CabinClassId = 3 }));
        Assert.Equal(OracleVerdict.NoMatch, ProvisionRuleOracle.Evaluate(fare, priced with { RbdId = 9 }));
        Assert.Equal(OracleVerdict.UnsupportedContext, ProvisionRuleOracle.Evaluate(fare, priced with { RbdId = null }));
        Assert.Equal(OracleVerdict.Match, ProvisionRuleOracle.Evaluate(flight, flown));
        Assert.Equal(OracleVerdict.NoMatch, ProvisionRuleOracle.Evaluate(flight, flown with { OperatingAirlineId = 1 }));
        Assert.Equal(OracleVerdict.NoMatch, ProvisionRuleOracle.Evaluate(flight, flown with { FlightNumber = "A124" }));
        Assert.Equal(OracleVerdict.UnsupportedContext, ProvisionRuleOracle.Evaluate(flight, flown with { FlightNumber = null }));
        Assert.Equal(OracleVerdict.Match, ProvisionRuleOracle.Evaluate(flight, flown with { MarketingAirlineId = 9 }));
    }

    [Fact]
    public void V121_E13_E14_a_lower_sequence_not_available_blocks_its_match_and_never_falls_through_to_a_broader_paid()
    {
        var blockedFlight = Active(Groups(flightApplication: Flights(flights: [123])), CommercialDisposition.NotAvailable, 10, 5001);
        var blockedCustomer = Active(Groups(salesRestrictions: Sales(pointsOfSale: [V122Fixtures.PointOfSale], customers: [9001])), CommercialDisposition.NotAvailable, 20, 5002);
        var paid = Active(Groups(), CommercialDisposition.Paid, 100, 5003);
        AncillaryProvision[] provisions = [paid, blockedCustomer, blockedFlight];

        Assert.Equal(new OracleDecision(OracleOutcome.NotAvailable, 5001), ProvisionRuleOracle.Select(provisions, new OracleContext { FlightId = 123, CustomerId = 1 }));
        Assert.Equal(new OracleDecision(OracleOutcome.Paid, 5003), ProvisionRuleOracle.Select(provisions, new OracleContext { FlightId = 124, CustomerId = 1 }));
        Assert.Equal(new OracleDecision(OracleOutcome.NotAvailable, 5002), ProvisionRuleOracle.Select(provisions, new OracleContext { FlightId = 124, CustomerId = 9001 }));
        Assert.Equal(new OracleDecision(OracleOutcome.NotAvailable, 5001), ProvisionRuleOracle.Select(provisions, new OracleContext { FlightId = 123, CustomerId = 9001 }));
        Assert.Equal(new OracleDecision(OracleOutcome.UnsupportedContext, 5001), ProvisionRuleOracle.Select(provisions, new OracleContext { CustomerId = 1 }));
        Assert.Equal(new OracleDecision(OracleOutcome.UnsupportedContext, 5002), ProvisionRuleOracle.Select(provisions, new OracleContext { FlightId = 124 }));
    }

    [Fact]
    public void V121_E15_E16_a_matching_not_available_is_an_explicit_denial_and_nothing_matching_is_no_match()
    {
        var denied = Active(Groups(), CommercialDisposition.NotAvailable, 10, 5001);
        var adultsOnly = Active(Groups(Passengers([PassengerTypeCode.ADT])), CommercialDisposition.Paid, 10, 5002);
        var free = Active(Groups(Passengers([PassengerTypeCode.INF])), CommercialDisposition.Free, 20, 5003);
        var draft = Provision(Groups(), id: 5004, sequence: 1);
        var retired = Active(Groups(), CommercialDisposition.Paid, 2, 5005);

        retired.Supersede(Now.AddMinutes(1));

        Assert.Equal(new OracleDecision(OracleOutcome.NotAvailable, 5001), ProvisionRuleOracle.Select([denied], new OracleContext()));
        Assert.Equal(new OracleDecision(OracleOutcome.NoMatch, null), ProvisionRuleOracle.Select([adultsOnly, free, draft, retired], new OracleContext { PassengerType = PassengerTypeCode.CHD }));
        Assert.Equal(new OracleDecision(OracleOutcome.Free, 5003), ProvisionRuleOracle.Select([adultsOnly, free, draft, retired], new OracleContext { PassengerType = PassengerTypeCode.INF }));
        Assert.Equal(new OracleDecision(OracleOutcome.Paid, 5002), ProvisionRuleOracle.Select([adultsOnly, free, draft, retired], new OracleContext { PassengerType = PassengerTypeCode.ADT }));
        Assert.Equal(new OracleDecision(OracleOutcome.NoMatch, null), ProvisionRuleOracle.Select([draft, retired], new OracleContext()));
        Assert.Equal(new OracleDecision(OracleOutcome.NoMatch, null), ProvisionRuleOracle.Select([], new OracleContext()));
    }

    [Fact]
    public void V121_P01_P10_age_rates_are_alternatives_never_summed_and_a_unit_total_is_one_base_plus_its_components()
    {
        var insurance = Pricing(
            PricingUnit.PerPassenger,
            [Base(20m, PassengerTypeCode.ADT, 0, 65), Base(40m, PassengerTypeCode.ADT, 65, null)]);
        var withTax = Pricing(
            PricingUnit.PerPassenger,
            [Base(25m), Component(AncillaryPriceLineCategory.Tax, "YQ", 2.5m, countryId: 98), Component(AncillaryPriceLineCategory.Fee, "SVC", 1m)],
            id: 7002);

        Assert.Equal(20m, ProvisionRuleOracle.UnitTotal(insurance, PassengerTypeCode.ADT, 64));
        Assert.Equal(40m, ProvisionRuleOracle.UnitTotal(insurance, PassengerTypeCode.ADT, 65));
        Assert.Equal(20m, ProvisionRuleOracle.UnitTotal(insurance, PassengerTypeCode.ADT, 0));
        Assert.Null(ProvisionRuleOracle.UnitTotal(insurance, PassengerTypeCode.CHD, 10));
        Assert.Null(ProvisionRuleOracle.UnitTotal(insurance, PassengerTypeCode.ADT, null));
        Assert.DoesNotContain(60m, Enumerable.Range(0, 120).Select(age => ProvisionRuleOracle.UnitTotal(insurance, PassengerTypeCode.ADT, age)));
        Assert.Equal(28.5m, ProvisionRuleOracle.UnitTotal(withTax, PassengerTypeCode.CHD, null));
        Assert.Equal(
            new[] { ("SVC", (int?)null), ("YQ", (int?)98) },
            ProvisionRuleOracle.SelectedRates(withTax, null, null).Single().Components.Select(component => (component.Code!, component.CountryId)).OrderBy(component => component.Item1));
    }
}
