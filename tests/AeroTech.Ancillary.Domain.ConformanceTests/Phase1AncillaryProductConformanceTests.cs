using AeroTech.Ancillary.Domain.AncillaryProductAggregate;
using AeroTech.Ancillary.Domain.ConformanceTests.Fixtures;
using AeroTech.Ancillary.Domain.ServiceSubCodeAggregate;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;

namespace AeroTech.Ancillary.Domain.ConformanceTests;

public sealed class Phase1AncillaryProductConformanceTests
{
    private static readonly DateTimeOffset CreatedAt = new(2026, 10, 1, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Later = CreatedAt.AddHours(5);

    private readonly ServiceSubCode _subCodeS = SubCodes.Industry();
    private readonly ServiceSubCode _subCodeG = SubCodes.CarrierDefined();

    [Fact]
    public void P1_C01_DefineCreatesADraftVersionOneWithTheCodesOfTheSubCode()
    {
        var product = ProductSpec.X.Define(7, _subCodeS, CreatedAt);

        Assert.Equal((7L, 10, "XBAG1", 1), (product.Id, product.OwnerAirlineId, product.ProductRef, product.Version));
        Assert.Equal(AncillaryProductStatus.Draft, product.Status);
        Assert.Equal(AncillaryProductType.ExtraBaggage, product.Type);
        Assert.Equal("First extra bag 23kg", product.Name);
        Assert.Null(product.Description);
        Assert.Equal(AncillarySalesScope.TravellerBound, product.SalesScope);
        Assert.Equal((AncillaryQuantityUnit.Piece, 1, 1), (product.Quantity.Unit, product.Quantity.Min, product.Quantity.Max));
        Assert.Equal((AncillaryDocumentType.EmdAssociated, "0CC", "C"), (product.Document.Type, product.Document.Rfisc, product.Document.Rfic));
        Assert.Equal(
            ("C", "BG", null, "B1", null),
            (product.Codes.ServiceTypeCode, product.Codes.GroupCode, product.Codes.SubGroupCode, product.Codes.Description1Code, product.Codes.Description2Code));
        Assert.Equal(
            (false, null, null, null, null),
            (product.Terms.Refundable, product.Terms.Commissionable, product.Terms.Reusable, product.Terms.FormOfRefundCode, product.Terms.InterlineSettlementAllowed));
        Assert.Equal(AncillaryInventoryControl.Unlimited, product.InventoryControl);
        Assert.Equal((1, 23m, AncillaryWeightUnit.Kg), (product.Baggage!.Pieces, product.Baggage.Weight, product.Baggage.WeightUnit));
        Assert.Equal(CreatedAt, product.CreatedAt);
        Assert.Null(product.ActivatedAt);
        Assert.Null(product.RetiredAt);
    }

    [Fact]
    public void P1_C04_ChangeReplacesTheMutableFieldsAndCopiesTheCodesAgain()
    {
        var product = ProductSpec.X.Define(7, _subCodeS, CreatedAt);
        var other = SubCodes.CarrierDefined("XBH", subGroupCode: "XS", description1Code: "D1", description2Code: "D2");
        var change = new ProductSpec
        {
            Name = "Heavy bag",
            Description = "Up to 32 kg",
            Min = 1,
            Max = 3,
            Rfisc = "XBH",
            ServiceTypeCode = "P",
            Refundable = true,
            Commissionable = true,
            Reusable = false,
            FormOfRefundCode = "CASH",
            InterlineSettlementAllowed = false,
            Pieces = null,
            Weight = 32.5m,
            WeightUnit = AncillaryWeightUnit.Lbs
        };

        change.Change(product, other);

        Assert.Equal((7L, 10, "XBAG1", 1), (product.Id, product.OwnerAirlineId, product.ProductRef, product.Version));
        Assert.Equal(AncillaryProductType.ExtraBaggage, product.Type);
        Assert.Equal(AncillaryProductStatus.Draft, product.Status);
        Assert.Equal(("Heavy bag", "Up to 32 kg"), (product.Name, product.Description));
        Assert.Equal((1, 3), (product.Quantity.Min, product.Quantity.Max));
        Assert.Equal(("XBH", "C"), (product.Document.Rfisc, product.Document.Rfic));
        Assert.Equal(
            ("P", "BG", "XS", "D1", "D2"),
            (product.Codes.ServiceTypeCode, product.Codes.GroupCode, product.Codes.SubGroupCode, product.Codes.Description1Code, product.Codes.Description2Code));
        Assert.Equal(
            (true, true, false, "CASH", false),
            (product.Terms.Refundable, product.Terms.Commissionable, product.Terms.Reusable, product.Terms.FormOfRefundCode, product.Terms.InterlineSettlementAllowed));
        Assert.Equal((null, 32.5m, AncillaryWeightUnit.Lbs), (product.Baggage!.Pieces, product.Baggage.Weight, product.Baggage.WeightUnit));
    }

    [Theory]
    [InlineData(AncillaryProductStatus.Active)]
    [InlineData(AncillaryProductStatus.Suspended)]
    [InlineData(AncillaryProductStatus.Retired)]
    public void P1_C05_ChangeOfAVersionThatLeftDraftIsRefused(AncillaryProductStatus status)
    {
        var product = ProductIn(status);

        BusinessAssert.Throws(16103, 409, () => (ProductSpec.X with { Name = "Other" }).Change(product, _subCodeS));
        Assert.Equal("First extra bag 23kg", product.Name);
    }

    [Fact]
    public void P1_C07_BaggageDetailIsRequiredAndConsistent()
    {
        BusinessAssert.Throws(16106, 422, () => Define(ProductSpec.X with { HasBaggage = false }));
        BusinessAssert.Throws(16106, 422, () => Define(ProductSpec.X with { Pieces = null, Weight = null, WeightUnit = null }));
        BusinessAssert.Throws(16106, 422, () => Define(ProductSpec.X with { WeightUnit = null }));
        BusinessAssert.Throws(16106, 422, () => Define(ProductSpec.X with { Weight = null }));

        Assert.NotNull(Define(ProductSpec.X with { Pieces = null }));
        Assert.NotNull(Define(ProductSpec.X with { Weight = null, WeightUnit = null }));
    }

    [Fact]
    public void P1_C08_MissingRfiscIsInvalid()
        => BusinessAssert.Throws(16106, 422, () => (ProductSpec.X with { Rfisc = null }).Define(7, null, CreatedAt));

    [Fact]
    public void P1_C09_SubCodeMustBeAnActiveSubCodeOfTheAirline()
    {
        var retired = SubCodes.Industry();
        retired.Retire();
        var draft = Define(ProductSpec.X);

        BusinessAssert.Throws(16109, 422, () => ProductSpec.X.Define(7, null, CreatedAt));
        BusinessAssert.Throws(16109, 422, () => ProductSpec.X.Define(7, retired, CreatedAt));
        BusinessAssert.Throws(16109, 422, () => ProductSpec.X.Define(7, SubCodes.Industry(ownerAirlineId: 11), CreatedAt));
        BusinessAssert.Throws(16109, 422, () => ProductSpec.X.Define(7, _subCodeG, CreatedAt));
        BusinessAssert.Throws(16109, 422, () => ProductSpec.X.Change(draft, null));
        BusinessAssert.Throws(16109, 422, () => ProductSpec.X.Change(draft, retired));
    }

    [Fact]
    public void P1_C10_ExtraBagNeedsTheBaggageClassification()
    {
        BusinessAssert.Throws(16106, 422, () => ProductSpec.G.Define(7, SubCodes.CarrierDefined(rfic: "E"), CreatedAt));
        BusinessAssert.Throws(16106, 422, () => ProductSpec.G.Define(7, SubCodes.CarrierDefined(groupCode: "LG"), CreatedAt));
        BusinessAssert.Throws(16106, 422, () => Define(ProductSpec.X with { ServiceTypeCode = "F" }));

        Assert.Equal("P", Define(ProductSpec.X with { ServiceTypeCode = "P" }).Codes.ServiceTypeCode);
    }

    [Theory]
    [InlineData(1, 2)]
    [InlineData(2, 2)]
    public void P1_C11_IndustryCodeRequiresQuantityOneToOne(int min, int max)
    {
        var draft = Define(ProductSpec.X);

        BusinessAssert.Throws(16106, 422, () => Define(ProductSpec.X with { Min = min, Max = max }));
        BusinessAssert.Throws(16106, 422, () => (ProductSpec.X with { Min = min, Max = max }).Change(draft, _subCodeS));

        draft.Activate(_subCodeS, Later);

        Assert.Equal((1, 1), (draft.Quantity.Min, draft.Quantity.Max));
    }

    [Fact]
    public void P1_C11_CarrierDefinedCodeAllowsSeveralUnitsAndStaysCarrierLocal()
    {
        var product = ProductSpec.G.Define(7, _subCodeG, CreatedAt);

        product.Activate(_subCodeG, Later);

        Assert.Equal((1, 2), (product.Quantity.Min, product.Quantity.Max));
        Assert.Equal(AncillaryProductStatus.Active, product.Status);
        Assert.Equal(ServiceSubCodeSource.CarrierDefined, _subCodeG.Source);
    }

    [Fact]
    public void P1_C12_CarrierDefinedBaggageIsNotInterline()
    {
        BusinessAssert.Throws(16106, 422, () => (ProductSpec.G with { InterlineSettlementAllowed = true }).Define(7, _subCodeG, CreatedAt));

        Assert.False((ProductSpec.G with { InterlineSettlementAllowed = false }).Define(7, _subCodeG, CreatedAt).Terms.InterlineSettlementAllowed);
        Assert.Null(ProductSpec.G.Define(7, _subCodeG, CreatedAt).Terms.InterlineSettlementAllowed);
        Assert.True(Define(ProductSpec.X with { InterlineSettlementAllowed = true }).Terms.InterlineSettlementAllowed);
    }

    [Theory]
    [InlineData(0, 2)]
    [InlineData(2, 1)]
    [InlineData(1, 100)]
    public void P1_C13_QuantityBoundsAreChecked(int min, int max)
        => BusinessAssert.Throws(16106, 422, () => (ProductSpec.G with { Min = min, Max = max }).Define(7, _subCodeG, CreatedAt));

    [Fact]
    public void P1_C14_ActivateADraftSetsTheActivationTime()
    {
        var product = Define(ProductSpec.X);

        product.Activate(_subCodeS, Later);

        Assert.Equal(AncillaryProductStatus.Active, product.Status);
        Assert.Equal(Later, product.ActivatedAt);
    }

    [Fact]
    public void P1_C15_SuspendThenActivateKeepsTheFirstActivationTime()
    {
        var product = ProductIn(AncillaryProductStatus.Active);

        product.Suspend();

        Assert.Equal(AncillaryProductStatus.Suspended, product.Status);

        product.Activate(_subCodeS, Later.AddDays(3));

        Assert.Equal(AncillaryProductStatus.Active, product.Status);
        Assert.Equal(Later, product.ActivatedAt);
    }

    [Fact]
    public void P1_C16_StatusChangesOutsideTheLifecycleAreRefused()
    {
        var retired = ProductIn(AncillaryProductStatus.Retired);

        BusinessAssert.Throws(16104, 409, () => ProductIn(AncillaryProductStatus.Draft).Suspend());
        BusinessAssert.Throws(16104, 409, () => ProductIn(AncillaryProductStatus.Active).Activate(_subCodeS, Later));
        BusinessAssert.Throws(16104, 409, () => retired.Activate(_subCodeS, Later));
        BusinessAssert.Throws(16104, 409, retired.Suspend);
        BusinessAssert.Throws(16104, 409, () => retired.Retire(Later));
    }

    [Theory]
    [InlineData(AncillaryProductStatus.Draft)]
    [InlineData(AncillaryProductStatus.Active)]
    [InlineData(AncillaryProductStatus.Suspended)]
    public void P1_C17_RetireIsFinalFromEveryOtherStatus(AncillaryProductStatus status)
    {
        var product = ProductIn(status);
        var retiredAt = Later.AddDays(9);

        product.Retire(retiredAt);

        Assert.Equal(AncillaryProductStatus.Retired, product.Status);
        Assert.Equal(retiredAt, product.RetiredAt);
    }

    [Fact]
    public void P1_C18_ReviseCopiesEveryFieldIntoANewDraft()
    {
        var source = ProductIn(AncillaryProductStatus.Active);
        var revisedAt = Later.AddDays(30);

        var draft = source.Revise(8, 2, revisedAt);

        Assert.Equal((8L, 10, "XBAG1", 2), (draft.Id, draft.OwnerAirlineId, draft.ProductRef, draft.Version));
        Assert.Equal(AncillaryProductStatus.Draft, draft.Status);
        Assert.Equal(revisedAt, draft.CreatedAt);
        Assert.Null(draft.ActivatedAt);
        Assert.Null(draft.RetiredAt);
        Assert.Equal(
            (source.Type, source.Name, source.Description, source.SalesScope, source.InventoryControl),
            (draft.Type, draft.Name, draft.Description, draft.SalesScope, draft.InventoryControl));
        Assert.Equal(source.Quantity, draft.Quantity);
        Assert.Equal(source.Document, draft.Document);
        Assert.Equal(source.Codes, draft.Codes);
        Assert.Equal(source.Terms, draft.Terms);
        Assert.Equal(source.Baggage, draft.Baggage);
        Assert.NotSame(source.Quantity, draft.Quantity);
        Assert.NotSame(source.Document, draft.Document);
        Assert.NotSame(source.Codes, draft.Codes);
        Assert.NotSame(source.Terms, draft.Terms);
        Assert.NotSame(source.Baggage, draft.Baggage);
        Assert.Equal((AncillaryProductStatus.Active, 1), (source.Status, source.Version));
        Assert.Equal(Later, source.ActivatedAt);
    }

    [Theory]
    [InlineData(AncillaryProductStatus.Draft)]
    [InlineData(AncillaryProductStatus.Retired)]
    public void P1_C19_ReviseOfADraftOrRetiredVersionIsRefused(AncillaryProductStatus status)
        => BusinessAssert.Throws(16108, 409, () => ProductIn(status).Revise(8, 2, Later));

    [Fact]
    public void P1_C24_ActivationNeedsTheSubCodeToBeActive()
    {
        var product = Define(ProductSpec.X);

        _subCodeS.Retire();

        BusinessAssert.Throws(16109, 422, () => product.Activate(_subCodeS, Later));
        BusinessAssert.Throws(16109, 422, () => product.Activate(null, Later));
        Assert.Equal(AncillaryProductStatus.Draft, product.Status);

        _subCodeS.Reactivate();
        product.Activate(_subCodeS, Later);

        Assert.Equal(AncillaryProductStatus.Active, product.Status);
    }

    [Fact]
    public void P1_C25_SuspendedVersionCannotBeActivatedWhileItsSubCodeIsRetired()
    {
        var product = ProductIn(AncillaryProductStatus.Suspended);

        _subCodeS.Retire();

        BusinessAssert.Throws(16109, 422, () => product.Activate(_subCodeS, Later));
        Assert.Equal(AncillaryProductStatus.Suspended, product.Status);

        _subCodeS.Reactivate();
        product.Activate(_subCodeS, Later);

        Assert.Equal(AncillaryProductStatus.Active, product.Status);
    }

    private AncillaryProduct Define(ProductSpec spec) => spec.Define(7, _subCodeS, CreatedAt);

    private AncillaryProduct ProductIn(AncillaryProductStatus status)
    {
        var product = Define(ProductSpec.X);

        if (status is AncillaryProductStatus.Active or AncillaryProductStatus.Suspended)
            product.Activate(_subCodeS, Later);

        if (status == AncillaryProductStatus.Suspended)
            product.Suspend();

        if (status == AncillaryProductStatus.Retired)
            product.Retire(Later);

        return product;
    }
}
