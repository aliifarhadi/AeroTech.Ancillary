using System.Text.Json;
using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.CancelAncillaryHold.Service;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices.Service;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.ReleaseAncillaryHold.Service;
using AeroTech.Ancillary.Domain.AncillaryReservationAggregate;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.RestApi.V1.AncillaryReservationAggregate.Requests;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Holds;

[Collection(DatabaseCollection.Name)]
public class V10ReservationAcceptanceTests
{
    private readonly TestDatabase _database;
    private readonly FixedClock _clock = new();

    public V10ReservationAcceptanceTests(TestDatabase database) => _database = database;

    private static long NextOrderId => Interlocked.Increment(ref _lastOrderId);

    private static long _lastOrderId = 900_000;

    private async Task<(long DefinitionId, long ProvisionId, AncillaryScope Scope)> CatalogAsync()
    {
        var airlineId = _database.NextAirlineId();
        var scope = new AncillaryScope(_database, _clock);
        var supplier = await scope.RegisterSupplier.RegisterAsync(M1Commands.LocalSupplier(airlineId));
        var definition = await scope.DefineServiceDefinition.DefineAsync(M1Commands.LoungeDefinition(airlineId, supplier.Id));
        await scope.ActivateServiceDefinition.ActivateAsync(new TestActivateServiceDefinitionCommand(definition.Id));
        var provision = await scope.DefineProvision.DefineAsync(M1Commands.LoungeProvision(definition.Id));
        await scope.ActivateProvision.ActivateAsync(new TestActivateProvisionCommand(provision.Id));
        await new InventoryHarness(_database, _clock).ActivePolicyAsync(
            new InventoryFixture(airlineId),
            new TestDefineInventoryPolicyCommand(airlineId, definition.ServiceDefinitionRef, definition.Id, InventoryAuthority.Unlimited));

        return (definition.Id, provision.Id, scope);
    }

    private static long UnitOf(AncillaryHoldReceipt hold, long orderServiceId) => hold.Units.Single(unit => unit.OrderServiceId == orderServiceId).UnitId;

    [Fact]
    public async Task V10_L02_L08_L10_L12_L14_E08_a_partial_release_confirm_and_partial_cancel_persist_and_read_back()
    {
        var (_, provisionId, scope) = await CatalogAsync();
        await using var _ = scope;
        var orderId = NextOrderId;
        var (bag, meal, lounge) = (orderId * 10 + 1, orderId * 10 + 2, orderId * 10 + 3);
        var hold = await scope.HoldAncillaryServices.HoldAsync(M1Commands.Hold(
            $"V10-{orderId}", orderId, provisionId, _clock.Now.AddMinutes(30), (bag, 71, 301), (meal, 72, 301), (lounge, 73, 301)));

        var released = await scope.ReleaseAncillaryHold.ReleaseAsync(new TestReleaseAncillaryHoldCommand(hold.HoldId, [UnitOf(hold, meal)]));
        var confirmed = await scope.ConfirmAncillaryHold.ConfirmAsync(new TestConfirmAncillaryHoldCommand(hold.HoldId));
        var cancelled = await scope.CancelAncillaryHold.CancelAsync(new TestCancelAncillaryHoldCommand(hold.HoldId, "CUSTOMER", [UnitOf(hold, bag)]));

        await BusinessAssert.ThrowsAsync(16413, 409, () => scope.CancelAncillaryHold.CancelAsync(new TestCancelAncillaryHoldCommand(hold.HoldId, "SCHEDULE", [UnitOf(hold, bag)])));
        await BusinessAssert.ThrowsAsync(16418, 404, () => scope.ReleaseAncillaryHold.ReleaseAsync(new TestReleaseAncillaryHoldCommand(hold.HoldId, [999_999])));

        var read = await scope.GetHoldById.ExecuteAsync(hold.HoldId);

        Assert.Equal(AncillaryReservationStatus.Held, released.Status);
        Assert.Equal(AncillaryReservationStatus.Confirmed, confirmed.Status);
        Assert.Equal(AncillaryReservationStatus.Confirmed, cancelled.Status);
        Assert.Equal(
            new[] { (bag, AncillaryReservationUnitStatus.Cancelled, "CUSTOMER"), (meal, AncillaryReservationUnitStatus.Released, (string?)null), (lounge, AncillaryReservationUnitStatus.Confirmed, null) },
            read.Units().OrderBy(unit => unit.OrderServiceId).Select(unit => (unit.OrderServiceId, unit.Status, unit.CancellationReasonCode)));

        var all = await scope.CancelAncillaryHold.CancelAsync(new TestCancelAncillaryHoldCommand(hold.HoldId, "SCHEDULE"));

        Assert.Equal(AncillaryReservationStatus.Cancelled, all.Status);
        Assert.Equal(
            new[] { "CUSTOMER", null, "SCHEDULE" },
            (await scope.GetHoldById.ExecuteAsync(hold.HoldId)).Units().OrderBy(unit => unit.OrderServiceId).Select(unit => unit.CancellationReasonCode));
    }

