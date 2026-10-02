using AeroTech.Ancillary.Domain.AncillaryPriceRuleAggregate;
using AeroTech.Ancillary.Domain.AncillaryProductAggregate;
using AeroTech.Ancillary.Domain.AncillaryQuote;
using AeroTech.Ancillary.Domain.ConformanceTests.Fixtures;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;
using static AeroTech.Ancillary.Domain.ConformanceTests.Fixtures.Quotes;

namespace AeroTech.Ancillary.Domain.ConformanceTests;

public sealed class Phase1AncillaryQuoteConformanceTests
{
    private readonly QuoteWorld _world = new();

    [Fact]
    public void P1_Q01_GoldenSelectionIsPricedAsDocumented()
    {
        var product = _world.AddProduct(ProductSpec.X);
        var rule = _world.AddRule(RuleSpec.R);

        var result = _world.Quote(Golden(Selection(product, rule)));

        Assert.Equal((978, AsOf), (result.CurrencyId, result.AsOf));
        AssertGoldenItem(Assert.Single(result.Items), product, rule);
    }

    [Fact]
    public void P1_Q02_GoldenCatalogueReturnsTheSameItem()
    {
        var product = _world.AddProduct(ProductSpec.X);
        var rule = _world.AddRule(RuleSpec.R);

        var item = Assert.Single(_world.Quote(Golden()).Items);

        AssertGoldenItem(item, product, rule);
        Assert.Equal((1, rule.Id), (item.Product.Version, item.PriceRuleId));
    }

    [Fact]
    public void P1_Q03_CatalogueOrderIsTravellerThenBoundThenProductRef()
    {
        var request = Request(
            [Traveller("T1", "ADT", "F1", "F2"), Traveller("T2", "ADT", "F1", "F2")],
            [Bound("B1", Flight("F1")), Bound("B2", Flight("F2", 2, 1))]);

        _world.AddProduct(ProductSpec.X);
        _world.AddRule(RuleSpec.R);

        Assert.Equal(
            [("XBAG1", "T1", "B1"), ("XBAG1", "T1", "B2"), ("XBAG1", "T2", "B1"), ("XBAG1", "T2", "B2")],
            _world.Quote(request).Items.Select(Occurrence));

        _world.AddRule(RuleSpec.RG);
        _world.AddProduct(ProductSpec.G);

        Assert.Equal(
            [
                ("XBAG1", "T1", "B1"), ("XBAGG", "T1", "B1"),
                ("XBAG1", "T1", "B2"), ("XBAGG", "T1", "B2"),
                ("XBAG1", "T2", "B1"), ("XBAGG", "T2", "B1"),
                ("XBAG1", "T2", "B2"), ("XBAGG", "T2", "B2")
            ],
            _world.Quote(request).Items.Select(Occurrence));
    }

    [Fact]
    public void P1_Q04_BoundCoveringTwoFlightsIsNotOffered()
    {
        var product = _world.AddProduct(ProductSpec.X);
        var rule = _world.AddRule(RuleSpec.R);
        IReadOnlyList<AncillaryQuoteTraveller> travellers = [Traveller("T1", "ADT", "F1", "F2", "F3"), Traveller("T2", "ADT", "F3")];
        IReadOnlyList<AncillaryQuoteBound> bounds = [Bound("B1", Flight("F1"), Flight("F2", 2, 3)), Bound("B2", Flight("F3", 3, 1))];

        Assert.Equal(
            [("XBAG1", "T1", "B2"), ("XBAG1", "T2", "B2")],
            _world.Quote(Request(travellers, bounds)).Items.Select(Occurrence));
        BusinessAssert.Throws(16305, 422, () => _world.Quote(Request(travellers, bounds, [Selection(product, rule, "T1", "B1")])));
        Assert.Single(_world.Quote(Request(travellers, bounds, [Selection(product, rule, "T1", "B2")])).Items);
    }

