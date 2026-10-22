using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldGroupedSelections;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldSeatSelections;
using AeroTech.Ancillary.Query.Shopping.SeatMaps;
using AeroTech.Messages.AirPrice.Enums;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Holds;

[Collection(DatabaseCollection.Name)]
public class GroupedReservationAcceptanceTests
{
    private const long FlightId = 301;
    private const long PricingId = 901;
    private const string BagId = "grouped-bag-selection";
    private const string OtherBagId = "grouped-bag-selection-other";

    private static long _lastOrderId = 900_000;

    private readonly TestDatabase _database;
    private readonly FixedClock _clock = new();

    public GroupedReservationAcceptanceTests(TestDatabase database) => _database = database;

    private static long NextOrderId => Interlocked.Increment(ref _lastOrderId);

    private async Task<(long OrderId, AncillaryScope Scope)> CatalogAsync()
    {
        var airlineId = _database.NextAirlineId();
        var scope = new AncillaryScope(_database, _clock);
        var supplier = await scope.RegisterSupplier.RegisterAsync(M1Commands.LocalSupplier(airlineId));
        var seat = await scope.DefineServiceDefinition.DefineAsync(M1Commands.LoungeDefinition(airlineId, supplier.Id, "GRP_SEAT"));
        var bag = await scope.DefineServiceDefinition.DefineAsync(M1Commands.LoungeDefinition(airlineId, supplier.Id, "GRP_BAG"));
        await scope.ActivateServiceDefinition.ActivateAsync(new TestActivateServiceDefinitionCommand(seat.Id));
        await scope.ActivateServiceDefinition.ActivateAsync(new TestActivateServiceDefinitionCommand(bag.Id));
        var seatProvision = await scope.DefineProvision.DefineAsync(M1Commands.LoungeProvision(seat.Id));
        var bagProvision = await scope.DefineProvision.DefineAsync(M1Commands.LoungeProvision(bag.Id, maxQuantity: 3));
        await scope.ActivateProvision.ActivateAsync(new TestActivateProvisionCommand(seatProvision.Id));
        await scope.ActivateProvision.ActivateAsync(new TestActivateProvisionCommand(bagProvision.Id));

        scope.SeatSelections.Catalog[PricingId] = (seat.Id, seatProvision.Id);
        scope.GroupedSelections.Accepted[BagId] = (bag.Id, bagProvision.Id, 101, FlightId, 2);
        scope.GroupedSelections.Accepted[OtherBagId] = (bag.Id, bagProvision.Id, 101, FlightId, 3);

        return (NextOrderId, scope);
    }

    private static SeatHoldSelectionInput Seat(long orderServiceId)
        => new(orderServiceId, 101, SeatMapSelectionId.Encode("ADT-1", FlightId, PricingId, "3A"), PassengerTypeCode.ADT, null, new SeatHoldFlightInput("111", 1, 1, 2, 6, new DateTimeOffset(2026, 12, 1, 10, 0, 0, TimeSpan.Zero), new DateTimeOffset(2026, 12, 1, 13, 0, 0, TimeSpan.Zero), 1, 1, 11), null);

    private TestHoldGroupedSelectionsCommand Command(long orderId, string key, IReadOnlyList<SeatHoldSelectionInput> seats, IReadOnlyList<GroupedHoldSelectionInput> baggages, IReadOnlyList<GroupedHoldSelectionInput>? extraSeats = null)
        => new(key, orderId, 1, $"ORD-{orderId}", _clock.Now.AddMinutes(30), 1, M1Commands.Currency, null, seats, baggages, [], [], [], [], [], [], [], extraSeats ?? []);

    private async Task<int> RowsAsync(AncillaryScope scope, long orderId)
        => await scope.Command.AncillaryReservations.AsNoTracking().CountAsync(reservation => reservation.OrderId == orderId);

    [Fact]
    public async Task GRP_A01_a_mixed_hold_saves_one_reservation_with_a_seat_unit_and_a_bound_non_seat_unit_and_replays_only_the_same_batch()
    {
        var (orderId, scope) = await CatalogAsync();
        await using var _ = scope;
        var command = Command(orderId, $"GRP-{orderId}", [Seat(orderId * 10 + 1)], [new GroupedHoldSelectionInput(orderId * 10 + 2, BagId)]);

        var hold = await scope.HoldGroupedSelections.HoldAsync(command);
        var replay = await scope.HoldGroupedSelections.HoldAsync(command);
        var stored = await scope.Reservations.GetAsync(hold.HoldId);

        Assert.Equal((AncillaryReservationStatus.Held, 1, 1, 1), (hold.Status, hold.Seats.Count, hold.Baggages.Count, await RowsAsync(scope, orderId)));
        Assert.Equal((orderId * 10 + 1, "3A", AncillaryReservationUnitStatus.Held), (hold.Seats[0].OrderServiceId, hold.Seats[0].SeatNumber, hold.Seats[0].Status));
        Assert.Equal((orderId * 10 + 2, BagId, AncillaryReservationUnitStatus.Held), (hold.Baggages[0].OrderServiceId, hold.Baggages[0].SelectionId, hold.Baggages[0].Status));
        Assert.Equal(hold.HoldId, replay.HoldId);
        Assert.Equal(
            new[] { ((string?)null, (string?)"3A", 1), (BagId, null, 2) },
            stored!.Units.OrderBy(unit => unit.OrderServiceId).Select(unit => (unit.SelectionId, unit.SeatNumber, unit.Quantity)));
        Assert.Equal((orderId, 1, M1Commands.Currency), (scope.GroupedSelections.Verified[0].OrderId, scope.GroupedSelections.Verified[0].OrderCommercialVersion, scope.GroupedSelections.Verified[0].CurrencyId));
        await BusinessAssert.ThrowsAsync(16403, 409, () => scope.HoldGroupedSelections.HoldAsync(command with { Baggages = [new GroupedHoldSelectionInput(orderId * 10 + 2, OtherBagId)] }));
    }

