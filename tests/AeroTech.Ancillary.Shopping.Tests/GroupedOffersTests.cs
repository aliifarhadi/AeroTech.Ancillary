using System.Text.Json;
using System.Text.Json.Serialization;
using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.Backoffice;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto.OtaPanel;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetGroupedAncillaryOffers.Backoffice;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetGroupedAncillaryOffers.OtaPanel;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.EvaluateSelection;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetAncillaryOfferDetail;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Queries.GetGroupedAncillaryOffers;
using AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Snapshots;
using AeroTech.Ancillary.Query.Shopping.Common.FlightOffers;
using AeroTech.Ancillary.RestApi.V1.AncillaryOffers.Controllers;
using AeroTech.Ancillary.RestApi.V1._Shared;
using AeroTech.Ancillary.Shopping.Tests.Fixtures;
using AeroTech.Framework.Presentation.Json;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.BackofficeDesk;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.OrderDesk;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.ShoppingLab;

namespace AeroTech.Ancillary.Shopping.Tests;

public class GroupedOffersTests
{
    private const int Irr = 70;

    private static readonly DeskScope Desk = new(Airline, Pos, null, null, Now);

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(), new LongToStringJsonConverter(), new NullableLongToStringJsonConverter() }
    };

    private static ShoppingLab Shop()
    {
        var lab = new ShoppingLab();

        lab.Catalog.CurrencyScales[Irr] = 0;

        var bag = lab.Definition("A01");
        var seat = lab.Definition("A07");
        var extraSeat = lab.Definition("A09");
        var wifi = lab.Definition("A24");
        var pet = lab.Definition("A13");

        lab.Price(bag, lab.Provision(bag), Rate(5_000_000m, Irr));
        lab.Price(seat, lab.Provision(seat), Rate(1_500_000m, Irr));
        lab.Price(extraSeat, lab.Provision(extraSeat), Rate(9_000_000m, Irr));
        lab.Price(wifi, lab.Provision(wifi), Rate(900_000m, Irr));
        lab.Price(pet, lab.Provision(pet), Rate(2_000_000m, Irr));

        return lab;
    }

    private static async Task<GroupedAncillaryOffersSnapshot> GroupedAsync(ShoppingLab lab, FixedOrders? orders = null, long? orderId = null)
    {
        var source = Sources(
            new FlightOfferShoppingSource(
                new FixedOffers(Desk.Sell(await OfferWire.ReadAsync(OfferWire.Captured()))),
                new TestOnlyAirportReference(),
                new TestOnlyAirFareReference(),
                new FixedClock { Now = Desk.EvaluatedAtUtc },
                new FlightOfferShoppingOptions(SourceContextTtl)),
            orders);
        var offers = Offers(source, lab);
        var details = new GetAncillaryOfferDetailService(offers, lab.Catalog, lab.Catalog, lab.Catalog);
        var service = new GetGroupedAncillaryOffersService(offers, lab.Catalog, new GroupedSelectionIdProvider(new EvaluateSelectionService(details, lab.Engine)));

        return await service.ExecuteAsync(new OfferQuery(orderId is null ? OfferWire.OfferId : null, OrderId: orderId), Desk);
    }

    [Fact]
    public async Task GRP_A01_every_shown_candidate_lands_in_the_array_of_its_profile_and_seats_are_only_links()
    {
        var grouped = BackofficeGroupedAncillaryOffersMapper.Map(await GroupedAsync(Shop()));

        Assert.All(grouped.Baggages, bag => Assert.Equal("A01", bag.VariantCode));
        Assert.NotEmpty(grouped.Baggages);
        Assert.Equal("ExtraPiece", grouped.Baggages[0].ChargeKind.Name);
        Assert.All(grouped.Connectivity, wifi => Assert.Equal("A24", wifi.VariantCode));
        Assert.NotEmpty(grouped.Connectivity);
        Assert.All(grouped.ExtraSeats, extra => Assert.Equal("A09", extra.VariantCode));
        Assert.Empty(grouped.Pets);
        Assert.Empty(grouped.Meals);
        Assert.Empty(grouped.AssistedTravel);
        Assert.Empty(grouped.AirportServices);
        Assert.Empty(grouped.PriorityServices);
        Assert.Empty(grouped.Upgrades);
        Assert.DoesNotContain("A07", All(grouped).Select(item => item.VariantCode));
        Assert.All(All(grouped), item => Assert.Contains(item.Readiness.Name, new[] { "Selectable", "NeedsSelection", "NeedsQuote" }));
    }

    [Fact]
    public async Task GRP_A02_meta_and_seat_links_describe_the_offer_source_without_running_the_seat_map()
    {
        var snapshot = await GroupedAsync(Shop());
        var grouped = BackofficeGroupedAncillaryOffersMapper.Map(snapshot);
        var flightIds = snapshot.Shopping.Offer.Bounds.SelectMany(bound => bound.Flights.Select(flight => flight.FlightId)).Distinct().ToList();

        Assert.Equal((OfferWire.OfferId, (long?)null, "PreOrder"), (grouped.Meta.OfferId, grouped.Meta.OrderId, grouped.Meta.ShoppingStage.Name));
        Assert.Equal(snapshot.Shopping.CurrencyId, grouped.Meta.CurrencyId);
        Assert.Equal(flightIds, grouped.Seats.Select(seat => seat.FlightId));
        Assert.All(grouped.Seats, seat => Assert.Equal("/Backoffice/v1/SeatMaps", seat.SeatMapEndpoint));
    }

    [Fact]
    public async Task GRP_A03_the_order_source_reports_the_order_id_and_the_order_stage()
    {
        var air = Desk.Sell(await OfferWire.ReadAsync(OfferWire.Captured()));
        var order = OrderOf(Ticketed(air, (_, _) => true), ticketedAtUtc: Desk.EvaluatedAtUtc);
        var grouped = OtaPanelGroupedAncillaryOffersMapper.Map(await GroupedAsync(Shop(), new FixedOrders(order), OrderDesk.OrderId));

        Assert.Equal(((string?)null, (long?)OrderDesk.OrderId, ShoppingStage.PostTicketed.ToString()), (grouped.Meta.OfferId, grouped.Meta.OrderId, grouped.Meta.ShoppingStage));
        Assert.All(grouped.Seats, seat => Assert.Equal("/OtaPanel/v1/SeatMaps", seat.SeatMapEndpoint));
    }

    [Fact]
    public async Task GRP_A04_details_endpoint_follows_the_surface_and_the_family_only_when_details_are_required()
    {
        var snapshot = await GroupedAsync(Shop());
        var backoffice = BackofficeGroupedAncillaryOffersMapper.Map(snapshot);
        var panel = OtaPanelGroupedAncillaryOffersMapper.Map(snapshot);

        Assert.All(All(backoffice), item => Assert.Equal(item.SelectionId is null, item.DetailsEndpoint is not null));
        Assert.All(backoffice.Baggages.Where(bag => bag.SelectionId is null), bag => Assert.Equal("/Backoffice/v1/AncillaryOffers/Baggages/Details", bag.DetailsEndpoint));
        Assert.All(panel.Baggages.Where(bag => bag.SelectionId is null), bag => Assert.Equal("/OtaPanel/v1/AncillaryOffers/Baggages/Details", bag.DetailsEndpoint));
        Assert.All(panel.ExtraSeats, extra => Assert.Equal(((string?)null, "/OtaPanel/v1/AncillaryOffers/ExtraSeats/Details"), (extra.SelectionId, extra.DetailsEndpoint)));
        Assert.Equal(All(backoffice).Select(item => item.OfferItemRef), panel.Baggages.Concat<OtaPanelGroupedOfferDto>(panel.Connectivity).Concat(panel.ExtraSeats).Select(item => item.OfferItemRef));
    }

    [Fact]
    public async Task GRP_A05_the_wire_shape_has_nine_arrays_typed_fields_and_surface_enums()
    {
        var snapshot = await GroupedAsync(Shop());
        var backoffice = JsonSerializer.SerializeToNode(BackofficeGroupedAncillaryOffersMapper.Map(snapshot), Json)!.AsObject();
        var panel = JsonSerializer.SerializeToNode(OtaPanelGroupedAncillaryOffersMapper.Map(snapshot), Json)!.AsObject();

        foreach (var document in new[] { backoffice, panel })
            Assert.Equal(
                new[] { "meta", "seats", "baggages", "meals", "pets", "assistedTravel", "airportServices", "priorityServices", "connectivity", "upgrades", "extraSeats" },
                document.Select(member => member.Key));

        var bag = backoffice["baggages"]![0]!.AsObject();

        Assert.Equal(new[] { "value", "name", "title" }, bag["readiness"]!.AsObject().Select(member => member.Key));
        Assert.Equal(["offerItemRef", "variantCode", "serviceDefinitionRef", "name"], bag.Select(member => member.Key).Take(4));
        Assert.Equal("chargeKind", bag.Select(member => member.Key).ElementAt(14));
        Assert.Contains("chargeKind", bag.Select(member => member.Key));
        Assert.DoesNotContain("mealKind", bag.Select(member => member.Key));
        Assert.Equal(JsonValueKind.String, panel["baggages"]![0]!["readiness"]!.GetValueKind());
        Assert.Equal(JsonValueKind.String, panel["meta"]!["shoppingStage"]!.GetValueKind());
        Assert.Contains("extraOccupiedSeatCount", panel["extraSeats"]![0]!.AsObject().Select(member => member.Key));
    }

    [Fact]
    public void GRP_A06_the_two_grouped_controllers_sit_beside_the_legacy_controllers_with_the_same_route_and_policy()
    {
        foreach (var (type, route, policy) in new[]
                 {
                     (typeof(GroupedBackofficeController), "Backoffice/v{version:apiVersion}/AncillaryOffers", SurfaceAuthorization.Backoffice),
                     (typeof(GroupedOtaPanelController), "OtaPanel/v{version:apiVersion}/AncillaryOffers", SurfaceAuthorization.OtaPanel)
                 })
        {
            Assert.Equal(route, type.GetCustomAttributes(typeof(RouteAttribute), false).Cast<RouteAttribute>().Single().Template);
            Assert.Equal(policy, type.GetCustomAttributes(typeof(AuthorizeAttribute), false).Cast<AuthorizeAttribute>().Single().Policy);
            Assert.Equal(
                ["Grouped", "Baggages/Details", "Meals/Details", "Pets/Details", "AssistedTravel/Details", "AirportServices/Details", "PriorityServices/Details", "Connectivity/Details", "Upgrades/Details", "ExtraSeats/Details"],
                type.GetMethods().SelectMany(method => method.GetCustomAttributes(typeof(HttpPostAttribute), false).Cast<HttpPostAttribute>()).Select(post => post.Template));
        }
    }

    private static IEnumerable<BackofficeGroupedOfferDto> All(BackofficeGroupedAncillaryOffersDto grouped)
        => grouped.Baggages.Cast<BackofficeGroupedOfferDto>()
            .Concat(grouped.Meals)
            .Concat(grouped.Pets)
            .Concat(grouped.AssistedTravel)
            .Concat(grouped.AirportServices)
            .Concat(grouped.PriorityServices)
            .Concat(grouped.Connectivity)
            .Concat(grouped.Upgrades)
            .Concat(grouped.ExtraSeats);
}
