using AeroTech.Ancillary.Application.FlightOffers;
using AeroTech.Ancillary.Application.Orders;
using AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Arguments;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateBatch;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Ancillary.Shopping.Selection;
using AeroTech.Ancillary.Shopping.Tests.Fixtures;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.BackofficeDesk;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.OrderDesk;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.SelectionDesk;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.ShoppingLab;

namespace AeroTech.Ancillary.Shopping.Tests;

public class S2BasketCommitTests
{
    private const string Adult = "ADT-1";
    private const string Child = "CHD-1";

    private static ShoppingLab BagLab(params AncillaryPriceComponentArgs[] components)
    {
        var lab = new ShoppingLab();
        var bag = lab.Definition("A01");

        lab.Price(bag, lab.Provision(bag, stage: PurchaseStage.Both), Rate(100m, components: components));

        return lab;
    }

    private static async Task<FlightOffer> AirAsync() => Sale.Sell(await OfferWire.ReadAsync(OfferWire.Captured()));

    private static SelectionLineRequest Bag(string id, string offerItemRef, int quantity)
        => new(id, SelectionLineKind.Ancillary, offerItemRef, new AncillarySelection { Quantity = quantity }, null);

    private static async Task<SelectionBatchSnapshot> EvaluateAsync(ShoppingLab lab, OrderCommercialContext order, string? signingKey = SigningKey, params Func<AncillaryShoppingSnapshot, SelectionLineRequest>[] lines)
    {
        var orders = new FixedOrders(order);
        var shopping = await ShopOrderAsync(lab, orders);
        var (service, _) = await BatchAsync(lab, orders: orders, signingKey: signingKey);

        return await service.ExecuteAsync(ByOrder, lines.Select(line => line(shopping)).ToList(), Sale);
    }

    [Fact]
    public async Task B8_one_unusable_line_withholds_the_reference_of_every_line_of_the_basket()
    {
        var batch = await EvaluateAsync(
            BagLab(),
            OrderOf(await AirAsync()),
            SigningKey,
            shopping => Bag("valid", RefOf(shopping, "A01", Adult), 1),
            shopping => Bag("invalid", RefOf(shopping, "A01", Child), 0));

        Assert.Equal((true, false), (batch.Lines[0].CanPurchase, batch.Lines[1].CanPurchase));
        Assert.All(batch.Lines, line => Assert.Null(line.PricedSelection));
        Assert.Equal((false, (decimal?)null), (batch.Basket.AllReady, batch.Basket.Total));
        Assert.Empty(batch.Basket.ReasonCodes);
    }

    [Fact]
    public async Task B8_an_unapplied_shared_fee_withholds_every_reference_although_each_line_is_accepted()
    {
        var batch = await EvaluateAsync(
            BagLab(Fee("TKT", 7m, FeeApplicationUnit.Ticket)),
            OrderOf(await AirAsync()),
            SigningKey,
            shopping => Bag("adult", RefOf(shopping, "A01", Adult), 1));

        Assert.Equal((SelectionEvaluationStatus.Accepted, false), (batch.Lines[0].Status, batch.Lines[0].CanPurchase));
        Assert.Null(batch.Lines[0].PricedSelection);
        Assert.Equal((false, ShoppingReasonCodes.SharedFeeNotApplied), (batch.Basket.AllReady, batch.Basket.ReasonCodes.Single()));
    }

    [Fact]
    public async Task B8_an_order_bound_basket_whose_every_line_is_purchasable_gets_a_reference_on_every_line()
    {
        var batch = await EvaluateAsync(
            BagLab(),
            OrderOf(await AirAsync()),
            SigningKey,
            shopping => Bag("adult", RefOf(shopping, "A01", Adult), 1),
            shopping => Bag("child", RefOf(shopping, "A01", Child), 2));

        Assert.Equal((true, (decimal?)300m), (batch.Basket.AllReady, batch.Basket.Total));
        Assert.All(batch.Lines, line => Assert.NotNull(line.PricedSelection));
    }

    [Fact]
    public async Task B8_an_offer_preview_is_priced_but_never_ready_and_never_needs_the_signing_key()
    {
        var lab = BagLab();
        var shopping = await SnapshotAsync(lab);
        var (service, _) = await BatchAsync(lab, signingKey: null);

        var batch = await service.ExecuteAsync(new OfferQuery(OfferWire.OfferId), [Bag("adult", RefOf(shopping, "A01", Adult), 1)], Sale);

        Assert.True(batch.Lines[0].CanPurchase);
        Assert.Null(batch.Lines[0].PricedSelection);
        Assert.Equal((false, (decimal?)100m, ShoppingReasonCodes.OrderBindingRequired), (batch.Basket.AllReady, batch.Basket.Total, batch.Basket.ReasonCodes.Single()));
    }

