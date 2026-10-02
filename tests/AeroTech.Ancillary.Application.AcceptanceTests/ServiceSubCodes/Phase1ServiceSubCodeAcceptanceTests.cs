using AeroTech.Ancillary.Application.AcceptanceTests.Fixtures;
using AeroTech.Ancillary.Application.ServiceSubCodeAggregate.Commands.RegisterServiceSubCode;
using AeroTech.Ancillary.Application.ServiceSubCodeAggregate.Commands.RegisterServiceSubCode.Backoffice;
using AeroTech.Ancillary.Domain.ServiceSubCodeAggregate;
using AeroTech.Ancillary.Query.ServiceSubCodeAggregate.Dto;
using AeroTech.Messages.Ancillary.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AeroTech.Ancillary.Application.AcceptanceTests.ServiceSubCodes;

[Collection(DatabaseCollection.Name)]
public sealed class Phase1ServiceSubCodeAcceptanceTests(TestDatabase database)
{
    private readonly AncillaryHarness _harness = new(database);
    private readonly BackofficeRegisterServiceSubCodeCommandValidator _validator = new();

    private int Airline => _harness.AirlineId;

    [Fact]
    public async Task P1_S01_IndustryCodeIsEnabledWithOnlyAirlineAndCode()
    {
        var result = await _harness.RegisterAsync(Phase1Commands.SubCodeS(Airline));

        Assert.Equal(
            (Airline, "0CC", ServiceSubCodeSource.Industry, ServiceSubCodeStatus.Active),
            (result.OwnerAirlineId, result.Code, result.Source, result.Status));

        var subCode = await _harness.GetSubCodeAsync(result.Id);

        Assert.Equal((result.Id, Airline, "0CC", "Industry", "Active"), (subCode.Id, subCode.OwnerAirlineId, subCode.Code, subCode.Source.Name, subCode.Status.Name));
        Assert.Equal(("C", "BG", null, "B1", null, "FIRST EXCESS BAG"), Attributes(subCode));
        Assert.Equal(_harness.Clock.Now, subCode.CreatedAt);
    }

    [Theory]
    [InlineData("C", null, null)]
    [InlineData(null, "BG", null)]
    [InlineData(null, null, "FIRST EXCESS BAG")]
    [InlineData("E", "LG", "LOUNGE")]
    public async Task P1_S02_IndustryCodeWithAnAttributeIsRefused(string? rfic, string? groupCode, string? commercialName)
    {
        var command = Phase1Commands.SubCodeS(Airline) with { Rfic = rfic, GroupCode = groupCode, CommercialName = commercialName };

        ValidationAssert.Accepts(_validator, command);
        await BusinessAssert.ThrowsAsync(16112, 422, () => _harness.RegisterAsync(command));

        Assert.Equal(ServiceSubCodeStatus.Active, (await _harness.RegisterAsync(Phase1Commands.SubCodeS(Airline))).Status);
    }

    [Theory]
    [InlineData("0CD")]
    [InlineData("1AA")]
    public async Task P1_S03_IndustryCodeOutsideTheReferenceIsRefused(string code)
        => await BusinessAssert.ThrowsAsync(16114, 422, () => _harness.RegisterAsync(Phase1Commands.SubCodeS(Airline) with { Code = code }));

    [Fact]
    public async Task P1_S04_CarrierDefinedCodeKeepsEveryFieldSent()
    {
        var command = new BackofficeRegisterServiceSubCodeCommand(Airline, "XBH", "C", "BG", "XS", "D1", "D2", "Heavy Bag 32");

        var result = await _harness.RegisterAsync(command);

        Assert.Equal((ServiceSubCodeSource.CarrierDefined, ServiceSubCodeStatus.Active), (result.Source, result.Status));

        var subCode = await _harness.GetSubCodeAsync(result.Id);

        Assert.Equal((Airline, "XBH", "CarrierDefined", "Active"), (subCode.OwnerAirlineId, subCode.Code, subCode.Source.Name, subCode.Status.Name));
        Assert.Equal(("C", "BG", "XS", "D1", "D2", "Heavy Bag 32"), Attributes(subCode));
    }

