using System.Text.Json;
using System.Text.Json.Nodes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.SelectionSnapshotAggregate.Commands.PriceAncillaryOffers;
using AeroTech.Ancillary.Domain.AncillaryPricingAggregate;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate;
using AeroTech.Ancillary.Domain.SelectionSnapshotAggregate;
using AeroTech.Ancillary.Domain.SelectionSnapshotAggregate.Arguments;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.ResolveAncillarySelection.Service;
using AeroTech.Ancillary.Query.Shopping.SeatMaps;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Dto.Service;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.ResolveSeatMapSelections;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.ResolveSeatMapSelections.Service;
using AeroTech.Ancillary.Shopping.Offers;
using AeroTech.Ancillary.Shopping.Tests.Fixtures;
using AeroTech.Framework.Core.Domain.Exceptions;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;

namespace AeroTech.Ancillary.Shopping.Tests;

public sealed class R4ResolveContractTests
{
    private const string OrderReference = "123456";
    private const long Order = 123456;
    private const int Version = 3;
    private const int NotFound = 16811;
    private const int NotSellable = 16820;

    private static readonly string Binding = new('a', 43);

    private static ResolveSeatMapSelectionsService SeatResolver(V8Lab lab)
        => new(lab.Resolver, lab.Shop.Catalog, lab.Shop.Catalog, lab.Shop.Catalog);

    private static async Task<ServiceSeatMapResolutionDto> ResolveSeatsAsync(V8Lab lab, string selectionId)
        => ServiceSeatMapResolutionMapper.Map(await SeatResolver(lab).ExecuteAsync(selectionId));

    private static AncillaryOfferRow Row(IEnumerable<AncillaryOfferRow> rows, string variantCode)
        => Assert.Single(rows, row => row.Candidate.VariantCode == variantCode);

    private static SelectionSnapshotLineArgs Line(string id, AncillaryPricing pricing, AncillaryProvision provision, string traveller, string coverage)
        => new(id, pricing.Rates.Single().Id, provision.CoverageScope, traveller, coverage, Binding, Binding, 1, null);

    private static SelectionSnapshotLineArgs Seat(string traveller, long flightId, string seatNumber, AncillaryPricing pricing, AncillaryProvision provision)
        => new(SeatMapSelectionId.Encode(traveller, flightId, pricing.Id, seatNumber, ShoppingLab.Pos), pricing.Rates.Single().Id, ServiceCoverageScope.Sector, traveller, $"B1:F{flightId}", Binding, Binding, 1, null);

    private static string StoreForOffer(V8Lab lab, params SelectionSnapshotLineArgs[] lines)
    {
        var snapshot = SelectionSnapshot.CreateForOffer("s_offersnapshot000000000", V8Lab.Offer, lines, lab.Clock.Now, lab.Clock.Now.AddMinutes(15));

        lab.Shop.Catalog.Snapshots.Add(snapshot);

        return snapshot.Id;
    }

    private static string StoreForOrder(V8Lab lab, params SelectionSnapshotLineArgs[] lines)
    {
        var snapshot = SelectionSnapshot.CreateForOrder("s_ordersnapshot000000000", Order, Version, lines, lab.Clock.Now, lab.Clock.Now.AddMinutes(15));

        lab.Shop.Catalog.Snapshots.Add(snapshot);

        return snapshot.Id;
    }

    private static async Task<(V8Lab Lab, AncillaryPricing Pricing, AncillaryProvision Provision)> SeatCatalogAsync(decimal amount = 0m)
    {
        var lab = new V8Lab();
        var (_, provision, pricing) = lab.Sellable("A07", amount: amount, scope: ServiceCoverageScope.Sector);

        await Task.CompletedTask;

        return (lab, pricing, provision);
    }

