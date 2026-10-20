using AeroTech.Ancillary.Domain.AncillaryPricingAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate.Arguments;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateBatch;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateBatch.Api;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateBatch.Backoffice;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateBatch.OtaPanel;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOffers.Api;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.Shopping.Common.SalesScope;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.GetSeatMap;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.GetSeatMap.Backoffice;
using AeroTech.Ancillary.Shopping.Results;
using AeroTech.Ancillary.Shopping.Selection;
using AeroTech.Ancillary.Shopping.Tests.Fixtures;
using AeroTech.Framework.Core.Domain.Exceptions;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.BackofficeDesk;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.SeatMapDesk;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.ShoppingLab;

namespace AeroTech.Ancillary.Shopping.Tests;

public class S2SelectionBatchTests
{
    private const string Adult = "ADT-1";
    private const string Child = "CHD-1";

    private static readonly AncillarySalesScope Scope = Sale;

    private static ShoppingLab BagAndSeatLab(params AncillaryPriceComponentArgs[] bagComponents)
    {
        var lab = new ShoppingLab();
        var bag = lab.Definition("A01");
        var seat = lab.Definition("A07");

        lab.Price(bag, lab.Provision(bag), Rate(100m, components: bagComponents.Length > 0 ? bagComponents : [Tax("VAT", 10m, TaxTreatment.AddedToBase)]));
        lab.Price(seat, lab.Provision(seat, rules: Rules(seats: new ProvisionSeatApplicationArgs([], ["W"]))), Rate(15m));

        return lab;
    }

    private static async Task<string> BagRefAsync(ShoppingLab lab, string traveller)
        => (await SnapshotAsync(lab)).Items.First(item => item.Candidate.VariantCode == "A01" && item.Candidate.Travellers.SequenceEqual([traveller])).OfferItemRef;

    private static async Task<string> SeatIdAsync(GetSeatMapService seatMaps, string seatNumber, string traveller)
        => (await seatMaps.ExecuteAsync(new BackofficeGetSeatMapQuery { OfferId = OfferWire.OfferId }, Scope)).Flights[0].OffersOf(seatNumber).Single(offer => offer.TravellerRef == traveller).Id!;

    private static SelectionLineRequest Bag(string id, string offerItemRef, int quantity)
        => new(id, SelectionLineKind.Ancillary, offerItemRef, new AncillarySelection { Quantity = quantity }, null);

    private static SelectionLineRequest Seat(string id, string seatMapSelectionId) => new(id, SelectionLineKind.SeatMap, null, null, seatMapSelectionId);

    private static Task<SelectionBatchSnapshot> EvaluateAsync(EvaluateBatchService service, params SelectionLineRequest[] lines)
        => service.ExecuteAsync(new OfferQuery(OfferWire.OfferId), lines, Scope);

    private static (SelectionEvaluationStatus, bool, decimal?, string) Verdict(SelectionLineSnapshot line)
        => (line.Status, line.CanPurchase, line.Total, string.Join(',', line.ReasonCodes));

    [Fact]
    public async Task PRICE_01_two_lines_of_one_offer_are_priced_by_their_own_quantity_and_the_basket_adds_them_without_an_order_bound_reference()
    {
        var lab = BagAndSeatLab();
        var (service, _) = await SelectionDesk.BatchAsync(lab);

        var batch = await EvaluateAsync(service, Bag("adult", await BagRefAsync(lab, Adult), 1), Bag("child", await BagRefAsync(lab, Child), 2));

        Assert.Equal(new[] { "adult", "child" }, batch.Lines.Select(line => line.ClientSelectionId));
        Assert.Equal((SelectionEvaluationStatus.Accepted, true, (decimal?)110m, ""), Verdict(batch.Lines[0]));
        Assert.Equal((SelectionEvaluationStatus.Accepted, true, (decimal?)220m, ""), Verdict(batch.Lines[1]));
        Assert.All(batch.Lines, line => Assert.Null(line.PricedSelection));
        Assert.Equal((true, (decimal?)330m, Eur), (batch.Basket.AllReady, batch.Basket.Total, batch.Basket.CurrencyId));
        Assert.Equal(new[] { ShoppingReasonCodes.OrderBindingRequired }, batch.Basket.ReasonCodes);
    }

    [Fact]
    public async Task PRICE_19_one_unusable_line_leaves_the_basket_not_ready_while_every_line_keeps_its_own_verdict()
    {
        var lab = BagAndSeatLab();
        var (service, _) = await SelectionDesk.BatchAsync(lab);

        var batch = await EvaluateAsync(
            service,
            Bag("valid", await BagRefAsync(lab, Adult), 1),
            Bag("unknown", "not-an-offer-item", 1),
            Bag("zero", await BagRefAsync(lab, Child), 0));

        Assert.Equal((SelectionEvaluationStatus.Accepted, true, (decimal?)110m, ""), Verdict(batch.Lines[0]));
        Assert.Equal((SelectionEvaluationStatus.Rejected, false, (decimal?)null, ShoppingReasonCodes.OfferItemNotOffered), Verdict(batch.Lines[1]));
        Assert.Equal((SelectionEvaluationStatus.Rejected, false), (batch.Lines[2].Status, batch.Lines[2].CanPurchase));
        Assert.Equal((false, (decimal?)null), (batch.Basket.AllReady, batch.Basket.Total));
    }

