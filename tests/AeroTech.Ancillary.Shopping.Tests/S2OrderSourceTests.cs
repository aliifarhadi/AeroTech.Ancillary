using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.FlightOffers;
using AeroTech.Ancillary.Application.Orders;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Ancillary.Providers.Orders;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateBatch;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.Shopping.Common.FlightOffers;
using AeroTech.Ancillary.Query.Shopping.Common.PricedSelections;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Ancillary.Shopping.Context;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Ancillary.Shopping.Selection;
using AeroTech.Ancillary.Shopping.Tests.Fixtures;
using AeroTech.Framework.Core.Domain.Exceptions;
using AeroTech.Messages.Ancillary.Enums;
using AeroTech.Messages.Core.Enums;
using Xunit;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.BackofficeDesk;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.OrderDesk;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.SeatMapDesk;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.ShoppingLab;

namespace AeroTech.Ancillary.Shopping.Tests;

public class S2OrderSourceTests
{
    private const string Adult = "ADT-1";
    private const string Child = "CHD-1";

    private static readonly OfferQuery ByOrder = new(null, OrderId: OrderDesk.OrderId);

    private static async Task<FlightOffer> AirAsync() => Sale.Sell(await OfferWire.ReadAsync(OfferWire.Captured()));

    private static ShoppingLab BagLab()
    {
        var lab = new ShoppingLab();
        var bag = lab.Definition("A01");

        lab.Price(bag, lab.Provision(bag, stage: PurchaseStage.Both), Rate(100m));

        return lab;
    }

    private static async Task<AncillaryShoppingSnapshot> ShopAsync(ShoppingLab lab, IOrderCommercialContextReader orders, OfferQuery? query = null, AncillarySalesScope? scope = null)
    {
        var (source, _) = await SourceAsync(lab, orders: orders);

        return await Offers(source, lab).ExecuteAsync(query ?? ByOrder, scope ?? Sale);
    }

    private static async Task<BusinessException> RefusedAsync(ShoppingLab lab, IOrderCommercialContextReader orders, OfferQuery? query = null, AncillarySalesScope? scope = null)
        => await Assert.ThrowsAsync<BusinessException>(() => ShopAsync(lab, orders, query, scope));

    private static IEnumerable<string> Coverage(AncillaryShoppingSnapshot snapshot, string traveller)
        => snapshot.Items.Where(item => item.Candidate.Travellers.SequenceEqual([traveller])).Select(item => item.Candidate.CandidateIdentity.CoverageRef).Order(StringComparer.Ordinal);

    [Fact]
    public async Task FLOW_02_an_order_before_ticketing_is_shopped_from_its_own_facts_and_its_items_are_bound_to_the_order()
    {
        var lab = BagLab();
        var air = await AirAsync();
        var orderBased = await ShopAsync(lab, new FixedOrders(OrderOf(air)));
        var offerBased = await SnapshotAsync(lab);
        var context = orderBased.PointsOfSale.Single().Context;

        Assert.Equal(
            (ShoppingSourceKind.Order, (long?)OrderDesk.OrderId, (int?)1, "7001", ShoppingStage.PreOrder, (DateTimeOffset?)null),
            (context.SourceIdentity.SourceKind, context.SourceIdentity.OrderId, context.SourceIdentity.OrderCommercialVersion, context.SourceIdentity.TrustedSourceReference, context.ShoppingStage, context.SourceIdentity.TicketedAtUtc));
        Assert.Equal(new[] { (Adult, (long?)101L, (DateOnly?)new DateOnly(1990, 5, 20)), (Child, 102L, new DateOnly(2018, 5, 20)) }, context.Travellers.Select(traveller => (traveller.TravellerRef, traveller.OrderTravellerId, traveller.DateOfBirth)));
        Assert.Equal((OfferWire.OfferId, true), (orderBased.Offer.OfferId, context.CoverageCompleteness.ExistingServicesComplete));
        Assert.Equal(offerBased.Items.Count(), orderBased.Items.Count());
        Assert.Empty(orderBased.Items.Select(item => item.OfferItemRef).Intersect(offerBased.Items.Select(item => item.OfferItemRef)));
    }

