using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Provisions;

[Collection(DatabaseCollection.Name)]
public class M1ProvisionAcceptanceTests
{
    private readonly TestDatabase _database;
    private readonly FixedClock _clock = new();

    public M1ProvisionAcceptanceTests(TestDatabase database) => _database = database;

    private async Task<(long DefinitionId, AncillaryScope Scope)> ActiveDefinitionAsync()
    {
        var airlineId = _database.NextAirlineId();
        var scope = new AncillaryScope(_database, _clock);
        var supplier = await scope.RegisterSupplier.RegisterAsync(M1Commands.LocalSupplier(airlineId));
        var definition = await scope.DefineServiceDefinition.DefineAsync(M1Commands.LoungeDefinition(airlineId, supplier.Id));

        await scope.ActivateServiceDefinition.ActivateAsync(new TestActivateServiceDefinitionCommand(definition.Id));

        return (definition.Id, scope);
    }

    private static TestDefineProvisionCommand PaidLounge(long definitionId)
        => M1Commands.LoungeProvision(definitionId, disposition: CommercialDisposition.Paid);

    [Fact]
    public async Task M1_D01_a_provision_for_a_missing_definition_is_refused()
    {
        await using var scope = new AncillaryScope(_database, _clock);

        await BusinessAssert.ThrowsAsync(16304, 422, () => scope.DefineProvision.DefineAsync(
            M1Commands.LoungeProvision(999_999_999)));
    }

    [Fact]
    public async Task M1_D02_the_unique_active_sequence_is_backed_by_a_filtered_index()
    {
        var (definitionId, scope) = await ActiveDefinitionAsync();
        await using var _ = scope;

        var indexes = await scope.Command.Database
            .SqlQueryRaw<string>(
                "SELECT i.name AS Value FROM sys.indexes i JOIN sys.tables t ON t.object_id = i.object_id " +
                "WHERE t.name = 'AncillaryProvisions' AND i.has_filter = 1 AND i.is_unique = 1")
            .ToListAsync();

        Assert.Contains("IX_AncillaryProvisions_ServiceDefinitionId_Sequence_Active", indexes);
    }
}
