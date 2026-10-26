using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldSeatSelections;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldSeatSelections.Service;
using AeroTech.Ancillary.Query.Shopping.SeatMaps;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Holds;

[Collection(DatabaseCollection.Name)]
public class SeatReservationAcceptanceTests
{
    private const long FlightId = 301;
    private const long PricingId = 901;

    private readonly TestDatabase _database;
    private readonly FixedClock _clock = new();

    public SeatReservationAcceptanceTests(TestDatabase database) => _database = database;

    private static long NextOrderId => Interlocked.Increment(ref _lastOrderId);

    private static long _lastOrderId = 800_000;

    private async Task<(long OrderId, AncillaryScope Scope)> SeatCatalogAsync()
    {
        var airlineId = _database.NextAirlineId();
        var scope = new AncillaryScope(_database, _clock);
        var supplier = await scope.RegisterSupplier.RegisterAsync(M1Commands.LocalSupplier(airlineId));
        var definition = await scope.DefineServiceDefinition.DefineAsync(M1Commands.LoungeDefinition(airlineId, supplier.Id));
        await scope.ActivateServiceDefinition.ActivateAsync(new TestActivateServiceDefinitionCommand(definition.Id));
        var provision = await scope.DefineProvision.DefineAsync(M1Commands.LoungeProvision(definition.Id));
        await scope.ActivateProvision.ActivateAsync(new TestActivateProvisionCommand(provision.Id));

        scope.SeatSelections.Catalog[PricingId] = (definition.Id, provision.Id);

        return (NextOrderId, scope);
    }

    private static string SeatId(string travellerRef, string seatNumber, long pricingId = PricingId) => SeatMapSelectionId.Encode(travellerRef, FlightId, pricingId, seatNumber);

    private static SeatHoldSelectionInput Seat(long orderServiceId, long travellerId, string id, PassengerTypeCode type = PassengerTypeCode.ADT)
        => new(orderServiceId, travellerId, id, type, null, new SeatHoldFlightInput("111", 1, 1, 2, 6, new DateTimeOffset(2026, 12, 1, 10, 0, 0, TimeSpan.Zero), new DateTimeOffset(2026, 12, 1, 13, 0, 0, TimeSpan.Zero), 1, 1, 11), null);

    private TestHoldSeatSelectionsCommand Command(long orderId, string key, params SeatHoldSelectionInput[] seats)
        => new(key, orderId, 1, $"ORD-{orderId}", _clock.Now.AddMinutes(30), 1, M1Commands.Currency, null, seats);

    [Fact]
    public async Task SEAT_A01_a_seat_hold_stores_every_seat_with_its_selection_id_pricing_and_number_and_replays_only_the_same_seats()
    {
        var (orderId, scope) = await SeatCatalogAsync();
        await using var _ = scope;
        var adult = SeatId("ADT-1", "3A");
        var child = SeatId("CHD-1", "3b");
        var command = Command(orderId, $"SEAT-{orderId}", Seat(orderId * 10 + 1, 101, adult), Seat(orderId * 10 + 2, 102, child, PassengerTypeCode.CHD));

        var hold = await scope.HoldSeatSelections.HoldAsync(command);
        var replay = await scope.HoldSeatSelections.HoldAsync(command);
        var stored = await scope.Reservations.GetAsync(hold.HoldId);

        Assert.Equal((AncillaryReservationStatus.Held, _clock.Now.AddMinutes(30), 2), (hold.Status, hold.ExpiresAt, hold.Units.Count));
        Assert.Equal(hold.HoldId, replay.HoldId);
        Assert.Equal(
            new[] { (adult, "3A", PricingId, 101L, FlightId), (child, "3B", PricingId, 102L, FlightId) },
            stored!.Units.OrderBy(unit => unit.TravellerId).Select(unit => (unit.SeatMapSelectionId!, unit.SeatNumber!, unit.PricingId!.Value, unit.TravellerId!.Value, unit.CoveredFlightIds.Single())));
        Assert.Equal((orderId, 1L, M1Commands.Currency), (scope.SeatSelections.Verified[0].OrderId, scope.SeatSelections.Verified[0].PointOfSaleId, scope.SeatSelections.Verified[0].CurrencyId));
        await BusinessAssert.ThrowsAsync(16403, 409, () => scope.HoldSeatSelections.HoldAsync(command with { Seats = [Seat(orderId * 10 + 1, 101, SeatId("ADT-1", "3C")), command.Seats[1]] }));
    }

