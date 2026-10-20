using System.Text.Json.Nodes;
using AeroTech.Ancillary.Stage2.EndToEndTests.Host;
using AeroTech.Ancillary.Stage2.EndToEndTests.Ordering;
using AeroTech.Ancillary.Stage2.EndToEndTests.Scenario;
using Xunit;
using static AeroTech.Ancillary.Stage2.EndToEndTests.Scenario.Stage2Flows;

namespace AeroTech.Ancillary.Stage2.EndToEndTests;

public class C4OrderingAddTests
{
    private static string AgencyOffer(Stage2World world) => world.Ports.AirAvail.Publish(TestAirAvail.PartnerApiChannel, E2EIdentities.CustomerId);

    private static JsonArray Services(JsonNode order) => order["services"]!.AsArray();

    private static JsonArray Seats(JsonNode order) => order["seats"]!.AsArray();

    [Fact]
    public async Task C4_F1_an_offer_preview_becomes_an_order_whose_new_candidates_are_signed_added_through_resolve_and_read_back()
    {
        await using var world = await Stage2World.StartAsync(Stage2Catalog.Standard());
        var offerId = AgencyOffer(world);
        var offerServices = await ServicesAsync(world, Api, offerId);
        var preview = await BatchAsync(
            world,
            Api,
            [AncillaryLine("wifi", Pick(offerServices, "A24", Traveller(Api, Adult)).OfferItemRef, new { planCode = "WIFI_FULL_FLIGHT", deviceCount = 1 })],
            offerId);

        var orderId = await CreateOrderAsync(world, offerId);
        var before = await OrderAsync(world, orderId);
        var orderServices = await ServicesAsync(world, Api, orderId: orderId);
        var wifi = Pick(orderServices, "A24", Traveller(Api, Adult));
        var bag = Pick(orderServices, "A01", Traveller(Api, Adult));
        var planRef = wifi.Node["ancillary"]!["optionGroups"]![0]!["choices"]![0]!["optionRef"]!.GetValue<string>();
        var batch = await BatchAsync(
            world,
            Api,
            [
                AncillaryLine("wifi", wifi.OfferItemRef, new { planCode = "WIFI_FULL_FLIGHT", deviceCount = 1 }, new Dictionary<string, string> { ["planCode"] = planRef }),
                AncillaryLine("bag", bag.OfferItemRef, new { quantity = 1 })
            ],
            orderId: orderId);
        var added = await AddAsync(world, orderId, 1, batch.Refs, "f1-add");
        var after = await OrderAsync(world, orderId);

        Assert.False(preview.AllReady);
        Assert.Empty(preview.Refs);
        Assert.DoesNotContain(wifi.OfferItemRef, offerServices.Select(service => service.OfferItemRef));
        Assert.True(batch.AllReady);
        Assert.Equal(2, batch.Refs.Count);
        Assert.Equal((1, 0), (before["commercialVersion"]!.GetValue<int>(), Services(before).Count));
        Assert.Equal(201, added.Status);
        Assert.Equal(2, after["commercialVersion"]!.GetValue<int>());

        var services = Services(after).Select(node => node!).ToList();
        var addedWifi = services.Single(service => service["variantCode"]!.GetValue<string>() == "A24");
        var addedBag = services.Single(service => service["variantCode"]!.GetValue<string>() == "A01");

        Assert.Equal(2, services.Count);
        Assert.All(services, service =>
        {
            Assert.Equal((1001L, Adult, Stage2Catalog.ProviderKey, 2), (service["orderTravellerId"]!.GetValue<long>(), service["travellerRef"]!.GetValue<string>(), service["fulfillmentProviderKey"]!.GetValue<string>(), service["createdAtVersion"]!.GetValue<int>()));
            Assert.Equal(Stage2Catalog.Irr, service["currencyId"]!.GetValue<int>());
            Assert.Contains(FirstFlightId, service["flightIds"]!.AsArray().Select(flight => flight!.GetValue<long>()));
        });
        Assert.Equal(Stage2Catalog.WifiPrice, addedWifi["lineTotal"]!.GetValue<decimal>());
        Assert.Equal("WIFI_FULL_FLIGHT", addedWifi["selection"]!["planCode"]!.GetValue<string>());
        Assert.Equal((Stage2Catalog.BagPrice, 1), (addedBag["lineTotal"]!.GetValue<decimal>(), addedBag["quantity"]!.GetValue<int>()));
    }

