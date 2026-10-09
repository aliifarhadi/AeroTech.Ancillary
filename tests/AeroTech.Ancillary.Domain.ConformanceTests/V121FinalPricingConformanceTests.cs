using AeroTech.Ancillary.Domain.AncillaryPricingAggregate;
using AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Entities;
using AeroTech.Ancillary.Domain.AncillaryPricingAggregate.ValueObjects;
using AeroTech.Ancillary.Domain.ConformanceTests.Fakes;
using AeroTech.Ancillary.Domain.ConformanceTests.Fixtures;
using AeroTech.Ancillary.Domain.ConformanceTests.Oracle;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;
using static AeroTech.Ancillary.Domain.ConformanceTests.Fixtures.P1Fixtures;

namespace AeroTech.Ancillary.Domain.ConformanceTests;

public class V121FinalPricingConformanceTests
{
    private const int Usd = 155;
    private const int Gbp = 53;
    private const int Jpy = 75;
    private const int Kwd = 82;

    private static readonly IReadOnlyDictionary<int, int> Scales = new Dictionary<int, int> { [Eur] = 2, [Usd] = 2, [Gbp] = 2, [Jpy] = 0, [Kwd] = 3 };

    private static AncillaryPricingRateArgs Rate(
        int currencyId,
        decimal baseAmount,
        PassengerTypeCode? passengerTypeCode = null,
        int? ageFromInclusive = null,
        int? ageToExclusive = null,
        params AncillaryPriceComponentArgs[] components)
        => new(passengerTypeCode, ageFromInclusive, ageToExclusive, baseAmount, currencyId, components);

    private static AncillaryPriceComponentArgs Tax(string code, decimal amount, int currencyId, int? countryId = null, int? stationAirportId = null, bool? includedInSource = null)
        => new(AncillaryPriceLineCategory.Tax, code, null, countryId, stationAirportId, amount, currencyId, null, includedInSource);

    private static AncillaryPriceComponentArgs Fee(string code, decimal amount, int currencyId, FeeApplicationUnit? unit = FeeApplicationUnit.Item)
        => new(AncillaryPriceLineCategory.Fee, code, null, null, null, amount, currencyId, unit, null);

    private static AncillaryPricing Define(PricingUnit pricingUnit, params AncillaryPricingRateArgs[] rates)
        => AncillaryPricing.Define(7001, 5001, pricingUnit, 1, rates, Scales, new SequentialIdGenerator(), Now);

    private static string Text(Money money) => FormattableString.Invariant($"{money.CurrencyId}:{money.Amount}");

    [Fact]
    public void PR01_one_rate_is_a_base_price_plus_its_own_tax_and_fee_in_one_currency()
    {
        var pricing = Define(
            PricingUnit.PerPassenger,
            Rate(Eur, 40m, PassengerTypeCode.ADT, components: [Tax("XT", 4m, Eur), Fee("SVC", 1m, Eur)]));
        var rate = Assert.Single(pricing.Rates);

        Assert.Equal(new[] { "AncillaryPricingId", "AgeFromInclusive", "AgeToExclusive", "BaseAmount", "BasePrice", "Components", "CurrencyId", "IsUnitTotalComplete", "PassengerTypeCode", "UnappliedFees", "UnitTotal" }.OrderBy(name => name, StringComparer.Ordinal), PropertiesOf<AncillaryPricingRate>());
        Assert.Equal(new[] { "Amount", "AncillaryPricingRateId", "Category", "Code", "CountryId", "FeeApplicationUnit", "Name", "StationAirportId", "TaxIncludedInSource" }, PropertiesOf<AncillaryPriceComponent>());
        Assert.Equal(new[] { "ActivatedAt", "AncillaryProvisionId", "CreatedAt", "PricingUnit", "Rates", "RetiredAt", "Status", "SuspendedAt", "Version" }, PropertiesOf<AncillaryPricing>());
        Assert.Equal($"{Eur}:40", Text(rate.BasePrice));
        Assert.Equal(new[] { $"Fee SVC {Eur}:1 Item", $"Tax XT {Eur}:4 " }, rate.Components.Select(component => $"{component.Category} {component.Code} {Text(component.Amount)} {component.FeeApplicationUnit}").OrderBy(text => text, StringComparer.Ordinal));
        Assert.Equal($"{Eur}:45", Text(rate.UnitTotal));
        Assert.True(rate.IsUnitTotalComplete);
        Assert.Empty(rate.UnappliedFees);
    }