    [Fact]
    public async Task SEAT_A02_a_seat_that_is_not_sellable_or_sent_twice_holds_nothing()
    {
        var (orderId, scope) = await SeatCatalogAsync();
        await using var _ = scope;
        var adult = SeatId("ADT-1", "3A");

        await BusinessAssert.ThrowsAsync(16415, 422, () => scope.HoldSeatSelections.HoldAsync(Command(orderId, $"SEAT-{orderId}", Seat(orderId * 10 + 1, 101, adult), Seat(orderId * 10 + 2, 102, SeatId("CHD-1", "3B", 902)))));
        await BusinessAssert.ThrowsAsync(16402, 422, () => scope.HoldSeatSelections.HoldAsync(Command(orderId, $"SEAT-{orderId}-D", Seat(orderId * 10 + 1, 101, adult), Seat(orderId * 10 + 2, 102, adult))));

        var hold = await scope.HoldSeatSelections.HoldAsync(Command(orderId, $"SEAT-{orderId}-OK", Seat(orderId * 10 + 1, 101, adult)));

        Assert.Equal(AncillaryReservationStatus.Held, hold.Status);
    }

    [Fact]
    public async Task SEAT_A03_release_frees_a_held_hold_once_so_the_seat_can_be_held_again()
    {
        var (orderId, scope) = await SeatCatalogAsync();
        await using var _ = scope;
        var hold = await scope.HoldSeatSelections.HoldAsync(Command(orderId, $"SEAT-{orderId}", Seat(orderId * 10 + 1, 101, SeatId("ADT-1", "3A"))));

        var released = await scope.ReleaseAncillaryHold.ReleaseAsync(new TestReleaseAncillaryHoldCommand(hold.HoldId));
        var replay = await scope.ReleaseAncillaryHold.ReleaseAsync(new TestReleaseAncillaryHoldCommand(hold.HoldId));
        var read = await scope.GetHoldById.ExecuteAsync(hold.HoldId);

        Assert.Equal((AncillaryReservationStatus.Released, AncillaryReservationStatus.Released, AncillaryReservationStatus.Released), (released.Status, replay.Status, read.Status));
        Assert.All(read.Units(), unit => Assert.Equal(AncillaryReservationUnitStatus.Released, unit.Status));
        await BusinessAssert.ThrowsAsync(16412, 409, () => scope.ConfirmAncillaryHold.ConfirmAsync(new TestConfirmAncillaryHoldCommand(hold.HoldId)));

        var again = await scope.HoldSeatSelections.HoldAsync(Command(orderId, $"SEAT-{orderId}-2", Seat(orderId * 10 + 1, 101, SeatId("ADT-1", "3A"))));

        Assert.Equal(AncillaryReservationStatus.Held, again.Status);
        await BusinessAssert.ThrowsAsync(16401, 404, () => scope.ReleaseAncillaryHold.ReleaseAsync(new TestReleaseAncillaryHoldCommand(1)));
    }

    [Fact]
    public async Task SEAT_A04_cancel_ends_a_confirmed_hold_with_its_reason_and_refuses_a_hold_that_is_not_confirmed()
    {
        var (orderId, scope) = await SeatCatalogAsync();
        await using var _ = scope;
        var hold = await scope.HoldSeatSelections.HoldAsync(Command(orderId, $"SEAT-{orderId}", Seat(orderId * 10 + 1, 101, SeatId("ADT-1", "3A"))));

        await BusinessAssert.ThrowsAsync(16414, 409, () => scope.CancelAncillaryHold.CancelAsync(new TestCancelAncillaryHoldCommand(hold.HoldId, "CHANGE")));
        await scope.ConfirmAncillaryHold.ConfirmAsync(new TestConfirmAncillaryHoldCommand(hold.HoldId));
        await BusinessAssert.ThrowsAsync(16411, 409, () => scope.ReleaseAncillaryHold.ReleaseAsync(new TestReleaseAncillaryHoldCommand(hold.HoldId)));

        var cancelled = await scope.CancelAncillaryHold.CancelAsync(new TestCancelAncillaryHoldCommand(hold.HoldId, "CHANGE"));
        var replay = await scope.CancelAncillaryHold.CancelAsync(new TestCancelAncillaryHoldCommand(hold.HoldId, "OTHER"));
        var read = await scope.GetHoldById.ExecuteAsync(hold.HoldId);

        Assert.Equal((AncillaryReservationStatus.Cancelled, AncillaryReservationStatus.Cancelled), (cancelled.Status, replay.Status));
        Assert.All(read.Units(), unit => Assert.Equal((AncillaryReservationUnitStatus.Cancelled, "CHANGE"), (unit.Status, unit.CancellationReasonCode)));
        await BusinessAssert.ThrowsAsync(16413, 409, () => scope.ConfirmAncillaryHold.ConfirmAsync(new TestConfirmAncillaryHoldCommand(hold.HoldId)));
    }

