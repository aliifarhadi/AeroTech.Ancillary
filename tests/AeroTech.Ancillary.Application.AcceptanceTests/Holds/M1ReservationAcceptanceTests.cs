using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.AncillaryReservationAggregate.Commands.HoldAncillaryServices;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Holds;

[Collection(DatabaseCollection.Name)]
public class M1ReservationAcceptanceTests
{
    private readonly TestDatabase _database;
    private readonly FixedClock _clock = new();

    public M1ReservationAcceptanceTests(TestDatabase database) => _database = database;

    private async Task<(long DefinitionId, long ProvisionId, AncillaryScope Scope)> WalkingCatalogAsync()
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

    private static long NextOrderId => Interlocked.Increment(ref _lastOrderId);

    private static long _lastOrderId = 700_000;

    [Fact]
    public async Task M1_N01_N06_U08_a_hold_accepts_id_based_fulfillment_facts_only()
    {
        var (definitionId, provisionId, scope) = await WalkingCatalogAsync();
        await using var _ = scope;
        var orderId = NextOrderId;

        var hold = await scope.HoldAncillaryServices.HoldAsync(M1Commands.Hold(
            $"IDEMP-{orderId}", orderId, provisionId, _clock.Now.AddMinutes(30),
            (orderId * 10 + 1, 71, 301), (orderId * 10 + 2, 72, 301)));

        Assert.Equal(AncillaryReservationStatus.Held, hold.Status);
        Assert.Equal(_clock.Now.AddMinutes(30), hold.ExpiresAt);
        Assert.Equal(2, hold.Units.Count);
        Assert.All(hold.Units, unit => Assert.Equal(AncillaryReservationUnitStatus.Held, unit.Status));

        var read = await scope.GetHoldById.ExecuteAsync(hold.HoldId);
        var stored = await scope.Reservations.GetAsync(hold.HoldId);

        Assert.Equal(orderId, read.OrderId);
        Assert.Equal(hold.Units.Select(unit => unit.UnitId), read.Units().Select(unit => unit.UnitId));
        Assert.Equal(provisionId, Assert.Single(Assert.Single(read.AirportServices!).Products).ProvisionId);
        Assert.All(stored!.Units, unit =>
        {
            Assert.Equal(definitionId, unit.ServiceDefinitionId);
            Assert.Equal(provisionId, unit.ProvisionId);
        });
    }

    [Fact]
    public async Task M1_N05_a_hold_for_an_unknown_provision_is_refused()
    {
        var (_, _, scope) = await WalkingCatalogAsync();
        await using var _ = scope;
        var orderId = NextOrderId;

        await BusinessAssert.ThrowsAsync(16419, 409, () => scope.HoldAncillaryServices.HoldAsync(M1Commands.Hold(
            $"IDEMP-{orderId}", orderId, 999_999_999, _clock.Now.AddMinutes(30), (orderId * 10 + 1, 71, 301))));
    }

    [Fact]
    public async Task M1_N09_the_same_idempotent_request_replays_and_a_different_body_conflicts()
    {
        var (_, provisionId, scope) = await WalkingCatalogAsync();
        await using var _ = scope;
        var orderId = NextOrderId;
        var command = M1Commands.Hold(
            $"IDEMP-{orderId}", orderId, provisionId, _clock.Now.AddMinutes(30), (orderId * 10 + 1, 71, 301));

        var first = await scope.HoldAncillaryServices.HoldAsync(command);
        var replay = await scope.HoldAncillaryServices.HoldAsync(command);

        Assert.Equal(first.HoldId, replay.HoldId);
        Assert.Equal(first.Units.Select(unit => unit.UnitId), replay.Units.Select(unit => unit.UnitId));

        await BusinessAssert.ThrowsAsync(16403, 409, () => scope.HoldAncillaryServices.HoldAsync(command with
        {
            Reference = "ORD-OTHER"
        }));
    }

    [Fact]
    public async Task M1_N02_an_order_service_with_a_live_unit_cannot_be_held_again()
    {
        var (_, provisionId, scope) = await WalkingCatalogAsync();
        await using var _ = scope;
        var orderId = NextOrderId;
        var orderServiceId = orderId * 10 + 1;

        await scope.HoldAncillaryServices.HoldAsync(M1Commands.Hold(
            $"IDEMP-{orderId}", orderId, provisionId, _clock.Now.AddMinutes(30), (orderServiceId, 71, 301)));

        await BusinessAssert.ThrowsAsync(16406, 409, () => scope.HoldAncillaryServices.HoldAsync(M1Commands.Hold(
            $"IDEMP-{orderId}-X", orderId, provisionId, _clock.Now.AddMinutes(30), (orderServiceId, 71, 301))));
    }

