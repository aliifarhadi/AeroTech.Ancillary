using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.Shopping.SeatMaps.Dto.Service
{
    public sealed record ServiceSeatSettlementDto(EnumValueDto ReissueRefund, EnumValueDto? FormOfRefund, bool Commissionable, bool InterlineSettlement);
}
