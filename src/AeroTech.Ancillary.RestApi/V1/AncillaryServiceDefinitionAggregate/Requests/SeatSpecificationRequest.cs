using System.Text.Json.Serialization;
using AeroTech.Ancillary.Application.AncillaryServiceDefinitionAggregate.Commands.DefineAncillaryServiceDefinition;
using AeroTech.Ancillary.Domain.AncillaryServiceDefinitionAggregate;
using AeroTech.Ancillary.Query.AncillaryServiceDefinitionAggregate.Dto;
using AeroTech.Ancillary.RestApi.V1._Shared.OpenApi;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.RestApi.V1.AncillaryServiceDefinitionAggregate.Requests
{
    [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
    [VariantSpecification(
        typeof(SeatVariantSpecificationDto),
        "The seat fields of the variant named by variantCode: A07 Standard seat, A08 Preferred seat, A09 Extra seat. " +
        "The first v1 body is still accepted: seatPurpose, requiresAdjacentSeat, extraSeatPurpose, extraOccupiedSeatCount and " +
        "requiresExternalTicketAction may be sent on every variant and must then agree with it, otherwise the answer is 422 with code 16202.")]
    public sealed record SeatSpecificationRequest(
        SeatPurpose? SeatPurpose,
        IReadOnlyList<string>? SeatCharacteristicCodes,
        IReadOnlyList<int>? ApplicableCabinIds,
        bool RequiresExitRowEligibility,
        bool RequiresAdjacentSeat,
        ExtraSeatPurpose? ExtraSeatPurpose,
        int? ExtraOccupiedSeatCount,
        bool RequiresExternalTicketAction)
    {
        public SeatSpecificationInput ToInput(string variantCode)
            => new(
                SeatPurpose ?? variantCode switch
                {
                    AncillaryVariant.PreferredSeat => Messages.Ancillary.Enums.SeatPurpose.Preferred,
                    AncillaryVariant.ExtraSeat => Messages.Ancillary.Enums.SeatPurpose.ExtraSeat,
                    _ => Messages.Ancillary.Enums.SeatPurpose.Standard
                },
                SeatCharacteristicCodes,
                ApplicableCabinIds,
                RequiresExitRowEligibility,
                RequiresAdjacentSeat,
                ExtraSeatPurpose,
                ExtraOccupiedSeatCount,
                RequiresExternalTicketAction);
    }
}