    [Fact]
    public async Task V10_I27_allocations_round_trip_through_sql_with_three_decimals()
    {
        var (definitionId, provisionId, scope) = await CatalogAsync();
        await using var _ = scope;
        var orderId = NextOrderId;
        var reservation = AncillaryReservation.Hold(
            _database.Ids.NewId(),
            $"V10-{orderId}",
            orderId,
            $"ORD-{orderId}",
            _clock.Now.AddMinutes(30),
            [
                new(orderId * 10 + 1, definitionId, provisionId, 71, ServiceCoverageScope.Sector, [301], 2, VariantCode: AncillaryVariant.Lounge, ServiceDefinitionRef: "LNG_IKA_CIP",
                    Allocations: [new(InventoryResourceKind.FlightCount, 41, 2m), new(InventoryResourceKind.FlightWeight, 42, 10.125m)])
            ],
            _database.Ids,
            _clock.Now);

        await scope.Reservations.AddAsync(reservation);
        await scope.UnitOfWork.SaveChangesAsync();

        await using var reader = new AncillaryScope(_database, _clock);
        var stored = Assert.Single((await reader.Reservations.GetAsync(reservation.Id))!.Units);

        Assert.Equal(
            new[] { (InventoryResourceKind.FlightCount, 41L, 2m), (InventoryResourceKind.FlightWeight, 42L, 10.125m) },
            stored.Allocations.OrderBy(allocation => allocation.ResourceKind).Select(allocation => (allocation.ResourceKind, allocation.InventorySourceId, allocation.Amount)));
    }

    [Fact]
    public void V10_A01_A02_A16_I13_the_hold_contract_refuses_a_missing_expiry_unknown_members_and_malformed_unit_lists()
    {
        const string valid = """
            {"idempotencyKey":"K","orderId":7001,"reference":"ORD","requestedExpiresAt":"2026-10-01T10:30:00Z",
             "airportServices":[{"variantCode":"A20","products":[{"provisionId":3,"units":[{"orderServiceId":1,"unit":{"travellerId":4},"coverage":{"flightIds":[301]},
                          "selected":{"quantity":1,"options":{"airportService":{"timeWithOffset":"2026-11-01T16:00:00+03:30","guestCount":1}}}}]}]}]}
            """;

        Assert.Equal(1, HostJson.Bind<HoldAncillaryServicesRequest>(valid).AirportServices![0].Products[0].Units[0].Selected!.Options!.AirportService!.GuestCount);
        Assert.Throws<JsonException>(() => HostJson.Bind<HoldAncillaryServicesRequest>(valid.Replace("\"guestCount\":1", "\"guestCount\":1,\"acceptedWeightKgTotal\":16.5")));
        Assert.Throws<JsonException>(() => HostJson.Bind<HoldAncillaryServicesRequest>(valid.Replace("\"quantity\":1", "\"quantity\":1,\"seatNumber\":\"3A\"")));
        Assert.Throws<JsonException>(() => HostJson.Bind<HoldAncillaryServicesRequest>(valid.Replace("\"orderId\":7001", "\"orderId\":7001,\"currencyId\":70")));
        Assert.Throws<JsonException>(() => HostJson.Bind<CancelAncillaryHoldRequest>("""{"reasonCode":"CUSTOMER","unitIds":[1],"amount":1}"""));

        var holds = new ServiceHoldAncillaryServicesCommandValidator();
        var request = HostJson.Bind<HoldAncillaryServicesRequest>(valid);
        var command = new ServiceHoldAncillaryServicesCommand(
            request.IdempotencyKey, request.OrderId, request.Reference, request.RequestedExpiresAt, null, null, null, request.AirportServices, null, null, null);

        Assert.False(holds.Validate(command with { RequestedExpiresAt = default }).IsValid);
        Assert.False(holds.Validate(command with { AirportServices = [] }).IsValid);
        Assert.False(holds.Validate(command with { AirportServices = [request.AirportServices![0] with { Products = [] }] }).IsValid);
        Assert.True(holds.Validate(command).IsValid);

        var releases = new ServiceReleaseAncillaryHoldCommandValidator();
        var cancels = new ServiceCancelAncillaryHoldCommandValidator();

        Assert.True(releases.Validate(new ServiceReleaseAncillaryHoldCommand(1, null)).IsValid);
        Assert.True(releases.Validate(new ServiceReleaseAncillaryHoldCommand(1, [5, 6])).IsValid);
        Assert.False(releases.Validate(new ServiceReleaseAncillaryHoldCommand(1, [])).IsValid);
        Assert.False(releases.Validate(new ServiceReleaseAncillaryHoldCommand(1, [5, 5])).IsValid);
        Assert.False(releases.Validate(new ServiceReleaseAncillaryHoldCommand(1, [0])).IsValid);
        Assert.True(cancels.Validate(new ServiceCancelAncillaryHoldCommand(1, "CUSTOMER", null)).IsValid);
        Assert.False(cancels.Validate(new ServiceCancelAncillaryHoldCommand(1, "CUSTOMER", [])).IsValid);
    }
}
