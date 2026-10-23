using System.Text.Json.Serialization;
using AeroTech.Messages.Ancillary.Enums;

namespace AeroTech.Ancillary.Query.Shopping.AncillaryOffers.Dto
{
    public sealed record ExtraSeatDetailsDto(
        SeatPurpose SeatPurpose,
        bool RequiresExitRowEligibility,
        bool RequiresAdjacentSeat,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] ExtraSeatPurpose? ExtraSeatPurpose,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int? ExtraOccupiedSeatCount,
        bool RequiresExternalTicketAction,
        IReadOnlyList<string> SeatCharacteristicCodes,
        IReadOnlyList<int> ApplicableCabinIds);
}
