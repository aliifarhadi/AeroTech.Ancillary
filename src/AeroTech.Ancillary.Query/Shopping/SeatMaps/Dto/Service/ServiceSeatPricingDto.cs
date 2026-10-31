using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.Shopping.SeatMaps.Dto.Service
{
    public sealed record ServiceSeatPricingDto(long Id, int Version, EnumValueDto? PricingUnit, ServiceSeatRateDto Rate);
}
