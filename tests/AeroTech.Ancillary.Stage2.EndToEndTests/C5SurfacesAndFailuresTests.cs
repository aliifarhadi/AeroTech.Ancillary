using System.Text.Json.Nodes;
using AeroTech.Ancillary.Stage2.EndToEndTests.Host;
using AeroTech.Ancillary.Stage2.EndToEndTests.Ordering;
using AeroTech.Ancillary.Stage2.EndToEndTests.Scenario;
using Xunit;
using static AeroTech.Ancillary.Stage2.EndToEndTests.Scenario.Stage2Flows;

namespace AeroTech.Ancillary.Stage2.EndToEndTests;

public class C5SurfacesAndFailuresTests
{
    private static string AgencyOffer(Stage2World world) => world.Ports.AirAvail.Publish(TestAirAvail.PartnerApiChannel, E2EIdentities.CustomerId);

    private static async Task<(long OrderId, BatchView Batch)> SignedBagAsync(Stage2World world, string traveller = Adult, string? offerId = null)
    {
        var orderId = await CreateOrderAsync(world, offerId ?? AgencyOffer(world));
        var bag = Pick(await ServicesAsync(world, OtaPanel, orderId: orderId), "A01", traveller);

        return (orderId, await BatchAsync(world, OtaPanel, [AncillaryLine("bag", bag.OfferItemRef, new { quantity = 1 })], orderId: orderId));
    }

    private static async Task<(int Version, int Services, int Seats)> StateAsync(Stage2World world, long orderId)
    {
        var order = await OrderAsync(world, orderId);

        return (order["commercialVersion"]!.GetValue<int>(), order["services"]!.AsArray().Count, order["seats"]!.AsArray().Count);
    }

    private static IEnumerable<string> ResolveReasons(E2EResponse add)
        => add.Body!["resolve"]!["results"]!.AsArray().SelectMany(result => result!["reasonCodes"]!.AsArray().Select(code => code!.GetValue<string>()));

    private static IEnumerable<string> ResolveStatuses(E2EResponse add)
        => add.Body!["resolve"]!["results"]!.AsArray().Select(result => result!["status"]!.GetValue<string>());

    [Fact]
    public async Task C5_F5_the_three_surfaces_show_the_same_canonical_offer_through_their_own_shapes_and_the_same_seat_map()
    {
        await using var world = await Stage2World.StartAsync(Stage2Catalog.Standard());
        var agencyOffer = AgencyOffer(world);
        var airlineOffer = world.Ports.AirAvail.Publish(TestAirAvail.BackofficeChannel, null);
        var canonical = new Dictionary<string, IReadOnlyList<(string Variant, string Traveller)>>();

        foreach (var (surface, offerId) in new[] { (Backoffice, airlineOffer), (Api, agencyOffer), (OtaPanel, agencyOffer) })
        {
            var services = await ServicesAsync(world, surface, offerId);
            var bag = Pick(services, "A01", Traveller(surface, Adult));
            var details = await DetailsAsync(world, surface, bag.OfferItemRef, offerId);
            var batch = await BatchAsync(world, surface, [AncillaryLine("bag", bag.OfferItemRef, new { quantity = 1 })], offerId);
            var maps = await SeatMapsAsync(world, surface, offerId);

            Assert.Equal(200, details.Status);
            Assert.Equal(("Accepted", false, Stage2Catalog.BagPrice), (batch.Lines.Single().Status, batch.AllReady, batch.Total));
            Assert.Equal(200, maps.Status);
            Assert.Equal(
                ["AVAILABLE", "AVAILABLE", "AVAILABLE", "BLOCKED"],
                Stage2Json.SeatOffers(maps.Data, 0).Where(row => row.Pricing["travelerId"]!.GetValue<string>() == Stage2Json.SeatOffers(maps.Data, 0).First().Pricing["travelerId"]!.GetValue<string>())
                    .Select(row => row.Pricing["seatAvailabilityStatus"]!.GetValue<string>())
                    .Order(StringComparer.Ordinal));
            canonical[surface] = services
                .Select(service => (service.VariantCode, service.Travellers.Count == 1 ? (service.Travellers[0] is "1" or Adult ? Adult : Child) : "order"))
                .Order()
                .ToList();
        }

        Assert.Equal(canonical[Backoffice], canonical[Api]);
        Assert.Equal(canonical[Api], canonical[OtaPanel]);
    }

