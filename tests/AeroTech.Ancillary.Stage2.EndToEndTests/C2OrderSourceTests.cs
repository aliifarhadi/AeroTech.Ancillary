using AeroTech.Ancillary.Application.Orders;
using AeroTech.Ancillary.Providers;
using AeroTech.Ancillary.Providers.Orders;
using AeroTech.Ancillary.Stage2.EndToEndTests.Host;
using AeroTech.Ancillary.Stage2.EndToEndTests.Ordering;
using AeroTech.Ancillary.Stage2.EndToEndTests.Scenario;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using static AeroTech.Ancillary.Stage2.EndToEndTests.Scenario.Stage2Flows;

namespace AeroTech.Ancillary.Stage2.EndToEndTests;

public class C2OrderSourceTests
{
    [Fact]
    public async Task C2_01_production_keeps_the_not_connected_ordering_reader_and_only_the_test_host_reads_the_simulator_store()
    {
        var production = new ServiceCollection().AddProviders(new ConfigurationBuilder().Build());

        await using var world = await Stage2World.StartAsync(Stage2Catalog.Standard());

        Assert.Equal(typeof(NotConnectedOrderCommercialContextReader), production.Single(descriptor => descriptor.ServiceType == typeof(IOrderCommercialContextReader)).ImplementationType);
        Assert.IsType<TestOrderCommercialContextReader>(world.HostService<IOrderCommercialContextReader>());

        var offerId = world.Ports.AirAvail.Publish(TestAirAvail.PartnerApiChannel, E2EIdentities.CustomerId);
        var orderId = await CreateOrderAsync(world, offerId);
        var byOffer = await ServicesAsync(world, OtaPanel, offerId);
        var byOrder = await ServicesAsync(world, OtaPanel, orderId: orderId);

        Assert.Equal(byOffer.Select(service => (service.VariantCode, string.Join(',', service.Travellers))).Order(), byOrder.Select(service => (service.VariantCode, string.Join(',', service.Travellers))).Order());
        Assert.Empty(byOffer.Select(service => service.OfferItemRef).Intersect(byOrder.Select(service => service.OfferItemRef)));
    }

    [Fact]
    public async Task C2_02_an_order_bound_batch_is_ready_and_signs_a_reference_for_every_line_after_details_on_the_order_candidate()
    {
        await using var world = await Stage2World.StartAsync(Stage2Catalog.Standard());
        var orderId = await CreateOrderAsync(world, world.Ports.AirAvail.Publish(TestAirAvail.PartnerApiChannel, E2EIdentities.CustomerId));
        var services = await ServicesAsync(world, OtaPanel, orderId: orderId);
        var bag = Pick(services, "A01", Adult);
        var wifi = Pick(services, "A24", Child);

        var details = await DetailsAsync(world, OtaPanel, bag.OfferItemRef, orderId: orderId);
        var batch = await BatchAsync(
            world,
            OtaPanel,
            [AncillaryLine("bag", bag.OfferItemRef, new { quantity = 1 }), AncillaryLine("wifi", wifi.OfferItemRef, new { planCode = "WIFI_FULL_FLIGHT", deviceCount = 1 })],
            orderId: orderId);

        Assert.Equal(200, details.Status);
        Assert.True(batch.AllReady);
        Assert.Empty(batch.BasketReasonCodes);
        Assert.All(batch.Lines, line => Assert.Equal(("Accepted", true), (line.Status, line.CanPurchase)));
        Assert.Equal(2, batch.Refs.Distinct().Count());
    }

    [Fact]
    public async Task C2_03_an_unreachable_ordering_an_unknown_order_and_an_order_closed_for_changes_are_refused_honestly()
    {
        await using var world = await Stage2World.StartAsync(Stage2Catalog.Standard());
        var orderId = await CreateOrderAsync(world, world.Ports.AirAvail.Publish(TestAirAvail.PartnerApiChannel, E2EIdentities.CustomerId));

        var unknown = await ListAsync(world, OtaPanel, OtaPanel, orderId: orderId + 1000);

        world.Ports.Orders.OrderingUnavailable = true;

        var down = await ListAsync(world, OtaPanel, OtaPanel, orderId: orderId);

        world.Ports.Orders.OrderingUnavailable = false;
        await MutateAsync(world, orderId, TestOrderMutations.CloseForChanges);

        var closed = await ListAsync(world, OtaPanel, OtaPanel, orderId: orderId);
        var both = await Stage2World.PostAsync(world.Surface(OtaPanel), "OtaPanel/v1/AncillaryOffers", new { offerId = "x", orderId });

        Assert.Equal((403, 16809), (unknown.Status, unknown.ErrorCode));
        Assert.Equal((503, 16815), (down.Status, down.ErrorCode));
        Assert.Equal((422, 16801), (closed.Status, closed.ErrorCode));
        Assert.Equal((422, 16801), (both.Status, both.ErrorCode));
    }

    [Fact]
    public async Task C2_04_an_agency_order_is_not_shopped_by_another_agency_or_by_the_airline_backoffice_on_another_channel()
    {
        await using var world = await Stage2World.StartAsync(Stage2Catalog.Standard());
        var orderId = await CreateOrderAsync(world, world.Ports.AirAvail.Publish(TestAirAvail.PartnerApiChannel, E2EIdentities.CustomerId));

        var foreign = await ListAsync(world, E2EIdentities.ForeignApi, Api, orderId: orderId);
        var backoffice = await ListAsync(world, Backoffice, Backoffice, orderId: orderId);

        Assert.Equal((403, 16809), (foreign.Status, foreign.ErrorCode));
        Assert.Equal((403, 16809), (backoffice.Status, backoffice.ErrorCode));
    }
}
