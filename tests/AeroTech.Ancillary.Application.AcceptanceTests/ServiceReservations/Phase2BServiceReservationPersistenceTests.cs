using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.AncillaryPriceRuleAggregate.Commands.DefineAncillaryPriceRule;
using AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.ReleaseServiceReservation;
using AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.ReleaseServiceReservation.Service;
using AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.ReserveServiceReservation;
using AeroTech.Ancillary.Application.ServiceReservationAggregate.Commands.ReserveServiceReservation.Service;
using AeroTech.Ancillary.Domain.ServiceReservationAggregate;
using AeroTech.Ancillary.Domain.ServiceReservationAggregate.Arguments;
using AeroTech.Framework.Core.Domain.Exceptions;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AeroTech.Ancillary.Application.AcceptanceTests.ServiceReservations;

[Collection(DatabaseCollection.Name)]
public sealed class Phase2BServiceReservationPersistenceTests(TestDatabase database) : IAsyncLifetime
{
    private readonly AncillaryHarness _harness = new(database);

    private AncillaryPriceRuleResult _ruleR = null!;
    private AncillaryPriceRuleResult _ruleRL = null!;

    private int Airline => _harness.AirlineId;

    private string KeyPrefix => $"p2b-{Airline}:";

    public async Task InitializeAsync()
    {
        await _harness.RegisterAsync(Phase1Commands.SubCodeS(Airline));
        await _harness.RegisterAsync(Phase2Commands.SubCodeL(Airline));
        await _harness.ArrangeProductAsync(Phase1Commands.ProductX(Airline));
        await _harness.ArrangeProductAsync(Phase2Commands.ProductL(Airline));

        _ruleR = await _harness.ArrangePriceRuleAsync(Phase1Commands.RuleR(Airline));
        _ruleRL = await _harness.ArrangePriceRuleAsync(Phase2Commands.RuleRL(Airline));
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task P2B_V11_UniqueIndexRejectsASecondReservationWithTheSameKey()
    {
        await _harness.ReserveAsync(Reserve("v11-index"));

        await using var scope = _harness.NewScope();

        await scope.Reservations.AddAsync(ServiceReservation.Reserve(
            database.Ids.NewId(),
            KeyPrefix + "v11-index",
            "order:9001",
            _harness.Clock.Now.AddMinutes(30),
            [new ServiceReservationUnitArgs("U1", Airline, "XBAG1", 1, _ruleR.Id, "T1", "B1", null, [100], 1, 978, 38.50m, AncillaryInventoryControl.Unlimited)],
            database.Ids,
            _harness.Clock.Now));

        await Assert.ThrowsAsync<DbUpdateException>(() => scope.Command.SaveChangesAsync());
        Assert.Equal(1, await StoredReservationsAsync());
    }

    [Fact]
    public async Task P2B_V11_LoserOfTwoSimultaneousReservesReceivesTheWinnersReservation()
    {
        var command = Reserve("v11-gated");

        await using var losing = _harness.NewScope();

        var gate = new GatedServiceReservationRepository(losing.Reservations);
        var loser = new ReserveServiceReservationService(gate, losing.Products, losing.PriceRules, losing.UnitOfWork, database.Ids, _harness.Clock)
            .ReserveAsync(command);

        await gate.Loaded;

        var winner = await _harness.ReserveAsync(command);

        gate.Release();

        var lost = await loser;

        Assert.Equal((winner.ReservationId, winner.Units[0].UnitRef), (lost.ReservationId, lost.Units[0].UnitRef));
        Assert.Equal((1, 2), (await StoredReservationsAsync(), await StoredUnitsAsync()));
    }

    [Fact]
    public async Task P2B_V11_ParallelReservesWithTheSameKeyEndWithOneReservation()
    {
        var command = Reserve("v11-parallel");

        var results = await Task.WhenAll(Enumerable.Range(0, 4).Select(_ => Task.Run(() => _harness.ReserveAsync(command))));

        Assert.Single(results.Select(result => (result.ReservationId, result.Units[0].UnitRef)).Distinct());
        Assert.Equal((1, 2), (await StoredReservationsAsync(), await StoredUnitsAsync()));
    }

    [Fact]
    public async Task P2B_V19_LoserOfSimultaneousConfirmAndReleaseChangesNothing()
    {
        var held = await _harness.ReserveAsync(Reserve("v19-gated"));

        await using var losing = _harness.NewScope();

        var gate = new GatedServiceReservationRepository(losing.Reservations);
        var loser = new ReleaseServiceReservationService(gate, losing.UnitOfWork, _harness.Clock)
            .ReleaseAsync(new ServiceReleaseServiceReservationCommand(held.ReservationId));

        await gate.Loaded;

        var winner = await _harness.ConfirmReservationAsync(held.ReservationId);
        var afterWinner = await StatusesAsync(held.ReservationId);

        gate.Release();

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => loser);

        Assert.All(winner.Units, unit => Assert.Equal(ServiceReservationUnitStatus.Confirmed, unit.Status));
        Assert.Equal([ServiceReservationUnitStatus.Confirmed, ServiceReservationUnitStatus.Confirmed], afterWinner);
        Assert.Equal(afterWinner, await StatusesAsync(held.ReservationId));
    }

