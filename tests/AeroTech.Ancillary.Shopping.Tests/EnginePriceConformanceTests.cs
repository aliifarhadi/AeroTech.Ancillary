using AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Arguments;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Ancillary.Shopping.Selection;
using AeroTech.Ancillary.Shopping.Tests.Fixtures;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.ShoppingLab;

namespace AeroTech.Ancillary.Shopping.Tests;

public class EnginePriceConformanceTests
{
    private sealed record Priced(ShoppingLab Lab, AncillaryShoppingContext Context, CanonicalAncillaryOfferCandidate Shown)
    {
        public Task<CanonicalAncillarySelectionEvaluation> ChooseAsync(AncillarySelection selection)
            => Lab.Engine.EvaluateSelectionAsync(Context, Shown.CandidateIdentity, selection);
    }

    private static async Task<Priced> PricedAsync(
        string code,
        AncillaryPricingRateArgs[] rates,
        AncillaryShoppingContext? context = null,
        PricingUnit? pricingUnit = null,
        AncillaryQuantityUnit? quantityUnit = null,
        CommercialDisposition? disposition = null)
    {
        var lab = new ShoppingLab();
        var definition = lab.Definition(code, pricingUnit: pricingUnit);
        var provision = lab.Provision(definition, disposition: disposition, quantityUnit: quantityUnit);

        if (provision.PriceOrigin == PriceOrigin.Filed)
            lab.Price(definition, provision, rates);

        var trip = context ?? Trip.Context();

        return new Priced(lab, trip, (await lab.Engine.ShopAsync(trip, new ShoppingFilter { IncludeNonSellable = true })).Candidates[0]);
    }

    [Fact]
    public async Task P32B_ENG_01_the_unit_total_is_base_plus_added_tax_plus_item_fee_and_the_selected_amount_is_that_total_per_piece()
    {
        var priced = await PricedAsync("A01", [Rate(5_000m, components: [Tax("VAT", 450m, TaxTreatment.AddedToBase), Fee("SVC", 50m, FeeApplicationUnit.Item)])]);
        var searched = priced.Shown.Price;
        var two = await priced.ChooseAsync(new AncillarySelection { Quantity = 2 });
        var three = await priced.ChooseAsync(new AncillarySelection { Quantity = 3 });

        Assert.Equal(
            (PriceAssessmentStatus.Complete, PricingUnit.PerPiece, 5_000m, 5_500m, true),
            (searched.Status, searched.PricingUnit, searched.BaseAmount, searched.CompleteUnitTotal, searched.IsOrderLevelTotalComplete));
        Assert.Equal(("VAT", 450m), (searched.AddedTaxLines.Single().Code, searched.AddedTaxLines.Single().Amount));
        Assert.Equal(("SVC", 50m, FeeApplicationUnit.Item), (searched.AppliedUnitFeeLines.Single().Code, searched.AppliedUnitFeeLines.Single().Amount, searched.AppliedUnitFeeLines.Single().FeeApplicationUnit));
        Assert.Empty(searched.UnappliedFeeLines);
        Assert.Null(searched.RequestedQuantityTotal);
        Assert.Equal((SelectionEvaluationStatus.Accepted, 5_500m, 11_000m), (two.Status, two.Candidate.Price.CompleteUnitTotal, two.Candidate.Price.RequestedQuantityTotal));
        Assert.Equal((SelectionEvaluationStatus.Accepted, 16_500m), (three.Status, three.Candidate.Price.RequestedQuantityTotal));
        Assert.Null(priced.Shown.Price.RequestedQuantityTotal);
    }

    [Fact]
    public async Task P32B_ENG_02_an_included_tax_is_reported_and_never_added_also_when_a_quantity_is_selected()
    {
        var priced = await PricedAsync(
            "A01",
            [Rate(1_090m, components: [Tax("INC", 90m, TaxTreatment.IncludedInBase), Tax("VAT", 10m, TaxTreatment.AddedToBase)])]);
        var two = await priced.ChooseAsync(new AncillarySelection { Quantity = 2 });

        Assert.Equal((1_090m, 1_100m), (priced.Shown.Price.BaseAmount, priced.Shown.Price.CompleteUnitTotal));
        Assert.Equal(("INC", 90m), (priced.Shown.Price.IncludedTaxLines.Single().Code, priced.Shown.Price.IncludedTaxLines.Single().Amount));
        Assert.Equal(2_200m, two.Candidate.Price.RequestedQuantityTotal);
    }

