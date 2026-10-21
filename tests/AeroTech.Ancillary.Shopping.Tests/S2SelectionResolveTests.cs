using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.FlightOffers;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateBatch;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.ResolveSelections;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.ResolveSelections.Service;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Ancillary.Shopping.Selection;
using AeroTech.Ancillary.Shopping.Tests.Fixtures;
using AeroTech.Framework.Core.Domain.Exceptions;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.BackofficeDesk;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.OrderDesk;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.SeatMapDesk;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.ShoppingLab;
using OfferControllers = AeroTech.Ancillary.RestApi.V1.AncillaryOffers.Controllers;

namespace AeroTech.Ancillary.Shopping.Tests;

public class S2SelectionResolveTests
{
    private const string Adult = "ADT-1";
    private const string Child = "CHD-1";
    private const int Version = 4;

    private sealed record Desk(ShoppingLab Lab, FixedClock Clock, FixedOrders Orders, FlightOffer Air, IReadOnlyList<string> Refs)
    {
        public async Task<SelectionResolutionSnapshot> ResolveAsync(IReadOnlyList<string>? refs = null, long orderId = OrderDesk.OrderId, int expected = Version)
        {
            var (batch, _) = await SelectionDesk.BatchAsync(Lab, clock: Clock, orders: Orders);

            return await new ResolveSelectionsService(SelectionDesk.Tokens(Clock), batch).ExecuteAsync(orderId, expected, refs ?? Refs);
        }
    }

    private static async Task<Desk> IssuedAsync()
    {
        var lab = new ShoppingLab();
        var bag = lab.Definition("A01");

        lab.Price(bag, lab.Provision(bag, stage: PurchaseStage.Both), Rate(100m));

        var clock = new FixedClock { Now = Now };
        var air = Sale.Sell(await OfferWire.ReadAsync(OfferWire.Captured()));
        var orders = new FixedOrders(OrderOf(air, commercialVersion: Version));
        var (batch, _) = await SelectionDesk.BatchAsync(lab, clock: clock, orders: orders);
        var (source, _) = await SourceAsync(lab, orders: orders);
        var items = (await Offers(source, lab).ExecuteAsync(new OfferQuery(null, OrderId: OrderDesk.OrderId), Sale)).Items.ToList();
        var adultBag = items.First(row => row.Candidate.VariantCode == "A01" && row.Candidate.Travellers.SequenceEqual([Adult]));
        var childBag = items.First(row => row.Candidate.VariantCode == "A01" && row.Candidate.Travellers.SequenceEqual([Child]));
        var issued = await batch.ExecuteAsync(
            new OfferQuery(null, OrderId: OrderDesk.OrderId),
            [
                new SelectionLineRequest("adult-bag", adultBag.OfferItemRef, new AncillarySelection { Quantity = 2 }),
                new SelectionLineRequest("child-bag", childBag.OfferItemRef, new AncillarySelection { Quantity = 1 })
            ],
            Sale);

        return new Desk(lab, clock, orders, air, issued.Lines.Select(line => line.PricedSelection!.Value).ToList());
    }

    private static (SelectionResolutionStatus, string) Verdict(ResolvedSelectionSnapshot result) => (result.Status, string.Join(',', result.ReasonCodes));

