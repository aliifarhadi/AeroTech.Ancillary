using AeroTech.Ancillary.Application.AcceptanceTests.Fakes;
using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;

namespace AeroTech.Ancillary.Application.AcceptanceTests.Suppliers;

[Collection(DatabaseCollection.Name)]
public class M1SupplierAcceptanceTests
{
    private readonly TestDatabase _database;
    private readonly FixedClock _clock = new();

    public M1SupplierAcceptanceTests(TestDatabase database) => _database = database;

    [Fact]
    public async Task M1_A01_a_registered_local_supplier_is_stored_and_readable()
    {
        var airlineId = _database.NextAirlineId();
        await using var scope = new AncillaryScope(_database, _clock);

        var result = await scope.RegisterSupplier.RegisterAsync(M1Commands.LocalSupplier(airlineId));

        Assert.True(result.Id > 0);
        Assert.Equal(SupplierFulfillmentKind.Local, result.FulfillmentKind);
        Assert.Null(result.FulfillmentProviderKey);
        Assert.Equal(SupplierStatus.Active, result.Status);

        await using var reader = new AncillaryScope(_database, _clock);
        var detail = await reader.GetSupplierById.ExecuteAsync(result.Id);

        Assert.Equal(airlineId, detail.OwnerAirlineId);
        Assert.Equal("Dot Air", detail.Name);
        Assert.Equal("Local", detail.FulfillmentKind.Name);
        Assert.Null(detail.FulfillmentProviderKey);
        Assert.Equal("Active", detail.Status.Name);
        Assert.Equal(_clock.Now, detail.CreatedAt);
        Assert.Null(detail.RetiredAt);
    }

    [Fact]
    public async Task M1_A01_an_unknown_supplier_read_is_a_business_404()
    {
        await using var scope = new AncillaryScope(_database, _clock);

        await BusinessAssert.ThrowsAsync(16101, 404, () => scope.GetSupplierById.ExecuteAsync(999_999_999));
    }
}