    [Fact]
    public void P1_S04_CarrierDefinedCodeWithoutARequiredAttributeIsMalformed()
    {
        var command = Phase1Commands.SubCodeG(Airline);

        ValidationAssert.Accepts(_validator, command);
        ValidationAssert.Rejects(_validator, command with { Rfic = null });
        ValidationAssert.Rejects(_validator, command with { GroupCode = null });
        ValidationAssert.Rejects(_validator, command with { CommercialName = null });
    }

    [Theory]
    [InlineData("98A")]
    [InlineData("99Z")]
    public async Task P1_S05_ReservedPrefixIsRefused(string code)
    {
        var command = Phase1Commands.SubCodeS(Airline) with { Code = code };

        ValidationAssert.Accepts(_validator, command);
        await BusinessAssert.ThrowsAsync(16112, 422, () => _harness.RegisterAsync(command));
    }

    [Theory]
    [InlineData("XB", "C", "BG", "EXTRA BAG")]
    [InlineData("XBGG", "C", "BG", "EXTRA BAG")]
    [InlineData("xbg", "C", "BG", "EXTRA BAG")]
    [InlineData("XB-", "C", "BG", "EXTRA BAG")]
    [InlineData("", "C", "BG", "EXTRA BAG")]
    [InlineData("XBG", "CC", "BG", "EXTRA BAG")]
    [InlineData("XBG", "1", "BG", "EXTRA BAG")]
    [InlineData("XBG", "c", "BG", "EXTRA BAG")]
    [InlineData("XBG", "C", "B", "EXTRA BAG")]
    [InlineData("XBG", "C", "BGG", "EXTRA BAG")]
    [InlineData("XBG", "C", "B-", "EXTRA BAG")]
    [InlineData("XBG", "C", "BG", "ABCDEFGHIJKLMNOPQRSTUVWXYZ01234")]
    [InlineData("XBG", "C", "BG", "EXTRA/BAG")]
    [InlineData("XBG", "C", "BG", "EXTRA-BAG")]
    [InlineData("XBG", "C", "BG", "EXTRA.BAG")]
    [InlineData("XBG", "C", "BG", "")]
    public void P1_S05_MalformedSubCodeIsRejected(string code, string rfic, string groupCode, string commercialName)
    {
        var valid = Phase1Commands.SubCodeG(Airline);

        ValidationAssert.Accepts(_validator, valid with { CommercialName = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123" });
        ValidationAssert.Rejects(_validator, valid with { Code = code, Rfic = rfic, GroupCode = groupCode, CommercialName = commercialName });
    }

    [Fact]
    public async Task P1_S06_CodeIsRegisteredOnlyOncePerAirline()
    {
        var industry = await _harness.RegisterAsync(Phase1Commands.SubCodeS(Airline));
        var carrier = await _harness.RegisterAsync(Phase1Commands.SubCodeG(Airline));

        await BusinessAssert.ThrowsAsync(16111, 409, () => _harness.RegisterAsync(Phase1Commands.SubCodeS(Airline)));
        await BusinessAssert.ThrowsAsync(16111, 409, () => _harness.RegisterAsync(Phase1Commands.SubCodeG(Airline)));

        await _harness.RetireSubCodeAsync(industry.Id);
        await _harness.RetireSubCodeAsync(carrier.Id);

        await BusinessAssert.ThrowsAsync(16111, 409, () => _harness.RegisterAsync(Phase1Commands.SubCodeS(Airline)));
        await BusinessAssert.ThrowsAsync(16111, 409, () => _harness.RegisterAsync(Phase1Commands.SubCodeG(Airline)));

        var otherAirline = database.NextAirlineId();
        var otherIndustry = await _harness.RegisterAsync(Phase1Commands.SubCodeS(otherAirline));
        var otherCarrier = await _harness.RegisterAsync(Phase1Commands.SubCodeG(otherAirline) with { Rfic = "C", GroupCode = "BG", CommercialName = "OTHER BAG" });

        Assert.Equal(Attributes(await _harness.GetSubCodeAsync(industry.Id)), Attributes(await _harness.GetSubCodeAsync(otherIndustry.Id)));
        Assert.Equal(("C", "BG", null, null, null, "OTHER BAG"), Attributes(await _harness.GetSubCodeAsync(otherCarrier.Id)));
        Assert.Equal(("C", "BG", null, null, null, "EXTRA BAG"), Attributes(await _harness.GetSubCodeAsync(carrier.Id)));
    }

    [Fact]
    public async Task P1_S06_UniqueIndexRejectsASecondRowForTheSameAirlineAndCode()
    {
        await _harness.RegisterAsync(Phase1Commands.SubCodeS(Airline));

        await using var scope = _harness.NewScope();

        await scope.SubCodes.AddAsync(ServiceSubCode.Register(database.Ids.NewId(), Airline, "0CC", null, null, null, null, null, null, _harness.Clock.Now));

        await Assert.ThrowsAsync<DbUpdateException>(() => scope.Command.SaveChangesAsync());
    }

    [Fact]
    public async Task P1_S07_RetireAndReactivateKeepTheAttributes()
    {
        var registered = await _harness.RegisterAsync(Phase1Commands.SubCodeS(Airline));
        var before = await _harness.GetSubCodeAsync(registered.Id);

        var retired = await _harness.RetireSubCodeAsync(registered.Id);

        Assert.Equal(ServiceSubCodeStatus.Retired, retired.Status);
        Assert.Equal("Retired", (await _harness.GetSubCodeAsync(registered.Id)).Status.Name);
        await BusinessAssert.ThrowsAsync(16113, 409, () => _harness.RetireSubCodeAsync(registered.Id));

        var reactivated = await _harness.ReactivateSubCodeAsync(registered.Id);

        Assert.Equal(ServiceSubCodeStatus.Active, reactivated.Status);
        Assert.Equal(before, await _harness.GetSubCodeAsync(registered.Id));
        await BusinessAssert.ThrowsAsync(16113, 409, () => _harness.ReactivateSubCodeAsync(registered.Id));

        var unknown = database.Ids.NewId();

        await BusinessAssert.ThrowsAsync(16110, 404, () => _harness.RetireSubCodeAsync(unknown));
        await BusinessAssert.ThrowsAsync(16110, 404, () => _harness.ReactivateSubCodeAsync(unknown));
        await BusinessAssert.ThrowsAsync(16110, 404, () => _harness.GetSubCodeAsync(unknown));
    }

    [Fact]
    public async Task P1_S08_AttributesOfASubCodeCanNeverBeChanged()
    {
        var registered = await _harness.RegisterAsync(Phase1Commands.SubCodeG(Airline));
        var before = await _harness.GetSubCodeAsync(registered.Id);

        await _harness.RetireSubCodeAsync(registered.Id);
        await BusinessAssert.ThrowsAsync(
            16111,
            409,
            () => _harness.RegisterAsync(Phase1Commands.SubCodeG(Airline) with { Rfic = "E", GroupCode = "LG", CommercialName = "LOUNGE" }));
        await _harness.ReactivateSubCodeAsync(registered.Id);

        Assert.Equal(before, await _harness.GetSubCodeAsync(registered.Id));

        var useCases = typeof(IRegisterServiceSubCodeService).Assembly
            .GetTypes()
            .Where(type => type.Namespace?.Contains(".ServiceSubCodeAggregate.Commands.", StringComparison.Ordinal) == true)
            .Select(type => type.Namespace!.Split(".Commands.")[1].Split('.')[0])
            .Distinct()
            .Order()
            .ToArray();

        Assert.Equal(["ReactivateServiceSubCode", "RegisterServiceSubCode", "RetireServiceSubCode"], useCases);
    }

    private static (string, string, string?, string?, string?, string) Attributes(BackofficeServiceSubCodeDto subCode)
        => (subCode.Rfic, subCode.GroupCode, subCode.SubGroupCode, subCode.Description1Code, subCode.Description2Code, subCode.CommercialName);
}