    [Fact]
    public void PR02_PR03_two_currencies_are_two_authored_alternatives_and_a_currency_that_was_not_authored_is_never_converted()
    {
        var pricing = Define(
            PricingUnit.PerPassenger,
            Rate(Eur, 40m, PassengerTypeCode.ADT, components: [Tax("XT", 4m, Eur)]),
            Rate(Usd, 49m, PassengerTypeCode.ADT, components: [Tax("XT", 1.5m, Usd)]));

        Assert.Equal(new[] { $"{Eur}:44", $"{Usd}:50.5" }, pricing.Rates.OrderBy(rate => rate.CurrencyId).Select(rate => Text(rate.UnitTotal)));
        Assert.Equal((OracleRateOutcome.Selected, $"{Usd}:50.5"), Selected(pricing, Usd, PassengerTypeCode.ADT, 30));
        Assert.Equal((OracleRateOutcome.Selected, $"{Eur}:44"), Selected(pricing, Eur, PassengerTypeCode.ADT, 30));
        Assert.Equal((OracleRateOutcome.NoMatchingCurrency, null), Selected(pricing, Gbp, PassengerTypeCode.ADT, 30));
        Assert.Equal((OracleRateOutcome.NoMatchingSelector, null), Selected(pricing, Eur, PassengerTypeCode.CHD, 8));
        Assert.DoesNotContain(pricing.Rates, rate => rate.UnitTotal.Amount == 94.5m);
    }

    private static (OracleRateOutcome, string?) Selected(AncillaryPricing pricing, int currencyId, PassengerTypeCode? passengerType, int? age)
    {
        var selected = ProvisionRuleOracle.SelectRate(pricing, currencyId, passengerType, age);

        return (selected.Outcome, selected.Rate is null ? null : Text(selected.Rate.UnitTotal));
    }

    [Theory]
    [InlineData(Jpy, "1201.5", 16512)]
    [InlineData(Jpy, "1201", 0)]
    [InlineData(Kwd, "12.125", 0)]
    [InlineData(Kwd, "12.1251", 16512)]
    [InlineData(Eur, "10.009", 16512)]
    [InlineData(Eur, "10.01", 0)]
    public void PR04_PR05_PR06_PR07_an_amount_keeps_the_scale_of_its_currency_and_is_never_rounded(int currencyId, string amount, int refusal)
    {
        var value = decimal.Parse(amount, System.Globalization.CultureInfo.InvariantCulture);

        if (refusal == 0)
        {
            Assert.Equal(value, Define(PricingUnit.PerItem, Rate(currencyId, value)).Rates.Single().BasePrice.Amount);
            Assert.Equal(value, Define(PricingUnit.PerItem, Rate(currencyId, 100m, components: [Tax("XT", value, currencyId)])).Rates.Single().Components.Single().Amount.Amount);
        }
        else
        {
            BusinessAssert.Throws(refusal, 422, () => Define(PricingUnit.PerItem, Rate(currencyId, value)));
            BusinessAssert.Throws(refusal, 422, () => Define(PricingUnit.PerItem, Rate(currencyId, 100m, components: [Tax("XT", value, currencyId)])));
        }
    }

    [Fact]
    public void PR04_a_currency_that_the_reference_does_not_know_and_a_scale_beyond_storage_are_refused()
    {
        BusinessAssert.Throws(16511, 422, () => Define(PricingUnit.PerItem, Rate(999999, 10m)));
        BusinessAssert.Throws(16502, 422, () => Define(PricingUnit.PerItem, Rate(Kwd, 1.1234567m)));
        BusinessAssert.Throws(16502, 422, () => Define(PricingUnit.PerItem, Rate(0, 10m)));
    }

    [Fact]
    public void PR08_a_component_in_another_currency_than_its_rate_is_refused()
    {
        BusinessAssert.Throws(16513, 422, () => Define(PricingUnit.PerItem, Rate(Eur, 40m, components: [Tax("XT", 4m, Usd)])));
        BusinessAssert.Throws(16513, 422, () => Define(PricingUnit.PerItem, Rate(Eur, 40m, components: [Fee("SVC", 1m, Usd)])));
        BusinessAssert.Throws(16513, 422, () => Money.Of(1m, Eur).Add(Money.Of(1m, Usd)));
    }

