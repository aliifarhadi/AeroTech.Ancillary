using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.Shopping.SeatMaps.Dto.Service
{
    public sealed record ServiceSeatQuantityDto(EnumValueDto Unit, int MinQuantity, int MaxQuantity);
}