    [Fact]
    public async Task M1_N02_an_expired_hold_frees_its_order_services()
    {
        var (_, provisionId, scope) = await WalkingCatalogAsync();
        await using var _ = scope;
        var orderId = NextOrderId;
        var orderServiceId = orderId * 10 + 1;

        await scope.HoldAncillaryServices.HoldAsync(M1Commands.Hold(
            $"IDEMP-{orderId}", orderId, provisionId, _clock.Now.AddMinutes(30), (orderServiceId, 71, 301)));

        _clock.Now = _clock.Now.AddMinutes(31);

        var second = await scope.HoldAncillaryServices.HoldAsync(M1Commands.Hold(
            $"IDEMP-{orderId}-X", orderId, provisionId, _clock.Now.AddMinutes(30), (orderServiceId, 71, 301)));

        Assert.Equal(AncillaryReservationStatus.Held, second.Status);
    }

    [Fact]
    public async Task M1_N11_confirm_commits_the_hold_and_replays_safely()
    {
        var (_, provisionId, scope) = await WalkingCatalogAsync();
        await using var _ = scope;
        var orderId = NextOrderId;

        var hold = await scope.HoldAncillaryServices.HoldAsync(M1Commands.Hold(
            $"IDEMP-{orderId}", orderId, provisionId, _clock.Now.AddMinutes(30), (orderId * 10 + 1, 71, 301)));

        var confirmed = await scope.ConfirmAncillaryHold.ConfirmAsync(new TestConfirmAncillaryHoldCommand(hold.HoldId));

        Assert.Equal(AncillaryReservationStatus.Confirmed, confirmed.Status);
        Assert.All(confirmed.Units, unit => Assert.Equal(AncillaryReservationUnitStatus.Confirmed, unit.Status));

        var replay = await scope.ConfirmAncillaryHold.ConfirmAsync(new TestConfirmAncillaryHoldCommand(hold.HoldId));

        Assert.Equal(AncillaryReservationStatus.Confirmed, replay.Status);

        _clock.Now = _clock.Now.AddHours(2);

        var read = await scope.GetHoldById.ExecuteAsync(hold.HoldId);

        Assert.Equal(AncillaryReservationStatus.Confirmed, read.Status);
    }

    [Fact]
    public async Task M1_N11_an_expired_hold_cannot_be_confirmed()
    {
        var (_, provisionId, scope) = await WalkingCatalogAsync();
        await using var _ = scope;
        var orderId = NextOrderId;

        var hold = await scope.HoldAncillaryServices.HoldAsync(M1Commands.Hold(
            $"IDEMP-{orderId}", orderId, provisionId, _clock.Now.AddMinutes(30), (orderId * 10 + 1, 71, 301)));

        _clock.Now = _clock.Now.AddMinutes(31);

        await BusinessAssert.ThrowsAsync(16407, 409, () => scope.ConfirmAncillaryHold.ConfirmAsync(
            new TestConfirmAncillaryHoldCommand(hold.HoldId)));

        var read = await scope.GetHoldById.ExecuteAsync(hold.HoldId);

        Assert.Equal(AncillaryReservationStatus.Expired, read.Status);
    }

    [Fact]
    public async Task M1_A12_A14_an_external_supplier_fails_deterministically_and_local_routing_ignores_the_id()
    {
        var airlineId = _database.NextAirlineId();
        await using var scope = new AncillaryScope(_database, _clock);
        var supplier = await scope.RegisterSupplier.RegisterAsync(M1Commands.ExternalSupplier(airlineId));
        var definition = await scope.DefineServiceDefinition.DefineAsync(M1Commands.LoungeDefinition(airlineId, supplier.Id));
        await scope.ActivateServiceDefinition.ActivateAsync(new TestActivateServiceDefinitionCommand(definition.Id));
        var provision = await scope.DefineProvision.DefineAsync(M1Commands.LoungeProvision(definition.Id));
        await scope.ActivateProvision.ActivateAsync(new TestActivateProvisionCommand(provision.Id));
        var orderId = NextOrderId;

        await BusinessAssert.ThrowsAsync(16104, 422, () => scope.HoldAncillaryServices.HoldAsync(M1Commands.Hold(
            $"IDEMP-{orderId}", orderId, provision.Id, _clock.Now.AddMinutes(30), (orderId * 10 + 1, 71, 301))));
    }

    [Fact]
    public async Task M1_U08_the_hold_request_shape_carries_no_money_members()
    {
        var properties = new[]
            {
                typeof(IHoldAncillaryServicesCommand),
                typeof(AncillaryVariantHoldInput<>),
                typeof(AncillaryProductHoldInput<>),
                typeof(AncillaryUnitHoldInput<>),
                typeof(HoldUnitInput),
                typeof(HoldCoverageInput),
                typeof(SelectedUnitInput),
                typeof(SelectedUnitInput<>),
                typeof(BaggageHoldOptionsInput),
                typeof(AssistedTravelHoldOptionsInput),
                typeof(AssistedTravelHoldSelectionOptions),
                typeof(AirportServiceHoldOptionsInput),
                typeof(PriorityHoldOptionsInput),
                typeof(ConnectivityHoldOptionsInput)
            }
            .SelectMany(type => type.GetProperties())
            .Select(property => property.Name)
            .ToList();

        Assert.DoesNotContain(properties, name => name.Contains("Currency", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(properties, name => name.Contains("Revenue", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(properties, name => name.Contains("Amount", StringComparison.OrdinalIgnoreCase));
    }
}
