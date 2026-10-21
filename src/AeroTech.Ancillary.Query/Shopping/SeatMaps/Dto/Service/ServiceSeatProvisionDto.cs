using AeroTech.Ancillary.Query._Shared.Enums;

namespace AeroTech.Ancillary.Query.Shopping.SeatMaps.Dto.Service
{
    public sealed record ServiceSeatProvisionDto(
        long Id,
        int Sequence,
        EnumValueDto CoverageScope,
        EnumValueDto PurchaseStage,
        EnumValueDto PriceOrigin,
        EnumValueDto ApplicationType,
        ServiceSeatQuantityDto Quantity,
        ServiceSeatOutcomeDto Outcome,
        ServiceSeatSettlementDto Settlement,
        ServiceSeatAvailabilityDto Availability,
        ServiceSeatFulfillmentDto Fulfillment);
}