    [Fact]
    public async Task C4_F2_an_existing_pre_issue_order_adds_two_pieces_for_one_traveller_with_its_exact_price_and_one_version_step()
    {
        await using var world = await Stage2World.StartAsync(Stage2Catalog.Standard());
        var orderId = await CreateOrderAsync(world, AgencyOffer(world));
        var bag = Pick(await ServicesAsync(world, OtaPanel, orderId: orderId), "A01", Child);

        var batch = await BatchAsync(world, OtaPanel, [AncillaryLine("bags", bag.OfferItemRef, new { quantity = 2 })], orderId: orderId);
        var added = await AddAsync(world, orderId, 1, batch.Refs, "f2-add");
        var after = await OrderAsync(world, orderId);
        var service = Services(after).Single()!;

        Assert.Equal(201, added.Status);
        Assert.Equal(2, after["commercialVersion"]!.GetValue<int>());
        Assert.Equal((1002L, Child, 2, 2 * Stage2Catalog.BagPrice), (service["orderTravellerId"]!.GetValue<long>(), service["travellerRef"]!.GetValue<string>(), service["quantity"]!.GetValue<int>(), service["lineTotal"]!.GetValue<decimal>()));
        Assert.Equal(["B1"], service["portionRefs"]!.AsArray().Select(portion => portion!.GetValue<string>()));
    }

    [Fact]
    public async Task C4_F3_a_ticketed_order_with_open_coupons_adds_a_service_and_a_used_or_unknown_coupon_blocks_without_mutation()
    {
        await using var world = await Stage2World.StartAsync(Stage2Catalog.Standard());
        var orderId = await CreateOrderAsync(world, AgencyOffer(world));

        await MutateAsync(world, orderId, TestOrderMutations.Ticket);

        var open = Pick(await ServicesAsync(world, OtaPanel, orderId: orderId), "A01", Adult);
        var openBatch = await BatchAsync(world, OtaPanel, [AncillaryLine("bag", open.OfferItemRef, new { quantity = 1 })], orderId: orderId);
        var added = await AddAsync(world, orderId, 2, openBatch.Refs, "f3-open");

        await MutateAsync(world, orderId, TestOrderMutations.Coupon, Adult, "B1", openForUse: false);
        await MutateAsync(world, orderId, TestOrderMutations.Coupon, Child, "B1", openForUse: null);

        var before = await OrderAsync(world, orderId);
        var closed = await ServicesAsync(world, OtaPanel, orderId: orderId);
        var adultBags = closed.Where(service => service.VariantCode == "A01" && service.Travellers.SequenceEqual([Adult])).ToList();
        var childBag = closed.SingleOrDefault(service => service.VariantCode == "A01" && service.Travellers.SequenceEqual([Child]) && service.Node["coveredFlightRefs"]!.AsArray().Select(flight => flight!.GetValue<string>()).SequenceEqual(["B1:F1"]));
        var after = await OrderAsync(world, orderId);

        Assert.True(openBatch.AllReady);
        Assert.Equal(201, added.Status);
        Assert.Equal(3, before["commercialVersion"]!.GetValue<int>());
        Assert.DoesNotContain(adultBags, service => service.Node["coveredFlightRefs"]!.AsArray().Select(flight => flight!.GetValue<string>()).SequenceEqual(["B1:F1"]) && service.CanPurchase);
        Assert.True(childBag is null || !childBag.CanPurchase);
        Assert.Equal((3, 1), (after["commercialVersion"]!.GetValue<int>(), Services(after).Count));
    }

