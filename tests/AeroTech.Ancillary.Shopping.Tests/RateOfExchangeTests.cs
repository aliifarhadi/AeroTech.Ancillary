using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Ancillary.Domain._Shared.Contracts;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Ancillary.Shopping.Tests.Fixtures;
using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.BackofficeDesk;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.ShoppingLab;

namespace AeroTech.Ancillary.Shopping.Tests;

public class RateOfExchangeTests
{
    private static readonly PublishedRateOfExchange EurToUsd = new("T1", Eur, Usd, 1.1m, 2, 1d);

    private static (ShoppingLab Lab, string Reference) Wifi(params int[] currencies)
    {
        var lab = new ShoppingLab();
        var wifi = lab.Definition("A24");

        lab.Price(
            wifi,
            lab.Provision(wifi),
            currencies.Select(currency => Rate(currency == Jpy ? 15_000m : 100m, currency, components: [Tax("VAT", currency == Jpy ? 1_500m : 10m, TaxTreatment.AddedToBase, currency)])).ToArray());

        return (lab, wifi.ServiceDefinitionRef);
    }

    [Fact]
    public async Task ROE_01_a_filed_rate_in_another_currency_is_converted_with_the_published_rate_and_the_target_scale()
    {
        var (lab, wifi) = Wifi(Eur);

        lab.Catalog.RatesOfExchange.Add(EurToUsd);

        var result = await lab.Engine.ShopAsync(Trip.Context(currencyId: Usd));
        var price = result.One(wifi).Price;

        Assert.Equal((PriceAssessmentStatus.Complete, Usd, Eur, 110.00m, 121.00m), (price.Status, price.CurrencyId, price.FiledCurrencyId, price.BaseAmount, price.CompleteUnitTotal));
        Assert.Equal((11.00m, Usd), (price.AddedTaxLines.Single().Amount, price.AddedTaxLines.Single().CurrencyId));
        Assert.Equal(("T1", Eur, Usd, 1.1m), (price.RateOfExchange!.RateOfExchangeTableId, price.RateOfExchange.FromCurrencyId, price.RateOfExchange.ToCurrencyId, price.RateOfExchange.Rate));
        Assert.Equal(["T1"], result.RatesOfExchange.Select(rate => rate.RateOfExchangeTableId));
        Assert.Equal(OfferReadiness.NeedsSelection, result.One(wifi).OfferReadiness);
    }

    [Fact]
    public async Task ROE_02_without_a_published_rate_the_product_is_withheld_instead_of_priced_in_the_filed_currency()
    {
        var (lab, wifi) = Wifi(Eur);
        var hidden = await lab.Engine.ShopAsync(Trip.Context(currencyId: Usd));
        var shown = await lab.Engine.ShopAsync(Trip.Context(currencyId: Usd), new ShoppingFilter { IncludeNonSellable = true });
        var price = shown.One(wifi).Price;

        Assert.Empty(hidden.Candidates);
        Assert.Empty(hidden.RatesOfExchange);
        Assert.Equal((PriceAssessmentStatus.Unavailable, OfferReadiness.Unavailable), (price.Status, shown.One(wifi).OfferReadiness));
        Assert.Contains(ShoppingReasonCodes.RateOfExchangeNotFound, price.ReasonCodes);
        Assert.Null(price.CompleteUnitTotal);
        Assert.Null(price.RateOfExchange);
    }

    [Fact]
    public async Task ROE_03_the_target_currency_itself_is_never_converted_or_rounded()
    {
        var (lab, wifi) = Wifi(Eur);

        lab.Catalog.RatesOfExchange.Add(EurToUsd);

        var price = (await lab.Engine.ShopAsync(Trip.Context(currencyId: Eur))).One(wifi).Price;

        Assert.Equal((Eur, Eur, 100m, 110m, 1m), (price.CurrencyId, price.FiledCurrencyId, price.BaseAmount, price.CompleteUnitTotal, price.RateOfExchange!.Rate));
        Assert.Equal(Eur.ToString(), price.RateOfExchange.RateOfExchangeTableId);
    }

    [Fact]
    public async Task ROE_04_the_target_currency_wins_and_otherwise_the_first_filed_currency_with_a_rate_is_taken()
    {
        var (lab, wifi) = Wifi(Jpy, Eur);

        lab.Catalog.RatesOfExchange.Add(new PublishedRateOfExchange("T2", Eur, Kwd, 0.3105m, 3, 1d));

        var onlyEuroConvertible = (await lab.Engine.ShopAsync(Trip.Context(currencyId: Kwd))).One(wifi).Price;

        lab.Catalog.RatesOfExchange.Add(new PublishedRateOfExchange("T3", Jpy, Kwd, 0.002m, 3, 1d));

        var firstFiled = (await lab.Engine.ShopAsync(Trip.Context(currencyId: Kwd))).One(wifi).Price;
        var filedInJpy = (await lab.Engine.ShopAsync(Trip.Context(currencyId: Jpy))).One(wifi).Price;

        Assert.Equal((Eur, "T2", 31.050m, 34.155m), (onlyEuroConvertible.FiledCurrencyId, onlyEuroConvertible.RateOfExchange!.RateOfExchangeTableId, onlyEuroConvertible.BaseAmount, onlyEuroConvertible.CompleteUnitTotal));
        Assert.Equal((Jpy, "T3", 30.000m, 33.000m), (firstFiled.FiledCurrencyId, firstFiled.RateOfExchange!.RateOfExchangeTableId, firstFiled.BaseAmount, firstFiled.CompleteUnitTotal));
        Assert.Equal((Jpy, Jpy, 15_000m, 16_500m), (filedInJpy.FiledCurrencyId, filedInJpy.CurrencyId, filedInJpy.BaseAmount, filedInJpy.CompleteUnitTotal));
    }

    [Fact]
    public async Task TZ_01_the_origin_airport_time_zone_of_the_reference_data_reaches_every_flight_of_the_context()
    {
        var lab = new ShoppingLab();
        var dated = lab.Definition("A24", "WIFI_DATED");

        lab.Price(dated, lab.Provision(dated, rules: Rules(dayTime: new([new(127, new TimeOnly(0, 0), new TimeOnly(23, 59), DayTimeRestrictionEffect.Allow)]))), Rate(40m));

        var airports = new TestOnlyAirportReference();

        foreach (var airportId in Enumerable.Range(1, 40))
            airports.TimeZones[airportId] = "Asia/Tehran";

        var zoned = await SnapshotAsync(lab, airports: airports);
        var unzoned = await SnapshotAsync(lab);

        Assert.All(zoned.PointsOfSale.Single().Context.Flights, flight => Assert.Equal("Asia/Tehran", flight.OriginIanaTimeZoneId));
        Assert.All(unzoned.PointsOfSale.Single().Context.Flights, flight => Assert.Null(flight.OriginIanaTimeZoneId));
        Assert.DoesNotContain(zoned.Items, item => item.Candidate.ReasonCodes.Contains(ShoppingReasonCodes.TimeZoneNotVerified));
        Assert.Contains(unzoned.Items, item => item.Candidate.ReasonCodes.Contains(ShoppingReasonCodes.TimeZoneNotVerified));
        Assert.All(zoned.Items, item => Assert.Equal(OfferReadiness.NeedsSelection, item.Candidate.OfferReadiness));
    }
}