    [Fact]
    public async Task FLOW_03_FLOW_10_a_ticketed_order_is_shopped_post_ticketed_and_a_used_coupon_closes_only_its_own_traveller_and_flight()
    {
        var lab = BagLab();
        var air = Ticketed(await AirAsync(), (ticket, coupon) => !(ticket.TravellerRef == Child && coupon.BoundId == "B1"));
        var ticketedAt = Now.AddDays(-1);
        var snapshot = await ShopAsync(lab, new FixedOrders(OrderOf(air, ticketedAtUtc: ticketedAt)));
        var context = snapshot.PointsOfSale.Single().Context;

        Assert.Equal((ShoppingStage.PostTicketed, (DateTimeOffset?)ticketedAt), (context.ShoppingStage, context.SourceIdentity.TicketedAtUtc));
        Assert.Equal(new[] { "B1", "B2" }, Coverage(snapshot, Adult));
        Assert.Equal(new[] { "B2" }, Coverage(snapshot, Child));
    }

    [Fact]
    public async Task FLOW_09_a_ticketed_order_without_coupon_states_leaves_every_candidate_unverified_instead_of_sellable()
    {
        var lab = BagLab();
        var snapshot = await ShopAsync(lab, new FixedOrders(OrderOf(await AirAsync(), ticketedAtUtc: Now.AddDays(-1))));

        Assert.NotEmpty(snapshot.Items);
        Assert.All(snapshot.Items, item => Assert.Equal(OfferReadiness.NeedsVerification, item.Candidate.OfferReadiness));
        Assert.All(snapshot.Items, item => Assert.Contains(ShoppingReasonCodes.CouponStateNotVerified, item.Candidate.ReasonCodes));
    }

    [Fact]
    public async Task FLOW_11_SEAT_18_an_order_closed_for_changes_or_without_its_source_offer_is_refused_honestly()
    {
        var lab = BagLab();
        var air = await AirAsync();

        var closed = await RefusedAsync(lab, new FixedOrders(OrderOf(air, acceptsChanges: false)));
        var withoutOffer = await RefusedAsync(lab, new FixedOrders(OrderOf(air with { OfferId = "" })));

        Assert.Equal((16801, 422), (closed.Code, closed.HttpStatus));
        Assert.Equal((16801, 422), (withoutOffer.Code, withoutOffer.HttpStatus));
    }

    [Fact]
    public async Task FLOW_12_FLOW_13_an_order_is_read_from_ordering_only_and_an_unreachable_ordering_fails_closed()
    {
        var lab = BagLab();
        var offers = new FixedOffers(await AirAsync());
        var flightOffers = new FlightOfferShoppingSource(offers, new TestOnlyAirportReference(), new TestOnlyAirFareReference(), new FixedClock { Now = Now }, new FlightOfferShoppingOptions(SourceContextTtl));

        var read = await Offers(Sources(flightOffers, new FixedOrders(OrderOf(await AirAsync()))), lab).ExecuteAsync(ByOrder, Sale);
        var unreachable = await Assert.ThrowsAsync<BusinessException>(
            () => Offers(Sources(flightOffers, new NotConnectedOrderCommercialContextReader()), lab).ExecuteAsync(ByOrder, Sale));

        Assert.NotEmpty(read.Items);
        Assert.Equal(0, offers.Calls);
        Assert.Equal((16815, 503), (unreachable.Code, unreachable.HttpStatus));
    }

    [Fact]
    public async Task FLOW_15_an_offer_id_and_an_order_id_together_select_no_source()
    {
        var orders = new FixedOrders(OrderOf(await AirAsync()));

        var refused = await RefusedAsync(BagLab(), orders, new OfferQuery(OfferWire.OfferId, OrderId: OrderDesk.OrderId));

        Assert.Equal((16801, 422), (refused.Code, refused.HttpStatus));
        Assert.Empty(orders.Asked);
    }

