using System.Reflection;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Entities;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.ValueObjects;
using AeroTech.Ancillary.Domain.ConformanceTests.Fixtures;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;
using AeroTech.Messages.Core.Enums;
using Xunit;
using static AeroTech.Ancillary.Domain.ConformanceTests.Fixtures.P1Fixtures;

namespace AeroTech.Ancillary.Domain.ConformanceTests;

public class P1ProvisionCriteriaConformanceTests
{
    [Fact]
    public void P1_REQ_the_provision_carries_the_v11_typed_blocks_next_to_the_baseline_fields()
    {
        Assert.Equal(
            new[]
            {
                "ActivatedAt", "AdvancePurchase", "Application", "Availability", "CoverageScope", "CreatedAt", "Fare", "Fee",
                "Fulfillment", "Outcome", "Passenger", "PriceLines", "Quantity", "RetiredAt", "RoutePairs", "Sales",
                "SalesDiscontinueAt", "SalesEffectiveFrom", "Sequence", "ServiceDefinitionId", "Settlement", "Status",
                "SuspendedAt", "Travel"
            },
            PropertiesOf<AncillaryProvision>());
        Assert.Equal(new[] { "PassengerTypeCodes" }, PropertiesOf<PassengerCriteria>());
        Assert.Equal(new[] { "CustomerIds", "CustomerTypes", "PointOfSaleIds" }, PropertiesOf<SalesCriteria>());
        Assert.Equal(
            new[]
            {
                "AircraftIds", "DaysOfWeek", "DestinationAirportIds", "FlightIds", "FlightNumbers", "MarketingAirlineIds",
                "OperatingAirlineIds", "OriginAirportIds", "TimeFrom", "TimeTo", "TravelFrom", "TravelTo", "ViaAirportIds"
            },
            PropertiesOf<TravelCriteria>());
        Assert.Equal(
            new[] { "AncillaryProvisionId", "DestinationAirportId", "Direction", "OriginAirportId" },
            PropertiesOf<ProvisionRoutePair>());
        Assert.Equal(
            new[] { "AirFareIds", "AirFareTypes", "CabinClassIds", "FareBasisCodes", "FareFamilyIds", "RbdIds" },
            PropertiesOf<FareCriteria>());
        Assert.Equal(new[] { "Period", "Unit" }, PropertiesOf<AdvancePurchaseCriteria>());
        Assert.Equal(new[] { "Baggage", "Seat", "Type" }, PropertiesOf<ProvisionApplication>());
        Assert.Equal(new[] { "FulfillmentProviderKey" }, PropertiesOf<FulfillmentDefinition>());
    }

