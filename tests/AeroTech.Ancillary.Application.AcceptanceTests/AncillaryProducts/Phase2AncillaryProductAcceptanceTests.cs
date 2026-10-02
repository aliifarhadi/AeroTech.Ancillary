using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.DefineAncillaryProduct;
using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.DefineAncillaryProduct.Backoffice;
using AeroTech.Ancillary.Query.AncillaryProductAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryProductAggregate.Queries.GetAncillaryProductsPaginated.Backoffice;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AeroTech.Ancillary.Application.AcceptanceTests.AncillaryProducts;

[Collection(DatabaseCollection.Name)]
public sealed class Phase2AncillaryProductAcceptanceTests(TestDatabase database) : IAsyncLifetime
{
    private readonly AncillaryHarness _harness = new(database);

    private int Airline => _harness.AirlineId;

    private BackofficeDefineAncillaryProductCommand X => Phase1Commands.ProductX(Airline);

    private BackofficeDefineAncillaryProductCommand L => Phase2Commands.ProductL(Airline);

    private BackofficeDefineAncillaryProductCommand LG => Phase2Commands.ProductLG(Airline);

    public async Task InitializeAsync()
    {
        await _harness.RegisterAsync(Phase1Commands.SubCodeS(Airline));
        await _harness.RegisterAsync(Phase2Commands.SubCodeL(Airline));
        await _harness.RegisterAsync(Phase2Commands.SubCodeGL(Airline));
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task P2_C01_LoungeProductIsDefinedActivatedAndReadWithItsLounge()
    {
        var defined = await _harness.DefineAsync(L);
        var activated = await _harness.ActivateProductAsync(defined.Id);
        var lounge = await _harness.GetProductAsync(defined.Id);
        var bag = await _harness.GetProductAsync((await _harness.DefineAsync(X)).Id);

        Assert.Equal((Airline, "LNGTHR", 1, AncillaryProductStatus.Draft), (defined.OwnerAirlineId, defined.ProductRef, defined.Version, defined.Status));
        Assert.Equal(AncillaryProductStatus.Active, activated.Status);
        Assert.Equal(("LoungeAccess", "Lounge access", null, "TravellerSegment", "Unlimited", "Active"), (lounge.Type.Name, lounge.Name, lounge.Description, lounge.SalesScope.Name, lounge.InventoryControl.Name, lounge.Status.Name));
        Assert.Equal(("Each", 1, 1), (lounge.Quantity.Unit.Name, lounge.Quantity.Min, lounge.Quantity.Max));
        Assert.Equal(("EmdStandalone", "0BX", "E"), (lounge.Document.Type.Name, lounge.Document.Rfisc, lounge.Document.Rfic));
        Assert.Equal(new ProductCodesDto("F", "LG", null, null, null), lounge.Codes);
        Assert.Equal(new ProductTermsDto(false, null, null, null, null), lounge.Terms);
        Assert.Equal([1], lounge.Lounge!.AirportIds);
        Assert.Null(lounge.Baggage);
        Assert.Null(bag.Lounge);
        Assert.NotNull(bag.Baggage);

        var page = await _harness.RunAsync(scope => scope.GetProductsPaginated.ExecuteAsync(
            new BackofficeGetAncillaryProductsPaginatedQuery { OwnerAirlineId = Airline }));

        Assert.Equal(
            [("LNGTHR", true, false), ("XBAG1", false, true)],
            page.Results.Select(row => (row.ProductRef, row.Lounge is not null, row.Baggage is not null)));
    }

    [Theory]
    [InlineData(AncillaryProductType.LoungeAccess, AncillarySalesScope.TravellerBound, AncillaryDocumentType.EmdStandalone, AncillaryQuantityUnit.Each)]
    [InlineData(AncillaryProductType.LoungeAccess, AncillarySalesScope.TravellerSegment, AncillaryDocumentType.EmdAssociated, AncillaryQuantityUnit.Each)]
    [InlineData(AncillaryProductType.LoungeAccess, AncillarySalesScope.TravellerSegment, AncillaryDocumentType.EmdStandalone, AncillaryQuantityUnit.Piece)]
    [InlineData(AncillaryProductType.ExtraBaggage, AncillarySalesScope.TravellerSegment, AncillaryDocumentType.EmdAssociated, AncillaryQuantityUnit.Piece)]
    [InlineData(AncillaryProductType.ExtraBaggage, AncillarySalesScope.TravellerBound, AncillaryDocumentType.EmdStandalone, AncillaryQuantityUnit.Piece)]
    [InlineData(AncillaryProductType.ExtraBaggage, AncillarySalesScope.TravellerBound, AncillaryDocumentType.EmdAssociated, AncillaryQuantityUnit.Each)]
    public async Task P2_C02_CombinationThatIsNotSoldIsRefused(
        AncillaryProductType type,
        AncillarySalesScope salesScope,
        AncillaryDocumentType documentType,
        AncillaryQuantityUnit unit)
    {
        var valid = type == AncillaryProductType.LoungeAccess ? L : X;
        var unsold = valid with
        {
            SalesScope = salesScope,
            Quantity = valid.Quantity with { Unit = unit },
            Document = valid.Document with { Type = documentType }
        };

        await BusinessAssert.ThrowsAsync(16105, 422, () => _harness.DefineAsync(unsold));

        var draft = await _harness.DefineAsync(valid);
        var before = await _harness.GetProductAsync(draft.Id);

        await BusinessAssert.ThrowsAsync(16105, 422, () => _harness.ChangeAsync(Phase1Commands.ChangeTo(draft.Id, unsold)));
        Assert.Equal(before, await _harness.GetProductAsync(draft.Id), ProductComparer.Instance);
    }

    [Fact]
    public async Task P2_C03_CombinationIsCheckedBeforeEveryOtherProductRule()
    {
        var broken = L with
        {
            SalesScope = AncillarySalesScope.TravellerBound,
            Lounge = null,
            Document = new ProductDocument(AncillaryDocumentType.EmdStandalone, "0ZZ")
        };

        await BusinessAssert.ThrowsAsync(16105, 422, () => _harness.DefineAsync(broken));
        await BusinessAssert.ThrowsAsync(16105, 422, () => _harness.DefineAsync(broken with { Quantity = new ProductQuantity(AncillaryQuantityUnit.Each, 0, 100) }));
        await BusinessAssert.ThrowsAsync(16105, 422, () => _harness.DefineAsync(broken with { Lounge = new ProductLounge([]) }));
        await BusinessAssert.ThrowsAsync(16105, 422, () => _harness.DefineAsync(broken with { Baggage = new ProductBaggage(null, null, null) }));

        var draft = await _harness.DefineAsync(L);

        await BusinessAssert.ThrowsAsync(16105, 422, () => _harness.ChangeAsync(Phase1Commands.ChangeTo(draft.Id, broken with { Quantity = new ProductQuantity(AncillaryQuantityUnit.Each, 0, 100) })));
        await BusinessAssert.ThrowsAsync(16106, 422, () => _harness.DefineAsync(broken with { SalesScope = AncillarySalesScope.TravellerSegment, ProductRef = "LNGNL" }));
        await BusinessAssert.ThrowsAsync(16109, 422, () => _harness.DefineAsync(broken with { SalesScope = AncillarySalesScope.TravellerSegment, ProductRef = "LNGZZ", Lounge = new ProductLounge([1]) }));
    }

    [Fact]
    public async Task P2_C04_LoungeDetailIsRequiredAndConsistent()
    {
        await InvalidAsync(L with { Lounge = null });
        await InvalidAsync(L with { Lounge = new ProductLounge([]) });
        await InvalidAsync(L with { Lounge = new ProductLounge([1, 1]) });
        await InvalidAsync(L with { Lounge = new ProductLounge([1, 0]) });
        await InvalidAsync(L with { Baggage = new ProductBaggage(1, 23m, AncillaryWeightUnit.Kg) });
        await InvalidAsync(X with { Lounge = new ProductLounge([1]) });

        var draft = await _harness.DefineAsync(L);

        await BusinessAssert.ThrowsAsync(16106, 422, () => _harness.ChangeAsync(Phase1Commands.ChangeTo(draft.Id, L with { Lounge = null })));
    }

    [Fact]
    public void P2_C04_LoungeWithoutAnAirportListIsMalformed()
    {
        var validator = new BackofficeDefineAncillaryProductCommandValidator();

        ValidationAssert.Accepts(validator, L);
        ValidationAssert.Accepts(validator, L with { Lounge = new ProductLounge([]) });
        ValidationAssert.Accepts(validator, X);
        ValidationAssert.Rejects(validator, L with { Lounge = new ProductLounge(null!) });
        ApiJson.Malformed<BackofficeDefineAncillaryProductCommand>("""{ "lounge": { "airportIds": ["THR"] } }""");
        ApiJson.Malformed<BackofficeDefineAncillaryProductCommand>("""{ "lounge": { "airportIds": 1 } }""");
        Assert.Equal([1, 5], ApiJson.Deserialize<BackofficeDefineAncillaryProductCommand>("""{ "lounge": { "airportIds": [1, 5] } }""").Lounge!.AirportIds);
    }

    [Fact]
    public async Task P2_C05_LoungeNeedsTheLoungeClassification()
    {
        await _harness.RegisterAsync(Phase2Commands.SubCodeGL(Airline) with { Code = "XEB", GroupCode = "BG" });
        await _harness.RegisterAsync(Phase2Commands.SubCodeGL(Airline) with { Code = "XCL", Rfic = "C" });

        await InvalidAsync(L with { Document = new ProductDocument(AncillaryDocumentType.EmdStandalone, "0CC") });
        await InvalidAsync(LG with { Document = new ProductDocument(AncillaryDocumentType.EmdStandalone, "XEB") });
        await InvalidAsync(LG with { Document = new ProductDocument(AncillaryDocumentType.EmdStandalone, "XCL") });

        var charges = await _harness.DefineAsync(L with { Codes = new ProductCodes("C") });
        var other = await _harness.DefineAsync(LG with { Codes = new ProductCodes("Z") });

        Assert.Equal("C", (await _harness.GetProductAsync(charges.Id)).Codes.ServiceTypeCode);
        Assert.Equal("Z", (await _harness.GetProductAsync(other.Id)).Codes.ServiceTypeCode);
    }

    [Fact]
    public async Task P2_C06_LoungeIndustryCodeRequiresQuantityOneToOne()
    {
        await InvalidAsync(L with { Quantity = new ProductQuantity(AncillaryQuantityUnit.Each, 1, 2) });
        await BusinessAssert.ThrowsAsync(16105, 422, () => _harness.DefineAsync(L with { Document = new ProductDocument(AncillaryDocumentType.EmdAssociated, "0BX") }));

        var industry = await _harness.ArrangeProductAsync(L);
        var carrierDefined = await _harness.ArrangeProductAsync(LG);

        Assert.Equal((1, 1), Quantity(await _harness.GetProductAsync(industry.Id)));
        Assert.Equal((1, 2), Quantity(await _harness.GetProductAsync(carrierDefined.Id)));
        Assert.Equal([1, 5], (await _harness.GetProductAsync(carrierDefined.Id)).Lounge!.AirportIds);
    }

    [Fact]
    public async Task P2_C07_CarrierDefinedLoungeMayBeInterline()
    {
        var product = await _harness.ArrangeProductAsync(LG with { Terms = new ProductTerms(false, null, null, null, true) });

        Assert.True((await _harness.GetProductAsync(product.Id)).Terms.InterlineSettlementAllowed);
        Assert.Equal(AncillaryProductStatus.Active, product.Status);
    }

    [Fact]
    public async Task P2_C08_ChangeReplacesTheLoungeAndReviseCarriesIt()
    {
        var defined = await _harness.DefineAsync(L);

        await _harness.ChangeAsync(Phase1Commands.ChangeTo(defined.Id, LG with { Lounge = new ProductLounge([7, 3]) }));

        var changed = await _harness.GetProductAsync(defined.Id);

        Assert.Equal([7, 3], changed.Lounge!.AirportIds);
        Assert.Equal(("XLG", "E", "LG", 2), (changed.Document.Rfisc, changed.Document.Rfic, changed.Codes.GroupCode, changed.Quantity.Max));
        Assert.Equal(("LNGTHR", 1, "Draft"), (changed.ProductRef, changed.Version, changed.Status.Name));

        await _harness.ActivateProductAsync(defined.Id);

        var revised = await _harness.ReviseProductAsync(defined.Id);
        var draft = await _harness.GetProductAsync(revised.Id);
        var stored = await _harness.RunAsync(scope => scope.Command.AncillaryProducts
            .AsNoTracking()
            .Where(product => product.OwnerAirlineId == Airline)
            .OrderBy(product => product.Version)
            .ToListAsync());

        Assert.Equal((2, "Draft"), (draft.Version, draft.Status.Name));
        Assert.Equal([7, 3], draft.Lounge!.AirportIds);
        Assert.Null(draft.Baggage);
        Assert.Equal([[7, 3], [7, 3]], stored.Select(product => product.Lounge!.AirportIds));
    }

    [Fact]
    public async Task P2_C09_ExtraBagOnALoungeCodeIsStillRefused()
        => await InvalidAsync(X with { ProductRef = "XBAGL", Document = new ProductDocument(AncillaryDocumentType.EmdAssociated, "XLG") });

    private Task InvalidAsync(BackofficeDefineAncillaryProductCommand command)
        => BusinessAssert.ThrowsAsync(16106, 422, () => _harness.DefineAsync(command));

    private static (int Min, int Max) Quantity(BackofficeAncillaryProductDto product) => (product.Quantity.Min, product.Quantity.Max);

    private sealed class ProductComparer : IEqualityComparer<BackofficeAncillaryProductDto>
    {
        public static readonly ProductComparer Instance = new();

        public bool Equals(BackofficeAncillaryProductDto? left, BackofficeAncillaryProductDto? right)
            => left is not null
               && right is not null
               && left with { Lounge = null } == right with { Lounge = null }
               && (left.Lounge?.AirportIds ?? []).SequenceEqual(right.Lounge?.AirportIds ?? []);

        public int GetHashCode(BackofficeAncillaryProductDto product) => product.Id.GetHashCode();
    }
}