    [Fact]
    public void P1_Q05_BoundOfAnotherMarketingAirlineIsNotOffered()
    {
        _world.AddProduct(ProductSpec.X);
        _world.AddRule(RuleSpec.R);

        var result = _world.Quote(Request(
            [Traveller("T1", "ADT", "F1", "F2")],
            [Bound("B1", Flight("F1", marketingAirlineId: 11)), Bound("B2", Flight("F2", 2, 1))]));

        Assert.Equal([("XBAG1", "T1", "B2")], result.Items.Select(Occurrence));
    }

    [Fact]
    public void P1_Q06_OtherCurrencyGivesAnEmptyCatalogue()
    {
        _world.AddProduct(ProductSpec.X);
        _world.AddRule(RuleSpec.R);

        var result = _world.Quote(Request([Traveller("T1", "ADT", "F1")], [Bound("B1", Flight("F1"))], currencyId: 840));

        Assert.Equal(840, result.CurrencyId);
        Assert.Empty(result.Items);
    }

    [Fact]
    public void P1_Q07_LowestPriorityNumberWins()
    {
        _world.AddProduct(ProductSpec.X);
        _world.AddRule(RuleSpec.R with { Priority = 2, Lines = [RuleSpec.Ancillary(20.00m)] });
        var first = _world.AddRule(RuleSpec.R with { Priority = 1, Lines = [RuleSpec.Ancillary(35.00m)] });

        var item = Assert.Single(_world.Quote(Golden()).Items);

        Assert.Equal((first.Id, 35.00m), (item.PriceRuleId, item.Total));
    }

    [Fact]
    public void P1_Q08_PassengerTypeConditionSelectsTheRule()
    {
        _world.AddProduct(ProductSpec.X);
        var child = _world.AddRule(RuleSpec.R with { Priority = 1, Lines = [RuleSpec.Ancillary(20.00m)], PassengerTypes = [PassengerTypeCode.CHD] });
        var anyone = _world.AddRule(RuleSpec.R with { Priority = 2 });

        var items = _world.Quote(Request(
            [Traveller("T1", "ADT", "F1"), Traveller("T2", "CHD", "F1")],
            [Bound("B1", Flight("F1"))])).Items;

        Assert.Equal([("T1", anyone.Id, 38.50m), ("T2", child.Id, 20.00m)], items.Select(item => (item.TravellerRef, item.PriceRuleId, item.Total)));
    }

    [Fact]
    public void P1_Q09_SalesWindowIsComparedWithAsOfOnly()
    {
        var salesFrom = Instant("2026-10-01T00:00:00+00:00");
        var salesTo = Instant("2026-10-02T00:00:00+00:00");

        _world.AddProduct(ProductSpec.X);
        _world.AddRule(RuleSpec.R with { SalesFrom = salesFrom, SalesTo = salesTo });

        Assert.True(_world.Clock > salesTo.AddYears(4));
        Assert.Single(Catalogue(salesFrom));
        Assert.Single(Catalogue(salesTo.AddSeconds(-1)));
        Assert.Empty(Catalogue(salesTo));
        Assert.Empty(Catalogue(salesFrom.AddSeconds(-1)));
    }

    [Fact]
    public void P1_Q10_TravelWindowUsesTheLocalDepartureDate()
    {
        var day = new DateOnly(2026, 10, 10);

        _world.AddProduct(ProductSpec.X);
        _world.AddRule(RuleSpec.R with { TravelFrom = day, TravelTo = day });

        Assert.Single(_world.Quote(Request([Traveller("T1", "ADT", "F1")], [Bound("B1", Flight("F1", departure: "2026-10-10T23:30:00+03:30"))])).Items);
        Assert.Empty(_world.Quote(Request([Traveller("T1", "ADT", "F1")], [Bound("B1", Flight("F1", departure: "2026-10-11T00:30:00+03:30"))])).Items);
        Assert.Single(_world.Quote(Request([Traveller("T1", "ADT", "F1")], [Bound("B1", Flight("F1", departure: "2026-10-10T00:30:00+03:30"))])).Items);
    }

