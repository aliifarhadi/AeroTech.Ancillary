using AeroTech.Ancillary.Domain.AncillaryPriceRuleAggregate;
using AeroTech.Ancillary.Domain.AncillaryProductAggregate;
using AeroTech.Ancillary.Domain.AncillaryQuote;
using AeroTech.Ancillary.Domain.ConformanceTests.Fixtures;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;
using static AeroTech.Ancillary.Domain.ConformanceTests.Fixtures.Quotes;

namespace AeroTech.Ancillary.Domain.ConformanceTests;

public sealed class Phase2AncillaryQuoteConformanceTests
{
    private readonly QuoteWorld _world = new();
    private readonly AncillaryProduct _bag;
    private readonly AncillaryProduct _lounge;
    private readonly AncillaryPriceRule _bagRule;
    private readonly AncillaryPriceRule _loungeRule;

    public Phase2AncillaryQuoteConformanceTests()
    {
        _bag = _world.AddProduct(ProductSpec.X);
        _bagRule = _world.AddRule(RuleSpec.R);
        _lounge = _world.AddProduct(ProductSpec.L);
        _loungeRule = _world.AddRule(RuleSpec.RL);
    }

    [Fact]
    public void P2_Q01_GoldenExampleGivesTheDocumentedItemsInOrder()
    {
        var result = _world.Quote(LoungeGolden());

        Assert.Equal(
            [("XBAG1", "T1", "B1", null), ("LNGTHR", "T1", "B1", "F1"), ("XBAG1", "T1", "B2", null)],
            result.Items.Select(Position));

        var lounge = result.Items[1];
        var line = Assert.Single(lounge.PriceLines);

        Assert.Same(_lounge, lounge.Product);
        Assert.Equal(["F1"], lounge.CoveredFlightRefs);
        Assert.Equal((1, 1, 1, _loungeRule.Id), (lounge.Product.Quantity.Min, lounge.MaxQuantity, lounge.Quantity, lounge.PriceRuleId));
        Assert.Equal((AncillaryProductType.LoungeAccess, AncillarySalesScope.TravellerSegment, AncillaryQuantityUnit.Each), (lounge.Product.Type, lounge.Product.SalesScope, lounge.Product.Quantity.Unit));
        Assert.Equal((AncillaryDocumentType.EmdStandalone, "E", "0BX"), (lounge.Product.Document.Type, lounge.Product.Document.Rfic, lounge.Product.Document.Rfisc));
        Assert.Equal(("F", "LG"), (lounge.Product.Codes.ServiceTypeCode, lounge.Product.Codes.GroupCode));
        Assert.Equal([1], lounge.Product.Lounge!.AirportIds);
        Assert.Null(lounge.Product.Baggage);
        Assert.Equal((AncillaryPriceLineCategory.Ancillary, null, "Lounge access", 20.00m, 20.00m), (line.Category, line.Code, line.Name, line.UnitAmount, line.Amount));
        Assert.Equal((20.00m, 20.00m), (lounge.UnitTotal, lounge.Total));
        Assert.Equal([38.50m, 38.50m], new[] { result.Items[0], result.Items[2] }.Select(item => item.Total));
        Assert.All(new[] { result.Items[0], result.Items[2] }, item => Assert.Null(item.Product.Lounge));
        Assert.Equal([["F1"], ["F2"]], new[] { result.Items[0], result.Items[2] }.Select(item => item.CoveredFlightRefs));
    }

    [Fact]
    public void P2_Q02_TwoFlightBoundGivesOnlyTheLoungeOfItsFirstFlight()
    {
        var result = _world.Quote(Request(
            [Traveller("T1", "ADT", "F1", "F2")],
            [Bound("B1", Flight("F1"), Flight("F2", 2, 3))]));

        Assert.Equal([("LNGTHR", "T1", "B1", "F1")], result.Items.Select(Position));
    }

