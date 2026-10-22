using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldSeatSelections;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Services;
using AeroTech.Ancillary.Domain.AncillaryProvisionAggregate.Arguments;
using AeroTech.Ancillary.Query.Shopping.SeatMaps;
using AeroTech.Ancillary.Query.Shopping.SeatMaps.Queries.VerifySeatSelections;
using AeroTech.Ancillary.Shopping.Tests.Fixtures;
using AeroTech.Framework.Core.Domain.Exceptions;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.BackofficeDesk;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.SeatMapDesk;
using static AeroTech.Ancillary.Shopping.Tests.Fixtures.ShoppingLab;

namespace AeroTech.Ancillary.Shopping.Tests;

public class SeatSelectionVerifierTests
{
    private const long FlightId = 1558420526869774453;
    private const long OrderId = 7001;
    private const long AdultId = 101;

    private sealed record Desk(ShoppingLab Lab, long DefinitionId, long ProvisionId, long PricingId)
    {
        public SeatSelectionVerifier Verifier()
            => new(Lab.Engine, new FixedSeatMaps(Aircraft()), new HomeAirline(), new TestOnlyAirportReference(), new TestOnlyAirFareReference(), new FixedClock { Now = Now });

        public Task<IReadOnlyList<VerifiedSeatSelection>> VerifyAsync(params SeatHoldSelectionInput[] seats)
            => Verifier().VerifyAsync(new SeatSelectionVerification(OrderId, 1, Pos, Eur, null, seats));

        public Task<IReadOnlyList<VerifiedSeatSelection>> VerifyTicketedAsync(params SeatHoldSelectionInput[] seats)
            => Verifier().VerifyAsync(new SeatSelectionVerification(OrderId, 1, Pos, Eur, Now.AddDays(-1), seats));
    }

    private static Desk NewDesk()
    {
        var lab = new ShoppingLab();
        var seat = lab.Definition("A07");
        var provision = lab.Provision(seat, stage: PurchaseStage.Both, rules: Rules(seats: new ProvisionSeatApplicationArgs([], ["W", "A"])));
        var pricing = lab.Price(seat, provision, Rate(15m));

        return new Desk(lab, seat.Id, provision.Id, pricing.Id);
    }

    private static SeatHoldFlightInput Flight(long? rbdId = 11)
        => new("111", 1, 1, 2, 6, Now.AddDays(10), Now.AddDays(10).AddHours(3), 1, 1, rbdId);

    private static SeatHoldSelectionInput Seat(string id, long orderServiceId = 9001, long travellerId = AdultId, PassengerTypeCode passengerType = PassengerTypeCode.ADT, SeatHoldFlightInput? flight = null)
        => new(orderServiceId, travellerId, id, passengerType, new DateOnly(1990, 5, 20), flight ?? Flight(), null);

    private static async Task<BusinessException> RefusedAsync(Desk desk, params SeatHoldSelectionInput[] seats)
        => await Assert.ThrowsAsync<BusinessException>(() => desk.VerifyAsync(seats));

    [Fact]
    public async Task VER_01_a_sellable_seat_id_verifies_to_its_definition_provision_pricing_and_seat_on_the_named_flight()
    {
        var desk = NewDesk();
        var id = SeatMapSelectionId.Encode("ADT-1", FlightId, desk.PricingId, "3a");

        var verified = Assert.Single(await desk.VerifyAsync(Seat(id)));

        Assert.Equal(new VerifiedSeatSelection(9001, AdultId, id, "3A", FlightId, desk.DefinitionId, desk.ProvisionId, desk.PricingId), verified);
    }

    [Fact]
    public async Task VER_02_two_travellers_on_one_flight_verify_together_and_keep_the_request_order()
    {
        var desk = NewDesk();
        var adult = SeatMapSelectionId.Encode("ADT-1", FlightId, desk.PricingId, "3A");
        var child = SeatMapSelectionId.Encode("CHD-1", FlightId, desk.PricingId, "3C");

        var verified = await desk.VerifyAsync(Seat(child, 9002, 102, PassengerTypeCode.CHD), Seat(adult));

        Assert.Equal(new[] { ("3C", 102L, 9002L), ("3A", AdultId, 9001L) }, verified.Select(seat => (seat.SeatNumber, seat.TravellerId, seat.OrderServiceId)));
    }