    [Theory]
    [InlineData(FeeApplicationUnit.Ticket)]
    [InlineData(FeeApplicationUnit.OneWay)]
    [InlineData(FeeApplicationUnit.RoundTrip)]
    [InlineData(FeeApplicationUnit.SectorOrPortion)]
    public async Task P32B_ENG_03_a_fee_that_is_not_per_item_is_listed_outside_the_unit_total_and_blocks_any_selected_amount(FeeApplicationUnit unit)
    {
        var priced = await PricedAsync("A01", [Rate(5_000m, components: [Tax("VAT", 450m, TaxTreatment.AddedToBase), Fee("ORD", 700m, unit)])]);
        var two = await priced.ChooseAsync(new AncillarySelection { Quantity = 2 });

        Assert.Equal((PriceAssessmentStatus.Complete, 5_450m, false), (priced.Shown.Price.Status, priced.Shown.Price.CompleteUnitTotal, priced.Shown.Price.IsOrderLevelTotalComplete));
        Assert.Equal(("ORD", 700m, unit), (priced.Shown.Price.UnappliedFeeLines.Single().Code, priced.Shown.Price.UnappliedFeeLines.Single().Amount, priced.Shown.Price.UnappliedFeeLines.Single().FeeApplicationUnit));
        Assert.Empty(priced.Shown.Price.AppliedUnitFeeLines);
        Assert.Contains(ShoppingReasonCodes.FeeNotAppliedAtUnitLevel, priced.Shown.Price.ReasonCodes);
        Assert.Equal((5_450m, null, false), (two.Candidate.Price.CompleteUnitTotal, two.Candidate.Price.RequestedQuantityTotal, two.Candidate.Price.IsOrderLevelTotalComplete));
    }

    [Fact]
    public async Task P32B_ENG_04_a_free_or_a_quoted_or_a_not_available_service_carries_no_money()
    {
        var free = (await PricedAsync("A23", [])).Shown;
        var quoted = (await PricedAsync("A10", [])).Shown;
        var refused = (await PricedAsync("A24", [], disposition: CommercialDisposition.NotAvailable)).Shown;

        Assert.Equal((PriceOrigin.Free, PriceAssessmentStatus.Complete, true), (free.Price.Origin, free.Price.Status, free.Price.IsOrderLevelTotalComplete));
        Assert.Equal((PriceOrigin.ExternalQuote, PriceAssessmentStatus.QuoteRequired, OfferReadiness.NeedsQuote, Quote), (quoted.Price.Origin, quoted.Price.Status, quoted.OfferReadiness, quoted.Price.QuoteProviderKey));
        Assert.Equal((PriceOrigin.NotAvailable, PriceAssessmentStatus.Unavailable, OfferReadiness.Unavailable), (refused.Price.Origin, refused.Price.Status, refused.OfferReadiness));
        Assert.All(new[] { free, quoted, refused }, candidate =>
        {
            Assert.Equal((null, null, null, null), (candidate.Price.CurrencyId, candidate.Price.BaseAmount, candidate.Price.CompleteUnitTotal, candidate.Price.RequestedQuantityTotal));
            Assert.Empty(candidate.Price.AddedTaxLines);
            Assert.Empty(candidate.Price.IncludedTaxLines);
            Assert.Empty(candidate.Price.AppliedUnitFeeLines);
            Assert.Empty(candidate.Price.UnappliedFeeLines);
        });
        Assert.False(quoted.Price.IsOrderLevelTotalComplete);
    }

    [Fact]
    public async Task P32B_ENG_05_the_rate_of_the_target_currency_is_the_only_one_used_and_a_missing_one_is_never_replaced()
    {
        AncillaryPricingRateArgs[] rates = [Rate(40m, Eur), Rate(45.50m, Usd), Rate(12.345m, Kwd)];
        var euro = (await PricedAsync("A01", rates)).Shown.Price;
        var dollar = (await PricedAsync("A01", rates, Trip.Context(currencyId: Usd))).Shown.Price;
        var dinar = (await PricedAsync("A01", rates, Trip.Context(currencyId: Kwd))).Shown.Price;
        var yen = (await PricedAsync("A01", rates, Trip.Context(currencyId: Jpy))).Shown;

        Assert.Equal((Eur, 40m), (euro.CurrencyId, euro.CompleteUnitTotal));
        Assert.Equal((Usd, 45.50m), (dollar.CurrencyId, dollar.CompleteUnitTotal));
        Assert.Equal((Kwd, 12.345m), (dinar.CurrencyId, dinar.CompleteUnitTotal));
        Assert.Equal((PriceAssessmentStatus.Unavailable, null, null), (yen.Price.Status, yen.Price.CurrencyId, yen.Price.CompleteUnitTotal));
        Assert.Contains(ShoppingReasonCodes.RateNotFound, yen.Price.ReasonCodes);
        Assert.Equal(OfferReadiness.Unavailable, yen.OfferReadiness);
    }

