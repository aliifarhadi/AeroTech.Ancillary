using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.Shopping.SeatMaps.Dto.Service
{
    public sealed record ServiceSeatPriceComponentDto(
        long Id,
        EnumValueDto Category,
        string? Code,
        string? Name,
        int? CountryId,
        int? StationAirportId,
        decimal Amount,
        EnumValueDto? FeeApplicationUnit,
        EnumValueDto? TaxTreatment);
}
