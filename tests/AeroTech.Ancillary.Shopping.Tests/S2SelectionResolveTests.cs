using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.FlightOffers;
using AeroTech.Ancillary.Application.Orders;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateBatch;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.ResolveSelections;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.ResolveSelections.Service;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.GetSeatMap;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.GetSeatMap.Backoffice;
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
        public async Task<SelectionResolutionSnapshot> ResolveAsync(IReadOnlyList<string>? refs = null, long orderId = OrderDesk.OrderId, int expected = Version, IOccupiedSeatProvider? occupied = null)
        {
            var (batch, _) = await SelectionDesk.BatchAsync(Lab, occupied, clock: Clock, orders: Orders);

            return await new ResolveSelectionsService(SelectionDesk.Tokens(Clock), batch).ExecuteAsync(orderId, expected, refs ?? Refs);
        }
    }

    private static async Task<Desk> IssuedAsync()
    {
        var lab = new ShoppingLab();
        var bag = lab.Definition("A01");
        var seat = lab.Definition("A07");

        lab.Price(bag, lab.Provision(bag, stage: PurchaseStage.Both), Rate(100m));
        lab.Price(seat, lab.Provision(seat, stage: PurchaseStage.Both, rules: Rules(seats: new ProvisionSeatApplicationArgs([], ["W"]))), Rate(15m));

        var clock = new FixedClock { Now = Now };
        var air = Sale.Sell(await OfferWire.ReadAsync(OfferWire.Captured()));
        var orders = new FixedOrders(OrderOf(air, commercialVersion: Version));
        var (batch, seatMaps) = await SelectionDesk.BatchAsync(lab, clock: clock, orders: orders);
        var (source, _) = await SourceAsync(lab, orders: orders);
        var item = (await new GetAncillaryOffersService(source, lab.Engine).ExecuteAsync(new OfferQuery(null, OrderId: OrderDesk.OrderId), Sale))
            .Items.First(row => row.Candidate.VariantCode == "A01" && row.Candidate.Travellers.SequenceEqual([Adult]));
        var map = await seatMaps.ExecuteAsync(new BackofficeGetSeatMapQuery { OrderId = OrderDesk.OrderId }, Sale);
        var issued = await batch.ExecuteAsync(
            new OfferQuery(null, OrderId: OrderDesk.OrderId),
            [
                new SelectionLineRequest("bag", SelectionLineKind.Ancillary, item.OfferItemRef, new AncillarySelection { Quantity = 2 }, null),
                new SelectionLineRequest("paid-seat", SelectionLineKind.SeatMap, null, null, map.Flights[0].OffersOf("3A").Single(offer => offer.TravellerRef == Child).Id),
                new SelectionLineRequest("included-seat", SelectionLineKind.SeatMap, null, null, map.Flights[0].OffersOf("3B").Single(offer => offer.TravellerRef == Adult).Id)
            ],
            Sale);

        return new Desk(lab, clock, orders, air, issued.Lines.Select(line => line.PricedSelection!.Value).ToList());
    }

    private static (SelectionResolutionStatus, string) Verdict(ResolvedSelectionSnapshot result) => (result.Status, string.Join(',', result.ReasonCodes));

    [Fact]
    public async Task SEAT_03_every_signed_line_of_an_unchanged_order_resolves_valid_as_a_typed_service_or_included_seat()
    {
        var desk = await IssuedAsync();

        var resolution = await desk.ResolveAsync();
        var answer = ServiceSelectionResolutionMapper.Map(resolution, "corr-1");
        var bag = answer.Results[0].Service!;
        var paidSeat = answer.Results[1].Service!;
        var includedSeat = answer.Results[2].IncludedSeat!;

        Assert.Equal((OrderDesk.OrderId, (int?)Version, "corr-1", true), (answer.OrderId, answer.OrderCommercialVersion, answer.CorrelationId, answer.AllValid));
        Assert.Equal(desk.Refs, answer.Results.Select(result => result.PricedSelectionRef));
        Assert.All(answer.Results, result => Assert.Equal(("Valid", 0), (result.Status, result.ReasonCodes.Count)));
        Assert.Equal(("Ancillary", "EXTRA_BAG", 200m), (bag.Kind, bag.ServiceDefinitionRef, bag.Price.LineTotal!.Value));
        Assert.Equal(new[] { 101L }, bag.OrderTravellerIds);
        Assert.Equal(("SeatMap", "SEAT_STANDARD", "3A", 15m), (paidSeat.Kind, paidSeat.ServiceDefinitionRef, paidSeat.SeatNumber, paidSeat.Price.LineTotal!.Value));
        Assert.Equal(new[] { 102L }, paidSeat.OrderTravellerIds);
        Assert.Equal(paidSeat.FlightIds.Single(), includedSeat.FlightId);
        Assert.Equal(((long?)101L, Adult, "3B", 0m), (includedSeat.OrderTravellerId, includedSeat.TravellerRef, includedSeat.SeatNumber, includedSeat.Fee));
        Assert.Null(answer.Results[2].Service);
        Assert.Equal(paidSeat.FlightCapacityId, includedSeat.FlightCapacityId);
    }

    [Fact]
    public async Task SEC_01_SEC_25_one_tampered_reference_is_rejected_alone_and_the_answer_is_not_all_valid()
    {
        var desk = await IssuedAsync();
        var tampered = desk.Refs[0][..^4] + (desk.Refs[0][^4..] == "AAAA" ? "BBBB" : "AAAA");

        var resolution = await desk.ResolveAsync([tampered, desk.Refs[1], desk.Refs[2]]);

        Assert.Equal((SelectionResolutionStatus.Rejected, ShoppingReasonCodes.PricedSelectionNotAuthentic), Verdict(resolution.Results[0]));
        Assert.Null(resolution.Results[0].Line);
        Assert.All(resolution.Results.Skip(1), result => Assert.Equal(SelectionResolutionStatus.Valid, result.Status));
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

        Assert.Equal((SelectionResolutionStatus.Stale, ShoppingReasonCodes.OfferItemNotOffered), Verdict(resolution.Results[0]));
        Assert.Equal((SelectionResolutionStatus.Stale, ShoppingReasonCodes.SeatSelectionNotOffered), Verdict(resolution.Results[1]));
        Assert.Equal(SelectionResolutionStatus.Valid, resolution.Results[2].Status);
    }

    [Fact]
    public async Task SEAT_07_a_seat_taken_after_evaluation_is_stale_and_its_price_is_never_a_hold()
    {
        var desk = await IssuedAsync();

        var resolution = await desk.ResolveAsync(occupied: new FixedOccupiedSeats("3A"));

        Assert.Equal((SelectionResolutionStatus.Stale, ShoppingReasonCodes.SeatSelectionNotOffered), Verdict(resolution.Results[1]));
        Assert.Equal(new[] { SelectionResolutionStatus.Valid, SelectionResolutionStatus.Valid }, new[] { resolution.Results[0].Status, resolution.Results[2].Status });
    }

    [Fact]
    public async Task PRICE_20_the_same_reference_twice_is_a_conflict_and_neither_copy_resolves()
    {
        var desk = await IssuedAsync();

        var resolution = await desk.ResolveAsync([desk.Refs[0], desk.Refs[0], desk.Refs[1], desk.Refs[1]]);

        Assert.Equal((SelectionResolutionStatus.Rejected, ShoppingReasonCodes.DuplicateSelection), Verdict(resolution.Results[0]));
        Assert.Equal((SelectionResolutionStatus.Rejected, ShoppingReasonCodes.DuplicateSelection), Verdict(resolution.Results[1]));
        Assert.All(resolution.Results.Skip(2), result => Assert.Equal(SelectionResolutionStatus.Rejected, result.Status));
        Assert.All(resolution.Results.Skip(2), result => Assert.Contains(ShoppingReasonCodes.SeatSelectedTwice, result.ReasonCodes));
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