    [Fact]
    public void P1_D03_no_adult_child_or_infant_price_member_exists()
    {
        var members = PropertiesOf<AncillaryProvision>()
            .Concat(PropertiesOf<ProvisionPriceLine>())
            .Concat(PropertiesOf<FeeDefinition>())
            .ToList();

        Assert.DoesNotContain(members, name => name.Contains("Adult", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(members, name => name.Contains("Child", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(members, name => name.Contains("Infant", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void P1_D04_E04_no_unauthorized_criterion_exists_in_the_provision_model()
    {
        string[] forbidden =
        [
            "MinAge", "MaxAge", "AgeYears", "DateOfBirth", "Occurrence", "FrequentFlyer", "Keyword", "Score", "Pcc",
            "TicketDesignator", "AccountCode", "TourCode", "Tariff", "RuleNumber", "Channel", "Office", "Wildcard",
            "FareFamilyName", "Metadata", "Attributes"
        ];

        var members = typeof(AncillaryProvision).Assembly.GetTypes()
            .Where(type => type.Namespace is not null
                           && type.Namespace.StartsWith(typeof(AncillaryProvision).Namespace!, StringComparison.Ordinal))
            .SelectMany(type => type.GetProperties().Select(property => $"{type.Name}.{property.Name}"))
            .ToList();

        Assert.NotEmpty(members);

        foreach (var name in forbidden)
            Assert.DoesNotContain(members, member => member.Contains(name, StringComparison.Ordinal));
    }

    [Fact]
    public void P1_D01_D02_separate_provisions_of_one_definition_carry_the_passenger_type_prices()
    {
        var adult = Provision(1, 10, 25m, passenger: Passengers(PassengerTypeCode.ADT));
        var child = Provision(2, 20, 15m, passenger: Passengers(PassengerTypeCode.CHD));
        var infantFree = Provision(3, 30, disposition: CommercialDisposition.Free, passenger: Passengers(PassengerTypeCode.INF));
        var infantBlocked = Provision(4, 40, disposition: CommercialDisposition.NotAvailable, passenger: Passengers(PassengerTypeCode.INF));

        Assert.All(new[] { adult, child, infantFree, infantBlocked }, provision => Assert.Equal(1001, provision.ServiceDefinitionId));
        Assert.Equal(new[] { PassengerTypeCode.ADT }, adult.Passenger.PassengerTypeCodes);
        Assert.Equal(25m, adult.PriceLines.Single().UnitAmount);
        Assert.Equal(new[] { PassengerTypeCode.CHD }, child.Passenger.PassengerTypeCodes);
        Assert.Equal(15m, child.PriceLines.Single().UnitAmount);
        Assert.Equal(Eur, adult.Fee!.CurrencyId);
        Assert.Equal(CommercialDisposition.Free, infantFree.Outcome.Disposition);
        Assert.Empty(infantFree.PriceLines);
        Assert.Equal(CommercialDisposition.NotAvailable, infantBlocked.Outcome.Disposition);
        Assert.Null(infantBlocked.Fee);
    }

    [Fact]
    public void P1_E01_E02_E03_sales_lists_hold_their_values_and_reject_malformed_ones()
    {
        var sales = Sales([501, 502], [9001], [CustomerType.TravelAgency, CustomerType.Organization]);

        Assert.Equal(new long[] { 501, 502 }, sales.PointOfSaleIds);
        Assert.Equal(new long[] { 9001 }, sales.CustomerIds);
        Assert.Equal(new[] { CustomerType.TravelAgency, CustomerType.Organization }, sales.CustomerTypes);
        Assert.Empty(Sales().PointOfSaleIds);

        BusinessAssert.Throws(16302, 422, () => Sales(pointOfSaleIds: [0]));
        BusinessAssert.Throws(16302, 422, () => Sales(pointOfSaleIds: [501, 501]));
        BusinessAssert.Throws(16302, 422, () => Sales(customerIds: [-7]));
        BusinessAssert.Throws(16302, 422, () => Sales(customerTypes: [(CustomerType)99]));
        BusinessAssert.Throws(16302, 422, () => Sales(customerTypes: [CustomerType.Individual, CustomerType.Individual]));
        BusinessAssert.Throws(16302, 422, () => Passengers(PassengerTypeCode.ADT, PassengerTypeCode.ADT));
        BusinessAssert.Throws(16302, 422, () => Passengers((PassengerTypeCode)(-1)));
    }

    [Fact]
    public void P1_F01_F03_F06_F08_F09_travel_id_lists_hold_their_values_and_reject_malformed_ones()
    {
        var travel = Travel(
            originAirportIds: [Thr, Mhd],
            destinationAirportIds: [Ist],
            viaAirportIds: [Mhd],
            marketingAirlineIds: [1, 2],
            operatingAirlineIds: [3],
            flightIds: [81234, 81240],
            aircraftIds: [320, 321]);

        Assert.Equal(new[] { Thr, Mhd }, travel.OriginAirportIds);
        Assert.Equal(new[] { Ist }, travel.DestinationAirportIds);
        Assert.Equal(new[] { Mhd }, travel.ViaAirportIds);
        Assert.Equal(new[] { 1, 2 }, travel.MarketingAirlineIds);
        Assert.Equal(new[] { 3 }, travel.OperatingAirlineIds);
        Assert.Equal(new long[] { 81234, 81240 }, travel.FlightIds);
        Assert.Equal(new[] { 320, 321 }, travel.AircraftIds);

        BusinessAssert.Throws(16302, 422, () => Travel(originAirportIds: [Thr, Thr]));
        BusinessAssert.Throws(16302, 422, () => Travel(destinationAirportIds: [0]));
        BusinessAssert.Throws(16302, 422, () => Travel(viaAirportIds: [-1]));
        BusinessAssert.Throws(16302, 422, () => Travel(marketingAirlineIds: [1, 1]));
        BusinessAssert.Throws(16302, 422, () => Travel(operatingAirlineIds: [0]));
        BusinessAssert.Throws(16302, 422, () => Travel(flightIds: [0]));
        BusinessAssert.Throws(16302, 422, () => Travel(aircraftIds: [320, 320]));
    }

    [Fact]
    public void P1_F02_route_pairs_are_directional_or_both_directions_between_two_different_airports()
    {
        var provision = Provision(routePairs: [Pair(Thr, Ist), Pair(Mhd, Ist, RoutePairDirection.BothDirections)]);

        Assert.Equal(
            new[] { (Thr, Ist, RoutePairDirection.Directional), (Mhd, Ist, RoutePairDirection.BothDirections) },
            provision.RoutePairs.Select(pair => (pair.OriginAirportId, pair.DestinationAirportId, pair.Direction)));
        Assert.All(provision.RoutePairs, pair => Assert.Equal(provision.Id, pair.AncillaryProvisionId));
        Assert.Equal(new[] { "BothDirections", "Directional" }, Enum.GetNames<RoutePairDirection>().OrderBy(name => name, StringComparer.Ordinal));

        BusinessAssert.Throws(16302, 422, () => Provision(routePairs: [Pair(Thr, Thr)]));
        BusinessAssert.Throws(16302, 422, () => Provision(routePairs: [Pair(0, Ist)]));
        BusinessAssert.Throws(16302, 422, () => Provision(routePairs: [Pair(Thr, -6)]));
        BusinessAssert.Throws(16302, 422, () => Provision(routePairs: [Pair(Thr, Ist), Pair(Thr, Ist, RoutePairDirection.BothDirections)]));
        BusinessAssert.Throws(16302, 422, () => Provision(routePairs: [Pair(Thr, Ist, (RoutePairDirection)9)]));
    }

    [Fact]
    public void P1_F04_travel_dates_hold_their_values_and_a_reversed_range_is_refused()
    {
        var window = Travel(travelFrom: new DateOnly(2026, 12, 20), travelTo: new DateOnly(2026, 12, 31));
        var sameDay = Travel(travelFrom: new DateOnly(2026, 12, 20), travelTo: new DateOnly(2026, 12, 20));
        var openEnded = Travel(travelFrom: new DateOnly(2026, 12, 20));

        Assert.Equal((new DateOnly(2026, 12, 20), new DateOnly(2026, 12, 31)), (window.TravelFrom!.Value, window.TravelTo!.Value));
        Assert.Equal(sameDay.TravelFrom, sameDay.TravelTo);
        Assert.Null(openEnded.TravelTo);

        BusinessAssert.Throws(16302, 422, () => Travel(travelFrom: new DateOnly(2026, 12, 31), travelTo: new DateOnly(2026, 12, 20)));
    }

    [Fact]
    public void P1_F05_days_of_week_and_a_time_window_hold_their_values_and_the_window_needs_both_bounds()
    {
        var travel = Travel(
            daysOfWeek: [DayOfWeek.Monday, DayOfWeek.Friday],
            timeFrom: new TimeOnly(22, 0),
            timeTo: new TimeOnly(2, 0));

        Assert.Equal(new[] { DayOfWeek.Monday, DayOfWeek.Friday }, travel.DaysOfWeek);
        Assert.Equal((new TimeOnly(22, 0), new TimeOnly(2, 0)), (travel.TimeFrom!.Value, travel.TimeTo!.Value));

        BusinessAssert.Throws(16302, 422, () => Travel(timeFrom: new TimeOnly(8, 0)));
        BusinessAssert.Throws(16302, 422, () => Travel(timeTo: new TimeOnly(12, 0)));
        BusinessAssert.Throws(16302, 422, () => Travel(daysOfWeek: [DayOfWeek.Friday, DayOfWeek.Friday]));
        BusinessAssert.Throws(16302, 422, () => Travel(daysOfWeek: [(DayOfWeek)9]));
    }

    [Fact]
    public void P1_F07_flight_numbers_are_stored_trimmed_and_upper_cased()
    {
        Assert.Equal(new[] { "W5112", "W5116" }, Travel(flightNumbers: [" w5112", "W5116 "]).FlightNumbers);

        BusinessAssert.Throws(16302, 422, () => Travel(flightNumbers: ["W5 112"]));
        BusinessAssert.Throws(16302, 422, () => Travel(flightNumbers: [""]));
        BusinessAssert.Throws(16302, 422, () => Travel(flightNumbers: [new string('9', 17)]));
        BusinessAssert.Throws(16302, 422, () => Travel(flightNumbers: ["W5112", "w5112"]));
    }

    [Fact]
    public void P1_G01_G06_every_fare_dimension_holds_its_values_and_they_coexist_in_one_provision()
    {
        var provision = Provision(fare: Fare(
            airFareIds: [7001, 7002],
            airFareTypes: [AirFareType.Public, AirFareType.Private],
            fareFamilyIds: [5, 6],
            fareBasisCodes: [" y26lt", "QOW"],
            cabinClassIds: [2],
            rbdIds: [41, 42]));

        Assert.Equal(new long[] { 7001, 7002 }, provision.Fare.AirFareIds);
        Assert.Equal(new[] { AirFareType.Public, AirFareType.Private }, provision.Fare.AirFareTypes);
        Assert.Equal(new long[] { 5, 6 }, provision.Fare.FareFamilyIds);
        Assert.Equal(new[] { "Y26LT", "QOW" }, provision.Fare.FareBasisCodes);
        Assert.Equal(new[] { 2 }, provision.Fare.CabinClassIds);
        Assert.Equal(new long[] { 41, 42 }, provision.Fare.RbdIds);

        BusinessAssert.Throws(16302, 422, () => Fare(airFareIds: [0]));
        BusinessAssert.Throws(16302, 422, () => Fare(airFareTypes: [(AirFareType)99]));
        BusinessAssert.Throws(16302, 422, () => Fare(fareFamilyIds: [5, 5]));
        BusinessAssert.Throws(16302, 422, () => Fare(fareBasisCodes: ["Y26LT", "y26lt"]));
        BusinessAssert.Throws(16302, 422, () => Fare(fareBasisCodes: [new string('Y', 65)]));
        BusinessAssert.Throws(16302, 422, () => Fare(cabinClassIds: [-2]));
        BusinessAssert.Throws(16302, 422, () => Fare(rbdIds: [41, 41]));
    }

    [Fact]
    public void P1_G07_a_fare_family_is_a_numeric_identity()
    {
        var elementType = typeof(FareCriteria).GetProperty(nameof(FareCriteria.FareFamilyIds))!.PropertyType.GetGenericArguments().Single();

        Assert.Equal(typeof(long), elementType);
        Assert.DoesNotContain(PropertiesOf<FareCriteria>(), name => name.Contains("Name", StringComparison.Ordinal));
    }

    [Fact]
    public void P1_H01_an_advance_purchase_period_must_be_positive()
    {
        var threeDays = AdvancePurchaseCriteria.Create(3, TimeUnit.Days);

        Assert.Equal((3, TimeUnit.Days), (threeDays.Period, threeDays.Unit));

        BusinessAssert.Throws(16302, 422, () => AdvancePurchaseCriteria.Create(0, TimeUnit.Days));
        BusinessAssert.Throws(16302, 422, () => AdvancePurchaseCriteria.Create(-1, TimeUnit.Hours));
        BusinessAssert.Throws(16302, 422, () => AdvancePurchaseCriteria.Create(1, (TimeUnit)99));
    }

    [Fact]
    public void P1_H02_the_advance_purchase_unit_is_the_canonical_air_price_time_unit()
    {
        Assert.Equal(typeof(TimeUnit), typeof(AdvancePurchaseCriteria).GetProperty(nameof(AdvancePurchaseCriteria.Unit))!.PropertyType);
        Assert.Equal("AeroTech.Messages.AirPrice.Enums", typeof(TimeUnit).Namespace);
        Assert.DoesNotContain(
            typeof(ProvisionStatus).Assembly.GetTypes(),
            type => type.IsEnum
                    && type.Namespace == typeof(ProvisionStatus).Namespace
                    && (type.Name.Contains("TimeUnit", StringComparison.Ordinal)
                        || type.Name.Contains("AdvancePurchase", StringComparison.Ordinal)
                        || type.Name.Contains("WeightUnit", StringComparison.Ordinal)));

        foreach (var unit in Enum.GetValues<TimeUnit>())
            Assert.Equal(unit, AdvancePurchaseCriteria.Create(1, unit).Unit);
    }

    [Fact]
    public void P1_H03_the_advance_purchase_block_is_data_without_eligibility_arithmetic()
    {
        var declaredMethods = typeof(AdvancePurchaseCriteria)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .ToArray();

        Assert.Equal(new[] { "Create" }, declaredMethods);
    }

    [Fact]
    public void P1_I01_a_paid_provision_requires_a_fee_and_at_least_one_price_line()
    {
        BusinessAssert.Throws(16302, 422, () => Provision(priceLines: []));
        BusinessAssert.Throws(16302, 422, () => Provision(
            priceLines: [new ProvisionPriceLineArgs(AncillaryPriceLineCategory.Ancillary, null, null, 0m)]));
        BusinessAssert.Throws(16302, 422, () => Provision(
            priceLines: [new ProvisionPriceLineArgs(AncillaryPriceLineCategory.Ancillary, null, null, -5m)]));
        BusinessAssert.Throws(16302, 422, () => Provision(
            priceLines: [new ProvisionPriceLineArgs(AncillaryPriceLineCategory.Ancillary, null, null, 10.005m)]));

        var paid = Provision(amount: 25m);

        Assert.Equal(Eur, paid.Fee!.CurrencyId);
        Assert.Equal(25m, Assert.Single(paid.PriceLines).UnitAmount);
    }

    [Theory]
    [InlineData(CommercialDisposition.Free)]
    [InlineData(CommercialDisposition.NotAvailable)]
    public void P1_I02_a_free_or_not_available_provision_cannot_carry_a_payable_amount(CommercialDisposition disposition)
    {
        var provision = Provision(disposition: disposition);

        Assert.Null(provision.Fee);
        Assert.Empty(provision.PriceLines);

        var template = Provision();

        BusinessAssert.Throws(16302, 422, () => AncillaryProvision.Define(
            502,
            1001,
            10,
            null,
            null,
            ServiceCoverageScope.Sector,
            Passengers(),
            Sales(),
            Travel(),
            [],
            Fare(),
            null,
            QuantityRule.Create(AncillaryQuantityUnit.Each, 1, 1),
            Standard(),
            CommercialOutcome.Create(disposition, false, false),
            FeeDefinition.Create(Eur, FeeApplicationUnit.Item),
            [new ProvisionPriceLineArgs(AncillaryPriceLineCategory.Ancillary, null, null, 25m)],
            SettlementDefinition.Create(ReissueRefundPolicy.NonRefundable, null, false, false),
            AvailabilityDefinition.Create(false),
            FulfillmentDefinition.Create(template.Fulfillment.FulfillmentProviderKey),
            new Fakes.SequentialIdGenerator(),
            Now));
    }

    [Fact]
    public void P1_I04_a_price_line_can_carry_code_country_and_station_evidence()
    {
        var provision = Provision(priceLines:
        [
            new ProvisionPriceLineArgs(AncillaryPriceLineCategory.Ancillary, null, "Extra bag", 30m),
            new ProvisionPriceLineArgs(AncillaryPriceLineCategory.Tax, "VAT", "Value added tax", 2.70m, 98, Thr),
            new ProvisionPriceLineArgs(AncillaryPriceLineCategory.Fee, null, "Handling", 1.05m)
        ]);

        var tax = provision.PriceLines.Single(line => line.Category == AncillaryPriceLineCategory.Tax);
        var ancillary = provision.PriceLines.Single(line => line.Category == AncillaryPriceLineCategory.Ancillary);

        Assert.Equal(("VAT", 98, Thr, 2.70m), (tax.Code, tax.CountryId, tax.StationAirportId, tax.UnitAmount));
        Assert.Null(ancillary.CountryId);
        Assert.Null(ancillary.StationAirportId);
        Assert.Equal(
            new[] { "AncillaryProvisionId", "Category", "Code", "CountryId", "Name", "StationAirportId", "UnitAmount" },
            PropertiesOf<ProvisionPriceLine>());

        BusinessAssert.Throws(16302, 422, () => Provision(
            priceLines: [new ProvisionPriceLineArgs(AncillaryPriceLineCategory.Tax, "VAT", null, 2.70m, 0)]));
        BusinessAssert.Throws(16302, 422, () => Provision(
            priceLines: [new ProvisionPriceLineArgs(AncillaryPriceLineCategory.Tax, "VAT", null, 2.70m, null, -4)]));
    }

    [Fact]
    public void P1_I05_the_price_model_has_one_filed_currency_and_no_conversion_member()
    {
        string[] conversionTerms = ["Rate", "Roe", "Exchange", "Convert", "SellingCurrency"];

        var members = PropertiesOf<FeeDefinition>().Concat(PropertiesOf<ProvisionPriceLine>()).Concat(PropertiesOf<AncillaryProvision>()).ToList();

        Assert.Equal(new[] { "ApplicationUnit", "CurrencyId" }, PropertiesOf<FeeDefinition>());

        foreach (var term in conversionTerms)
            Assert.DoesNotContain(members, member => member.Contains(term, StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(FeeApplicationUnit.PerOneKilogramOver)]
    [InlineData(FeeApplicationUnit.PerFiveKilogramsOver)]
    [InlineData(FeeApplicationUnit.HalfPercentOfFarePerKilogram)]
    [InlineData(FeeApplicationUnit.OnePercentOfFarePerKilogram)]
    [InlineData(FeeApplicationUnit.OneAndHalfPercentOfFarePerKilogram)]
    public void P1_I06_a_fee_unit_without_a_frozen_formula_can_be_authored_but_not_activated(FeeApplicationUnit unit)
    {
        var draft = Provision(feeApplicationUnit: unit);

        BusinessAssert.Throws(16305, 422, () => draft.Activate(Now));
        Assert.Equal(ProvisionStatus.Draft, draft.Status);
    }

    [Fact]
    public void P1_J01_a_standard_application_has_no_family_specific_child_object()
    {
        var provision = Provision();

        Assert.Equal(ProvisionApplicationType.Standard, provision.Application.Type);
        Assert.Null(provision.Application.Baggage);
        Assert.Null(provision.Application.Seat);
        Assert.Equal(new[] { "Baggage", "Seat", "Standard" }, Enum.GetNames<ProvisionApplicationType>().OrderBy(name => name, StringComparer.Ordinal));

        BusinessAssert.Throws(16302, 422, () => ProvisionApplication.Create(ProvisionApplicationType.Standard, Baggage().Baggage, null));
        BusinessAssert.Throws(16302, 422, () => ProvisionApplication.Create(
            ProvisionApplicationType.Standard, null, SeatApplication.Create(null, ["W"])));
        BusinessAssert.Throws(16302, 422, () => ProvisionApplication.Create(ProvisionApplicationType.Baggage, null, null));
        BusinessAssert.Throws(16302, 422, () => ProvisionApplication.Create(ProvisionApplicationType.Seat, null, null));
        BusinessAssert.Throws(16302, 422, () => ProvisionApplication.Create((ProvisionApplicationType)9, null, null));
    }

    [Fact]
    public void P1_J02_the_baggage_application_holds_exactly_the_typed_descriptors()
    {
        Assert.Equal(
            new[]
            {
                "FirstExcessPiece", "FreePieces", "LastExcessPiece", "PurchaseApplication", "RuleDeference", "TravelApplication",
                "Weight", "WeightUnit"
            },
            PropertiesOf<BaggageApplication>());
        Assert.Equal(typeof(WeightUnit), typeof(BaggageApplication).GetProperty(nameof(BaggageApplication.WeightUnit))!.PropertyType);
        Assert.Equal("AeroTech.Messages.AirPrice.Enums", typeof(WeightUnit).Namespace);
        Assert.Equal(
            new[] { "AllSectors", "AnySectorOnJourney", "AtLeastOneSector", "MostSignificantSector" },
            Enum.GetNames<BaggageTravelApplication>().OrderBy(name => name, StringComparer.Ordinal));
        Assert.Equal(
            new[] { "CheckIn", "Prepaid", "PrepaidAndCheckIn" },
            Enum.GetNames<BaggagePurchaseApplication>().OrderBy(name => name, StringComparer.Ordinal));
        Assert.Equal(
            new[] { "MarketingCarrier", "OperatingCarrier" },
            Enum.GetNames<BaggageRuleDeference>().OrderBy(name => name, StringComparer.Ordinal));

        var baggage = BaggageApplication.Create(
            2, 1, 3, 23.50m, WeightUnit.Lbs, BaggageTravelApplication.MostSignificantSector,
            BaggagePurchaseApplication.PrepaidAndCheckIn, BaggageRuleDeference.OperatingCarrier);

        Assert.Equal((2, 1, 3, 23.50m), (baggage.FreePieces, baggage.FirstExcessPiece, baggage.LastExcessPiece, baggage.Weight));
        Assert.Equal(WeightUnit.Lbs, baggage.WeightUnit);
        Assert.Equal(BaggageTravelApplication.MostSignificantSector, baggage.TravelApplication);
        Assert.Equal(BaggagePurchaseApplication.PrepaidAndCheckIn, baggage.PurchaseApplication);
        Assert.Equal(BaggageRuleDeference.OperatingCarrier, baggage.RuleDeference);

        BaggageApplication Create(
            int? freePieces = null,
            int? firstExcessPiece = null,
            int? lastExcessPiece = null,
            decimal? weight = null,
            WeightUnit weightUnit = WeightUnit.Kg,
            BaggageTravelApplication? travelApplication = null,
            BaggagePurchaseApplication purchaseApplication = BaggagePurchaseApplication.Prepaid,
            BaggageRuleDeference? ruleDeference = null)
            => BaggageApplication.Create(
                freePieces, firstExcessPiece, lastExcessPiece, weight, weightUnit, travelApplication, purchaseApplication, ruleDeference);

        BusinessAssert.Throws(16302, 422, () => Create(freePieces: -1));
        BusinessAssert.Throws(16302, 422, () => Create(firstExcessPiece: 0));
        BusinessAssert.Throws(16302, 422, () => Create(firstExcessPiece: 3, lastExcessPiece: 1));
        BusinessAssert.Throws(16302, 422, () => Create(weight: 0m));
        BusinessAssert.Throws(16302, 422, () => Create(weight: 23.505m));
        BusinessAssert.Throws(16302, 422, () => Create(weight: 10000000m));
        BusinessAssert.Throws(16302, 422, () => Create(weightUnit: (WeightUnit)9));
        BusinessAssert.Throws(16302, 422, () => Create(travelApplication: (BaggageTravelApplication)9));
        BusinessAssert.Throws(16302, 422, () => Create(purchaseApplication: (BaggagePurchaseApplication)9));
        BusinessAssert.Throws(16302, 422, () => Create(ruleDeference: (BaggageRuleDeference)9));
        Assert.Null(Create().Weight);
    }

    [Fact]
    public void P1_J02_every_typed_baggage_descriptor_is_stored_and_none_blocks_activation()
    {
        foreach (var travelApplication in Enum.GetValues<BaggageTravelApplication>())
        foreach (var ruleDeference in new BaggageRuleDeference?[] { null, BaggageRuleDeference.MarketingCarrier, BaggageRuleDeference.OperatingCarrier })
        {
            var provision = Provision(application: ProvisionApplication.Create(
                ProvisionApplicationType.Baggage,
                BaggageApplication.Create(
                    null, 1, 1, 20m, WeightUnit.Kg, travelApplication, BaggagePurchaseApplication.CheckIn, ruleDeference),
                null));

            provision.Activate(Now);

            Assert.Equal(ProvisionStatus.Active, provision.Status);
            Assert.Equal(travelApplication, provision.Application.Baggage!.TravelApplication);
            Assert.Equal(ruleDeference, provision.Application.Baggage.RuleDeference);
        }
    }

    [Fact]
    public void P1_J03_a_seat_application_requires_seat_numbers_or_characteristic_codes()
    {
        BusinessAssert.Throws(16302, 422, () => SeatApplication.Create(null, null));
        BusinessAssert.Throws(16302, 422, () => SeatApplication.Create([], []));
        BusinessAssert.Throws(16302, 422, () => SeatApplication.Create(["12A", "12a"], null));
        BusinessAssert.Throws(16302, 422, () => SeatApplication.Create([new string('1', 17)], null));
        BusinessAssert.Throws(16302, 422, () => SeatApplication.Create(null, [new string('W', 26)]));
        BusinessAssert.Throws(16302, 422, () => SeatApplication.Create(null, ["W W"]));

        var byCharacteristic = Provision(application: Seat(null, ["W", "1A_AQC_PREMIUM_SEAT"]));

        Assert.Equal(new[] { "W", "1A_AQC_PREMIUM_SEAT" }, byCharacteristic.Application.Seat!.SeatCharacteristicCodes);
        Assert.Empty(byCharacteristic.Application.Seat.SeatNumbers);
    }

    [Fact]
    public void P1_J04_exact_seat_numbers_require_aircraft_ids()
    {
        BusinessAssert.Throws(16302, 422, () => Provision(application: Seat([" 12a", "12B"], null)));
        BusinessAssert.Throws(16302, 422, () => Provision(application: Seat(["12A"], ["W"])));

        var exact = Provision(application: Seat([" 12a", "12B"], null), travel: Travel(aircraftIds: [320]));

        Assert.Equal(new[] { "12A", "12B" }, exact.Application.Seat!.SeatNumbers);
        Assert.Equal(new[] { 320 }, exact.Travel.AircraftIds);
    }

    [Fact]
    public void P1_J05_the_seat_application_stores_no_live_seat_state()
    {
        string[] liveState = ["Occupied", "Blocked", "Available", "Hold", "Status", "Inventory"];

        Assert.Equal(new[] { "SeatCharacteristicCodes", "SeatNumbers" }, PropertiesOf<SeatApplication>());

        foreach (var term in liveState)
            Assert.DoesNotContain(PropertiesOf<SeatApplication>(), name => name.Contains(term, StringComparison.Ordinal));
    }

    [Fact]
    public void P1_K05_a_draft_is_replaced_as_a_whole_by_a_change()
    {
        var provision = Provision(
            passenger: Passengers(PassengerTypeCode.ADT),
            routePairs: [Pair(Thr, Ist)],
            advancePurchase: AdvancePurchaseCriteria.Create(3, TimeUnit.Days),
            application: Baggage());
        var firstLineId = provision.PriceLines.Single().Id;

        provision.Change(
            20,
            new DateTimeOffset(2026, 11, 1, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 12, 1, 0, 0, 0, TimeSpan.Zero),
            ServiceCoverageScope.Journey,
            Passengers(PassengerTypeCode.CHD),
            Sales(pointOfSaleIds: [601]),
            Travel(viaAirportIds: [Mhd]),
            [Pair(Mhd, Ist, RoutePairDirection.BothDirections), Pair(Thr, Mhd)],
            Fare(fareFamilyIds: [6]),
            null,
            QuantityRule.Create(AncillaryQuantityUnit.Piece, 1, 2),
            Standard(),
            CommercialOutcome.Create(CommercialDisposition.Paid, true, true),
            FeeDefinition.Create(Eur, FeeApplicationUnit.Item),
            [
                new ProvisionPriceLineArgs(AncillaryPriceLineCategory.Ancillary, null, "Extra bag", 15m),
                new ProvisionPriceLineArgs(AncillaryPriceLineCategory.Tax, "VAT", null, 1.35m, 98)
            ],
            SettlementDefinition.Create(ReissueRefundPolicy.Refundable, FormOfRefund.OriginalPayment, true, false),
            AvailabilityDefinition.Create(true),
            FulfillmentDefinition.Create("Ancillary"),
            new Fakes.SequentialIdGenerator());

        Assert.Equal(ProvisionStatus.Draft, provision.Status);
        Assert.Equal(20, provision.Sequence);
        Assert.Equal(ServiceCoverageScope.Journey, provision.CoverageScope);
        Assert.Equal(new[] { PassengerTypeCode.CHD }, provision.Passenger.PassengerTypeCodes);
        Assert.Equal(new long[] { 601 }, provision.Sales.PointOfSaleIds);
        Assert.Equal(new[] { Mhd }, provision.Travel.ViaAirportIds);
        Assert.Equal(2, provision.RoutePairs.Count);
        Assert.Equal(new long[] { 6 }, provision.Fare.FareFamilyIds);
        Assert.Null(provision.AdvancePurchase);
        Assert.Equal(ProvisionApplicationType.Standard, provision.Application.Type);
        Assert.Null(provision.Application.Baggage);
        Assert.Equal(AncillaryQuantityUnit.Piece, provision.Quantity.Unit);
        Assert.Equal(new[] { 15m, 1.35m }, provision.PriceLines.Select(line => line.UnitAmount));
        Assert.DoesNotContain(provision.PriceLines, line => line.Id == firstLineId && line.UnitAmount == 25m);
        Assert.True(provision.Availability.MustCheckAvailability);
        Assert.Equal(ReissueRefundPolicy.Refundable, provision.Settlement.ReissueRefund);
    }

    [Fact]
    public void P1_K05_a_refused_change_leaves_the_draft_untouched()
    {
        var provision = Provision(passenger: Passengers(PassengerTypeCode.ADT), routePairs: [Pair(Thr, Ist)]);

        BusinessAssert.Throws(16302, 422, () => provision.Change(
            0,
            null,
            null,
            ServiceCoverageScope.Journey,
            Passengers(PassengerTypeCode.CHD),
            Sales(),
            Travel(),
            [Pair(Thr, Thr)],
            Fare(),
            null,
            QuantityRule.Create(AncillaryQuantityUnit.Each, 1, 1),
            Standard(),
            CommercialOutcome.Create(CommercialDisposition.Free, false, false),
            null,
            [],
            SettlementDefinition.Create(ReissueRefundPolicy.NonRefundable, null, false, false),
            AvailabilityDefinition.Create(false),
            FulfillmentDefinition.Create("Ancillary"),
            new Fakes.SequentialIdGenerator()));

        Assert.Equal(10, provision.Sequence);
        Assert.Equal(ServiceCoverageScope.Sector, provision.CoverageScope);
        Assert.Equal(new[] { PassengerTypeCode.ADT }, provision.Passenger.PassengerTypeCodes);
        Assert.Equal(CommercialDisposition.Paid, provision.Outcome.Disposition);
        Assert.Single(provision.RoutePairs);
        Assert.Single(provision.PriceLines);
    }

    [Fact]
    public void P1_K06_the_provision_lifecycle_uses_only_the_existing_status_values()
    {
        Assert.Equal(new[] { "Active", "Draft", "Retired", "Suspended" }, Enum.GetNames<ProvisionStatus>().OrderBy(name => name, StringComparer.Ordinal));

        var provision = Provision();

        BusinessAssert.Throws(16303, 409, () => provision.Suspend(Now));
        BusinessAssert.Throws(16303, 409, () => provision.Reactivate());

        provision.Activate(Now.AddMinutes(1));

        Assert.Equal((ProvisionStatus.Active, Now.AddMinutes(1)), (provision.Status, provision.ActivatedAt!.Value));
        BusinessAssert.Throws(16303, 409, () => provision.Activate(Now));
        BusinessAssert.Throws(16303, 409, () => provision.Reactivate());

        provision.Suspend(Now.AddMinutes(2));

        Assert.Equal((ProvisionStatus.Suspended, Now.AddMinutes(2)), (provision.Status, provision.SuspendedAt!.Value));
        BusinessAssert.Throws(16303, 409, () => provision.Suspend(Now));
        BusinessAssert.Throws(16303, 409, () => provision.Activate(Now));

        provision.Reactivate();

        Assert.Equal(ProvisionStatus.Active, provision.Status);
        Assert.Null(provision.SuspendedAt);
        Assert.Equal(Now.AddMinutes(1), provision.ActivatedAt);

        provision.Retire(Now.AddMinutes(3));

        Assert.Equal((ProvisionStatus.Retired, Now.AddMinutes(3)), (provision.Status, provision.RetiredAt!.Value));
        BusinessAssert.Throws(16303, 409, () => provision.Retire(Now));
        BusinessAssert.Throws(16303, 409, () => provision.Reactivate());
        BusinessAssert.Throws(16303, 409, () => provision.Suspend(Now));
        BusinessAssert.Throws(16303, 409, () => provision.Activate(Now));
    }

    [Fact]
    public void P1_K06_a_draft_and_a_suspended_provision_can_be_retired_directly()
    {
        var draft = Provision(1);
        var suspended = Provision(2);

        suspended.Activate(Now);
        suspended.Suspend(Now);
        draft.Retire(Now);
        suspended.Retire(Now);

        Assert.Equal(ProvisionStatus.Retired, draft.Status);
        Assert.Equal(ProvisionStatus.Retired, suspended.Status);
    }

    [Theory]
    [InlineData(ProvisionStatus.Active)]
    [InlineData(ProvisionStatus.Suspended)]
    [InlineData(ProvisionStatus.Retired)]
    public void P1_K05_a_provision_that_left_draft_is_immutable(ProvisionStatus status)
    {
        var provision = Provision(passenger: Passengers(PassengerTypeCode.ADT));

        provision.Activate(Now);

        if (status == ProvisionStatus.Suspended)
            provision.Suspend(Now);

        if (status == ProvisionStatus.Retired)
            provision.Retire(Now);

        BusinessAssert.Throws(16303, 409, () => provision.Change(
            20,
            null,
            null,
            ServiceCoverageScope.Sector,
            Passengers(PassengerTypeCode.CHD),
            Sales(),
            Travel(),
            [],
            Fare(),
            null,
            QuantityRule.Create(AncillaryQuantityUnit.Each, 1, 1),
            Standard(),
            CommercialOutcome.Create(CommercialDisposition.Free, false, false),
            null,
            [],
            SettlementDefinition.Create(ReissueRefundPolicy.NonRefundable, null, false, false),
            AvailabilityDefinition.Create(false),
            FulfillmentDefinition.Create("Ancillary"),
            new Fakes.SequentialIdGenerator()));

        Assert.Equal(status, provision.Status);
        Assert.Equal(10, provision.Sequence);
        Assert.Equal(new[] { PassengerTypeCode.ADT }, provision.Passenger.PassengerTypeCodes);
    }
}