    [Fact]
    public async Task P2B_V19_ParallelConfirmAndReleaseLeaveExactlyOneOutcome()
    {
        var held = await _harness.ReserveAsync(Reserve("v19-parallel"));

        var outcomes = await Task.WhenAll(
            Task.Run(() => TryAsync(() => _harness.ConfirmReservationAsync(held.ReservationId), ServiceReservationUnitStatus.Confirmed)),
            Task.Run(() => TryAsync(() => _harness.ReleaseReservationAsync(held.ReservationId), ServiceReservationUnitStatus.Released)));

        var succeeded = Assert.Single(outcomes.OfType<ServiceReservationUnitStatus>());
        var statuses = await StatusesAsync(held.ReservationId);

        Assert.Equal([succeeded, succeeded], statuses);
    }

    private static async Task<ServiceReservationUnitStatus?> TryAsync(Func<Task<ServiceReservationResult>> operation, ServiceReservationUnitStatus target)
    {
        try
        {
            var result = await operation();

            Assert.All(result.Units, unit => Assert.Equal(target, unit.Status));

            return target;
        }
        catch (DbUpdateConcurrencyException)
        {
            return null;
        }
        catch (BusinessException exception) when (exception.Code is 16407 or 16410)
        {
            return null;
        }
    }

    private ServiceReserveServiceReservationCommand Reserve(string name)
        => Phase2BCommands.Reserve(
            KeyPrefix + name,
            _harness.Clock.Now.AddMinutes(30),
            Phase2BCommands.Context(Airline, _harness.Clock.Now),
            Phase2BCommands.BagUnit(_ruleR.Id, "U1"),
            Phase2BCommands.LoungeUnit(_ruleRL.Id));

    private Task<int> StoredReservationsAsync()
        => _harness.RunAsync(scope => scope.Command.ServiceReservations.CountAsync(reservation => reservation.IdempotencyKey.StartsWith(KeyPrefix)));

    private Task<int> StoredUnitsAsync()
        => _harness.RunAsync(scope => scope.Command.ServiceReservations
            .Where(reservation => reservation.IdempotencyKey.StartsWith(KeyPrefix))
            .SelectMany(reservation => reservation.Units)
            .CountAsync());

    private Task<List<ServiceReservationUnitStatus>> StatusesAsync(long reservationId)
        => _harness.RunAsync(async scope => (await scope.Command.ServiceReservations
                .AsNoTracking()
                .Include(reservation => reservation.Units)
                .SingleAsync(reservation => reservation.Id == reservationId))
            .Units
            .OrderBy(unit => unit.Id)
            .Select(unit => unit.Status)
            .ToList());
}