    [Fact]
    public async Task FLOW_16_an_unknown_order_and_another_customers_order_are_refused_alike()
    {
        var lab = BagLab();
        var orders = new FixedOrders(OrderOf((await AirAsync()) with { SalesChannel = Channel.PartnerAPI, CustomerId = 5001 }));
        var agency = new AncillarySalesScope(Airline, Channel.PartnerAPI, [Pos], TravelAgencyId: 77, CustomerId: 5002);

        var unknown = await RefusedAsync(lab, orders, new OfferQuery(null, OrderId: 9999));
        var foreign = await RefusedAsync(lab, orders, scope: agency);

        Assert.Equal((16809, 403), (unknown.Code, unknown.HttpStatus));
        Assert.Equal((16809, 403), (foreign.Code, foreign.HttpStatus));
    }

    [Fact]
    public async Task FLOW_14_a_new_commercial_version_of_the_order_gives_every_item_a_new_reference()
    {
        var lab = BagLab();
        var air = await AirAsync();

        var first = await ShopAsync(lab, new FixedOrders(OrderOf(air, commercialVersion: 1)));
        var second = await ShopAsync(lab, new FixedOrders(OrderOf(air, commercialVersion: 2)));

        Assert.Empty(first.Items.Select(item => item.OfferItemRef).Intersect(second.Items.Select(item => item.OfferItemRef)));
    }

    [Fact]
    public async Task S2_ORDER_the_services_the_order_already_holds_reach_the_engine_as_existing_services()
    {
        var lab = BagLab();
        var held = Service(9001, Adult, "BAG_23", "B1", "B1:F1");
        var context = (await ShopAsync(lab, new FixedOrders(OrderOf(await AirAsync(), services: held)))).PointsOfSale.Single().Context;

        Assert.Equal(
            (9001L, Adult, "BAG_23", ExistingServiceCommercialState.Active),
            context.ExistingServiceFacts.Select(service => (service.OrderServiceId!.Value, service.TravellerRef, service.ServiceRef, service.CommercialState)).Single());
    }

    [Fact]
    public async Task SEAT_03_an_order_bound_basket_issues_a_signed_reference_per_complete_line_that_reads_back_its_order_and_price()
    {
        var lab = BagLab();
        var clock = new FixedClock { Now = Now };
        var orders = new FixedOrders(OrderOf(await AirAsync(), commercialVersion: 4));
        var (service, _) = await SelectionDesk.BatchAsync(lab, clock: clock, orders: orders);
        var shopped = await ShopAsync(lab, orders);
        var adultBag = shopped.Items.First(item => item.Candidate.VariantCode == "A01" && item.Candidate.Travellers.SequenceEqual([Adult])).OfferItemRef;
        var childBag = shopped.Items.First(item => item.Candidate.VariantCode == "A01" && item.Candidate.Travellers.SequenceEqual([Child])).OfferItemRef;

        var batch = await service.ExecuteAsync(
            ByOrder,
            [
                new SelectionLineRequest("adult-bag", adultBag, new AncillarySelection { Quantity = 1 }),
                new SelectionLineRequest("child-bag", childBag, new AncillarySelection { Quantity = 2 })
            ],
            Sale);
        var tokens = SelectionDesk.Tokens(clock);
        var readings = new List<PricedSelectionReading>();

        foreach (var line in batch.Lines)
            readings.Add(await tokens.ReadAsync(line.PricedSelection!.Value));

        Assert.Equal((true, (decimal?)300m), (batch.Basket.AllReady, batch.Basket.Total));
        Assert.Empty(batch.Basket.ReasonCodes);
        Assert.All(batch.Lines, line => Assert.Equal(Now + SourceContextTtl, line.PricedSelection!.ExpiresAtUtc));
        Assert.Equal(
            new[]
            {
                (SelectionResolutionStatus.Valid, OrderDesk.OrderId, 4, (string?)"EXTRA_BAG", 100m, adultBag),
                (SelectionResolutionStatus.Valid, OrderDesk.OrderId, 4, "EXTRA_BAG", 200m, childBag)
            },
            readings.Select(reading => (reading.Status, reading.Selection!.OrderId, reading.Selection.OrderCommercialVersion, reading.Selection.ServiceDefinitionRef, reading.Selection.Total, reading.Selection.OfferItemRef)));
    }
}