    [Fact]
    public async Task C5_01_a_commercial_change_after_signing_makes_resolve_stale_and_the_add_inserts_nothing()
    {
        await using var world = await Stage2World.StartAsync(Stage2Catalog.Standard());
        var (orderId, batch) = await SignedBagAsync(world);

        await MutateAsync(world, orderId, TestOrderMutations.CommercialChange);

        var add = await AddAsync(world, orderId, 2, batch.Refs, "stale-version");

        Assert.Equal((409, "SelectionsNotCommittable"), (add.Status, add.Error));
        Assert.Equal(["Stale"], ResolveStatuses(add));
        Assert.Contains("OrderVersionChanged", ResolveReasons(add));
        Assert.Equal((2, 0, 0), await StateAsync(world, orderId));
    }

    [Fact]
    public async Task C5_02_a_price_change_after_signing_makes_resolve_stale_and_the_add_inserts_nothing()
    {
        await using var world = await Stage2World.StartAsync(Stage2Catalog.Standard(euroBag: true));
        var (orderId, batch) = await SignedBagAsync(world);

        Stage2Catalog.ChangeEuroRate(world.Lab, Stage2Catalog.EuroToRial + 5_000m);

        var add = await AddAsync(world, orderId, 1, batch.Refs, "price-change");

        Assert.True(batch.AllReady);
        Assert.Equal(Stage2Catalog.EuroBagPrice * Stage2Catalog.EuroToRial, batch.Total);
        Assert.Equal((409, "SelectionsNotCommittable"), (add.Status, add.Error));
        Assert.Equal(["Stale"], ResolveStatuses(add));
        Assert.Contains("PriceChanged", ResolveReasons(add));
        Assert.Equal((1, 0, 0), await StateAsync(world, orderId));
    }

    [Fact]
    public async Task C5_03_a_basket_with_one_invalid_line_signs_nothing_and_an_add_without_references_is_refused()
    {
        await using var world = await Stage2World.StartAsync(Stage2Catalog.Standard());
        var orderId = await CreateOrderAsync(world, AgencyOffer(world));
        var wifi = Pick(await ServicesAsync(world, OtaPanel, orderId: orderId), "A24", Adult);

        var batch = await BatchAsync(
            world,
            OtaPanel,
            [AncillaryLine("wifi", wifi.OfferItemRef, new { planCode = "WIFI_FULL_FLIGHT", deviceCount = 1 }), AncillaryLine("ghost", new string('x', 43), new { quantity = 1 })],
            orderId: orderId);
        var add = await AddAsync(world, orderId, 1, batch.Refs, "mixed");

        Assert.False(batch.AllReady);
        Assert.Empty(batch.Refs);
        Assert.Equal(("Accepted", true), (batch.Line("wifi").Status, batch.Line("wifi").CanPurchase));
        Assert.Contains("OfferItemNotOffered", batch.Line("ghost").ReasonCodes);
        Assert.Equal((422, "AddRequestIncomplete"), (add.Status, add.Error));
        Assert.Equal((1, 0, 0), await StateAsync(world, orderId));
    }

    [Fact]
    public async Task C5_04_a_per_ticket_shared_fee_keeps_the_basket_unready_and_signs_nothing()
    {
        await using var world = await Stage2World.StartAsync(Stage2Catalog.Standard(wifiTicketFee: true));
        var orderId = await CreateOrderAsync(world, AgencyOffer(world));
        var wifi = Pick(await ServicesAsync(world, OtaPanel, orderId: orderId), "A24", Adult);

        var batch = await BatchAsync(world, OtaPanel, [AncillaryLine("wifi", wifi.OfferItemRef, new { planCode = "WIFI_FULL_FLIGHT", deviceCount = 1 })], orderId: orderId);

        Assert.False(batch.AllReady);
        Assert.Empty(batch.Refs);
        Assert.Contains("SharedFeeNotApplied", batch.BasketReasonCodes);
    }