    [Fact]
    public void PR09_PR10_PR11_a_rate_key_is_unique_and_age_bands_of_one_currency_and_passenger_type_never_overlap()
    {
        BusinessAssert.Throws(16508, 409, () => Define(PricingUnit.PerPassenger, Rate(Eur, 40m, PassengerTypeCode.ADT), Rate(Eur, 41m, PassengerTypeCode.ADT)));
        BusinessAssert.Throws(16508, 409, () => Define(PricingUnit.PerPassenger, Rate(Eur, 40m, PassengerTypeCode.ADT), Rate(Eur, 30m, PassengerTypeCode.ADT, 60, null)));
        BusinessAssert.Throws(16508, 409, () => Define(PricingUnit.PerPassenger, Rate(Eur, 40m, PassengerTypeCode.ADT, 0, 65), Rate(Eur, 30m, PassengerTypeCode.ADT, 60, null)));
        BusinessAssert.Throws(16508, 409, () => Define(PricingUnit.PerPassenger, Rate(Eur, 40m), Rate(Eur, 30m, PassengerTypeCode.ADT)));

        var bands = Define(
            PricingUnit.PerPassenger,
            Rate(Eur, 40m, PassengerTypeCode.ADT, 0, 65),
            Rate(Eur, 30m, PassengerTypeCode.ADT, 65, null),
            Rate(Usd, 45m, PassengerTypeCode.ADT));

        Assert.Equal((OracleRateOutcome.Selected, $"{Eur}:40"), Selected(bands, Eur, PassengerTypeCode.ADT, 64));
        Assert.Equal((OracleRateOutcome.Selected, $"{Eur}:30"), Selected(bands, Eur, PassengerTypeCode.ADT, 65));
        Assert.Equal((OracleRateOutcome.Selected, $"{Usd}:45"), Selected(bands, Usd, PassengerTypeCode.ADT, 65));
        Assert.Equal(3, bands.Rates.Count);
    }

    [Fact]
    public void PR12_PR13_a_non_passenger_unit_files_exactly_one_rate_per_currency_and_no_passenger_selector()
    {
        var piece = Define(PricingUnit.PerPiece, Rate(Eur, 60m), Rate(Usd, 70m));

        Assert.Equal(new[] { Eur, Usd }, piece.Rates.Select(rate => rate.CurrencyId).OrderBy(id => id));
        BusinessAssert.Throws(16508, 409, () => Define(PricingUnit.PerPiece, Rate(Eur, 60m), Rate(Eur, 61m)));
        BusinessAssert.Throws(16508, 409, () => Define(PricingUnit.PerPiece, Rate(Eur, 60m, PassengerTypeCode.CHD)));
        BusinessAssert.Throws(16508, 409, () => Define(PricingUnit.PerPiece, Rate(Eur, 60m, null, 2, 12)));
        BusinessAssert.Throws(16508, 409, () => Define(PricingUnit.PerPiece));
    }

    [Fact]
    public void PR14_a_component_is_unique_by_category_code_country_station_and_fee_unit_inside_its_rate()
    {
        BusinessAssert.Throws(16508, 409, () => Define(PricingUnit.PerItem, Rate(Eur, 40m, components: [Tax("XT", 4m, Eur, 98, 2), Tax("XT", 1m, Eur, 98, 2)])));

        var distinct = Define(
            PricingUnit.PerItem,
            Rate(Eur, 40m, components: [Tax("XT", 4m, Eur, 98), Tax("XT", 1m, Eur, 99), Tax("YQ", 1m, Eur, 98), Fee("XT", 1m, Eur)]),
            Rate(Usd, 45m, components: [Tax("XT", 4m, Usd, 98)]));

        Assert.Equal(5, distinct.Rates.Sum(rate => rate.Components.Count));
    }

    [Fact]
    public void PR15_PR22_a_fee_per_ticket_stays_outside_the_unit_total_and_is_reported_as_unapplied()
    {
        var rate = Define(
            PricingUnit.PerItem,
            Rate(Eur, 40m, components: [Tax("XT", 4m, Eur), Fee("SVC", 1m, Eur), Fee("TKT", 7m, Eur, FeeApplicationUnit.Ticket), Fee("SEC", 3m, Eur, FeeApplicationUnit.SectorOrPortion)])).Rates.Single();

        Assert.Equal($"{Eur}:45", Text(rate.UnitTotal));
        Assert.False(rate.IsUnitTotalComplete);
        Assert.Equal(new[] { $"SEC {Eur}:3 SectorOrPortion", $"TKT {Eur}:7 Ticket" }, rate.UnappliedFees.Select(fee => $"{fee.Code} {Text(fee.Amount)} {fee.FeeApplicationUnit}").OrderBy(text => text, StringComparer.Ordinal));
        Assert.NotEqual(55m, rate.UnitTotal.Amount);
    }

