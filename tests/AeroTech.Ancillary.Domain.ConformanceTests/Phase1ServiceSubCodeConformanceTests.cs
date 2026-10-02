using System.Reflection;
using AeroTech.Ancillary.Domain.ConformanceTests.Fixtures;
using AeroTech.Ancillary.Domain.ServiceSubCodeAggregate;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;

namespace AeroTech.Ancillary.Domain.ConformanceTests;

public sealed class Phase1ServiceSubCodeConformanceTests
{
    [Fact]
    public void P1_S01_IndustryCodeTakesItsAttributesFromTheReference()
    {
        var subCode = SubCodes.Industry();

        Assert.Equal(ServiceSubCodeStatus.Active, subCode.Status);
        Assert.Equal(ServiceSubCodeSource.Industry, subCode.Source);
        Assert.Equal(("C", "BG", null, "B1", null, "FIRST EXCESS BAG"), Attributes(subCode));
        Assert.Equal(SubCodes.RegisteredAt, subCode.CreatedAt);
    }

    [Fact]
    public void P1_S01_IndustryReferenceHoldsTheFirstExcessBag()
        => Assert.Contains(
            new IndustrySubCode("0CC", "BG", null, "B1", null, "FIRST EXCESS BAG", "C", AncillaryDocumentType.EmdAssociated, 1),
            IndustrySubCodeReference.Entries);

    [Theory]
    [InlineData("C", null, null, null, null, null)]
    [InlineData(null, "BG", null, null, null, null)]
    [InlineData(null, null, "XX", null, null, null)]
    [InlineData(null, null, null, "B1", null, null)]
    [InlineData(null, null, null, null, "XX", null)]
    [InlineData(null, null, null, null, null, "FIRST EXCESS BAG")]
    public void P1_S02_IndustryCodeWithAnAttributeIsInvalid(
        string? rfic,
        string? groupCode,
        string? subGroupCode,
        string? description1Code,
        string? description2Code,
        string? commercialName)
        => BusinessAssert.Throws(16112, 422, () => ServiceSubCode.Register(
            1,
            SubCodes.AirlineId,
            "0CC",
            rfic,
            groupCode,
            subGroupCode,
            description1Code,
            description2Code,
            commercialName,
            SubCodes.RegisteredAt));

    [Theory]
    [InlineData("0CD")]
    [InlineData("1AA")]
    [InlineData("000")]
    public void P1_S03_IndustryCodeOutsideTheReferenceIsRefused(string code)
        => BusinessAssert.Throws(16114, 422, () => SubCodes.Industry(code));

    [Fact]
    public void P1_S04_CarrierDefinedCodeKeepsTheAttributesSent()
    {
        var subCode = SubCodes.CarrierDefined("XBH", subGroupCode: "XS", description1Code: "D1", description2Code: "D2", commercialName: "Heavy Bag 32");

        Assert.Equal(ServiceSubCodeStatus.Active, subCode.Status);
        Assert.Equal(ServiceSubCodeSource.CarrierDefined, subCode.Source);
        Assert.Equal("XBH", subCode.Code);
        Assert.Equal(("C", "BG", "XS", "D1", "D2", "Heavy Bag 32"), Attributes(subCode));
    }

    [Theory]
    [InlineData("98A")]
    [InlineData("99Z")]
    public void P1_S05_ReservedPrefixIsInvalid(string code)
        => BusinessAssert.Throws(16112, 422, () => SubCodes.CarrierDefined(code));

    [Fact]
    public void P1_S07_RetireAndReactivateKeepTheAttributes()
    {
        var subCode = SubCodes.CarrierDefined(subGroupCode: "XS");
        var attributes = Attributes(subCode);

        subCode.Retire();

        Assert.Equal(ServiceSubCodeStatus.Retired, subCode.Status);
        BusinessAssert.Throws(16113, 409, subCode.Retire);

        subCode.Reactivate();

        Assert.Equal(ServiceSubCodeStatus.Active, subCode.Status);
        Assert.Equal(attributes, Attributes(subCode));
        BusinessAssert.Throws(16113, 409, subCode.Reactivate);
    }

    [Fact]
    public void P1_S08_NoOperationChangesTheAttributes()
    {
        var operations = typeof(ServiceSubCode)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .Order()
            .ToArray();

        var publicSetters = typeof(ServiceSubCode)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(property => property.SetMethod is { IsPublic: true })
            .Select(property => property.Name)
            .ToArray();

        Assert.Equal([nameof(ServiceSubCode.Reactivate), nameof(ServiceSubCode.Retire)], operations);
        Assert.Empty(publicSetters);
    }

    private static (string Rfic, string GroupCode, string? SubGroupCode, string? Description1Code, string? Description2Code, string CommercialName) Attributes(ServiceSubCode subCode)
        => (subCode.Rfic, subCode.GroupCode, subCode.SubGroupCode, subCode.Description1Code, subCode.Description2Code, subCode.CommercialName);
}