    [Fact]
    public async Task C5_05_an_unlinked_traveller_signs_nothing_and_a_reference_of_another_order_inserts_nothing()
    {
        await using var world = await Stage2World.StartAsync(Stage2Catalog.Standard());
        var offerId = AgencyOffer(world);
        var orderId = await CreateOrderAsync(world, offerId);

        await MutateAsync(world, orderId, TestOrderMutations.UnlinkTraveller, Child);

        var services = await ServicesAsync(world, OtaPanel, orderId: orderId);
        var unlinked = await BatchAsync(
            world,
            OtaPanel,
            [AncillaryLine("adult", Pick(services, "A01", Adult).OfferItemRef, new { quantity = 1 }), AncillaryLine("child", Pick(services, "A01", Child).OfferItemRef, new { quantity = 1 })],
            orderId: orderId);
        var (otherOrderId, otherBatch) = await SignedBagAsync(world, offerId: offerId);
        var crossed = await AddAsync(world, orderId, 1, otherBatch.Refs, "crossed");

        Assert.False(unlinked.AllReady);
        Assert.Empty(unlinked.Refs);
        Assert.Contains("OrderTravellerNotLinked", unlinked.Line("child").ReasonCodes);
        Assert.True(otherBatch.AllReady);
        Assert.Equal((409, "SelectionsNotCommittable"), (crossed.Status, crossed.Error));
        Assert.Equal(["Rejected"], ResolveStatuses(crossed));
        Assert.Contains("PricedSelectionForAnotherOrder", ResolveReasons(crossed));
        Assert.Equal((1, 0, 0), await StateAsync(world, orderId));
        Assert.Equal((1, 0, 0), await StateAsync(world, otherOrderId));
    }

    [Fact]
    public async Task C5_06_a_tampered_reference_is_rejected_and_an_expired_reference_is_refused_without_insertion()
    {
        await using var world = await Stage2World.StartAsync(Stage2Catalog.Standard());
        var (orderId, batch) = await SignedBagAsync(world);
        var signed = batch.Refs.Single();
        var tampered = signed[..^4] + (signed.EndsWith("AAAA", StringComparison.Ordinal) ? "BBBB" : "AAAA");

        var forged = await AddAsync(world, orderId, 1, [tampered], "forged");

        world.Ports.Clock.Now = world.Ports.Clock.Now.AddMinutes(31);

        var expired = await AddAsync(world, orderId, 1, batch.Refs, "expired");

        Assert.Equal((409, "SelectionsNotCommittable"), (forged.Status, forged.Error));
        Assert.Equal(["Rejected"], ResolveStatuses(forged));
        Assert.Contains("PricedSelectionNotAuthentic", ResolveReasons(forged));
        Assert.Equal((409, "SelectionsNotCommittable"), (expired.Status, expired.Error));
        Assert.Equal(["Expired"], ResolveStatuses(expired));
        Assert.Equal((1, 0, 0), await StateAsync(world, orderId));
    }

    [Fact]
    public async Task C5_07_a_seat_taken_after_signing_is_stale_at_resolve_and_the_whole_seat_basket_inserts_nothing()
    {
        await using var world = await Stage2World.StartAsync(Stage2Catalog.Standard());
        var orderId = await CreateOrderAsync(world, AgencyOffer(world));
        var maps = (await SeatMapsAsync(world, Api, orderId: orderId)).Data;
        var batch = await BatchAsync(
            world,
            Api,
            [SeatLine("paid", SeatId(maps, 0, "3A", Traveller(Api, Adult))!), SeatLine("free", SeatId(maps, 0, "3B", Traveller(Api, Child))!)],
            orderId: orderId);

        world.Ports.Occupancy.Occupy(FirstFlightId, "3A");

        var add = await AddAsync(world, orderId, 1, batch.Refs, "taken");

        Assert.True(batch.AllReady);
        Assert.Equal((409, "SelectionsNotCommittable"), (add.Status, add.Error));
        Assert.Contains("SeatSelectionNotOffered", ResolveReasons(add));
        Assert.Equal((1, 0, 0), await StateAsync(world, orderId));
    }