    [Fact]
    public async Task SEAT_A05_the_expiry_run_persists_expired_for_past_holds_only_and_an_expired_hold_cannot_be_confirmed_or_released()
    {
        var (orderId, scope) = await SeatCatalogAsync();
        await using var _ = scope;
        var early = await scope.HoldSeatSelections.HoldAsync(Command(orderId, $"SEAT-{orderId}", Seat(orderId * 10 + 1, 101, SeatId("ADT-1", "3A"))));

        _clock.Now = _clock.Now.AddMinutes(20);

        var late = await scope.HoldSeatSelections.HoldAsync(Command(orderId, $"SEAT-{orderId}-2", Seat(orderId * 10 + 2, 102, SeatId("CHD-1", "3B"))));

        _clock.Now = _clock.Now.AddMinutes(11);

        Assert.Contains(orderId, await scope.ExpireAncillaryHolds.ListDueOrderIdsAsync(1000));

        var expired = await scope.ExpireAncillaryHolds.ExpireAsync(orderId);
        var earlyRead = await scope.GetHoldById.ExecuteAsync(early.HoldId);
        var lateRead = await scope.GetHoldById.ExecuteAsync(late.HoldId);

        Assert.Equal(1, expired);
        Assert.Equal((AncillaryReservationStatus.Expired, AncillaryReservationStatus.Held), (earlyRead.Status, lateRead.Status));
        Assert.All(earlyRead.Units(), unit => Assert.Equal(AncillaryReservationUnitStatus.Expired, unit.Status));
        Assert.Equal(AncillaryReservationStatus.Expired, (await scope.GetHoldById.ExecuteAsync(early.HoldId)).Status);
        await BusinessAssert.ThrowsAsync(16407, 409, () => scope.ConfirmAncillaryHold.ConfirmAsync(new TestConfirmAncillaryHoldCommand(early.HoldId)));
        await BusinessAssert.ThrowsAsync(16407, 409, () => scope.ReleaseAncillaryHold.ReleaseAsync(new TestReleaseAncillaryHoldCommand(early.HoldId)));
    }

    [Fact]
    public async Task SEAT_A06_a_new_hold_runs_under_the_order_lock_and_a_busy_order_is_refused_before_anything_is_written()
    {
        var (orderId, scope) = await SeatCatalogAsync();
        await using var _ = scope;

        scope.HoldLock.Busy = true;

        await BusinessAssert.ThrowsAsync(16416, 409, () => scope.HoldSeatSelections.HoldAsync(Command(orderId, $"SEAT-{orderId}", Seat(orderId * 10 + 1, 101, SeatId("ADT-1", "3A")))));

        scope.HoldLock.Busy = false;

        var hold = await scope.HoldSeatSelections.HoldAsync(Command(orderId, $"SEAT-{orderId}", Seat(orderId * 10 + 1, 101, SeatId("ADT-1", "3A"))));
        var replay = await scope.HoldSeatSelections.HoldAsync(Command(orderId, $"SEAT-{orderId}", Seat(orderId * 10 + 1, 101, SeatId("ADT-1", "3A"))));

        Assert.Equal(hold.HoldId, replay.HoldId);
        Assert.Equal(new[] { orderId }, scope.HoldLock.Acquired);
    }

    [Fact]
    public void SEAT_A08_a_ticketed_order_must_state_the_coupon_of_every_seat()
    {
        var validator = new ServiceHoldSeatSelectionsCommandValidator();
        var seat = Seat(1, 101, SeatId("ADT-1", "3A"));
        ServiceHoldSeatSelectionsCommand Command(DateTimeOffset? ticketedAtUtc, params SeatHoldSelectionInput[] seats)
            => new("KEY-1", 7001, 1, "ORD-7001", _clock.Now.AddMinutes(30), 1, M1Commands.Currency, ticketedAtUtc, seats, null);

        Assert.True(validator.Validate(Command(null, seat)).IsValid);
        Assert.False(validator.Validate(Command(_clock.Now, seat)).IsValid);
        Assert.True(validator.Validate(Command(_clock.Now, seat with { CouponOpenForUse = true })).IsValid);
    }

    [Fact]
    public void SEAT_A07_the_hold_request_shapes_carry_no_money_member()
    {
        var names = typeof(SeatHoldSelectionInput).GetProperties().Concat(typeof(SeatHoldFlightInput).GetProperties()).Concat(typeof(SeatHoldFareInput).GetProperties()).Concat(typeof(AncillaryUnitHoldInput<>).GetProperties()).Concat(typeof(SelectedUnitInput).GetProperties())
            .Select(property => property.Name);

        Assert.DoesNotContain(names, name => name.Contains("Amount", StringComparison.Ordinal) || name.Contains("Price", StringComparison.Ordinal) && name != "PricingId" || name.Contains("Fee", StringComparison.Ordinal));
    }
}
