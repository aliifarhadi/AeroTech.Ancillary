using AeroTech.Ancillary.Domain.AncillaryProductAggregate;
using AeroTech.Ancillary.Domain.ConformanceTests.Fixtures;
using AeroTech.Ancillary.Domain.ServiceSubCodeAggregate;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;

namespace AeroTech.Ancillary.Domain.ConformanceTests;

public sealed class Phase2AncillaryProductConformanceTests
{
    private static readonly DateTimeOffset CreatedAt = new(2026, 10, 1, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Later = CreatedAt.AddHours(5);

    private readonly ServiceSubCode _subCodeS = SubCodes.Industry();
    private readonly ServiceSubCode _subCodeL = SubCodes.Industry("0BX");
    private readonly ServiceSubCode _subCodeGL = SubCodes.CarrierDefinedLounge();

    [Fact]
    public void P2_C01_LoungeProductCarriesItsLoungeAndTheCodesOfItsSubCode()
    {
        var lounge = ProductSpec.L.Define(7, _subCodeL, CreatedAt);
        var bag = ProductSpec.X.Define(8, _subCodeS, CreatedAt);

        lounge.Activate(_subCodeL, Later);

        Assert.Equal((AncillaryProductType.LoungeAccess, AncillarySalesScope.TravellerSegment, AncillaryProductStatus.Active), (lounge.Type, lounge.SalesScope, lounge.Status));
        Assert.Equal((AncillaryQuantityUnit.Each, 1, 1), (lounge.Quantity.Unit, lounge.Quantity.Min, lounge.Quantity.Max));
        Assert.Equal((AncillaryDocumentType.EmdStandalone, "0BX", "E"), (lounge.Document.Type, lounge.Document.Rfisc, lounge.Document.Rfic));
        Assert.Equal(
            ("F", "LG", null, null, null),
            (lounge.Codes.ServiceTypeCode, lounge.Codes.GroupCode, lounge.Codes.SubGroupCode, lounge.Codes.Description1Code, lounge.Codes.Description2Code));
        Assert.Equal([1], lounge.Lounge!.AirportIds);
        Assert.Null(lounge.Baggage);
        Assert.Null(bag.Lounge);
        Assert.NotNull(bag.Baggage);
    }

    [Theory]
    [InlineData(AncillaryProductType.LoungeAccess, AncillarySalesScope.TravellerBound, AncillaryDocumentType.EmdStandalone, AncillaryQuantityUnit.Each)]
    [InlineData(AncillaryProductType.LoungeAccess, AncillarySalesScope.TravellerSegment, AncillaryDocumentType.EmdAssociated, AncillaryQuantityUnit.Each)]
    [InlineData(AncillaryProductType.LoungeAccess, AncillarySalesScope.TravellerSegment, AncillaryDocumentType.EmdStandalone, AncillaryQuantityUnit.Piece)]
    [InlineData(AncillaryProductType.ExtraBaggage, AncillarySalesScope.TravellerSegment, AncillaryDocumentType.EmdAssociated, AncillaryQuantityUnit.Piece)]
    [InlineData(AncillaryProductType.ExtraBaggage, AncillarySalesScope.TravellerBound, AncillaryDocumentType.EmdStandalone, AncillaryQuantityUnit.Piece)]
    [InlineData(AncillaryProductType.ExtraBaggage, AncillarySalesScope.TravellerBound, AncillaryDocumentType.EmdAssociated, AncillaryQuantityUnit.Each)]
    public void P2_C02_CombinationThatIsNotSoldIsRefused(
        AncillaryProductType type,
        AncillarySalesScope salesScope,
        AncillaryDocumentType documentType,
        AncillaryQuantityUnit unit)
    {
        var (valid, subCode) = type == AncillaryProductType.LoungeAccess ? (ProductSpec.L, _subCodeL) : (ProductSpec.X, _subCodeS);
        var unsold = valid with { SalesScope = salesScope, DocumentType = documentType, Unit = unit };
        var draft = valid.Define(7, subCode, CreatedAt);

        BusinessAssert.Throws(16105, 422, () => unsold.Define(8, subCode, CreatedAt));
        BusinessAssert.Throws(16105, 422, () => unsold.Change(draft, subCode));
        Assert.Equal((valid.SalesScope, valid.DocumentType, valid.Unit), (draft.SalesScope, draft.Document.Type, draft.Quantity.Unit));
    }

    [Fact]
    public void P2_C03_CombinationIsCheckedBeforeEveryOtherProductRule()
    {
        var broken = ProductSpec.L with { SalesScope = AncillarySalesScope.TravellerBound, LoungeAirportIds = null, Rfisc = "0ZZ", Name = "" };

        BusinessAssert.Throws(16105, 422, () => broken.Define(7, null, CreatedAt));
        BusinessAssert.Throws(16105, 422, () => (broken with { Rfisc = null }).Define(7, null, CreatedAt));
        BusinessAssert.Throws(16105, 422, () => (broken with { OwnerAirlineId = 0, ProductRef = "x" }).Define(7, null, CreatedAt));
        BusinessAssert.Throws(16105, 422, () => broken.Change(ProductSpec.L.Define(7, _subCodeL, CreatedAt), null));
        BusinessAssert.Throws(16106, 422, () => (broken with { SalesScope = AncillarySalesScope.TravellerSegment }).Define(7, null, CreatedAt));
        BusinessAssert.Throws(16109, 422, () => (ProductSpec.L with { Rfisc = "0ZZ" }).Define(7, null, CreatedAt));
    }

    [Fact]
    public void P2_C04_LoungeDetailIsRequiredAndConsistent()
    {
        BusinessAssert.Throws(16106, 422, () => DefineLounge(ProductSpec.L with { LoungeAirportIds = null }));
        BusinessAssert.Throws(16106, 422, () => DefineLounge(ProductSpec.L with { LoungeAirportIds = [] }));
        BusinessAssert.Throws(16106, 422, () => DefineLounge(ProductSpec.L with { LoungeAirportIds = [1, 5, 1] }));
        BusinessAssert.Throws(16106, 422, () => DefineLounge(ProductSpec.L with { LoungeAirportIds = [1, 0] }));
        BusinessAssert.Throws(16106, 422, () => DefineLounge(ProductSpec.L with { HasBaggage = true }));
        BusinessAssert.Throws(16106, 422, () => (ProductSpec.X with { LoungeAirportIds = [1] }).Define(7, _subCodeS, CreatedAt));

        Assert.Equal([5, 1, 9], DefineLounge(ProductSpec.L with { LoungeAirportIds = [5, 1, 9] }).Lounge!.AirportIds);
    }

    [Fact]
    public void P2_C05_LoungeNeedsTheLoungeClassification()
    {
        BusinessAssert.Throws(16106, 422, () => (ProductSpec.L with { Rfisc = "0CC" }).Define(7, _subCodeS, CreatedAt));
        BusinessAssert.Throws(16106, 422, () => ProductSpec.LG.Define(7, SubCodes.CarrierDefined(SubCodes.LoungeCode, rfic: "E", groupCode: "BG"), CreatedAt));
        BusinessAssert.Throws(16106, 422, () => ProductSpec.LG.Define(7, SubCodes.CarrierDefined(SubCodes.LoungeCode, rfic: "C", groupCode: "LG"), CreatedAt));

        foreach (var serviceTypeCode in new[] { "A", "C", "F", "P", "Z" })
            Assert.Equal(serviceTypeCode, DefineLounge(ProductSpec.L with { ServiceTypeCode = serviceTypeCode }).Codes.ServiceTypeCode);
    }

    [Fact]
    public void P2_C06_LoungeIndustryCodeRequiresQuantityOneToOne()
    {
        BusinessAssert.Throws(16106, 422, () => DefineLounge(ProductSpec.L with { Max = 2 }));
        BusinessAssert.Throws(16105, 422, () => DefineLounge(ProductSpec.L with { DocumentType = AncillaryDocumentType.EmdAssociated }));

        var industry = DefineLounge(ProductSpec.L);
        var carrierDefined = ProductSpec.LG.Define(8, _subCodeGL, CreatedAt);

        carrierDefined.Activate(_subCodeGL, Later);

        Assert.Equal((1, 1), (industry.Quantity.Min, industry.Quantity.Max));
        Assert.Equal((1, 2, AncillaryProductStatus.Active), (carrierDefined.Quantity.Min, carrierDefined.Quantity.Max, carrierDefined.Status));
        Assert.Equal([1, 5], carrierDefined.Lounge!.AirportIds);
    }

    [Fact]
    public void P2_C07_CarrierDefinedLoungeMayBeInterline()
    {
        var product = (ProductSpec.LG with { InterlineSettlementAllowed = true }).Define(7, _subCodeGL, CreatedAt);

        product.Activate(_subCodeGL, Later);

        Assert.True(product.Terms.InterlineSettlementAllowed);
        Assert.Equal(AncillaryProductStatus.Active, product.Status);
    }

    [Fact]
    public void P2_C08_ChangeReplacesTheLoungeAndReviseCopiesIt()
    {
        var other = SubCodes.CarrierDefined("XLH", rfic: "E", groupCode: "LG", subGroupCode: "LS", description1Code: "L1", commercialName: "LOUNGE H");
        var product = DefineLounge(ProductSpec.L);

        (ProductSpec.L with { Rfisc = "XLH", Max = 3, LoungeAirportIds = [7, 3] }).Change(product, other);

        Assert.Equal([7, 3], product.Lounge!.AirportIds);
        Assert.Equal(("XLH", "E"), (product.Document.Rfisc, product.Document.Rfic));
        Assert.Equal(("LG", "LS", "L1", null), (product.Codes.GroupCode, product.Codes.SubGroupCode, product.Codes.Description1Code, product.Codes.Description2Code));

        product.Activate(other, Later);

        var draft = product.Revise(8, 2, Later.AddDays(1));

        Assert.Equal(AncillaryProductStatus.Draft, draft.Status);
        Assert.Equal(product.Lounge, draft.Lounge);
        Assert.NotSame(product.Lounge, draft.Lounge);
        Assert.Equal([7, 3], draft.Lounge!.AirportIds);
        Assert.Null(draft.Baggage);
    }

    [Fact]
    public void P2_C09_ExtraBagOnALoungeCodeIsStillRefused()
        => BusinessAssert.Throws(16106, 422, () => (ProductSpec.G with { Rfisc = SubCodes.LoungeCode }).Define(7, _subCodeGL, CreatedAt));

    [Fact]
    public void P2_G01_OnlyThePhase2ValuesWereAdded()
    {
        Assert.Equal([("ExtraBaggage", 1), ("LoungeAccess", 2)], Members<AncillaryProductType>());
        Assert.Equal([("TravellerBound", 1), ("TravellerSegment", 2)], Members<AncillarySalesScope>());
        Assert.Equal([("EmdAssociated", 2), ("EmdStandalone", 3)], Members<AncillaryDocumentType>());
        Assert.Equal([("Piece", 1), ("Each", 3)], Members<AncillaryQuantityUnit>());
        Assert.Equal([("Unlimited", 1)], Members<AncillaryInventoryControl>());
        Assert.Equal([("Ancillary", 1), ("Tax", 2)], Members<AncillaryPriceLineCategory>());
        Assert.Equal([("Kg", 1), ("Lbs", 2)], Members<AncillaryWeightUnit>());
        Assert.Equal(2, SoldCombinations.Rows.Count);
    }

    private AncillaryProduct DefineLounge(ProductSpec spec) => spec.Define(7, _subCodeL, CreatedAt);

    private static IEnumerable<(string Name, int Value)> Members<TEnum>()
        where TEnum : struct, Enum
        => Enum.GetValues<TEnum>().Select(value => (value.ToString(), Convert.ToInt32(value))).OrderBy(member => member.Item2);
}
