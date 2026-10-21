using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.Shopping.SeatMaps.Dto.Service
{
    public sealed record ServiceSeatBookingDto(EnumValueDto Method, string? SsrCode, string? SsimCode, EnumValueDto ConfirmationRequirement);
}
