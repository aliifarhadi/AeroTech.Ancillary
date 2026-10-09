using AeroTech.Ancillary.Domain.AncillaryPricingAggregate;
using AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Entities;
using AeroTech.Ancillary.Domain.ConformanceTests.Fakes;
using AeroTech.Ancillary.Domain.ConformanceTests.Fixtures;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;
using static AeroTech.Ancillary.Domain.ConformanceTests.Fixtures.P1Fixtures;

namespace AeroTech.Ancillary.Domain.ConformanceTests;

public class V12PricingConformanceTests
{
    private const AncillaryPriceLineCategory Tax = AncillaryPriceLineCategory.Tax;
    private const AncillaryPriceLineCategory Fee = AncillaryPriceLineCategory.Fee;

    private static void Change(AncillaryPricing pricing, SequentialIdGenerator ids, params FiledLine[] lines)
        => pricing.Change(FiledPrice.ToRates(lines, Eur, FeeApplicationUnit.Item), FiledPrice.Scales, ids);

    [Fact]
    public void V12_REQ_the_pricing_root_carries_exactly_the_filed_fields_and_no_currency_or_fee_unit_of_its_own()
    {
        Assert.Equal(
            new[] { "ActivatedAt", "AncillaryProvisionId", "CreatedAt", "PricingUnit", "Rates", "RetiredAt", "Status", "SuspendedAt", "Version" },
            PropertiesOf<AncillaryPricing>());
        Assert.Equal(
            new[] { ("Draft", 1), ("Active", 2), ("Suspended", 3), ("Retired", 4) },
            Enum.GetValues<PricingStatus>().Select(status => (status.ToString(), (int)status)));
        Assert.Equal(
            new[] { ("Ancillary", 1), ("Tax", 2), ("Fee", 3) },
            Enum.GetValues<AncillaryPriceLineCategory>().Select(category => (category.ToString(), (int)category)));
        Assert.Empty(typeof(AncillaryPricingRate).GetConstructors());
        Assert.Empty(typeof(AncillaryPriceComponent).GetConstructors());
    }

    [Fact]
    public void V12_P04_per_passenger_rates_are_selected_by_passenger_type_and_half_open_age_bands()
    {
        var pricing = Pricing(
            PricingUnit.PerPassenger,
            [
                Base(20m, PassengerTypeCode.ADT, 0, 65),
                Base(40m, PassengerTypeCode.ADT, 65),
                Base(10m, PassengerTypeCode.CHD),
                Component(Tax, "T1", 2m, PassengerTypeCode.ADT, 0, 65)
            ]);

        Assert.Equal((3, 1), (pricing.Rates.Count, pricing.Rates.Sum(rate => rate.Components.Count)));
        Assert.Equal(
            new[] { ((int?)0, (int?)65, 20m, 22m), (65, null, 40m, 40m) },
            pricing.Rates
                .Where(rate => rate.PassengerTypeCode == PassengerTypeCode.ADT)
                .Select(rate => (rate.AgeFromInclusive, rate.AgeToExclusive, rate.BasePrice.Amount, rate.UnitTotal.Amount)));

        var ageOnly = Pricing(PricingUnit.PerPassenger, [Base(20m, null, 0, 65), Base(40m, null, 65)]);

        Assert.Equal(2, ageOnly.Rates.Count);
        BusinessAssert.Throws(16508, 409, () => Pricing(PricingUnit.PerPassenger, [Base(20m, PassengerTypeCode.ADT, 0, 65), Base(40m, PassengerTypeCode.ADT, 60)]));
        BusinessAssert.Throws(16508, 409, () => Pricing(PricingUnit.PerPassenger, [Base(20m, PassengerTypeCode.ADT, 0), Base(40m, PassengerTypeCode.ADT, 65)]));
        BusinessAssert.Throws(16508, 409, () => Pricing(PricingUnit.PerPassenger, [Base(20m, PassengerTypeCode.ADT), Base(40m, PassengerTypeCode.ADT, 65)]));
        BusinessAssert.Throws(16508, 409, () => Pricing(PricingUnit.PerPassenger, [Base(20m), Base(10m, PassengerTypeCode.CHD)]));
        BusinessAssert.Throws(16502, 422, () => Pricing(PricingUnit.PerPassenger, [Base(20m, PassengerTypeCode.ADT, 65, 65)]));
        BusinessAssert.Throws(16502, 422, () => Pricing(PricingUnit.PerPassenger, [Base(20m, PassengerTypeCode.ADT, null, 65)]));
        BusinessAssert.Throws(16502, 422, () => Pricing(PricingUnit.PerPassenger, [Base(20m, PassengerTypeCode.ADT, -1, 65)]));
        BusinessAssert.Throws(16502, 422, () => Pricing(PricingUnit.PerPassenger, [Base(20m, (PassengerTypeCode)9999)]));

        var sameBandForTwoTypes = Pricing(
            PricingUnit.PerPassenger,
            [Base(20m, PassengerTypeCode.ADT, 0, 65), Base(12m, PassengerTypeCode.CHD, 0, 65)]);

        Assert.Equal(2, sameBandForTwoTypes.Rates.Count);
    }

