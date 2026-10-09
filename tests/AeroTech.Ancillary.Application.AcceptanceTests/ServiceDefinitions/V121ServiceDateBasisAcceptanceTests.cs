using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionsPaginated.Backoffice;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static AeroTech.Ancillary.Application.AcceptanceTests.Fixtures.P1Commands;

namespace AeroTech.Ancillary.Application.AcceptanceTests.ServiceDefinitions;

[Collection(DatabaseCollection.Name)]
public class V121ServiceDateBasisAcceptanceTests
{
    private readonly TestDatabase _database;
    private readonly FixedClock _clock = new();
    private readonly FamilyProof _proof;

    public V121ServiceDateBasisAcceptanceTests(TestDatabase database)
    {
        _database = database;
        _proof = new FamilyProof(database, _clock);
    }

    private async Task<TResult> RequestAsync<TResult>(Func<AncillaryScope, Task<TResult>> request)
    {
        await using var scope = new AncillaryScope(_database, _clock);

        return await request(scope);
    }

    private Task RefusedAsync(int code, int httpStatus, Func<AncillaryScope, Task> request)
        => BusinessAssert.ThrowsAsync(code, httpStatus, async () =>
        {
            await using var scope = new AncillaryScope(_database, _clock);

            await request(scope);
        });

    [Theory]
    [InlineData(ServiceDateBasis.FlightDeparture, 1)]
    [InlineData(ServiceDateBasis.ServiceStart, 2)]
    public async Task V121_D12_D13_every_service_date_basis_is_stored_typed_and_read_back_in_both_stores(ServiceDateBasis basis, int stored)
    {
        var airlineId = _database.NextAirlineId();
        var supplierId = await _proof.SupplierAsync(M1Commands.LocalSupplier(airlineId));
        var definition = await _proof.DefinitionAsync(
            CarrierDefinition(airlineId, supplierId, $"BASIS_{stored}", "BAS", "F", "TS", "Dated service", serviceDateBasis: basis));

        Assert.Equal((basis.ToString(), stored, "Active"), (definition.ServiceDateBasis!.Name, definition.ServiceDateBasis.Value, definition.Status.Name));
        Assert.Equal(stored, (int)basis);
        Assert.Equal(
            new[] { $"{stored}|{stored}" },
            await RequestAsync(scope => scope.Command.Database
                .SqlQuery<string>($"SELECT CONCAT((SELECT ServiceDateBasis FROM Ancillary.AncillaryServiceDefinitions WHERE Id = {definition.Id}), '|', (SELECT ServiceDateBasis FROM ReadModel.AncillaryServiceDefinitions WHERE Id = {definition.Id})) AS Value")
                .ToListAsync()));
        await RefusedAsync(16202, 422, scope => scope.DefineServiceDefinition.DefineAsync(
            CarrierDefinition(airlineId, supplierId, $"BASIS_BAD_{stored}", "BAS", "F", "TS", "Dated service", serviceDateBasis: (ServiceDateBasis)99)));
    }