    [Fact]
    public async Task T01_T04_a_general_offer_snapshot_states_its_source_and_every_line()
    {
        var lab = new V8Lab();
        lab.Sellable("A05", amount: 40m);
        lab.Sellable("A01", amount: 100m, maxQuantity: 2);
        lab.Sellable("A15", amount: 0m);
        var context = V8Lab.Context();
        var rows = await lab.ShopAsync(context);
        var selection = await lab.SubmitAsync(
            [
                V8Lab.Direct(Row(rows, "A05").OfferItemId!),
                V8Lab.Direct(Row(rows, "A01").OfferItemId!, 2),
                V8Lab.Line(Row(rows, "A15").OfferItemId!, AncillaryProfile.AssistedTravel, new AssistedTravelSelectionOptions("WCHS", null, null))
            ],
            context);

        var answer = ServiceResolvedSelectionsMapper.Map(await lab.Resolver.ResolveAsync(selection.SelectionId));

        Assert.Equal(ShoppingSourceKind.Offer, answer.SourceKind);
        Assert.Equal(V8Lab.Offer, answer.SourceReference);
        Assert.Equal(selection.ExpiresAtUtc, answer.ExpiresAtUtc);
        Assert.Equal(3, answer.SelectedLineCount);
        Assert.Null(answer.OrderId);
        Assert.Null(answer.OrderCommercialVersion);
        Assert.Equal(answer.SelectedLineCount, answer.Baggages.Count + answer.AssistedTravel.Count);
        Assert.Equal(2, answer.Baggages[1].Selected.Quantity);
        Assert.All(answer.Baggages, item => Assert.True(item.Pricing.IsUnitTotalComplete));
        Assert.NotNull(Assert.Single(answer.AssistedTravel).Selected.Options);
    }

    [Fact]
    public async Task T02_a_general_order_snapshot_states_the_order_as_text()
    {
        var lab = new V8Lab();
        var (_, provision, pricing) = lab.Sellable("A01", amount: 100m);
        var selectionId = StoreForOrder(lab, Line("d_line", pricing, provision, "1", "P1"));

        var answer = ServiceResolvedSelectionsMapper.Map(await lab.Resolver.ResolveAsync(selectionId));

        Assert.Equal(ShoppingSourceKind.Order, answer.SourceKind);
        Assert.Equal(OrderReference, answer.SourceReference);
        Assert.Equal(Order, answer.OrderId);
        Assert.Equal(Version, answer.OrderCommercialVersion);
        Assert.Equal(1, answer.SelectedLineCount);
    }

    [Fact]
    public async Task T03_completeness_comes_from_the_rate_and_never_from_the_amount()
    {
        var lab = new V8Lab();
        var (_, freeProvision, freePricing) = lab.Sellable("A11", amount: 0m);
        var definition = lab.Shop.Definition("A01", confirmation: ConfirmationRequirement.Immediate);
        var provision = lab.Shop.Provision(definition, disposition: CommercialDisposition.Paid);
        var pricing = lab.Shop.Price(definition, provision, ShoppingLab.Rate(0m, components: ShoppingLab.Fee("TK", 5m, FeeApplicationUnit.Ticket)));
        var selectionId = StoreForOffer(lab, Line("d_free", freePricing, freeProvision, "T1", "F1"), Line("d_open", pricing, provision, "T1", "P1"));

        var answer = ServiceResolvedSelectionsMapper.Map(await lab.Resolver.ResolveAsync(selectionId));

        var complete = Assert.Single(answer.Meals).Pricing;
        var incomplete = Assert.Single(answer.Baggages).Pricing;

        Assert.Equal((0m, true), (complete.UnitAmount, complete.IsUnitTotalComplete));
        Assert.Equal((0m, false), (incomplete.UnitAmount, incomplete.IsUnitTotalComplete));
    }

