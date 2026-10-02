using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;

namespace AeroTech.Ancillary.Application.AcceptanceTests.ServiceSubCodes;

[Collection(DatabaseCollection.Name)]
public sealed class Phase2ServiceSubCodeAcceptanceTests(TestDatabase database)
{
    private readonly AncillaryHarness _harness = new(database);

    private int Airline => _harness.AirlineId;

    [Fact]
    public async Task P2_S01_LoungeCodeIsEnabledWithOnlyAirlineAndCode()
    {
        var result = await _harness.RegisterAsync(Phase2Commands.SubCodeL(Airline));

        Assert.Equal((Airline, "0BX", ServiceSubCodeSource.Industry, ServiceSubCodeStatus.Active), (result.OwnerAirlineId, result.Code, result.Source, result.Status));

        var subCode = await _harness.GetSubCodeAsync(result.Id);

        Assert.Equal(("Industry", "Active"), (subCode.Source.Name, subCode.Status.Name));
        Assert.Equal(
            ("E", "LG", null, null, null, "LOUNGE ACCESS"),
            (subCode.Rfic, subCode.GroupCode, subCode.SubGroupCode, subCode.Description1Code, subCode.Description2Code, subCode.CommercialName));
    }

    [Theory]
    [InlineData("E", null, null)]
    [InlineData(null, "LG", null)]
    [InlineData(null, null, "LOUNGE ACCESS")]
    public async Task P2_S02_LoungeCodeWithAnAttributeIsRefused(string? rfic, string? groupCode, string? commercialName)
    {
        var command = Phase2Commands.SubCodeL(Airline) with { Rfic = rfic, GroupCode = groupCode, CommercialName = commercialName };

        await BusinessAssert.ThrowsAsync(16112, 422, () => _harness.RegisterAsync(command));

        await _harness.RegisterAsync(Phase2Commands.SubCodeL(Airline));

        await BusinessAssert.ThrowsAsync(16112, 422, () => _harness.RegisterAsync(command));
    }

    [Fact]
    public async Task P2_S03_SecondBagCodeIsStillOutsideTheReference()
        => await BusinessAssert.ThrowsAsync(16114, 422, () => _harness.RegisterAsync(Phase1Commands.SubCodeS(Airline) with { Code = "0CD" }));
}