    [Fact]
    public void P1_Q11_OriginAndDestinationConditionsLookAtTheCoveredFlight()
    {
        _world.AddProduct(ProductSpec.X);
        var fallback = _world.AddRule(RuleSpec.R with { Priority = 2 });
        var matching = _world.AddRule(RuleSpec.R with { Priority = 1, OriginAirportIds = [1], DestinationAirportIds = [2] });

        Assert.Equal(matching.Id, Assert.Single(_world.Quote(Golden()).Items).PriceRuleId);

        matching.Retire();
        _world.AddRule(RuleSpec.R with { Priority = 1, OriginAirportIds = [2] });

        Assert.Equal(fallback.Id, Assert.Single(_world.Quote(Golden()).Items).PriceRuleId);

        _world.Rules.Single(rule => rule.Priority == 1 && rule.Status == AncillaryPriceRuleStatus.Active).Retire();
        _world.AddRule(RuleSpec.R with { Priority = 1, DestinationAirportIds = [1] });

        Assert.Equal(fallback.Id, Assert.Single(_world.Quote(Golden()).Items).PriceRuleId);
    }

    [Fact]
    public void P1_Q12_OnlyActiveProductsAndRulesAreUsed()
    {
        var draft = _world.AddDraftProduct(ProductSpec.X);
        var rule = _world.AddRule(RuleSpec.R);

        Assert.Empty(_world.Quote(Golden()).Items);

        draft.Activate(SubCodes.Industry(), _world.Clock);

        Assert.Single(_world.Quote(Golden()).Items);

        draft.Suspend();

        Assert.Empty(_world.Quote(Golden()).Items);

        draft.Retire(_world.Clock);

        Assert.Empty(_world.Quote(Golden()).Items);

        var world = new QuoteWorld();

        world.AddProduct(ProductSpec.X);
        var draftRule = world.AddDraftRule(RuleSpec.R);

        Assert.Empty(world.Quote(Golden()).Items);

        draftRule.Activate();

        Assert.Single(world.Quote(Golden()).Items);

        draftRule.Suspend();

        Assert.Empty(world.Quote(Golden()).Items);

        draftRule.Retire();

        Assert.Empty(world.Quote(Golden()).Items);
        Assert.Equal(AncillaryPriceRuleStatus.Active, rule.Status);
    }

    [Fact]
    public void P1_Q13_RevisedVersionReplacesTheOfferedOne()
    {
        var first = _world.AddProduct(ProductSpec.X);
        var rule = _world.AddRule(RuleSpec.R);
        var second = _world.AddRevision(first, ProductSpec.X with { Name = "First extra bag" });

        var item = Assert.Single(_world.Quote(Golden()).Items);

        Assert.Equal((2, "First extra bag", rule.Id), (item.Product.Version, item.Product.Name, item.PriceRuleId));
        BusinessAssert.Throws(16309, 409, () => _world.Quote(Golden(Selection(first, rule))));
        Assert.Single(_world.Quote(Golden(Selection(second, rule))).Items);
    }

    [Fact]
    public void P1_Q14_ReplacedRuleMakesTheSelectionStale()
    {
        var product = _world.AddProduct(ProductSpec.X);
        var old = _world.AddRule(RuleSpec.R);

        old.Retire();
        var current = _world.AddRule(RuleSpec.R with { Lines = [RuleSpec.Ancillary(40.00m)] });

        BusinessAssert.Throws(16309, 409, () => _world.Quote(Golden(Selection(product, old))));

        var item = Assert.Single(_world.Quote(Golden(Selection(product, current))).Items);

        Assert.Equal((current.Id, 40.00m), (item.PriceRuleId, item.Total));
    }

