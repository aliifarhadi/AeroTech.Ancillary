using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.Shopping.SeatMaps.Dto.Service
{
    public sealed record ServiceSeatOutcomeDto(EnumValueDto Disposition, bool DocumentRequired, bool BookingRequired);
}