    [Fact]
    public async Task C5_08_an_ordering_outage_during_resolve_inserts_nothing_and_the_order_path_never_calls_airavail()
    {
        await using var world = await Stage2World.StartAsync(Stage2Catalog.Standard());
        var (orderId, batch) = await SignedBagAsync(world);
        var airAvailCalls = world.Ports.AirAvail.Calls;

        world.Ports.Orders.OrderingUnavailable = true;

        var orderingDown = await AddAsync(world, orderId, 1, batch.Refs, "ordering-down");

        world.Ports.Orders.OrderingUnavailable = false;
        world.Ports.AirAvail.Unavailable = true;

        var withoutAirAvail = await AddAsync(world, orderId, 1, batch.Refs, "airavail-down");

        Assert.Equal((502, "AncillaryResolveFailed", 503), (orderingDown.Status, orderingDown.Error, orderingDown.Body!["resolveStatus"]!.GetValue<int>()));
        Assert.Equal(16815, orderingDown.Body["resolveBody"]!["errors"]![0]!["code"]!.GetValue<int>());
        Assert.Equal(201, withoutAirAvail.Status);
        Assert.Equal(airAvailCalls, world.Ports.AirAvail.Calls);
        Assert.Equal((2, 1, 0), await StateAsync(world, orderId));
    }

    [Fact]
    public async Task C5_09_a_traveller_link_changed_after_signing_changes_the_order_source_so_resolve_is_stale_and_inserts_nothing()
    {
        await using var world = await Stage2World.StartAsync(Stage2Catalog.Standard());
        var (orderId, batch) = await SignedBagAsync(world);

        await MutateAsync(world, orderId, TestOrderMutations.UnlinkTraveller, Adult);

        var add = await AddAsync(world, orderId, 1, batch.Refs, "unlinked-after-signing");

        Assert.Equal((409, "SelectionsNotCommittable"), (add.Status, add.Error));
        Assert.Equal(["Stale"], ResolveStatuses(add));
        Assert.Contains("SourceVersionChanged", ResolveReasons(add));
        Assert.Equal((1, 0, 0), await StateAsync(world, orderId));
    }

    [Fact]
    public async Task C5_10_an_order_scoped_service_resolves_valid_but_the_ordering_simulator_refuses_it_because_ordering_has_no_traveller_less_service()
    {
        await using var world = await Stage2World.StartAsync(Stage2Catalog.Standard(orderScopedWifi: true));
        var orderId = await CreateOrderAsync(world, AgencyOffer(world));
        var wifi = (await ServicesAsync(world, OtaPanel, orderId: orderId)).Single(service => service.VariantCode == "A24" && service.Travellers.Count == 0);

        var batch = await BatchAsync(world, OtaPanel, [AncillaryLine("wifi", wifi.OfferItemRef, new { planCode = "WIFI_FULL_FLIGHT", deviceCount = 1 })], orderId: orderId);
        var add = await AddAsync(world, orderId, 1, batch.Refs, "order-scope");
        var problems = add.Body!["problems"]!.AsArray().Select(problem => problem!["code"]!.GetValue<string>());

        Assert.True(batch.AllReady);
        Assert.Equal(["Valid"], ResolveStatuses(add));
        Assert.Equal((409, "SelectionsNotCommittable"), (add.Status, add.Error));
        Assert.Contains("OrderScopedServiceNotRepresentable", problems);
        Assert.Equal((1, 0, 0), await StateAsync(world, orderId));
    }
}