    [Fact]
    public async Task RESOLVE_01_every_signed_line_of_an_unchanged_order_resolves_valid_as_a_typed_service()
    {
        var desk = await IssuedAsync();

        var resolution = await desk.ResolveAsync();
        var answer = ServiceSelectionResolutionMapper.Map(resolution, "corr-1");
        var adultBag = answer.Results[0].Service!;
        var childBag = answer.Results[1].Service!;

        Assert.Equal((OrderDesk.OrderId, (int?)Version, "corr-1", true), (answer.OrderId, answer.OrderCommercialVersion, answer.CorrelationId, answer.AllValid));
        Assert.Equal(desk.Refs, answer.Results.Select(result => result.PricedSelectionRef));
        Assert.All(answer.Results, result => Assert.Equal(("Valid", 0), (result.Status, result.ReasonCodes.Count)));
        Assert.Equal(("EXTRA_BAG", 200m, "101"), (adultBag.ServiceDefinitionRef, adultBag.Price.LineTotal!.Value, string.Join(',', adultBag.OrderTravellerIds)));
        Assert.Equal(("EXTRA_BAG", 100m, "102"), (childBag.ServiceDefinitionRef, childBag.Price.LineTotal!.Value, string.Join(',', childBag.OrderTravellerIds)));
    }

    [Fact]
    public async Task SEC_01_SEC_25_one_tampered_reference_is_rejected_alone_and_the_answer_is_not_all_valid()
    {
        var desk = await IssuedAsync();
        var tampered = desk.Refs[0][..^4] + (desk.Refs[0][^4..] == "AAAA" ? "BBBB" : "AAAA");

        var resolution = await desk.ResolveAsync([tampered, desk.Refs[1]]);

        Assert.Equal((SelectionResolutionStatus.Rejected, ShoppingReasonCodes.PricedSelectionNotAuthentic), Verdict(resolution.Results[0]));
        Assert.Null(resolution.Results[0].Line);
        Assert.Equal(SelectionResolutionStatus.Valid, resolution.Results[1].Status);
        Assert.False(resolution.AllValid);
    }

    [Fact]
    public async Task SEC_03_an_expired_reference_is_expired_and_never_renewed()
    {
        var desk = await IssuedAsync();

        desk.Clock.Now = Now + SourceContextTtl;

        Assert.All((await desk.ResolveAsync()).Results, result => Assert.Equal((SelectionResolutionStatus.Expired, ""), Verdict(result)));
    }

    [Fact]
    public async Task SEC_06_references_of_another_order_are_rejected_without_reading_or_pricing_that_order()
    {
        var desk = await IssuedAsync();

        var resolution = await desk.ResolveAsync(orderId: 7002);

        Assert.All(resolution.Results, result => Assert.Equal((SelectionResolutionStatus.Rejected, ShoppingReasonCodes.PricedSelectionForAnotherOrder), Verdict(result)));
        Assert.All(resolution.Results, result => Assert.Null(result.Line));
        Assert.DoesNotContain(7002L, desk.Orders.Asked);
    }

    [Fact]
    public async Task FLOW_14_SEC_09_a_changed_order_version_makes_every_reference_stale_before_its_expiry()
    {
        var desk = await IssuedAsync();

        desk.Orders.Put(OrderOf(desk.Air, commercialVersion: Version + 1));

        var expectedOld = await desk.ResolveAsync();
        var expectedNew = await desk.ResolveAsync(expected: Version + 1);

        Assert.All(expectedOld.Results.Concat(expectedNew.Results), result => Assert.Equal((SelectionResolutionStatus.Stale, ShoppingReasonCodes.OrderVersionChanged), Verdict(result)));
        Assert.Equal((int?)(Version + 1), expectedNew.OrderCommercialVersion);
    }

    [Fact]
    public async Task SEC_10_SEC_11_a_withdrawn_product_makes_its_reference_stale_and_never_falls_back_to_another_offer()
    {
        var desk = await IssuedAsync();

        desk.Lab.Catalog.Provisions.Clear();

        var resolution = await desk.ResolveAsync();

        Assert.All(resolution.Results, result => Assert.Equal((SelectionResolutionStatus.Stale, ShoppingReasonCodes.OfferItemNotOffered), Verdict(result)));
    }

