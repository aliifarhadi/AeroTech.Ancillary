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
        var seat = await PublishedAsync(scope, "A07", airlineId, supplier.Id);
        var bag = await PublishedAsync(scope, "A01", airlineId, supplier.Id);

        scope.SeatSelections.Catalog[PricingId] = seat;
        scope.GroupedSelections.Accepted[BagId] = (bag.DefinitionId, bag.ProvisionId, 101, ServiceCoverageScope.Portion, FlightId, 2);
        scope.GroupedSelections.Accepted[OtherBagId] = (bag.DefinitionId, bag.ProvisionId, 101, ServiceCoverageScope.Portion, FlightId, 3);

        return (NextOrderId, scope);
    }

    private static async Task<(long DefinitionId, long ProvisionId)> PublishedAsync(AncillaryScope scope, string variantCode, int airlineId, long supplierId)
    {
        var variant = V122Catalog.Case(variantCode);
        var definitionId = (await scope.DefineServiceDefinition.DefineAsync(V122Catalog.Define(variant, airlineId, supplierId))).Id;

        await scope.ActivateServiceDefinition.ActivateAsync(new TestActivateServiceDefinitionCommand(definitionId));

        var provisionId = (await scope.DefineProvision.DefineAsync(V122Catalog.Rule(variant, definitionId))).Id;
        var pricingId = (await scope.DefinePricing.DefineAsync(V12Commands.Pricing(provisionId, P1Commands.Eur, V12Commands.Base(12m)))).Id;

        await scope.PublishProvision.PublishAsync(new TestPublishProvisionCommand(provisionId, pricingId));

        return (definitionId, provisionId);
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

    [Fact]
    public async Task GRP_A05_a_replay_is_answered_from_the_stored_hold_before_any_verification_and_a_changed_batch_under_the_same_key_conflicts()
    {
        var (orderId, scope) = await CatalogAsync();
        await using var _ = scope;
        var command = Command(orderId, $"GRP-{orderId}", [Seat(orderId * 10 + 1)], [new GroupedHoldSelectionInput(orderId * 10 + 2, BagId)]);
        var hold = await scope.HoldGroupedSelections.HoldAsync(command);
        var verifications = scope.GroupedSelections.Verified.Count;

        scope.GroupedSelections.Accepted.Clear();
        scope.SeatSelections.Catalog.Clear();
        var replay = await scope.HoldGroupedSelections.HoldAsync(command);

        Assert.Equal((hold.HoldId, verifications, 1), (replay.HoldId, scope.GroupedSelections.Verified.Count, await RowsAsync(scope, orderId)));
        await BusinessAssert.ThrowsAsync(16403, 409, () => scope.HoldGroupedSelections.HoldAsync(command with { Baggages = [new GroupedHoldSelectionInput(orderId * 10 + 2, OtherBagId)] }));
        await BusinessAssert.ThrowsAsync(16403, 409, () => scope.HoldGroupedSelections.HoldAsync(command with { Seats = [] }));
        await BusinessAssert.ThrowsAsync(16402, 422, () => scope.HoldGroupedSelections.HoldAsync(Command(orderId, $"GRP-{orderId}-N", [], [new GroupedHoldSelectionInput(orderId * 10 + 3, OtherBagId)])));
        Assert.Equal(1, await RowsAsync(scope, orderId));
    }

    [Fact]
    public async Task GRP_A06_a_replay_that_moves_a_stored_baggage_unit_into_another_family_is_a_key_conflict_and_the_row_stays_as_saved()
    {
        var (orderId, scope) = await CatalogAsync();
        await using var _ = scope;
        var bag = new GroupedHoldSelectionInput(orderId * 10 + 2, BagId);
        var command = Command(orderId, $"GRP-{orderId}", [Seat(orderId * 10 + 1)], [bag]);
        var hold = await scope.HoldGroupedSelections.HoldAsync(command);

        scope.GroupedSelections.Accepted.Clear();
        scope.SeatSelections.Catalog.Clear();

        await BusinessAssert.ThrowsAsync(16403, 409, () => scope.HoldGroupedSelections.HoldAsync(command with { Baggages = [], Meals = [bag] }));
        await BusinessAssert.ThrowsAsync(16403, 409, () => scope.HoldGroupedSelections.HoldAsync(command with { Baggages = [], PriorityServices = [bag] }));
        var replay = await scope.HoldGroupedSelections.HoldAsync(command);
        var stored = await scope.Reservations.GetAsync(hold.HoldId);

        Assert.Equal((hold.HoldId, 1, 1, 0), (replay.HoldId, replay.Baggages.Count, replay.Seats.Count, replay.Meals.Count));
        Assert.Equal((1, BagId), (await RowsAsync(scope, orderId), stored!.Units.Single(unit => unit.SelectionId is not null).SelectionId));
    }

    [Fact]
    public async Task GRP_A07_a_hold_stored_by_the_legacy_endpoint_is_never_replayed_as_a_grouped_hold()
    {
        var (orderId, scope) = await CatalogAsync();
        await using var _ = scope;
        var key = $"GRP-{orderId}-LEGACY";
        var (definitionId, provisionId) = scope.SeatSelections.Catalog[PricingId];
        var legacy = await scope.HoldAncillaryServices.HoldAsync(M1Commands.Hold(key, orderId, definitionId, provisionId, _clock.Now.AddMinutes(30), (orderId * 10 + 2, 101, FlightId)));

        await BusinessAssert.ThrowsAsync(16403, 409, () => scope.HoldGroupedSelections.HoldAsync(
            new TestHoldGroupedSelectionsCommand(key, orderId, 1, $"ORD-{orderId}", _clock.Now.AddMinutes(30), 1, M1Commands.Currency, null, [], [new GroupedHoldSelectionInput(orderId * 10 + 2, BagId)], [], [], [], [], [], [], [], [])));
        var stored = await scope.Reservations.GetAsync(legacy.HoldId);

        Assert.Equal((1, (string?)null), (await RowsAsync(scope, orderId), stored!.Units.Single().SelectionId));
    }
}