    [Fact]
    public void P1_Q15_OnlyTheFlightsTheTravellerFliesAreCovered()
    {
        _world.AddProduct(ProductSpec.X);
        _world.AddRule(RuleSpec.R with { Priority = 1, Lines = [RuleSpec.Ancillary(20.00m)], OriginAirportIds = [1] });
        var anywhere = _world.AddRule(RuleSpec.R with { Priority = 2 });
        IReadOnlyList<AncillaryQuoteBound> bounds = [Bound("B1", Flight("F1"), Flight("F2", 2, 3)), Bound("B2", Flight("F3", 3, 1))];

        var items = _world.Quote(Request(
            [Traveller("T1", "ADT", "F1", "F2"), Traveller("T2", "ADT", "F2"), Traveller("T3", "ADT", "F3")],
            bounds)).Items;

        Assert.Equal([("XBAG1", "T2", "B1"), ("XBAG1", "T3", "B2")], items.Select(Occurrence));
        Assert.Equal(["F2"], items[0].CoveredFlightRefs);
        Assert.Equal(anywhere.Id, items[0].PriceRuleId);
        Assert.Equal(["F3"], items[1].CoveredFlightRefs);
        BusinessAssert.Throws(16301, 422, () => _world.Quote(Request([Traveller("T1", "ADT", "F9")], bounds)));
    }

    [Fact]
    public void P1_Q16_SelectionOfAnUnknownProductIsRefused()
    {
        _world.AddProduct(ProductSpec.X);
        var rule = _world.AddRule(RuleSpec.R);

        BusinessAssert.Throws(16302, 422, () => _world.Quote(Golden(new AncillaryQuoteSelection("NOSUCH", 1, rule.Id, "T1", "B1", null, 1))));
    }

    [Fact]
    public void P1_Q17_SelectionMustFitTheBoundScope()
    {
        var product = _world.AddProduct(ProductSpec.X);
        var rule = _world.AddRule(RuleSpec.R);

        BusinessAssert.Throws(16303, 422, () => _world.Quote(Golden(Selection(product, rule, flightRef: "F1"))));
        BusinessAssert.Throws(16303, 422, () => _world.Quote(Golden(Selection(product, rule, boundRef: null))));
        BusinessAssert.Throws(16303, 422, () => _world.Quote(Golden(Selection(product, rule, boundRef: null, flightRef: "F1"))));
    }

    [Fact]
    public void P1_Q18_SelectionReferencesMustBeInTheRequest()
    {
        var product = _world.AddProduct(ProductSpec.X);
        var rule = _world.AddRule(RuleSpec.R);

        BusinessAssert.Throws(16304, 422, () => _world.Quote(Golden(Selection(product, rule, travellerRef: "T9"))));
        BusinessAssert.Throws(16304, 422, () => _world.Quote(Golden(Selection(product, rule, boundRef: "B9"))));
        BusinessAssert.Throws(16304, 422, () => _world.Quote(Golden(Selection(product, rule, flightRef: "F9"))));
    }

    [Fact]
    public void P1_Q19_SelectionOfAnOccurrenceThatIsNotApplicableIsRefused()
    {
        var product = _world.AddProduct(ProductSpec.X);
        var rule = _world.AddRule(RuleSpec.R);

        BusinessAssert.Throws(16305, 422, () => _world.Quote(Request(
            [Traveller("T1", "ADT", "F1")],
            [Bound("B1", Flight("F1", marketingAirlineId: 11))],
            [Selection(product, rule)])));
        BusinessAssert.Throws(16305, 422, () => _world.Quote(Request(
            [Traveller("T1", "ADT", "F1")],
            [Bound("B1", Flight("F1"))],
            [Selection(product, rule)],
            currencyId: 840)));
        BusinessAssert.Throws(16305, 422, () => _world.Quote(Request(
            [Traveller("T1", "ADT", "F1"), Traveller("T2", "ADT", "F2")],
            [Bound("B1", Flight("F1")), Bound("B2", Flight("F2", 2, 1))],
            [Selection(product, rule, "T2", "B1")])));
    }

