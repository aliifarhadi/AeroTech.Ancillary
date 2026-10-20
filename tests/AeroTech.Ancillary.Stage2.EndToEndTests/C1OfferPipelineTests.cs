using System.Text.Json.Nodes;
using AeroTech.Ancillary.Stage2.EndToEndTests.Host;
using AeroTech.Ancillary.Stage2.EndToEndTests.Scenario;
using Xunit;
using static AeroTech.Ancillary.Stage2.EndToEndTests.Scenario.Stage2Flows;

namespace AeroTech.Ancillary.Stage2.EndToEndTests;

public class C1OfferPipelineTests
{
    private static string AgencyOffer(Stage2World world) => world.Ports.AirAvail.Publish(TestAirAvail.PartnerApiChannel, E2EIdentities.CustomerId);

    [Fact]
    public async Task C1_01_the_real_host_lists_offer_candidates_through_the_api_route_without_any_external_call()
    {
        await using var world = await Stage2World.StartAsync(Stage2Catalog.Standard());
        var offerId = AgencyOffer(world);

        var list = await ListAsync(world, Api, Api, offerId);
        var services = Stage2Json.ApiServices(list.Data);

        Assert.Equal(200, list.Status);
        Assert.Equal(["A01", "A06", "A24"], services.Select(service => service.VariantCode).Distinct().Order(StringComparer.Ordinal));
        Assert.DoesNotContain(services, service => service.VariantCode is "A07" or "A08");
        Assert.Equal(1, world.Ports.AirAvail.Calls);
    }

    [Fact]
    public async Task C1_02_an_inline_plan_needs_no_details_a_typed_product_opens_details_on_click_and_single_evaluate_prices_the_choice()
    {
        await using var world = await Stage2World.StartAsync(Stage2Catalog.Standard());
        var offerId = AgencyOffer(world);
        var services = await ServicesAsync(world, Api, offerId);
        var wifi = Pick(services, "A24", Traveller(Api, Adult));
        var bike = Pick(services, "A06", Traveller(Api, Adult));
        var plan = wifi.Node["ancillary"]!["optionGroups"]![0]!;

        Assert.Equal(("Inline", false), (wifi.Node["ancillary"]!["interaction"]!["mode"]!.GetValue<string>(), wifi.Node["ancillary"]!["interaction"]!["requiresDetails"]!.GetValue<bool>()));
        Assert.Equal(("planCode", true), (plan["fieldName"]!.GetValue<string>(), plan["inlineChoicesIncluded"]!.GetValue<bool>()));
        Assert.True(bike.Node["ancillary"]!["interaction"]!["requiresDetails"]!.GetValue<bool>());

        var details = await DetailsAsync(world, Api, bike.OfferItemRef, offerId);

        Assert.Equal(200, details.Status);
        Assert.Equal(bike.OfferItemRef, details.Data["data"]!["id"]!.GetValue<string>());

        var choice = plan["choices"]![0]!["value"]!.GetValue<string>();
        var evaluated = await EvaluateAsync(world, Api, offerId, wifi.OfferItemRef, new { planCode = choice, deviceCount = 1 });

        Assert.Equal(200, evaluated.Status);
        Assert.Equal(("Accepted", true), (evaluated.Data["data"]!["status"]!.GetValue<string>(), evaluated.Data["data"]!["canPurchase"]!.GetValue<bool>()));
        Assert.Null(evaluated.Data["data"]!["pricedSelectionRef"]);
    }

