using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.ChangeAncillaryProduct.Backoffice;
using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.DefineAncillaryProduct;
using AeroTech.Ancillary.Application.AncillaryProductAggregate.Commands.DefineAncillaryProduct.Backoffice;
using AeroTech.Ancillary.Query.AncillaryProductAggregate.Dto;
using AeroTech.Ancillary.Query.AncillaryProductAggregate.Queries.GetAncillaryProductsPaginated.Backoffice;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;

namespace AeroTech.Ancillary.Application.AcceptanceTests.AncillaryProducts;

[Collection(DatabaseCollection.Name)]
public sealed class Phase1AncillaryProductAcceptanceTests(TestDatabase database) : IAsyncLifetime
{
    private readonly AncillaryHarness _harness = new(database);
    private readonly BackofficeDefineAncillaryProductCommandValidator _defineValidator = new();
    private readonly BackofficeChangeAncillaryProductCommandValidator _changeValidator = new();

    private long _subCodeS;
    private long _subCodeG;

    private int Airline => _harness.AirlineId;

    private BackofficeDefineAncillaryProductCommand X => Phase1Commands.ProductX(Airline);

    private BackofficeDefineAncillaryProductCommand G => Phase1Commands.ProductG(Airline);

    public async Task InitializeAsync()
    {
        _subCodeS = (await _harness.RegisterAsync(Phase1Commands.SubCodeS(Airline))).Id;
        _subCodeG = (await _harness.RegisterAsync(Phase1Commands.SubCodeG(Airline))).Id;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task P1_C01_ProductIsDefinedAsDraftVersionOneWithTheCodesOfItsSubCode()
    {
        var result = await _harness.DefineAsync(X);

        Assert.Equal((Airline, "XBAG1", 1, AncillaryProductStatus.Draft), (result.OwnerAirlineId, result.ProductRef, result.Version, result.Status));

        var product = await _harness.GetProductAsync(result.Id);

        Assert.Equal((result.Id, Airline, "XBAG1", 1, "ExtraBaggage", "Draft"), (product.Id, product.OwnerAirlineId, product.ProductRef, product.Version, product.Type.Name, product.Status.Name));
        Assert.Equal(("First extra bag 23kg", null, "TravellerBound", "Unlimited"), (product.Name, product.Description, product.SalesScope.Name, product.InventoryControl.Name));
        Assert.Equal(("Piece", 1, 1), (product.Quantity.Unit.Name, product.Quantity.Min, product.Quantity.Max));
        Assert.Equal(("EmdAssociated", "0CC", "C"), (product.Document.Type.Name, product.Document.Rfisc, product.Document.Rfic));
        Assert.Equal(new ProductCodesDto("C", "BG", null, "B1", null), product.Codes);
        Assert.Equal(new ProductTermsDto(false, null, null, null, null), product.Terms);
        Assert.Equal((1, 23m, "Kg"), (product.Baggage!.Pieces, product.Baggage.Weight, product.Baggage.WeightUnit!.Name));
        Assert.Equal((_harness.Clock.Now, null, null), (product.CreatedAt, product.ActivatedAt, product.RetiredAt));
    }

    [Fact]
    public async Task P1_C02_ProductReferenceIsUsedOnlyOncePerAirline()
    {
        await _harness.DefineAsync(X);

        await BusinessAssert.ThrowsAsync(16102, 409, () => _harness.DefineAsync(X with { Name = "Second" }));

        var otherAirline = database.NextAirlineId();

        await _harness.RegisterAsync(Phase1Commands.SubCodeS(otherAirline));

        Assert.Equal(AncillaryProductStatus.Draft, (await _harness.DefineAsync(Phase1Commands.ProductX(otherAirline))).Status);
    }

    [Theory]
    [InlineData("xbag")]
    [InlineData("X")]
    [InlineData("X-BAG")]
    [InlineData("ABCDEFGHIJKLMNOPQRSTU")]
    public void P1_C03_MalformedProductRefIsRejected(string productRef)
    {
        ValidationAssert.Accepts(_defineValidator, X with { ProductRef = "ABCDEFGHIJKLMNOPQRST" });
        ValidationAssert.Accepts(_defineValidator, X with { ProductRef = "X1" });
        ValidationAssert.Rejects(_defineValidator, X with { ProductRef = productRef });
    }

    [Fact]
    public async Task P1_C04_ChangeReplacesTheMutableFieldsOfADraftAndCopiesTheCodesAgain()
    {
        var defined = await _harness.DefineAsync(X);

        await _harness.RegisterAsync(Phase1Commands.SubCodeG(Airline) with { Code = "XBH", SubGroupCode = "XS", Description1Code = "D1", Description2Code = "D2" });

        var content = X with
        {
            Name = "Heavy bag",
            Description = "Up to 32 kg",
            Quantity = new ProductQuantity(AncillaryQuantityUnit.Piece, 1, 3),
            Document = new ProductDocument(AncillaryDocumentType.EmdAssociated, "XBH"),
            Codes = new ProductCodes("P"),
            Terms = new ProductTerms(true, true, false, "CASH", false),
            Baggage = new ProductBaggage(null, 32.5m, AncillaryWeightUnit.Lbs)
        };

        var result = await _harness.ChangeAsync(Phase1Commands.ChangeTo(defined.Id, content));
        var product = await _harness.GetProductAsync(defined.Id);

        Assert.Equal((defined.Id, Airline, "XBAG1", 1, AncillaryProductStatus.Draft), (result.Id, result.OwnerAirlineId, result.ProductRef, result.Version, result.Status));
        Assert.Equal((Airline, "XBAG1", 1, "ExtraBaggage", "Draft"), (product.OwnerAirlineId, product.ProductRef, product.Version, product.Type.Name, product.Status.Name));
        Assert.Equal(("Heavy bag", "Up to 32 kg"), (product.Name, product.Description));
        Assert.Equal((1, 3), (product.Quantity.Min, product.Quantity.Max));
        Assert.Equal(("XBH", "C"), (product.Document.Rfisc, product.Document.Rfic));
        Assert.Equal(new ProductCodesDto("P", "BG", "XS", "D1", "D2"), product.Codes);
        Assert.Equal(new ProductTermsDto(true, true, false, "CASH", false), product.Terms);
        Assert.Equal((null, 32.5m, "Lbs"), (product.Baggage!.Pieces, product.Baggage.Weight, product.Baggage.WeightUnit!.Name));
    }

    [Theory]
    [InlineData(AncillaryProductStatus.Active)]
    [InlineData(AncillaryProductStatus.Suspended)]
    [InlineData(AncillaryProductStatus.Retired)]
    public async Task P1_C05_VersionThatLeftDraftCannotBeChanged(AncillaryProductStatus status)
    {
        var id = await ProductXInAsync(status);
        var before = await _harness.GetProductAsync(id);

        await BusinessAssert.ThrowsAsync(16103, 409, () => _harness.ChangeAsync(Phase1Commands.ChangeTo(id, X with { Name = "Other" })));

        Assert.Equal(before, await _harness.GetProductAsync(id));
    }

    [Fact]
    public async Task P1_C07_BaggageDetailIsRequiredAndConsistent()
    {
        await InvalidAsync(X with { Baggage = null });
        await InvalidAsync(X with { Baggage = new ProductBaggage(null, null, null) });
        await InvalidAsync(X with { Baggage = new ProductBaggage(1, 23m, null) });
        await InvalidAsync(X with { Baggage = new ProductBaggage(1, null, AncillaryWeightUnit.Kg) });

        var draft = await _harness.DefineAsync(X);

        await BusinessAssert.ThrowsAsync(16106, 422, () => _harness.ChangeAsync(Phase1Commands.ChangeTo(draft.Id, X with { Baggage = null })));
    }

    [Fact]
    public async Task P1_C08_RfiscAndServiceTypeCodeAreRequired()
    {
        ValidationAssert.Accepts(_defineValidator, X with { Document = new ProductDocument(AncillaryDocumentType.EmdAssociated, null) });
        await InvalidAsync(X with { Document = new ProductDocument(AncillaryDocumentType.EmdAssociated, null) });

        ValidationAssert.Rejects(_defineValidator, X with { Document = new ProductDocument(AncillaryDocumentType.EmdAssociated, "0c") });
        ValidationAssert.Rejects(_defineValidator, X with { Codes = new ProductCodes(null!) });
        ValidationAssert.Rejects(_defineValidator, X with { Codes = null! });
        ValidationAssert.Rejects(_changeValidator, Phase1Commands.ChangeTo(1, X with { Document = new ProductDocument(AncillaryDocumentType.EmdAssociated, "0c") }));
        ValidationAssert.Rejects(_changeValidator, Phase1Commands.ChangeTo(1, X with { Codes = new ProductCodes(null!) }));
        ValidationAssert.Accepts(_changeValidator, Phase1Commands.ChangeTo(1, X));
    }

    [Fact]
    public async Task P1_C09_SubCodeMustBeEnabledAndActiveForTheAirline()
    {
        var notEnabled = X with { Document = new ProductDocument(AncillaryDocumentType.EmdAssociated, "0ZZ") };

        await BusinessAssert.ThrowsAsync(16109, 422, () => _harness.DefineAsync(notEnabled));

        var draft = await _harness.DefineAsync(X);

        await BusinessAssert.ThrowsAsync(16109, 422, () => _harness.ChangeAsync(Phase1Commands.ChangeTo(draft.Id, notEnabled)));

        await _harness.RetireSubCodeAsync(_subCodeG);

        await BusinessAssert.ThrowsAsync(16109, 422, () => _harness.DefineAsync(G));
        await BusinessAssert.ThrowsAsync(16109, 422, () => _harness.ChangeAsync(Phase1Commands.ChangeTo(draft.Id, G)));

        Assert.Equal("0CC", (await _harness.GetProductAsync(draft.Id)).Document.Rfisc);
    }

    [Fact]
    public async Task P1_C10_ExtraBagNeedsTheBaggageClassification()
    {
        await _harness.RegisterAsync(Phase1Commands.SubCodeG(Airline) with { Code = "XAE", Rfic = "E" });
        await _harness.RegisterAsync(Phase1Commands.SubCodeG(Airline) with { Code = "XLG", GroupCode = "LG" });

        await InvalidAsync(G with { Document = new ProductDocument(AncillaryDocumentType.EmdAssociated, "XAE") });
        await InvalidAsync(G with { Document = new ProductDocument(AncillaryDocumentType.EmdAssociated, "XLG") });
        await InvalidAsync(X with { Codes = new ProductCodes("F") });

        var prepaid = await _harness.DefineAsync(X with { Codes = new ProductCodes("P") });

        Assert.Equal("P", (await _harness.GetProductAsync(prepaid.Id)).Codes.ServiceTypeCode);
    }

    [Theory]
    [InlineData(1, 2)]
    [InlineData(2, 2)]
    public async Task P1_C11_IndustryCodeRequiresQuantityOneToOne(int min, int max)
    {
        var quantity = new ProductQuantity(AncillaryQuantityUnit.Piece, min, max);

        await InvalidAsync(X with { Quantity = quantity });

        var draft = await _harness.DefineAsync(X);

        await BusinessAssert.ThrowsAsync(16106, 422, () => _harness.ChangeAsync(Phase1Commands.ChangeTo(draft.Id, X with { Quantity = quantity })));
        Assert.Equal(AncillaryProductStatus.Active, (await _harness.ActivateProductAsync(draft.Id)).Status);
        Assert.Equal((1, 1), Quantity(await _harness.GetProductAsync(draft.Id)));
    }

    [Fact]
    public async Task P1_C11_CarrierDefinedCodeAllowsSeveralUnitsAndStaysCarrierLocal()
    {
        var generic = await _harness.ArrangeProductAsync(G);

        Assert.Equal((1, 2), Quantity(await _harness.GetProductAsync(generic.Id)));
        Assert.Equal("CarrierDefined", (await _harness.GetSubCodeAsync(_subCodeG)).Source.Name);
    }

    [Fact]
    public async Task P1_C12_CarrierDefinedBaggageIsNotInterline()
    {
        await InvalidAsync(G with { Terms = new ProductTerms(false, null, null, null, true) });

        var notInterline = await _harness.DefineAsync(G with { Terms = new ProductTerms(false, null, null, null, false) });
        var undefined = await _harness.DefineAsync(G with { ProductRef = "XBAGH" });
        var industry = await _harness.DefineAsync(X with { Terms = new ProductTerms(false, null, null, null, true) });

        Assert.False((await _harness.GetProductAsync(notInterline.Id)).Terms.InterlineSettlementAllowed);
        Assert.Null((await _harness.GetProductAsync(undefined.Id)).Terms.InterlineSettlementAllowed);
        Assert.True((await _harness.GetProductAsync(industry.Id)).Terms.InterlineSettlementAllowed);
    }

    [Theory]
    [InlineData(0, 2)]
    [InlineData(2, 1)]
    [InlineData(1, 100)]
    public async Task P1_C13_QuantityBoundsAreChecked(int min, int max)
    {
        var command = G with { Quantity = new ProductQuantity(AncillaryQuantityUnit.Piece, min, max) };

        ValidationAssert.Accepts(_defineValidator, command);
        await InvalidAsync(command);
    }

    [Fact]
    public async Task P1_C14_ActivateADraftSetsTheActivationTimeFromTheClock()
    {
        var draft = await _harness.DefineAsync(X);
        var definedAt = _harness.Clock.Now;

        _harness.Clock.Now = definedAt.AddHours(6);

        var result = await _harness.ActivateProductAsync(draft.Id);
        var product = await _harness.GetProductAsync(draft.Id);

        Assert.Equal((AncillaryProductStatus.Active, null), (result.Status, result.RetiredVersionId));
        Assert.Equal(("Active", definedAt, definedAt.AddHours(6)), (product.Status.Name, product.CreatedAt, product.ActivatedAt));
    }

    [Fact]
    public async Task P1_C15_SuspendThenActivateKeepsTheFirstActivationTime()
    {
        var product = await _harness.ArrangeProductAsync(X);
        var activatedAt = _harness.Clock.Now;

        Assert.Equal(AncillaryProductStatus.Suspended, (await _harness.SuspendProductAsync(product.Id)).Status);
        Assert.Equal("Suspended", (await _harness.GetProductAsync(product.Id)).Status.Name);

        _harness.Clock.Now = activatedAt.AddDays(2);

        Assert.Equal(AncillaryProductStatus.Active, (await _harness.ActivateProductAsync(product.Id)).Status);
        Assert.Equal(("Active", activatedAt), ((await _harness.GetProductAsync(product.Id)).Status.Name, (await _harness.GetProductAsync(product.Id)).ActivatedAt));
    }

    [Fact]
    public async Task P1_C16_StatusChangesOutsideTheLifecycleAreRefused()
    {
        var draft = await _harness.DefineAsync(X);

        await BusinessAssert.ThrowsAsync(16104, 409, () => _harness.SuspendProductAsync(draft.Id));

        await _harness.ActivateProductAsync(draft.Id);
        await BusinessAssert.ThrowsAsync(16104, 409, () => _harness.ActivateProductAsync(draft.Id));

        await _harness.RetireProductAsync(draft.Id);
        await BusinessAssert.ThrowsAsync(16104, 409, () => _harness.ActivateProductAsync(draft.Id));
        await BusinessAssert.ThrowsAsync(16104, 409, () => _harness.SuspendProductAsync(draft.Id));
        await BusinessAssert.ThrowsAsync(16104, 409, () => _harness.RetireProductAsync(draft.Id));
    }

    [Theory]
    [InlineData(AncillaryProductStatus.Draft)]
    [InlineData(AncillaryProductStatus.Active)]
    [InlineData(AncillaryProductStatus.Suspended)]
    public async Task P1_C17_RetireIsFinalFromEveryOtherStatus(AncillaryProductStatus status)
    {
        var id = await ProductXInAsync(status);

        _harness.Clock.Now = _harness.Clock.Now.AddDays(5);

        var result = await _harness.RetireProductAsync(id);
        var product = await _harness.GetProductAsync(id);

        Assert.Equal(AncillaryProductStatus.Retired, result.Status);
        Assert.Equal(("Retired", _harness.Clock.Now), (product.Status.Name, product.RetiredAt));
    }

    [Fact]
    public async Task P1_C18_ReviseCreatesANewDraftWithIdenticalFieldValues()
    {
        var first = await _harness.ArrangeProductAsync(X with { Description = "One bag", Terms = new ProductTerms(true, false, true, "CASH", true) });
        var source = await _harness.GetProductAsync(first.Id);

        _harness.Clock.Now = _harness.Clock.Now.AddDays(30);

        var result = await _harness.ReviseProductAsync(first.Id);
        var draft = await _harness.GetProductAsync(result.Id);

        Assert.NotEqual(first.Id, result.Id);
        Assert.Equal((Airline, "XBAG1", 2, AncillaryProductStatus.Draft), (result.OwnerAirlineId, result.ProductRef, result.Version, result.Status));
        Assert.Equal("Draft", draft.Status.Name);
        Assert.Equal(source with { Id = result.Id, Version = 2, Status = draft.Status, CreatedAt = _harness.Clock.Now, ActivatedAt = null }, draft);
        Assert.Equal(source, await _harness.GetProductAsync(first.Id));
    }

    [Fact]
    public async Task P1_C19_OnlyOneDraftAndOnlyPublishedVersionsCanBeRevised()
    {
        var first = await _harness.ArrangeProductAsync(X);
        var draft = await _harness.ReviseProductAsync(first.Id);

        await BusinessAssert.ThrowsAsync(16107, 409, () => _harness.ReviseProductAsync(first.Id));
        await BusinessAssert.ThrowsAsync(16108, 409, () => _harness.ReviseProductAsync(draft.Id));

        await _harness.RetireProductAsync(draft.Id);
        await BusinessAssert.ThrowsAsync(16108, 409, () => _harness.ReviseProductAsync(draft.Id));

        await _harness.SuspendProductAsync(first.Id);

        Assert.Equal(3, (await _harness.ReviseProductAsync(first.Id)).Version);
    }

    [Fact]
    public async Task P1_C21_RetiredVersionStaysReadableAndIsListedAfterTheNewOne()
    {
        var first = await _harness.ArrangeProductAsync(X);
        var before = await _harness.GetProductAsync(first.Id);
        var second = await _harness.ReviseProductAsync(first.Id);

        _harness.Clock.Now = _harness.Clock.Now.AddDays(1);
        await _harness.ActivateProductAsync(second.Id);

        var retired = await _harness.GetProductAsync(first.Id);
        var page = await _harness.RunAsync(scope => scope.GetProductsPaginated.ExecuteAsync(
            new BackofficeGetAncillaryProductsPaginatedQuery { OwnerAirlineId = Airline, ProductRef = "XBAG1" }));

        Assert.Equal(before with { Status = retired.Status, RetiredAt = _harness.Clock.Now }, retired);
        Assert.Equal("Retired", retired.Status.Name);
        Assert.Equal(2, page.TotalCount);
        Assert.Equal([(second.Id.ToString(), 2, "Active"), (first.Id.ToString(), 1, "Retired")], page.Results.Select(row => (row.Id, row.Version, row.Status.Name)));
        Assert.Equal(retired.Codes, page.Results.Last().Codes);
    }

    [Fact]
    public async Task P1_C23_OperationOnAnUnknownIdIsNotFound()
    {
        var unknown = database.Ids.NewId();

        await BusinessAssert.ThrowsAsync(16101, 404, () => _harness.ChangeAsync(Phase1Commands.ChangeTo(unknown, X)));
        await BusinessAssert.ThrowsAsync(16101, 404, () => _harness.ActivateProductAsync(unknown));
        await BusinessAssert.ThrowsAsync(16101, 404, () => _harness.SuspendProductAsync(unknown));
        await BusinessAssert.ThrowsAsync(16101, 404, () => _harness.RetireProductAsync(unknown));
        await BusinessAssert.ThrowsAsync(16101, 404, () => _harness.ReviseProductAsync(unknown));
        await BusinessAssert.ThrowsAsync(16101, 404, () => _harness.GetProductAsync(unknown));
    }

    [Fact]
    public async Task P1_C24_DraftCannotBeActivatedWhileItsSubCodeIsRetired()
    {
        var draft = await _harness.DefineAsync(X);

        await _harness.RetireSubCodeAsync(_subCodeS);

        await BusinessAssert.ThrowsAsync(16109, 422, () => _harness.ActivateProductAsync(draft.Id));
        Assert.Equal("Draft", (await _harness.GetProductAsync(draft.Id)).Status.Name);

        await _harness.ReactivateSubCodeAsync(_subCodeS);

        Assert.Equal(AncillaryProductStatus.Active, (await _harness.ActivateProductAsync(draft.Id)).Status);
    }

    [Fact]
    public async Task P1_C25_RetiringASubCodeLeavesAnActiveProductOfferedAndBlocksReactivation()
    {
        var product = await _harness.ArrangeProductAsync(X);
        var rule = await _harness.ArrangePriceRuleAsync(Phase1Commands.RuleR(Airline));
        var before = await _harness.GetProductAsync(product.Id);

        await _harness.RetireSubCodeAsync(_subCodeS);

        var item = Assert.Single((await _harness.QuoteAsync(Phase1Commands.Golden(Airline))).Items);

        Assert.Equal(("XBAG1", 1, rule.Id, "0CC"), (item.ProductRef, item.ProductVersion, item.PriceRuleId, item.Document.Rfisc));
        Assert.Equal(before, await _harness.GetProductAsync(product.Id));

        await _harness.SuspendProductAsync(product.Id);

        await BusinessAssert.ThrowsAsync(16109, 422, () => _harness.ActivateProductAsync(product.Id));
        Assert.Equal("Suspended", (await _harness.GetProductAsync(product.Id)).Status.Name);

        await _harness.ReactivateSubCodeAsync(_subCodeS);

        Assert.Equal(AncillaryProductStatus.Active, (await _harness.ActivateProductAsync(product.Id)).Status);
    }

    private Task InvalidAsync(BackofficeDefineAncillaryProductCommand command)
        => BusinessAssert.ThrowsAsync(16106, 422, () => _harness.DefineAsync(command));

    private async Task<long> ProductXInAsync(AncillaryProductStatus status)
    {
        var product = await _harness.DefineAsync(X);

        if (status is AncillaryProductStatus.Active or AncillaryProductStatus.Suspended)
            await _harness.ActivateProductAsync(product.Id);

        if (status == AncillaryProductStatus.Suspended)
            await _harness.SuspendProductAsync(product.Id);

        if (status == AncillaryProductStatus.Retired)
            await _harness.RetireProductAsync(product.Id);

        return product.Id;
    }

    private static (int Min, int Max) Quantity(BackofficeAncillaryProductDto product) => (product.Quantity.Min, product.Quantity.Max);
}
