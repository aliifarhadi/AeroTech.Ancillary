using AeroTech.Ancillary.Domain.ConformanceTests.Fixtures;
using AeroTech.Ancillary.Domain.SupplierAggregate;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;
using static AeroTech.Ancillary.Domain.ConformanceTests.Fixtures.P1Fixtures;

namespace AeroTech.Ancillary.Domain.ConformanceTests;

public class P1SupplierConformanceTests
{
    [Fact]
    public void P1_K06_a_supplier_is_retired_once_and_retired_is_terminal()
    {
        var supplier = Supplier();

        supplier.Retire(Now.AddDays(1));

        Assert.Equal((SupplierStatus.Retired, Now.AddDays(1)), (supplier.Status, supplier.RetiredAt!.Value));
        BusinessAssert.Throws(16103, 409, () => supplier.Retire(Now.AddDays(2)));
        Assert.Equal(Now.AddDays(1), supplier.RetiredAt);
        Assert.Equal(new[] { "Active", "Retired" }, Enum.GetNames<SupplierStatus>().OrderBy(name => name, StringComparer.Ordinal));
        Assert.DoesNotContain(
            typeof(Supplier).GetMethods().Select(method => method.Name),
            name => name.Contains("Reactivate", StringComparison.Ordinal) || name.Contains("Activate", StringComparison.Ordinal));
    }

    [Fact]
    public void P1_B02_B03_B04_the_supplier_keeps_its_baseline_shape_without_secrets_or_transport_settings()
    {
        string[] forbidden = ["Credential", "Password", "Secret", "Token", "Url", "Uri", "Endpoint", "ApiKey", "Certificate"];

        Assert.Equal(
            new[] { "CreatedAt", "FulfillmentKind", "FulfillmentProviderKey", "Name", "OwnerAirlineId", "RetiredAt", "Status" },
            PropertiesOf<Supplier>());

        foreach (var term in forbidden)
            Assert.DoesNotContain(PropertiesOf<Supplier>(), name => name.Contains(term, StringComparison.OrdinalIgnoreCase));

        var first = SupplierAggregate.Supplier.Register(2001, Airline, "Sky Lounge", SupplierFulfillmentKind.Local, null, Now);
        var second = SupplierAggregate.Supplier.Register(2002, Airline, "Sky Lounge", SupplierFulfillmentKind.External, "LoungePartnerA", Now);

        Assert.NotEqual(first.Id, second.Id);
        Assert.Equal(first.Name, second.Name);
        Assert.Equal((SupplierFulfillmentKind.External, "LoungePartnerA"), (second.FulfillmentKind, second.FulfillmentProviderKey));
    }
}
