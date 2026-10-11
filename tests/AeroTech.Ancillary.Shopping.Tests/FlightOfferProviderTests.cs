using System.Net;
using System.Text.Json.Nodes;
using AeroTech.Ancillary.Shopping.Tests.Fixtures;
using AeroTech.Framework.Core.Domain.Exceptions;
using AeroTech.Messages.AirPrice.Enums;
using Xunit;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.OfferWire;

namespace AeroTech.Ancillary.Shopping.Tests;

public class FlightOfferProviderTests
{
    private const string Refused = """{"errors":[{"code":6026,"title":"Offer must be priced before ordering.","detail":null}]}""";

    private static async Task<BusinessException> FailsAsync(string json, string offerId = OfferId)
        => await Assert.ThrowsAsync<BusinessException>(() => ReadAsync(json, offerId));

    private static JsonObject Bound(JsonObject data, int index) => data["airTransports"]![index]!.AsObject();

    private static JsonObject Coupon(JsonObject data, int ticket, int coupon) => data["tickets"]![ticket]!["coupons"]![coupon]!.AsObject();

    [Fact]
    public async Task CT_01_the_captured_producer_answer_is_read_without_losing_the_joins()
    {
        var handler = StubHandler.Json(HttpStatusCode.OK, Captured());
        var offer = await Provider(handler).GetByOfferIdAsync(OfferId);

        Assert.Equal((HttpMethod.Post, "https://flight-offers.test/service/v1/FlightOffers/Details"), (handler.LastRequest!.Method, handler.LastRequest.RequestUri!.ToString()));
        Assert.Equal(OfferId, JsonNode.Parse(handler.LastBody!)!["OfferId"]!.GetValue<string>());
        Assert.Null(handler.LastRequest.Headers.Authorization);
        Assert.Equal((OfferId, 70, "IRR", 480000000m, 528010024m), (offer.OfferId, offer.CurrencyId, offer.CurrencyCode, offer.BaseAmount, offer.TotalAmount));
        Assert.Null(offer.LastTicketingDate);
        Assert.Equal(new[] { ("B1", 1, BoundDirection.Outbound, 2, 6), ("B2", 2, BoundDirection.Inbound, 6, 2) }, offer.Bounds.Select(bound => (bound.BoundId, bound.Sequence, bound.Direction!.Value, bound.OriginAirportId, bound.DestinationAirportId)));

        var flight = Assert.Single(offer.Bounds[0].Flights);

        Assert.Equal((1558420526869774453L, 1, "1124", 1, 1, 1, 1, 25L, "Y"), (flight.FlightId, flight.FlightVersion, flight.FlightNumber, flight.OperatingAirlineId, flight.MarketingAirlineId, flight.AircraftId!.Value, flight.CabinClassId!.Value, flight.RbdId!.Value, flight.BookingClass));
        Assert.Equal(TimeSpan.FromMinutes(210), flight.DepartureAt.Offset);
        Assert.Single(flight.Legs);
        Assert.Equal(new[] { ("ADT-1", 1, PassengerTypeCode.ADT), ("CHD-1", 2, PassengerTypeCode.CHD) }, offer.Tickets.Select(ticket => (ticket.TravellerRef, ticket.TravellerIndex, ticket.PassengerTypeCode)));
        Assert.All(offer.Tickets, ticket => Assert.Equal(new[] { "B1", "B2" }, ticket.Coupons.Select(coupon => coupon.BoundId)));

        var coupon = offer.Tickets[0].Coupons[0];

        Assert.Equal((1, 25m, WeightUnit.Kg, 1, 20m, WeightUnit.Kg), (coupon.CheckedBaggage!.Pieces, coupon.CheckedBaggage.Weight, coupon.CheckedBaggage.Unit, coupon.CabinBaggage!.Pieces, coupon.CabinBaggage.Weight, coupon.CabinBaggage.Unit));
        Assert.Equal(new[] { 1469435125056929792L }, coupon.AirFareIds);

        var fare = offer.FareComponents.Single(component => component.AirFareId == 1469435125056929792L);

        Assert.Equal(("B1", "YIKAIST/OW", "Flex", AirFareType.Public, 1, 25L), (fare.BoundId, fare.FareBasis, fare.FareFamily, fare.FareType!.Value, fare.CabinClassId!.Value, fare.RbdId!.Value));
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, Refused, 16804, 422)]
    [InlineData(HttpStatusCode.NotFound, "", 16804, 422)]
    [InlineData(HttpStatusCode.Unauthorized, "", 16806, 502)]
    [InlineData(HttpStatusCode.InternalServerError, "<html>error</html>", 16806, 502)]
    [InlineData(HttpStatusCode.ServiceUnavailable, "", 16806, 502)]
    [InlineData(HttpStatusCode.OK, """{"data":null,"errors":null}""", 16806, 502)]
    [InlineData(HttpStatusCode.OK, "not json", 16806, 502)]
    public async Task CT_03_CT_18_a_refused_or_failed_provider_answer_is_a_typed_error_never_an_empty_success(HttpStatusCode status, string body, int code, int httpStatus)
    {
        var failure = await Assert.ThrowsAsync<BusinessException>(() => Provider(StubHandler.Json(status, body)).GetByOfferIdAsync(OfferId));

        Assert.Equal((code, httpStatus), (failure.Code, failure.HttpStatus));
        Assert.DoesNotContain(OfferId, failure.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CT_03_the_refusal_of_an_unpriced_offer_id_carries_the_producer_reason()
    {
        var failure = await Assert.ThrowsAsync<BusinessException>(() => Provider(StubHandler.Json(HttpStatusCode.BadRequest, Refused)).GetByOfferIdAsync("1|0|0;1~2"));

        Assert.Contains("Offer must be priced before ordering.", failure.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CT_18_a_timeout_an_unreachable_provider_and_a_missing_address_fail_closed()
    {
        var timeout = await Assert.ThrowsAsync<BusinessException>(() => Provider(new StubHandler(_ => throw new TaskCanceledException())).GetByOfferIdAsync(OfferId));
        var unreachable = await Assert.ThrowsAsync<BusinessException>(() => Provider(new StubHandler(_ => throw new HttpRequestException())).GetByOfferIdAsync(OfferId));
        var silent = StubHandler.Json(HttpStatusCode.OK, Captured());
        var unconfigured = await Assert.ThrowsAsync<BusinessException>(() => Provider(silent, configured: false).GetByOfferIdAsync(OfferId));

        Assert.All(new[] { timeout, unreachable, unconfigured }, failure => Assert.Equal((16806, 502), (failure.Code, failure.HttpStatus)));
        Assert.Equal(0, silent.Calls);
    }

    [Fact]
    public async Task CT_18_an_answer_without_a_journey_is_an_unknown_offer_not_an_empty_list()
    {
        var empty = Changed(data =>
        {
            data["airTransports"] = new JsonArray();
            data["tickets"] = new JsonArray();
        });
        var failure = await FailsAsync(empty);

        Assert.Equal((16805, 404), (failure.Code, failure.HttpStatus));
    }

    [Fact]
    public async Task CT_07_an_answer_with_broken_joins_is_refused_as_inconsistent()
    {
        var failures = new[]
        {
            await FailsAsync(Captured(), OfferId + "x"),
            await FailsAsync(Changed(data => Bound(data, 0)["boundOfferId"] = "")),
            await FailsAsync(Changed(data => Bound(data, 1)["boundOfferId"] = Bound(data, 0)["boundOfferId"]!.GetValue<string>())),
            await FailsAsync(Changed(data => Bound(data, 1)["boundId"] = "B1")),
            await FailsAsync(Changed(data => data["pricingUnits"]![0]!["coveredBoundOfferIds"] = new JsonArray("404"))),
            await FailsAsync(Changed(data => data["pricingUnits"]![0]!["fareComponents"]![0]!["boundId"] = "B9")),
            await FailsAsync(Changed(data => Coupon(data, 0, 0)["flightId"] = "777")),
            await FailsAsync(Changed(data => Coupon(data, 0, 1)["boundId"] = "B1")),
            await FailsAsync(Changed(data => data["tickets"]![1]!["coupons"]!.AsArray().RemoveAt(1))),
            await FailsAsync(Changed(data => data["tickets"]![1]!["travellerRef"] = "ADT-1")),
            await FailsAsync(Changed(data => data["tickets"]![1]!["passengerTypeCode"] = "XYZ")),
            await FailsAsync(Changed(data => data["tickets"]![1]!["passengerTypeCode"] = "2")),
            await FailsAsync(Changed(data => Bound(data, 0)["flights"]![0]!["arrivalDateTime"] = "2026-10-15T08:15:00+03:30")),
            await FailsAsync(Changed(data => data["currencyId"] = 0))
        };

        Assert.All(failures, failure => Assert.Equal((16807, 502), (failure.Code, failure.HttpStatus)));
    }

    [Fact]
    public async Task CT_08_CT_10_fare_references_and_baggage_units_are_kept_only_when_the_producer_states_them()
    {
        var offer = await ReadAsync(Changed(data =>
        {
            Coupon(data, 0, 0)["baggagePieces"] = 0;
            Coupon(data, 0, 0)["baggageWeight"] = 0;
            Coupon(data, 0, 1)["baggageUnit"] = "";
            Coupon(data, 0, 1)["cabinBaggageUnit"] = "0";
            Coupon(data, 1, 0)["baggageUnit"] = "Lbs";
            Coupon(data, 1, 0)["pricings"]![0]!["reference"] = "not-a-fare";
            Coupon(data, 1, 1)["pricings"]!.AsArray().Add(new JsonObject { ["category"] = 0, ["reference"] = "42" });
            data["pricingUnits"]![0]!["fareComponents"]![0]!["fareType"] = "Mystery";
        }));

        Assert.Equal((0, 0m, WeightUnit.Kg), (offer.Tickets[0].Coupons[0].CheckedBaggage!.Pieces, offer.Tickets[0].Coupons[0].CheckedBaggage!.Weight, offer.Tickets[0].Coupons[0].CheckedBaggage!.Unit));
        Assert.Null(offer.Tickets[0].Coupons[1].CheckedBaggage);
        Assert.Null(offer.Tickets[0].Coupons[1].CabinBaggage);
        Assert.Equal(WeightUnit.Lbs, offer.Tickets[1].Coupons[0].CheckedBaggage!.Unit);
        Assert.Empty(offer.Tickets[1].Coupons[0].AirFareIds);
        Assert.Equal(2, offer.Tickets[1].Coupons[1].AirFareIds.Count);
        Assert.Null(offer.FareComponents[0].FareType);
    }

    [Fact]
    public async Task CT_17_a_technical_leg_stays_itinerary_detail_of_one_sold_flight()
    {
        var offer = await ReadAsync(Changed(data =>
        {
            var flight = Bound(data, 0)["flights"]![0]!.AsObject();
            var first = flight["legs"]![0]!.DeepClone().AsObject();
            var second = flight["legs"]![0]!.DeepClone().AsObject();

            first["destinationAirportId"] = 3;
            first["arrivalDateTime"] = "2026-10-15T09:00:00+03:30";
            second["sequence"] = 2;
            second["legId"] = "1558420526869774999";
            second["originAirportId"] = 3;
            second["departureDateTime"] = "2026-10-15T09:40:00+03:30";
            flight["legs"] = new JsonArray(first, second);
        }));
        var sold = Assert.Single(offer.Bounds[0].Flights);

        Assert.Equal(new[] { (2, 3), (3, 6) }, sold.Legs.Select(leg => (leg.OriginAirportId, leg.DestinationAirportId)));
        Assert.All(offer.Tickets, ticket => Assert.Equal(2, ticket.Coupons.Count));
    }
}
