using System.Text.Json;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Ancillary.Query.Shopping.SeatMaps;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.GetSeatMap.Backoffice;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.ResolveSeatMapSelections;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.ResolveSeatMapSelections.Service;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Snapshots;
using AeroTech.Ancillary.Shopping.Tests.Fixtures;
using AeroTech.Framework.Core.Domain.Exceptions;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.BackofficeDesk;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.SeatMapDesk;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.SelectionDesk;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.ShoppingLab;
using SeatMapControllers = AeroTech.Ancillary.RestApi.V1.SeatMaps.Controllers;

namespace AeroTech.Ancillary.Shopping.Tests;

public class S2SeatMapResolveTests
{
    private const string Adult = "ADT-1";
    private const string Child = "CHD-1";

    private sealed record Desk(ShoppingLab Lab, long PaidPricingId, long FreePricingId)
    {
        public async Task<string> IdAsync(string seatNumber, string traveller, int flight = 0)
        {
            var (_, seatMaps) = await SeatResolveAsync(Lab);
            var map = await seatMaps.ExecuteAsync(new BackofficeGetSeatMapQuery { OfferId = OfferWire.OfferId }, Sale);

            return map.Flights[flight].OffersOf(seatNumber).Single(offer => offer.TravellerRef == traveller).Id!;
        }

        public async Task<SeatMapResolutionSnapshot> ResolveAsync(params string[] ids)
        {
            var (service, _) = await SeatResolveAsync(Lab);

            return await service.ExecuteAsync(ids);
        }

        public async Task<BusinessException> RefusedAsync(IReadOnlyList<string>? ids)
        {
            var (service, _) = await SeatResolveAsync(Lab);

            return await Assert.ThrowsAsync<BusinessException>(() => service.ExecuteAsync(ids));
        }
    }

    private static Desk NewDesk()
    {
        var lab = new ShoppingLab();
        var window = lab.Definition("A07");
        var middle = lab.Definition("A07", "SEAT_STANDARD_MIDDLE");
        var paid = lab.Price(window, lab.Provision(window, stage: PurchaseStage.Both, rules: Rules(seats: new ProvisionSeatApplicationArgs([], ["W"]))), Rate(15m, components: [Tax("XT", 1.5m, TaxTreatment.AddedToBase)]));
        var free = lab.Price(middle, lab.Provision(middle, 20, Rules(seats: new ProvisionSeatApplicationArgs([], ["9"])), stage: PurchaseStage.Both), Rate(0m));

        return new Desk(lab, paid.Id, free.Id);
    }