    [Fact]
    public async Task PRICE_20_the_same_offer_item_twice_in_one_basket_rejects_both_lines_without_choosing_a_winner()
    {
        var lab = BagAndSeatLab();
        var (service, _) = await SelectionDesk.BatchAsync(lab);
        var bag = await BagRefAsync(lab, Adult);

        var batch = await EvaluateAsync(service, Bag("first", bag, 1), Bag("second", bag, 2));

        Assert.All(batch.Lines, line => Assert.Equal((SelectionEvaluationStatus.Rejected, false, ShoppingReasonCodes.DuplicateSelection), (line.Status, line.CanPurchase, line.ReasonCodes.Single())));
        Assert.All(batch.Lines, line => Assert.Equal(SelectionEvaluationStatus.Accepted, line.Ancillary!.Evaluation.Status));
        Assert.False(batch.Basket.AllReady);
    }

    [Fact]
    public async Task PRICE_12_a_shared_ticket_fee_is_listed_once_for_the_basket_and_never_charged_inside_a_line()
    {
        var lab = BagAndSeatLab(Fee("TKT", 7m, FeeApplicationUnit.Ticket));
        var (service, _) = await SelectionDesk.BatchAsync(lab);

        var batch = await EvaluateAsync(service, Bag("adult", await BagRefAsync(lab, Adult), 1), Bag("child", await BagRefAsync(lab, Child), 1));

        Assert.All(batch.Lines, line => Assert.Equal((SelectionEvaluationStatus.Accepted, false, (decimal?)null), (line.Status, line.CanPurchase, line.Total)));
        Assert.Equal(("TKT", 7m, FeeApplicationUnit.Ticket), (batch.Basket.SharedFees.Single().Code, batch.Basket.SharedFees.Single().Amount, batch.Basket.SharedFees.Single().FeeApplicationUnit!.Value));
        Assert.Equal((false, (decimal?)null), (batch.Basket.AllReady, batch.Basket.Total));
        Assert.Contains(ShoppingReasonCodes.SharedFeeNotApplied, batch.Basket.ReasonCodes);
    }

    [Fact]
    public async Task SEAT_02_seat_map_ids_evaluate_to_the_paid_seat_and_the_included_seat_with_their_flight_capacity()
    {
        var lab = BagAndSeatLab();
        var (service, seatMaps) = await SelectionDesk.BatchAsync(lab);

        var batch = await EvaluateAsync(service, Seat("paid", await SeatIdAsync(seatMaps, "3A", Adult)), Seat("included", await SeatIdAsync(seatMaps, "3B", Child)));
        var paid = batch.Lines[0].Seat!;
        var included = batch.Lines[1].Seat!;

        Assert.Equal((SelectionEvaluationStatus.Accepted, true, (decimal?)15m, ""), Verdict(batch.Lines[0]));
        Assert.Equal((SelectionEvaluationStatus.Accepted, true, (decimal?)0m, ""), Verdict(batch.Lines[1]));
        Assert.Equal((false, "SEAT_STANDARD", "3A", Adult), (paid.IsIncludedSeat, paid.Offer.Candidate!.ServiceDefinitionRef, paid.Offer.SeatNumber, paid.Offer.TravellerRef));
        Assert.Equal((true, (CanonicalAncillaryOfferCandidate?)null, "3B", Child), (included.IsIncludedSeat, included.Offer.Candidate, included.Offer.SeatNumber, included.Offer.TravellerRef));
        Assert.Equal((paid.Flight.Flight.FlightId, paid.Flight.Flight.FlightCapacityId), (included.Flight.Flight.FlightId, included.Flight.Flight.FlightCapacityId));
        Assert.Equal((true, (decimal?)15m), (batch.Basket.AllReady, batch.Basket.Total));
    }

    [Fact]
    public async Task SEAT_05_SEAT_06_one_seat_for_two_travellers_and_two_seats_for_one_traveller_on_a_flight_are_basket_conflicts()
    {
        var lab = BagAndSeatLab();
        var (service, seatMaps) = await SelectionDesk.BatchAsync(lab);

        var sameSeat = await EvaluateAsync(service, Seat("adult", await SeatIdAsync(seatMaps, "3A", Adult)), Seat("child", await SeatIdAsync(seatMaps, "3A", Child)));
        var sameTraveller = await EvaluateAsync(service, Seat("window", await SeatIdAsync(seatMaps, "3A", Adult)), Seat("middle", await SeatIdAsync(seatMaps, "3B", Adult)));

        Assert.All(sameSeat.Lines, line => Assert.Equal((SelectionEvaluationStatus.Rejected, false, ShoppingReasonCodes.SeatSelectedTwice), (line.Status, line.CanPurchase, line.ReasonCodes.Single())));
        Assert.All(sameTraveller.Lines, line => Assert.Equal((SelectionEvaluationStatus.Rejected, false, ShoppingReasonCodes.TravellerSeatSelectedTwice), (line.Status, line.CanPurchase, line.ReasonCodes.Single())));
    }

