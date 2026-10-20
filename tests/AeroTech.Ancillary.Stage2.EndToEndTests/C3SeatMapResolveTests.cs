using AeroTech.Ancillary.Stage2.EndToEndTests.Host;
using AeroTech.Ancillary.Stage2.EndToEndTests.Scenario;
using Xunit;
using static AeroTech.Ancillary.Stage2.EndToEndTests.Scenario.Stage2Flows;

namespace AeroTech.Ancillary.Stage2.EndToEndTests;

public class C3SeatMapResolveTests
{
    [Fact]
    public async Task C3_01_the_seat_map_of_an_order_prices_a_category_seat_frees_an_uncategorised_seat_and_closes_taken_and_other_cabin_seats()
    {
        await using var world = await Stage2World.StartAsync(Stage2Catalog.Standard());
        var orderId = await CreateOrderAsync(world, world.Ports.AirAvail.Publish(TestAirAvail.PartnerApiChannel, E2EIdentities.CustomerId));

        world.Ports.Occupancy.Occupy(FirstFlightId, "3C");

        var maps = await SeatMapsAsync(world, Api, orderId: orderId);

        Assert.Equal(200, maps.Status);
        Assert.Equal(2, maps.Data["data"]!.AsArray().Count);
        Assert.Equal(43, SeatId(maps.Data, 0, "3A", Traveller(Api, Adult))!.Length);
        Assert.Equal("AVAILABLE", SeatStatus(maps.Data, 0, "3A", Traveller(Api, Adult)));
        Assert.Equal("AVAILABLE", SeatStatus(maps.Data, 0, "3B", Traveller(Api, Child)));
        Assert.Equal("OCCUPIED", SeatStatus(maps.Data, 0, "3C", Traveller(Api, Adult)));
        Assert.Equal("BLOCKED", SeatStatus(maps.Data, 0, "1A", Traveller(Api, Adult)));
    }

    [Fact]
    public async Task C3_02_a_paid_and_a_free_seat_on_the_order_are_signed_together_and_the_same_seat_twice_signs_nothing()
    {
        await using var world = await Stage2World.StartAsync(Stage2Catalog.Standard());
        var orderId = await CreateOrderAsync(world, world.Ports.AirAvail.Publish(TestAirAvail.PartnerApiChannel, E2EIdentities.CustomerId));
        var maps = (await SeatMapsAsync(world, Api, orderId: orderId)).Data;
        var paid = SeatId(maps, 0, "3A", Traveller(Api, Adult))!;
        var free = SeatId(maps, 0, "3B", Traveller(Api, Child))!;
        var sameSeatForChild = SeatId(maps, 0, "3A", Traveller(Api, Child))!;

        var basket = await BatchAsync(world, Api, [SeatLine("paid", paid), SeatLine("free", free)], orderId: orderId);
        var clash = await BatchAsync(world, Api, [SeatLine("adult", paid), SeatLine("child", sameSeatForChild)], orderId: orderId);

        Assert.True(basket.AllReady);
        Assert.Equal(2, basket.Refs.Count);
        Assert.Equal(Stage2Catalog.SeatPrice, basket.Total);
        Assert.False(clash.AllReady);
        Assert.Empty(clash.Refs);
        Assert.All(clash.Lines, line => Assert.Contains("SeatSelectedTwice", line.ReasonCodes));
    }

    [Fact]
    public async Task C3_03_a_missing_flight_occupancy_and_an_unreachable_flightflow_block_the_seat_map_instead_of_showing_free_seats()
    {
        await using var world = await Stage2World.StartAsync(Stage2Catalog.Standard());
        var orderId = await CreateOrderAsync(world, world.Ports.AirAvail.Publish(TestAirAvail.PartnerApiChannel, E2EIdentities.CustomerId));

        world.Ports.Occupancy.MissingFlights.Add(SecondFlightId);

        var missing = await SeatMapsAsync(world, Api, orderId: orderId);

        world.Ports.Occupancy.MissingFlights.Clear();
        world.Ports.Occupancy.Unavailable = true;

        var down = await SeatMapsAsync(world, Api, orderId: orderId);

        Assert.Equal((502, 16814), (missing.Status, missing.ErrorCode));
        Assert.Equal((502, 16814), (down.Status, down.ErrorCode));
    }

    [Fact]
    public async Task C3_04_an_exit_row_seat_is_signed_only_for_a_test_verified_adult_who_accepts_the_terms()
    {
        await using var world = await Stage2World.StartAsync(Stage2Catalog.Standard(exitRow: true), verifiedExitRows: true);
        var orderId = await CreateOrderAsync(world, world.Ports.AirAvail.Publish(TestAirAvail.PartnerApiChannel, E2EIdentities.CustomerId));
        var maps = (await SeatMapsAsync(world, Api, orderId: orderId)).Data;
        var exitRow = SeatId(maps, 0, "3A", Traveller(Api, Adult))!;
        var bag = Pick(await ServicesAsync(world, Api, orderId: orderId), "A01", Traveller(Api, Adult));

        var accepted = await BatchAsync(world, Api, [SeatLine("exit", exitRow, exitRowTermsAccepted: true)], orderId: orderId);
        var silent = await BatchAsync(world, Api, [SeatLine("exit", exitRow)], orderId: orderId);
        var misplaced = await Stage2World.PostAsync(
            world.Surface(Api),
            "Api/v1/AncillaryOffers/Selections/EvaluateBatch",
            new { orderId, selections = new[] { new { clientSelectionId = "bag", kind = "Ancillary", offerItemRef = bag.OfferItemRef, selection = new { quantity = 1 }, exitRowTermsAccepted = true } } });

        Assert.Equal("BLOCKED", SeatStatus(maps, 0, "3A", Traveller(Api, Child)));
        Assert.True(accepted.AllReady);
        Assert.Single(accepted.Refs);
        Assert.Equal(Stage2Catalog.ExitRowPrice, accepted.Total);
        Assert.False(silent.AllReady);
        Assert.Empty(silent.Refs);
        Assert.Equal((422, 16801), (misplaced.Status, misplaced.ErrorCode));

        var added = await AddAsync(world, orderId, 1, accepted.Refs, "exit-row");
        var seat = (await OrderAsync(world, orderId))["services"]!.AsArray().Single()!;

        Assert.Equal(201, added.Status);
        Assert.Equal(("A08", "3A", 1001L, true), (seat["variantCode"]!.GetValue<string>(), seat["seatNumber"]!.GetValue<string>(), seat["orderTravellerId"]!.GetValue<long>(), seat["selection"]!["exitRowTermsAccepted"]!.GetValue<bool>()));
    }
}