    [Fact]
    public async Task V121_D12_the_basis_is_editable_on_a_first_draft_and_fixed_for_every_later_version_of_the_identity()
    {
        var airlineId = _database.NextAirlineId();
        var supplierId = await _proof.SupplierAsync(M1Commands.LocalSupplier(airlineId));
        var command = CarrierDefinition(airlineId, supplierId, "LOUNGE_ROOM", "LNG", "F", "LG", "Lounge room", serviceDateBasis: ServiceDateBasis.ServiceStart, variant: "A20");
        var draft = await RequestAsync(scope => scope.DefineServiceDefinition.DefineAsync(command));

        Assert.Equal(ServiceDateBasis.ServiceStart, draft.ServiceDateBasis);

        var corrected = await RequestAsync(scope => scope.ChangeServiceDefinition.ChangeAsync(Change(draft.Id, command with { ServiceDateBasis = ServiceDateBasis.FlightDeparture })));

        Assert.Equal(ServiceDateBasis.FlightDeparture, corrected.ServiceDateBasis);
        await RefusedAsync(16212, 409, scope => scope.AssignServiceDateBasis.AssignAsync(new TestAssignServiceDateBasisCommand(draft.Id, ServiceDateBasis.ServiceStart)));
        await RefusedAsync(16214, 409, scope => scope.AssignServiceDateBasis.AssignAsync(new TestAssignServiceDateBasisCommand(draft.Id, ServiceDateBasis.FlightDeparture)));
        await RefusedAsync(16201, 404, scope => scope.AssignServiceDateBasis.AssignAsync(new TestAssignServiceDateBasisCommand(999_999_999, ServiceDateBasis.ServiceStart)));

        await RequestAsync(scope => scope.ActivateServiceDefinition.ActivateAsync(new TestActivateServiceDefinitionCommand(draft.Id)));
        await RefusedAsync(16212, 409, scope => scope.ChangeServiceDefinition.ChangeAsync(Change(draft.Id, command with { ServiceDateBasis = ServiceDateBasis.ServiceStart })));
        await RefusedAsync(16203, 409, scope => scope.ChangeServiceDefinition.ChangeAsync(Change(draft.Id, command with { ServiceDateBasis = ServiceDateBasis.FlightDeparture, CommercialName = "Hotel suite" })));

        var revision = await RequestAsync(scope => scope.ReviseServiceDefinition.ReviseAsync(new TestServiceDefinitionLifecycleCommand(draft.Id)));

        Assert.Equal((2, ServiceDateBasis.FlightDeparture, ServiceDefinitionStatus.Draft), (revision.Version, revision.ServiceDateBasis!.Value, revision.Status));
        await RefusedAsync(16212, 409, scope => scope.ChangeServiceDefinition.ChangeAsync(
            Change(revision.Id, command with { ServiceDateBasis = ServiceDateBasis.ServiceStart })));
        await RefusedAsync(16212, 409, scope => scope.DefineServiceDefinition.DefineAsync(command with { ServiceDateBasis = ServiceDateBasis.Activation }));

        var renamed = await RequestAsync(scope => scope.ChangeServiceDefinition.ChangeAsync(
            Change(revision.Id, command with { ServiceDateBasis = ServiceDateBasis.FlightDeparture, CommercialName = "Hotel room night" })));

        Assert.Equal(ServiceDateBasis.FlightDeparture, renamed.ServiceDateBasis);
        Assert.Equal(
            new[] { ("FlightDeparture", 1), ("FlightDeparture", 2) },
            (await RequestAsync(scope => scope.Query.AncillaryServiceDefinitions.AsNoTracking()
                .Where(row => row.OwnerAirlineId == airlineId && row.ServiceDefinitionRef == "LOUNGE_ROOM")
                .OrderBy(row => row.Version)
                .ToListAsync()))
            .Select(row => (row.ServiceDateBasis!.Value.ToString(), row.Version)));

        var other = await RequestAsync(scope => scope.DefineServiceDefinition.DefineAsync(command with { ServiceDefinitionRef = "LOUNGE_ROOM_FLEX", ServiceDateBasis = ServiceDateBasis.ServiceStart }));

        Assert.Equal(ServiceDateBasis.ServiceStart, other.ServiceDateBasis);
        Assert.Equal(
            2,
            (await RequestAsync(scope => scope.GetServiceDefinitionsPaginated.ExecuteAsync(
                new BackofficeGetAncillaryServiceDefinitionsPaginatedQuery { OwnerAirlineId = airlineId, ServiceDefinitionRef = "LOUNGE_ROOM" }))).TotalCount);
    }

    [Theory]
    [InlineData(ServiceDateBasis.CheckIn)]
    [InlineData(ServiceDateBasis.CoverageStart)]
    [InlineData(ServiceDateBasis.Activation)]
    public async Task V122_a_service_date_basis_that_no_profile_variant_uses_is_refused(ServiceDateBasis basis)
    {
        var airlineId = _database.NextAirlineId();
        var supplierId = await _proof.SupplierAsync(M1Commands.LocalSupplier(airlineId));

        Assert.DoesNotContain(AncillaryVariant.All, variant => variant.ServiceDateBases.Contains(basis));
        await RefusedAsync(16202, 422, scope => scope.DefineServiceDefinition.DefineAsync(
            CarrierDefinition(airlineId, supplierId, $"BASIS_{basis}".ToUpperInvariant(), "BAS", "F", "TS", "Dated service", serviceDateBasis: basis)));
    }
}