    [Fact]
    public async Task VER_03_a_seat_that_no_category_prices_or_whose_pricing_moved_is_not_sellable()
    {
        var desk = NewDesk();
        var uncategorized = SeatMapSelectionId.Encode("ADT-1", FlightId, desk.PricingId, "3B");
        var otherPricing = SeatMapSelectionId.Encode("ADT-1", FlightId, desk.PricingId + 1, "3A");

        var blocked = await RefusedAsync(desk, Seat(uncategorized));
        var moved = await RefusedAsync(desk, Seat(otherPricing));

        Assert.Equal((16415, 422), (blocked.Code, blocked.HttpStatus));
        Assert.Equal((16415, 422), (moved.Code, moved.HttpStatus));
        Assert.Contains("PriceChanged", moved.Message);
    }

    [Fact]
    public async Task VER_04_an_unreadable_id_an_unknown_seat_number_and_a_withdrawn_pricing_are_not_sellable()
    {
        var desk = NewDesk();
        var unknownSeat = SeatMapSelectionId.Encode("ADT-1", FlightId, desk.PricingId, "99Z");
        var valid = SeatMapSelectionId.Encode("ADT-1", FlightId, desk.PricingId, "3A");

        var unreadable = await RefusedAsync(desk, Seat("not-an-id!"));
        var missing = await RefusedAsync(desk, Seat(unknownSeat));

        desk.Lab.Catalog.Pricings.Clear();

        var withdrawn = await RefusedAsync(desk, Seat(valid));

        Assert.All(new[] { unreadable, missing, withdrawn }, refused => Assert.Equal((16415, 422), (refused.Code, refused.HttpStatus)));
    }

    [Fact]
    public async Task VER_06_after_ticketing_a_seat_needs_an_open_coupon_and_a_refusal_names_the_engine_reason()
    {
        var desk = NewDesk();
        var id = SeatMapSelectionId.Encode("ADT-1", FlightId, desk.PricingId, "3A");

        var unknown = await Assert.ThrowsAsync<BusinessException>(() => desk.VerifyTicketedAsync(Seat(id)));
        var closed = await Assert.ThrowsAsync<BusinessException>(() => desk.VerifyTicketedAsync(Seat(id) with { CouponOpenForUse = false }));
        var open = Assert.Single(await desk.VerifyTicketedAsync(Seat(id) with { CouponOpenForUse = true }));

        Assert.Equal((16415, true), (unknown.Code, unknown.Message.Contains("CouponStateNotVerified", StringComparison.Ordinal)));
        Assert.Equal((16415, true), (closed.Code, closed.Message.Contains("CouponNotOpenForUse", StringComparison.Ordinal)));
        Assert.Equal(("3A", desk.PricingId), (open.SeatNumber, open.PricingId));
    }

    [Fact]
    public async Task VER_05_conflicting_facts_for_one_flight_or_one_traveller_are_an_invalid_request()
    {
        var desk = NewDesk();
        var adultWindow = SeatMapSelectionId.Encode("ADT-1", FlightId, desk.PricingId, "3A");
        var adultAisle = SeatMapSelectionId.Encode("ADT-1", FlightId, desk.PricingId, "3C");

        var flights = await RefusedAsync(desk, Seat(adultWindow), Seat(adultAisle, 9002, flight: Flight(rbdId: 12)));
        var travellers = await RefusedAsync(desk, Seat(adultWindow), Seat(adultAisle, 9002, travellerId: 102));

        Assert.Equal((16402, 422), (flights.Code, flights.HttpStatus));
        Assert.Equal((16402, 422), (travellers.Code, travellers.HttpStatus));
    }
}