    [Fact]
    public void P1_Q20_SelectedQuantityMustBeWithinTheProductLimits()
    {
        var first = _world.AddProduct(ProductSpec.X);
        var firstRule = _world.AddRule(RuleSpec.R);
        var generic = _world.AddProduct(ProductSpec.G);
        var genericRule = _world.AddRule(RuleSpec.RG);

        BusinessAssert.Throws(16306, 422, () => _world.Quote(Golden(Selection(first, firstRule, quantity: 2))));

        var item = Assert.Single(_world.Quote(Golden(Selection(generic, genericRule, quantity: 2))).Items);
        var line = Assert.Single(item.PriceLines);

        Assert.Equal((2, 2, 30.00m, 60.00m, 30.00m, 60.00m), (item.Quantity, item.MaxQuantity, line.UnitAmount, line.Amount, item.UnitTotal, item.Total));
        BusinessAssert.Throws(16306, 422, () => _world.Quote(Golden(Selection(generic, genericRule, quantity: 3))));
        BusinessAssert.Throws(16306, 422, () => _world.Quote(Golden(Selection(generic, genericRule, quantity: 0))));
    }

    [Fact]
    public void P1_Q21_SameOccurrenceSelectedTwiceIsRefused()
    {
        var generic = _world.AddProduct(ProductSpec.G);
        var rule = _world.AddRule(RuleSpec.RG);

        BusinessAssert.Throws(16307, 422, () => _world.Quote(Golden(Selection(generic, rule), Selection(generic, rule))));
    }

    [Fact]
    public void P1_Q22_FirstFailingSelectionFailsTheWholeRequest()
    {
        var product = _world.AddProduct(ProductSpec.X);
        var rule = _world.AddRule(RuleSpec.R);
        var unknown = new AncillaryQuoteSelection("NOSUCH", 1, rule.Id, "T1", "B1", null, 1);

        BusinessAssert.Throws(16302, 422, () => _world.Quote(Golden(Selection(product, rule), unknown)));
        BusinessAssert.Throws(16306, 422, () => _world.Quote(Golden(Selection(product, rule, quantity: 2), unknown)));
    }

    [Fact]
    public void P1_Q23_InconsistentRequestIsRefused()
    {
        _world.AddProduct(ProductSpec.X);
        _world.AddRule(RuleSpec.R);

        BusinessAssert.Throws(16301, 422, () => _world.Quote(Request(
            [Traveller("T1", "ADT", "F1"), Traveller("T1", "ADT", "F1")],
            [Bound("B1", Flight("F1"))])));
        BusinessAssert.Throws(16301, 422, () => _world.Quote(Request(
            [Traveller("T1", "ADT", "F1")],
            [Bound("B1", Flight("F1")), Bound("B2", Flight("F1", 2, 1))])));
        BusinessAssert.Throws(16301, 422, () => _world.Quote(Request(
            [Traveller("T1", "ADT", "F1")],
            [Bound("B1", Flight("F1")), Bound("B1", Flight("F2", 2, 1))])));
        BusinessAssert.Throws(16301, 422, () => _world.Quote(Request(
            [Traveller("T1", "XXX", "F1")],
            [Bound("B1", Flight("F1"))])));
        BusinessAssert.Throws(16301, 422, () => _world.Quote(Request(
            [Traveller("T1", "ADT", "F1", "F1")],
            [Bound("B1", Flight("F1"))])));
    }

    [Fact]
    public void P1_Q24_QuoteChangesNothingAndIsRepeatable()
    {
        var product = _world.AddProduct(ProductSpec.G);
        var rule = _world.AddRule(RuleSpec.RG);
        var before = State(product, rule);

        var first = _world.Quote(Golden(Selection(product, rule, quantity: 2)));
        var second = _world.Quote(Golden(Selection(product, rule, quantity: 2)));

        Assert.Equal(before, State(product, rule));
        Assert.Empty(product.GetEvents());
        Assert.Empty(rule.GetEvents());
        Assert.Equal(
            first.Items.Select(item => (Occurrence(item), item.Quantity, item.PriceRuleId, item.Total)),
            second.Items.Select(item => (Occurrence(item), item.Quantity, item.PriceRuleId, item.Total)));
    }