    [Fact]
    public void P2_Q03_TravellerOnTheSecondFlightGetsTheBagAndNoLounge()
    {
        var result = _world.Quote(Request(
            [Traveller("T1", "ADT", "F1", "F2"), Traveller("T2", "ADT", "F2")],
            [Bound("B1", Flight("F1"), Flight("F2", 2, 3))]));

        Assert.Equal([("LNGTHR", "T1", "B1", "F1"), ("XBAG1", "T2", "B1", null)], result.Items.Select(Position));
        Assert.Equal(["F2"], result.Items[1].CoveredFlightRefs);
    }

    [Fact]
    public void P2_Q04_ItemsAreOrderedByBoundThenBagThenFlight()
    {
        _world.AddProduct(ProductSpec.LG);
        _world.AddRule(RuleSpec.RLG);

        Assert.Equal(
            [("XBAG1", "T1", "B1", null), ("LNGTHR", "T1", "B1", "F1"), ("LNGXLG", "T1", "B1", "F1"), ("XBAG1", "T1", "B2", null)],
            _world.Quote(LoungeGolden()).Items.Select(Position));
        Assert.Equal(
            [("XBAG1", "T1", "B1", null), ("LNGTHR", "T1", "B1", "F1"), ("LNGXLG", "T1", "B1", "F1"), ("XBAG1", "T1", "B2", null), ("LNGXLG", "T1", "B2", "F2")],
            _world.Quote(Request([Traveller("T1", "ADT", "F1", "F2")], TwoOneFlightBounds(5))).Items.Select(Position));
    }

    [Fact]
    public void P2_Q05_LoungeSelectionIsPricedWithOrWithoutItsBound()
    {
        var withoutBound = Assert.Single(_world.Quote(LoungeGolden(LoungeSelection(_lounge, _loungeRule))).Items);
        var withBound = Assert.Single(_world.Quote(LoungeGolden(LoungeSelection(_lounge, _loungeRule, boundRef: "B1"))).Items);

        Assert.Equal(("LNGTHR", "T1", "B1", "F1"), Position(withoutBound));
        Assert.Equal(Position(withoutBound), Position(withBound));
        Assert.Equal((withoutBound.Quantity, withoutBound.MaxQuantity, withoutBound.PriceRuleId, withoutBound.Total), (withBound.Quantity, withBound.MaxQuantity, withBound.PriceRuleId, withBound.Total));
        Assert.Equal(20.00m, withoutBound.Total);
    }

    [Fact]
    public void P2_Q06_SelectionMustFitTheScopeOfItsProduct()
    {
        BusinessAssert.Throws(16303, 422, () => _world.Quote(LoungeGolden(LoungeSelection(_lounge, _loungeRule, boundRef: "B2"))));
        BusinessAssert.Throws(16303, 422, () => _world.Quote(LoungeGolden(Selection(_lounge, _loungeRule))));
        BusinessAssert.Throws(16303, 422, () => _world.Quote(LoungeGolden(Selection(_bag, _bagRule, flightRef: "F1"))));
    }

    [Fact]
    public void P2_Q07_LoungeSelectionMustBeApplicable()
    {
        BusinessAssert.Throws(16305, 422, () => _world.Quote(LoungeGolden(LoungeSelection(_lounge, _loungeRule, flightRef: "F2"))));
        BusinessAssert.Throws(16305, 422, () => _world.Quote(Request(
            [Traveller("T1", "ADT", "F2"), Traveller("T2", "ADT", "F1")],
            TwoOneFlightBounds(),
            [LoungeSelection(_lounge, _loungeRule)])));
        BusinessAssert.Throws(16304, 422, () => _world.Quote(LoungeGolden(LoungeSelection(_lounge, _loungeRule, flightRef: "F9"))));
    }