    [Theory]
    [InlineData(PricingUnit.PerRoom)]
    [InlineData(PricingUnit.PerItem)]
    [InlineData(PricingUnit.PerVehicle)]
    [InlineData(PricingUnit.PerSeat)]
    [InlineData(PricingUnit.PerPiece)]
    [InlineData(PricingUnit.PerKilogram)]
    public void V12_P05_a_non_passenger_unit_files_one_generic_rate_and_refuses_passenger_or_age_selectors(PricingUnit pricingUnit)
    {
        var pricing = Pricing(pricingUnit, [Base(45m), Component(Tax, "VAT", 4.5m), Component(Fee, "SVC", 1m)]);
        var rate = Assert.Single(pricing.Rates);

        Assert.Equal(pricingUnit, pricing.PricingUnit);
        Assert.Equal((Eur, 45m, 50.5m, Eur), (rate.CurrencyId, rate.BasePrice.Amount, rate.UnitTotal.Amount, rate.UnitTotal.CurrencyId));
        Assert.All(rate.Components, component => Assert.Equal(Eur, component.Amount.CurrencyId));
        BusinessAssert.Throws(16508, 409, () => Pricing(pricingUnit, [Base(45m, PassengerTypeCode.ADT)]));
        BusinessAssert.Throws(16508, 409, () => Pricing(pricingUnit, [Base(45m, null, 0, 65), Base(60m, null, 65)]));
    }

    [Fact]
    public void V12_P06_each_rate_has_exactly_one_base_and_uniquely_coded_tax_or_fee_components()
    {
        BusinessAssert.Throws(16508, 409, () => Pricing(PricingUnit.PerItem, []));
        BusinessAssert.Throws(16508, 409, () => Pricing(PricingUnit.PerItem, [Base(45m), Base(5m)]));
        BusinessAssert.Throws(16508, 409, () => Pricing(PricingUnit.PerItem, [Base(45m), Component(Tax, "VAT", 4m), Component(Tax, "VAT", 1m)]));
        BusinessAssert.Throws(16502, 422, () => Pricing(PricingUnit.PerItem, [Base(45m), Component(Tax, null, 4m)]));
        BusinessAssert.Throws(16502, 422, () => Pricing(PricingUnit.PerItem, [Base(45m), Component(Fee, null, 4m)]));
        BusinessAssert.Throws(16502, 422, () => Pricing(PricingUnit.PerItem, [Base(45m), Component(Tax, "TOOLONGCODE1", 4m)]));

        var pricing = Pricing(
            PricingUnit.PerItem,
            [
                Base(45m),
                Component(Tax, "VAT", 4m, countryId: 1),
                Component(Tax, "VAT", 1m, countryId: 2),
                Component(Tax, "APT", 2m, stationAirportId: 2),
                Component(Fee, "VAT", 3m)
            ]);
        var rate = Assert.Single(pricing.Rates);
        var airportTax = rate.Components.Single(component => component.Code == "APT");

        Assert.Equal(4, rate.Components.Count);
        Assert.Equal((Tax, "APT", (int?)null, (int?)2, 2m), (airportTax.Category, airportTax.Code, airportTax.CountryId, airportTax.StationAirportId, airportTax.Amount.Amount));
        Assert.Equal(5, rate.Components.Select(component => component.Id).Append(rate.Id).Distinct().Count());
        Assert.All(rate.Components, component => Assert.Equal(rate.Id, component.AncillaryPricingRateId));
        Assert.Equal(55m, rate.UnitTotal.Amount);
    }