    [Fact]
    public async Task T05_T06_a_seat_offer_snapshot_states_its_source_and_keeps_every_selected_seat()
    {
        var (lab, pricing, provision) = await SeatCatalogAsync();
        var first = Seat("ADT-1", 101, "7A", pricing, provision);
        var second = Seat("CHD-1", 101, "7B", pricing, provision);
        var selectionId = StoreForOffer(lab, first, second);

        var answer = await ResolveSeatsAsync(lab, selectionId);

        Assert.Equal(ShoppingSourceKind.Offer, answer.SourceKind);
        Assert.Equal(V8Lab.Offer, answer.SourceReference);
        Assert.Equal(lab.Clock.Now.AddMinutes(15), answer.ExpiresAtUtc);
        Assert.Equal(2, answer.SelectedLineCount);
        Assert.Null(answer.OrderId);
        Assert.Equal([first.OfferItemId, second.OfferItemId], answer.Seats.Select(seat => seat.Id));
        Assert.All(answer.Seats, seat =>
        {
            Assert.Equal(ShoppingLab.Pos, seat.PointOfSaleId);
            Assert.Equal(answer.ExpiresAtUtc, seat.ExpiresAtUtc);
            Assert.Equal(pricing.Rates.Single().Id, seat.Pricing.Rate.Id);
            Assert.Equal((0m, true), (seat.Pricing.Rate.UnitTotal, seat.Pricing.Rate.IsUnitTotalComplete));
        });
    }

    [Fact]
    public async Task T07_a_seat_order_snapshot_stays_an_order_source()
    {
        var (lab, pricing, provision) = await SeatCatalogAsync(amount: 25m);
        var selectionId = StoreForOrder(lab, Seat("1", 101, "7A", pricing, provision));

        var answer = await ResolveSeatsAsync(lab, selectionId);

        Assert.Equal(ShoppingSourceKind.Order, answer.SourceKind);
        Assert.Equal(OrderReference, answer.SourceReference);
        Assert.Equal((Order, Version), (answer.OrderId!.Value, answer.OrderCommercialVersion!.Value));
        Assert.Equal(1, answer.SelectedLineCount);
        Assert.Equal(25m, Assert.Single(answer.Seats).Pricing.Rate.UnitTotal);
    }

    [Fact]
    public async Task T07_a_general_snapshot_is_never_a_seat_selection()
    {
        var lab = new V8Lab();
        var (_, provision, pricing) = lab.Sellable("A01", amount: 100m);
        var selectionId = StoreForOffer(lab, Line("d_line", pricing, provision, "T1", "P1"));

        var refused = await Assert.ThrowsAsync<BusinessException>(() => SeatResolver(lab).ExecuteAsync(selectionId));

        Assert.Equal(NotFound, refused.Code);
    }

    [Fact]
    public async Task T07_a_seat_snapshot_is_never_a_general_selection()
    {
        var (lab, pricing, provision) = await SeatCatalogAsync();
        var selectionId = StoreForOffer(lab, Seat("ADT-1", 101, "7A", pricing, provision));

        var refused = await Assert.ThrowsAsync<BusinessException>(() => lab.Resolver.ResolveAsync(selectionId));

        Assert.Equal(NotSellable, refused.Code);
    }

    [Fact]
    public async Task T07_one_unreadable_seat_line_fails_the_whole_snapshot()
    {
        var (lab, pricing, provision) = await SeatCatalogAsync();
        var selectionId = StoreForOffer(lab, Seat("ADT-1", 101, "7A", pricing, provision), Line("not-a-seat-id", pricing, provision, "CHD-1", "B1:F101"));

        var refused = await Assert.ThrowsAsync<BusinessException>(() => SeatResolver(lab).ExecuteAsync(selectionId));

        Assert.Equal(NotFound, refused.Code);
    }

    [Fact]
    public async Task T08_an_expired_snapshot_answers_nothing()
    {
        var (lab, pricing, provision) = await SeatCatalogAsync();
        var seats = StoreForOffer(lab, Seat("ADT-1", 101, "7A", pricing, provision));

        lab.Clock.Now = lab.Clock.Now.AddMinutes(15);

        await Assert.ThrowsAsync<BusinessException>(() => SeatResolver(lab).ExecuteAsync(seats));
        await Assert.ThrowsAsync<BusinessException>(() => lab.Resolver.ResolveAsync(seats));
    }