    [Fact]
    public void P2_Q08_OccurrenceIsIdentifiedByProductTravellerAndFlight()
    {
        IReadOnlyList<AncillaryQuoteTraveller> travellers = [Traveller("T1", "ADT", "F1", "F2"), Traveller("T2", "ADT", "F1", "F2")];

        BusinessAssert.Throws(16307, 422, () => _world.Quote(LoungeGolden(
            LoungeSelection(_lounge, _loungeRule),
            LoungeSelection(_lounge, _loungeRule, boundRef: "B1"))));

        var result = _world.Quote(Request(
            travellers,
            TwoOneFlightBounds(),
            [LoungeSelection(_lounge, _loungeRule), LoungeSelection(_lounge, _loungeRule, "T2", boundRef: "B1")]));

        Assert.Equal([("LNGTHR", "T1", "B1", "F1"), ("LNGTHR", "T2", "B1", "F1")], result.Items.Select(Position));
    }

    [Fact]
    public void P2_Q09_SelectedQuantityMustBeWithinTheProductLimits()
    {
        var generic = _world.AddProduct(ProductSpec.LG);
        var genericRule = _world.AddRule(RuleSpec.RLG);

        var item = Assert.Single(_world.Quote(LoungeGolden(LoungeSelection(generic, genericRule, quantity: 2))).Items);

        Assert.Equal((2, 15.00m, 30.00m, 30.00m), (item.Quantity, item.PriceLines[0].UnitAmount, item.PriceLines[0].Amount, item.Total));
        BusinessAssert.Throws(16306, 422, () => _world.Quote(LoungeGolden(LoungeSelection(generic, genericRule, quantity: 3))));
        BusinessAssert.Throws(16306, 422, () => _world.Quote(LoungeGolden(LoungeSelection(_lounge, _loungeRule, quantity: 2))));
    }

    [Fact]
    public void P2_Q10_ExistingLoungeRemovesOnlyItsOwnOccurrence()
    {
        IReadOnlyList<AncillaryQuoteTraveller> travellers = [Traveller("T1", "ADT", "F1", "F2"), Traveller("T2", "ADT", "F1")];
        IReadOnlyList<AncillaryQuoteBound> bounds = [Bound("B1", Flight("F1")), Bound("B2", Flight("F2", 1, 3, "2026-10-15T08:00:00+03:30"))];
        IReadOnlyList<AncillaryQuoteExistingOccurrence> existing = [ExistingOnFlight("LNGTHR", "T1", "F1", 1)];

        var lounges = _world.Quote(Request(travellers, bounds, existing: existing)).Items.Where(item => ReferenceEquals(item.Product, _lounge));

        Assert.Equal([("LNGTHR", "T1", "B2", "F2"), ("LNGTHR", "T2", "B1", "F1")], lounges.Select(Position));
        BusinessAssert.Throws(16305, 422, () => _world.Quote(Request(travellers, bounds, [LoungeSelection(_lounge, _loungeRule)], existing)));
        Assert.Single(_world.Quote(Request(travellers, bounds, [LoungeSelection(_lounge, _loungeRule, flightRef: "F2")], existing)).Items);
        Assert.Single(_world.Quote(Request(travellers, bounds, [LoungeSelection(_lounge, _loungeRule, "T2")], existing)).Items);
    }

    [Fact]
    public void P2_Q11_ExistingLoungeReducesTheRemainingQuantity()
    {
        var generic = _world.AddProduct(ProductSpec.LG);
        var genericRule = _world.AddRule(RuleSpec.RLG);
        IReadOnlyList<AncillaryQuoteExistingOccurrence> existing = [ExistingOnFlight("LNGXLG", "T1", "F1", 1)];
        var request = Request([Traveller("T1", "ADT", "F1", "F2")], TwoOneFlightBounds(), existing: existing);

        var item = _world.Quote(request).Items.Single(candidate => ReferenceEquals(candidate.Product, generic));

        Assert.Equal((("LNGXLG", "T1", "B1", "F1"), 1, 1), (Position(item), item.MaxQuantity, item.Quantity));
        BusinessAssert.Throws(16306, 422, () => _world.Quote(request with { Selections = [LoungeSelection(generic, genericRule, quantity: 2)] }));
        Assert.Single(_world.Quote(request with { Selections = [LoungeSelection(generic, genericRule)] }).Items);
    }