    [Fact]
    public async Task GRP_A02_an_unverified_or_unsupported_entry_leaves_no_row_and_a_second_hold_of_the_same_order_service_is_refused()
    {
        var (orderId, scope) = await CatalogAsync();
        await using var _ = scope;

        await BusinessAssert.ThrowsAsync(16402, 422, () => scope.HoldGroupedSelections.HoldAsync(Command(orderId, $"GRP-{orderId}-X", [Seat(orderId * 10 + 1)], [], [new GroupedHoldSelectionInput(orderId * 10 + 3, BagId)])));
        await BusinessAssert.ThrowsAsync(16402, 422, () => scope.HoldGroupedSelections.HoldAsync(Command(orderId, $"GRP-{orderId}-U", [Seat(orderId * 10 + 1)], [new GroupedHoldSelectionInput(orderId * 10 + 2, "unknown-selection")])));
        await BusinessAssert.ThrowsAsync(16402, 422, () => scope.HoldGroupedSelections.HoldAsync(Command(orderId, $"GRP-{orderId}-D", [], [new GroupedHoldSelectionInput(orderId * 10 + 2, BagId), new GroupedHoldSelectionInput(orderId * 10 + 2, OtherBagId)])));
        Assert.Equal(0, await RowsAsync(scope, orderId));

        var hold = await scope.HoldGroupedSelections.HoldAsync(Command(orderId, $"GRP-{orderId}", [], [new GroupedHoldSelectionInput(orderId * 10 + 2, BagId)]));

        await BusinessAssert.ThrowsAsync(16406, 409, () => scope.HoldGroupedSelections.HoldAsync(Command(orderId, $"GRP-{orderId}-2", [], [new GroupedHoldSelectionInput(orderId * 10 + 2, OtherBagId)])));
        Assert.Equal((AncillaryReservationStatus.Held, 0, 1, 1), (hold.Status, hold.Seats.Count, hold.Baggages.Count, await RowsAsync(scope, orderId)));
    }

    [Fact]
    public async Task GRP_A03_the_existing_lifecycle_moves_the_whole_grouped_hold_and_a_released_service_can_be_held_again()
    {
        var (orderId, scope) = await CatalogAsync();
        await using var _ = scope;
        var hold = await scope.HoldGroupedSelections.HoldAsync(Command(orderId, $"GRP-{orderId}", [Seat(orderId * 10 + 1)], [new GroupedHoldSelectionInput(orderId * 10 + 2, BagId)]));

        await BusinessAssert.ThrowsAsync(16414, 409, () => scope.CancelAncillaryHold.CancelAsync(new TestCancelAncillaryHoldCommand(hold.HoldId, "CHANGE")));
        var released = await scope.ReleaseAncillaryHold.ReleaseAsync(new TestReleaseAncillaryHoldCommand(hold.HoldId));
        var read = await scope.GetHoldById.ExecuteAsync(hold.HoldId);

        Assert.Equal(AncillaryReservationStatus.Released, released.Status);
        Assert.All(read.Units, unit => Assert.Equal(AncillaryReservationUnitStatus.Released, unit.Status));

        var again = await scope.HoldGroupedSelections.HoldAsync(Command(orderId, $"GRP-{orderId}-2", [Seat(orderId * 10 + 1)], [new GroupedHoldSelectionInput(orderId * 10 + 2, BagId)]));
        var confirmed = await scope.ConfirmAncillaryHold.ConfirmAsync(new TestConfirmAncillaryHoldCommand(again.HoldId));
        var cancelled = await scope.CancelAncillaryHold.CancelAsync(new TestCancelAncillaryHoldCommand(again.HoldId, "CHANGE"));
        var final = await scope.GetHoldById.ExecuteAsync(again.HoldId);

        Assert.Equal((AncillaryReservationStatus.Confirmed, AncillaryReservationStatus.Cancelled, 2), (confirmed.Status, cancelled.Status, final.Units.Count));
        Assert.All(final.Units, unit => Assert.Equal((AncillaryReservationUnitStatus.Cancelled, "CHANGE"), (unit.Status, unit.CancellationReasonCode)));
    }

    [Fact]
    public async Task GRP_A04_a_grouped_hold_runs_under_the_order_lock_so_a_busy_order_is_refused_without_a_row_and_a_replay_takes_no_lock()
    {
        var (orderId, scope) = await CatalogAsync();
        await using var _ = scope;
        var command = Command(orderId, $"GRP-{orderId}", [Seat(orderId * 10 + 1)], [new GroupedHoldSelectionInput(orderId * 10 + 2, BagId)]);

        scope.HoldLock.Busy = true;
        await BusinessAssert.ThrowsAsync(16416, 409, () => scope.HoldGroupedSelections.HoldAsync(command));
        Assert.Equal(0, await RowsAsync(scope, orderId));

        scope.HoldLock.Busy = false;
        var hold = await scope.HoldGroupedSelections.HoldAsync(command);
        scope.HoldLock.Busy = true;
        var replay = await scope.HoldGroupedSelections.HoldAsync(command);

        Assert.Equal((hold.HoldId, 1), (replay.HoldId, await RowsAsync(scope, orderId)));
        Assert.Equal([orderId], scope.HoldLock.Acquired);
    }
}
