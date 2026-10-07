using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Query.SupplierAggregate.Queries.GetSuppliersPaginated.Backoffice;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Suppliers;

[Collection(DatabaseCollection.Name)]
public class P1SupplierAcceptanceTests
{
    private readonly TestDatabase _database;
    private readonly FixedClock _clock = new();

    public P1SupplierAcceptanceTests(TestDatabase database) => _database = database;

    [Fact]
    public async Task P1_K01_K06_a_supplier_is_retired_once_and_stays_listed_and_readable()
    {
        var airlineId = _database.NextAirlineId();
        await using var scope = new AncillaryScope(_database, _clock);
        var supplier = await scope.RegisterSupplier.RegisterAsync(M1Commands.ExternalSupplier(airlineId));
        var other = await scope.RegisterSupplier.RegisterAsync(M1Commands.LocalSupplier(airlineId, "Dot Air"));

        _clock.Now = _clock.Now.AddDays(3);

        var retired = await scope.RetireSupplier.RetireAsync(new TestRetireSupplierCommand(supplier.Id));

        Assert.Equal(SupplierStatus.Retired, retired.Status);

        await using var reader = new AncillaryScope(_database, _clock);
        var detail = await reader.GetSupplierById.ExecuteAsync(supplier.Id);

        Assert.Equal("Retired", detail.Status.Name);
        Assert.Equal(_clock.Now, detail.RetiredAt);
        Assert.Equal(("External", "LoungePartnerA", "Partner Lounge"), (detail.FulfillmentKind.Name, detail.FulfillmentProviderKey, detail.Name));
        Assert.Equal(SupplierStatus.Retired, (await reader.Suppliers.GetAsync(supplier.Id))!.Status);
        Assert.Equal(SupplierStatus.Retired, (await reader.Query.Suppliers.AsNoTracking().SingleAsync(row => row.Id == supplier.Id)).Status);

        var retiredRows = await reader.GetSuppliersPaginated.ExecuteAsync(
            new BackofficeGetSuppliersPaginatedQuery { OwnerAirlineId = airlineId, Status = SupplierStatus.Retired });
        var activeRows = await reader.GetSuppliersPaginated.ExecuteAsync(
            new BackofficeGetSuppliersPaginatedQuery { OwnerAirlineId = airlineId, Status = SupplierStatus.Active });

        Assert.Equal(supplier.Id.ToString(), Assert.Single(retiredRows.Results).Id);
        Assert.Equal(other.Id.ToString(), Assert.Single(activeRows.Results).Id);

        await BusinessAssert.ThrowsAsync(16103, 409, () => reader.RetireSupplier.RetireAsync(new TestRetireSupplierCommand(supplier.Id)));
        await BusinessAssert.ThrowsAsync(16101, 404, () => reader.RetireSupplier.RetireAsync(new TestRetireSupplierCommand(999_999_999)));
    }

    [Fact]
    public async Task P1_C03_a_definition_of_a_retired_supplier_cannot_be_activated_or_reactivated()
    {
        var airlineId = _database.NextAirlineId();
        await using var scope = new AncillaryScope(_database, _clock);
        var supplier = await scope.RegisterSupplier.RegisterAsync(M1Commands.LocalSupplier(airlineId));
        var draft = await scope.DefineServiceDefinition.DefineAsync(M1Commands.LoungeDefinition(airlineId, supplier.Id, "LNG_DRAFT"));
        var published = await scope.DefineServiceDefinition.DefineAsync(M1Commands.LoungeDefinition(airlineId, supplier.Id, "LNG_LIVE"));
        await scope.ActivateServiceDefinition.ActivateAsync(new TestActivateServiceDefinitionCommand(published.Id));
        await scope.SuspendServiceDefinition.SuspendAsync(new TestServiceDefinitionLifecycleCommand(published.Id));
        await scope.RetireSupplier.RetireAsync(new TestRetireSupplierCommand(supplier.Id));

        await using var writer = new AncillaryScope(_database, _clock);

        await BusinessAssert.ThrowsAsync(16206, 422, () => writer.ActivateServiceDefinition.ActivateAsync(
            new TestActivateServiceDefinitionCommand(draft.Id)));
        await BusinessAssert.ThrowsAsync(16206, 422, () => writer.ReactivateServiceDefinition.ReactivateAsync(
            new TestServiceDefinitionLifecycleCommand(published.Id)));

        await using var reader = new AncillaryScope(_database, _clock);

        Assert.Equal("Draft", (await reader.GetServiceDefinitionById.ExecuteAsync(draft.Id)).Status.Name);
        Assert.Equal("Suspended", (await reader.GetServiceDefinitionById.ExecuteAsync(published.Id)).Status.Name);
    }

    [Fact]
    public async Task P1_B01_two_suppliers_own_separate_definitions_of_the_same_sub_code()
    {
        var airlineId = _database.NextAirlineId();
        await using var scope = new AncillaryScope(_database, _clock);
        var dotAir = await scope.RegisterSupplier.RegisterAsync(M1Commands.LocalSupplier(airlineId, "Dot Air"));
        var partner = await scope.RegisterSupplier.RegisterAsync(M1Commands.LocalSupplier(airlineId, "Dot Air"));
        var own = await scope.DefineServiceDefinition.DefineAsync(M1Commands.LoungeDefinition(airlineId, dotAir.Id, "LNG_OWN"));
        var theirs = await scope.DefineServiceDefinition.DefineAsync(M1Commands.LoungeDefinition(airlineId, partner.Id, "LNG_PARTNER"));
        await scope.ActivateServiceDefinition.ActivateAsync(new TestActivateServiceDefinitionCommand(own.Id));
        await scope.ActivateServiceDefinition.ActivateAsync(new TestActivateServiceDefinitionCommand(theirs.Id));

        await using var reader = new AncillaryScope(_database, _clock);
        var first = await reader.GetServiceDefinitionById.ExecuteAsync(own.Id);
        var second = await reader.GetServiceDefinitionById.ExecuteAsync(theirs.Id);

        Assert.NotEqual(dotAir.Id, partner.Id);
        Assert.NotEqual(first.Id, second.Id);
        Assert.Equal((dotAir.Id, "0BX", "Active"), (first.SupplierId, first.ServiceSubCode, first.Status.Name));
        Assert.Equal((partner.Id, "0BX", "Active"), (second.SupplierId, second.ServiceSubCode, second.Status.Name));
        Assert.Equal(first.SupplierName, second.SupplierName);
    }
}