    [Fact]
    public void P1_Q25_AncillaryLineIsNamedAfterTheProductWhenItHasNoName()
    {
        _world.AddProduct(ProductSpec.X);
        var named = _world.AddRule(RuleSpec.R with { Lines = [RuleSpec.Ancillary(35.00m, "Bag fee"), RuleSpec.Tax("VAT", 3.50m)] });

        Assert.Equal(["Bag fee", null], Assert.Single(_world.Quote(Golden()).Items).PriceLines.Select(line => line.Name));

        named.Retire();
        _world.AddRule(RuleSpec.R with { Lines = [RuleSpec.Ancillary(35.00m), RuleSpec.Tax("VAT", 3.50m)] });

        Assert.Equal(["First extra bag 23kg", null], Assert.Single(_world.Quote(Golden()).Items).PriceLines.Select(line => line.Name));
    }

    [Fact]
    public void P1_Q26_AncillaryLineComesFirstAndTotalsIncludeEveryLine()
    {
        _world.AddProduct(ProductSpec.X);
        _world.AddRule(RuleSpec.R with { Lines = [RuleSpec.Tax("VAT", 3.50m), RuleSpec.Tax("YQ", 1.25m), RuleSpec.Ancillary(35.00m)] });

        var item = Assert.Single(_world.Quote(Golden()).Items);

        Assert.Equal(
            [(AncillaryPriceLineCategory.Ancillary, null, 35.00m), (AncillaryPriceLineCategory.Tax, "VAT", 3.50m), (AncillaryPriceLineCategory.Tax, "YQ", 1.25m)],
            item.PriceLines.Select(line => (line.Category, line.Code, line.Amount)));
        Assert.Equal((39.75m, 39.75m), (item.UnitTotal, item.Total));
    }

    [Fact]
    public void P1_Q27_ExistingPurchaseRemovesTheOccurrence()
    {
        var product = _world.AddProduct(ProductSpec.X);
        var rule = _world.AddRule(RuleSpec.R);
        IReadOnlyList<AncillaryQuoteTraveller> travellers = [Traveller("T1", "ADT", "F1"), Traveller("T2", "ADT", "F1")];
        IReadOnlyList<AncillaryQuoteBound> bounds = [Bound("B1", Flight("F1"))];
        IReadOnlyList<AncillaryQuoteExistingOccurrence> existing = [Existing("XBAG1", "T1", "B1", 1)];

        Assert.Equal([("XBAG1", "T2", "B1")], _world.Quote(Request(travellers, bounds, existing: existing)).Items.Select(Occurrence));
        BusinessAssert.Throws(16305, 422, () => _world.Quote(Request(travellers, bounds, [Selection(product, rule)], existing)));
        Assert.Single(_world.Quote(Request(travellers, bounds, [Selection(product, rule, "T2")], existing)).Items);
    }

    [Fact]
    public void P1_Q28_ExistingPurchaseReducesTheRemainingQuantity()
    {
        var generic = _world.AddProduct(ProductSpec.G);
        var rule = _world.AddRule(RuleSpec.RG);
        IReadOnlyList<AncillaryQuoteTraveller> travellers = [Traveller("T1", "ADT", "F1")];
        IReadOnlyList<AncillaryQuoteBound> bounds = [Bound("B1", Flight("F1"))];
        IReadOnlyList<AncillaryQuoteExistingOccurrence> one = [Existing("XBAGG", "T1", "B1", 1)];
        IReadOnlyList<AncillaryQuoteExistingOccurrence> two = [Existing("XBAGG", "T1", "B1", 1), Existing("XBAGG", "T1", "B1", 1)];

        var item = Assert.Single(_world.Quote(Request(travellers, bounds, existing: one)).Items);

        Assert.Equal((1, 1, 1), (item.Product.Quantity.Min, item.MaxQuantity, item.Quantity));
        BusinessAssert.Throws(16306, 422, () => _world.Quote(Request(travellers, bounds, [Selection(generic, rule, quantity: 2)], one)));
        Assert.Equal(30.00m, Assert.Single(_world.Quote(Request(travellers, bounds, [Selection(generic, rule)], one)).Items).Total);
        Assert.Empty(_world.Quote(Request(travellers, bounds, existing: two)).Items);
        Assert.Empty(_world.Quote(Request(travellers, bounds, existing: [Existing("XBAGG", "T1", "B1", 2)])).Items);
    }

