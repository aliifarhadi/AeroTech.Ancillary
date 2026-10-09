using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryInventoryPolicyAggregate.Queries.GetInventoryConfigurationSnapshot.Backoffice;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.EntityFrameworkCore;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.P1Commands;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;

public sealed class InventoryHarness
{
    private readonly TestDatabase _database;
    private readonly FixedClock _clock;

    public InventoryHarness(TestDatabase database, FixedClock clock)
    {
        _database = database;
        _clock = clock;
        Proof = new FamilyProof(database, clock);
    }

    public FamilyProof Proof { get; }

    public async Task<TResult> RequestAsync<TResult>(InventoryFixture fixture, Func<InventoryScope, Task<TResult>> request)
    {
        await using var scope = new InventoryScope(_database, _clock, fixture);

        return await request(scope);
    }

    public Task RefusedAsync(int code, int httpStatus, InventoryFixture fixture, Func<InventoryScope, Task> request)
        => BusinessAssert.ThrowsAsync(code, httpStatus, async () =>
        {
            await using var scope = new InventoryScope(_database, _clock, fixture);

            await request(scope);
        });

    public async Task<(InventoryFixture Fixture, int AirlineId, long SupplierId)> OperatorAsync(bool connected = true)
    {
        var airlineId = _database.NextAirlineId();
        var supplierId = await Proof.SupplierAsync(M1Commands.LocalSupplier(airlineId));

        return (connected ? InventoryFixture.Connected(airlineId) : new InventoryFixture(airlineId), airlineId, supplierId);
    }

    public Task<BackofficeServiceDefinitionDto> ProductAsync(
        int airlineId,
        long supplierId,
        string reference,
        PricingUnit pricingUnit = PricingUnit.PerPassenger,
        ServiceDateBasis basis = ServiceDateBasis.FlightDeparture)
        => Proof.DefinitionAsync(CarrierDefinition(airlineId, supplierId, reference, "SVC", "F", "TS", reference, pricingUnit: pricingUnit, serviceDateBasis: basis));

    public Task<InventoryConfigurationSnapshotDto> SnapshotAsync(InventoryFixture fixture, string reference, long? flightId = null, DateTimeOffset? atUtc = null)
        => RequestAsync(fixture, scope => scope.GetInventoryConfigurationSnapshot.ExecuteAsync(
            new BackofficeGetInventoryConfigurationSnapshotQuery { ServiceDefinitionRef = reference, FlightId = flightId, AtUtc = atUtc }));

    public async Task<List<string>> RowsAsync(FormattableString sql)
    {
        await using var context = _database.NewContext(_clock);

        return await context.Database.SqlQuery<string>(sql).ToListAsync();
    }

