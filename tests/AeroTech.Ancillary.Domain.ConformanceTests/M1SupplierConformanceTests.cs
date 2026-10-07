using AeroTech.Ancillary.Domain.ConformanceTests.Fixtures;
using AeroTech.Ancillary.Domain.SupplierAggregate;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;
using static AeroTech.Ancillary.Domain.ConformanceTests.Fixtures.M1Fixtures;

namespace AeroTech.Ancillary.Domain.ConformanceTests;

public class M1SupplierConformanceTests
{
    [Fact]
    public void M1_A01_a_local_supplier_registers_active_with_a_null_provider_key()
    {
        var supplier = LocalSupplier();

        Assert.Equal(2001, supplier.Id);
        Assert.Equal(Airline, supplier.OwnerAirlineId);
        Assert.Equal("Dot Air", supplier.Name);
        Assert.Equal(SupplierFulfillmentKind.Local, supplier.FulfillmentKind);
        Assert.Null(supplier.FulfillmentProviderKey);
        Assert.Equal(SupplierStatus.Active, supplier.Status);
        Assert.Equal(Now, supplier.CreatedAt);
        Assert.Null(supplier.RetiredAt);
    }

    [Fact]
    public void M1_A15_the_airline_itself_registers_like_any_other_supplier()
    {
        var supplier = Supplier.Register(2009, Airline, "Dot Air", SupplierFulfillmentKind.Local, null, Now);

        Assert.Equal(SupplierStatus.Active, supplier.Status);
        Assert.Equal(SupplierFulfillmentKind.Local, supplier.FulfillmentKind);
    }

    [Fact]
    public void M1_A01_an_external_supplier_requires_a_provider_key()
    {
        var supplier = ExternalSupplier();

        Assert.Equal("LoungePartnerA", supplier.FulfillmentProviderKey);

        BusinessAssert.Throws(16102, 422, () => Supplier.Register(2003, Airline, "Partner", SupplierFulfillmentKind.External, null, Now));
        BusinessAssert.Throws(16102, 422, () => Supplier.Register(2003, Airline, "Partner", SupplierFulfillmentKind.External, " ", Now));
        BusinessAssert.Throws(16102, 422, () => Supplier.Register(2003, Airline, "Partner", SupplierFulfillmentKind.External, new string('K', 51), Now));
    }

    [Fact]
    public void M1_A01_a_local_supplier_must_not_carry_a_provider_key()
    {
        BusinessAssert.Throws(16102, 422, () => Supplier.Register(2003, Airline, "Dot Air", SupplierFulfillmentKind.Local, "Key", Now));
    }

    [Fact]
    public void M1_A01_owner_airline_and_name_are_validated()
    {
        BusinessAssert.Throws(16102, 422, () => Supplier.Register(2003, 0, "Dot Air", SupplierFulfillmentKind.Local, null, Now));
        BusinessAssert.Throws(16102, 422, () => Supplier.Register(2003, Airline, "", SupplierFulfillmentKind.Local, null, Now));
        BusinessAssert.Throws(16102, 422, () => Supplier.Register(2003, Airline, new string('N', 101), SupplierFulfillmentKind.Local, null, Now));
    }
}