    [Fact]
    public void PR16_a_tax_never_has_a_fee_unit_and_a_fee_is_not_published_without_one()
    {
        BusinessAssert.Throws(16502, 422, () => Define(PricingUnit.PerItem, Rate(Eur, 40m, components: [new(AncillaryPriceLineCategory.Tax, "XT", null, null, null, 4m, Eur, FeeApplicationUnit.Item, null)])));
        BusinessAssert.Throws(16502, 422, () => Define(PricingUnit.PerItem, Rate(Eur, 40m, components: [new(AncillaryPriceLineCategory.Fee, "SVC", null, null, null, 1m, Eur, FeeApplicationUnit.Item, true)])));
        BusinessAssert.Throws(16502, 422, () => Define(PricingUnit.PerItem, Rate(Eur, 40m, components: [new(AncillaryPriceLineCategory.Tax, "", null, null, null, 4m, Eur, null, null)])));

        var draft = Define(PricingUnit.PerItem, Rate(Eur, 40m, components: [Fee("SVC", 1m, Eur, null)]));

        Assert.False(draft.Rates.Single().IsUnitTotalComplete);
        BusinessAssert.Throws(16515, 422, () => draft.Activate(Scales, Now));
        Assert.Equal(PricingStatus.Draft, draft.Status);
    }

    [Fact]
    public void PR17_a_paid_base_is_positive_and_no_amount_is_negative()
    {
        BusinessAssert.Throws(16502, 422, () => Define(PricingUnit.PerItem, Rate(Eur, 0m)));
        BusinessAssert.Throws(16502, 422, () => Define(PricingUnit.PerItem, Rate(Eur, -1m)));
        BusinessAssert.Throws(16502, 422, () => Define(PricingUnit.PerItem, Rate(Eur, 10m, components: [Tax("XT", -1m, Eur)])));
        Assert.Equal(0m, Define(PricingUnit.PerItem, Rate(Eur, 10m, components: [Tax("XT", 0m, Eur)])).Rates.Single().Components.Single().Amount.Amount);
    }

    [Fact]
    public void PR20_an_active_revision_is_never_edited_and_a_revision_copies_every_rate_into_a_new_draft()
    {
        var ids = new SequentialIdGenerator();
        var active = AncillaryPricing.Define(
            7001,
            5001,
            PricingUnit.PerPassenger,
            1,
            [Rate(Eur, 40m, PassengerTypeCode.ADT, components: [Tax("XT", 4m, Eur, includedInSource: true), Fee("SVC", 1m, Eur)]), Rate(Usd, 49m, PassengerTypeCode.ADT)],
            Scales,
            ids,
            Now);

        active.Activate(Scales, Now);

        BusinessAssert.Throws(16503, 409, () => active.Change([Rate(Eur, 1m)], Scales, ids));

        var revision = active.Revise(7002, 2, ids, Now);

        Assert.Equal((PricingStatus.Draft, 2, PricingStatus.Active), (revision.Status, revision.Version, active.Status));
        Assert.Equal(
            active.Rates.Select(rate => $"{Text(rate.BasePrice)} {rate.PassengerTypeCode} {string.Join(",", rate.Components.Select(component => $"{component.Code}{Text(component.Amount)}{component.TaxIncludedInSource}"))}"),
            revision.Rates.Select(rate => $"{Text(rate.BasePrice)} {rate.PassengerTypeCode} {string.Join(",", rate.Components.Select(component => $"{component.Code}{Text(component.Amount)}{component.TaxIncludedInSource}"))}"));
        Assert.Empty(active.Rates.Select(rate => rate.Id).Intersect(revision.Rates.Select(rate => rate.Id)));
        Assert.All(revision.Rates.SelectMany(rate => rate.Components), component => Assert.DoesNotContain(active.Rates.SelectMany(rate => rate.Components), original => ReferenceEquals(original.Amount, component.Amount)));
        Assert.True(active.Rates.First().Components.Single(component => component.Code == "XT").TaxIncludedInSource);
        Assert.Equal($"{Eur}:45", Text(active.Rates.Single(rate => rate.CurrencyId == Eur).UnitTotal));
    }

    [Fact]
    public void PR24_PR25_the_flat_base_category_and_percentage_fee_units_are_refused_explicitly()
    {
        BusinessAssert.Throws(16514, 422, () => Define(PricingUnit.PerItem, Rate(Eur, 40m, components: [new(AncillaryPriceLineCategory.Ancillary, "BASE", null, null, null, 40m, Eur, null, null)])));

        var percent = Define(PricingUnit.PerItem, Rate(Eur, 40m, components: [Fee("PCT", 1m, Eur, FeeApplicationUnit.OnePercentOfFarePerKilogram)]));
        var perKilogram = Define(PricingUnit.PerItem, Rate(Eur, 40m, components: [Fee("KG", 1m, Eur, FeeApplicationUnit.PerOneKilogramOver)]));

        BusinessAssert.Throws(16305, 422, () => percent.Activate(Scales, Now));
        BusinessAssert.Throws(16305, 422, () => perKilogram.Activate(Scales, Now));
        Assert.All(new[] { percent, perKilogram }, pricing => Assert.False(pricing.Rates.Single().IsUnitTotalComplete));
    }
}