    [Theory]
    [InlineData("flight-version")]
    [InlineData("flight-capacity")]
    public async Task B7_a_flight_change_that_keeps_the_order_version_and_the_prices_still_makes_every_reference_stale(string change)
    {
        var desk = await IssuedAsync();
        var bound = desk.Air.Bounds[0];
        var flight = bound.Flights[0];
        var changed = change == "flight-version" ? flight with { FlightVersion = flight.FlightVersion + 1 } : flight with { FlightCapacityId = flight.FlightCapacityId + 1 };

        desk.Orders.Put(OrderOf(desk.Air with { Bounds = [bound with { Flights = [changed, .. bound.Flights.Skip(1)] }, .. desk.Air.Bounds.Skip(1)] }, commercialVersion: Version));

        var resolution = await desk.ResolveAsync();

        Assert.All(resolution.Results, result => Assert.Equal((SelectionResolutionStatus.Stale, ShoppingReasonCodes.SourceVersionChanged), Verdict(result)));
        Assert.False(resolution.AllValid);
    }

    [Fact]
    public async Task PRICE_20_the_same_reference_twice_is_a_conflict_and_neither_copy_resolves()
    {
        var desk = await IssuedAsync();

        var resolution = await desk.ResolveAsync([desk.Refs[0], desk.Refs[0], desk.Refs[1]]);

        Assert.Equal((SelectionResolutionStatus.Rejected, ShoppingReasonCodes.DuplicateSelection), Verdict(resolution.Results[0]));
        Assert.Equal((SelectionResolutionStatus.Rejected, ShoppingReasonCodes.DuplicateSelection), Verdict(resolution.Results[1]));
        Assert.Equal(SelectionResolutionStatus.Valid, resolution.Results[2].Status);
    }

    [Fact]
    public async Task FLOW_11_FLOW_16_a_closed_order_or_an_order_ordering_no_longer_knows_fails_the_whole_resolution_honestly()
    {
        var desk = await IssuedAsync();

        desk.Orders.Put(OrderOf(desk.Air, commercialVersion: Version, acceptsChanges: false));

        var closed = await Assert.ThrowsAsync<BusinessException>(() => desk.ResolveAsync());
        var unknown = await Assert.ThrowsAsync<BusinessException>(
            () => (desk with { Orders = new FixedOrders() }).ResolveAsync());

        Assert.Equal((16801, 422), (closed.Code, closed.HttpStatus));
        Assert.Equal((16809, 403), (unknown.Code, unknown.HttpStatus));
    }

    [Theory]
    [InlineData(null, 4, 1)]
    [InlineData(7001L, null, 1)]
    [InlineData(7001L, 4, 0)]
    [InlineData(7001L, 4, 51)]
    public async Task S2_RESOLVE_a_malformed_request_is_refused_before_any_reference_is_read(long? orderId, int? expected, int count)
    {
        var desk = await IssuedAsync();
        var (batch, _) = await SelectionDesk.BatchAsync(desk.Lab, clock: desk.Clock, orders: desk.Orders);

        var refused = await Assert.ThrowsAsync<BusinessException>(
            () => new ResolveSelectionsService(SelectionDesk.Tokens(desk.Clock), batch).ExecuteAsync(orderId, expected, Enumerable.Repeat(desk.Refs[0], count).ToList()));

        Assert.Equal((16801, 422), (refused.Code, refused.HttpStatus));
    }

    [Fact]
    public void S2_RESOLVE_the_resolve_route_is_one_anonymous_service_operation_like_the_existing_service_holds()
    {
        var controller = typeof(OfferControllers.ServiceController);

        Assert.Equal("Service/v{version:apiVersion}/Ancillaries/Selections", controller.GetCustomAttributes(typeof(RouteAttribute), false).Cast<RouteAttribute>().Single().Template);
        Assert.Empty(controller.GetCustomAttributes(typeof(AuthorizeAttribute), true));
        Assert.Equal(new[] { "Resolve" }, controller.GetMethods().SelectMany(method => method.GetCustomAttributes(typeof(HttpPostAttribute), false).Cast<HttpPostAttribute>()).Select(post => post.Template));
    }
}