    [Fact]
    public void V12_P07_amounts_keep_the_scale_of_their_currency_and_a_base_is_positive()
    {
        BusinessAssert.Throws(16502, 422, () => Pricing(PricingUnit.PerItem, [Base(-1m)]));
        BusinessAssert.Throws(16502, 422, () => Pricing(PricingUnit.PerItem, [Base(0m)]));
        BusinessAssert.Throws(16512, 422, () => Pricing(PricingUnit.PerItem, [Base(10.123m)]));
        BusinessAssert.Throws(16502, 422, () => Pricing(PricingUnit.PerItem, [Base(10m), Component(Tax, "VAT", -0.01m)]));
        BusinessAssert.Throws(16512, 422, () => Pricing(PricingUnit.PerItem, [Base(10m), Component(Tax, "VAT", 0.001m)]));

        var pricing = Pricing(PricingUnit.PerItem, [Base(12.34m), Component(Tax, "VAT", 0m)]);

        Assert.Equal((12.34m, 0m), (pricing.Rates.Single().BasePrice.Amount, pricing.Rates.Single().Components.Single().Amount.Amount));
        BusinessAssert.Throws(16502, 422, () => AncillaryPricing.Define(1, 5001, PricingUnit.PerItem, 1, FiledPrice.ToRates([Base(10m)], 0, null), FiledPrice.Scales, new SequentialIdGenerator(), Now));
        BusinessAssert.Throws(16502, 422, () => AncillaryPricing.Define(1, 5001, (PricingUnit)42, 1, FiledPrice.ToRates([Base(10m)], Eur, null), FiledPrice.Scales, new SequentialIdGenerator(), Now));
        BusinessAssert.Throws(16502, 422, () => AncillaryPricing.Define(1, 5001, PricingUnit.PerItem, 0, FiledPrice.ToRates([Base(10m)], Eur, null), FiledPrice.Scales, new SequentialIdGenerator(), Now));
        Assert.DoesNotContain(
            typeof(AncillaryPricing).GetMethods().Concat(typeof(AncillaryPricingRate).GetMethods()).Select(method => method.Name),
            name => name.Contains("Convert", StringComparison.Ordinal) || name.Contains("Exchange", StringComparison.Ordinal) || name.Contains("Calculate", StringComparison.Ordinal));
    }

    [Fact]
    public void V12_P02_a_pricing_is_edited_in_draft_only_and_follows_the_four_state_lifecycle()
    {
        var ids = new SequentialIdGenerator();
        var pricing = Pricing(PricingUnit.PerItem, [Base(10m)], ids: ids);
        var firstRate = pricing.Rates.Single().Id;

        Change(pricing, ids, Base(12m), Component(Tax, "VAT", 1.2m));

        Assert.Equal((1, 1), (pricing.Rates.Count, pricing.Rates.Single().Components.Count));
        Assert.DoesNotContain(pricing.Rates, rate => rate.Id == firstRate);
        BusinessAssert.Throws(16508, 409, () => Change(pricing, ids, Base(12m), Base(13m)));
        Assert.Equal(12m, pricing.Rates.Single().BasePrice.Amount);
        BusinessAssert.Throws(16503, 409, () => pricing.Revise(7002, 2, ids, Now));
        BusinessAssert.Throws(16503, 409, () => pricing.Suspend(Now));
        BusinessAssert.Throws(16503, 409, () => pricing.Reactivate(FiledPrice.Scales));

        pricing.Activate(FiledPrice.Scales, Now.AddMinutes(1));

        Assert.Equal((PricingStatus.Active, Now.AddMinutes(1)), (pricing.Status, pricing.ActivatedAt!.Value));
        BusinessAssert.Throws(16503, 409, () => Change(pricing, ids, Base(99m)));
        BusinessAssert.Throws(16503, 409, () => pricing.Activate(FiledPrice.Scales, Now));
        Assert.Equal(12m, pricing.Rates.Single().BasePrice.Amount);

        pricing.Suspend(Now.AddMinutes(2));

        Assert.Equal((PricingStatus.Suspended, Now.AddMinutes(2)), (pricing.Status, pricing.SuspendedAt!.Value));
        BusinessAssert.Throws(16503, 409, () => Change(pricing, ids, Base(99m)));

        pricing.Reactivate(FiledPrice.Scales);

        Assert.Equal((PricingStatus.Active, (DateTimeOffset?)null), (pricing.Status, pricing.SuspendedAt));

        pricing.Retire(Now.AddMinutes(3));

        Assert.Equal((PricingStatus.Retired, Now.AddMinutes(3)), (pricing.Status, pricing.RetiredAt!.Value));
        BusinessAssert.Throws(16503, 409, () => pricing.Retire(Now));
        BusinessAssert.Throws(16503, 409, () => pricing.Reactivate(FiledPrice.Scales));
        BusinessAssert.Throws(16503, 409, () => pricing.Activate(FiledPrice.Scales, Now));
    }

