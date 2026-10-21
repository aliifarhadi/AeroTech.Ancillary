using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.Shopping.SeatMaps.Dto.Service
{
    public sealed record ServiceSeatSpecificationDto(
        EnumValueDto Purpose,
        bool RequiresExitRowEligibility,
        bool RequiresAdjacentSeat,
        EnumValueDto? ExtraSeatPurpose,
        int? ExtraOccupiedSeatCount,
        bool RequiresExternalTicketAction);
}
