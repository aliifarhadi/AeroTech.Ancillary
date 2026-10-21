using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.Shopping.SeatMaps.Dto.Service
{
    public sealed record ServiceSeatRateDto(
        long Id,
        EnumValueDto? PassengerTypeCode,
        int? AgeFromInclusive,
        int? AgeToExclusive,
        int CurrencyId,
        decimal BaseAmount,
        decimal UnitTotal,
        bool IsUnitTotalComplete,
        IReadOnlyList<ServiceSeatPriceComponentDto> Components);
}