    [Fact]
    public async Task SEAT_07_SEAT_13_a_seat_taken_since_the_map_or_an_invented_seat_id_is_not_offered()
    {
        var lab = BagAndSeatLab();
        var (_, seatMaps) = await SelectionDesk.BatchAsync(lab);
        var window = await SeatIdAsync(seatMaps, "3A", Adult);
        var (taken, _) = await SelectionDesk.BatchAsync(lab, new FixedOccupiedSeats("3A"));

        var batch = await EvaluateAsync(taken, Seat("taken", window), Seat("invented", new string('A', 43)));

        Assert.All(batch.Lines, line => Assert.Equal((SelectionEvaluationStatus.Rejected, false, (decimal?)null, ShoppingReasonCodes.SeatSelectionNotOffered), Verdict(line)));
    }

    [Fact]
    public async Task SEAT_22_one_basket_holds_a_bag_and_a_seat_and_adds_their_exact_totals()
    {
        var lab = BagAndSeatLab();
        var (service, seatMaps) = await SelectionDesk.BatchAsync(lab);

        var batch = await EvaluateAsync(service, Bag("bag", await BagRefAsync(lab, Adult), 2), Seat("seat", await SeatIdAsync(seatMaps, "3A", Adult)));

        Assert.Equal(new[] { SelectionLineKind.Ancillary, SelectionLineKind.SeatMap }, batch.Lines.Select(line => line.Kind));
        Assert.Equal((true, (decimal?)235m), (batch.Basket.AllReady, batch.Basket.Total));
    }

    [Fact]
    public async Task S2_BATCH_each_surface_maps_the_same_canonical_lines_with_its_own_identifiers()
    {
        var lab = BagAndSeatLab();
        var (service, seatMaps) = await SelectionDesk.BatchAsync(lab);
        var batch = await EvaluateAsync(service, Bag("bag", await BagRefAsync(lab, Adult), 1), Seat("seat", await SeatIdAsync(seatMaps, "3A", Child)));

        var backoffice = BackofficeSelectionBatchMapper.Map(batch);
        var api = ApiSelectionBatchMapper.Map(batch, new ApiOfferReferences(batch.Shopping.Offer, new Dictionary<int, string>(), new Dictionary<int, string>(), new Dictionary<int, string>(), new Dictionary<long, BaggageSpecificationArgs>()));
        var otaPanel = OtaPanelSelectionBatchMapper.Map(batch);

        Assert.Equal(("Ancillary", "Accepted", true, (string?)null), (backoffice.Lines[0].Kind.Name, backoffice.Lines[0].Status.Name, backoffice.Lines[0].Ancillary!.CanPurchase, backoffice.Lines[0].Ancillary!.PricedSelectionRef));
        Assert.Equal((Child, batch.Lines[1].Seat!.Flight.Flight.FlightId.ToString()), (backoffice.Lines[1].Seat!.TravelerId, backoffice.Lines[1].Seat!.SegmentId));
        Assert.Equal((ApiSelectionBatchMapper.AncillarySelectionBatch, "SeatMap", "2", "1"), (api.Data.Type, api.Data.Lines[1].Kind, api.Data.Lines[1].Seat!.TravelerId, api.Data.Lines[1].Seat!.SegmentId));
        Assert.Equal((Child, batch.Lines[1].Seat!.Flight.FlightRef, 15m), (otaPanel.Lines[1].Seat!.TravelerId, otaPanel.Lines[1].Seat!.SegmentId, otaPanel.Lines[1].Seat!.Total!.Value));
        Assert.Equal((true, (decimal?)125m), (otaPanel.Basket.AllReady, otaPanel.Basket.Total));
    }

    [Theory]
    [InlineData("empty")]
    [InlineData("duplicate-client-id")]
    [InlineData("ancillary-without-item")]
    [InlineData("seat-with-selection")]
    [InlineData("no-kind")]
    public async Task S2_BATCH_a_malformed_basket_is_refused_before_any_source_is_read(string shape)
    {
        var (service, _) = await SelectionDesk.BatchAsync(new ShoppingLab());
        SelectionLineRequest[] lines = shape switch
        {
            "empty" => [],
            "duplicate-client-id" => [Seat("same", "seat-a"), Seat("same", "seat-b")],
            "ancillary-without-item" => [new("line", SelectionLineKind.Ancillary, null, new AncillarySelection(), null)],
            "seat-with-selection" => [new("line", SelectionLineKind.SeatMap, null, new AncillarySelection(), "seat-a")],
            _ => [new("line", null, "item", null, null)]
        };

        var refused = await Assert.ThrowsAsync<BusinessException>(() => EvaluateAsync(service, lines));

        Assert.Equal((16801, 422), (refused.Code, refused.HttpStatus));
    }
}