    [Fact]
    public async Task B8_a_ready_order_basket_fails_closed_without_a_signing_key_and_a_basket_that_is_not_ready_never_asks_for_it()
    {
        var air = await AirAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => EvaluateAsync(BagLab(), OrderOf(air), null, shopping => Bag("adult", RefOf(shopping, "A01", Adult), 1)));

        var notReady = await EvaluateAsync(BagLab(), OrderOf(air), null, shopping => Bag("zero", RefOf(shopping, "A01", Adult), 0));

        Assert.False(notReady.Basket.AllReady);
    }

    [Fact]
    public async Task B5_a_traveller_the_order_does_not_link_makes_its_line_unverified_and_the_basket_issues_no_reference()
    {
        var air = await AirAsync();
        var order = OrderOf(air);
        var unlinked = order with { Travellers = order.Travellers.Where(traveller => traveller.TravellerRef != Child).ToList() };

        var batch = await EvaluateAsync(
            BagLab(),
            unlinked,
            SigningKey,
            shopping => Bag("adult", RefOf(shopping, "A01", Adult), 1),
            shopping => Bag("child", RefOf(shopping, "A01", Child), 1));

        Assert.Equal((SelectionEvaluationStatus.Accepted, true), (batch.Lines[0].Status, batch.Lines[0].CanPurchase));
        Assert.Equal((SelectionEvaluationStatus.Incomplete, false, ShoppingReasonCodes.OrderTravellerNotLinked), (batch.Lines[1].Status, batch.Lines[1].CanPurchase, batch.Lines[1].ReasonCodes.Single()));
        Assert.All(batch.Lines, line => Assert.Null(line.PricedSelection));
        Assert.False(batch.Basket.AllReady);
    }

    [Theory]
    [InlineData("same-traveller-twice")]
    [InlineData("same-order-traveller-twice")]
    public async Task B5_an_ambiguous_order_traveller_link_is_treated_as_no_link(string shape)
    {
        var air = await AirAsync();
        var order = OrderOf(air);
        var child = order.Travellers.Single(traveller => traveller.TravellerRef == Child);
        var ambiguous = shape == "same-traveller-twice"
            ? order with { Travellers = [.. order.Travellers, child with { OrderTravellerId = 999 }] }
            : order with { Travellers = order.Travellers.Select(traveller => traveller with { OrderTravellerId = 500 }).ToList() };

        var batch = await EvaluateAsync(BagLab(), ambiguous, SigningKey, shopping => Bag("child", RefOf(shopping, "A01", Child), 1));

        Assert.Equal((false, ShoppingReasonCodes.OrderTravellerNotLinked), (batch.Lines[0].CanPurchase, batch.Lines[0].ReasonCodes.Single()));
        Assert.Null(batch.Lines[0].PricedSelection);
    }

    [Fact]
    public async Task B5_a_true_order_scoped_service_has_no_beneficiary_and_still_gets_its_reference()
    {
        var lab = new ShoppingLab();
        var wifi = lab.Definition("A24");

        lab.Price(wifi, lab.Provision(wifi, scope: ServiceCoverageScope.Order, stage: PurchaseStage.Both), Rate(30m));

        var orders = new FixedOrders(OrderOf(await AirAsync()));
        var shopping = await ShopOrderAsync(lab, orders);
        var item = shopping.Items.Single(row => row.Candidate.CandidateIdentity.CoverageScope == ServiceCoverageScope.Order);
        var (service, _) = await BatchAsync(lab, orders: orders);

        var batch = await service.ExecuteAsync(
            ByOrder,
            [new SelectionLineRequest("wifi", SelectionLineKind.Ancillary, item.OfferItemRef, new AncillarySelection { PlanCode = wifi.ServiceDefinitionRef, DeviceCount = 1 }, null)],
            Sale);
        var reading = await Tokens(new() { Now = Now }).ReadAsync(batch.Lines[0].PricedSelection!.Value);

        Assert.True(batch.Basket.AllReady);
        Assert.Equal((SelectionResolutionStatus.Valid, ServiceCoverageScope.Order, 0), (reading.Status, reading.Selection!.CoverageScope, reading.Selection.TravellerRefs.Count));
    }
}