    [Fact]
    public async Task T08_a_retired_rate_answers_nothing()
    {
        var (lab, pricing, provision) = await SeatCatalogAsync();
        var seats = StoreForOffer(lab, Seat("ADT-1", 101, "7A", pricing, provision));

        lab.Shop.Catalog.Pricings.Clear();

        var refused = await Assert.ThrowsAsync<BusinessException>(() => SeatResolver(lab).ExecuteAsync(seats));

        Assert.Equal(NotFound, refused.Code);
    }

    [Fact]
    public async Task T08_a_retired_definition_or_provision_answers_nothing()
    {
        var (lab, pricing, provision) = await SeatCatalogAsync();
        var seats = StoreForOffer(lab, Seat("ADT-1", 101, "7A", pricing, provision));

        lab.Shop.Catalog.Provisions.Clear();

        var refused = await Assert.ThrowsAsync<BusinessException>(() => SeatResolver(lab).ExecuteAsync(seats));

        Assert.Equal(NotFound, refused.Code);
    }

    [Fact]
    public async Task T09_the_general_wire_adds_the_header_and_keeps_every_existing_member()
    {
        var lab = new V8Lab();
        var (_, provision, pricing) = lab.Sellable("A01", amount: 100m);
        var selectionId = StoreForOrder(lab, Line("d_line", pricing, provision, "1", "P1"));

        var answer = ServiceResolvedSelectionsMapper.Map(await lab.Resolver.ResolveAsync(selectionId));
        var wire = JsonNode.Parse(JsonSerializer.Serialize(answer, HostJson.Options))!;

        Assert.Equal("Order", wire["sourceKind"]!.GetValue<string>());
        Assert.Equal(OrderReference, wire["sourceReference"]!.GetValue<string>());
        Assert.Equal(1, wire["selectedLineCount"]!.GetValue<int>());
        Assert.NotNull(wire["expiresAtUtc"]);
        Assert.Equal(OrderReference, wire["orderId"]!.GetValue<string>());
        Assert.Equal(Version, wire["orderCommercialVersion"]!.GetValue<int>());
        Assert.All(
            new[] { "baggages", "meals", "pets", "assistedTravel", "airportServices", "priorityServices", "connectivity", "upgrades", "extraSeats", "travelInsurance" },
            family => Assert.NotNull(wire[family]));

        var line = wire["baggages"]![0]!["pricing"]!;

        Assert.True(line["isUnitTotalComplete"]!.GetValue<bool>());
        Assert.All(
            new[] { "pricingId", "pricingRateId", "pricingVersion", "currencyId", "quantity", "unitAmount", "totalAmount", "pricingUnit", "baseAmount", "taxLines", "feeLines" },
            member => Assert.NotNull(line[member]));
    }

    [Fact]
    public async Task T09_the_seat_wire_keeps_source_kind_a_plain_name()
    {
        var (lab, pricing, provision) = await SeatCatalogAsync();
        var seat = Seat("ADT-1", 101, "7A", pricing, provision);
        var selectionId = StoreForOffer(lab, seat);

        var wire = JsonNode.Parse(JsonSerializer.Serialize(await ResolveSeatsAsync(lab, selectionId), HostJson.Options))!;

        Assert.Equal("Offer", wire["sourceKind"]!.GetValue<string>());
        Assert.Equal(V8Lab.Offer, wire["sourceReference"]!.GetValue<string>());
        Assert.Equal(1, wire["selectedLineCount"]!.GetValue<int>());
        Assert.NotNull(wire["expiresAtUtc"]);
        Assert.Equal(seat.OfferItemId, wire["seats"]![0]!["id"]!.GetValue<string>());
        Assert.True(wire["seats"]![0]!["pricing"]!["rate"]!["isUnitTotalComplete"]!.GetValue<bool>());
    }
}
