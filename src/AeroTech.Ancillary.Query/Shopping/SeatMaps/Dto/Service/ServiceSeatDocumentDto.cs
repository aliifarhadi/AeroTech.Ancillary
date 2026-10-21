using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.Shopping.SeatMaps.Dto.Service
{
    public sealed record ServiceSeatDocumentDto(EnumValueDto Type, EnumValueDto? Routing, string? Rfic, string? Rfisc);
}
