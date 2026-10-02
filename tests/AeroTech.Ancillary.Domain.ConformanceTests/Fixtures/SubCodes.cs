using AeroTech.Ancillary.Domain.ServiceSubCodeAggregate;

namespace AeroTech.Ancillary.Domain.ConformanceTests.Fixtures;

public static class SubCodes
{
    public const int AirlineId = 10;
    public const string LoungeCode = "XLG";

    public static readonly DateTimeOffset RegisteredAt = new(2026, 9, 1, 8, 0, 0, TimeSpan.Zero);

    public static ServiceSubCode Industry(string code = "0CC", int ownerAirlineId = AirlineId)
        => ServiceSubCode.Register(1, ownerAirlineId, code, null, null, null, null, null, null, RegisteredAt);

    public static ServiceSubCode CarrierDefined(
        string code = "XBG",
        int ownerAirlineId = AirlineId,
        string rfic = "C",
        string groupCode = "BG",
        string? subGroupCode = null,
        string? description1Code = null,
        string? description2Code = null,
        string commercialName = "EXTRA BAG")
        => ServiceSubCode.Register(2, ownerAirlineId, code, rfic, groupCode, subGroupCode, description1Code, description2Code, commercialName, RegisteredAt);

    public static ServiceSubCode CarrierDefinedLounge(string code = LoungeCode, int ownerAirlineId = AirlineId)
        => CarrierDefined(code, ownerAirlineId, "E", "LG", commercialName: "LOUNGE");

    public static ServiceSubCode For(string code, int ownerAirlineId = AirlineId)
        => char.IsAsciiDigit(code[0])
            ? Industry(code, ownerAirlineId)
            : code == LoungeCode
                ? CarrierDefinedLounge(code, ownerAirlineId)
                : CarrierDefined(code, ownerAirlineId);
}