    [Fact]
    public void P1_Q29_ExistingEntriesMustReferToTheRequest()
    {
        _world.AddProduct(ProductSpec.X);
        _world.AddRule(RuleSpec.R);
        IReadOnlyList<AncillaryQuoteTraveller> travellers = [Traveller("T1", "ADT", "F1")];
        IReadOnlyList<AncillaryQuoteBound> bounds = [Bound("B1", Flight("F1"))];

        BusinessAssert.Throws(16304, 422, () => _world.Quote(Request(travellers, bounds, existing: [Existing("XBAG1", "T9", "B1", 1)])));
        BusinessAssert.Throws(16304, 422, () => _world.Quote(Request(travellers, bounds, existing: [Existing("XBAG1", "T1", "B9", 1)])));

        var ignored = _world.Quote(Request(travellers, bounds, existing: [Existing("NOSUCH", "T1", "B1", 1)])).Items;
        var none = _world.Quote(Request(travellers, bounds)).Items;

        Assert.Equal(none.Select(Occurrence), ignored.Select(Occurrence));
        Assert.Equal(1, Assert.Single(none).MaxQuantity);
    }

    private IReadOnlyList<AncillaryQuoteItem> Catalogue(DateTimeOffset asOf)
        => _world.Quote(Request([Traveller("T1", "ADT", "F1")], [Bound("B1", Flight("F1"))], asOf: asOf)).Items;

    private static (AncillaryProductStatus, int, string, AncillaryPriceRuleStatus, int, int) State(AncillaryProduct product, AncillaryPriceRule rule)
        => (product.Status, product.Version, product.Name, rule.Status, rule.Priority, rule.Lines.Count);

    private static void AssertGoldenItem(AncillaryQuoteItem item, AncillaryProduct product, AncillaryPriceRule rule)
    {
        Assert.Same(product, item.Product);
        Assert.Equal((10, "XBAG1", 1, AncillaryProductType.ExtraBaggage), (item.Product.OwnerAirlineId, item.Product.ProductRef, item.Product.Version, item.Product.Type));
        Assert.Equal(("First extra bag 23kg", null, AncillarySalesScope.TravellerBound), (item.Product.Name, item.Product.Description, item.Product.SalesScope));
        Assert.Equal(("T1", "B1"), (item.TravellerRef, item.BoundRef));
        Assert.Equal(["F1"], item.CoveredFlightRefs);
        Assert.Equal((AncillaryQuantityUnit.Piece, 1, 1, 1), (item.Product.Quantity.Unit, item.Product.Quantity.Min, item.MaxQuantity, item.Quantity));
        Assert.Equal(
            ("C", "BG", null, "B1", null),
            (item.Product.Codes.ServiceTypeCode, item.Product.Codes.GroupCode, item.Product.Codes.SubGroupCode, item.Product.Codes.Description1Code, item.Product.Codes.Description2Code));
        Assert.Equal((1, 23m, AncillaryWeightUnit.Kg), (item.Product.Baggage!.Pieces, item.Product.Baggage.Weight, item.Product.Baggage.WeightUnit));
        Assert.Equal((AncillaryDocumentType.EmdAssociated, "C", "0CC"), (item.Product.Document.Type, item.Product.Document.Rfic, item.Product.Document.Rfisc));
        Assert.Equal(AncillaryInventoryControl.Unlimited, item.Product.InventoryControl);
        Assert.Equal(rule.Id, item.PriceRuleId);
        Assert.Equal(
            [
                (AncillaryPriceLineCategory.Ancillary, null, "First extra bag 23kg", 35.00m, 35.00m),
                (AncillaryPriceLineCategory.Tax, "VAT", "Value added tax", 3.50m, 3.50m)
            ],
            item.PriceLines.Select(line => (line.Category, line.Code, line.Name, line.UnitAmount, line.Amount)));
        Assert.Equal((38.50m, 38.50m), (item.UnitTotal, item.Total));
    }
}
