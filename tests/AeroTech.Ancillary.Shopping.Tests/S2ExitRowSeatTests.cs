using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.FlightOffers;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateBatch;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.ResolveSelections;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.GetSeatMap;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.GetSeatMap.Backoffice;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Snapshots;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Ancillary.Shopping.SeatMaps;
using AeroTech.Ancillary.Shopping.Tests.Fixtures;
using AeroTech.Framework.Core.Domain.Exceptions;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.BackofficeDesk;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.OrderDesk;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.SeatMapDesk;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.SelectionDesk;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.ShoppingLab;

namespace AeroTech.Ancillary.Shopping.Tests;

public class S2ExitRowSeatTests
{
    private const string Adult = "ADT-1";
    private const string Child = "CHD-1";

    private sealed record Desk(ShoppingLab Lab, FixedOrders Orders, FixedClock Clock, bool VerifiedExitRows)
    {
        public async Task<(EvaluateBatchService Service, GetSeatMapService SeatMaps)> BatchAsync(IOccupiedSeatProvider? occupied = null)
            => await SelectionDesk.BatchAsync(Lab, occupied, clock: Clock, orders: Orders, verifiedExitRows: VerifiedExitRows);

        public async Task<SeatTravellerOffer> SeatAsync(string seatNumber, string traveller)
        {
            var (_, seatMaps) = await BatchAsync();
            var map = await seatMaps.ExecuteAsync(new BackofficeGetSeatMapQuery { OrderId = OrderDesk.OrderId }, Sale);

            return map.Flights[0].OffersOf(seatNumber).Single(offer => offer.TravellerRef == traveller);
        }

        public async Task<SelectionBatchSnapshot> EvaluateAsync(string seatMapSelectionId, bool? exitRowTermsAccepted)
        {
            var (service, _) = await BatchAsync();

            return await service.ExecuteAsync(ByOrder, [new SelectionLineRequest("exit-row", SelectionLineKind.SeatMap, null, null, seatMapSelectionId, exitRowTermsAccepted)], Sale);
        }
    }

    private static async Task<Desk> DeskAsync(bool verifiedExitRows = true)
    {
        var lab = new ShoppingLab();
        var exitRow = lab.Definition("A08");

        lab.Price(exitRow, lab.Provision(exitRow, stage: PurchaseStage.Both), Rate(40m));

        return new Desk(lab, new FixedOrders(OrderOf(Sale.Sell(await OfferWire.ReadAsync(OfferWire.Captured())))), new FixedClock { Now = Now }, verifiedExitRows);
    }

    [Fact]
    public async Task B2_a_verified_adult_who_accepts_the_terms_gets_a_signed_exit_row_seat_that_resolves_valid()
    {
        var desk = await DeskAsync();
        var seat = await desk.SeatAsync("3A", Adult);

        var batch = await desk.EvaluateAsync(seat.Id!, true);
        var token = await Tokens(desk.Clock).ReadAsync(batch.Lines[0].PricedSelection!.Value);
        var (service, _) = await desk.BatchAsync();
        var resolved = await new ResolveSelectionsService(Tokens(desk.Clock), service).ExecuteAsync(OrderDesk.OrderId, 1, [batch.Lines[0].PricedSelection!.Value]);

        Assert.Equal(SeatAvailabilityStatus.Available, seat.Status);
        Assert.Equal((SelectionEvaluationStatus.Accepted, true, (decimal?)40m), (batch.Lines[0].Status, batch.Lines[0].CanPurchase, batch.Lines[0].Total));
        Assert.True(batch.Basket.AllReady);
        Assert.Equal(("3A", (bool?)true, "SEAT_PREFERRED"), (token.Selection!.Selection!.SeatNumber, token.Selection.Selection.ExitRowTermsAccepted, token.Selection.ServiceDefinitionRef));
        Assert.Equal(SelectionResolutionStatus.Valid, resolved.Results.Single().Status);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(false)]
    public async Task B2_missing_or_refused_exit_row_terms_never_produce_a_reference(bool? accepted)
    {
        var desk = await DeskAsync();
        var seat = await desk.SeatAsync("3A", Adult);

        var batch = await desk.EvaluateAsync(seat.Id!, accepted);

        Assert.NotEqual(SelectionEvaluationStatus.Accepted, batch.Lines[0].Status);
        Assert.Equal((false, false), (batch.Lines[0].CanPurchase, batch.Basket.AllReady));
        Assert.Null(batch.Lines[0].PricedSelection);
    }

    [Fact]
    public async Task B2_accepting_the_terms_is_not_eligibility_an_unverified_adult_and_an_ineligible_child_never_see_an_exit_row_seat_as_free()
    {
        var unverified = await DeskAsync(verifiedExitRows: false);
        var verified = await DeskAsync();

        var unverifiedAdult = await unverified.SeatAsync("3A", Adult);
        var child = await verified.SeatAsync("3A", Child);

        Assert.Equal((SeatAvailabilityStatus.Blocked, (string?)null), (unverifiedAdult.Status, unverifiedAdult.Id));
        Assert.Equal((SeatAvailabilityStatus.Blocked, (string?)null), (child.Status, child.Id));
    }

    [Fact]
    public async Task B2_an_exit_row_seat_occupied_after_its_reference_was_signed_resolves_stale()
    {
        var desk = await DeskAsync();
        var seat = await desk.SeatAsync("3A", Adult);
        var batch = await desk.EvaluateAsync(seat.Id!, true);
        var (taken, _) = await desk.BatchAsync(new FixedOccupiedSeats("3A"));

        var resolved = await new ResolveSelectionsService(Tokens(desk.Clock), taken).ExecuteAsync(OrderDesk.OrderId, 1, [batch.Lines[0].PricedSelection!.Value]);

        Assert.Equal((SelectionResolutionStatus.Stale, ShoppingReasonCodes.SeatSelectionNotOffered), (resolved.Results.Single().Status, resolved.Results.Single().ReasonCodes.Single()));
    }

    [Fact]
    public async Task B2_an_ancillary_line_cannot_carry_the_seat_acknowledgement()
    {
        var desk = await DeskAsync();
        var (service, _) = await desk.BatchAsync();

        var refused = await Assert.ThrowsAsync<BusinessException>(
            () => service.ExecuteAsync(ByOrder, [new SelectionLineRequest("bag", SelectionLineKind.Ancillary, "item", null, null, true)], Sale));

        Assert.Equal((16801, 422), (refused.Code, refused.HttpStatus));
    }
}