    [Fact]
    public async Task P32B_ENG_06_a_weight_package_is_charged_per_package_and_its_kilograms_never_multiply_the_price()
    {
        var priced = await PricedAsync("A02", [Rate(800m)], Trip.Context() with { TravellerBaggageFacts = [Trip.WeightAllowance()] });
        var two = await priced.ChooseAsync(new AncillarySelection { PackageProductRef = "PACK10", Quantity = 2 });

        Assert.Equal((PricingUnit.PerItem, AncillaryQuantityUnit.Each, 800m), (priced.Shown.Price.PricingUnit, priced.Shown.Quantity.Unit, priced.Shown.Price.CompleteUnitTotal));
        Assert.Equal((SelectionEvaluationStatus.Accepted, 1_600m), (two.Status, two.Candidate.Price.RequestedQuantityTotal));
    }

    [Fact]
    public async Task P32B_ENG_07_a_per_kilogram_rate_has_no_selected_amount_while_the_chargeable_weight_is_not_defined()
    {
        var priced = await PricedAsync("A03", [Rate(250m)], pricingUnit: PricingUnit.PerKilogram, quantityUnit: AncillaryQuantityUnit.Kilogram);
        var measured = await priced.ChooseAsync(new AncillarySelection { BagRef = "BAG-1", MeasuredWeightKg = 27m });
        var forced = await priced.ChooseAsync(new AncillarySelection { BagRef = "BAG-1", MeasuredWeightKg = 27m, Quantity = 4 });

        Assert.Equal((PricingUnit.PerKilogram, AncillaryQuantityUnit.Kilogram, 250m), (priced.Shown.Price.PricingUnit, priced.Shown.Quantity.Unit, priced.Shown.Price.CompleteUnitTotal));
        Assert.All(new[] { priced.Shown.Price, measured.Candidate.Price, forced.Candidate.Price }, price =>
        {
            Assert.Equal((250m, null, false), (price.CompleteUnitTotal, price.RequestedQuantityTotal, price.IsOrderLevelTotalComplete));
            Assert.Contains(ShoppingReasonCodes.ChargeableWeightNotDetermined, price.ReasonCodes);
        });
        Assert.Equal(SelectionEvaluationStatus.Rejected, forced.Status);
        Assert.Contains(forced.FieldIssues, issue => issue is { Field: "Quantity", ReasonCode: ShoppingReasonCodes.SelectionFieldNotAllowed });
    }

    [Fact]
    public async Task P32B_ENG_08_the_searched_price_is_a_unit_rate_and_only_an_evaluated_selection_carries_an_amount()
    {
        var priced = await PricedAsync("A01", [Rate(5_000m, components: [Tax("VAT", 450m, TaxTreatment.AddedToBase)])]);
        var again = (await priced.Lab.Engine.ShopAsync(priced.Context)).Candidates[0];
        var one = await priced.ChooseAsync(new AncillarySelection { Quantity = 1 });
        var none = await priced.ChooseAsync(new AncillarySelection());

        Assert.Equal((5_450m, null), (priced.Shown.Price.CompleteUnitTotal, priced.Shown.Price.RequestedQuantityTotal));
        Assert.Equal(priced.Shown.Price.PricingRevisionId, again.Price.PricingRevisionId);
        Assert.Equal((SelectionEvaluationStatus.Accepted, 5_450m, 5_450m, false), (one.Status, one.Candidate.Price.CompleteUnitTotal, one.Candidate.Price.RequestedQuantityTotal, one.ProvisionChangedBySelection));
        Assert.Equal((SelectionEvaluationStatus.Incomplete, null), (none.Status, none.Candidate.Price.RequestedQuantityTotal));
        Assert.False(one.Candidate.Availability.IsGuaranteed);
    }

    [Fact]
    public async Task P32B_ENG_09_only_an_accepted_selection_carries_an_amount()
    {
        var bag = await PricedAsync("A01", [Rate(5_000m, components: [Tax("VAT", 450m, TaxTreatment.AddedToBase)])]);
        var package = await PricedAsync("A02", [Rate(800m)], Trip.Context() with { TravellerBaggageFacts = [Trip.WeightAllowance()] });
        var accepted = await bag.ChooseAsync(new AncillarySelection { Quantity = 2 });
        var rejected = await bag.ChooseAsync(new AncillarySelection { Quantity = 99 });
        var incomplete = await package.ChooseAsync(new AncillarySelection { Quantity = 1 });

        Assert.Equal((SelectionEvaluationStatus.Accepted, 10_900m), (accepted.Status, accepted.Candidate.Price.RequestedQuantityTotal));
        Assert.Equal((SelectionEvaluationStatus.Rejected, 5_450m, null), (rejected.Status, rejected.Candidate.Price.CompleteUnitTotal, rejected.Candidate.Price.RequestedQuantityTotal));
        Assert.Equal((SelectionEvaluationStatus.Incomplete, 800m, null), (incomplete.Status, incomplete.Candidate.Price.CompleteUnitTotal, incomplete.Candidate.Price.RequestedQuantityTotal));
    }
}
