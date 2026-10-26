using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Domain.AncillaryInventoryPolicyAggregate.Contracts;
using AeroTech.Ancillary.Shopping.Offers;
using AeroTech.Ancillary.Shopping.Tests.Fixtures;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;

namespace AeroTech.Ancillary.Shopping.Tests;

public sealed class V10SupplierSaleGuardTests
{
    private const int NotSellable = 16820;

    private static AncillaryOfferRow? Row(IEnumerable<AncillaryOfferRow> rows, string variantCode)
        => rows.SingleOrDefault(row => row.Candidate.VariantCode == variantCode);

    [Fact]
    public async Task ASupplierHeldProductIsNotListedForSaleBecauseItsHoldWouldAlwaysBeRefused()
    {
        var lab = new V8Lab();
        var (supplier, _, _) = lab.Sellable("A01");
        var (unlimited, _, _) = lab.Sellable("A05");

        lab.Shop.Policy(supplier, InventoryAuthority.Supplier);
        lab.Shop.Policy(unlimited, InventoryAuthority.Unlimited);

        var rows = await lab.ShopAsync(V8Lab.Context());

        Assert.Null(Row(rows, "A01"));
        Assert.NotNull(Row(rows, "A05")!.OfferItemId);
    }

    [Fact]
    public async Task ASelectionSignedBeforeTheStockMovedToASupplierIsRefusedAtPrice()
    {
        var lab = new V8Lab();
        var (definition, _, _) = lab.Sellable("A01");

        lab.Shop.Policy(definition, InventoryAuthority.Unlimited);

        var row = Row(await lab.ShopAsync(V8Lab.Context()), "A01")!;

        lab.Shop.Catalog.Inventory.Clear();
        lab.Shop.Policy(definition, InventoryAuthority.Supplier);

        var refused = await BusinessAssert.ThrowsAsync(NotSellable, 409, () => lab.SubmitAsync([V8Lab.Direct(row.OfferItemId!, 1)], V8Lab.Context()));

        Assert.Contains("sale conditions changed", refused.Message);
    }
}