    [Fact]
    public void V12_P08_a_superseded_or_revised_version_keeps_its_own_filed_rates()
    {
        var ids = new SequentialIdGenerator();
        var first = Pricing(PricingUnit.PerPassenger, [Base(20m, PassengerTypeCode.ADT), Base(10m, PassengerTypeCode.CHD)], ids: ids);

        BusinessAssert.Throws(16503, 409, () => first.Supersede(Now));

        first.Activate(FiledPrice.Scales, Now);

        BusinessAssert.Throws(16502, 422, () => first.Revise(7002, 1, ids, Now));

        var revision = first.Revise(7002, 2, ids, Now.AddDays(1));

        Assert.Equal((7002L, 2, PricingStatus.Draft, PricingUnit.PerPassenger, first.AncillaryProvisionId), (revision.Id, revision.Version, revision.Status, revision.PricingUnit!.Value, revision.AncillaryProvisionId));
        Assert.Equal(first.Rates.Select(rate => (rate.PassengerTypeCode, rate.BasePrice.Amount)), revision.Rates.Select(rate => (rate.PassengerTypeCode, rate.BasePrice.Amount)));
        Assert.Empty(first.Rates.Select(rate => rate.Id).Intersect(revision.Rates.Select(rate => rate.Id)));

        Change(revision, ids, Base(22m, PassengerTypeCode.ADT), Base(11m, PassengerTypeCode.CHD));
        first.Supersede(Now.AddDays(2));
        revision.Activate(FiledPrice.Scales, Now.AddDays(2));

        Assert.Equal((PricingStatus.Retired, Now.AddDays(2)), (first.Status, first.RetiredAt!.Value));
        Assert.Equal(new[] { 20m, 10m }, first.Rates.Select(rate => rate.BasePrice.Amount));
        Assert.Equal(new[] { 22m, 11m }, revision.Rates.Select(rate => rate.BasePrice.Amount));
        Assert.Equal(PricingStatus.Active, revision.Status);
    }

    [Fact]
    public void V12_REQ_the_fee_application_unit_belongs_to_a_fee_component_and_unsupported_units_are_not_activated()
    {
        var perRoomPerTicket = Pricing(PricingUnit.PerRoom, [Base(45m), Component(Fee, "SVC", 1m)], FeeApplicationUnit.Ticket);
        var withoutFee = Pricing(PricingUnit.PerVehicle, [Base(80m)], null);
        var feeWithoutUnit = Pricing(PricingUnit.PerVehicle, [Base(80m), Component(Fee, "SVC", 1m)], null);

        perRoomPerTicket.Activate(FiledPrice.Scales, Now);
        withoutFee.Activate(FiledPrice.Scales, Now);

        Assert.Equal((PricingUnit.PerRoom, FeeApplicationUnit.Ticket, 45m), (perRoomPerTicket.PricingUnit!.Value, perRoomPerTicket.Rates.Single().Components.Single().FeeApplicationUnit!.Value, perRoomPerTicket.Rates.Single().UnitTotal.Amount));
        Assert.Empty(withoutFee.Rates.Single().Components);
        BusinessAssert.Throws(16515, 422, () => feeWithoutUnit.Activate(FiledPrice.Scales, Now));

        foreach (var unsupported in new[]
                 {
                     FeeApplicationUnit.PerOneKilogramOver, FeeApplicationUnit.PerFiveKilogramsOver, FeeApplicationUnit.HalfPercentOfFarePerKilogram,
                     FeeApplicationUnit.OnePercentOfFarePerKilogram, FeeApplicationUnit.OneAndHalfPercentOfFarePerKilogram
                 })
        {
            var pricing = Pricing(PricingUnit.PerKilogram, [Base(5m), Component(Fee, "SVC", 1m)], unsupported);

            Assert.Equal(unsupported, pricing.Rates.Single().Components.Single().FeeApplicationUnit);
            BusinessAssert.Throws(16305, 422, () => pricing.Activate(FiledPrice.Scales, Now));
            Assert.Equal(PricingStatus.Draft, pricing.Status);
        }

        BusinessAssert.Throws(16502, 422, () => Pricing(PricingUnit.PerItem, [Base(5m), Component(Fee, "SVC", 1m)], (FeeApplicationUnit)77));
    }
}