    [Fact]
    public async Task C4_F4_a_paid_and_an_included_seat_are_assigned_to_the_right_passenger_flight_and_capacity()
    {
        await using var world = await Stage2World.StartAsync(Stage2Catalog.Standard());
        var orderId = await CreateOrderAsync(world, AgencyOffer(world));
        var maps = (await SeatMapsAsync(world, Api, orderId: orderId)).Data;
        var batch = await BatchAsync(
            world,
            Api,
            [SeatLine("paid", SeatId(maps, 0, "3A", Traveller(Api, Adult))!), SeatLine("free", SeatId(maps, 0, "3B", Traveller(Api, Child))!)],
            orderId: orderId);
        var added = await AddAsync(world, orderId, 1, batch.Refs, "f4-seats");
        var after = await OrderAsync(world, orderId);
        var paid = Services(after).Single()!;
        var included = Seats(after).Single()!;
        var capacity = after["airServices"]!.AsArray().First(air => air!["flightId"]!.GetValue<long>() == FirstFlightId)!["flightCapacityId"]!.GetValue<long>();

        Assert.Equal(201, added.Status);
        Assert.Equal(2, after["commercialVersion"]!.GetValue<int>());
        Assert.Equal(("A07", "3A", 1001L, Stage2Catalog.SeatPrice, Stage2Catalog.ProviderKey), (paid["variantCode"]!.GetValue<string>(), paid["seatNumber"]!.GetValue<string>(), paid["orderTravellerId"]!.GetValue<long>(), paid["lineTotal"]!.GetValue<decimal>(), paid["fulfillmentProviderKey"]!.GetValue<string>()));
        Assert.Equal(capacity, paid["flightCapacityId"]!.GetValue<long>());
        Assert.Equal(("3B", 1002L, FirstFlightId, capacity, 0m), (included["seatNumber"]!.GetValue<string>(), included["orderTravellerId"]!.GetValue<long>(), included["flightId"]!.GetValue<long>(), included["flightCapacityId"]!.GetValue<long>(), included["fee"]!.GetValue<decimal>()));
        Assert.Equal(TestOrderStore.AirTransportProviderKey, included["fulfillmentProviderKey"]!.GetValue<string>());
    }

    [Fact]
    public async Task C4_05_a_repeated_add_replays_its_first_result_a_reused_key_with_another_payload_conflicts_and_an_old_version_is_refused()
    {
        await using var world = await Stage2World.StartAsync(Stage2Catalog.Standard());
        var orderId = await CreateOrderAsync(world, AgencyOffer(world));
        var services = await ServicesAsync(world, OtaPanel, orderId: orderId);
        var batch = await BatchAsync(world, OtaPanel, [AncillaryLine("bag", Pick(services, "A01", Adult).OfferItemRef, new { quantity = 1 })], orderId: orderId);
        var other = await BatchAsync(world, OtaPanel, [AncillaryLine("bag", Pick(services, "A01", Child).OfferItemRef, new { quantity = 1 })], orderId: orderId);

        var first = await AddAsync(world, orderId, 1, batch.Refs, "replay-key");
        var replay = await AddAsync(world, orderId, 1, batch.Refs, "replay-key");
        var reused = await AddAsync(world, orderId, 1, other.Refs, "replay-key");
        var stale = await AddAsync(world, orderId, 1, other.Refs, "another-key");
        var after = await OrderAsync(world, orderId);

        Assert.Equal((201, false), (first.Status, first.Body!["replayed"]!.GetValue<bool>()));
        Assert.Equal((201, true), (replay.Status, replay.Body!["replayed"]!.GetValue<bool>()));
        Assert.Equal(first.Body["addedServiceIds"]!.ToJsonString(), replay.Body["addedServiceIds"]!.ToJsonString());
        Assert.Equal((409, "IdempotencyKeyReusedWithAnotherPayload"), (reused.Status, reused.Error));
        Assert.Equal((409, "OrderVersionConflict"), (stale.Status, stale.Error));
        Assert.Equal((2, 1), (after["commercialVersion"]!.GetValue<int>(), Services(after).Count));
    }
}