    [Fact]
    public async Task C1_03_an_offer_batch_is_a_priced_preview_that_is_never_ready_and_carries_no_reference()
    {
        await using var world = await Stage2World.StartAsync(Stage2Catalog.Standard());
        var offerId = AgencyOffer(world);
        var services = await ServicesAsync(world, Api, offerId);
        var wifi = Pick(services, "A24", Traveller(Api, Adult));
        var bag = Pick(services, "A01", Traveller(Api, Adult));

        var batch = await BatchAsync(
            world,
            Api,
            [AncillaryLine("wifi", wifi.OfferItemRef, new { planCode = "WIFI_FULL_FLIGHT", deviceCount = 1 }), AncillaryLine("bag", bag.OfferItemRef, new { quantity = 1 })],
            offerId);

        Assert.Equal(200, batch.Status);
        Assert.All(batch.Lines, line => Assert.Equal(("Accepted", true, (string?)null), (line.Status, line.CanPurchase, line.PricedSelectionRef)));
        Assert.False(batch.AllReady);
        Assert.Equal(["OrderBindingRequired"], batch.BasketReasonCodes);
        Assert.Equal(Stage2Catalog.WifiPrice + Stage2Catalog.BagPrice, batch.Total);
    }

    [Fact]
    public async Task C1_04_the_surface_policies_and_the_offer_scope_refuse_a_missing_identity_a_foreign_surface_a_foreign_agency_and_a_foreign_country()
    {
        await using var world = await Stage2World.StartAsync(Stage2Catalog.Standard());
        var offerId = AgencyOffer(world);
        var body = Source(offerId, null);

        var anonymous = await Stage2World.PostAsync(world.Anonymous(), "Api/v1/AncillaryOffers", body);
        var panelOnApi = await Stage2World.PostAsync(world.Surface(E2EIdentities.OtaPanel), "Api/v1/AncillaryOffers", body);
        var backofficeOnApi = await Stage2World.PostAsync(world.Surface(E2EIdentities.Backoffice), "Api/v1/AncillaryOffers", body);
        var foreignAgency = await Stage2World.PostAsync(world.Surface(E2EIdentities.ForeignApi), "Api/v1/AncillaryOffers", body);
        var foreignCountry = await Stage2World.PostAsync(world.Surface(E2EIdentities.Api, countryId: 116), "Api/v1/AncillaryOffers", body);
        var backofficeOnAgencyOffer = await Stage2World.PostAsync(world.Surface(E2EIdentities.Backoffice), "Backoffice/v1/AncillaryOffers", body);

        Assert.Equal(401, anonymous.Status);
        Assert.Equal(403, panelOnApi.Status);
        Assert.Equal(403, backofficeOnApi.Status);
        Assert.Equal((403, 16809), (foreignAgency.Status, foreignAgency.ErrorCode));
        Assert.Equal((403, 16809), (foreignCountry.Status, foreignCountry.ErrorCode));
        Assert.Equal((403, 16809), (backofficeOnAgencyOffer.Status, backofficeOnAgencyOffer.ErrorCode));
    }

    [Fact]
    public async Task C1_05_an_unknown_offer_and_an_unavailable_airavail_fail_honestly_through_the_real_provider_adapter()
    {
        await using var world = await Stage2World.StartAsync(Stage2Catalog.Standard());
        var known = AgencyOffer(world);
        var unknown = TestAirAvail.OfferId(TestAirAvail.PartnerApiChannel, E2EIdentities.CustomerId, 99);

        var refused = await ListAsync(world, Api, Api, unknown);

        world.Ports.AirAvail.Unavailable = true;

        var down = await ListAsync(world, Api, Api, known);

        Assert.Equal((422, 16804), (refused.Status, refused.ErrorCode));
        Assert.Equal((502, 16806), (down.Status, down.ErrorCode));
    }

    [Fact]
    public async Task C1_06_the_sql_guard_stops_a_route_that_would_reach_sql_server_so_no_shopping_route_can_hide_a_database_call()
    {
        await using var world = await Stage2World.StartAsync(Stage2Catalog.Standard());

        world.ExpectEscape("SQL AncillaryQueryDbContext: connection opening");

        using var response = await world.Surface(E2EIdentities.Backoffice).GetAsync("Backoffice/v1/Suppliers/Paginated?PageNumber=1&PageSize=5");

        Assert.Equal(500, (int)response.StatusCode);
        Assert.Contains("SQL AncillaryQueryDbContext: connection opening", world.Ports.Guard.Escapes);
        Assert.IsType<JsonObject>(JsonNode.Parse(await response.Content.ReadAsStringAsync()));
    }
}