    [Fact]
    public void SEATID_01_the_seat_id_is_traveller_flight_pricing_and_seat_and_reads_back_without_a_secret()
    {
        var id = SeatMapSelectionId.Encode("ADT-1", 1558420526869774453, 9000000000000000777, "3a ");

        Assert.Matches("^[A-Za-z0-9_-]+$", id);
        Assert.Equal(new SeatMapSelection("ADT-1", 1558420526869774453, 9000000000000000777, "3A"), SeatMapSelectionId.Decode(id));
        Assert.Equal("ADT-1|1558420526869774453|9000000000000000777|3A", System.Text.Encoding.UTF8.GetString(System.Buffers.Text.Base64Url.DecodeFromChars(id)));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not base64url!")]
    [InlineData("QURULTF8MXwz")]
    [InlineData("QURULTF8eHwyfDNB")]
    [InlineData("QURULTF8MXwtMnwzQQ")]
    [InlineData("fDF8MnwzQQ")]
    public void SEATID_02_an_id_that_is_not_four_well_typed_parts_decodes_to_nothing(string? id)
    {
        Assert.Null(SeatMapSelectionId.Decode(id));
    }

    [Fact]
    public async Task SEATRES_01_a_seat_id_resolves_to_its_traveller_flight_seat_and_the_definition_provision_and_pricing_it_names()
    {
        var desk = NewDesk();
        var id = await desk.IdAsync("3A", Adult);

        var seat = ServiceSeatMapResolutionMapper.Map(await desk.ResolveAsync(id)).Seats.Single();
        var provision = desk.Lab.Catalog.Provisions.Single(row => row.Id == seat.Provision.Id);

        Assert.Equal((id, Adult, SeatMapSelectionId.Decode(id)!.FlightId, "3A"), (seat.Id, seat.TravellerRef, seat.FlightId, seat.SeatNumber));
        Assert.Equal(("SEAT_STANDARD", "A07", "Seat", (int)AncillaryProfile.Seat), (seat.ServiceDefinition.ServiceDefinitionRef, seat.ServiceDefinition.VariantCode, seat.ServiceDefinition.Profile!.Name, seat.ServiceDefinition.Profile.Value));
        Assert.Equal(provision.ServiceDefinitionId, seat.ServiceDefinition.Id);
        Assert.Equal((desk.PaidPricingId, "PerSeat"), (seat.Pricing.Id, seat.Pricing.PricingUnit!.Name));
        Assert.Equal(("Paid", "Filed", provision.Fulfillment.FulfillmentProviderKey), (seat.Provision.Outcome.Disposition.Name, seat.Provision.PriceOrigin.Name, seat.Provision.Fulfillment.FulfillmentProviderKey));
        Assert.Equal((Eur, 15m, 16.5m, true), (seat.Pricing.Rates.Single().CurrencyId, seat.Pricing.Rates.Single().BaseAmount, seat.Pricing.Rates.Single().UnitTotal, seat.Pricing.Rates.Single().IsUnitTotalComplete));
        Assert.Equal(("Tax", "XT", 1.5m, "AddedToBase"), (seat.Pricing.Rates.Single().Components.Single().Category.Name, seat.Pricing.Rates.Single().Components.Single().Code, seat.Pricing.Rates.Single().Components.Single().Amount, seat.Pricing.Rates.Single().Components.Single().TaxTreatment!.Name));
    }

    [Fact]
    public async Task SEATRES_02_a_zero_rated_seat_resolves_with_its_zero_rate_and_two_seats_resolve_in_request_order()
    {
        var desk = NewDesk();
        var paid = await desk.IdAsync("3A", Adult);
        var free = await desk.IdAsync("3B", Child);

        var seats = ServiceSeatMapResolutionMapper.Map(await desk.ResolveAsync(free, paid)).Seats;

        Assert.Equal(new[] { free, paid }, seats.Select(seat => seat.Id));
        Assert.Equal((desk.FreePricingId, 0m, "SEAT_STANDARD_MIDDLE"), (seats[0].Pricing.Id, seats[0].Pricing.Rates.Single().UnitTotal, seats[0].ServiceDefinition.ServiceDefinitionRef));
    }

    [Fact]
    public async Task SEATRES_03_every_enum_of_the_answer_carries_its_value_name_and_title_and_every_group_is_an_object()
    {
        var desk = NewDesk();
        var json = JsonSerializer.SerializeToElement(ServiceSeatMapResolutionMapper.Map(await desk.ResolveAsync(await desk.IdAsync("3A", Adult))), new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var seat = json.GetProperty("seats")[0];

        Assert.Equal(
            new[] { "id", "travellerRef", "flightId", "seatNumber", "serviceDefinition", "provision", "pricing" },
            seat.EnumerateObject().Select(property => property.Name));
        Assert.All(
            new[]
            {
                seat.GetProperty("serviceDefinition").GetProperty("booking").GetProperty("method"),
                seat.GetProperty("serviceDefinition").GetProperty("document").GetProperty("type"),
                seat.GetProperty("serviceDefinition").GetProperty("seat").GetProperty("purpose"),
                seat.GetProperty("provision").GetProperty("coverageScope"),
                seat.GetProperty("provision").GetProperty("quantity").GetProperty("unit"),
                seat.GetProperty("provision").GetProperty("outcome").GetProperty("disposition"),
                seat.GetProperty("provision").GetProperty("settlement").GetProperty("reissueRefund"),
                seat.GetProperty("pricing").GetProperty("pricingUnit")
            },
            value => Assert.Equal(new[] { "value", "name", "title" }, value.EnumerateObject().Select(property => property.Name)));
    }

    [Fact]
    public async Task SEATRES_04_an_id_whose_pricing_is_no_longer_active_or_is_not_a_seat_is_not_found()
    {
        var desk = NewDesk();
        var id = await desk.IdAsync("3A", Adult);
        var bag = desk.Lab.Definition("A01");
        var bagPricing = desk.Lab.Price(bag, desk.Lab.Provision(bag), Rate(100m));
        var flightId = SeatMapSelectionId.Decode(id)!.FlightId;

        desk.Lab.Catalog.Pricings.RemoveAll(pricing => pricing.Id == desk.PaidPricingId);

        var withdrawn = await desk.RefusedAsync([id]);
        var notSeat = await desk.RefusedAsync([SeatMapSelectionId.Encode(Adult, flightId, bagPricing.Id, "3A")]);

        Assert.Equal((16811, 404), (withdrawn.Code, withdrawn.HttpStatus));
        Assert.Equal((16811, 404), (notSeat.Code, notSeat.HttpStatus));
    }

    [Fact]
    public async Task SEATRES_05_a_malformed_request_is_refused_before_anything_is_read()
    {
        var desk = NewDesk();
        var adultWindow = await desk.IdAsync("3A", Adult);
        var childWindow = await desk.IdAsync("3A", Child);
        var adultMiddle = await desk.IdAsync("3B", Adult);
        var adultSecondFlight = await desk.IdAsync("3A", Adult, flight: 1);
        var refusals = new List<BusinessException>
        {
            await desk.RefusedAsync(null),
            await desk.RefusedAsync([]),
            await desk.RefusedAsync(Enumerable.Range(0, 51).Select(index => SeatMapSelectionId.Encode(Adult, index + 1, 1, "3A")).ToList()),
            await desk.RefusedAsync([adultWindow, adultWindow]),
            await desk.RefusedAsync(["not-an-id!"]),
            await desk.RefusedAsync([adultWindow, childWindow]),
            await desk.RefusedAsync([adultWindow, adultMiddle])
        };

        Assert.All(refusals, refused => Assert.Equal((16801, 422), (refused.Code, refused.HttpStatus)));
        Assert.Equal(2, (await desk.ResolveAsync(adultWindow, adultSecondFlight)).Seats.Count);
    }

    [Fact]
    public void SEATRES_06_the_seat_map_resolve_route_is_one_anonymous_service_operation_taking_only_ids()
    {
        var controller = typeof(SeatMapControllers.ServiceController);

        Assert.Equal("Service/v{version:apiVersion}/Ancillaries/SeatMaps", controller.GetCustomAttributes(typeof(RouteAttribute), false).Cast<RouteAttribute>().Single().Template);
        Assert.Empty(controller.GetCustomAttributes(typeof(AuthorizeAttribute), true));
        Assert.Equal(new[] { "Resolve" }, controller.GetMethods().SelectMany(method => method.GetCustomAttributes(typeof(HttpPostAttribute), false).Cast<HttpPostAttribute>()).Select(post => post.Template));
        Assert.Equal(new[] { "Ids" }, typeof(ServiceResolveSeatMapSelectionsQuery).GetProperties().Select(property => property.Name));
    }
}
