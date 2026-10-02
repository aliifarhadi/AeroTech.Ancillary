using AeroTech.Ancillary.Domain.ConformanceTests.Fixtures;
using AeroTech.Ancillary.Domain.ServiceSubCodeAggregate;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;

namespace AeroTech.Ancillary.Domain.ConformanceTests;

public sealed class Phase2ServiceSubCodeConformanceTests
{
    [Fact]
    public void P2_S01_LoungeCodeTakesItsAttributesFromTheReference()
    {
        var subCode = SubCodes.Industry("0BX");

        Assert.Equal((ServiceSubCodeStatus.Active, ServiceSubCodeSource.Industry), (subCode.Status, subCode.Source));
        Assert.Equal(
            ("E", "LG", null, null, null, "LOUNGE ACCESS"),
            (subCode.Rfic, subCode.GroupCode, subCode.SubGroupCode, subCode.Description1Code, subCode.Description2Code, subCode.CommercialName));
    }

    [Fact]
    public void P2_S01_IndustryReferenceHoldsExactlyTheBagAndTheLoungeEntries()
        => Assert.Equal(
            [
                new IndustrySubCode("0CC", "BG", null, "B1", null, "FIRST EXCESS BAG", "C", AncillaryDocumentType.EmdAssociated, 1),
                new IndustrySubCode("0BX", "LG", null, null, null, "LOUNGE ACCESS", "E", AncillaryDocumentType.EmdStandalone, 1)
            ],
            IndustrySubCodeReference.Entries);

    [Theory]
    [InlineData("E", null, null, null, null, null)]
    [InlineData(null, "LG", null, null, null, null)]
    [InlineData(null, null, "XX", null, null, null)]
    [InlineData(null, null, null, "XX", null, null)]
    [InlineData(null, null, null, null, "XX", null)]
    [InlineData(null, null, null, null, null, "LOUNGE ACCESS")]
    public void P2_S02_LoungeCodeWithAnAttributeIsInvalid(
        string? rfic,
        string? groupCode,
        string? subGroupCode,
        string? description1Code,
        string? description2Code,
        string? commercialName)
        => BusinessAssert.Throws(16112, 422, () => ServiceSubCode.Register(
            1,
            SubCodes.AirlineId,
            "0BX",
            rfic,
            groupCode,
            subGroupCode,
            description1Code,
            description2Code,
            commercialName,
            SubCodes.RegisteredAt));

    [Fact]
    public void P2_S03_SecondBagCodeIsStillOutsideTheReference()
        => BusinessAssert.Throws(16114, 422, () => SubCodes.Industry("0CD"));
}