    [Fact]
    public void P2_Q12_ExistingEntryWithoutAFlightDoesNotTouchLoungeItems()
    {
        var without = _world.Quote(LoungeGolden()).Items;
        var with = _world.Quote(Request(
            [Traveller("T1", "ADT", "F1", "F2")],
            TwoOneFlightBounds(),
            existing: [Existing("LNGTHR", "T1", "B1", 1)])).Items;

        Assert.Equal(without.Select(Position), with.Select(Position));
        Assert.Equal(1, with.Single(item => ReferenceEquals(item.Product, _lounge)).MaxQuantity);
    }

    [Fact]
    public void P2_Q13_AirportConditionsLookAtTheFlightOfTheOccurrence()
    {
        var world = new QuoteWorld();
        var lounge = world.AddProduct(ProductSpec.L);
        var plain = world.AddRule(RuleSpec.RL with { Priority = 2 });
        var fromTwo = world.AddRule(RuleSpec.RL with { Priority = 1, Lines = [RuleSpec.Ancillary(10.00m)], OriginAirportIds = [2] });

        Assert.Equal(plain.Id, world.Quote(LoungeGolden()).Items.Single(item => ReferenceEquals(item.Product, lounge)).PriceRuleId);

        fromTwo.Retire();
        var toTwo = world.AddRule(RuleSpec.RL with { Priority = 1, Lines = [RuleSpec.Ancillary(12.00m)], DestinationAirportIds = [2] });

        var item = world.Quote(LoungeGolden()).Items.Single(candidate => ReferenceEquals(candidate.Product, lounge));

        Assert.Equal((toTwo.Id, 12.00m), (item.PriceRuleId, item.Total));
    }

    [Fact]
    public void P2_Q14_LoungeIsNotOfferedOnAFlightOfAnotherAirline()
    {
        IReadOnlyList<AncillaryQuoteTraveller> travellers = [Traveller("T1", "ADT", "F1")];
        IReadOnlyList<AncillaryQuoteBound> bounds = [Bound("B1", Flight("F1", marketingAirlineId: 11))];

        Assert.Empty(_world.Quote(Request(travellers, bounds)).Items);
        BusinessAssert.Throws(16305, 422, () => _world.Quote(Request(travellers, bounds, [LoungeSelection(_lounge, _loungeRule)])));
    }

    [Fact]
    public void P2_Q15_LoungeSelectionMustStillBeCurrent()
    {
        var revised = _world.AddRevision(_lounge, ProductSpec.L with { Name = "Lounge" });

        BusinessAssert.Throws(16309, 409, () => _world.Quote(LoungeGolden(LoungeSelection(_lounge, _loungeRule))));
        Assert.Single(_world.Quote(LoungeGolden(LoungeSelection(revised, _loungeRule))).Items);

        _loungeRule.Retire();
        var current = _world.AddRule(RuleSpec.RL with { Lines = [RuleSpec.Ancillary(25.00m)] });

        BusinessAssert.Throws(16309, 409, () => _world.Quote(LoungeGolden(LoungeSelection(revised, _loungeRule))));
        Assert.Equal(25.00m, Assert.Single(_world.Quote(LoungeGolden(LoungeSelection(revised, current))).Items).Total);
    }

    [Fact]
    public void P2_Q16_QuoteChangesNothingAndIsRepeatable()
    {
        var before = State();

        var first = _world.Quote(LoungeGolden());
        var second = _world.Quote(LoungeGolden(LoungeSelection(_lounge, _loungeRule)));
        var third = _world.Quote(LoungeGolden());

        Assert.Equal(before, State());
        Assert.Empty(_lounge.GetEvents());
        Assert.Empty(_loungeRule.GetEvents());
        Assert.Single(second.Items);
        Assert.Equal(first.Items.Select(item => (Position(item), item.Total)), third.Items.Select(item => (Position(item), item.Total)));
    }

    private (AncillaryProductStatus, int, int, AncillaryPriceRuleStatus, int) State()
        => (_lounge.Status, _lounge.Version, _lounge.Lounge!.AirportIds.Count, _loungeRule.Status, _loungeRule.Lines.Count);
}
