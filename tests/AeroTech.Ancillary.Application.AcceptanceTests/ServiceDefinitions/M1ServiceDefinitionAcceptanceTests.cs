using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;

namespace AeroTech.Ancillary.Application.AcceptanceTests.ServiceDefinitions;

[Collection(DatabaseCollection.Name)]
public class M1ServiceDefinitionAcceptanceTests
{
    private readonly TestDatabase _database;
    private readonly FixedClock _clock = new();

    public M1ServiceDefinitionAcceptanceTests(TestDatabase database) => _database = database;

    [Fact]
    public async Task M1_C01_C04_a_definition_is_defined_as_draft_and_activated_once()
    {
        var airlineId = _database.NextAirlineId();
        await using var scope = new AncillaryScope(_database, _clock);
        var supplier = await scope.RegisterSupplier.RegisterAsync(M1Commands.LocalSupplier(airlineId));

        var draft = await scope.DefineServiceDefinition.DefineAsync(M1Commands.LoungeDefinition(airlineId, supplier.Id));

        Assert.Equal(ServiceDefinitionStatus.Draft, draft.Status);
        Assert.Equal(1, draft.Version);
        Assert.Equal("F", draft.ServiceTypeCode);
        Assert.Equal("0BX", draft.ServiceSubCode);

        var activated = await scope.ActivateServiceDefinition.ActivateAsync(new TestActivateServiceDefinitionCommand(draft.Id));

        Assert.Equal(ServiceDefinitionStatus.Active, activated.Status);

        await using var reader = new AncillaryScope(_database, _clock);
        var detail = await reader.GetServiceDefinitionById.ExecuteAsync(draft.Id);

        Assert.Equal(supplier.Id, detail.SupplierId);
        Assert.Equal("Dot Air", detail.SupplierName);
        Assert.Equal("LNG_IKA_CIP", detail.ServiceDefinitionRef);
        Assert.Equal("Industry", detail.SubCodeSource.Name);
        Assert.Equal("LG", detail.GroupCode);
        Assert.Equal("EMD Standalone", detail.DocumentType.Title);
        Assert.Equal("0BX", detail.DocumentRfisc);
        Assert.Equal("No Booking Process Required", detail.BookingMethod.Title);
        Assert.Equal("Active", detail.Status.Name);
        Assert.Equal(_clock.Now, detail.ActivatedAt);
    }

    [Fact]
    public async Task M1_A10_a_definition_for_a_missing_supplier_is_refused()
    {
        var airlineId = _database.NextAirlineId();
        await using var scope = new AncillaryScope(_database, _clock);

        await BusinessAssert.ThrowsAsync(16205, 422, () => scope.DefineServiceDefinition.DefineAsync(
            M1Commands.LoungeDefinition(airlineId, 999_999_999)));
    }

    [Fact]
    public async Task M1_C04_only_one_active_definition_per_airline_and_reference()
    {
        var airlineId = _database.NextAirlineId();
        await using var scope = new AncillaryScope(_database, _clock);
        var supplier = await scope.RegisterSupplier.RegisterAsync(M1Commands.LocalSupplier(airlineId));
        var first = await scope.DefineServiceDefinition.DefineAsync(M1Commands.LoungeDefinition(airlineId, supplier.Id));
        var second = await scope.DefineServiceDefinition.DefineAsync(M1Commands.LoungeDefinition(airlineId, supplier.Id));

        await scope.ActivateServiceDefinition.ActivateAsync(new TestActivateServiceDefinitionCommand(first.Id));

        await BusinessAssert.ThrowsAsync(16204, 409, () => scope.ActivateServiceDefinition.ActivateAsync(
            new TestActivateServiceDefinitionCommand(second.Id)));
    }

    [Fact]
    public async Task M1_C10_the_same_reference_may_be_active_for_two_airlines()
    {
        var firstAirline = _database.NextAirlineId();
        var secondAirline = _database.NextAirlineId();
        await using var scope = new AncillaryScope(_database, _clock);
        var firstSupplier = await scope.RegisterSupplier.RegisterAsync(M1Commands.LocalSupplier(firstAirline));
        var secondSupplier = await scope.RegisterSupplier.RegisterAsync(M1Commands.LocalSupplier(secondAirline, "Mahan"));
        var first = await scope.DefineServiceDefinition.DefineAsync(M1Commands.LoungeDefinition(firstAirline, firstSupplier.Id));
        var second = await scope.DefineServiceDefinition.DefineAsync(M1Commands.LoungeDefinition(secondAirline, secondSupplier.Id));

        await scope.ActivateServiceDefinition.ActivateAsync(new TestActivateServiceDefinitionCommand(first.Id));
        var activated = await scope.ActivateServiceDefinition.ActivateAsync(new TestActivateServiceDefinitionCommand(second.Id));

        Assert.Equal(ServiceDefinitionStatus.Active, activated.Status);
    }
}