    public Task<List<string>> SourceDifferencesAsync(int airlineId)
        => RowsAsync($"""
            SELECT CONCAT('FlightCountInventory ', COALESCE(command.Id, model.Id)) AS Value
            FROM Ancillary.FlightCountInventories AS command
            FULL JOIN ReadModel.FlightCountInventories AS model ON model.Id = command.Id
            WHERE COALESCE(command.OwnerAirlineId, model.OwnerAirlineId) = {airlineId}
              AND (command.Id IS NULL OR model.Id IS NULL OR command.FlightId <> model.FlightId OR command.ResourceId <> model.ResourceId OR command.CountUnit <> model.CountUnit OR command.TotalCapacity <> model.TotalCapacity OR command.ClosedForSale <> model.ClosedForSale OR command.Status <> model.Status OR command.Version <> model.Version OR command.CreatedAt <> model.CreatedAt OR command.UpdatedAt <> model.UpdatedAt
                OR model.AdjustmentCount <> (SELECT COUNT(*) FROM Ancillary.FlightCountAdjustments AS line WHERE line.FlightCountInventoryId = command.Id))
            UNION ALL
            SELECT CONCAT('FlightCountAdjustment ', COALESCE(command.Id, model.Id))
            FROM Ancillary.FlightCountAdjustments AS command
            FULL JOIN ReadModel.FlightCountAdjustments AS model ON model.Id = command.Id
            WHERE EXISTS (SELECT 1 FROM Ancillary.FlightCountInventories AS owner WHERE owner.Id = COALESCE(command.FlightCountInventoryId, model.FlightCountInventoryId) AND owner.OwnerAirlineId = {airlineId})
              AND (command.Id IS NULL OR model.Id IS NULL OR command.PreviousTotal <> model.PreviousTotal OR command.NewTotal <> model.NewTotal OR command.ReasonCode <> model.ReasonCode OR command.ActorId <> model.ActorId OR command.CorrelationId <> model.CorrelationId OR command.OccurredAt <> model.OccurredAt OR command.ExpectedVersion <> model.ExpectedVersion OR command.ResultingVersion <> model.ResultingVersion OR command.FlightCountInventoryId <> model.FlightCountInventoryId)
            UNION ALL
            SELECT CONCAT('FlightWeightInventory ', COALESCE(command.Id, model.Id)) AS Value
            FROM Ancillary.FlightWeightInventories AS command
            FULL JOIN ReadModel.FlightWeightInventories AS model ON model.Id = command.Id
            WHERE COALESCE(command.OwnerAirlineId, model.OwnerAirlineId) = {airlineId}
              AND (command.Id IS NULL OR model.Id IS NULL OR command.FlightId <> model.FlightId OR command.WeightResourceId <> model.WeightResourceId OR command.CapacityKg <> model.CapacityKg OR command.ClosedForSale <> model.ClosedForSale OR command.Status <> model.Status OR command.Version <> model.Version OR command.CreatedAt <> model.CreatedAt OR command.UpdatedAt <> model.UpdatedAt
                OR model.AdjustmentCount <> (SELECT COUNT(*) FROM Ancillary.FlightWeightAdjustments AS line WHERE line.FlightWeightInventoryId = command.Id))
            UNION ALL
            SELECT CONCAT('FlightWeightAdjustment ', COALESCE(command.Id, model.Id))
            FROM Ancillary.FlightWeightAdjustments AS command
            FULL JOIN ReadModel.FlightWeightAdjustments AS model ON model.Id = command.Id
            WHERE EXISTS (SELECT 1 FROM Ancillary.FlightWeightInventories AS owner WHERE owner.Id = COALESCE(command.FlightWeightInventoryId, model.FlightWeightInventoryId) AND owner.OwnerAirlineId = {airlineId})
              AND (command.Id IS NULL OR model.Id IS NULL OR command.PreviousKg <> model.PreviousKg OR command.NewKg <> model.NewKg OR command.ReasonCode <> model.ReasonCode OR command.ActorId <> model.ActorId OR command.CorrelationId <> model.CorrelationId OR command.OccurredAt <> model.OccurredAt OR command.ExpectedVersion <> model.ExpectedVersion OR command.ResultingVersion <> model.ResultingVersion OR command.FlightWeightInventoryId <> model.FlightWeightInventoryId)
            UNION ALL
            SELECT CONCAT('AirportSlotInventory ', COALESCE(command.Id, model.Id)) AS Value
            FROM Ancillary.AirportSlotInventories AS command
            FULL JOIN ReadModel.AirportSlotInventories AS model ON model.Id = command.Id
            WHERE COALESCE(command.OwnerAirlineId, model.OwnerAirlineId) = {airlineId}
              AND (command.Id IS NULL OR model.Id IS NULL OR command.AirportId <> model.AirportId OR command.FacilityId <> model.FacilityId OR command.StartUtc <> model.StartUtc OR command.EndUtc <> model.EndUtc OR command.CapacityPersons <> model.CapacityPersons OR command.ClosedForSale <> model.ClosedForSale OR command.Status <> model.Status OR command.Version <> model.Version OR command.CreatedAt <> model.CreatedAt OR command.UpdatedAt <> model.UpdatedAt
                OR model.AdjustmentCount <> (SELECT COUNT(*) FROM Ancillary.AirportSlotAdjustments AS line WHERE line.AirportSlotInventoryId = command.Id))
            UNION ALL
            SELECT CONCAT('AirportSlotAdjustment ', COALESCE(command.Id, model.Id))
            FROM Ancillary.AirportSlotAdjustments AS command
            FULL JOIN ReadModel.AirportSlotAdjustments AS model ON model.Id = command.Id
            WHERE EXISTS (SELECT 1 FROM Ancillary.AirportSlotInventories AS owner WHERE owner.Id = COALESCE(command.AirportSlotInventoryId, model.AirportSlotInventoryId) AND owner.OwnerAirlineId = {airlineId})
              AND (command.Id IS NULL OR model.Id IS NULL OR command.PreviousTotal <> model.PreviousTotal OR command.NewTotal <> model.NewTotal OR command.ReasonCode <> model.ReasonCode OR command.ActorId <> model.ActorId OR command.CorrelationId <> model.CorrelationId OR command.OccurredAt <> model.OccurredAt OR command.ExpectedVersion <> model.ExpectedVersion OR command.ResultingVersion <> model.ResultingVersion OR command.AirportSlotInventoryId <> model.AirportSlotInventoryId)
            """);

    public async Task<(int Succeeded, int[] Codes)> RaceAsync(IEnumerable<Func<Task>> attempts)
    {
        var outcomes = await Task.WhenAll(attempts.Select(async attempt =>
        {
            try
            {
                await attempt();

                return 0;
            }
            catch (AeroTech.Framework.Core.Domain.Exceptions.BusinessException exception)
            {
                return exception.Code;
            }
        }));

        return (outcomes.Count(code => code == 0), outcomes.Where(code => code != 0).Distinct().OrderBy(code => code).ToArray());
    }
}
