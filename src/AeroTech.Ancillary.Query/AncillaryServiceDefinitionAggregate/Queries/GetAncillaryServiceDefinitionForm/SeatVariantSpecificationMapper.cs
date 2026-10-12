using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;

namespace AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Queries.GetAncillaryServiceDefinitionForm
{
    public static class SeatVariantSpecificationMapper
    {
        public static SeatVariantSpecificationDto ToDto(AncillaryServiceDefinition definition)
        {
            var seat = definition.Seat!.ToArgs();
            var characteristics = seat.SeatCharacteristicCodes ?? [];
            var cabins = seat.ApplicableCabinIds ?? [];

            return definition.VariantCode switch
            {
                AncillaryVariant.StandardSeat => new StandardSeatSpecificationDto(characteristics, cabins, seat.RequiresExitRowEligibility),
                AncillaryVariant.PreferredSeat => new PreferredSeatSpecificationDto(characteristics, cabins, seat.RequiresExitRowEligibility),
                _ => new ExtraSeatSpecificationDto(
                    characteristics,
                    cabins,
                    seat.RequiresExitRowEligibility,
                    seat.RequiresAdjacentSeat,
                    seat.ExtraSeatPurpose!.Value,
                    seat.ExtraOccupiedSeatCount!.Value,
                    seat.RequiresExternalTicketAction)
            };
        }
    }
}
